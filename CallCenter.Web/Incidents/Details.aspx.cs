using CallCenter.Business.Services;
using CallCenter.Web.Infrastructure;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using CallCenter.Web.Helpers;


namespace CallCenter.Web.Incidents
{
    public partial class Details : System.Web.UI.Page
    {
        private string _cs;
        private Guid _incidentId = Guid.Empty;
        private Guid _userId = Guid.Empty;
        private string _username = "";
        private string _role = "";
        private readonly IEmailSender _mailer = new SmtpEmailSender();


        protected void Page_Load(object sender, EventArgs e)
        {
            System.Web.UI.ValidationSettings.UnobtrusiveValidationMode =
                System.Web.UI.UnobtrusiveValidationMode.None;

            _cs = System.Configuration.ConfigurationManager
                      .ConnectionStrings["CallCenterDb"].ConnectionString;

            string qs = Request.QueryString["id"];
            if (string.IsNullOrEmpty(qs) || !Guid.TryParse(qs, out _incidentId))
            {
                lblHeaderInfo.CssClass = "alert alert-danger";
                lblHeaderInfo.Text = "Incidencia no válida.";

                DisableChat();
                DisableActions();
                return;
            }

            // siempre cargo usuario (también en postbacks para permisos)
            LoadUser();

            if (!IsPostBack)
            {
                LoadIncidentHeader();   // guarda Status y Assigned en ViewState
                BindMessages();
                BindAttachments();
                BindAssignableUsers();
                ApplyPermissions();     // deshabilita si es estado final

                if (Request.QueryString["reassigned"] == "1")
                {
                    lblActionsMsg.CssClass = "text-success";
                    lblActionsMsg.Text = "Incidencia reasignada (estado: Asignado).";
                }
                else if (Request.QueryString["resolved"] == "1")
                {
                    lblActionsMsg.CssClass = "text-success";
                    lblActionsMsg.Text = "Incidencia marcada como Resuelta.";
                }
                else if (Request.QueryString["closed"] == "1")
                {
                    lblActionsMsg.CssClass = "text-success";
                    lblActionsMsg.Text = "Incidencia cerrada correctamente.";
                }
            }
        }

