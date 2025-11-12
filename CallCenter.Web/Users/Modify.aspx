<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="Modify.aspx.cs"
    Inherits="CallCenter.Web.Users.Modify"
    ResponseEncoding="utf-8" Culture="es-AR" UICulture="es-AR" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
      <title>Modificar usuario</title>
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body class="bg-light">
<form id="form2" runat="server" class="container py-5">
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
  <div class="container">
    <a class="navbar-brand fw-semibold text-white" 
       href="~/Default.aspx" 
       id="HyperLinkHome" 
       runat="server">📞 Call Center</a>
    
  </div>
</nav>

      <div class="card shadow-sm mx-auto" style="max-width:600px;">
    <div class="card-body">



   
        <h4 class="mb-3">Modificar Usuario</h4>
        <asp:ValidationSummary runat="server" CssClass="alert alert-danger py-2" />
        <div class="mb-3">
          <label class="form-label" for="txtUsername">Usuario</label>
          <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" />
          <asp:RequiredFieldValidator runat="server" ControlToValidate="txtUsername" ErrorMessage="Usuario obligatorio" CssClass="text-danger small" />
        </div>
        <div class="mb-3">
          <label class="form-label" for="txtFullName">Nombre completo</label>
          <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" />
          <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFullName" ErrorMessage="Nombre obligatorio" CssClass="text-danger small" />
        </div>
        <div class="mb-3">
          <label class="form-label" for="txtEmail">Email</label>
          <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" />
          <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" ErrorMessage="Email obligatorio" CssClass="text-danger small" />
        </div>
    <div class="mb-3">
   <label class="form-label">Rol</label>
   <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select">
     <asp:ListItem Text="Seleccionar..." Value="" />
     <asp:ListItem Text="Administrador" Value="Administrador" />
     <asp:ListItem Text="Supervisor" Value="Supervisor" />
     <asp:ListItem Text="Telefonista" Value="Telefonista" />
   </asp:DropDownList>
 </div>


      
      <div class="d-flex justify-content-between">
        <asp:Button ID="btnModify" runat="server" Text="Modificar Usuario"
                    CssClass="btn btn-primary px-4"
                    OnClick="btnModify_Click" />
        <asp:HyperLink ID="hlBack" runat="server"
                       NavigateUrl="~/Default.aspx"
                       CssClass="btn btn-outline-secondary px-4">
          Volver al panel
        </asp:HyperLink>
      </div>

      <asp:Label ID="Label1" runat="server" CssClass="d-block mt-3 text-center fw-semibold"></asp:Label>
 

 
    </div>
  </div>
</form>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>