<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="password.aspx.cs" Inherits="Bill_Software.Update.password" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link href="/WebProperty/css/style.css" rel="stylesheet" type="text/css" />
    <title>Aminrup Technologies.</title>
    <link rel="shortcut icon" href="../../WebImages/i2i_logo.png" />
    <style type="text/css">
        .style1 {
            width: 100%;
        }

        .style2 {
            text-align: right;
        }

        .style4 {
            color: #FF3300;
        }

        .style5 {
            text-align: center;
        }

        .smallNote {
            font-size: 12px;
            color: #666;
        }
    </style>

    <script type="text/javascript">
        function ValidateField(validator, arg) {
            var textBox1 = document.getElementById('<%=txtNewPassword.ClientID %>');
            var textBox2 = document.getElementById('<%=txtConfNewPassword.ClientID %>');
            if (textBox1.value == textBox2.value)
                arg.IsValid = true;//Valid Value
            else
                arg.IsValid = false;//Invalid Value
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div style="width: 500px; height: 300px">

            <%--<table cellpadding="0" cellspacing="1" class="style1">
                <tr>
                    <td width="50%" class="style2" colspan="2">
                        <a href="../Setting.aspx" onclick="opener.location='../Setting.aspx';self. close();return false;">
                            <span class="style4">Close This Window
                                <asp:Image ID="Image1" runat="server"
                                    ImageUrl="~/corporate/business/WebImages/close-window.png"
                                    ToolTip="Close This Window.." /></span></a>

                    </td>
                </tr>
                <tr>
                    <td class="style2" width="40%">&nbsp;</td>
                    <td class="style2" width="60%">&nbsp;</td>
                </tr>
                <tr>
                    <td colspan="2">
                        <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300"
                            BorderStyle="Solid" BorderWidth="1px" Visible="False">
                            &nbsp;
                <asp:Image ID="Image2" runat="server" Height="16px"
                    ImageUrl="~/corporate/business/WebImages/Exclamation.png" Width="16px" />
                            <asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
                        </asp:Panel>
                        <asp:Panel ID="PanelOk" runat="server" BorderColor="#006600"
                            BorderStyle="Solid" BorderWidth="1px" Visible="False">
                            &nbsp;
                <asp:Image ID="Image3" runat="server" Height="16px"
                    ImageUrl="~/corporate/business/WebImages/tick-icon.png" Width="16px" />
                            <asp:Label ID="LabelOk" runat="server"></asp:Label>
                        </asp:Panel>
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td class="style2">&nbsp;</td>
                </tr>
                <tr>
                    <td class="style2">Your Current Password : </td>
                    <td>&nbsp;
                    <asp:Label ID="lblCrntPassword" runat="server">●●●●●●●●</asp:Label>
                    </td>
                </tr>
                <tr>
                    <td class="style2">Enter Your Current Password : </td>
                    <td>&nbsp;<asp:TextBox ID="txtCrntPassword" runat="server" class="textbox_style"
                        TextMode="Password"></asp:TextBox>
                        &nbsp;</td>
                </tr>
                <tr>
                    <td class="style2">Enter New Password : </td>
                    <td>&nbsp;<asp:TextBox ID="txtNewPassword" runat="server" class="textbox_style"
                        TextMode="Password"></asp:TextBox>
                        &nbsp;</td>
                </tr>
                <tr>
                    <td class="style2">Re-Enter New Password : </td>
                    <td>&nbsp;<asp:TextBox ID="txtConfNewPassword" runat="server" TextMode="Password" class="textbox_style"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td class="style5" colspan="2">
                        <asp:CustomValidator ID="CustomValidator1" runat="server"
                            ErrorMessage="Password didn't match.." ValidationGroup="grp1"
                            ClientValidationFunction=" ValidateField"></asp:CustomValidator>
                    </td>
                </tr>
                <tr>
                    <td class="style5" colspan="2">
                        <asp:Button ID="btnUpdate" class="btn_style" runat="server" Text="Update" OnClick="btnUpdate_Click" CausesValidation="true" ValidationGroup="grp1" CssClass="btn_style" />
                        <asp:Button ID="btnReset" runat="server" OnClick="btnReset_Click" Text="Reset" class="btn_style"
                            Visible="False" CssClass="btn_style" />
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
            </table>--%>

            <table cellpadding="0" cellspacing="1" class="style1">
                <tr>
                    <td width="50%" class="style2" colspan="2">
                        <a href="../Setting.aspx" onclick="opener.location='../Setting.aspx';self.close();return false;">
                            <span class="style4">Close This Window
                                <asp:Image ID="Image1" runat="server" ImageUrl="~/corporate/business/WebImages/close-window.png" ToolTip="Close This Window.." /></span></a>
                    </td>
                </tr>

                <tr>
                    <td colspan="2">
                        <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300"
                            BorderStyle="Solid" BorderWidth="1px" Visible="False">
                            &nbsp;
                            <asp:Image ID="Image2" runat="server" Height="16px"
                                ImageUrl="~/corporate/business/WebImages/Exclamation.png" Width="16px" />
                            <asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
                        </asp:Panel>

                        <asp:Panel ID="PanelOk" runat="server" BorderColor="#006600"
                            BorderStyle="Solid" BorderWidth="1px" Visible="False">
                            &nbsp;
                            <asp:Image ID="Image3" runat="server" Height="16px"
                                ImageUrl="~/corporate/business/WebImages/tick-icon.png" Width="16px" />
                            <asp:Label ID="LabelOk" runat="server"></asp:Label>
                        </asp:Panel>
                    </td>
                </tr>

                <!-- Show current password placeholder -->
                <tr>
                    <td class="style2">Your Current Password : </td>
                    <td>&nbsp;<asp:Label ID="lblCrntPassword" runat="server">●●●●●●●●</asp:Label></td>
                </tr>

                <!-- Panel: when user has NO email - ask for email first -->
                <asp:Panel ID="pnlEmailEntry" runat="server" Visible="false">
                    <tr>
                        <td class="style2">Enter Your Email : </td>
                        <td>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="textbox_style" />
                            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="Email required." Display="Dynamic" />
                        </td>
                    </tr>
                    <tr>
                        <td class="style5" colspan="2">
                            <asp:Button ID="btnSaveEmail" runat="server" Text="Save & Send OTP" OnClick="btnSaveEmail_Click" CssClass="btn_style" />
                        </td>
                    </tr>
                </asp:Panel>

                <!-- Panel: send OTP (when email present) -->
                <asp:Panel ID="pnlSendOtp" runat="server" Visible="false">
                    <tr>
                        <td class="style2">Email on file : </td>
                        <td>&nbsp;<asp:Label ID="lblUserEmail" runat="server" CssClass="style3"></asp:Label></td>
                    </tr>
                    <tr>
                        <td class="style2">Send OTP to email : </td>
                        <td>&nbsp;
                            <asp:Button ID="btnSendOtp" runat="server" Text="Send OTP" OnClick="btnSendOtp_Click" CssClass="btn_style" />
                            <span class="smallNote">&nbsp;&nbsp;OTP expires in 10 minutes.</span>
                        </td>
                    </tr>
                </asp:Panel>

                <!-- Panel: verify OTP -->
                <asp:Panel ID="pnlVerifyOtp" runat="server" Visible="false">
                    <tr>
                        <td class="style2">Enter OTP : </td>
                        <td>&nbsp;<asp:TextBox ID="txtOtp" runat="server" CssClass="textbox_style" /></td>
                    </tr>
                    <tr>
                        <td class="style5" colspan="2">
                            <asp:Button ID="btnVerifyOtp" runat="server" Text="Verify OTP" OnClick="btnVerifyOtp_Click" CssClass="btn_style" />
                        </td>
                    </tr>
                </asp:Panel>

                <!-- Panel: Change password (only after OTP verified) -->
                <asp:Panel ID="pnlChangePassword" runat="server" Visible="false">
                    <tr>
                        <td class="style2">Enter Your Current Password : </td>
                        <td>&nbsp;<asp:TextBox ID="txtCrntPassword" runat="server" class="textbox_style" TextMode="Password"></asp:TextBox>&nbsp;</td>
                    </tr>
                    <tr>
                        <td class="style2">Enter New Password : </td>
                        <td>&nbsp;<asp:TextBox ID="txtNewPassword" runat="server" class="textbox_style" TextMode="Password"></asp:TextBox>&nbsp;</td>
                    </tr>
                    <tr>
                        <td class="style2">Re-Enter New Password : </td>
                        <td>&nbsp;<asp:TextBox ID="txtConfNewPassword" runat="server" TextMode="Password" class="textbox_style"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td class="style5" colspan="2">
                            <asp:CustomValidator ID="CustomValidator1" runat="server"
                                ErrorMessage="Password didn't match.." ValidationGroup="grp1"
                                ClientValidationFunction="ValidateField"></asp:CustomValidator>
                        </td>
                    </tr>
                    <tr>
                        <td class="style5" colspan="2">
                            <asp:Button ID="btnUpdate" class="btn_style" runat="server" Text="Update" OnClick="btnUpdate_Click" CausesValidation="true" ValidationGroup="grp1" CssClass="btn_style" />
                            <asp:Button ID="btnReset" runat="server" OnClick="btnReset_Click" Text="Reset" class="btn_style" Visible="False" CssClass="btn_style" />
                        </td>
                    </tr>
                </asp:Panel>

                <tr>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
            </table>

        </div>
    </form>
</body>
</html>
