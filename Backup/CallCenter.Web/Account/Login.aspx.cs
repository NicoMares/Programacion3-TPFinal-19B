using System;
using System.Web.Security;
using System.Web.UI;
using CallCenter.Business.Repositories;
using CallCenter.Business.Services;

namespace CallCenter.Web.Account
{
    public partial class Login : Page
    {
        private IUserService _userService;

        protected void Page_Init(object sender, EventArgs e)
        {
            var userRepo = new UserRepository();
            _userService = new UserService(userRepo);
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string role;
            if (_userService.ValidateCredentials(txtEmail.Text.Trim(), txtPassword.Text, out role))
            {
                FormsAuthentication.RedirectFromLoginPage(txtEmail.Text.Trim(), false);
            }
            else
            {
                lblError.Text = "Credenciales inválidas o usuario inactivo.";
            }
        }
    }
}
