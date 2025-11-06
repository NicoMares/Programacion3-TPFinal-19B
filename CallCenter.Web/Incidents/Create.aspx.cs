// Incidents/Create.aspx.cs
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace CallCenter.Web.Incidents
{
    public partial class Create : System.Web.UI.Page
    {
        private string _cs;

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Web.UI.ValidationSettings.UnobtrusiveValidationMode =
                System.Web.UI.UnobtrusiveValidationMode.None;

            _cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;

            if (!IsPostBack)
            {
                BindLookups();

                // Preseleccionar cliente si venís de "Nuevo cliente"
                string cid = Request.QueryString["customerId"];
                if (!string.IsNullOrEmpty(cid))
                {
                    var item = ddlCustomer.Items.FindByValue(cid);
                    if (item != null) ddlCustomer.SelectedValue = cid;

                    if (Request.QueryString["created"] == "1")
                    {
                        lblMsg.CssClass = "alert alert-success";
                        lblMsg.Text = "Cliente creado correctamente. Ya podés cargar la incidencia.";
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
            Page.Validate("inc");
            if (!Page.IsValid) return;

            int customerId, priorityId, typeId;
            if (!int.TryParse(ddlCustomer.SelectedValue, out customerId) ||
                !int.TryParse(ddlPriority.SelectedValue, out priorityId) ||
                !int.TryParse(ddlType.SelectedValue, out typeId))
            {
                lblMsg.CssClass = "alert alert-danger";
                lblMsg.Text = "Seleccioná Cliente, Prioridad y Tipo válidos.";
                return;
            }

            string problem = txtProblem.Text == null ? "" : txtProblem.Text.Trim();
            if (problem.Length == 0)
            {
                lblMsg.CssClass = "alert alert-danger";
                lblMsg.Text = "La problemática es obligatoria.";
                return;
            }

            // Usuario actual
            string username = Context.User == null ? "" : Context.User.Identity.Name;
            if (string.IsNullOrEmpty(username))
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            Guid currentUserId = Guid.Empty;
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand u = new SqlCommand(
                    "SELECT TOP 1 Id FROM dbo.Users WHERE Username=@u AND IsDeleted=0 AND IsBlocked=0;", cn))
                {
                    u.Parameters.Add("@u", SqlDbType.NVarChar, 100).Value = username;
                    object o = u.ExecuteScalar();
                    if (o == null)
                    {
                        lblMsg.CssClass = "alert alert-danger";
                        lblMsg.Text = "Usuario inválido.";
                        return;
                    }
                    currentUserId = (Guid)o;
                }

                // Insert Incidence (nace Abierto y asignada al creador)
                Guid newId = Guid.Empty;
                using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.Incidents
    (Id, CustomerId, IncidentTypeId, PriorityId, Problem, Status, CreatedAt, CreatedByUserId, AssignedToUserId)
OUTPUT inserted.Id
VALUES
    (NEWID(), @c, @t, @p, @prob, N'Abierto', SYSUTCDATETIME(), @uid, @uid);", cn))
                {
                    cmd.Parameters.Add("@c", SqlDbType.Int).Value = customerId;
                    cmd.Parameters.Add("@t", SqlDbType.Int).Value = typeId;
                    cmd.Parameters.Add("@p", SqlDbType.Int).Value = priorityId;
                    cmd.Parameters.Add("@prob", SqlDbType.NVarChar, 2000).Value = problem;
                    cmd.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = currentUserId;

                    object o = cmd.ExecuteScalar();
                    if (o != null) newId = (Guid)o;
                }

                if (newId == Guid.Empty)
                {
                    lblMsg.CssClass = "alert alert-danger";
                    lblMsg.Text = "No se pudo crear la incidencia.";
                    return;
                }

                // (Opcional) dejar traza inicial en el chat
                using (SqlCommand m = new SqlCommand(@"
INSERT INTO dbo.IncidentMessages(IncidentId, UserId, SenderName, Message)
VALUES(@iid, @uid, @sn, @msg);", cn))
                {
                    m.Parameters.Add("@iid", SqlDbType.UniqueIdentifier).Value = newId;
                    m.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = currentUserId;
                    m.Parameters.Add("@sn", SqlDbType.NVarChar, 200).Value = username;
                    m.Parameters.Add("@msg", SqlDbType.NVarChar, 2000).Value = "Incidencia creada (estado: Abierto).";
                    m.ExecuteNonQuery();
                }

                // (Opcional) envío de mail al cliente aquí si ya tenés tu servicio listo
                // string customerEmail = GetCustomerEmail(cn, customerId);  // implementar si querés
                // EmailSender.Send(customerEmail, "Alta de incidencia", "...");

                // Redirigir al detalle
                Response.Redirect("~/Incidents/Details.aspx?id=" + newId.ToString());
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
