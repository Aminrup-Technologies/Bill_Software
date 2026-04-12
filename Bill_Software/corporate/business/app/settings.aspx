<%@ Page Title="Settings" Language="C#" Async="true" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="settings.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .settings-container {
            max-width: 600px;
            margin: 30px auto;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: #f9f9f9;
            padding: 25px;
            border-radius: 8px;
            border: 1px solid #ddd;
        }

        .settings-header {
            border-bottom: 2px solid #333;
            margin-bottom: 20px;
            padding-bottom: 10px;
        }

        .profile-pic-container {
            text-align: center;
            margin-bottom: 20px;
        }

        .profile-pic {
            width: 120px;
            height: 120px;
            border-radius: 50%;
            object-fit: cover;
            border: 3px solid #0056b3;
            background-color: #fff;
            box-shadow: 0 4px 8px rgba(0,0,0,0.1);
        }

        .form-group {
            margin-bottom: 15px;
        }

            .form-group label {
                display: block;
                font-weight: 600;
                margin-bottom: 5px;
                color: #333;
            }

        .form-control {
            width: 100%;
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 4px;
            box-sizing: border-box;
        }

        .checkbox-group {
            margin-top: 10px;
            margin-bottom: 20px;
        }

            .checkbox-group label {
                display: inline;
                font-weight: normal;
                margin-left: 5px;
            }

        .btn-save {
            background-color: #0056b3;
            color: white;
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 16px;
            width: 100%;
        }

            .btn-save:hover {
                background-color: #004494;
            }

        .btn-verify {
            background-color: #28a745;
            color: white;
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 16px;
            width: 100%;
            margin-top: 10px;
        }

            .btn-verify:hover {
                background-color: #218838;
            }

        .alert {
            padding: 10px;
            margin-bottom: 15px;
            border-radius: 4px;
            display: none;
        }

        .alert-success {
            background-color: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
            display: block;
        }

        .alert-danger {
            background-color: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
            display: block;
        }

        .alert-warning {
            background-color: #fff3cd;
            color: #856404;
            border: 1px solid #ffeeba;
            display: block;
        }

        .note {
            font-size: 0.85em;
            color: #666;
            font-style: italic;
        }

        .otp-panel {
            background: #fff;
            padding: 20px;
            border: 2px dashed #0056b3;
            border-radius: 8px;
            margin-top: 20px;
        }

        .verified-badge {
            color: #28a745;
            font-size: 0.85em;
            margin-left: 8px;
            font-weight: bold;
            background: #e2f0e5;
            padding: 2px 6px;
            border-radius: 4px;
        }
        /* New Password Section Style */
        .password-panel {
            background: #fff;
            padding: 20px;
            border: 1px solid #ddd;
            border-radius: 8px;
            margin-bottom: 25px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.05);
        }
        /* Password Toggle Styles */
        .password-wrapper {
            position: relative;
            display: flex;
            align-items: center;
        }

            .password-wrapper .form-control {
                padding-right: 40px;
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
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script>
        // Handles toggling for both password fields dynamically
        function togglePasswordVisibility(inputId, iconId) {
            var passwordInput = document.getElementById(inputId);
            var eyeIcon = document.getElementById(iconId);

            if (passwordInput.type === 'password') {
                passwordInput.type = 'text';
                eyeIcon.innerHTML = '<path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path><line x1="1" y1="1" x2="23" y2="23"></line>';
            } else {
                passwordInput.type = 'password';
                eyeIcon.innerHTML = '<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle>';
            }
        }
    </script>
    <div class="settings-container">
        <h2 class="settings-header">Account Settings</h2>

        <asp:Label ID="lblMessage" runat="server" EnableViewState="false"></asp:Label>

        <asp:Panel ID="pnlChangePassword" runat="server" CssClass="password-panel">
            <h3 style="margin-top: 0; color: #333; font-size: 18px;">Security Settings</h3>
            <asp:Label ID="lblPasswordLockoutWarning" runat="server" CssClass="alert alert-danger" Visible="false"
                Text="You are required to change your temporary password before accessing the system."></asp:Label>

            <div class="form-group">
                <label>New Password</label>
                <div class="password-wrapper">
                    <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                    <button type="button" class="btn-toggle-password" onclick="togglePasswordVisibility('<%= txtNewPassword.ClientID %>', 'eyeIcon1')" aria-label="Show password" title="Show password">
                        <svg id="eyeIcon1" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle>
                        </svg>
                    </button>
                </div>
            </div>
            <div class="form-group">
                <label>Confirm New Password</label>
                <div class="password-wrapper">
                    <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                    <button type="button" class="btn-toggle-password" onclick="togglePasswordVisibility('<%= txtConfirmPassword.ClientID %>', 'eyeIcon2')" aria-label="Show password" title="Show password">
                        <svg id="eyeIcon2" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle>
                        </svg>
                    </button>
                </div>
                <asp:CompareValidator ID="cvPasswordMatch" runat="server"
                    ControlToValidate="txtConfirmPassword"
                    ControlToCompare="txtNewPassword"
                    Operator="Equal" Type="String"
                    ErrorMessage="⚠️ Passwords do not match!"
                    ForeColor="#d93025" Display="Dynamic"
                    Style="margin-top: 5px; font-weight: bold; font-size: 13px; display: block;">
                </asp:CompareValidator>
            </div>
            <asp:Button ID="btnUpdatePassword" runat="server" Text="Update Password" CssClass="btn-save" Style="background-color: #28a745;" OnClick="btnUpdatePassword_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlStandardProfile" runat="server">
            <asp:Label ID="lblContactLockoutWarning" runat="server" CssClass="alert alert-danger" Visible="false" Text="Security Verification: You must add and verify both your Phone Number and Email Address to access the ERP."></asp:Label>
            <div class="profile-pic-container">
                <asp:Image ID="imgProfile" runat="server" CssClass="profile-pic" AlternateText="User Profile" />
            </div>

            <div class="form-group">
                <label>Full Name</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="form-control"></asp:TextBox>
            </div>

            <div class="form-group">
                <label>
                    Phone Number 
                    <asp:Label ID="lblPhoneVerified" runat="server" CssClass="verified-badge" Visible="false">&#10004; Verified</asp:Label>
                    <span class="note" style="display: block; margin-top: 2px;">(Changes require WhatsApp OTP)</span>
                </label>
                <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control"></asp:TextBox>
            </div>

            <div class="form-group">
                <label>
                    Email Address 
                    <asp:Label ID="lblEmailVerified" runat="server" CssClass="verified-badge" Visible="false">&#10004; Verified</asp:Label>
                    <span class="note" style="display: block; margin-top: 2px;">(Changes require Email OTP)</span>
                </label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox>
            </div>

            <div class="form-group">
                <label>Update Profile Picture</label>
                <asp:FileUpload ID="fuProfilePic" runat="server" CssClass="form-control" />
            </div>

            <div class="checkbox-group">
                <asp:CheckBox ID="chkEmailAlerts" runat="server" />
                <label for="<%= chkEmailAlerts.ClientID %>">Enable Email Alerts</label>
                <br />
                <asp:CheckBox ID="chkWhatsAppAlerts" runat="server" />
                <label for="<%= chkWhatsAppAlerts.ClientID %>">Enable WhatsApp Alerts</label>
            </div>

            <div class="form-group">
                <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn-save" OnClick="btnSave_Click" />
            </div>

            <asp:Panel ID="pnlOtp" runat="server" CssClass="otp-panel" Visible="false">
                <h3 style="margin-top: 0; color: #0056b3;">Verification Required</h3>
                <p style="font-size: 0.9em; color: #555;">We have sent a 6-digit verification code to confirm your contact detail changes.</p>
                <div class="form-group">
                    <label>Enter OTP Code</label>
                    <asp:TextBox ID="txtOtp" runat="server" CssClass="form-control" MaxLength="6" autocomplete="off"></asp:TextBox>
                </div>
                <asp:Button ID="btnVerifyOtp" runat="server" Text="Verify & Apply Changes" CssClass="btn-verify" OnClick="btnVerifyOtp_Click" />
                <asp:Button ID="btnCancelOtp" runat="server" Text="Cancel" CssClass="btn-save" Style="background-color: #6c757d; margin-top: 10px;" OnClick="btnCancelOtp_Click" />
            </asp:Panel>
        </asp:Panel>

    </div>
</asp:Content>
