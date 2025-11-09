<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="RoleAssign.aspx.cs"
    Inherits="CallCenter.Web.Account.Register"
    ResponseEncoding="utf-8" Culture="es-AR" UICulture="es-AR" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <title>Modify Role | CallCenter</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style> body{background:#f5f7fb;} .auth-card{max-width:480px;} </style>
</head>
<body>
<form id="form1" runat="server">
  <div class="min-vh-100 d-flex align-items-center justify-content-center">
    <div class="card shadow auth-card">
   
        <h4 class="mb-3">Modificar Rol</h4>
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
  <label class="form-label" for="cboRole">Rol</label>
  <asp:DropDownList ID="cboRole" runat="server" CssClass="form-control" />
  <asp:RequiredFieldValidator runat="server" ControlToValidate="cboRole" ErrorMessage="Rol obligatorio" CssClass="text-danger small" />
</div>
        <div class="mb-3">
          
        <asp:Button ID="btnRegister" runat="server" Text="Crear Usuario" CssClass="btn btn-success w-100" OnClick="btnRegister_Click" />
        <asp:Label ID="lblMsg" runat="server" CssClass="d-block mt-3"></asp:Label>
        <hr class="my-4" />
        <div class="text-center small text-muted">
          ¿Ya tenés cuenta? <a href="~/Account/Login.aspx">Iniciar sesión</a>
        </div>
      </div>
    </div>
  </div>
</form>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
