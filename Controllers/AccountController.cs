using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Progra3_TPFinal_19B.Application.Contracts;
using Progra3_TPFinal_19B.Models;
using Progra3_TPFinal_19B.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Progra3_TPFinal_19B.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IUserRepository _users;
        private readonly IPasswordResetTokenRepository _tokens;
        private readonly IEmailSender _mail;

        public AccountController(IUserRepository users, IPasswordResetTokenRepository tokens, IEmailSender mail)
        {
            _users = users;
            _tokens = tokens;
            _mail = mail;
        }

        // ========== LOGIN ==========
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            var normalized = (email ?? "").Trim();
            var user = await _users.FindForLoginAsync(normalized); // por Email o Username, y !IsDeleted

            if (user == null || user.IsBlocked)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View();
            }

            using var sha = SHA256.Create();
            var inputHash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(password ?? "")));
            if (!string.Equals(user.PasswordHash, inputHash, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? "Telefonista"),
                new Claim("UserId", user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true, AllowRefresh = true });

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // ========== REGISTER ==========
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string email, string password, string confirmPassword, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Completá email y contraseña.");
                return View();
            }
            if (!string.Equals(password, confirmPassword))
            {
                ModelState.AddModelError("", "Las contraseñas no coinciden.");
                return View();
            }

            var normalized = email.Trim();
            if (await _users.ExistsLoginAsync(normalized))
            {
                ModelState.AddModelError("", "El email ya está registrado.");
                return View();
            }

            using var sha = SHA256.Create();
            var hex = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = normalized,
                FullName = normalized,
                Email = normalized,
                Role = "Telefonista",
                IsBlocked = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = null,
                PasswordHash = hex
            };

            await _users.CreateAsync(user);

            TempData["RegisterOk"] = "Cuenta creada con éxito. Iniciá sesión para continuar.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        // ========== LOGOUT ==========
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied() => View();

        // ========== FORGOT / RESET PASSWORD ==========
        [HttpGet]
        public IActionResult ForgotPassword() => View(new Models.ViewModels.ForgotPasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(Models.ViewModels.ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var normalized = (vm.Email ?? "").Trim();
            var user = await _users.FindForLoginAsync(normalized);

            // Siempre respondemos igual
            if (user != null && !user.IsBlocked)
            {
                var bytes = RandomNumberGenerator.GetBytes(32);
                var token = WebEncoders.Base64UrlEncode(bytes);

                await _tokens.CreateAsync(new PasswordResetToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30)
                });

                var resetLink = Url.Action("ResetPassword", "Account", new { token }, Request.Scheme)!;
                var body = EmailTemplates.ResetPassword(user.FullName ?? user.Username, resetLink);
                await _mail.SendAsync(user.Email ?? user.Username, "Restablecer tu contraseña", body);
            }

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation() => View();

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return BadRequest();
            var valid = await _tokens.IsValidAsync(token);
            if (!valid) return BadRequest("Token inválido o vencido.");

            return View(new Models.ViewModels.ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(Models.ViewModels.ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var pr = await _tokens.GetByTokenAsync(vm.Token);
            if (pr == null || pr.UsedAt != null || pr.ExpiresAt <= DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Token inválido o vencido.");
                return View(vm);
            }

            using var sha = SHA256.Create();
            var hex = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(vm.Password)));

            await _users.UpdatePasswordAsync(pr.UserId, hex);
            await _tokens.MarkUsedAsync(pr.Id);

            TempData["Msg"] = "Contraseña actualizada. Ya podés iniciar sesión.";
            return RedirectToAction(nameof(Login));
        }
    }
}
