using System;
using System.Web.Security;
using CallCenter.Business.Repositories;
using CallCenter.Business.Services;

namespace CallCenter.Web.Account
{
    public partial class Login : System.Web.UI.Page
    {
        private AuthService _auth;

        protected void Page_Init(object sender, EventArgs e)
        {
            var cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
            var repo = new UserRepository(cs);
            _auth = new AuthService(repo);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Web.UI.ValidationSettings.UnobtrusiveValidationMode =
                System.Web.UI.UnobtrusiveValidationMode.None;

            if (!IsPostBack && Request.QueryString["registered"] == "1")
            {
                lblError.CssClass = "d-block mt-3 text-success fw-semibold";
                lblError.Text = "Cuenta creada. Podés iniciar sesión.";
            }
        }



        protected void btnLogin_Click(object sender, EventArgs e)
        {
            var login = txtLogin.Text?.Trim() ?? "";   // puede ser Username o Email
            var pass = txtPassword.Text ?? "";

            if (_auth.TryLogin(login, pass, out var user))
            {
                var ticket = new FormsAuthenticationTicket(
                    1, user.Username, DateTime.Now, DateTime.Now.AddMinutes(120),
                    chkRemember.Checked, user.Role, FormsAuthentication.FormsCookiePath);

                var enc = FormsAuthentication.Encrypt(ticket);
                var cookie = new System.Web.HttpCookie(FormsAuthentication.FormsCookieName, enc)
                {
                    HttpOnly = true,
                    Path = FormsAuthentication.FormsCookiePath
                };
                if (chkRemember.Checked) cookie.Expires = ticket.Expiration;
                Response.Cookies.Add(cookie);

                var returnUrl = Request.QueryString["ReturnUrl"];
                Response.Redirect(string.IsNullOrEmpty(returnUrl) ? "~/Default.aspx" : returnUrl);
            }
            else
            {
                lblError.Text = "Credenciales inválidas o usuario bloqueado/eliminado.";
            }
        }
    }
}
