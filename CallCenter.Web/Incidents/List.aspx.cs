using System;
using System.Data;
using System.Data.SqlClient;

namespace CallCenter.Web.Incidents
{
    public partial class List : System.Web.UI.Page
    {
        private string _cs;

        protected void Page_Load(object sender, EventArgs e)
        {
            _cs = System.Configuration.ConfigurationManager
                    .ConnectionStrings["CallCenterDb"].ConnectionString;

            if (!IsPostBack)
                BindGrid();
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            BindGrid();
        }

        private void BindGrid()
        {
            Guid userId = Guid.Empty;
            string role = null;
            string username = Context?.User?.Identity?.Name ?? "";
            if (!string.IsNullOrWhiteSpace(username))
            {
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
                                userId = rd.GetGuid(0);
                                role = rd.GetString(1);
                            }
                        }
                    }
                }
            }

            bool onlyMine = string.Equals(role, "Telefonista", StringComparison.OrdinalIgnoreCase);

            string status = null, priority = null, q = null;
            try { status = ddlStatus?.SelectedValue; } catch { }
            try { priority = ddlPriority?.SelectedValue; } catch { }
            try { q = txtSearch?.Text?.Trim(); } catch { }

            var dt = new DataTable();
            using (var cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (var cmd = new SqlCommand(@"
SELECT i.Id, i.CreatedAt, i.Status, i.Problem,
       c.Name   AS CustomerName,
       t.Name   AS TypeName,
       p.Name   AS PriorityName,
       u.Username AS AssignedTo
FROM dbo.Incidents i
JOIN dbo.Customers     c ON c.Id = i.CustomerId
JOIN dbo.IncidentTypes t ON t.Id = i.IncidentTypeId
JOIN dbo.Priorities    p ON p.Id = i.PriorityId
LEFT JOIN dbo.Users    u ON u.Id = i.AssignedToUserId
WHERE
    -- restricción por rol
    (@onlyMine = 0 OR i.AssignedToUserId = @uid)
    -- filtros opcionales
    AND (@status IS NULL OR i.Status = @status)
    AND (@priority IS NULL OR p.Name = @priority)
    AND (
         @q IS NULL
         OR c.Name LIKE '%' + @q + '%'
         OR i.Problem LIKE '%' + @q + '%'
         OR t.Name LIKE '%' + @q + '%'
         OR u.Username LIKE '%' + @q + '%'
        )
ORDER BY i.CreatedAt DESC;", cn))
                {
                    cmd.Parameters.Add("@onlyMine", SqlDbType.Bit).Value = onlyMine ? 1 : 0;
                    cmd.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value =
                        (userId == Guid.Empty ? (object)DBNull.Value : userId);

                    cmd.Parameters.Add("@status", SqlDbType.NVarChar, 50).Value =
                        string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status;
                    cmd.Parameters.Add("@priority", SqlDbType.NVarChar, 50).Value =
                        string.IsNullOrWhiteSpace(priority) ? (object)DBNull.Value : priority;
                    cmd.Parameters.Add("@q", SqlDbType.NVarChar, 200).Value =
                        string.IsNullOrWhiteSpace(q) ? (object)DBNull.Value : q;

                    using (var rd = cmd.ExecuteReader())
                        dt.Load(rd);
                }
            }

            if (!dt.Columns.Contains("CreatedAtLocal"))
                dt.Columns.Add("CreatedAtLocal", typeof(DateTime));
            foreach (DataRow r in dt.Rows)
                r["CreatedAtLocal"] = Convert.ToDateTime(r["CreatedAt"]).ToLocalTime();

            gvIncidents.DataSource = dt;
            gvIncidents.DataBind();
        }
        protected void btnClear_Click(object sender, EventArgs e)
        {
            if (txtSearch != null) txtSearch.Text = "";
            if (ddlStatus != null) ddlStatus.ClearSelection();
            if (ddlPriority != null) ddlPriority.ClearSelection();
            if (ddlType != null) ddlType.ClearSelection();

            BindGrid();
        }
        protected void gv_PageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvIncidents.PageIndex = e.NewPageIndex;
            BindGrid();
        }


    }
}
