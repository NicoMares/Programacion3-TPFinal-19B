// Controllers/IncidentsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Progra3_TPFinal_19B.Models;
using Progra3_TPFinal_19B.Models.ViewModels;
using System;
using System.Data;
using Microsoft.AspNetCore.Hosting;
using System.IO;


namespace Progra3_TPFinal_19B.Controllers
{
    public class IncidentsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;


        public IncidentsController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;

        }

        public IActionResult Index(string? q, string? state, string? priority, string? assigned)
        {
            var list = new List<IncidentListItemViewModel>();

            using var cn = new SqlConnection(_configuration.GetConnectionString("CallCenterDb"));
            using var cmd = new SqlCommand(@"
SELECT i.Id, i.Number,
       ISNULL(c.Name, N'')      AS CustomerName,
       ISNULL(t.Name, N'')      AS TypeName,
       ISNULL(p.Name, N'')      AS PriorityName,
       ISNULL(i.State, N'')     AS State,
       ISNULL(u.Username, N'')  AS AssignedToUsername,
       i.CreatedAt
FROM dbo.Incidents i
LEFT JOIN dbo.Customers     c ON c.Id = i.CustomerId
LEFT JOIN dbo.IncidentTypes t ON t.Id = i.TypeId
LEFT JOIN dbo.Priorities    p ON p.Id = i.PriorityId
LEFT JOIN dbo.Users         u ON u.Id = i.AssignedToUserId
WHERE i.IsDeleted = 0
  AND (@q IS NULL OR @q = '' OR i.Number LIKE '%'+@q+'%' OR c.Name LIKE '%'+@q+'%' OR i.Problem LIKE '%'+@q+'%')
  AND (@state IS NULL OR @state = '' OR i.State = @state)
  AND (@priority IS NULL OR @priority = '' OR p.Name = @priority)
  AND (@assigned IS NULL OR @assigned = '' OR u.Username LIKE '%'+@assigned+'%')
ORDER BY i.CreatedAt DESC;", cn);

            cmd.Parameters.Add("@q", SqlDbType.NVarChar, 200).Value = (object?)q ?? DBNull.Value;
            cmd.Parameters.Add("@state", SqlDbType.NVarChar, 20).Value = (object?)state ?? DBNull.Value;
            cmd.Parameters.Add("@priority", SqlDbType.NVarChar, 50).Value = (object?)priority ?? DBNull.Value;
            cmd.Parameters.Add("@assigned", SqlDbType.NVarChar, 200).Value = (object?)assigned ?? DBNull.Value;

            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new IncidentListItemViewModel
                {
                    Id = rd.GetGuid(0),
                    Number = rd.GetString(1),
                    CustomerName = rd.GetString(2),
                    TypeName = rd.GetString(3),
                    PriorityName = rd.GetString(4),
                    State = rd.GetString(5),
                    AssignedToUsername = rd.GetString(6),
                    CreatedAt = rd.GetDateTime(7)
                });
            }

            return View(list);
        }

        public IActionResult Details(Guid id)
        {
            using var cn = new SqlConnection(_configuration.GetConnectionString("CallCenterDb"));
            cn.Open();

            IncidentDetailsViewModel vm;

            using (var cmd = new SqlCommand(@"
SELECT i.Id, i.Number,
       i.CustomerId, ISNULL(c.Name,N'')       AS CustomerName,
       ISNULL(t.Name,N'')                     AS TypeName,
       ISNULL(p.Name,N'')                     AS PriorityName,
       ISNULL(i.State,N'')                    AS State,
       i.AssignedToUserId, ISNULL(u.Username,N'') AS AssignedToUsername,
       ISNULL(i.Problem,N'')                  AS Problem,
       i.CreatedAt, i.CreatedByUserId
FROM dbo.Incidents i
LEFT JOIN dbo.Customers     c ON c.Id = i.CustomerId
LEFT JOIN dbo.IncidentTypes t ON t.Id = i.TypeId
LEFT JOIN dbo.Priorities    p ON p.Id = i.PriorityId
LEFT JOIN dbo.Users         u ON u.Id = i.AssignedToUserId
WHERE i.Id = @Id AND i.IsDeleted = 0;", cn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
                using var rd = cmd.ExecuteReader();
                if (!rd.Read()) return NotFound();

                vm = new IncidentDetailsViewModel
                {
                    Id = rd.GetGuid(0),
                    Number = rd.GetString(1),
                    CustomerId = rd.GetGuid(2),
                    CustomerName = rd.GetString(3),
                    TypeName = rd.GetString(4),
                    PriorityName = rd.GetString(5),
                    State = rd.GetString(6),
                    AssignedToUserId = rd.GetGuid(7),
                    AssignedToUsername = rd.GetString(8),
                    Problem = rd.GetString(9),
                    CreatedAt = rd.GetDateTime(10)
                };

                var createdByUserId = rd.GetGuid(11);

                ViewBag.History = LoadHistory(cn, id, vm.CreatedAt, createdByUserId);
            }

            var comments = new List<IncidentCommentItemViewModel>();
            using (var cmd = new SqlCommand(@"
SELECT ic.Id, ic.AuthorUserId, ISNULL(u.Username,N'') AS AuthorUsername,
       ic.Text, ic.CreatedAt
FROM dbo.IncidentComments ic
LEFT JOIN dbo.Users u ON u.Id = ic.AuthorUserId
WHERE ic.IncidentId = @IncidentId AND ic.IsDeleted = 0
ORDER BY ic.CreatedAt ASC;", cn))
            {
                cmd.Parameters.Add("@IncidentId", SqlDbType.UniqueIdentifier).Value = id;
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    comments.Add(new IncidentCommentItemViewModel
                    {
                        Id = rd.GetGuid(0),
                        AuthorUserId = rd.GetGuid(1),
                        AuthorUsername = rd.GetString(2),
                        Text = rd.GetString(3),
                        CreatedAt = rd.GetDateTime(4)
                    });
                }
            }

            vm.Comments = comments;
            var files = new List<IncidentFileItemViewModel>();
            using (var cmd = new SqlCommand(@"
SELECT Id, OriginalName, ContentType, SizeBytes, Path
FROM dbo.IncidentFiles
WHERE IncidentId = @IncidentId AND IsDeleted = 0
ORDER BY CreatedAt ASC;", cn))
            {
                cmd.Parameters.Add("@IncidentId", SqlDbType.UniqueIdentifier).Value = id;
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    files.Add(new IncidentFileItemViewModel
                    {
                        Id = rd.GetGuid(0),
                        OriginalName = rd.GetString(1),
                        ContentType = rd.IsDBNull(2) ? null : rd.GetString(2),
                        SizeBytes = rd.GetInt64(3),
                        Path = rd.GetString(4)
                    });
                }
            }
            vm.Files = files;

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddComment(Guid incidentId, string text)
        {
            if (incidentId == Guid.Empty || string.IsNullOrWhiteSpace(text))
            {
                TempData["Err"] = "El comentario no puede estar vacío.";
                return RedirectToAction(nameof(Details), new { id = incidentId });
            }

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                TempData["Err"] = "Usuario no identificado.";
                return RedirectToAction(nameof(Details), new { id = incidentId });
            }

            using var cn = new SqlConnection(_configuration.GetConnectionString("CallCenterDb"));
            cn.Open();
            using var tx = cn.BeginTransaction();

            try
            {
                // Insertar comentario
                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.IncidentComments(IncidentId, AuthorUserId, Text, CreatedByUserId)
VALUES (@IncidentId, @UserId, @Text, @UserId);", cn, tx))
                {
                    cmd.Parameters.Add("@IncidentId", SqlDbType.UniqueIdentifier).Value = incidentId;
                    cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                    cmd.Parameters.Add("@Text", SqlDbType.NVarChar, 2000).Value = text.Trim();
                    cmd.ExecuteNonQuery();
                }

                // Actualizar estado a EnAnalisis solo si no está Resuelto o Cerrado
                using (var cmd = new SqlCommand(@"
UPDATE dbo.Incidents
SET State = N'EnAnalisis',
    UpdatedAt = SYSUTCDATETIME(),
    UpdatedByUserId = @UserId
WHERE Id = @Id AND IsDeleted = 0 
  AND State NOT IN (N'Resuelto', N'Cerrado');", cn, tx))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = incidentId;
                    cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }

            TempData["Ok"] = "Comentario agregado.";
            return RedirectToAction(nameof(Details), new { id = incidentId });
        }


        [HttpGet]
        public IActionResult Reassign(Guid id)
        {
            using var cn = new SqlConnection(_configuration.GetConnectionString("CallCenterDb"));
            cn.Open();

            string? state;
            using (var cmd = new SqlCommand("SELECT State FROM dbo.Incidents WHERE Id=@Id AND IsDeleted=0", cn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
                state = cmd.ExecuteScalar() as string;
                if (state is null) return NotFound();
            }
            if (state is "Resuelto" or "Cerrado")
            {
                TempData["Err"] = $"La incidencia ya está {state}.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var vm = new IncidentReassignViewModel { Id = id };

            using (var cmd = new SqlCommand(
                "SELECT Number, AssignedToUserId FROM dbo.Incidents WHERE Id=@Id AND IsDeleted=0", cn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
                using var rd = cmd.ExecuteReader();
                if (!rd.Read()) return NotFound();
                vm.Number = rd.GetString(0);
                if (!rd.IsDBNull(1)) vm.AssignedToUserId = rd.GetGuid(1);
            }

            var users = new List<SelectListItem>();
            using (var cmd = new SqlCommand(
                "SELECT Id, Username FROM dbo.Users WHERE IsDeleted = 0 ORDER BY Username", cn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                    users.Add(new SelectListItem(rd.GetString(1), rd.GetGuid(0).ToString()));
            }
            vm.Users = users;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reassign(IncidentReassignViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                using var cnReload = new SqlConnection(_configuration.GetConnectionString("CallCenterDb"));
                using var cmdReload = new SqlCommand("SELECT Id, Username FROM dbo.Users WHERE IsDeleted=0 ORDER BY Username", cnReload);
                cnReload.Open();
                using var rd = cmdReload.ExecuteReader();
                var list = new List<SelectListItem>();
                while (rd.Read()) list.Add(new SelectListItem(rd.GetString(1), rd.GetGuid(0).ToString()));
                vm.Users = list;
                return View(vm);
            }

            var actorId = GetCurrentUserId();
            if (actorId == Guid.Empty)
            {
                ModelState.AddModelError("", "Usuario no identificado.");
                return View(vm);
            }

            using var cn = new SqlConnection(_configuration.GetConnectionString("CallCenterDb"));
            cn.Open();
            using var tx = cn.BeginTransaction();

            try
            {
                // Actualizar asignación y estado EnAnalisis
                using (var cmd = new SqlCommand(@"
UPDATE dbo.Incidents
SET AssignedToUserId = @NewUser,
    State = N'EnAnalisis',
    UpdatedAt = SYSUTCDATETIME(),
    UpdatedByUserId = @Updater
WHERE Id = @Id AND IsDeleted = 0
  AND State NOT IN (N'Resuelto', N'Cerrado');", cn, tx))
                {
                    cmd.Parameters.Add("@NewUser", SqlDbType.UniqueIdentifier).Value = vm.AssignedToUserId;
                    cmd.Parameters.Add("@Updater", SqlDbType.UniqueIdentifier).Value = actorId;
                    cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = vm.Id;
                    cmd.ExecuteNonQuery();
                }

                // Insertar registro histórico en IncidentAssignments
                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.IncidentAssignments
(IncidentId, AssignedByUserId, AssignedToUserId, Note, CreatedByUserId)
VALUES (@IncidentId, @AssignedBy, @AssignedTo, @Note, @CreatedBy);", cn, tx))
                {
                    cmd.Parameters.Add("@IncidentId", SqlDbType.UniqueIdentifier).Value = vm.Id;
                    cmd.Parameters.Add("@AssignedBy", SqlDbType.UniqueIdentifier).Value = actorId;
                    cmd.Parameters.Add("@AssignedTo", SqlDbType.UniqueIdentifier).Value = vm.AssignedToUserId;
                    cmd.Parameters.Add("@Note", SqlDbType.NVarChar, 1000).Value = (object?)vm.Reason ?? DBNull.Value;
                    cmd.Parameters.Add("@CreatedBy", SqlDbType.UniqueIdentifier).Value = actorId;
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }

            TempData["Ok"] = "Incidencia reasignada correctamente.";
            return RedirectToAction(nameof(Details), new { id = vm.Id });
        }



        [HttpGet]
        public IActionResult Create()
        {
            var vm = new IncidentCreateViewModel();

            using var cn = new SqlConnection(_configuration.GetConnectionString("CallCenterDb"));
            cn.Open();

            using (var cmd = new SqlCommand("SELECT Id, Name FROM Customers WHERE IsDeleted = 0 ORDER BY Name", cn))
            using (var rd = cmd.ExecuteReader())
            {
                var list = new List<SelectListItem>();
                while (rd.Read())
                    list.Add(new SelectListItem(rd.GetString(1), rd.GetGuid(0).ToString()));
                vm.Customers = list;
            }

            
            using (var cmd = new SqlCommand("SELECT Id, Name FROM IncidentTypes ORDER BY Name", cn))
            using (var rd = cmd.ExecuteReader())
            {
                var list = new List<SelectListItem>();
                while (rd.Read())
                    list.Add(new SelectListItem(rd.GetString(1), rd.GetGuid(0).ToString()));
                vm.Types = list;
            }

            using (var cmd = new SqlCommand("SELECT Id, Name FROM Priorities ORDER BY Name", cn))
            using (var rd = cmd.ExecuteReader())
            {
                var list = new List<SelectListItem>();
                while (rd.Read())
                    list.Add(new SelectListItem(rd.GetString(1), rd.GetGuid(0).ToString()));
                vm.Priorities = list;
            }

            using (var cmd = new SqlCommand("SELECT Id, Username FROM Users WHERE IsDeleted = 0 ORDER BY Username", cn))
            using (var rd = cmd.ExecuteReader())
            {
                var list = new List<SelectListItem>();
                while (rd.Read())
                    list.Add(new SelectListItem(rd.GetString(1), rd.GetGuid(0).ToString()));
                vm.Users = list;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public IActionResult Create(IncidentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // recargar combos si es necesario (omito por brevedad)
                return View(model);
            }

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                ModelState.AddModelError("", "Usuario no identificado.");
                return View(model);
            }

            var number = $"INC-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}";
            Guid newId;

            using var cn = new SqlConnection(_configuration.GetConnectionString("CallCenterDb"));
            cn.Open();
            using var tx = cn.BeginTransaction();

            try
            {
                // Insert Incidents y devolver Id
                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Incidents
(Number, CustomerId, TypeId, PriorityId, Problem, State, OwnerUserId, AssignedToUserId, CreatedByUserId)
OUTPUT inserted.Id
VALUES (@Number,@CustomerId,@TypeId,@PriorityId,@Problem,N'Abierto',@OwnerUserId,@AssignedToUserId,@CreatedByUserId);", cn, tx))
                {
                    cmd.Parameters.Add("@Number", SqlDbType.NVarChar, 30).Value = number;
                    cmd.Parameters.Add("@CustomerId", SqlDbType.UniqueIdentifier).Value = model.CustomerId;
                    cmd.Parameters.Add("@TypeId", SqlDbType.UniqueIdentifier).Value = model.TypeId;
                    cmd.Parameters.Add("@PriorityId", SqlDbType.UniqueIdentifier).Value = model.PriorityId;
                    cmd.Parameters.Add("@Problem", SqlDbType.NVarChar, 2000).Value = model.Problem.Trim();
                    cmd.Parameters.Add("@OwnerUserId", SqlDbType.UniqueIdentifier).Value = userId;
                    cmd.Parameters.Add("@AssignedToUserId", SqlDbType.UniqueIdentifier).Value = model.AssignedToUserId;
                    cmd.Parameters.Add("@CreatedByUserId", SqlDbType.UniqueIdentifier).Value = userId;

                    newId = (Guid)cmd.ExecuteScalar()!;
                }

                // Guardar archivos (si hay)
                if (model.Files is { Count: > 0 })
                {
                    var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var relDir = Path.Combine("uploads", "incidents", newId.ToString("N"));
                    var absDir = Path.Combine(root, relDir);
                    Directory.CreateDirectory(absDir);

                    foreach (var f in model.Files.Where(f => f != null && f.Length > 0))
                    {
                        // nombre seguro
                        var ext = Path.GetExtension(f.FileName);
                        var stored = $"{Guid.NewGuid():N}{ext}";
                        var absPath = Path.Combine(absDir, stored);
                        var relPath = "/" + Path.Combine(relDir, stored).Replace("\\", "/");

                        using (var fs = new FileStream(absPath, FileMode.Create))
                            f.CopyTo(fs);

                        using var cmdFile = new SqlCommand(@"
INSERT INTO dbo.IncidentFiles(IncidentId, OriginalName, StoredName, ContentType, SizeBytes, Path, CreatedByUserId)
VALUES (@IncidentId, @OriginalName, @StoredName, @ContentType, @SizeBytes, @Path, @UserId);", cn, tx);

                        cmdFile.Parameters.Add("@IncidentId", SqlDbType.UniqueIdentifier).Value = newId;
                        cmdFile.Parameters.Add("@OriginalName", SqlDbType.NVarChar, 260).Value = Path.GetFileName(f.FileName);
                        cmdFile.Parameters.Add("@StoredName", SqlDbType.NVarChar, 260).Value = stored;
                        cmdFile.Parameters.Add("@ContentType", SqlDbType.NVarChar, 100).Value = (object?)f.ContentType ?? DBNull.Value;
                        cmdFile.Parameters.Add("@SizeBytes", SqlDbType.BigInt).Value = f.Length;
                        cmdFile.Parameters.Add("@Path", SqlDbType.NVarChar, 500).Value = relPath;
                        cmdFile.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

                        cmdFile.ExecuteNonQuery();
                    }
                }

                tx.Commit();
            }
            catch (SqlException ex) when (ex.Number is 2627 or 2601) // Unique Number
            {
                tx.Rollback();
                // reintento simple: podrías regenerar number y reinsertar; omito para brevedad
                ModelState.AddModelError("", "No se pudo generar el número único. Volvé a intentar.");
                return View(model);
            }
            catch
            {
                tx.Rollback();
                throw;
            }

            TempData["Ok"] = "Incidencia creada correctamente.";
            return RedirectToAction(nameof(Details), new { id = newId });
        }


        private static string GenerateNumber()
            => $"INC-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}";

        private Guid GetCurrentUserId()
        {
            var str = User?.FindFirst("UserId")?.Value;
            return Guid.TryParse(str, out var id) ? id : Guid.Empty;
        }


        private static IncidentHistoryItemViewModel OpenEvent(DateTime createdAt, string openedByUser)
        => new() { When = createdAt, Kind = "Abierto", Who = openedByUser, Detail = null };

        private List<IncidentHistoryItemViewModel> LoadHistory(SqlConnection cn, Guid incidentId, DateTime createdAt, Guid createdByUserId)
        {
            var items = new List<IncidentHistoryItemViewModel>();

            string openedBy = "";
            using (var cmd = new SqlCommand("SELECT ISNULL(Username,N'') FROM dbo.Users WHERE Id=@id", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = createdByUserId;
                openedBy = (string?)cmd.ExecuteScalar() ?? "";
            }
            items.Add(OpenEvent(createdAt, openedBy));

            using (var cmd = new SqlCommand(@"
SELECT ia.CreatedAt,
       uBy.Username   AS ByUser,
       uTo.Username   AS ToUser,
       ia.Note
FROM dbo.IncidentAssignments ia
JOIN dbo.Users uBy ON uBy.Id = ia.AssignedByUserId
JOIN dbo.Users uTo ON uTo.Id = ia.AssignedToUserId
WHERE ia.IncidentId = @IncidentId AND ia.IsDeleted = 0
ORDER BY ia.CreatedAt ASC;", cn))
            {
                cmd.Parameters.Add("@IncidentId", SqlDbType.UniqueIdentifier).Value = incidentId;
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    var when = rd.GetDateTime(0);
                    var by = rd.GetString(1);
                    var to = rd.GetString(2);
                    var note = rd.IsDBNull(3) ? null : rd.GetString(3);

                    items.Add(new IncidentHistoryItemViewModel
                    {
                        When = when,
                        Kind = "Reasignado",
                        Who = by,
                        Detail = string.IsNullOrWhiteSpace(note) ? $"a {to}" : $"a {to} • {note}"
                    });
                }
            }

            using (var cmd = new SqlCommand(@"
SELECT ic.CreatedAt,
       u.Username,
       ic.Text
FROM dbo.IncidentComments ic
JOIN dbo.Users u ON u.Id = ic.AuthorUserId
WHERE ic.IncidentId = @IncidentId AND ic.IsDeleted = 0
  AND (ic.Text LIKE N'RESUELTO:%' OR ic.Text LIKE N'CERRADO:%' OR ic.Text LIKE N'REABIERTO:%')
ORDER BY ic.CreatedAt ASC;", cn))
            {
                cmd.Parameters.Add("@IncidentId", SqlDbType.UniqueIdentifier).Value = incidentId;
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    var when = rd.GetDateTime(0);
                    var who = rd.GetString(1);
                    var text = rd.GetString(2);

                    string kind = text.StartsWith("RESUELTO:", StringComparison.OrdinalIgnoreCase) ? "Resuelto"
                                : text.StartsWith("CERRADO:", StringComparison.OrdinalIgnoreCase) ? "Cerrado"
                                : "Reabierto";
                    var detail = text[(text.IndexOf(':') + 1)..].Trim();

                    items.Add(new IncidentHistoryItemViewModel
                    {
                        When = when,
                        Kind = kind,
                        Who = who,
                        Detail = string.IsNullOrWhiteSpace(detail) ? null : detail
                    });
                }
            }

            items.Sort((a, b) => a.When.CompareTo(b.When));
            return items;
        }
        [HttpGet]
        public IActionResult Resolve(Guid id)
        {
            using var cn = new SqlConnection(_configuration.GetConnectionString("CallCenterDb"));
            using var cmd = new SqlCommand("SELECT Id, Number, State FROM dbo.Incidents WHERE Id=@Id AND IsDeleted=0", cn);
            cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
            cn.Open();
            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return NotFound();

            var state = rd.GetString(2);
            if (state is "Resuelto" or "Cerrado")
            {
                TempData["Err"] = $"La incidencia ya está {state}.";
                return RedirectToAction(nameof(Details), new { id });
            }
            return View(new Incident { Id = rd.GetGuid(0), Number = rd.GetString(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Resolve(Incident model)
        {
            using var cnState = new SqlConnection(_configuration.GetConnectionString("CallCenterDb"));
            using var cmdState = new SqlCommand("SELECT State FROM dbo.Incidents WHERE Id=@Id AND IsDeleted=0", cnState);
            cmdState.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = model.Id;
            cnState.Open();
            var state = cmdState.ExecuteScalar() as string ?? "";
            if (state is "Resuelto" or "Cerrado")
            {
                TempData["Err"] = $"La incidencia ya está {state}.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }

            if (model.Id == Guid.Empty || string.IsNullOrWhiteSpace(model.ResolutionNote))
            {
                TempData["Err"] = "La nota de resolución es obligatoria.";
                return RedirectToAction(nameof(Resolve), new { id = model.Id });
            }

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                TempData["Err"] = "Usuario no identificado.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }

            using var cn = new SqlConnection(_configuration.GetConnectionString("CallCenterDb"));
            cn.Open();
            using var tx = cn.BeginTransaction();

            try
            {
                using (var cmd = new SqlCommand(@"
UPDATE dbo.Incidents
SET State = N'Resuelto',
    ResolutionNote = @Note,
    UpdatedAt = SYSUTCDATETIME(),
    UpdatedByUserId = @UserId
WHERE Id = @Id AND IsDeleted = 0;", cn, tx))
                {
                    cmd.Parameters.Add("@Note", SqlDbType.NVarChar, 2000).Value = model.ResolutionNote!.Trim();
                    cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                    cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = model.Id;
                    if (cmd.ExecuteNonQuery() == 0) { tx.Rollback(); return NotFound(); }
                }

                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.IncidentComments(IncidentId, AuthorUserId, Text, CreatedByUserId)
VALUES (@IncidentId, @UserId, @Text, @UserId);", cn, tx))
                {
                    cmd.Parameters.Add("@IncidentId", SqlDbType.UniqueIdentifier).Value = model.Id;
                    cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                    cmd.Parameters.Add("@Text", SqlDbType.NVarChar, 2000).Value = "RESUELTO: " + model.ResolutionNote!.Trim();
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch { tx.Rollback(); throw; }

            TempData["Ok"] = "Incidencia resuelta.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
            throw new NotImplementedException();
        }

        [HttpGet]
        public IActionResult Close(Guid id)
        {
            using var cn = new SqlConnection(_configuration.GetConnectionString("CallCenterDb"));
            using var cmd = new SqlCommand("SELECT Id, Number, State FROM dbo.Incidents WHERE Id=@Id AND IsDeleted=0", cn);
            cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
            cn.Open();
            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return NotFound();

            var state = rd.GetString(2);
            if (state is "Cerrado")
            {
                TempData["Err"] = "La incidencia ya está Cerrada.";
                return RedirectToAction(nameof(Details), new { id });
            }
            return View(new Incident { Id = rd.GetGuid(0), Number = rd.GetString(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Close(Incident model)
        {
            using var cnState = new SqlConnection(_configuration.GetConnectionString("CallCenterDb"));
            using var cmdState = new SqlCommand("SELECT State FROM dbo.Incidents WHERE Id=@Id AND IsDeleted=0", cnState);
            cmdState.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = model.Id;
            cnState.Open();
            var state = cmdState.ExecuteScalar() as string ?? "";
            if (state is "Cerrado")
            {
                TempData["Err"] = "La incidencia ya está Cerrada.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            if (model.Id == Guid.Empty || string.IsNullOrWhiteSpace(model.CloseComment))
            {
                TempData["Err"] = "El comentario de cierre es obligatorio.";
                return RedirectToAction(nameof(Close), new { id = model.Id });
            }

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                TempData["Err"] = "Usuario no identificado.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }

            using var cn = new SqlConnection(_configuration.GetConnectionString("CallCenterDb"));
            cn.Open();
            using var tx = cn.BeginTransaction();

            try
            {
                using (var cmd = new SqlCommand(@"
UPDATE dbo.Incidents
SET State = N'Cerrado',
    CloseComment = @Comment,
    UpdatedAt = SYSUTCDATETIME(),
    UpdatedByUserId = @UserId
WHERE Id = @Id AND IsDeleted = 0;", cn, tx))
                {
                    cmd.Parameters.Add("@Comment", SqlDbType.NVarChar, 2000).Value = model.CloseComment!.Trim();
                    cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                    cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = model.Id;
                    if (cmd.ExecuteNonQuery() == 0) { tx.Rollback(); return NotFound(); }
                }

                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.IncidentComments(IncidentId, AuthorUserId, Text, CreatedByUserId)
VALUES (@IncidentId, @UserId, @Text, @UserId);", cn, tx))
                {
                    cmd.Parameters.Add("@IncidentId", SqlDbType.UniqueIdentifier).Value = model.Id;
                    cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                    cmd.Parameters.Add("@Text", SqlDbType.NVarChar, 2000).Value = "CERRADO: " + model.CloseComment!.Trim();
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch { tx.Rollback(); throw; }

            TempData["Ok"] = "Incidencia cerrada.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
            throw new NotImplementedException();
        }
        [HttpGet]
        public IActionResult DownloadFile(Guid id)
        {
            using var cn = new SqlConnection(_configuration.GetConnectionString("CallCenterDb"));
            using var cmd = new SqlCommand(@"
SELECT f.OriginalName, f.StoredName, f.ContentType, f.Path, i.Id as IncidentId
FROM dbo.IncidentFiles f
JOIN dbo.Incidents i ON i.Id = f.IncidentId
WHERE f.Id = @Id AND f.IsDeleted = 0;", cn);
            cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;

            cn.Open();
            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return NotFound();

            var original = rd.GetString(0);
            var stored = rd.GetString(1);
            var contentType = rd.IsDBNull(2) ? "application/octet-stream" : rd.GetString(2);
            var relPath = rd.GetString(3);
            var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var absPath = Path.Combine(root, relPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (!System.IO.File.Exists(absPath)) return NotFound();

            var bytes = System.IO.File.ReadAllBytes(absPath);
            return File(bytes, contentType, original);
        }
    }

}
