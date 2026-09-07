<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="reset_password.aspx.cs" Inherits="Bill_Software.reset_password" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>FLAME-EX | Reset Password</title>
    <link rel="shortcut icon" href="corporate/business/WebImages/aagrouplogo.png" />
    <link href="corporate/WebProperty/css/style.css" rel="stylesheet" type="text/css" />
    <style>
        body { margin: 0; padding: 0; font-family: 'Segoe UI', Arial, sans-serif; background-color: #f4f7f6; }
        .login-wrapper { display: flex; justify-content: center; align-items: center; min-height: 100vh; padding: 20px; box-sizing: border-box; }
        .login-card { background: #fff; border-radius: 8px; box-shadow: 0 4px 15px rgba(0,0,0,0.1); max-width: 480px; width: 100%; overflow: hidden; }
        .form-section { padding: 30px; }
        .login-title { font: bold 22px/28px 'Arial Black', Gadget, sans-serif; color: #153e75; margin-bottom: 20px; text-transform: uppercase; text-align: center; }
        .form-group { margin-bottom: 15px; text-align: left; }
        .form-group label { display: block; font-weight: bold; color: #333; margin-bottom: 5px; }
        .form-control { width: 100%; box-sizing: border-box; padding: 10px; border: 1px solid #ccc; border-radius: 4px; }
        .btn-submit { width: 100%; padding: 12px; background: #153e75; color: #fff; border: none; border-radius: 4px; font-weight: bold; cursor: pointer; }
        .help-text { font-size: 13px; color: #666; margin-bottom: 15px; line-height: 1.4; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-wrapper">
            <div class="login-card">
                <div class="form-section">
                    <div class="login-title">Reset Password</div>

                    <asp:Panel ID="PanelError" runat="server" Visible="false" style="margin-bottom: 15px; color: #d93025;">
                        <asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
                    </asp:Panel>
                    <asp:Panel ID="PanelOk" runat="server" Visible="false" style="margin-bottom: 15px; color: #0b7a2b;">
                        <asp:Label ID="lblOkMsg" runat="server"></asp:Label>
                    </asp:Panel>

                    <asp:Panel ID="pnlResetForm" runat="server">
                        <div class="help-text">
                            Enter the User ID and one-time reset token from your email or WhatsApp, then choose a new password. The token can be used only once.
                        </div>
                        <div class="form-group">
                            <label>User ID</label>
                            <asp:TextBox ID="txtUserId" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <label>Reset token</label>
                            <asp:TextBox ID="txtToken" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <label>New password</label>
                            <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <label>Confirm new password</label>
                            <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                        </div>
                        <asp:Button ID="btnReset" runat="server" CssClass="btn-submit" Text="Set New Password" OnClick="btnReset_Click" />
                    </asp:Panel>

                    <div style="text-align: center; margin-top: 15px;">
                        <asp:HyperLink ID="lnkLogin" runat="server" NavigateUrl="~/index.aspx" Style="color: #153e75; text-decoration: none; font-size: 14px;">Back to Login</asp:HyperLink>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
