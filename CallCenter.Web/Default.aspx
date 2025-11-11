<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs"
    Inherits="CallCenter.Web._Default" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
  <meta charset="utf-8" />
  <title>Inicio | CallCenter</title>
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">
  <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
    <div class="container">
      <asp:HyperLink ID="hlHome" runat="server"
                     NavigateUrl="~/Default.aspx"
                     CssClass="navbar-brand fw-semibold text-white">
        📞 Call Center
      </asp:HyperLink>

      <div class="ms-auto">
        <asp:Button ID="btnLogout" runat="server"
                    CssClass="btn btn-outline-light btn-sm"
                    Text="Cerrar sesión"
                    OnClick="btnLogout_Click"
                    UseSubmitBehavior="false"
                    CausesValidation="false" />
      </div>
    </div>
  </nav>

  <div class="container py-5">
    <h2 class="mb-4">Panel principal</h2>
    <div class="row g-4">
      <div class="col-12 col-md-6 col-lg-4">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <h5 class="card-title">Nueva Incidencia</h5>
            <p class="card-text text-muted">Ingresar nuevo reclamo de cliente.</p>
            <asp:HyperLink ID="hlCreate" runat="server" NavigateUrl="~/Incidents/Create.aspx" CssClass="btn btn-primary">
              Crear incidencia
            </asp:HyperLink>
          </div>
        </div>
      </div>      


        <!-- Card: Ver Incidencias -->
        <div class="col-12 col-md-6 col-lg-4">
          <div class="card shadow-sm h-100">
            <div class="card-body">
              <h5 class="card-title">Listado Incidencias</h5>
              <p class="card-text text-muted">Listado completo de incidencias.</p>
              <asp:HyperLink ID="hlListInc" runat="server" NavigateUrl="~/Incidents/List.aspx" CssClass="btn btn-outline-primary">
                Ver incidencias
              </asp:HyperLink>
            </div>
          </div>
        </div>
                <!-- Card: Crear Cliente -->
<div class="col-12 col-md-6 col-lg-4">
  <div class="card shadow-sm h-100">
    <div class="card-body">
      <h5 class="card-title">Clientes</h5>
      <p class="card-text text-muted">Dar de alta un nuevo cliente.</p>
      <asp:HyperLink ID="hlNewCustomer" runat="server" NavigateUrl="~/Customers/Create.aspx" CssClass="btn btn-success">
        Crear cliente
      </asp:HyperLink>
    </div>
  </div>
</div>

<!-- Card: Ver Clientes -->
<div class="col-12 col-md-6 col-lg-4">
  <div class="card shadow-sm h-100">
    <div class="card-body">
      <h5 class="card-title">Listar Clientes</h5>
      <p class="card-text text-muted">Listado y búsqueda de clientes.</p>
      <asp:HyperLink ID="hlListCustomers" runat="server"
          NavigateUrl="~/Customers/List.aspx"
          CssClass="btn btn-outline-primary">
        Ver clientes
      </asp:HyperLink>
    </div>
  </div>
</div>
       

        <!-- Crear Usuario (solo Supervisor) -->
<div class="col-12 col-md-6 col-lg-4">
  <asp:Panel ID="pnlCreateUser" runat="server" Visible="false">
    <div class="card shadow-sm h-100">
      <div class="card-body">
        <h5 class="card-title">Crear Usuario</h5>
        <p class="card-text text-muted">Dar de alta un nuevo usuario del sistema.</p>
        <asp:HyperLink ID="hlCreateUser" runat="server"
                       NavigateUrl="~/Users/Create.aspx"
                       CssClass="btn btn-success">
          Crear usuario
        </asp:HyperLink>
      </div>
    </div>
  </asp:Panel>
</div>



        <div class="col-12 col-md-6 col-lg-4">
          <asp:Panel ID="pnlListUsers" runat="server" Visible="false">
            <div class="card shadow-sm h-100">
              <div class="card-body">
                <h5 class="card-title">Listar Usuarios</h5>
                <p class="card-text text-muted">Usuarios de tipo Telefonista. Bloquear/Desbloquear acceso.</p>
                <asp:HyperLink ID="hlListUsers" runat="server" 
                               NavigateUrl="~/Users/List.aspx" 
                               CssClass="btn btn-primary">
                  Ver Usuarios
                </asp:HyperLink>
              </div>
            </div>
          </asp:Panel>
        </div>


        

    </div>
  </div>
</form>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
</body>


</html>
