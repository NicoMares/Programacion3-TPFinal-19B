// Controllers/CustomersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Progra3_TPFinal_19B.Application.Contracts;
using Progra3_TPFinal_19B.Models;
using Progra3_TPFinal_19B.Models.ViewModels;
using System.Security.Claims;

[Authorize]
public class CustomersController : Controller
{
    private readonly ICustomerRepository _customers;
    public CustomersController(ICustomerRepository customers) => _customers = customers;

    public async Task<IActionResult> Index(int page = 1, int size = 20)
    {
        var list = await _customers.ListAsync(page, size);
        return View(list);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var c = await _customers.GetByIdAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }

    //public IActionResult Create() => View(new Customer());

    // GET: Customers/Create
    [HttpGet]
    public IActionResult Create()
    {
        var vm = new CustomerCreateViewModel(); // VM vacía para la vista
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // Validaciones amistosas antes de ir a la DB
        if (await _customers.ExistsByDocumentAsync(vm.DocumentNumber))
        {
            ModelState.AddModelError(nameof(vm.DocumentNumber), "Ya existe un cliente con ese documento.");
            return View(vm);
        }
        if (await _customers.ExistsByEmailAsync(vm.Email))
        {
            ModelState.AddModelError(nameof(vm.Email), "Ya existe un cliente con ese email.");
            return View(vm);
        }

        var uid = User.FindFirstValue("UserId");
        if (string.IsNullOrWhiteSpace(uid)) return Unauthorized();

        var c = new Customer
        {
            Id = Guid.NewGuid(),
            DocumentNumber = vm.DocumentNumber,
            Name = vm.Name,
            Email = vm.Email,
            Phone = vm.Phone,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = Guid.Parse(uid),
            IsDeleted = false
        };

        try
        {
            await _customers.CreateAsync(c);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            // Choque de índice único (documento o email) — sin tirar 500
            ModelState.AddModelError("", "Documento o Email ya existen.");
            // Si querés ser más específico:
            // if (ex.Message.Contains("UQ_Customers_Document")) ModelState.AddModelError(nameof(vm.DocumentNumber), "Ya existe un cliente con ese documento.");
            // else if (ex.Message.Contains("UQ_Customers_Email")) ModelState.AddModelError(nameof(vm.Email), "Ya existe un cliente con ese email.");
            return View(vm);
        }

        TempData["Msg"] = "Cliente creado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var c = await _customers.GetByIdAsync(id);
        if (c == null) return NotFound();

        var vm = new CustomerCreateViewModel
        {
            Id = c.Id,
            DocumentNumber = c.DocumentNumber,
            Name = c.Name,
            Email = c.Email,
            Phone = c.Phone
        };
        ViewBag.CustomerId = c.Id; // compat con tu vista actual
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CustomerCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var existing = await _customers.GetByIdAsync(id);
        if (existing == null) return NotFound();

        // Solo checkeá duplicado si cambió el valor
        if (!string.Equals(vm.DocumentNumber, existing.DocumentNumber, StringComparison.OrdinalIgnoreCase)
            && await _customers.ExistsByDocumentAsync(vm.DocumentNumber))
        {
            ModelState.AddModelError(nameof(vm.DocumentNumber), "Ya existe un cliente con ese documento.");
            return View(vm);
        }
        if (!string.Equals(vm.Email, existing.Email, StringComparison.OrdinalIgnoreCase)
            && await _customers.ExistsByEmailAsync(vm.Email, id))
        {
            ModelState.AddModelError(nameof(vm.Email), "Ya existe un cliente con ese email.");
            return View(vm);
        }

        var uid = User.FindFirstValue("UserId");
        if (string.IsNullOrWhiteSpace(uid)) return Unauthorized();

        existing.DocumentNumber = vm.DocumentNumber;
        existing.Name = vm.Name;
        existing.Email = vm.Email;
        existing.Phone = vm.Phone;
        existing.UpdatedByUserId = Guid.Parse(uid);

        try
        {
            await _customers.UpdateAsync(existing);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            ModelState.AddModelError("", "Documento o Email ya existen.");
            return View(vm);
        }

        TempData["Msg"] = "Cliente actualizado.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Customers/Delete/{id}
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var c = await _customers.GetByIdAsync(id);
        if (c == null) return NotFound();
        return View(c); // usa la vista Delete.cshtml fuertemente tipada a Customer
    }

    // POST: Customers/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)   // nombre distinto para evitar confusión
    {
        var uid = User.FindFirstValue("UserId");
        if (string.IsNullOrEmpty(uid)) return Unauthorized();

        await _customers.DeleteAsync(id, Guid.Parse(uid));
        TempData["Msg"] = "Cliente eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private Guid GetUserIdOrThrow()
    {
        var userIdStr = User.FindFirstValue("UserId");
        if (string.IsNullOrEmpty(userIdStr))
        {
            throw new InvalidOperationException("No se pudo obtener el UserId del usuario autenticado.");
        }
        return Guid.Parse(userIdStr);
    }
}
