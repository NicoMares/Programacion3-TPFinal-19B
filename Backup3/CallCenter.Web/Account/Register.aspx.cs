using System;
using CallCenter.Business.Repositories;
using CallCenter.Business.Services;

namespace CallCenter.Web.Account
{
    public partial class Register : System.Web.UI.Page
    {
        private RegistrationService _reg;

        protected void Page_Init(object sender, EventArgs e)
        {
            string cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
            UserRepository repo = new UserRepository(cs);
            _reg = new RegistrationService(repo);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Web.UI.ValidationSettings.UnobtrusiveValidationMode =
                System.Web.UI.UnobtrusiveValidationMode.None;
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text == null ? "" : txtUsername.Text.Trim();
            string fullName = txtFullName.Text == null ? "" : txtFullName.Text.Trim();
            string email = txtEmail.Text == null ? "" : txtEmail.Text.Trim();
            string pass = txtPassword.Text ?? "";
            string confirm = txtConfirm.Text ?? "";

            if (pass != confirm)
            {
                lblMsg.CssClass = "text-danger fw-semibold";
                lblMsg.Text = "Las contraseñas no coinciden.";
                return;
            }

            System.Guid id;
            string err;
            bool ok = _reg.RegisterTelefonista(username, fullName, email, pass, out id, out err);
            if (ok)
            {
                // éxito → redirigimos a Login con mensaje
                Response.Redirect("~/Account/Login.aspx?registered=1");
            }
            else
            {
                lblMsg.CssClass = "text-danger fw-semibold";
                lblMsg.Text = err;
            }
        }
    }
}
