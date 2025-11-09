using System;
using System.Data;
using System.Data.SqlClient;

namespace CallCenter.Web.Customers
{
    public partial class List : System.Web.UI.Page
    {
        private string _cs;

        protected void Page_Load(object sender, EventArgs e)
        {
            _cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
            if (!IsPostBack) BindGrid();
        }

        private void BindGrid()
        {
            string sql = @"
SELECT Id, Name, Document, Email, Phone, Address
FROM dbo.Customers
WHERE IsDeleted = 0
";

            if (!string.IsNullOrWhiteSpace(txtName.Text))
                sql += " AND Name LIKE @n";
            if (!string.IsNullOrWhiteSpace(txtDoc.Text))
                sql += " AND Document LIKE @d";
            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
                sql += " AND Email LIKE @e";

            sql += " ORDER BY Name;";

            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    if (!string.IsNullOrWhiteSpace(txtName.Text))
                        cmd.Parameters.Add("@n", SqlDbType.NVarChar, 220).Value = "%" + txtName.Text.Trim() + "%";
                    if (!string.IsNullOrWhiteSpace(txtDoc.Text))
                        cmd.Parameters.Add("@d", SqlDbType.NVarChar, 60).Value = "%" + txtDoc.Text.Trim() + "%";
                    if (!string.IsNullOrWhiteSpace(txtEmail.Text))
                        cmd.Parameters.Add("@e", SqlDbType.NVarChar, 270).Value = "%" + txtEmail.Text.Trim() + "%";

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        da.Fill(dt);

                    gv.DataSource = dt;
                    gv.DataBind();

                    lblInfo.Text = "Resultados: " + dt.Rows.Count;
                }
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
            txtName.Text = "";
            txtDoc.Text = "";
            txtEmail.Text = "";
            gv.PageIndex = 0;
            BindGrid();
        }
    }
}
