using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace CallCenter.Business.Helpers
{
    public static class AppHelpers
    {
        // ====== HTML ENCODE interno ======
        private static string HtmlEncode(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }

        // ====== Email ======
        public static void SendEmail(string to, string subject, string html)
        {
            var host = ConfigurationManager.AppSettings["Mail:Smtp"];
            var port = int.Parse(ConfigurationManager.AppSettings["Mail:Port"] ?? "587");
            var user = ConfigurationManager.AppSettings["Mail:User"];
            var pass = ConfigurationManager.AppSettings["Mail:Pass"];
            var fromName = ConfigurationManager.AppSettings["Mail:FromName"] ?? "Call Center";

            using (var msg = new MailMessage())
            {
                msg.From = new MailAddress(user, fromName);
                msg.To.Add(new MailAddress(to));
                msg.Subject = subject;
                msg.Body = html;
                msg.IsBodyHtml = true;

                using (var smtp = new SmtpClient(host, port))
                {
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential(user, pass);
                    smtp.Send(msg);
                }
            }
        }

        // ====== Email templates ======
        public static string BuildIncidentCreatedEmail(string customerName, Guid incidentId, string tipo, string prioridad, string problema)
        {
            string idCorto = incidentId.ToString().Substring(0, 8);
            string fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            return $@"<!doctype html><html><body style='font-family:Segoe UI,Arial,sans-serif;background:#f6f8fa;padding:24px;color:#111;'>
<table width='100%' cellpadding='0' cellspacing='0' style='max-width:640px;margin:auto;background:#fff;border:1px solid #e9ecef;border-radius:8px'>
<tr><td style='padding:20px 24px'>
<h2 style='margin:0 0 8px 0;color:#0d6efd'>Incidencia creada</h2>
<p style='margin:0;color:#555'>Hola {HtmlEncode(customerName)}, registramos tu reclamo.</p>
<hr style='border:none;border-top:1px solid #eee;margin:16px 0' />
<p><strong>N°:</strong> {idCorto}</p>
<p><strong>Fecha:</strong> {fecha}</p>
<p><strong>Estado inicial:</strong> Abierto</p>
<p><strong>Tipo:</strong> {HtmlEncode(tipo)}</p>
<p><strong>Prioridad:</strong> {HtmlEncode(prioridad)}</p>
<div style='background:#f8f9fa;border:1px solid #eee;border-radius:6px;padding:12px'>
<strong>Detalle del problema:</strong>
<div>{HtmlEncode(problema).Replace("\n", "<br/>")}</div>
</div>
<p style='margin-top:16px;color:#555;font-size:13px'>Te mantendremos informado ante cualquier actualización.</p>
<p style='margin:0;color:#999;font-size:12px'>Este correo fue generado automáticamente. No responda a este mensaje.</p>
</td></tr></table></body></html>";
        }

        public static string BuildIncidentResolvedEmail(string customerName, Guid incidentId, string tipo, string prioridad, string resolutionNote)
        {
            string idCorto = incidentId.ToString().Substring(0, 8);
            string fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            return $@"<!doctype html><html><body style='font-family:Segoe UI,Arial,sans-serif;background:#f6f8fa;padding:24px;color:#111;'>
<table width='100%' cellpadding='0' cellspacing='0' style='max-width:640px;margin:auto;background:#fff;border:1px solid #e9ecef;border-radius:8px'>
<tr><td style='padding:20px 24px'>
<h2 style='margin:0 0 8px 0;color:#198754'>Incidencia resuelta</h2>
<p style='margin:0;color:#555'>Hola {HtmlEncode(customerName)}, tu reclamo fue <strong>resuelto</strong>.</p>
<hr style='border:none;border-top:1px solid #eee;margin:16px 0' />
<p><strong>N°:</strong> {idCorto}</p>
<p><strong>Fecha:</strong> {fecha}</p>
<p><strong>Estado:</strong> Resuelto</p>
<p><strong>Tipo:</strong> {HtmlEncode(tipo)}</p>
<p><strong>Prioridad:</strong> {HtmlEncode(prioridad)}</p>
<div style='background:#f8f9fa;border:1px solid #eee;border-radius:6px;padding:12px'>
<strong>Detalle de resolución:</strong>
<div>{HtmlEncode(resolutionNote).Replace("\n", "<br/>")}</div>
</div>
<p style='margin-top:16px;color:#555;font-size:13px'>Si tu problema persiste, podés responder este correo o comunicarte nuevamente con soporte.</p>
<p style='margin:0;color:#999;font-size:12px'>Este correo fue generado automáticamente. No responda a este mensaje.</p>
</td></tr></table></body></html>";
        }

        public static string BuildIncidentClosedEmail(string customerName, Guid incidentId, string tipo, string prioridad, string closeComment)
        {
            string idCorto = incidentId.ToString().Substring(0, 8);
            string fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            return $@"<!doctype html><html><body style='font-family:Segoe UI,Arial,sans-serif;background:#f6f8fa;padding:24px;color:#111;'>
<table width='100%' cellpadding='0' cellspacing='0' style='max-width:640px;margin:auto;background:#fff;border:1px solid #e9ecef;border-radius:8px'>
<tr><td style='padding:20px 24px'>
<h2 style='margin:0 0 8px 0;color:#dc3545'>Incidencia cerrada</h2>
<p style='margin:0;color:#555'>Hola {HtmlEncode(customerName)}, tu reclamo fue <strong>cerrado</strong>.</p>
<hr style='border:none;border-top:1px solid #eee;margin:16px 0' />
<p><strong>N°:</strong> {idCorto}</p>
<p><strong>Fecha:</strong> {fecha}</p>
<p><strong>Estado:</strong> Cerrado</p>
<p><strong>Tipo:</strong> {HtmlEncode(tipo)}</p>
<p><strong>Prioridad:</strong> {HtmlEncode(prioridad)}</p>
<div style='background:#f8f9fa;border:1px solid #eee;border-radius:6px;padding:12px'>
<strong>Comentario de cierre:</strong>
<div>{HtmlEncode(closeComment).Replace("\n", "<br/>")}</div>
</div>
<p style='margin-top:16px;color:#555;font-size:13px'>Si necesitás volver a abrir el caso, respondé este correo o contactá a soporte.</p>
<p style='margin:0;color:#999;font-size:12px'>Este correo fue generado automáticamente. No responda a este mensaje.</p>
</td></tr></table></body></html>";
        }

        // ====== Hash SHA256 ======
        public static string Sha256(string input)
        {
            if (input == null) input = "";
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        // ====== Helpers DB ======
        public static SqlConnection NewConn(string cs) => new SqlConnection(cs);

        public static string GetScalarString(SqlConnection cn, SqlTransaction tx, string sql, params SqlParameter[] ps)
        {
            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                if (ps != null && ps.Length > 0) cmd.Parameters.AddRange(ps);
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? null : Convert.ToString(o);
            }
        }

        // ====== Estados ======
        public static bool IsLockedStatus(string status) =>
            !string.IsNullOrWhiteSpace(status) &&
            (status.Equals("Resuelto", StringComparison.OrdinalIgnoreCase)
          || status.Equals("Cerrado", StringComparison.OrdinalIgnoreCase));
    }
}
