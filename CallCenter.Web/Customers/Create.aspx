<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Create.aspx.cs" Inherits="CallCenter.Web.Customers.Create" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
  <meta charset="utf-8" />
  <title>Nuevo cliente</title>
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">
  <div class="container py-4" style="max-width:760px;">
    <h3 class="mb-3">Nuevo cliente</h3>
    <asp:ValidationSummary runat="server" CssClass="alert alert-danger py-2" ValidationGroup="c" />

    <div class="row g-3">
      <div class="col-md-6">
        <label class="form-label">Nombre</label>
        <asp:TextBox ID="txtName" runat="server" CssClass="form-control" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtName" ErrorMessage="Obligatorio" CssClass="text-danger small" ValidationGroup="c" />
      </div>
      <div class="col-md-6">
        <label class="form-label">Documento</label>
        <asp:TextBox ID="txtDoc" runat="server" CssClass="form-control" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDoc" ErrorMessage="Obligatorio" CssClass="text-danger small" ValidationGroup="c" />
      </div>
      <div class="col-md-6">
        <label class="form-label">Email</label>
        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" ErrorMessage="Obligatorio" CssClass="text-danger small" ValidationGroup="c" />
      </div>
      <div class="col-md-6">
        <label class="form-label">Teléfono</label>
        <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" />
      </div>
      <div class="col-12">
        <label class="form-label">Dirección</label>
        <asp:TextBox ID="txtAddress" runat="server" CssClass="form-control" />
      </div>
    </div>

    <div class="mt-4 d-flex gap-2">
      <asp:Button ID="btnSave" runat="server" Text="Guardar"
          CssClass="btn btn-success"
          ValidationGroup="c" UseSubmitBehavior="false"
          OnClientClick="if(typeof(Page_ClientValidate)==='function'){ if(!Page_ClientValidate('c')){ return false; } } this.disabled=true; this.value='Guardando...';"
          OnClick="btnSave_Click" />
      <a runat="server" href="~/Incidents/Create.aspx" class="btn btn-outline-secondary">Cancelar</a>
    </div>

    <asp:Label ID="lblMsg" runat="server" CssClass="d-block mt-3"></asp:Label>
  </div>
</form>
</body>
</html>
