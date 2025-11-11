<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Details.aspx.cs" Inherits="CallCenter.Web.Incidents.Details" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
  <meta charset="utf-8" />
  <title>Detalle de Incidencia</title>
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.1/font/bootstrap-icons.css" rel="stylesheet" />

  <style>
    body { background-color: #f8f9fa; }
    .chat-box { max-height: 420px; overflow-y: auto; background: #fff; border: 1px solid #dee2e6; border-radius: .5rem; padding: .5rem; }
    .msg { padding: .5rem .75rem; border-radius: .5rem; margin-bottom: .5rem; word-break: break-word; }
    .me { background: #e7f1ff; }
    .other { background: #f1f3f5; }
    .meta { font-size: .8rem; color: #6c757d; }
    .right-panel { background: #fff; border: 1px solid #dee2e6; border-radius: .5rem; padding: 1rem; position: sticky; top: 1rem; }
    .ticket-card { background: #fff; border: 1px solid #dee2e6; border-radius: .5rem; padding: 1rem; margin-bottom: 1rem; }
  </style>
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
    <h3 class="mb-0">Detalle de incidencia</h3>
    <asp:HyperLink ID="hlBack" runat="server" NavigateUrl="~/Default.aspx"
                   CssClass="btn btn-outline-primary">Volver al panel</asp:HyperLink>
  </div>

  <div class="row g-4">
    <div class="col-lg-8 col-md-7">
     <!-- Detalles del Ticket -->
<div class="ticket-card shadow-sm mb-3">
  <h5 class="border-bottom pb-2 mb-3">🧾 Detalles del Ticket</h5>
  <asp:Label ID="lblHeaderInfo" runat="server"></asp:Label>
</div>

      <!-- Chat -->
      <div class="card shadow-sm mb-4">
        <div class="card-header d-flex justify-content-between align-items-center">
          <span class="fw-semibold">💬 Conversación</span>
          <asp:Button ID="btnRefresh" runat="server" Text="Actualizar"
                      CssClass="btn btn-sm btn-outline-secondary"
                      OnClick="btnRefresh_Click" UseSubmitBehavior="false" />
        </div>
        <div class="card-body">
          <div id="chat" class="chat-box mb-3">
            <asp:Repeater ID="rpMsgs" runat="server">
              <ItemTemplate>
                <div class='msg <%# (bool)Eval("IsMe") ? "me" : "other" %>'>
                  <div class="meta">
                    <strong><%# Eval("SenderName") %></strong> · <%# Eval("CreatedAtLocal","{0:dd/MM/yyyy HH:mm}") %>
                  </div>
                  <div><%# Eval("Message") %></div>
                </div>
              </ItemTemplate>
            </asp:Repeater>
          </div>
          <div class="row g-2">
            <div class="col-12">
              <asp:TextBox ID="txtMsg" runat="server" CssClass="form-control"
                           TextMode="MultiLine" Rows="3" />
            </div>
            <div class="col-12 d-flex justify-content-end">
              <asp:Button ID="btnSend" runat="server" Text="Enviar"
                          CssClass="btn btn-primary"
                          OnClick="btnSend_Click" UseSubmitBehavior="false" />
            </div>
          </div>
          <asp:Label ID="lblChatMsg" runat="server" CssClass="d-block mt-2"></asp:Label>
        </div>
      </div>

      <!-- Archivos adjuntos -->
      <div class="card shadow-sm mb-4">
        <div class="card-header"><strong>📎 Archivos adjuntos</strong></div>
        <div class="card-body">
          <asp:Repeater ID="rpFiles" runat="server">
            <HeaderTemplate><ul class="list-group list-group-flush"></HeaderTemplate>
            <ItemTemplate>
              <li class="list-group-item d-flex justify-content-between align-items-center">
                <span><i class="bi bi-file-earmark"></i> <%# Eval("FileName") %></span>
                <asp:HyperLink runat="server"
    CssClass="btn btn-sm btn-outline-primary"
    NavigateUrl='<%# ResolveUrl("~/Incidents/Download.aspx?id=" + Eval("Id")) %>'
    Text="Descargar" />

              </li>
            </ItemTemplate>
            <FooterTemplate></ul></FooterTemplate>
          </asp:Repeater>
          <asp:Label ID="lblFilesInfo" runat="server"
                     CssClass="text-muted small mt-2 d-block"></asp:Label>
        </div>
      </div>
    </div>

    <!-- ⚙️ Columna derecha: acciones -->
    <div class="col-lg-4 col-md-5">
      <div class="card shadow-sm">
        <div class="card-header"><strong>Acciones</strong></div>
        <div class="card-body">
          

          <asp:DropDownList ID="ddlAssign" runat="server" CssClass="form-select mb-2" Visible="false"></asp:DropDownList>
          <asp:Button ID="btnAssign" runat="server" Text="Reasignar"
                      CssClass="btn btn-outline-warning w-100 mb-3"
                      OnClick="btnAssign_Click" UseSubmitBehavior="true" Visible="false" />

          <!-- Resolver -->
                      <button id="btnToggleResolve" runat="server" type="button"
                    class="btn btn-outline-success w-100 mb-2"
                    onclick="toggleBox('resolveBox')">
              Resolver
            </button>
            <div id="resolveBox" class="collapse mt-2">
              <asp:TextBox ID="txtResolution" runat="server" CssClass="form-control mb-2"
                           TextMode="MultiLine" Rows="3" placeholder="Describa la resolución..."></asp:TextBox>
              <asp:Button ID="btnResolve" runat="server" Text="Confirmar resolución"
                          CssClass="btn btn-success w-100" OnClick="btnResolve_Click" />
            </div>

          <!-- Cerrar -->
                      <button id="btnToggleClose" runat="server" type="button"
                    class="btn btn-outline-danger w-100 mb-2"
                    onclick="toggleBox('closeBox')">
              Cerrar incidencia
            </button>
            <div id="closeBox" class="collapse mt-2">
              <asp:TextBox ID="txtClose" runat="server" CssClass="form-control mb-2"
                           TextMode="MultiLine" Rows="3" placeholder="Comentario final de cierre..."></asp:TextBox>
              <asp:Button ID="btnClose" runat="server" Text="Confirmar cierre"
                          CssClass="btn btn-danger w-100" OnClick="btnClose_Click" />
            </div>

          <asp:Label ID="lblActionsMsg" runat="server" CssClass="d-block mt-3"></asp:Label>
        </div>
      </div>
    </div>
  </div>
</div>


  <script>
      window.addEventListener('load', function () {
          var box = document.getElementById('chat');
          if (box) box.scrollTop = box.scrollHeight;
      });
  </script>
</form>
   <script>
       function toggleBox(id) {
           const el = document.getElementById(id);
           if (el) el.classList.toggle('show');
       }
       window.addEventListener('load', () => {
           const box = document.getElementById('chat');
           if (box) box.scrollTop = box.scrollHeight;
       });
   </script>

</body>
</html>
