<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index_card.aspx.cs" Inherits="Bill_Software.index_card" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Aminrup Technologies.</title>
    <link rel="shortcut icon" href="corporate/business/WebImages/i2i_logo.png" />
    <style type="text/css">
        .auto-style1 {
            width: 100%;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <div style="width:100%; height:100px;">

    </div><!--end of Heder-->
    <div style="width:100%;">
        
        <table cellpadding="0" cellspacing="0" class="auto-style1">
            <tr>
                <td style="width:20%;">&nbsp;</td>
                <td style="width:60%;">&nbsp;</td>
                <td style="width:20%;">&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td>
                    <table class="auto-style1">
                        <tr>
                            <td style="width:50%;">&nbsp;</td>
                            <td style="width:50%;">&nbsp;</td>
                        </tr>
                        <tr>
                            <td style="text-align:center" colspan="2">
                                <asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td style="text-align:right"><strong>Username</strong></td>
                            <td>
                                <asp:TextBox ID="txtUserName" runat="server"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td style="text-align:right"><strong>Password</strong>&nbsp;</td>
                            <td>
                                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" style="text-align: center">
                                <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="LOG IN" />
                            </td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                    </table>
                </td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
        </table>
        
    </div><!--end of Footer-->

    </div>
    </form>
</body>
</html>
