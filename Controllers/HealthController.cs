// Controllers/HealthController.cs (reemplazo completo)
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient; // para DataSource/Database si querés castear
using Progra3_TPFinal_19B.Infrastructure; // IDbConnectionFactory

namespace Progra3_TPFinal_19B.Controllers
{
    public class HealthController : Controller
    {
        private readonly IDbConnectionFactory _factory;
        public HealthController(IDbConnectionFactory factory) => _factory = factory;

        [HttpGet("/health/db")]
        public async Task<IActionResult> Db()
        {
            try
            {
                using var conn = _factory.CreateConnection();
                await (conn as System.Data.Common.DbConnection)!.OpenAsync();
                return Ok("DB OK");
            }
            catch (Exception ex)
            {
                return StatusCode(503, $"DB DOWN: {ex.GetType().Name} - {ex.Message}");
            }
        }

        [HttpGet("/health/db/schema")]
        public async Task<IActionResult> DbSchema()
        {
            try
            {
                using var conn = _factory.CreateConnection();
                await (conn as System.Data.Common.DbConnection)!.OpenAsync();

                // Info básica servidor/base
                string? db = null, server = null;
                using (var cmd1 = conn.CreateCommand())
                {
                    cmd1.CommandText = "SELECT DB_NAME() AS Db, @@SERVERNAME AS ServerName";
                    using var r1 = await (cmd1 as System.Data.Common.DbCommand)!.ExecuteReaderAsync();
                    if (await r1.ReadAsync())
                    {
                        db = r1["Db"]?.ToString();
                        server = r1["ServerName"]?.ToString();
                    }
                }

                // Columnas de dbo.Users
                var cols = new List<string>();
                using (var cmd2 = conn.CreateCommand())
                {
                    cmd2.CommandText = @"
SELECT o.type_desc AS ObjectType, c.name AS ColumnName
FROM sys.objects o
JOIN sys.columns c ON c.object_id = o.object_id
WHERE o.object_id = OBJECT_ID('dbo.Users')
ORDER BY c.column_id";
                    using var r2 = await (cmd2 as System.Data.Common.DbCommand)!.ExecuteReaderAsync();
                    while (await r2.ReadAsync())
                        cols.Add($"{r2["ObjectType"]}:{r2["ColumnName"]}");
                }

                // DataSource/Database si es SqlConnection (opcional)
                string? dataSource = null;
                if (conn is SqlConnection sc)
                {
                    dataSource = sc.DataSource;
                    db ??= sc.Database;
                }

                return Ok(new
                {
                    DataSource = dataSource,
                    Database = db,
                    Server = server,
                    UsersObjectFound = cols.Count > 0,
                    Columns = cols
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
