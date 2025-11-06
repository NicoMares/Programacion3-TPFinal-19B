using System;
using CallCenter.Business.Repositories;
using CallCenter.Business.Services;
using CallCenter.Web.Infrastructure;

namespace CallCenter.Web.Account
{
    public partial class Forgot : System.Web.UI.Page
    {
        private PasswordResetService _svc;
        private IEmailSender _mail;

        protected void Page_Init(object sender, EventArgs e)
        {
            string cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
            UserRepository users = new UserRepository(cs);
            PasswordResetTokenRepository tokens = new PasswordResetTokenRepository(cs);
            _svc = new PasswordResetService(users, tokens);
            _mail = new SmtpEmailSender();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Web.UI.ValidationSettings.UnobtrusiveValidationMode =
                System.Web.UI.UnobtrusiveValidationMode.None;
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text == null ? "" : txtEmail.Text.Trim();
            string token = _svc.RequestReset(email, TimeSpan.FromMinutes(60));

            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority) + ResolveUrl("~/");
            string link = baseUrl + "Account/Reset.aspx?token=" + token;

            string html =
                "<p>Recibimos una solicitud para restablecer tu contraseña.</p>" +
                "<p>Si fuiste vos, hacé clic en el siguiente enlace:</p>" +
                "<p><a href=\"" + link + "\">Restablecer contraseña</a></p>" +
                "<p>Este enlace expira en 60 minutos.</p>";

            if (!string.IsNullOrEmpty(token))
                _mail.Send(email, "Restablecer contraseña", html);

            // ✅ Mensaje visual Bootstrap con ícono e indicador de tiempo
            lblMsg.CssClass = "alert alert-success d-flex align-items-center mt-3";
            lblMsg.Text = @"
                <div class='d-flex align-items-center'>
                    <div class='spinner-border text-success me-3' role='status' style='width:1.5rem;height:1.5rem;'></div>
                    <div>
                        <strong>Correo enviado correctamente.</strong><br/>
                        Serás redirigido al inicio de sesión en <span id='countdown' class='fw-bold'>5</span> segundos...
                    </div>
                </div>";
            btnSend.Visible = false;
            txtEmail.Enabled = false;


            // ✅ Script contador regresivo + redirección
            string script = @"
                <script>
                    let s = 5;
                    const el = document.getElementById('countdown');
                    const timer = setInterval(() => {
                        s--;
                        if (el) el.textContent = s;
                        if (s <= 0) {
                            clearInterval(timer);
                            window.location = 'Login.aspx';
                        }
                    }, 1000);
                </script>";
            ClientScript.RegisterStartupScript(this.GetType(), "redir", script, false);
        }
    }
}
