using System;
using System.Data;
using System.Data.SqlClient;

namespace CallCenter.Web
{
    public partial class _Default : System.Web.UI.Page
    {
        private string _cs;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var role = GetCurrentUserRole();
                // Crear Usuario: solo Supervisor
                pnlCreateUser.Visible = role != null &&
                    role.Trim().Equals("Supervisor", StringComparison.OrdinalIgnoreCase);

                // (por si también tenés la card de "Listar Usuarios" para Admin/Supervisor)
                 pnlListUsers.Visible = role != null &&
                  (role.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ||
                   role.Equals("Supervisor", StringComparison.OrdinalIgnoreCase));
            }
        }


        private string GetCurrentUserRole()
        {
            var username = Context?.User?.Identity?.Name ?? "";
            if (string.IsNullOrWhiteSpace(username)) return null;

            var cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
            using (var cn = new SqlConnection(cs))
            {
                cn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT TOP 1 [Role] FROM dbo.Users WHERE Username=@u AND IsDeleted=0 AND IsBlocked=0;", cn))
                {
                    cmd.Parameters.Add("@u", SqlDbType.NVarChar, 100).Value = username;
                    var o = cmd.ExecuteScalar();
                    return o == null ? null : Convert.ToString(o);
                }
            }
        }
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            System.Web.Security.FormsAuthentication.SignOut();
            Session.Clear();
            Response.Redirect("~/Account/Login.aspx");
        }

    }
}
