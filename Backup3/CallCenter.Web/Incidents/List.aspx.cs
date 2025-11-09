using System;
using System.Data;
using System.Data.SqlClient;

namespace CallCenter.Web.Incidents
{
    public partial class List : System.Web.UI.Page
    {
        private string _cs;
        private Guid _userId = Guid.Empty;
        private string _role = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Web.UI.ValidationSettings.UnobtrusiveValidationMode =
                System.Web.UI.UnobtrusiveValidationMode.None;

            _cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;

            if (!IsPostBack)
            {
                LoadUser();
                BindGrid();
            }
        }

        private void LoadUser()
        {
            string username = Context.User == null ? "" : Context.User.Identity.Name;
            if (string.IsNullOrEmpty(username))
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
                    cmd.Parameters.Add("@u", SqlDbType.NVarChar, 100).Value = username;
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

        private void BindGrid()
        {
            bool onlyMine = string.Equals(_role, "Telefonista", StringComparison.OrdinalIgnoreCase);

            string sql = @"
SELECT i.Id,
       c.Name AS Cliente,
       t.Name AS Tipo,
       p.Name AS Prioridad,
       i.Status,
       i.CreatedAt,
       u.Username AS Assignee   -- << mostrar asignado
FROM dbo.Incidents i
JOIN dbo.Customers     c ON c.Id  = i.CustomerId
JOIN dbo.IncidentTypes t ON t.Id  = i.IncidentTypeId
JOIN dbo.Priorities    p ON p.Id  = i.PriorityId
LEFT JOIN dbo.Users    u ON u.Id  = i.AssignedToUserId  -- << puede ser null
WHERE 1=1
";

            if (onlyMine) sql += " AND i.AssignedToUserId = @uid";
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                sql += " AND (c.Name LIKE @q OR i.Problem LIKE @q OR CONVERT(NVARCHAR(36), i.Id) LIKE @q)";
            if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                sql += " AND i.Status = @st";
            if (!string.IsNullOrEmpty(ddlPriority.SelectedValue))
                sql += " AND i.PriorityId = @pr";
            if (!string.IsNullOrEmpty(ddlType.SelectedValue))
                sql += " AND i.IncidentTypeId = @tp";

            sql += " ORDER BY i.CreatedAt DESC;";

            using (var cn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cn.Open();

                if (onlyMine)
                    cmd.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = _userId;
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    cmd.Parameters.Add("@q", SqlDbType.NVarChar, 300).Value = "%" + txtSearch.Text.Trim() + "%";
                if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                    cmd.Parameters.Add("@st", SqlDbType.NVarChar, 20).Value = ddlStatus.SelectedValue;
                if (!string.IsNullOrEmpty(ddlPriority.SelectedValue))
                    cmd.Parameters.Add("@pr", SqlDbType.Int).Value = int.Parse(ddlPriority.SelectedValue);
                if (!string.IsNullOrEmpty(ddlType.SelectedValue))
                    cmd.Parameters.Add("@tp", SqlDbType.Int).Value = int.Parse(ddlType.SelectedValue);

                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
                gv.DataSource = dt;
                gv.DataBind();

                lblInfo.CssClass = "text-muted";
                lblInfo.Text = (onlyMine ? "Mostrando incidencias asignadas a mí" : "Mostrando todas las incidencias")
                               + " (" + dt.Rows.Count + ")";
            }
        }


        protected void gv_PageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gv.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            gv.PageIndex = 0;
            BindGrid();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlStatus.ClearSelection();
            ddlPriority.ClearSelection();
            ddlType.ClearSelection();
            gv.PageIndex = 0;
            BindGrid();
        }
    }
}
