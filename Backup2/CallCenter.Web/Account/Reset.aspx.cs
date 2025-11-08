using System;
using System.Threading;
using System.Web.UI;
using CallCenter.Business.Repositories;
using CallCenter.Business.Services;

namespace CallCenter.Web.Account
{
    public partial class Reset : Page
    {
        private PasswordResetService _svc;

        protected void Page_Init(object sender, EventArgs e)
        {
            string cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
            UserRepository users = new UserRepository(cs);
            PasswordResetTokenRepository tokens = new PasswordResetTokenRepository(cs);
            _svc = new PasswordResetService(users, tokens);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Web.UI.ValidationSettings.UnobtrusiveValidationMode =
                System.Web.UI.UnobtrusiveValidationMode.None;
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            string token = Request.QueryString["token"];
            string pass = txtPassword.Text ?? "";
            string confirm = txtConfirm.Text ?? "";

            if (string.IsNullOrWhiteSpace(token))
            {
                lblMsg.CssClass = "text-danger fw-semibold";
                lblMsg.Text = "El enlace no es válido.";
                return;
            }

            if (pass != confirm)
            {
                lblMsg.CssClass = "text-danger fw-semibold";
                lblMsg.Text = "Las contraseñas no coinciden.";
                return;
            }

            bool ok = _svc.ResetPassword(token, pass);

            if (ok)
            {
                lblMsg.CssClass = "text-success fw-semibold";
                lblMsg.Text = "Contraseña actualizada correctamente. Redirigiendo al login...";

                // Redirección automática al login tras 2 s
                string script = "setTimeout(function(){ window.location='Login.aspx'; }, 2000);";
                ClientScript.RegisterStartupScript(this.GetType(), "redirect", script, true);
            }
            else
            {
                lblMsg.CssClass = "text-danger fw-semibold";
                lblMsg.Text = "El enlace expiró o ya fue utilizado.";
            }
        }
    }
}
