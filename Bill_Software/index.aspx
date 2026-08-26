<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="Bill_Software.index" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>FLAME-EX | Login</title>
    <link rel="shortcut icon" href="corporate/business/WebImages/aagrouplogo.png" />
    <link href="corporate/WebProperty/css/style.css" rel="stylesheet" type="text/css" />

    <style>
        /* Base styles (Mobile First Approach) */
        body {
            margin: 0;
            padding: 0;
            font-family: 'Segoe UI', Arial, sans-serif;
            background-color: #f4f7f6;
        }

        .login-wrapper {
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            padding: 20px;
            box-sizing: border-box;
        }

        /* Mobile Layout (Flex Column) */
        .login-card {
            background: #fff;
            border-radius: 8px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
            display: flex;
            flex-direction: column;
            max-width: 800px;
            width: 100%;
            overflow: hidden;
        }

        /* Section 1: Top Logo on Mobile */
        .brand-header {
            background-color: #f9f9f9;
            padding: 30px 20px 10px 20px;
            text-align: center;
            order: 1;
        }

        /* Section 2: Middle Form on Mobile */
        .form-section {
            padding: 20px 30px 30px 30px;
            order: 2;
        }

        /* Section 3: Bottom Info on Mobile */
        .extra-info {
            background-color: #f9f9f9;
            padding: 10px 20px 30px 20px;
            text-align: center;
            order: 3;
        }

        /* Form Elements */
        .login-title {
            font: bold 24px/28px 'Arial Black', Gadget, sans-serif;
            color: #153e75;
            margin-bottom: 20px;
            text-transform: uppercase;
            text-align: center;
        }

        .form-group {
            margin-bottom: 15px;
            text-align: left;
        }

            .form-group label {
                display: block;
                font-weight: bold;
                color: #333;
                margin-bottom: 5px;
                font-size: 14px;
            }

        .form-control {
            width: 100%;
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 4px;
            box-sizing: border-box;
            font-family: inherit;
        }

        .btn-submit {
            width: 100%;
            padding: 12px;
            background-color: #153e75;
            color: white;
            border: none;
            border-radius: 4px;
            font-weight: bold;
            font-size: 16px;
            cursor: pointer;
            transition: background 0.3s;
            margin-top: 10px;
        }

            .btn-submit:hover {
                background-color: #0b2447;
            }

        .remember-me {
            display: flex;
            align-items: center;
            gap: 8px;
            margin-top: 10px;
            font-size: 14px;
        }

        .error-panel {
            margin-bottom: 15px;
            padding: 10px;
            background: #ffe6e6;
            border: 1px solid #FF3300;
            border-radius: 4px;
            color: #d93025;
            font-size: 14px;
            display: flex;
            align-items: center;
        }

        .footer-text {
            font-size: 12px;
            color: #666;
            margin-top: 20px;
        }

            .footer-text a {
                text-decoration: none;
                color: #153e75;
            }

            /* Password Toggle Styles */
            .password-wrapper {
                position: relative;
                display: flex;
                align-items: center;
            }

            .password-wrapper .form-control {
                padding-right: 40px; /* Make room for the eye icon */
            }

            .btn-toggle-password {
                position: absolute;
                right: 10px;
                background: none;
                border: none;
                color: #666;
                cursor: pointer;
                font-size: 18px;
                padding: 0;
                outline: none;
            }

            .btn-toggle-password:hover {
                color: #153e75;
            }

        /* Desktop Layout (CSS Grid) */
        @media (min-width: 768px) {
            .login-card {
                display: grid;
                grid-template-columns: 1fr 1fr;
                grid-template-rows: auto 1fr;
                flex-direction: unset; /* Remove flex column behavior */
            }

            /* Put form on the left, spanning both rows */
            .form-section {
                grid-column: 1;
                grid-row: 1 / span 2;
                padding: 40px;
                display: flex;
                flex-direction: column;
                justify-content: center;
            }

            .login-title {
                text-align: left;
            }

            /* Put logo top right */
            .brand-header {
                grid-column: 2;
                grid-row: 1;
                padding-top: 50px;
                display: flex;
                flex-direction: column;
                justify-content: flex-end;
            }

            /* Put extra info bottom right */
            .extra-info {
                grid-column: 2;
                grid-row: 2;
                padding-bottom: 40px;
                display: flex;
                flex-direction: column;
                justify-content: flex-start;
            }
        }
    </style>

    <script type="text/javascript">
        var _paq = window._paq = window._paq || [];
        _paq.push(['trackPageView']);
        _paq.push(['enableLinkTracking']);
        (function () {
            var u = "//visitors.aminruptechnologies.co.in/";
            _paq.push(['setTrackerUrl', u + 'matomo.php']);
            _paq.push(['setSiteId', '1']);
            var d = document, g = d.createElement('script'), s = d.getElementsByTagName('script')[0];
            g.async = true; g.src = u + 'matomo.js'; s.parentNode.insertBefore(g, s);
        })();

        function togglePasswordVisibility() {
        // Get the ASP.NET generated client ID for the textbox
        var passwordInput = document.getElementById('<%= txtPassword.ClientID %>');
        var eyeIcon = document.getElementById('eyeIcon');
        
        if (passwordInput.type === 'password') {
            // Show password
            passwordInput.type = 'text';
            
            // Change to "Eye Off" (slashed eye) icon
            eyeIcon.innerHTML = '<path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path><line x1="1" y1="1" x2="23" y2="23"></line>';
            
        } else {
            // Hide password
            passwordInput.type = 'password';
            
            // Change back to normal Eye icon
            eyeIcon.innerHTML = '<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle>';
        }
    }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-wrapper">
            <div class="login-card">

                <div class="brand-header">
                    <asp:Image ID="Image4" runat="server" ImageUrl="~/corporate/business/WebImages/aagrouplogo.png" Height="102px" Width="102px" Style="margin: 0 auto 15px;" />
                    <h2 style="margin: 0; color: #153e75;">FLAME-EX</h2>
                </div>

                <div class="form-section">
                    <div class="login-title">ERP-LOGIN</div>

                    <asp:Panel ID="PanelError" runat="server" CssClass="error-panel" Visible="False">
                        <asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" />
                        &nbsp;<asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
                    </asp:Panel>

                    <asp:Panel ID="pnlLogin" runat="server">
                        <div class="form-group">
                            <label>Login As</label>
                            <asp:DropDownList ID="cmbLoginAs" runat="server" CssClass="form-control">
                                <asp:ListItem>ADMIN</asp:ListItem>
                                <asp:ListItem Selected="True">Employee</asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <div class="form-group">
                            <label>User ID</label>
                            <asp:TextBox ID="txtUserName" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>

                        <div class="form-group">
                            <label>Password</label>
                            <div class="password-wrapper">
                                <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                <button type="button" class="btn-toggle-password" onclick="togglePasswordVisibility()" aria-label="Show password" title="Show password">
                                    <svg id="eyeIcon" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                        <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                                        <circle cx="12" cy="12" r="3"></circle>
                                    </svg>
                                </button>
                            </div>
                        </div>

                        <div class="remember-me">
                            <asp:CheckBox ID="chkRememberMe" runat="server" />
                            <label style="margin: 0; cursor: pointer;">Remember Me</label>
                        </div>

                        <asp:Button ID="btnLogin" runat="server" CssClass="btn-submit" Text="Login" OnClick="btnLogin_Click" />

                        <div style="text-align: center; margin-top: 15px;">
                            <asp:LinkButton ID="lnkForgotPassword" runat="server" OnClick="lnkForgotPassword_Click" Style="color: #153e75; text-decoration: none; font-size: 14px;">Forgot Password?</asp:LinkButton>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlForgotPassword" runat="server" Visible="false">
                        <div class="form-group">
                            <label>Enter your User ID to reset password</label>
                            <asp:TextBox ID="txtForgotUserId" runat="server" CssClass="form-control" placeholder="User ID"></asp:TextBox>
                        </div>
    
                        <div style="font-size: 13px; color: #666; margin-bottom: 15px; line-height: 1.4;">
                            A temporary password will be sent to your registered email address. 
                            <br /><br />
                            <span style="color: #d93025; font-weight: bold;">Note:</span> If you no longer have access to your registered email, please contact your System Administrator to update your account details.
                        </div>

                        <asp:Button ID="btnSendReset" runat="server" CssClass="btn-submit" Text="Send Reset Link" OnClick="btnSendReset_Click" />
    
                        <div style="text-align: center; margin-top: 15px;">
                            <asp:LinkButton ID="lnkBackToLogin" runat="server" OnClick="lnkBackToLogin_Click" Style="color: #153e75; text-decoration: none; font-size: 14px;">Back to Login</asp:LinkButton>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlEmailVerification" runat="server" Visible="false">
                        <div class="form-group">
                            <label>Verify or Update Your Email</label>
                            <asp:TextBox ID="txtVerifyEmail" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <asp:Button ID="btnSendOTP" runat="server" CssClass="btn-submit" Text="Send OTP" OnClick="btnSendOTP_Click" />

                        <asp:Panel ID="pnlEnterOTP" runat="server" Visible="false" Style="margin-top: 15px;">
                            <div class="form-group">
                                <label>Enter OTP</label>
                                <asp:TextBox ID="txtOTP" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <asp:Button ID="btnVerifyOTP" runat="server" CssClass="btn-submit" Text="Verify OTP" OnClick="btnVerifyOTP_Click" />
                        </asp:Panel>
                    </asp:Panel>
                </div>

                <div class="extra-info">
                    <p style="color: #666; font-size: 14px; margin-top: 0;">Do not attempt to login unless you are an authorised user.</p>

                    <div style="font-size: 12px; color: #888; margin: 20px 0;">
                        IP Address: <strong>
                            <asp:Label ID="lblIP" runat="server"></asp:Label></strong><br />
                        PC Name: <strong>
                            <asp:Label ID="lblpcname" runat="server"></asp:Label></strong>
                    </div>

                    <div class="footer-text">
                        <b>&copy; <a href="https://www.aminruptechnologies.co.in/" target="_blank">Aminrup Technologies</a></b><br />
                        <asp:Image ID="Image3" runat="server" ImageUrl="~/corporate/business/WebImages/oh4y.png" Width="25px" Height="16px" Style="vertical-align: middle; margin-top: 5px;" />
                        <span>
                            <asp:Label ID="lbl_crntyr" runat="server"></asp:Label></span>
                    </div>
                </div>

            </div>
        </div>
    </form>
</body>
</html>
