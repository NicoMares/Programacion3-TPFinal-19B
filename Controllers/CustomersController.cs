using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Progra3_TPFinal_19B.Data;
using Progra3_TPFinal_19B.Models;
using Progra3_TPFinal_19B.Models.ViewModels;
using System;

namespace Progra3_TPFinal_19B.Controllers
{
    public class CustomersController : Controller
    {
        private readonly CallCenterDbContext _db;
        private static readonly Guid AdminSeedId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        public CustomersController(CallCenterDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Index()
        {
            var list = _db.Customers
                          .Where(c => !c.IsDeleted)
                          .OrderByDescending(c => c.CreatedAt)
                          .ToList();
            return View(list);
        }

        [HttpGet]
        public IActionResult Create() => View(new CustomerCreateViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerCreateViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (_db.Customers.Any(c => c.DocumentNumber == vm.DocumentNumber))
                ModelState.AddModelError(nameof(vm.DocumentNumber), "Ya existe un cliente con ese documento.");
            if (_db.Users.Any(u => u.Username == vm.Email || u.Email == vm.Email))
                ModelState.AddModelError(nameof(vm.Email), "Ya existe un usuario con ese email.");

            if (!ModelState.IsValid) return View(vm);

            var creatorId = AdminSeedId;

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = vm.Email,
                FullName = vm.Name,
                Role = "Cliente",
                PasswordHash = null!,
                Email = vm.Email,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = creatorId,
                IsDeleted = false,
                IsBlocked = false
            };
            _db.Users.Add(user);

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                DocumentNumber = vm.DocumentNumber,
                Name = vm.Name,
                Email = vm.Email,
                Phone = vm.Phone,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = creatorId,
                IsDeleted = false
            };
            _db.Customers.Add(customer);

            await _db.SaveChangesAsync();

            TempData["Msg"] = "Cliente creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var customer = _db.Customers.FirstOrDefault(c => c.Id == id && !c.IsDeleted);
            if (customer == null) return NotFound();

            var vm = new CustomerCreateViewModel
            {
                Name = customer.Name,
                DocumentNumber = customer.DocumentNumber,
                Email = customer.Email,
                Phone = customer.Phone
            };
            ViewBag.CustomerId = customer.Id;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CustomerCreateViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var customer = _db.Customers.FirstOrDefault(c => c.Id == id && !c.IsDeleted);
            if (customer == null) return NotFound();

            customer.Name = vm.Name;
            customer.DocumentNumber = vm.DocumentNumber;
            customer.Email = vm.Email;
            customer.Phone = vm.Phone;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedByUserId = AdminSeedId;

            var user = _db.Users.FirstOrDefault(u => u.Email == customer.Email);
            if (user != null)
            {
                user.FullName = vm.Name;
                user.Email = vm.Email;
                user.Username = vm.Email;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedByUserId = AdminSeedId;
            }

            await _db.SaveChangesAsync();

            TempData["Msg"] = "Cliente actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var customer = _db.Customers.FirstOrDefault(c => c.Id == id && !c.IsDeleted);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var customer = await _db.Customers
                .Where(c => c.Id == id && !c.IsDeleted)
                .SingleOrDefaultAsync();

            if (customer == null) return NotFound();

            customer.IsDeleted = true;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedByUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            var userId = await _db.Users
                .Where(u => u.Email == customer.Email && !u.IsDeleted)
                .Select(u => u.Id)
                .SingleOrDefaultAsync();   

            if (userId != Guid.Empty)
            {
                var userStub = new User
                {
                    Id = userId,
                    IsDeleted = true,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedByUserId = Guid.Parse("11111111-1111-1111-1111-111111111111")
                };
                _db.Attach(userStub);
                _db.Entry(userStub).Property(x => x.IsDeleted).IsModified = true;
                _db.Entry(userStub).Property(x => x.UpdatedAt).IsModified = true;
                _db.Entry(userStub).Property(x => x.UpdatedByUserId).IsModified = true;
            }

            await _db.SaveChangesAsync();
            TempData["Msg"] = "Cliente eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
    }