        // ===== Helpers base =====
        private void LoadUser()
        {
            _username = Context.User == null ? "" : Context.User.Identity.Name;
            if (string.IsNullOrEmpty(_username))
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand ucmd = new SqlCommand(
                    "SELECT TOP 1 Id, [Role] FROM dbo.Users WHERE Username=@u AND IsDeleted=0 AND IsBlocked=0;", cn))
                {
                    ucmd.Parameters.Add("@u", SqlDbType.NVarChar, 100).Value = _username;
                    using (SqlDataReader rd = ucmd.ExecuteReader())
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

        private void LoadIncidentHeader()
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
            SELECT i.Id, c.Name AS Cliente, p.Name AS Prioridad, t.Name AS Tipo, 
                   i.Problem, i.Status, i.CreatedAt,
                   u.Username AS AsignadoA
            FROM dbo.Incidents i
            INNER JOIN dbo.Customers c ON c.Id = i.CustomerId
            INNER JOIN dbo.Priorities p ON p.Id = i.PriorityId
            INNER JOIN dbo.IncidentTypes t ON t.Id = i.IncidentTypeId
            LEFT JOIN dbo.Users u ON u.Id = i.AssignedToUserId
            WHERE i.Id = @id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = _incidentId;

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            string asignado = rd["AsignadoA"] == DBNull.Value ? "(Sin asignar)" : rd["AsignadoA"].ToString();
                            string estado = Convert.ToString(rd["Status"]);

                            lblHeaderInfo.Text = $@"
                        <div class='row g-2'>
                            <div class='col-md-6'><strong>ID:</strong> {rd["Id"]}</div>
                            <div class='col-md-6'><strong>Cliente:</strong> {HttpUtility.HtmlEncode(rd["Cliente"].ToString())}</div>
                            <div class='col-md-6'><strong>Tipo:</strong> {HttpUtility.HtmlEncode(rd["Tipo"].ToString())}</div>
                            <div class='col-md-6'><strong>Prioridad:</strong> {HttpUtility.HtmlEncode(rd["Prioridad"].ToString())}</div>
                            <div class='col-md-6'><strong>Asignado a:</strong> {HttpUtility.HtmlEncode(asignado)}</div>
                            <div class='col-md-6'><strong>Estado:</strong> {HttpUtility.HtmlEncode(estado)}</div>
                            <div class='col-12'><strong>Problema:</strong><br>{HttpUtility.HtmlEncode(rd["Problem"].ToString())}</div>
                            <div class='col-12'><strong>Fecha:</strong> {Convert.ToDateTime(rd["CreatedAt"]).ToLocalTime():dd/MM/yyyy HH:mm}</div>
                        </div>";

                            AppUiHelpers.LockActionsByStatus(
                                estado,
                                btnResolve, btnClose, btnAssign, ddlAssign,
                                btnToggleResolve, btnToggleClose
       
                            );
                        }
                        else
                        {
                            lblHeaderInfo.CssClass = "alert alert-danger";
                            lblHeaderInfo.Text = "Incidencia no encontrada.";
                            DisableChat();
                            DisableActions();
                        }
                    }
                }
            }
        }


        private bool IsFinalStatus()
        {
            var st = (ViewState["Status"] as string ?? "").Trim();
            return string.Equals(st, "Resuelto", StringComparison.OrdinalIgnoreCase)
                || string.Equals(st, "Cerrado", StringComparison.OrdinalIgnoreCase);
        }

        private void DisableChat()
        {
            txtMsg.Enabled = false;
            btnSend.Enabled = false;
            btnRefresh.Enabled = false;
        }

        private void DisableActions()
        {
            ddlAssign.Enabled = false;
            btnAssign.Enabled = false;
            txtResolution.Enabled = false;
            btnResolve.Enabled = false;
            txtClose.Enabled = false;
            btnClose.Enabled = false;
        }

        private void ApplyPermissions()
        {
            // bloquear todo si es estado final
            if (IsFinalStatus())
            {
                DisableActions();
                return;
            }

            // si no es final, aplicar permisos por rol
            bool canAssign =
                string.Equals(_role, "Administrador", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_role, "Supervisor", StringComparison.OrdinalIgnoreCase);

            ddlAssign.Visible = canAssign;
            btnAssign.Visible = canAssign;
        }

        private void BindAssignableUsers()
        {
            // si final, ni muestro
            if (IsFinalStatus())
            {
                ddlAssign.Visible = false;
                btnAssign.Visible = false;
                return;
            }

            bool canAssign =
                string.Equals(_role, "Administrador", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_role, "Supervisor", StringComparison.OrdinalIgnoreCase);

            ddlAssign.Visible = canAssign;
            btnAssign.Visible = canAssign;
            if (!canAssign) return;

            ddlAssign.Items.Clear();

            Guid currentAssigned = Guid.Empty;
            if (ViewState["AssignedToUserId"] != null && ViewState["AssignedToUserId"] != DBNull.Value)
                currentAssigned = (Guid)ViewState["AssignedToUserId"];

            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Id, Username FROM dbo.Users WHERE IsDeleted=0 AND IsBlocked=0 ORDER BY Username;", cn))
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var text = rd.GetString(1);
                        var val = rd.GetGuid(0).ToString();
                        ddlAssign.Items.Add(new System.Web.UI.WebControls.ListItem(text, val));
                    }
                }
            }

            if (currentAssigned != Guid.Empty)
            {
                var li = ddlAssign.Items.FindByValue(currentAssigned.ToString());
                if (li != null) ddlAssign.SelectedValue = currentAssigned.ToString();
            }
        }

        // ===== Chat =====
        protected void BindMessages()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SenderName", typeof(string));
            dt.Columns.Add("Message", typeof(string));
            dt.Columns.Add("CreatedAtLocal", typeof(DateTime));
            dt.Columns.Add("IsMe", typeof(bool));

            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT m.SenderName, m.Message, m.CreatedAt, m.UserId
                    FROM dbo.IncidentMessages m
                    WHERE m.IncidentId = @id
                    ORDER BY m.CreatedAt ASC;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = _incidentId;
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            string sender = Convert.ToString(rd["SenderName"]);
                            string msg = Convert.ToString(rd["Message"]);
                            DateTime createdUtc = Convert.ToDateTime(rd["CreatedAt"]);
                            Guid uid = rd.IsDBNull(3) ? Guid.Empty : rd.GetGuid(3);

                            DataRow row = dt.NewRow();
                            row["SenderName"] = sender;
                            row["Message"] = HttpUtility.HtmlEncode(msg);
                            row["CreatedAtLocal"] = createdUtc.ToLocalTime();
                            row["IsMe"] = (uid != Guid.Empty && uid == _userId);
                            dt.Rows.Add(row);
                        }
                    }
                }
            }

            rpMsgs.DataSource = dt;
            rpMsgs.DataBind();
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            string msg = txtMsg.Text == null ? "" : txtMsg.Text.Trim();
            if (msg.Length == 0) return;

            if (_userId == Guid.Empty || string.IsNullOrEmpty(_username))
                LoadUser();

            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction())
                {
                    try
                    {
                        // 1) Insertar mensaje
                        using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.IncidentMessages(IncidentId, UserId, SenderName, Message)
VALUES(@iid, @uid, @sn, @msg);", cn, tx))
                        {
                            cmd.Parameters.Add("@iid", SqlDbType.UniqueIdentifier).Value = _incidentId;
                            cmd.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = _userId;
                            cmd.Parameters.Add("@sn", SqlDbType.NVarChar, 200).Value = _username;
                            cmd.Parameters.Add("@msg", SqlDbType.NVarChar, 2000).Value = msg;
                            cmd.ExecuteNonQuery();
                        }

                        // 2) Si no está Resuelto/Cerrado → pasar a En Análisis
                        string statusActual = "";
                        using (SqlCommand s = new SqlCommand(
                            "SELECT Status FROM dbo.Incidents WHERE Id=@id;", cn, tx))
                        {
                            s.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = _incidentId;
                            object o = s.ExecuteScalar();
                            statusActual = (o == null || o == DBNull.Value) ? "" : (string)o;
                        }

                        if (!statusActual.Equals("Resuelto", StringComparison.OrdinalIgnoreCase) &&
                            !statusActual.Equals("Cerrado", StringComparison.OrdinalIgnoreCase) &&
                            !statusActual.Equals("En Análisis", StringComparison.OrdinalIgnoreCase))
                        {
                            using (SqlCommand u = new SqlCommand(
                                "UPDATE dbo.Incidents SET Status=N'En Análisis' WHERE Id=@id;", cn, tx))
                            {
                                u.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = _incidentId;
                                u.ExecuteNonQuery();
                            }

                            using (SqlCommand m = new SqlCommand(@"
INSERT INTO dbo.IncidentMessages(IncidentId, UserId, SenderName, Message)
VALUES(@iid, @uid, @sn, @msg);", cn, tx))
                            {
                                m.Parameters.Add("@iid", SqlDbType.UniqueIdentifier).Value = _incidentId;
                                m.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = _userId;
                                m.Parameters.Add("@sn", SqlDbType.NVarChar, 200).Value = _username; // o "Sistema"
                                m.Parameters.Add("@msg", SqlDbType.NVarChar, 2000).Value =
                                    "Estado cambiado a 'En Análisis' por actividad en el chat.";
                                m.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        lblChatMsg.CssClass = "text-danger";
                        lblChatMsg.Text = "No se pudo enviar el mensaje.";
                        return;
                    }
                }
            }

            txtMsg.Text = "";
            lblChatMsg.CssClass = "text-success";
            lblChatMsg.Text = "Mensaje enviado.";
            LoadIncidentHeader();
            BindMessages();
            // PRG
            Response.Redirect("Details.aspx?id=" + _incidentId.ToString(), endResponse: true);
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            BindMessages();
        }

        private void InsertSystemNote(string text)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.IncidentMessages(IncidentId, UserId, SenderName, Message)
VALUES(@iid, @uid, @sn, @msg);", cn))
                {
                    cmd.Parameters.Add("@iid", SqlDbType.UniqueIdentifier).Value = _incidentId;
                    cmd.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = _userId; // autor actual
                    cmd.Parameters.Add("@sn", SqlDbType.NVarChar, 200).Value = _username;   // visible en chat
                    cmd.Parameters.Add("@msg", SqlDbType.NVarChar, 2000).Value = text;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ===== Acciones de la incidencia =====
        protected void btnAnalysis_Click(object sender, EventArgs e)
        {
            if (IsFinalStatus())
            {
                lblActionsMsg.CssClass = "text-warning";
                lblActionsMsg.Text = "La incidencia está finalizada. No se pueden realizar acciones.";
                return;
            }

            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE dbo.Incidents SET Status=N'En Análisis' WHERE Id=@id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = _incidentId;
                    int n = cmd.ExecuteNonQuery();
                    if (n > 0)
                    {
                        InsertSystemNote("Estado cambiado a 'En Análisis'.");
                        Response.Redirect("Details.aspx?id=" + _incidentId.ToString());
                        return;
                    }
                }
            }
            lblActionsMsg.CssClass = "text-danger";
            lblActionsMsg.Text = "No se pudo actualizar el estado.";
        }

        protected void btnAssign_Click(object sender, EventArgs e)
        {
            bool canAssign =
                string.Equals(_role, "Administrador", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_role, "Supervisor", StringComparison.OrdinalIgnoreCase);
            if (!canAssign) return;

            if (!Guid.TryParse(ddlAssign.SelectedValue, out Guid newUid) || newUid == Guid.Empty)
            {
                lblActionsMsg.CssClass = "text-danger";
                lblActionsMsg.Text = "Seleccione un usuario válido.";
                return;
            }

            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction())
                {
                    try
                    {
                        // Evitar cambios si está Resuelto/Cerrado
                        string statusActual = "";
                        using (SqlCommand s = new SqlCommand(
                            "SELECT Status FROM dbo.Incidents WHERE Id=@id;", cn, tx))
                        {
                            s.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = _incidentId;
                            object o = s.ExecuteScalar();
                            statusActual = (o == null || o == DBNull.Value) ? "" : (string)o;
                        }
                        if (statusActual.Equals("Resuelto", StringComparison.OrdinalIgnoreCase) ||
                            statusActual.Equals("Cerrado", StringComparison.OrdinalIgnoreCase))
                        {
                            lblActionsMsg.CssClass = "text-danger";
                            lblActionsMsg.Text = "La incidencia está cerrada o resuelta. No se puede reasignar.";
                            tx.Rollback();
                            return;
                        }

                        // Reasignar y pasar a En Análisis
                        using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.Incidents
SET AssignedToUserId=@uid, Status=N'En Análisis'
WHERE Id=@id;", cn, tx))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = _incidentId;
                            cmd.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = newUid;
                            int n = cmd.ExecuteNonQuery();
                            if (n <= 0) throw new Exception("No se pudo reasignar.");
                        }

                        // Notas al chat
                        InsertSystemNoteTx(cn, tx, "Incidencia reasignada a " + ddlAssign.SelectedItem.Text + ".");
                        InsertSystemNoteTx(cn, tx, "Estado cambiado a 'En Análisis' por reasignación.");

                        tx.Commit();

                        lblActionsMsg.CssClass = "text-success";
                        lblActionsMsg.Text = "Incidencia reasignada (estado: En Análisis).";
                        LoadIncidentHeader();
                        BindMessages();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        lblActionsMsg.CssClass = "text-danger";
                        lblActionsMsg.Text = "No se pudo reasignar: " + ex.Message;
                    }
                }
            }
        }

        protected void btnResolve_Click(object sender, EventArgs e)
        {
            Page.Validate("resolve");
            if (!Page.IsValid) return;

            string note = txtResolution.Text == null ? "" : txtResolution.Text.Trim();

            bool resuelto = false;

            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.Incidents
SET Status=N'Resuelto', ResolvedAt=SYSUTCDATETIME(), ResolutionNote=@n
WHERE Id=@id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = _incidentId;
                    cmd.Parameters.Add("@n", SqlDbType.NVarChar, 1000).Value = (object)note ?? DBNull.Value;
                    int n = cmd.ExecuteNonQuery();
                    resuelto = n > 0;
                }
            }

            if (!resuelto)
            {
                lblActionsMsg.CssClass = "text-danger";
                lblActionsMsg.Text = "No se pudo resolver la incidencia.";
                return;
            }

            // 1️⃣ Insertar nota al chat
            InsertSystemNote("Incidencia RESUELTA. Nota: " + note);

            lblActionsMsg.CssClass = "text-success";
            lblActionsMsg.Text = "Incidencia marcada como Resuelta.";
            LoadIncidentHeader();
            BindMessages();

            // 2️⃣ Obtener email y datos del cliente
            try
            {
                string custEmail = null, custName = null, tipo = null, prioridad = null;
                using (SqlConnection cn = new SqlConnection(_cs))
                {
                    cn.Open();
                    using (SqlCommand c = new SqlCommand(@"
SELECT TOP 1 cu.Email, cu.Name,
       it.Name AS Tipo, pr.Name AS Prioridad
FROM dbo.Incidents i
JOIN dbo.Customers cu ON cu.Id = i.CustomerId
JOIN dbo.IncidentTypes it ON it.Id = i.IncidentTypeId
JOIN dbo.Priorities pr ON pr.Id = i.PriorityId
WHERE i.Id = @id;", cn))
                    {
                        c.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = _incidentId;
                        using (var rd = c.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                custEmail = Convert.ToString(rd["Email"]);
                                custName = Convert.ToString(rd["Name"]);
                                tipo = Convert.ToString(rd["Tipo"]);
                                prioridad = Convert.ToString(rd["Prioridad"]);
                            }
                        }
                    }
                }

                // 3️⃣ Enviar mail si hay email válido
                if (!string.IsNullOrWhiteSpace(custEmail))
                {
                    string asunto = $"[Call Center] Incidencia resuelta #{_incidentId.ToString().Substring(0, 8)}";
                    string html = BuildIncidentResolvedEmail(
                        custName ?? "",
                        _incidentId,
                        tipo ?? "",
                        prioridad ?? "",
                        note ?? ""
                    );
                    _mailer.Send(custEmail, asunto, html);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MAIL RESUELTO ERROR: " + ex.Message);
                // no interrumpe el flujo
            }

            // 4️⃣ Refrescar vista
            Response.Redirect("Details.aspx?id=" + _incidentId.ToString(), endResponse: true);
        }


        protected void btnClose_Click(object sender, EventArgs e)
        {
            Page.Validate("close");
            if (!Page.IsValid) return;

            string comment = txtClose.Text == null ? "" : txtClose.Text.Trim();

            // 1) Cerrar en DB
            bool cerrado = false;
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.Incidents
SET Status=N'Cerrado', ClosedComment=@c
WHERE Id=@id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = _incidentId;
                    cmd.Parameters.Add("@c", SqlDbType.NVarChar, 1000).Value = comment;
                    int n = cmd.ExecuteNonQuery();
                    cerrado = n > 0;
                }
            }

            if (!cerrado)
            {
                lblActionsMsg.CssClass = "text-danger";
                lblActionsMsg.Text = "No se pudo cerrar la incidencia.";
                return;
            }

            // 2) Nota al chat
            InsertSystemNote("Incidencia CERRADA. Comentario: " + comment);

            lblActionsMsg.CssClass = "text-success";
            lblActionsMsg.Text = "Incidencia cerrada correctamente.";
            LoadIncidentHeader();
            BindMessages();

            // 3) Obtener datos del cliente (email y nombre) y enviar correo
            try
            {
                string custEmail = null, custName = null, tipo = null, prioridad = null;
                using (SqlConnection cn = new SqlConnection(_cs))
                {
                    cn.Open();
                    using (SqlCommand c = new SqlCommand(@"
SELECT TOP 1 cu.Email, cu.Name,
       it.Name AS Tipo, pr.Name AS Prioridad
FROM dbo.Incidents i
JOIN dbo.Customers cu ON cu.Id = i.CustomerId
JOIN dbo.IncidentTypes it ON it.Id = i.IncidentTypeId
JOIN dbo.Priorities pr ON pr.Id = i.PriorityId
WHERE i.Id = @id;", cn))
                    {
                        c.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = _incidentId;
                        using (var rd = c.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                custEmail = Convert.ToString(rd["Email"]);
                                custName = Convert.ToString(rd["Name"]);
                                tipo = Convert.ToString(rd["Tipo"]);
                                prioridad = Convert.ToString(rd["Prioridad"]);
                            }
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(custEmail))
                {
                    string asunto = $"[Call Center] Incidencia cerrada #{_incidentId.ToString().Substring(0, 8)}";
                    string html = BuildIncidentClosedEmail(
                        custName ?? "",
                        _incidentId,
                        tipo ?? "",
                        prioridad ?? "",
                        comment ?? ""
                    );
                    _mailer.Send(custEmail, asunto, html);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MAIL CLOSE ERROR: " + ex.Message);
                // no interrumpe el flujo
            }

            // 4) Refrescar la pantalla
            Response.Redirect("Details.aspx?id=" + _incidentId.ToString(), endResponse: true);
        }



        private void BindAttachments()
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(Guid));
            dt.Columns.Add("FileName", typeof(string));
            dt.Columns.Add("SizePretty", typeof(string));
            dt.Columns.Add("Uploader", typeof(string));
            dt.Columns.Add("UploadedAtLocal", typeof(DateTime));

            using (var cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (var cmd = new SqlCommand(@"
SELECT a.Id, a.FileName, a.FileSizeBytes, u.Username AS Uploader, a.UploadedAt
FROM dbo.IncidentAttachments a
LEFT JOIN dbo.Users u ON u.Id = a.UploadedByUserId
WHERE a.IncidentId = @iid
ORDER BY a.UploadedAt DESC;", cn))
                {
                    cmd.Parameters.Add("@iid", SqlDbType.UniqueIdentifier).Value = _incidentId;
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            DataRow r = dt.NewRow();
                            r["Id"] = rd.GetGuid(0);
                            r["FileName"] = Convert.ToString(rd["FileName"]);
                            long size = Convert.ToInt64(rd["FileSizeBytes"]);
                            r["SizePretty"] = PrettySize(size);
                            r["Uploader"] = Convert.ToString(rd["Uploader"]);
                            r["UploadedAtLocal"] = Convert.ToDateTime(rd["UploadedAt"]).ToLocalTime();
                            dt.Rows.Add(r);
                        }
                    }
                }
            }

            rpFiles.DataSource = dt;
            rpFiles.DataBind();

            lblFilesInfo.Text = dt.Rows.Count == 0 ? "Sin adjuntos." : "";
        }

        private string PrettySize(long bytes)
        {
            const long K = 1024, M = K * 1024, G = M * 1024;
            if (bytes >= G) return (bytes / (double)G).ToString("0.##") + " GB";
            if (bytes >= M) return (bytes / (double)M).ToString("0.##") + " MB";
            if (bytes >= K) return (bytes / (double)K).ToString("0.##") + " KB";
            return bytes + " B";
        }

        private void InsertSystemNoteTx(SqlConnection cn, SqlTransaction tx, string text)
        {
            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.IncidentMessages(IncidentId, UserId, SenderName, Message)
VALUES(@iid, @uid, @sn, @msg);", cn, tx))
            {
                cmd.Parameters.Add("@iid", SqlDbType.UniqueIdentifier).Value = _incidentId;
                cmd.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = _userId; // o DBNull si querés “Sistema”
                cmd.Parameters.Add("@sn", SqlDbType.NVarChar, 200).Value = _username;   // o "Sistema"
                cmd.Parameters.Add("@msg", SqlDbType.NVarChar, 2000).Value = text;
                cmd.ExecuteNonQuery();
            }
        }




        private string BuildIncidentClosedEmail(string customerName, Guid incidentId, string tipo, string prioridad, string closeComment)
        {
            string idCorto = incidentId.ToString().Substring(0, 8);
            string fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            return $@"
<!doctype html>
<html>
  <body style='font-family:Segoe UI,Arial,sans-serif;background:#f6f8fa;padding:24px;color:#111;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='max-width:640px;margin:auto;background:#fff;border:1px solid #e9ecef;border-radius:8px'>
      <tr><td style='padding:20px 24px'>
        <h2 style='margin:0 0 8px 0;color:#dc3545'>Incidencia cerrada</h2>
        <p style='margin:0;color:#555'>Hola {System.Web.HttpUtility.HtmlEncode(customerName)}, tu reclamo fue <strong>cerrado</strong>.</p>
        <hr style='border:none;border-top:1px solid #eee;margin:16px 0' />
        <p style='margin:0'><strong>N°:</strong> {idCorto}</p>
        <p style='margin:0'><strong>Fecha:</strong> {fecha}</p>
        <p style='margin:0'><strong>Estado:</strong> Cerrado</p>
        <p style='margin:0'><strong>Tipo:</strong> {System.Web.HttpUtility.HtmlEncode(tipo)}</p>
        <p style='margin:0 0 12px 0'><strong>Prioridad:</strong> {System.Web.HttpUtility.HtmlEncode(prioridad)}</p>
        <div style='background:#f8f9fa;border:1px solid #eee;border-radius:6px;padding:12px'>
          <strong>Comentario de cierre:</strong>
          <div>{System.Web.HttpUtility.HtmlEncode(closeComment).Replace("\n", "<br/>")}</div>
        </div>
        <p style='margin:16px 0 0 0;color:#555;font-size:13px'>
          Si necesitás volver a abrir el caso, por favor respondé este correo o contactá a nuestro soporte.
        </p>
        <p style='margin:0;color:#999;font-size:12px'>Este correo fue generado automáticamente. No responda a este mensaje.</p>
      </td></tr>
    </table>
  </body>
</html>";
        }

    
        

    private string BuildIncidentResolvedEmail(string customerName, Guid incidentId, string tipo, string prioridad, string resolutionNote)
        {
            string idCorto = incidentId.ToString().Substring(0, 8);
            string fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            return $@"
<!doctype html>
<html>
  <body style='font-family:Segoe UI,Arial,sans-serif;background:#f6f8fa;padding:24px;color:#111;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='max-width:640px;margin:auto;background:#fff;border:1px solid #e9ecef;border-radius:8px'>
      <tr><td style='padding:20px 24px'>
        <h2 style='margin:0 0 8px 0;color:#198754'>Incidencia resuelta</h2>
        <p style='margin:0;color:#555'>Hola {System.Web.HttpUtility.HtmlEncode(customerName)}, tu reclamo fue <strong>resuelto</strong>.</p>
        <hr style='border:none;border-top:1px solid #eee;margin:16px 0' />
        <p style='margin:0'><strong>N°:</strong> {idCorto}</p>
        <p style='margin:0'><strong>Fecha:</strong> {fecha}</p>
        <p style='margin:0'><strong>Estado:</strong> Resuelto</p>
        <p style='margin:0'><strong>Tipo:</strong> {System.Web.HttpUtility.HtmlEncode(tipo)}</p>
        <p style='margin:0 0 12px 0'><strong>Prioridad:</strong> {System.Web.HttpUtility.HtmlEncode(prioridad)}</p>
        <div style='background:#f8f9fa;border:1px solid #eee;border-radius:6px;padding:12px'>
          <strong>Detalle de resolución:</strong>
          <div>{System.Web.HttpUtility.HtmlEncode(resolutionNote).Replace("\n", "<br/>")}</div>
        </div>
        <p style='margin:16px 0 0 0;color:#555;font-size:13px'>
          Si tu problema persiste, podés responder este correo o comunicarte nuevamente con soporte.
        </p>
        <p style='margin:0;color:#999;font-size:12px'>Este correo fue generado automáticamente. No respondas a este mensaje.</p>
      </td></tr>
    </table>
  </body>
</html>";
        }

    }
}