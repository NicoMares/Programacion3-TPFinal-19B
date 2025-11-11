<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="List.aspx.cs" Inherits="CallCenter.Web.Incidents.List" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
  <meta charset="utf-8" />
  <title>Incidencias</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
  <div class="container">
    <a class="navbar-brand fw-semibold text-white" 
       href="~/Default.aspx" 
       id="HyperLinkHome" 
       runat="server">📞 Call Center</a>
    
  </div>
</nav>
  <div class="container py-4">
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h3 class="mb-0">Incidencias</h3>
      <a runat="server" href="~/Default.aspx" class="btn btn-outline-secondary">Volver</a>
    </div>

    <div class="card mb-3">
      <div class="card-body">
        <div class="row g-3">
          <div class="col-md-4">
            <label class="form-label">Buscar</label>
            <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control"
              placeholder="Cliente, problema o ID..." />
          </div>
          <div class="col-md-3">
            <label class="form-label">Estado</label>
            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
              <asp:ListItem Value="">(Todos)</asp:ListItem>
              <asp:ListItem>Abierto</asp:ListItem>
              <asp:ListItem>Asignado</asp:ListItem>
              <asp:ListItem>En Análisis</asp:ListItem>
              <asp:ListItem>Resuelto</asp:ListItem>
              <asp:ListItem>Cerrado</asp:ListItem>
            </asp:DropDownList>
          </div>
          <div class="col-md-3">
            <label class="form-label">Prioridad</label>
            <asp:DropDownList ID="ddlPriority" runat="server" CssClass="form-select">
              <asp:ListItem Value="">(Todas)</asp:ListItem>
            </asp:DropDownList>
          </div>
          <div class="col-md-2">
            <label class="form-label">Tipo</label>
            <asp:DropDownList ID="ddlType" runat="server" CssClass="form-select">
              <asp:ListItem Value="">(Todos)</asp:ListItem>
            </asp:DropDownList>
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
    <asp:BoundField DataField="Cliente" HeaderText="Cliente" />
    <asp:BoundField DataField="Tipo" HeaderText="Tipo" />
    <asp:BoundField DataField="Prioridad" HeaderText="Prioridad" />
    <asp:BoundField DataField="Status" HeaderText="Estado" />
    <asp:BoundField DataField="Assignee" HeaderText="Asignado a" />
    <asp:BoundField DataField="CreatedAt" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
    <asp:HyperLinkField HeaderText="Detalle" DataNavigateUrlFields="Id"
        DataNavigateUrlFormatString="Details.aspx?id={0}" Text="Ver" />
  </Columns>
</asp:GridView>

    </div>
  </div>
</form>
</body>
</html>
