// Controllers/AccountController.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Progra3_TPFinal_19B.Data;
using Progra3_TPFinal_19B.Models;
using Progra3_TPFinal_19B.Models.ViewModels;
using Progra3_TPFinal_19B.Services; // IEmailSender
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Progra3_TPFinal_19B.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly CallCenterDbContext _db;
        private readonly IEmailSender _mail;

        public AccountController(CallCenterDbContext db, IEmailSender mail)
        {
            _db = db;
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
            var user = await _db.Users.FirstOrDefaultAsync(u => (u.Email == normalized || u.Username == normalized) && !u.IsDeleted);

            if (user == null || user.IsBlocked)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View();
            }

            // Hash SHA-256 (HEX) — debe coincidir con lo usado en Register/ResetPassword
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
            if (await _db.Users.AnyAsync(u => (u.Email == normalized || u.Username == normalized) && !u.IsDeleted))
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

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

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
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var normalized = (vm.Email ?? "").Trim();
            var user = await _db.Users.FirstOrDefaultAsync(u => (u.Email == normalized || u.Username == normalized) && !u.IsDeleted);

            // Seguridad: siempre respondemos igual
            if (user != null && !user.IsBlocked)
            {
                var bytes = RandomNumberGenerator.GetBytes(32);
                var token = WebEncoders.Base64UrlEncode(bytes);

                var pr = new PasswordResetToken
                {
                    UserId = user.Id,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30)
                };
                _db.PasswordResetTokens.Add(pr);
                await _db.SaveChangesAsync();

                var resetLink = Url.Action("ResetPassword", "Account", new { token }, Request.Scheme)!;
                var body = $@"
                    <p>Hola {user.FullName ?? user.Username},</p>
                    <p>Para restablecer tu contraseña hacé clic en el enlace:</p>
                    <p><a href=""{resetLink}"">Restablecer contraseña</a></p>
                    <p>El enlace vence en 30 minutos. Si no fuiste vos, ignorá este mensaje.</p>
                    <hr/><small>Progra3_TPFinal_19B</small>";
                await _mail.SendAsync(user.Email ?? user.Username, "Restablecer contraseña", body);
            }

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation() => View();

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return BadRequest();
            var exists = await _db.PasswordResetTokens
                .AnyAsync(t => t.Token == token && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow);
            if (!exists) return BadRequest("Token inválido o vencido.");

            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var pr = await _db.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == vm.Token && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow);
            if (pr == null)
            {
                ModelState.AddModelError("", "Token inválido o vencido.");
                return View(vm);
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == pr.UserId && !u.IsDeleted);
            if (user == null)
            {
                ModelState.AddModelError("", "Usuario no encontrado.");
                return View(vm);
            }

            using var sha = SHA256.Create();
            var hex = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(vm.Password)));
            user.PasswordHash = hex;
            user.UpdatedAt = DateTime.UtcNow;

            pr.UsedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Msg"] = "Contraseña actualizada. Ya podés iniciar sesión.";
            return RedirectToAction(nameof(Login));
        }
    }
}
