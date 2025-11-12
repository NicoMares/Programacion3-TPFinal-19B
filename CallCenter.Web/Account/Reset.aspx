<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Reset.aspx.cs" Inherits="CallCenter.Web.Account.Reset" ResponseEncoding="utf-8" Culture="es-AR" UICulture="es-AR" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
  <meta charset="utf-8" />
  <title>Restablecer contraseña</title>
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="~/Content/site.css" rel="stylesheet" />

</head>
<body>
<form id="form1" runat="server">
  <div class="min-vh-100 d-flex align-items-center justify-content-center">
    <div class="card shadow" style="max-width:480px;">
      <div class="card-body p-4">
        <h4 class="mb-3">Nueva contraseña</h4>
        <asp:ValidationSummary runat="server" CssClass="alert alert-danger py-2" />
        <div class="mb-3">
          <label class="form-label" for="txtPassword">Contraseña</label>
          <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" />
          <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword" ErrorMessage="Obligatorio" CssClass="text-danger small" />
        </div>
        <div class="mb-3">
          <label class="form-label" for="txtConfirm">Repetir contraseña</label>
          <asp:TextBox ID="txtConfirm" runat="server" TextMode="Password" CssClass="form-control" />
          <asp:RequiredFieldValidator runat="server" ControlToValidate="txtConfirm" ErrorMessage="Obligatorio" CssClass="text-danger small" />
          <asp:CompareValidator runat="server" ControlToCompare="txtPassword" ControlToValidate="txtConfirm" ErrorMessage="No coinciden" CssClass="text-danger small" />
        </div>
        <asp:Button ID="btnReset" runat="server" Text="Cambiar contraseña" CssClass="btn btn-success w-100" OnClick="btnReset_Click" />
        <asp:Label ID="lblMsg" runat="server" CssClass="d-block mt-3"></asp:Label>
        <div class="text-center small mt-3"><a href="~/Account/Login.aspx" runat="server">Volver al login</a></div>
      </div>
    </div>
  </div>
</form>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
