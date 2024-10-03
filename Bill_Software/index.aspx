<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="Bill_Software.index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>FLAME-EX | Login</title>
    <link rel="shortcut icon" href="corporate/business/WebImages/aagrouplogo.png" />
    <link href="corporate/WebProperty/css/style.css" rel="stylesheet" type="text/css" />
    <link href="corporate/WebProperty/css/menu.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <div class="main_div">
                <div class="header_outer">
                    <div class="top_header">
                        <table width="100%" border="0" cellspacing="0" cellpadding="0">
                            <tr>
                                <td width="3%">&nbsp;</td>
                                <td width="82%" height="112">
                                    <h2 style="font: normal 24px/36px Arial, Helvetica, sans-serif; font-weight:bolder; color: white;">
                                        <asp:Image ID="Image2" runat="server" ImageUrl="~/corporate/business/WebImages/aagrouplogo.png" Height="82px" Width="102px" />
                                        &nbsp;&nbsp;FLAME-EX</h2>

                                </td>
                                <td width="7%"></td>
                            </tr>
                        </table>

                    </div>
                    <!--end of top header-->

                    <div class="header_menu">
                        <div class="menu">
                        </div>
                    </div>
                    <!--end of header menu-->
                </div>
                <!--end of header outer-->

                <div class="body_contain_outer" style="height: 450px;">
                    <table width="90%" border="0" cellspacing="0" cellpadding="0" style="margin: 0 auto;">
                        <tr>
                            <td width="39%">
                                <div class="logindiv">

                                    <span style="background: none; font: bold 24px/28px 'Arial Black', Gadget, sans-serif; color: #336899; margin: 0 0 0 20px;">Login </span>

                                    <div class="logindiv_inner">
                                        <table width="95%" border="0" cellspacing="0" cellpadding="0">
                                            <tr>
                                                <td colspan="3" align="center"></td>
                                            </tr>
                                            <tr>
                                                <td colspan="3" align="left" valign="middle">

                                                    <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                                                        &nbsp;<asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" />
                                                        <span lang="en-us">&nbsp;</span><asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
                                                    </asp:Panel>

                                                </td>
                                            </tr>
                                            <tr>
                                                <td height="27" align="left" valign="middle">&nbsp;</td>
                                                <td align="left" valign="middle">&nbsp;</td>
                                                <td align="left" valign="middle"></td>
                                            </tr>
                                            <tr>
                                                <td height="28" align="right" valign="middle"><strong>Login As</strong></td>
                                                <td align="left" valign="middle">&nbsp;</td>
                                                <td align="left" valign="middle">
                                                    <asp:DropDownList ID="cmbLoginAs" runat="server" class="dropdown_style">
                                                        <asp:ListItem>ADMIN</asp:ListItem>
                                                        <asp:ListItem>Employee</asp:ListItem>
                                                    </asp:DropDownList>
                                                    &nbsp;</td>
                                            </tr>
                                            <tr>
                                                <td width="43%" height="40" align="right" valign="middle"><strong style="color: #333;">User<span
                                                    lang="en-us"> ID</span></strong></td>
                                                <td width="4%" align="left" valign="middle">&nbsp;</td>
                                                <td width="53%" align="left" valign="middle">
                                                    <asp:TextBox ID="txtUserName" runat="server" class="textbox_style"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle"><strong style="color: #333;">Password</strong></td>
                                                <td align="left" valign="middle">&nbsp;</td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtPassword" runat="server"
                                                        class="textbox_style" TextMode="Password"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td height="37" align="left">&nbsp;</td>
                                                <td align="left">&nbsp;</td>
                                                <td align="left">&nbsp;<asp:Button ID="btnLogin" runat="server" class="btn_style"
                                                    Text="Login" OnClick="btnLogin_Click" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="left">&nbsp;</td>
                                                <td align="left">&nbsp;</td>
                                                <td align="left">
                                                    <asp:CheckBox ID="chkRememberMe" runat="server" />
                                                    <strong>Remember Me</strong></td>
                                            </tr>
                                            <tr>
                                                <td align="left">&nbsp;</td>
                                                <td align="left">&nbsp;</td>
                                                <td align="left"></td>
                                            </tr>
                                            <tr>
                                                <td colspan="3" align="center"></td>
                                            </tr>
                                        </table>

                                    </div>
                                    <!--end of logindiv_inner-->

                                </div>

                            </td>
                            <td width="57%" height="437" style="text-align: center;">
                                <%--<asp:Image ID="Image4" runat="server" Height="93px" ImageUrl="~/corporate/business/WebImages/i2i_logo.png" Width="223px" />--%>
                                <asp:Image ID="Image4" runat="server" Height="102px" ImageUrl="~/corporate/business/WebImages/aagrouplogo.png" Width="102px" />
                                <br />
                                <br />
                                <br />
                                <p>Do not attempt to login unless you are an authorised user.</p>
                                <p>
                                    Your IP Address :<asp:Label ID="lblIP" runat="server" Font-Bold="True"></asp:Label>
                                    &nbsp;& Computer Name :<asp:Label ID="lblpcname" runat="server" Font-Bold="True"></asp:Label>
                                    &nbsp;will be recorded.
                                </p>
                                <p>
                                    &nbsp;
                                </p>
                                <p>
                                    <b><sup>© <a href="https://www.aminruptechnologies.co.in/" target="_blank" style="text-decoration: none;">Aminrup Technologies</a></sup></b>
                                    &nbsp;
   
                                    <asp:Image ID="Image3" runat="server" ImageUrl="~/corporate/business/WebImages/oh4y.png" Width="25px" Height="16px" />
                                    &nbsp;<sup>2024</sup>
                                </p>
                                <p>
                                    &nbsp;
                                </p>
                            </td>
                            <td width="4%">
                                <img src="Corporate/WebProperty/images/login_border.png" width="20" height="331" /></td>

                        </tr>
                    </table>

                </div>
                <!--end of body_contain_outer-->


                <br class="clear" />
                <div class="footer">
                    <table width="90%" border="0" cellspacing="0" cellpadding="0" style="margin: 0 auto;">
                        <tr>
                            <td width="34%" height="36">&nbsp;</td>
                            <td width="37%">&nbsp;</td>
                            <td width="29%" align="right">Resolution 1366 x 768 (Recomended).</td>
                        </tr>
                    </table>

                </div>

            </div>
            <!--end of main div-->
        </div>
    </form>
</body>
</html>
