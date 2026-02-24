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
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                    </div>

                    <div class="remember-me">
                        <asp:CheckBox ID="chkRememberMe" runat="server" />
                        <label style="margin: 0; cursor: pointer;">Remember Me</label>
                    </div>

                    <asp:Button ID="btnLogin" runat="server" CssClass="btn-submit" Text="Login" OnClick="btnLogin_Click" />
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
