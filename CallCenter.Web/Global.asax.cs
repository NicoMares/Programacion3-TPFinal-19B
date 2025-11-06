using System;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace CallCenter.Web
{
    public class Global : HttpApplication
    {
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            var ck = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (ck == null) return;

            var ticket = FormsAuthentication.Decrypt(ck.Value);
            if (ticket == null) return;

            var roles = string.IsNullOrWhiteSpace(ticket.UserData) ? Array.Empty<string>() : new[] { ticket.UserData };
            Context.User = new GenericPrincipal(new FormsIdentity(ticket), roles);
        }
        void Application_Start(object sender, EventArgs e)
        {
            System.Web.UI.ValidationSettings.UnobtrusiveValidationMode =System.Web.UI.UnobtrusiveValidationMode.None;
        }

    }
}
