namespace Progra3_TPFinal_19B.Services
{
    public static class EmailTemplates
    {
        public static string ResetPassword(string userName, string actionUrl)
        {
            var year = DateTime.UtcNow.Year.ToString();

            // --- HTML del mail ---
            string html = @"
<!doctype html>
<html lang='es'>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width,initial-scale=1'>
  <title>Restablecer contraseña</title>
</head>
<body style='margin:0; padding:0; background:#f5f7fb; font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif;'>
  <center style='width:100%; background:#f5f7fb;'>
    <table width='100%' cellspacing='0' cellpadding='0' border='0' style='background:#f5f7fb;'>
      <tr><td align='center' style='padding:24px 12px;'>
        <table width='600' cellspacing='0' cellpadding='0' border='0' style='width:600px; background:#ffffff; border-radius:12px; box-shadow:0 6px 24px rgba(20,30,50,.06); overflow:hidden;'>
          <tr>
            <td align='center' style='background:#0d6efd; padding:18px 24px;'>
              <div style='font-weight:700; font-size:18px; color:#fff;'>Progra3_TPFinal_19B • Call Center</div>
            </td>
          </tr>

          <tr>
            <td style='padding:32px; color:#192132;'>
              <div style='font-size:24px; font-weight:700; margin:0 0 8px;'>Restablecé tu contraseña</div>
              <p style='margin:0 0 16px;'>Hola <strong>{{UserName}}</strong>, recibimos una solicitud para restablecer tu contraseña.</p>
              <p style='margin:0 0 24px;'>Hacé clic en el botón para crear una nueva. El enlace vence en <strong>30 minutos</strong>.</p>

              <table cellspacing='0' cellpadding='0' border='0' style='margin:0 0 16px;'>
                <tr>
                  <td align='center' bgcolor='#0d6efd' style='border-radius:10px;'>
                    <a href='{{ActionUrl}}' style='display:inline-block; padding:12px 20px; font-weight:600; text-decoration:none; color:#ffffff; background:#0d6efd; border-radius:10px;'>Restablecer contraseña</a>
                  </td>
                </tr>
              </table>

              <p style='margin:16px 0 0; font-size:12px; color:#6c757d;'>
                Si el botón no funciona, copiá y pegá este enlace:<br>
                <span style='word-break:break-all; color:#0d6efd;'>{{ActionUrl}}</span>
              </p>

              <div style='margin-top:24px; padding:16px; background:#f8fafc; border:1px solid #eef2f7; border-radius:10px; color:#52627a;'>
                Si no solicitaste este cambio, podés ignorar este mensaje. Tu contraseña actual seguirá funcionando.
              </div>
            </td>
          </tr>

          <tr>
            <td align='center' style='padding:20px; background:#ffffff; border-top:1px solid #eef2f7; color:#8a94a6; font-size:12px;'>
              © {{Year}} Progra3_TPFinal_19B · Call Center<br>
              Este es un correo automático, por favor no respondas.
            </td>
          </tr>
        </table>
      </td></tr>
    </table>
  </center>
</body>
</html>";

            // Reemplazos dinámicos
            return html
                .Replace("{{UserName}}", System.Net.WebUtility.HtmlEncode(userName ?? ""))
                .Replace("{{ActionUrl}}", actionUrl ?? "#")
                .Replace("{{Year}}", year);
        }
    }
}
