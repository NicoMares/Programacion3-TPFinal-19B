// Incidents/Create.aspx.cs
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.IO;


namespace CallCenter.Web.Incidents
{
    public partial class Create : System.Web.UI.Page
    {
        private string _cs;
        private Guid _userId = Guid.Empty;
        private string _username = "";
        private string _role = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Web.UI.ValidationSettings.UnobtrusiveValidationMode =
                System.Web.UI.UnobtrusiveValidationMode.None;

            _cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;

            if (!IsPostBack)
            {
                LoadUser();
                BindLookups(); // llena ddl de cliente, tipo, prioridad (si ya lo tenías)
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
            // Validaciones mínimas
            if (ddlCustomer.SelectedValue == "" || ddlType.SelectedValue == "" || ddlPriority.SelectedValue == "")
            {
                lblInfo.CssClass = "text-danger";
                lblInfo.Text = "Seleccioná cliente, tipo y prioridad.";
                return;
            }

            // Asegurá usuario cargado
            if (_userId == Guid.Empty || string.IsNullOrEmpty(_username))
                LoadUser();

            Guid incidentId = Guid.Empty;

            using (var cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        // INSERT con CreatedByUserId y AssignedToUserId = creador
                        using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Incidents
( Id, CustomerId, IncidentTypeId, PriorityId, Problem, Status, CreatedAt, CreatedByUserId, AssignedToUserId )
OUTPUT INSERTED.Id
VALUES
( NEWID(), @c, @t, @p, @pr, N'Abierto', SYSUTCDATETIME(), @uid, @uid );", cn, tx))
                        {
                            // si CustomerId es GUID:
                            cmd.Parameters.Add("@c", SqlDbType.Int).Value = int.Parse(ddlCustomer.SelectedValue);
                            // si fuera INT, usar: cmd.Parameters.Add("@c", SqlDbType.Int).Value = int.Parse(ddlCustomer.SelectedValue);

                            cmd.Parameters.Add("@t", SqlDbType.Int).Value = int.Parse(ddlType.SelectedValue);
                            cmd.Parameters.Add("@p", SqlDbType.Int).Value = int.Parse(ddlPriority.SelectedValue);
                            cmd.Parameters.Add("@pr", SqlDbType.NVarChar, 1000).Value = (object)(txtProblem.Text ?? "").Trim();
                            cmd.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = _userId;

                            incidentId = (Guid)cmd.ExecuteScalar();
                        }

                        // Nota de sistema
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

                        // Adjuntos (si usás el FileUpload fuFiles)
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

            Response.Redirect("Details.aspx?id=" + incidentId.ToString(), endResponse: true);
        }

        private void SaveAttachments(Guid incidentId, SqlConnection cn, SqlTransaction tx)
        {
            // sin archivos? salir
            if (fuFiles == null || !fuFiles.HasFiles) return;

            // carpeta física donde guardamos
            // recomendación: ~/App_Data/Uploads  (no servida directamente)
            string baseDir = Server.MapPath("~/App_Data/Uploads");
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);

            foreach (var posted in fuFiles.PostedFiles)
            {
                var file = posted as HttpPostedFile;
                if (file == null) continue;
                if (file.ContentLength <= 0) continue;

                // (opcional) validar tamaño/extensiones
                // if (file.ContentLength > 50 * 1024 * 1024) throw new Exception("Archivo excede 50MB");

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


        // (Opcional) helper para email del cliente
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
    }
}
