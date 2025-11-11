<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Create.aspx.cs" Inherits="CallCenter.Web.Users.Create" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
  <meta charset="utf-8" />
  <title>Crear usuario</title>
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body class="bg-light">
<form id="form1" runat="server" class="container py-5">
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
      <h3 class="card-title mb-4 text-center">Nuevo Usuario</h3>

      <div class="mb-3">
        <label class="form-label">Nombre de usuario</label>
        <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" required="true"></asp:TextBox>
      </div>

      <div class="mb-3">
        <label class="form-label">Email</label>
        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" required="true"></asp:TextBox>
      </div>

      <div class="mb-3">
        <label class="form-label">Contraseña</label>
        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" required="true"></asp:TextBox>
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
        <asp:Button ID="btnCreate" runat="server" Text="Crear usuario"
                    CssClass="btn btn-primary px-4"
                    OnClick="btnCreate_Click" />
        <asp:HyperLink ID="hlBack" runat="server"
                       NavigateUrl="~/Default.aspx"
                       CssClass="btn btn-outline-secondary px-4">
          Volver al panel
        </asp:HyperLink>
      </div>

      <asp:Label ID="lblMsg" runat="server" CssClass="d-block mt-3 text-center fw-semibold"></asp:Label>
    </div>
  </div>
</form>
</body>
</html>
