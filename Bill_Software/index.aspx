<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="Bill_Software.index" %>

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
            font-family: Arial, sans-serif;
            background-color: #f4f7f6;
        }

        .login-wrapper {
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            padding: 20px;
        }

        .login-card {
            background: #fff;
            border-radius: 8px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
            display: flex;
            flex-direction: column; /* Stack vertically on small screens */
            max-width: 800px;
            width: 100%;
            overflow: hidden;
        }

        /* Section Styling */
        .login-form-section {
            padding: 30px 20px;
        }

        .login-info-section {
            background-color: #f9f9f9;
            padding: 30px 20px;
            text-align: center;
            display: flex;
            flex-direction: column;
            justify-content: center;
        }

        /* Form Elements */
        .login-title {
            font: bold 24px/28px 'Arial Black', Gadget, sans-serif;
            color: #336899;
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
            }

        .form-control {
            width: 100%;
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 4px;
            box-sizing: border-box;
        }

        .btn-submit {
            width: 100%;
            padding: 12px;
            background-color: #336899;
            color: white;
            border: none;
            border-radius: 4px;
            font-weight: bold;
            cursor: pointer;
            transition: background 0.3s;
            margin-top: 10px;
        }

            .btn-submit:hover {
                background-color: #26517a;
            }

        .remember-me {
            display: flex;
            align-items: center;
            gap: 8px;
            margin-top: 10px;
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
                color: #336899;
            }

        /* Desktop & Tablet Layout (Screens wider than 768px) */
        @media (min-width: 768px) {
            .login-card {
                flex-direction: row; /* Put panels side-by-side */
            }

            .login-form-section {
                flex: 1;
                padding: 40px;
            }

            .login-info-section {
                flex: 1;
                padding: 40px;
            }

            .login-title {
                text-align: left; /* Align title to left on larger screens */
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
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-wrapper">
            <div class="login-card">

                <div class="login-form-section">
                    <div class="login-title">Login</div>

                    <asp:Panel ID="PanelError" runat="server" CssClass="error-panel" Visible="False">
                        <asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" />
                        &nbsp;<asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
                    </asp:Panel>

                    <div class="form-group">
                        <label>Login As</label>
                        <asp:DropDownList ID="cmbLoginAs" runat="server" CssClass="form-control">
                            <asp:ListItem Enabled="false">ADMIN</asp:ListItem>
                            <asp:ListItem Selected="True">Employee</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="form-group">
                        <label>User ID</label>
                        <asp:TextBox ID="txtUserName" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label>Password</label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                    </div>

                    <div class="remember-me">
                        <asp:CheckBox ID="chkRememberMe" runat="server" />
                        <label style="margin: 0;">Remember Me</label>
                    </div>

                    <asp:Button ID="btnLogin" runat="server" CssClass="btn-submit" Text="Login" OnClick="btnLogin_Click" />
                </div>

                <div class="login-info-section">
                    <asp:Image ID="Image4" runat="server" ImageUrl="~/corporate/business/WebImages/aagrouplogo.png" Height="102px" Width="102px" Style="margin: 0 auto 20px;" />
                    <h2 style="margin: 0 0 10px 0; color: #333;">FLAME-EX</h2>
                    <p style="color: #666; font-size: 14px;">Do not attempt to login unless you are an authorised user.</p>

                    <div style="font-size: 12px; color: #888; margin-top: 20px;">
                        IP Address: <strong>
                            <asp:Label ID="lblIP" runat="server"></asp:Label></strong><br />
                        PC Name: <strong>
                            <asp:Label ID="lblpcname" runat="server"></asp:Label></strong>
                    </div>

                    <div class="footer-text">
                        <b>&copy; <a href="https://www.aminruptechnologies.co.in/" target="_blank">Aminrup Technologies</a></b><br />
                        <asp:Image ID="Image3" runat="server" ImageUrl="~/corporate/business/WebImages/oh4y.png" Width="25px" Height="16px" Style="vertical-align: middle;" />
                        <span>
                            <asp:Label ID="lbl_crntyr" runat="server"></asp:Label></span>
                    </div>
                </div>

            </div>
        </div>
    </form>
</body>
</html>
