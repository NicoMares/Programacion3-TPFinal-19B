using System;
using CallCenter.Business.Repositories;
using CallCenter.Domain.Abstractions;
using CallCenter.Domain.Entities;

namespace CallCenter.Web.Customers
{
    public partial class Create : System.Web.UI.Page
    {
        private ICustomerRepository _repo;

        protected void Page_Init(object sender, EventArgs e)
        {
            string cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
            _repo = new CustomerRepository(cs);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Web.UI.ValidationSettings.UnobtrusiveValidationMode =
                System.Web.UI.UnobtrusiveValidationMode.None;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate("c");
            if (!Page.IsValid) return;

            string name = txtName.Text == null ? "" : txtName.Text.Trim();
            string doc = txtDoc.Text == null ? "" : txtDoc.Text.Trim();
            string email = txtEmail.Text == null ? "" : txtEmail.Text.Trim();
            string phone = txtPhone.Text == null ? "" : txtPhone.Text.Trim();
            string addr = txtAddress.Text == null ? "" : txtAddress.Text.Trim();

            // Validación de documento: solo dígitos, no se pueden negativos, ni caracteres especiales
            if (!System.Text.RegularExpressions.Regex.IsMatch(doc, @"^\d+$") || (doc.StartsWith("-")))
            {
                lblMsg.CssClass = "alert alert-danger";
                lblMsg.Text = "El documento debe ser un número positivo y no puede contener caracteres .";
                return;
            }

            // Validación de email: sin caracteres especiales que no estan permitidos
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$"))
            {
                lblMsg.CssClass = "alert alert-danger";
                lblMsg.Text = "El email no es válido o contiene caracteres especiales lo cual no estan permitidos.";
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d+$") || (doc.StartsWith("-")))
            {
                lblMsg.CssClass = "alert alert-danger";
                lblMsg.Text = "El Numero de Telefono no es válido o contiene caracteres  lo cual no esta permitidos.";
                return;
            }



            if (_repo.ExistsByDocumentPhoneOrEmail(doc, email,phone))
            {
                lblMsg.CssClass = "alert alert-danger";
                lblMsg.Text = "Ya existe un cliente con ese documento , Numero de Telefono o email.";
                return;
            }


            Customer c = new Customer();
            c.Name = name;
            c.Document = doc;
            c.Email = email;
            c.Phone = phone;
            c.Address = addr;

            int newId = _repo.Insert(c);
            if (newId <= 0)
            {
                lblMsg.CssClass = "alert alert-danger";
                lblMsg.Text = "No se pudo crear el cliente.";
                return;
            }

            // volver a crear incidencia con el cliente preseleccionado
            Response.Redirect("~/Incidents/Create.aspx?customerId=" + newId + "&created=1");
        }
    }
}
