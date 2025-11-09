<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Forgot.aspx.cs" Inherits="CallCenter.Web.Account.Forgot" ResponseEncoding="utf-8" Culture="es-AR" UICulture="es-AR" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
  <meta charset="utf-8" />
  <title>Recuperar contraseña</title>
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">
  <div class="min-vh-100 d-flex align-items-center justify-content-center">
    <div class="card shadow" style="max-width:480px;">
      <div class="card-body p-4">
        <h4 class="mb-3">Recuperar contraseña</h4>
        <asp:ValidationSummary runat="server" CssClass="alert alert-danger py-2" />
        <div class="mb-3">
          <label for="txtEmail" class="form-label">Email</label>
          <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" />
          <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" ErrorMessage="Email obligatorio" CssClass="text-danger small" />
        </div>
        <asp:Button ID="btnSend" runat="server" Text="Enviar enlace" CssClass="btn btn-primary w-100" OnClick="btnSend_Click" />
        <asp:Label ID="lblMsg" runat="server" CssClass="d-block mt-3"></asp:Label>
        <div class="text-center small mt-3"><a href="~/Account/Login.aspx" runat="server">Volver al login</a></div>
      </div>
    </div>
  </div>
</form>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
