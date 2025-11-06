<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Details.aspx.cs" Inherits="CallCenter.Web.Incidents.Details" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
  <meta charset="utf-8" />
  <title>Detalle de Incidencia</title>
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
  <style>
    .chat-box{max-height:420px; overflow:auto; background:#fafafa; border-radius:.5rem; border:1px solid #e9ecef;}
    .msg{padding:.5rem .75rem; border-radius:.5rem; margin-bottom:.5rem;}
    .me{background:#e7f1ff;}
    .other{background:#f1f3f5;}
    .meta{font-size:.8rem; color:#6c757d;}
  </style>
</head>
<body>
<form id="form1" runat="server">
  <div class="container py-4">

    <div class="d-flex justify-content-between align-items-center mb-3">
      <h3 class="mb-0">Detalle de incidencia</h3>
      <asp:HyperLink ID="hlBack" runat="server" NavigateUrl="~/Default.aspx" CssClass="btn btn-outline-primary">
        Volver al panel
      </asp:HyperLink>
    </div>

    <asp:Label ID="lblInfo" runat="server" CssClass="d-block mb-3"></asp:Label>

    <!-- ======== CHAT ======== -->
    <div class="card shadow-sm mb-3">
      <div class="card-header d-flex justify-content-between align-items-center">
        <span class="fw-semibold">Conversación</span>
        <div class="d-flex align-items-center gap-2">
          <asp:Button ID="btnRefresh" runat="server" Text="Actualizar"
            CssClass="btn btn-sm btn-outline-secondary" OnClick="btnRefresh_Click" UseSubmitBehavior="false" />
        </div>
      </div>
      <div class="card-body">
        <div id="chat" class="chat-box mb-3">
          <asp:Repeater ID="rpMsgs" runat="server">
            <ItemTemplate>
              <div class='msg <%# (bool)Eval("IsMe") ? "me" : "other" %>'>
                <div class="meta">
                  <strong><%# Eval("SenderName") %></strong> ·
                  <%# Eval("CreatedAtLocal","{0:dd/MM/yyyy HH:mm}") %>
                </div>
                <div><%# Eval("Message") %></div>
              </div>
            </ItemTemplate>
          </asp:Repeater>
        </div>

        <div class="row g-2">
          <div class="col-12">
            <asp:TextBox ID="txtMsg" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
            <asp:RequiredFieldValidator runat="server"
              ControlToValidate="txtMsg"
              ErrorMessage="El mensaje es obligatorio"
              CssClass="text-danger small"
              ValidationGroup="chat" Display="Dynamic" EnableClientScript="true" />
          </div>
          <div class="col-12 d-flex justify-content-end">
            <asp:Button ID="btnSend" runat="server" Text="Enviar" CssClass="btn btn-primary"
              ValidationGroup="chat" CausesValidation="true"
              UseSubmitBehavior="true"
              OnClick="btnSend_Click" />
          </div>
        </div>

        <asp:Label ID="lblChatMsg" runat="server" CssClass="d-block mt-2"></asp:Label>
      </div>
    </div>

    <!-- ======== ACCIONES DE LA INCIDENCIA ======== -->
    <div class="card mt-4 shadow-sm">
      <div class="card-header fw-semibold">Acciones de la incidencia</div>
      <div class="card-body">

        <!-- Marcar En Análisis -->
        <div class="mb-3">
          <asp:Button ID="btnAnalysis" runat="server" Text="Marcar como En Análisis"
            CssClass="btn btn-outline-secondary" OnClick="btnAnalysis_Click" UseSubmitBehavior="true" />
        </div>

        <!-- Reasignar -->
        <div class="row g-2 align-items-end mb-3">
          <div class="col-md-6">
            <label class="form-label">Reasignar a</label>
            <asp:DropDownList ID="ddlAssign" runat="server" CssClass="form-select" />
          </div>
          <div class="col-md-3">
            <asp:Button ID="btnAssign" runat="server" Text="Reasignar"
              CssClass="btn btn-outline-primary w-100"
              OnClick="btnAssign_Click" UseSubmitBehavior="true" />
          </div>
        </div>

        <!-- Resolver -->
        <div class="mb-3">
          <label class="form-label">Nota de resolución (obligatoria)</label>
          <asp:TextBox ID="txtResolution" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
          <asp:RequiredFieldValidator runat="server"
            ControlToValidate="txtResolution"
            ErrorMessage="La nota de resolución es obligatoria"
            CssClass="text-danger small" ValidationGroup="resolve" Display="Dynamic" />
          <asp:Button ID="btnResolve" runat="server" Text="Resolver incidencia"
            CssClass="btn btn-success mt-2"
            ValidationGroup="resolve" CausesValidation="true"
            OnClick="btnResolve_Click" UseSubmitBehavior="true" />
        </div>

        <!-- Cerrar -->
        <div class="mb-3">
          <label class="form-label">Comentario de cierre (obligatorio)</label>
          <asp:TextBox ID="txtClose" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
          <asp:RequiredFieldValidator runat="server"
            ControlToValidate="txtClose"
            ErrorMessage="El comentario de cierre es obligatorio"
            CssClass="text-danger small" ValidationGroup="close" Display="Dynamic" />
          <asp:Button ID="btnClose" runat="server" Text="Cerrar incidencia"
            CssClass="btn btn-danger mt-2"
            ValidationGroup="close" CausesValidation="true"
            OnClick="btnClose_Click" UseSubmitBehavior="true" />
        </div>

        <asp:Label ID="lblActionsMsg" runat="server" CssClass="d-block mt-2"></asp:Label>
      </div>
    </div>

  </div>

  <!-- Auto-scroll del chat al final en cada carga -->
  <script type="text/javascript">
      window.addEventListener('load', function () {
          var box = document.getElementById('chat');
          if (box) { box.scrollTop = box.scrollHeight; }
      });
  </script>
</form>
</body>
</html>
