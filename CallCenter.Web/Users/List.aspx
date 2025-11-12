<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="List.aspx.cs" Inherits="CallCenter.Web.Users.List" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
  <meta charset="utf-8" />
  <title>Usuarios (Telefonistas)</title>
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.1/font/bootstrap-icons.css" rel="stylesheet" />



</head>
<body class="bg-light">
        <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
  <div class="container">
    <a class="navbar-brand fw-semibold text-white" 
       href="~/Default.aspx" 
       id="HyperLinkHome" 
       runat="server">📞 Call Center</a>
    
  </div>
</nav>
    
<form id="form1" runat="server" class="container py-4">
  <div class="d-flex justify-content-between align-items-center mb-3">
    <h3 class="mb-0">Usuarios · Telefonistas</h3>

       <!-- Card:boton Volver -->
    <asp:HyperLink ID="hlBack" runat="server" NavigateUrl="~/Default.aspx" CssClass="btn btn-outline-secondary">
      Volver al panel
    </asp:HyperLink>
  </div>
    <!-- Card:boton Modficar Usuarios -->
    <asp:HyperLink ID="hlModifyUser" runat="server" NavigateUrl="~/Users/Modify.aspx" CssClass="btn btn-primary ms-2">
  Modificar usuario
</asp:HyperLink>




  <asp:Label ID="lblMsg" runat="server" CssClass="d-block mb-3"></asp:Label>

  <div class="card shadow-sm">
    <div class="card-body">
    <asp:GridView ID="gvUsers" runat="server"
    CssClass="table table-striped table-hover"
    AutoGenerateColumns="False" DataKeyNames="Id"
    OnRowCommand="gvUsers_RowCommand"
    OnRowDataBound="gvUsers_RowDataBound"
    GridLines="None">
       <Columns>
  <asp:BoundField DataField="Username" HeaderText="Usuario" />
  <asp:BoundField DataField="Email" HeaderText="Email" />
  <asp:BoundField DataField="CreatedAtLocal" HeaderText="Creado" DataFormatString="{0:dd/MM/yyyy HH:mm}" />

  <asp:TemplateField HeaderText="Estado">
  <ItemTemplate>
    <asp:Literal ID="litEstado" runat="server" Mode="PassThrough"></asp:Literal>
  </ItemTemplate>
</asp:TemplateField>

<asp:TemplateField HeaderText="Acción">
  <ItemTemplate>
    <asp:LinkButton ID="lnkToggle" runat="server"
        CommandName="ToggleBlock"
        CommandArgument='<%# Eval("Id", "{0}") %>'
        CssClass="btn btn-sm"
        CausesValidation="false">...</asp:LinkButton>
  </ItemTemplate>
</asp:TemplateField>


</Columns>

      </asp:GridView>
    </div>
  </div>
</form>
</body>
</html>
