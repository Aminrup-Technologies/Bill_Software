<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SessionKeepAlive1.aspx.cs" Inherits="Bill_Software.SessionKeepAlive1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <%--<meta http-equiv="refresh" content="59;http://aminruptechnologies.co.in"/>--%>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager2" runat="server"></asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
    <asp:Label ID="lblName" runat="server"></asp:Label>
             <asp:Timer ID="Timer1" runat="server" Interval="60000" OnTick="Timer1_Tick">
            </asp:Timer>
         </ContentTemplate>
         <Triggers>
 
                <asp:PostBackTrigger ControlID="Timer1"  />
                
                
            </Triggers>
   
    </asp:UpdatePanel>
    </div>
    </form>
</body>
</html>
