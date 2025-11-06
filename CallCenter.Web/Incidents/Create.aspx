<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Create.aspx.cs" Inherits="CallCenter.Web.Incidents.Create" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
  <meta charset="utf-8" />
  <title>Nueva Incidencia</title>
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">
  <div class="container py-4" style="max-width:900px;">
    <h3 class="mb-3">Nueva Incidencia</h3>
    <asp:ValidationSummary runat="server" CssClass="alert alert-danger py-2" ValidationGroup="inc" />

    <div class="row g-3">
      <div class="col-md-6">
        <label class="form-label">Cliente</label>
        <div class="d-flex justify-content-between align-items-end">
            <label class="form-label mb-0">Cliente</label>
            <asp:HyperLink ID="hlNewCustomer" runat="server"
                NavigateUrl="~/Customers/Create.aspx"
                CssClass="btn btn-sm btn-outline-primary">
            + Nuevo cliente
            </asp:HyperLink>
        </div>
        <asp:DropDownList ID="ddlCustomer" runat="server" CssClass="form-select" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlCustomer" InitialValue="" ErrorMessage="Cliente obligatorio" CssClass="text-danger small" ValidationGroup="inc" />
      </div>
      <div class="col-md-3">
        <label class="form-label">Tipo</label>
        <asp:DropDownList ID="ddlType" runat="server" CssClass="form-select" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlType" InitialValue="" ErrorMessage="Tipo obligatorio" CssClass="text-danger small" ValidationGroup="inc" />
      </div>
      <div class="col-md-3">
        <label class="form-label">Prioridad</label>
        <asp:DropDownList ID="ddlPriority" runat="server" CssClass="form-select" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlPriority" InitialValue="" ErrorMessage="Prioridad obligatoria" CssClass="text-danger small" ValidationGroup="inc" />
      </div>

      <div class="col-12">
        <label class="form-label">Problemática</label>
        <asp:TextBox ID="txtProblem" runat="server" TextMode="MultiLine" Rows="5" CssClass="form-control" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtProblem" ErrorMessage="Problemática obligatoria" CssClass="text-danger small" ValidationGroup="inc" />
      </div>
    </div>

    <div class="mt-4 d-flex gap-2">
      <asp:Button ID="btnCreate" runat="server" Text="Crear" CssClass="btn btn-success"
          ValidationGroup="inc" UseSubmitBehavior="false"
          OnClientClick="if(typeof(Page_ClientValidate)==='function'){ if(!Page_ClientValidate('inc')){ return false; } } this.disabled=true; this.value='Creando...';"
          OnClick="btnCreate_Click" />
      <a runat="server" href="~/Default.aspx" class="btn btn-outline-secondary">Cancelar</a>
    </div>

    <asp:Label ID="lblMsg" runat="server" CssClass="d-block mt-3"></asp:Label>
  </div>
</form>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
