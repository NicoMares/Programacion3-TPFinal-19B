using System;
using CallCenter.Business.Repositories;
using CallCenter.Business.Services;
using System.Web.UI.WebControls; 
using System.Collections.Generic;

namespace CallCenter.Web.Account
{
    public partial class AssignRole : System.Web.UI.Page
    {
        private AssignRoleService _reg;
       
        private TextBox txtUsername;
        private TextBox txtFullName;
        private TextBox txtEmail;
        private Label lblMsg;
        private DropDownList cboRole;

        protected void Page_Init(object sender, EventArgs e)
        {
            string cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
            UserRepository repo = new UserRepository(cs);
            _reg = new AssignRoleService(repo);

            txtUsername = (TextBox)FindControl("txtUsername");
            txtFullName = (TextBox)FindControl("txtFullName");
            txtEmail = (TextBox)FindControl("txtEmail");
            lblMsg = (Label)FindControl("lblMsg");
            cboRole = (DropDownList)FindControl("cboRole");

           
            if (!IsPostBack && cboRole != null)
            {
                List<string> roles = GetRolesFromDb(repo);
                cboRole.Items.Clear();
                foreach (var role in roles)
                {
                    cboRole.Items.Add(new ListItem(role, role));
                }
            }
        }

        private List<string> GetRolesFromDb(UserRepository repo)
        {
            var roles = new List<string>();
            string cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
            using (var cn = new System.Data.SqlClient.SqlConnection(cs))
            {
                cn.Open();
                using (var cmd = new System.Data.SqlClient.SqlCommand("SELECT DISTINCT [Role] FROM dbo.Users WHERE [Role] IS NOT NULL AND [Role] <> '' ORDER BY [Role];", cn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        roles.Add(rd.GetString(0));
                    }
                }
            }
            return roles;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Web.UI.ValidationSettings.UnobtrusiveValidationMode =
                System.Web.UI.UnobtrusiveValidationMode.None;
        }

        protected void btnAssignRole_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text != null ? txtUsername.Text.Trim() : "";
            string fullName = txtFullName.Text == null ? "" : txtFullName.Text.Trim();
            string email = txtEmail.Text == null ? "" : txtEmail.Text.Trim();
            string role = cboRole == null ? "" : cboRole.SelectedValue;



            System.Guid id;
            string err;
           
            bool ok = _reg.AssignRole(username, fullName, email, out id, out err);
            if (ok)
            {
                // éxito → redirigimos a Login con mensaje
                Response.Redirect("~/Account/Login.aspx?registered=1");
            }
            else
            {
                lblMsg.CssClass = "text-danger fw-semibold";
                lblMsg.Text = err;
            }
        }
    }
}
