<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="Login.aspx.cs"
    Inherits="CallCenter.Web.Account.Login"
    ResponseEncoding="utf-8" Culture="es-AR" UICulture="es-AR" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <title>Iniciar sesión | CallCenter</title>

    <!-- Bootstrap 5 -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="~/Content/site.css" rel="stylesheet" />

    <style>
        body { background: #f5f7fb; }
        .auth-card { max-width: 420px; }
        .brand { font-weight: 700; letter-spacing: .3px; }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <div class="min-vh-100 d-flex align-items-center justify-content-center">
        <div class="card shadow auth-card">
            <div class="card-body p-4">
                <div class="text-center mb-3">
                    <div class="brand fs-4">CallCenter</div>
                    <div class="text-muted">Accedé a tu cuenta</div>
                </div>

                <!-- Summary como alerta -->
                <asp:ValidationSummary runat="server" CssClass="alert alert-danger py-2" EnableClientScript="true" />

                <div class="mb-3">
                    <label for="txtLogin" class="form-label">Email</label>
                    <asp:TextBox ID="txtLogin" runat="server" CssClass="form-control" />
                    <asp:RequiredFieldValidator runat="server"
                        ControlToValidate="txtLogin"
                        ErrorMessage="Email obligatorio"
                        Display="Dynamic" CssClass="text-danger small" />
                </div>

                <div class="mb-2">
                    <label for="txtPassword" class="form-label">Contraseña</label>
                    <div class="input-group">
                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" />
                        <button type="button" class="btn btn-outline-secondary" onclick="togglePwd()">👁</button>
                    </div>
                    <asp:RequiredFieldValidator runat="server"
                        ControlToValidate="txtPassword"
                        ErrorMessage="Contraseña obligatoria"
                        Display="Dynamic" CssClass="text-danger small" />
                </div>

                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div class="form-check">
                        <asp:CheckBox ID="chkRemember" runat="server" CssClass="form-check-input" />
                        <label class="form-check-label" for="chkRemember">Recordarme</label>
                    </div>
                    <a href="<%= ResolveUrl("~/Account/Forgot.aspx") %>" class="small">¿Olvidaste tu contraseña?</a>
                </div>

                <asp:Button ID="btnLogin" runat="server" Text="Ingresar" CssClass="btn btn-primary w-100" OnClick="btnLogin_Click" />

                <asp:Label ID="lblError" runat="server" CssClass="d-block mt-3 text-danger fw-semibold"></asp:Label>

                <hr class="my-4" />
                <div class="text-center small text-muted">
                    ¿No tenés cuenta?
<div class="text-center small text-muted">
  ¿No tenés cuenta?
  <a runat="server" href="~/Account/Register.aspx">Crear cuenta</a>
</div>
                </div>
            </div>
        </div>
    </div>
</form>

<!-- Bootstrap bundle (no necesita jQuery) -->
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
<script>
    function togglePwd() {
        var tb = document.getElementById('<%= txtPassword.ClientID %>');
        tb.type = (tb.type === 'password') ? 'text' : 'password';
    }
</script>
</body>
</html>
