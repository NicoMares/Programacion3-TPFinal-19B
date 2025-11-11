using System;
using System.Data;
using System.Data.SqlClient;
using CallCenter.Business.Helpers;

namespace CallCenter.Web.Users
{
    public partial class Create : System.Web.UI.Page
    {
        private string _cs;

        protected void Page_Load(object sender, EventArgs e)
        {
            _cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;

            if (!IsPostBack)
            {
                if (!IsCurrentUserSupervisor())
                    Response.Redirect("~/Default.aspx");
            }
        }
        private bool IsCurrentUserSupervisor()
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
                    return string.Equals(role, "Supervisor", StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string pass = txtPassword.Text.Trim();
            string role = ddlRole.SelectedValue;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(role))
            {
                lblMsg.CssClass = "text-danger text-center";
                lblMsg.Text = "⚠️ Complete todos los campos antes de continuar.";
                return;
            }

            string hash = AppHelpers.Sha256(pass);

            try
            {
                using (SqlConnection cn = new SqlConnection(_cs))
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.Users(Id, Username, FullName, Email, PasswordHash, Role, CreatedAt, IsDeleted, IsBlocked)
VALUES(NEWID(), @u, @f, @e, @p, @r, SYSUTCDATETIME(), 0, 0)", cn))
                    {
                        cmd.Parameters.Add("@u", SqlDbType.NVarChar, 100).Value = username;
                        cmd.Parameters.Add("@f", SqlDbType.NVarChar, 200).Value = username;
                        cmd.Parameters.Add("@e", SqlDbType.NVarChar, 256).Value = email;
                        cmd.Parameters.Add("@p", SqlDbType.NVarChar, 512).Value = hash;
                        cmd.Parameters.Add("@r", SqlDbType.NVarChar, 20).Value = role;
                        cmd.ExecuteNonQuery();
                    }
                }

                // ✅ Mostrar mensaje y limpiar formulario
                lblMsg.CssClass = "text-success text-center";
                lblMsg.Text = "✅ Usuario creado correctamente.";

                txtUsername.Text = string.Empty;
                txtEmail.Text = string.Empty;
                txtPassword.Text = string.Empty;
                ddlRole.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                lblMsg.CssClass = "text-danger text-center";
                lblMsg.Text = "❌ Error al crear el usuario: " + ex.Message;
            }
        }
    }
}
