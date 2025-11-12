using System;
using CallCenter.Business.Repositories;
using CallCenter.Business.Services;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CallCenter.Web.Users
{
    public partial class Modify : System.Web.UI.Page
    {
        private ModifyService _reg;

        private TextBox txtUsername;
        private TextBox txtFullName;
        private TextBox txtEmail;
        private Label lblMsg;
        private DropDownList ddlRole; 
  

        protected void Page_Init(object sender, EventArgs e)
        {
            string cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
            UserRepository repo = new UserRepository(cs);
            _reg = new ModifyService(repo);

            txtUsername = (TextBox)FindControl("txtUsername");
            txtFullName = (TextBox)FindControl("txtFullName");
            txtEmail = (TextBox)FindControl("txtEmail");
            lblMsg = (Label)FindControl("lblMsg");
            ddlRole = (DropDownList)FindControl("ddlRole"); 
          
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Web.UI.ValidationSettings.UnobtrusiveValidationMode =
                System.Web.UI.UnobtrusiveValidationMode.None;

            if (!IsPostBack)
            {           
                ddlRole.Items.Clear();
                string cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
                using (var cn = new SqlConnection(cs))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand("SELECT DISTINCT [Role] FROM dbo.Users WHERE IsDeleted=0 AND IsBlocked=0", cn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string role = reader["Role"].ToString();
                            if (!string.IsNullOrWhiteSpace(role))
                                ddlRole.Items.Add(new ListItem(role, role));
                        }
                    }
                }
            }
        }

        protected void btnModify_Click(object sender, EventArgs e)
        {
            string username = txtUsername?.Text?.Trim() ?? "";
            string fullName = txtFullName?.Text?.Trim() ?? "";
            string email = txtEmail?.Text?.Trim() ?? "";
            string role = ddlRole?.SelectedValue ?? "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            {
                lblMsg.CssClass = "text-danger fw-semibold";
                lblMsg.Text = "⚠️ Complete todos los campos antes de continuar.";
                return;
            }

            string cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
            try
            {
                using (var cn = new SqlConnection(cs))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand(@"
UPDATE dbo.Users
SET FullName = @f, Email = @e, Role = @r
WHERE Username = @u AND IsDeleted =0 AND IsBlocked =0", cn))
                    {
                        cmd.Parameters.Add("@u", SqlDbType.NVarChar, 100).Value = username;
                        cmd.Parameters.Add("@f", SqlDbType.NVarChar, 200).Value = fullName;
                        cmd.Parameters.Add("@e", SqlDbType.NVarChar, 256).Value = email;
                        cmd.Parameters.Add("@r", SqlDbType.NVarChar, 20).Value = role;
                        int rows = cmd.ExecuteNonQuery();
                    }
                }

                txtUsername.Text = string.Empty;
                txtFullName.Text = string.Empty;
                txtEmail.Text = string.Empty;
                ddlRole.SelectedIndex = 0;
                lblMsg.CssClass = "text-success text-center fw-semibold";
                lblMsg.Text = "✅ Modificación realizada correctamente.";
            }
            catch (Exception ex)
            {
                lblMsg.CssClass = "text-danger text-center";
                lblMsg.Text = "❌ Error al crear el usuario: " + ex.Message;
            }
}
    }
}