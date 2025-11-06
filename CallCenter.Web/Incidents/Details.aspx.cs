// Incidents/Details.aspx.cs
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace CallCenter.Web.Incidents
{
    public partial class Details : System.Web.UI.Page
    {
        private string _cs;
        private Guid _incidentId = Guid.Empty;
        private Guid _userId = Guid.Empty;
        private string _username = "";
        private string _role = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Web.UI.ValidationSettings.UnobtrusiveValidationMode =
                System.Web.UI.UnobtrusiveValidationMode.None;

            _cs = System.Configuration.ConfigurationManager
                      .ConnectionStrings["CallCenterDb"].ConnectionString;

            string qs = Request.QueryString["id"];
            if (string.IsNullOrEmpty(qs) || !Guid.TryParse(qs, out _incidentId))
            {
                lblInfo.CssClass = "alert alert-danger";
                lblInfo.Text = "Incidencia no válida.";
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
                           i.Problem, i.Status, i.CreatedAt, i.AssignedToUserId
                    FROM dbo.Incidents i
                    INNER JOIN dbo.Customers c ON c.Id = i.CustomerId
                    INNER JOIN dbo.Priorities p ON p.Id = i.PriorityId
                    INNER JOIN dbo.IncidentTypes t ON t.Id = i.IncidentTypeId
                    WHERE i.Id = @id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = _incidentId;
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            // persistir datos para permisos/ddl
                            ViewState["AssignedToUserId"] = rd.IsDBNull(7) ? (object)DBNull.Value : rd.GetGuid(7);
                            ViewState["Status"] = Convert.ToString(rd["Status"]) ?? "";

                            lblInfo.Text = "<div class='card p-3 shadow-sm'>" +
                                "<strong>ID:</strong> " + rd["Id"] + "<br/>" +
                                "<strong>Cliente:</strong> " + HttpUtility.HtmlEncode(Convert.ToString(rd["Cliente"])) + "<br/>" +
                                "<strong>Tipo:</strong> " + HttpUtility.HtmlEncode(Convert.ToString(rd["Tipo"])) + "<br/>" +
                                "<strong>Prioridad:</strong> " + HttpUtility.HtmlEncode(Convert.ToString(rd["Prioridad"])) + "<br/>" +
                                "<strong>Estado:</strong> " + HttpUtility.HtmlEncode(Convert.ToString(rd["Status"])) + "<br/>" +
                                "<strong>Problemática:</strong> " + HttpUtility.HtmlEncode(Convert.ToString(rd["Problem"])) + "<br/>" +
                                "<strong>Fecha:</strong> " + Convert.ToDateTime(rd["CreatedAt"]).ToLocalTime().ToString("dd/MM/yyyy HH:mm") + "</div>";
                        }
                        else
                        {
                            lblInfo.CssClass = "alert alert-danger";
                            lblInfo.Text = "Incidencia no encontrada.";
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
            btnAnalysis.Enabled = false;
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
                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO dbo.IncidentMessages(IncidentId, UserId, SenderName, Message)
                    VALUES(@iid, @uid, @sn, @msg);", cn))
                {
                    cmd.Parameters.Add("@iid", SqlDbType.UniqueIdentifier).Value = _incidentId;
                    cmd.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = _userId;
                    cmd.Parameters.Add("@sn", SqlDbType.NVarChar, 200).Value = _username;
                    cmd.Parameters.Add("@msg", SqlDbType.NVarChar, 2000).Value = msg;
                    cmd.ExecuteNonQuery();
                }
            }

            // PRG
            Response.Redirect("Details.aspx?id=" + _incidentId.ToString());
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
            if (IsFinalStatus())
            {
                lblActionsMsg.CssClass = "text-warning";
                lblActionsMsg.Text = "La incidencia está finalizada. No se puede reasignar.";
                return;
            }

            bool canAssign =
                string.Equals(_role, "Administrador", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_role, "Supervisor", StringComparison.OrdinalIgnoreCase);
            if (!canAssign)
            {
                lblActionsMsg.CssClass = "text-danger";
                lblActionsMsg.Text = "No tenés permisos para reasignar.";
                return;
            }

            Guid newUid;
            if (!Guid.TryParse(ddlAssign.SelectedValue, out newUid) || newUid == Guid.Empty)
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
                        using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.Incidents
SET AssignedToUserId=@uid, Status=N'Asignado'
WHERE Id=@id;", cn, tx))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = _incidentId;
                            cmd.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = newUid;
                            int n = cmd.ExecuteNonQuery();
                            if (n == 0) throw new Exception("Incidencia inexistente.");
                        }

                        string assigneeName = ddlAssign.SelectedItem != null ? ddlAssign.SelectedItem.Text : "(usuario)";
                        using (SqlCommand m = new SqlCommand(@"
INSERT INTO dbo.IncidentMessages(IncidentId, UserId, SenderName, Message)
VALUES(@iid, @uid, @sn, @msg);", cn, tx))
                        {
                            m.Parameters.Add("@iid", SqlDbType.UniqueIdentifier).Value = _incidentId;
                            m.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = _userId;
                            m.Parameters.Add("@sn", SqlDbType.NVarChar, 200).Value = _username;
                            m.Parameters.Add("@msg", SqlDbType.NVarChar, 2000).Value =
                                "Incidencia reasignada a " + assigneeName + ".";
                            m.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        lblActionsMsg.CssClass = "text-danger";
                        lblActionsMsg.Text = "No se pudo reasignar.";
                        return;
                    }
                }
            }

            Response.Redirect("Details.aspx?id=" + _incidentId.ToString() + "&reassigned=1");
        }

        protected void btnResolve_Click(object sender, EventArgs e)
        {
            if (IsFinalStatus())
            {
                lblActionsMsg.CssClass = "text-warning";
                lblActionsMsg.Text = "La incidencia está finalizada. No se puede resolver nuevamente.";
                return;
            }

            Page.Validate("resolve");
            if (!Page.IsValid) return;

            string note = txtResolution.Text == null ? "" : txtResolution.Text.Trim();

            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.Incidents
SET Status=N'Resuelto', ResolvedAt=SYSUTCDATETIME(), ResolutionNote=@n
WHERE Id=@id;", cn, tx))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = _incidentId;
                            cmd.Parameters.Add("@n", SqlDbType.NVarChar, 1000).Value = (object)note ?? DBNull.Value;
                            int n = cmd.ExecuteNonQuery();
                            if (n == 0) throw new Exception("Incidencia inexistente.");
                        }

                        using (SqlCommand m = new SqlCommand(@"
INSERT INTO dbo.IncidentMessages(IncidentId, UserId, SenderName, Message)
VALUES(@iid, @uid, @sn, @msg);", cn, tx))
                        {
                            m.Parameters.Add("@iid", SqlDbType.UniqueIdentifier).Value = _incidentId;
                            m.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = _userId;
                            m.Parameters.Add("@sn", SqlDbType.NVarChar, 200).Value = _username;
                            m.Parameters.Add("@msg", SqlDbType.NVarChar, 2000).Value =
                                "Incidencia RESUELTA. Nota: " + note;
                            m.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        lblActionsMsg.CssClass = "text-danger";
                        lblActionsMsg.Text = "No se pudo resolver la incidencia.";
                        return;
                    }
                }
            }

            Response.Redirect("Details.aspx?id=" + _incidentId.ToString() + "&resolved=1");
        }

        protected void btnClose_Click(object sender, EventArgs e)
        {
            if (IsFinalStatus())
            {
                lblActionsMsg.CssClass = "text-warning";
                lblActionsMsg.Text = "La incidencia ya está finalizada. No se puede cerrar nuevamente.";
                return;
            }

            Page.Validate("close");
            if (!Page.IsValid) return;

            string comment = txtClose.Text == null ? "" : txtClose.Text.Trim();

            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.Incidents
SET Status=N'Cerrado', ClosedComment=@c
WHERE Id=@id;", cn, tx))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = _incidentId;
                            cmd.Parameters.Add("@c", SqlDbType.NVarChar, 1000).Value = comment;
                            int n = cmd.ExecuteNonQuery();
                            if (n == 0) throw new Exception("Incidencia inexistente.");
                        }

                        using (SqlCommand m = new SqlCommand(@"
INSERT INTO dbo.IncidentMessages(IncidentId, UserId, SenderName, Message)
VALUES(@iid, @uid, @sn, @msg);", cn, tx))
                        {
                            m.Parameters.Add("@iid", SqlDbType.UniqueIdentifier).Value = _incidentId;
                            m.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = _userId;
                            m.Parameters.Add("@sn", SqlDbType.NVarChar, 200).Value = _username;
                            m.Parameters.Add("@msg", SqlDbType.NVarChar, 2000).Value =
                                "Incidencia CERRADA. Comentario: " + comment;
                            m.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        lblActionsMsg.CssClass = "text-danger";
                        lblActionsMsg.Text = "No se pudo cerrar la incidencia.";
                        return;
                    }
                }
            }

            Response.Redirect("Details.aspx?id=" + _incidentId.ToString() + "&closed=1");
        }
    }
}
