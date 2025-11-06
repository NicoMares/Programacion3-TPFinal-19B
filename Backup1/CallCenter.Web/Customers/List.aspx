<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="List.aspx.cs" Inherits="CallCenter.Web.Customers.List" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
  <meta charset="utf-8" />
  <title>Clientes</title>
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">
  <div class="container py-4">
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h3 class="mb-0">Clientes</h3>
      <a runat="server" href="~/Default.aspx" class="btn btn-outline-secondary">Volver</a>
    </div>

    <!-- Filtros -->
    <div class="card mb-3">
      <div class="card-body">
        <div class="row g-3">
          <div class="col-md-4">
            <label class="form-label">Nombre</label>
            <asp:TextBox ID="txtName" runat="server" CssClass="form-control" />
          </div>
          <div class="col-md-4">
            <label class="form-label">Documento</label>
            <asp:TextBox ID="txtDoc" runat="server" CssClass="form-control" />
          </div>
          <div class="col-md-4">
            <label class="form-label">Email</label>
            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" />
          </div>
        </div>
        <div class="mt-3 d-flex gap-2">
          <asp:Button ID="btnFilter" runat="server" Text="Filtrar" CssClass="btn btn-primary"
                      OnClick="btnFilter_Click" UseSubmitBehavior="false" />
          <asp:Button ID="btnClear" runat="server" Text="Limpiar" CssClass="btn btn-outline-secondary"
                      OnClick="btnClear_Click" UseSubmitBehavior="false" />
        </div>
      </div>
    </div>

    <asp:Label ID="lblInfo" runat="server" CssClass="d-block mb-2 text-muted"></asp:Label>

    <div class="table-responsive">
      <asp:GridView ID="gv" runat="server" CssClass="table table-striped table-hover"
          AutoGenerateColumns="False" AllowPaging="True" PageSize="15"
          OnPageIndexChanging="gv_PageIndexChanging">
        <Columns>
          <asp:BoundField DataField="Id" HeaderText="ID" />
          <asp:BoundField DataField="Name" HeaderText="Nombre" />
          <asp:BoundField DataField="Document" HeaderText="Documento" />
          <asp:BoundField DataField="Email" HeaderText="Email" />
          <asp:BoundField DataField="Phone" HeaderText="Teléfono" />
          <asp:BoundField DataField="Address" HeaderText="Dirección" />
         <asp:HyperLinkField HeaderText="Incidencias"
            Text="Ver incidencias"
            DataNavigateUrlFields="Id"
            DataNavigateUrlFormatString="~/Incidents/List.aspx?customerId={0}" />


        </Columns>
      </asp:GridView>
    </div>
  </div>
</form>
</body>
</html>
