<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="contactno.aspx.cs" Inherits="Bill_Software.Update.contactno" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link href="/WebProperty/css/style.css"rel="stylesheet" type="text/css" />
    <title>FLAME-EX.</title>
    <link rel="shortcut icon" href="../../WebImages/i2i_logo.png" />
        <style type="text/css">
        .style1
        {
            width: 100%;
        }
        .style2
        {
            text-align: right;
        }
        .style4
        {
            color: #FF3300;
        }
        .style5
        {
            text-align: center;
        }
    </style>
    <script type="text/javascript">
        function ValidateField() {
            if (document.getElementById('<%=txtnewContactNo.ClientID%>').value == "") {
                alert("Contact No. field can't be left blank.");
                document.getElementById('<%=txtnewContactNo.ClientID%>').focus();
                return false;
            }
            var strip = document.getElementById('<%=txtnewContactNo.ClientID%>').value.replace(/[\(\)\.\-\ ]/g, '');
            if (isNaN(parseInt(strip))) {
                alert("You have entered illegal characters in Contact No. field, which is not supposed to be as a Contact No.");
                document.getElementById('<%=txtnewContactNo.ClientID%>').focus();
                return false;
            }
            if (!(strip.length == 10)) {
                alert("Contact No. must be of 10 digit.");
                document.getElementById('<%=txtnewContactNo.ClientID%>').focus();
           return false;
       }
   }
</script>
</head>
<body>
    <form id="form1" runat="server">
    <div style="width: 500px; height: 300px">
    <table cellpadding="0" cellspacing="1" class="style1">
            <tr>
                <td width="50%" class="style2" colspan="2">
                    <a href="../Setting.aspx"onclick="opener.location='../Setting.aspx';self. close();return false;">
                    <span class="style4">Close This Window <asp:Image ID="Image1" runat="server" 
                        ImageUrl="~/corporate/business/WebImages/close-window.png"
                        ToolTip="Close This Window.." /></span></a>
                    
                </td>
            </tr>
            <tr>
                <td class="style2" width="40%">
                    &nbsp;</td>
                <td class="style2" width="60%">
                    &nbsp;</td>
            </tr>
            <tr>
                <td>
                    &nbsp;</td>
                <td class="style2">
                    &nbsp;</td>
            </tr>
            <tr>
                <td class="style2">
                    Your Current Contact No. : </td>
                <td>
                    &nbsp;
                    <asp:Label ID="lblCrntContactNo" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <td class="style2">
                    Enter Your New Contact No. : </td>
                <td>
                    &nbsp;<asp:TextBox ID="txtnewContactNo" runat="server" class="textbox_style"></asp:TextBox>
                    &nbsp;</td>
            </tr>
            <tr>
                <td class="style5" colspan="2">
                    &nbsp;</td>
            </tr>
            <tr>
                <td class="style5" colspan="2">
                    <asp:Button ID="btnUpdate" class="btn_style" runat="server" Text="Update" onclientclick="return ValidateField();" onclick="btnUpdate_Click" CssClass="btn_style" />
                </td>
            </tr>
            <tr>
                <td>
                    &nbsp;</td>
                <td>
                    &nbsp;</td>
            </tr>
            <tr>
                <td>
                    &nbsp;</td>
                <td>
                    &nbsp;</td>
            </tr>
            <tr>
                <td>
                    &nbsp;</td>
                <td>
                    &nbsp;</td>
            </tr>
            </table>
    
    </div>
    </form>
</body>
</html>
