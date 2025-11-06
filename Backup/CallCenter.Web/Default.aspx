<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CallCenter.Web.Default" %>
<!DOCTYPE html>
<html>
<head runat="server"><title>Home</title></head>
<body>
<form id="form1" runat="server">
  <div>
    <h2>Bienvenido</h2>
    <asp:LoginStatus ID="LoginStatus1" runat="server" LogoutAction="Redirect" LogoutPageUrl="~/Account/Login.aspx" />
  </div>
</form>
</body>
</html>
