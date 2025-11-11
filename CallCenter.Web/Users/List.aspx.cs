using System;
using System.Data;
using System.Data.SqlClient;

namespace CallCenter.Web.Users
{
    public partial class List : System.Web.UI.Page
    {
        private string _cs;

        protected void Page_Load(object sender, EventArgs e)
        {
            _cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
            if (!IsPostBack)
            {
                if (!IsAdminOrSupervisor()) { Response.Redirect("~/Default.aspx"); return; }
                BindGrid();
            }
        }

        private bool IsAdminOrSupervisor()
        {
            var username = Context?.User?.Identity?.Name ?? "";
            if (string.IsNullOrWhiteSpace(username)) return false;
            using (var cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT TOP 1 [Role] FROM dbo.Users WHERE Username=@u AND IsDeleted=0 AND IsBlocked=0;", cn))
                {
                    cmd.Parameters.Add("@u", SqlDbType.NVarChar, 100).Value = username;
                    var role = Convert.ToString(cmd.ExecuteScalar());
                    return role != null && (role.Equals("Administrador", StringComparison.OrdinalIgnoreCase)
                                         || role.Equals("Supervisor", StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        private void BindGrid()
        {
            var dt = new DataTable();
            using (var cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (var cmd = new SqlCommand(@"
SELECT Id, Username, Email, IsBlocked, CreatedAt
FROM dbo.Users
WHERE IsDeleted=0 AND Role=N'Telefonista'
ORDER BY Username;", cn))
                using (var rd = cmd.ExecuteReader())
                {
                    dt.Load(rd);
                }
            }
            dt.Columns.Add("CreatedAtLocal", typeof(DateTime));
            foreach (DataRow r in dt.Rows)
                r["CreatedAtLocal"] = Convert.ToDateTime(r["CreatedAt"]).ToLocalTime();

            gvUsers.DataSource = dt;
            gvUsers.DataBind();
        }

        protected void gvUsers_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName != "ToggleBlock") return;
            if (!Guid.TryParse(Convert.ToString(e.CommandArgument), out Guid id)) return;

            int n = 0;
            using (var cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (var cmd = new SqlCommand(@"
UPDATE dbo.Users
SET IsBlocked = CASE WHEN IsBlocked=1 THEN 0 ELSE 1 END
WHERE Id=@id AND Role=N'Telefonista';", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
                    n = cmd.ExecuteNonQuery();
                }
            }

            lblMsg.CssClass = (n > 0) ? "text-success mb-3" : "text-danger mb-3";
            lblMsg.Text = (n > 0) ? "Estado actualizado." : "No se pudo actualizar el estado.";
            BindGrid();
        }

        protected void gvUsers_RowDataBound(object sender, System.Web.UI.WebControls.GridViewRowEventArgs e)
        {
            if (e.Row.RowType != System.Web.UI.WebControls.DataControlRowType.DataRow) return;

            var drv = (System.Data.DataRowView)e.Row.DataItem;
            bool isBlocked = Convert.ToBoolean(drv["IsBlocked"]);

            // Badge de estado con ícono
            var litEstado = (System.Web.UI.WebControls.Literal)e.Row.FindControl("litEstado");
            if (litEstado != null)
                litEstado.Text = isBlocked
                    ? "<span class='badge bg-danger'><i class=\"bi bi-slash-circle\"></i> Bloqueado</span>"
                    : "<span class='badge bg-success'><i class=\"bi bi-check-circle\"></i> Activo</span>";

            // Botón con ícono y texto
            var lnk = (System.Web.UI.WebControls.LinkButton)e.Row.FindControl("lnkToggle");
            if (lnk != null)
            {
                lnk.CssClass = "btn btn-sm " + (isBlocked ? "btn-success" : "btn-danger");
                lnk.Controls.Clear(); // importante para no mezclar con "..."
                lnk.Controls.Add(new System.Web.UI.WebControls.Literal
                {
                    Text = isBlocked
                        ? "<i class='bi bi-unlock'></i> Desbloquear"
                        : "<i class='bi bi-lock'></i> Bloquear"
                });
            }
        }

    }
}
