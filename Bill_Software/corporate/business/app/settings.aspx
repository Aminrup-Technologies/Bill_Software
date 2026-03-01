<%@ Page Title="Settings" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="settings.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 { width: 100%; }
        .style2 { color: #FFFFFF; font-weight: bold; }
        
        /* Modern Container Styles */
        .settings-container { margin: 20px; font-family: Arial, sans-serif; }
        .setup-box { border: 1px solid #006699; background: #f4f9ff; padding: 20px; border-radius: 5px; margin-bottom: 20px; width: 60%; }
        .setup-box h3 { margin-top: 0; color: #006699; border-bottom: 1px solid #ccc; padding-bottom: 10px; }
        
        .form-row { margin-bottom: 15px; }
        .form-label { font-weight: bold; display: inline-block; width: 150px; }
        .form-control { padding: 6px; width: 220px; border: 1px solid #ccc; border-radius: 4px; }
        
        .btn-primary { background: #19658A; color: white; border: none; padding: 8px 15px; cursor: pointer; border-radius: 4px; font-weight: bold; }
        .btn-primary:hover { background: #134e6a; }
        .btn-secondary { background: #666; color: white; border: none; padding: 8px 15px; cursor: pointer; border-radius: 4px; font-weight: bold; }
        
        .alert-success { background: #d4edda; color: #155724; padding: 10px; border: 1px solid #c3e6cb; border-radius: 4px; margin-bottom: 15px; font-weight:bold; }
        .alert-danger { background: #f8d7da; color: #721c24; padding: 10px; border: 1px solid #f5c6cb; border-radius: 4px; margin-bottom: 15px; font-weight:bold; }
        
        /* Standard Settings Table */
        .std-settings td { padding: 12px; border-bottom: 1px solid #eee; }
        .std-settings .label-col { background: #eaf2ff; font-weight: bold; width: 25%; text-align: right; padding-right: 20px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="4" style="padding:8px;">&nbsp; <span class="style2">Account Settings</span> </td>
        </tr>
    </table>

    <div class="settings-container">
        
        <asp:Panel ID="PanelMsg" runat="server" Visible="false">
            <asp:Label ID="lblMsg" runat="server"></asp:Label>
        </asp:Panel>

        <asp:Panel ID="PanelVerifyEmail" runat="server" CssClass="setup-box" Visible="false">
            <h3>Step 1: Verify Your Email</h3>
            <p>For security, please verify your email address to continue setting up your account.</p>
            
            <div class="form-row">
                <span class="form-label">Email Address:</span>
                <asp:Label ID="lblVerifyEmailDisplay" runat="server" Font-Bold="true"></asp:Label>
            </div>
            
            <asp:Panel ID="PanelSendOtp" runat="server">
                <asp:Button ID="btnSendOtp" runat="server" Text="Send Verification OTP" CssClass="btn-primary" OnClick="btnSendOtp_Click" />
            </asp:Panel>

            <asp:Panel ID="PanelEnterOtp" runat="server" Visible="false" style="margin-top:15px; border-top:1px dashed #ccc; padding-top:15px;">
                <p style="color:green; font-weight:bold;">An OTP has been sent to your email! (Valid for 15 minutes)</p>
                <div class="form-row">
                    <span class="form-label">Enter 6-Digit OTP:</span>
                    <asp:TextBox ID="txtOtp" runat="server" CssClass="form-control" MaxLength="6"></asp:TextBox>
                </div>
                <asp:Button ID="btnVerifyOtp" runat="server" Text="Verify OTP" CssClass="btn-primary" OnClick="btnVerifyOtp_Click" />
                <asp:Button ID="btnResendOtp" runat="server" Text="Resend OTP" CssClass="btn-secondary" OnClick="btnSendOtp_Click" style="margin-left:10px;" />
            </asp:Panel>
        </asp:Panel>

        <asp:Panel ID="PanelChangePassword" runat="server" CssClass="setup-box" Visible="false">
            <h3>Step 2: Create a Permanent Password</h3>
            <p>You are currently using a temporary password. Please create a new, secure password.</p>
            
            <div class="form-row">
                <span class="form-label">New Password:</span>
                <asp:TextBox ID="txtNewPass" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
            </div>
            <div class="form-row">
                <span class="form-label">Confirm Password:</span>
                <asp:TextBox ID="txtConfirmPass" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
            </div>
            <asp:Button ID="btnSavePassword" runat="server" Text="Save Password & Finish Setup" CssClass="btn-primary" OnClick="btnSavePassword_Click" />
        </asp:Panel>

        <asp:Panel ID="PanelStandardSettings" runat="server" Visible="false">
            <p style="padding: 10px; color: #555;">Manage your account details below. Click the gear icon to update specific information.</p>
            
            <table class="style1 std-settings">
                <tr>
                    <td class="label-col">Name</td>
                    <td>
                        <asp:Label ID="lblName" runat="server" Font-Bold="true"></asp:Label>
                        <asp:Image ID="imgName" runat="server" Style="float: right; cursor:pointer;"
                            onclick="window.open('/corporate/business/app/Update/name.aspx','popupwindow','width=510px,height=310px,scrollbars=yes');return true"
                            ImageUrl="~/corporate/business/WebImages/settings_icon.png" ToolTip="Update Your Name" />
                    </td>
                </tr>
                <tr>
                    <td class="label-col">Password</td>
                    <td>
                        ●●●●●●●●
                        <asp:Image ID="imgPassword" runat="server" Style="float: right; cursor:pointer;"
                            onclick="window.open('/corporate/business/app/Update/password.aspx','popupwindow','width=520px,height=320px,scrollbars=yes');return true"
                            ImageUrl="~/corporate/business/WebImages/settings_icon.png" ToolTip="Update Your Password" />
                    </td>
                </tr>
                <tr>
                    <td class="label-col">Contact No.</td>
                    <td>
                        <asp:Label ID="lblContactNo" runat="server" Font-Bold="true"></asp:Label>
                        <asp:Image ID="imgContactNo" runat="server" Style="float: right; cursor:pointer;"
                            onclick="window.open('/corporate/business/app/Update/contactno.aspx','popupwindow','width=510px,height=310px,scrollbars=yes');return true"
                            ImageUrl="~/corporate/business/WebImages/settings_icon.png" ToolTip="Update Your Contact No" />
                    </td>
                </tr>
                <tr>
                    <td class="label-col">Email ID</td>
                    <td>
                        <asp:Label ID="lblEmailID" runat="server" Font-Bold="true"></asp:Label>
                        <asp:Image ID="imgEmailID" runat="server" Style="float: right; cursor:pointer;"
                            onclick="window.open('/corporate/business/app/Update/emailid.aspx','popupwindow','width=510px,height=310px,scrollbars=yes');return true"
                            ImageUrl="~/corporate/business/WebImages/settings_icon.png" ToolTip="Update Your Email ID" />
                    </td>
                </tr>
            </table>
        </asp:Panel>

    </div>
</asp:Content>

<%--<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="settings.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="4">&nbsp; <span class="style2">Settings</span> </td>
        </tr>
        <tr>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
        </tr>
        <tr>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
        </tr>
        <tr>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
        </tr>
        <tr>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
        </tr>
        <tr>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
        </tr>
        <tr>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
        </tr>
        <tr>
            <td width="25%">&nbsp;</td>
            <td width="25%" bgcolor="#94B8FF">&nbsp;Name&nbsp;</td>
            <td width="25%" bgcolor="#94B8FF">
                <asp:Image ID="imgName" runat="server" Style="float: right"
                    onclick="window.open('/corporate/business/app/Update/name.aspx','popupwindow','width=510px,height=310px,scrollbars=yes');return true"
                    ImageUrl="~/corporate/business/WebImages/settings_icon.png"
                    ToolTip="Update Your Name.." />
                &nbsp;<asp:Label ID="lblName" runat="server"></asp:Label>

            </td>
            <td width="25%">&nbsp;</td>
        </tr>
        <tr>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;Password&nbsp;</td>
            <td width="25%">
                <asp:Image ID="imgPassword" runat="server" Style="float: right"
                    onclick="window.open('/corporate/business/app/Update/password.aspx','popupwindow','width=520px,height=320px,scrollbars=yes');return true"
                    ImageUrl="~/corporate/business/WebImages/settings_icon.png"
                    ToolTip="Update Your Password.." />
                &nbsp;●●●●●●●●
            </td>
            <td width="25%">&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td bgcolor="#94B8FF">&nbsp;Contact No.&nbsp;</td>
            <td bgcolor="#94B8FF">
                <asp:Image ID="imgContactNo" runat="server" Style="float: right"
                    onclick="window.open('/corporate/business/app/Update/contactno.aspx','popupwindow','width=510px,height=310px,scrollbars=yes');return true"
                    ImageUrl="~/corporate/business/WebImages/settings_icon.png"
                    ToolTip="Update Your Contact No.." />
                &nbsp;<asp:Label ID="lblContactNo" runat="server"></asp:Label>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;Email ID&nbsp;</td>
            <td>
                <asp:Image ID="imgEmailID" runat="server" Style="float: right"
                    onclick="window.open('/corporate/business/app/Update/emailid.aspx','popupwindow','width=510px,height=310px,scrollbars=yes');return true"
                    ImageUrl="~/corporate/business/WebImages/settings_icon.png"
                    ToolTip="Update Your Email ID.." />
                &nbsp;<asp:Label ID="lblEmailID" runat="server"></asp:Label>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
    </table>
</asp:Content>--%>
