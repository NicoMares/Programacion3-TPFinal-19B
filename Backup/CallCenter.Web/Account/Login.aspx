<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="CallCenter.Web.Account.Login" %>
<!DOCTYPE html>
<html>
<head runat="server"><title>Login</title></head>
<body>
<form id="form1" runat="server">
  <div style="max-width:380px;margin:60px auto;font-family:Segoe UI">
    <h2>Ingresar</h2>
    <asp:ValidationSummary ID="vs" runat="server" ForeColor="Red" />
    <div>
      <label>Email</label><br />
      <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" Width="100%" />
      <asp:RequiredFieldValidator ControlToValidate="txtEmail" runat="server" ErrorMessage="Email requerido" ForeColor="Red" />
    </div>
    <div style="margin-top:8px">
      <label>Contraseña</label><br />
      <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" Width="100%" />
      <asp:RequiredFieldValidator ControlToValidate="txtPassword" runat="server" ErrorMessage="Contraseña requerida" ForeColor="Red" />
    </div>
    <div style="margin-top:12px">
      <asp:Button ID="btnLogin" runat="server" Text="Ingresar" OnClick="btnLogin_Click" Width="100%" />
    </div>
    <asp:Label ID="lblError" runat="server" ForeColor="Red" />
  </div>
</form>
</body>
</html>
