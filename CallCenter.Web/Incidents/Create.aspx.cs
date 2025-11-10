using CallCenter.Business.Services;
using CallCenter.Web.Infrastructure;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;


namespace CallCenter.Web.Incidents
{
    public partial class Create : System.Web.UI.Page
    {
        private string _cs;
        private Guid _userId = Guid.Empty;
        private string _username = "";
        private string _role = "";
        private readonly IEmailSender _mailer = new SmtpEmailSender(); 

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Web.UI.ValidationSettings.UnobtrusiveValidationMode =
                System.Web.UI.UnobtrusiveValidationMode.None;

            _cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;

            if (!IsPostBack)
            {
                LoadUser();
                BindLookups(); 
            }
        }
        private void LoadUser()
        {
            _username = Context.User == null ? "" : Context.User.Identity.Name;
            if (string.IsNullOrEmpty(_username))
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            using (var cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT TOP 1 Id, [Role] FROM dbo.Users WHERE Username=@u AND IsDeleted=0 AND IsBlocked=0;", cn))
                {
                    cmd.Parameters.Add("@u", SqlDbType.NVarChar, 100).Value = _username;
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            _userId = rd.GetGuid(0);
                            _role = rd.GetString(1);
                        }
                        else
                        {
                            Response.Redirect("~/Account/Login.aspx");
                        }
                    }
                }
            }
        }

        private void BindLookups()
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();

                // Customers
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Id, Name FROM dbo.Customers WHERE IsDeleted=0 ORDER BY Name;", cn))
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    ddlCustomer.Items.Clear();
                    while (rd.Read())
                        ddlCustomer.Items.Add(new System.Web.UI.WebControls.ListItem(
                            rd.GetString(1), rd.GetInt32(0).ToString()));
                }

                // Priorities
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Id, Name FROM dbo.Priorities ORDER BY OrderNum;", cn))
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    ddlPriority.Items.Clear();
                    while (rd.Read())
                        ddlPriority.Items.Add(new System.Web.UI.WebControls.ListItem(
                            rd.GetString(1), rd.GetInt32(0).ToString()));
                }

                // Incident Types
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Id, Name FROM dbo.IncidentTypes WHERE IsActive=1 ORDER BY Name;", cn))
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    ddlType.Items.Clear();
                    while (rd.Read())
                        ddlType.Items.Add(new System.Web.UI.WebControls.ListItem(
                            rd.GetString(1), rd.GetInt32(0).ToString()));
                }
            }
        }
        protected void btnCreate_Click(object sender, EventArgs e)
        {
            if (ddlCustomer.SelectedValue == "" || ddlType.SelectedValue == "" || ddlPriority.SelectedValue == "")
            {
                lblInfo.CssClass = "text-danger";
                lblInfo.Text = "Seleccioná cliente, tipo y prioridad.";
                return;
            }
            if (_userId == Guid.Empty || string.IsNullOrEmpty(_username))
                LoadUser();

            int customerId = int.Parse(ddlCustomer.SelectedValue); 
            string custEmail = null, custName = null;
            using (var cn0 = new SqlConnection(_cs))
            {
                cn0.Open();
                using (var c0 = new SqlCommand("SELECT TOP 1 Email, Name FROM dbo.Customers WHERE Id=@id;", cn0))
                {
                    c0.Parameters.Add("@id", SqlDbType.Int).Value = customerId;
                    using (var rd = c0.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            custEmail = Convert.ToString(rd["Email"]);
                            custName = Convert.ToString(rd["Name"]);
                        }
                    }
                }
            }

            Guid incidentId = Guid.Empty;

            using (var cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Incidents
( Id, CustomerId, IncidentTypeId, PriorityId, Problem, Status, CreatedAt, CreatedByUserId, AssignedToUserId )
OUTPUT INSERTED.Id
VALUES
( NEWID(), @c, @t, @p, @pr, N'Abierto', SYSUTCDATETIME(), @uid, @uid );", cn, tx))
                        {
                            cmd.Parameters.Add("@c", SqlDbType.Int).Value = customerId; // INT
                            cmd.Parameters.Add("@t", SqlDbType.Int).Value = int.Parse(ddlType.SelectedValue);
                            cmd.Parameters.Add("@p", SqlDbType.Int).Value = int.Parse(ddlPriority.SelectedValue);
                            cmd.Parameters.Add("@pr", SqlDbType.NVarChar, 1000).Value = (object)(txtProblem.Text ?? "").Trim();
                            cmd.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = _userId;

                            incidentId = (Guid)cmd.ExecuteScalar();
                        }

                        using (var m = new SqlCommand(@"
INSERT INTO dbo.IncidentMessages(IncidentId, UserId, SenderName, Message)
VALUES(@iid, @uid, @sn, @msg);", cn, tx))
                        {
                            m.Parameters.Add("@iid", SqlDbType.UniqueIdentifier).Value = incidentId;
                            m.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = _userId;
                            m.Parameters.Add("@sn", SqlDbType.NVarChar, 200).Value = _username;
                            m.Parameters.Add("@msg", SqlDbType.NVarChar, 2000).Value = "Incidencia creada.";
                            m.ExecuteNonQuery();
                        }

                        SaveAttachments(incidentId, cn, tx);

                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        lblInfo.CssClass = "text-danger";
                        lblInfo.Text = "No se pudo crear la incidencia: " + ex.Message;
                        return;
                    }
                }
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(custEmail))
                {
                    string asunto = $"[Call Center] Incidencia creada #{incidentId.ToString().Substring(0, 8)}";
                    string html = BuildIncidentCreatedEmail(custName, incidentId, ddlType.SelectedItem.Text,
                                                            ddlPriority.SelectedItem.Text, (txtProblem.Text ?? "").Trim());

                    _mailer.Send(custEmail, asunto, html);
                }
            }
            catch (Exception mailEx)
            {
                System.Diagnostics.Debug.WriteLine("MAIL ERROR: " + mailEx.Message);
            }

            Response.Redirect("Details.aspx?id=" + incidentId.ToString(), endResponse: true);
        }


        private void SaveAttachments(Guid incidentId, SqlConnection cn, SqlTransaction tx)
        {
            if (fuFiles == null || !fuFiles.HasFiles) return;

           
            string baseDir = Server.MapPath("~/App_Data/Uploads");
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);

            foreach (var posted in fuFiles.PostedFiles)
            {
                var file = posted as HttpPostedFile;
                if (file == null) continue;
                if (file.ContentLength <= 0) continue;

                

                string originalName = Path.GetFileName(file.FileName);
                string storedName = Guid.NewGuid().ToString("N") + Path.GetExtension(originalName);
                string fullPath = Path.Combine(baseDir, storedName);

                file.SaveAs(fullPath);

                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.IncidentAttachments(Id, IncidentId, FileName, StoredName, ContentType, FileSizeBytes, UploadedByUserId)
VALUES(NEWID(), @iid, @fn, @sn, @ct, @sz, @uid);", cn, tx))
                {
                    cmd.Parameters.Add("@iid", SqlDbType.UniqueIdentifier).Value = incidentId;
                    cmd.Parameters.Add("@fn", SqlDbType.NVarChar, 260).Value = originalName;
                    cmd.Parameters.Add("@sn", SqlDbType.NVarChar, 260).Value = storedName;
                    cmd.Parameters.Add("@ct", SqlDbType.NVarChar, 100).Value = (object)file.ContentType ?? DBNull.Value;
                    cmd.Parameters.Add("@sz", SqlDbType.BigInt).Value = file.ContentLength;
                    cmd.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = _userId;
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private string GetCustomerEmail(SqlConnection cn, int customerId)
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT Email FROM dbo.Customers WHERE Id=@id AND IsDeleted=0;", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = customerId;
                object o = cmd.ExecuteScalar();
                return o == null ? "" : Convert.ToString(o);
            }
        }

        private string BuildIncidentCreatedEmail(string customerName, Guid incidentId, string tipo, string prioridad, string problema)
        {
            string idCorto = incidentId.ToString().Substring(0, 8);
            string fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            return $@"
<!doctype html>
<html>
  <body style='font-family:Segoe UI,Arial,sans-serif;background:#f6f8fa;padding:24px;color:#111;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='max-width:640px;margin:auto;background:#fff;border:1px solid #e9ecef;border-radius:8px'>
      <tr><td style='padding:20px 24px'>
        <h2 style='margin:0 0 8px 0;color:#0d6efd'>Incidencia creada</h2>
        <p style='margin:0;color:#555'>Hola {System.Web.HttpUtility.HtmlEncode(customerName)}, registramos tu reclamo.</p>
        <hr style='border:none;border-top:1px solid #eee;margin:16px 0' />
        <p style='margin:0'><strong>N°:</strong> {idCorto}</p>
        <p style='margin:0'><strong>Fecha:</strong> {fecha}</p>
        <p style='margin:0'><strong>Estado inicial:</strong> Abierto</p>
        <p style='margin:0'><strong>Tipo:</strong> {System.Web.HttpUtility.HtmlEncode(tipo)}</p>
        <p style='margin:0 0 12px 0'><strong>Prioridad:</strong> {System.Web.HttpUtility.HtmlEncode(prioridad)}</p>
        <div style='background:#f8f9fa;border:1px solid #eee;border-radius:6px;padding:12px'>
          <strong>Detalle del problema:</strong>
          <div>{System.Web.HttpUtility.HtmlEncode(problema).Replace("\n", "<br/>")}</div>
        </div>
        <p style='margin:16px 0 0 0;color:#555;font-size:13px'>Te mantendremos informado ante cualquier actualización.</p>
        <p style='margin:0;color:#999;font-size:12px'>Este correo fue generado automáticamente. No responda a este mensaje.</p>
      </td></tr>
    </table>
  </body>
</html>";
        }
    }
}

