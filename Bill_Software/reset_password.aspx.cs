using System;
using Bill_Software.corporate.business.app;

namespace Bill_Software
{
    public partial class reset_password : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string uid = Request.QueryString["uid"];
                string token = Request.QueryString["token"];
                if (!string.IsNullOrWhiteSpace(uid))
                    txtUserId.Text = uid.Trim();
                if (!string.IsNullOrWhiteSpace(token))
                    txtToken.Text = token.Trim();
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            PanelError.Visible = false;
            PanelOk.Visible = false;

            string userId = txtUserId.Text.Trim();
            string token = txtToken.Text.Trim();

            string newPassword = txtNewPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                ShowError("User ID and reset token are required.");
                return;
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                ShowError("New password and confirmation do not match or are empty.");
                return;
            }

            try
            {
                string error;
                if (!PasswordResetService.TryCompleteReset(userId, token, newPassword, out error))
                {
                    ShowError(string.IsNullOrEmpty(error) ? "Invalid or expired reset token." : error);
                    return;
                }

                pnlResetForm.Visible = false;
                PanelOk.Visible = true;
                lblOkMsg.Text = "Password updated. You can now log in with your new password.";
            }
            catch (Exception)
            {
                ShowError("An error occurred while processing your request. Please try again.");
            }
        }

        private void ShowError(string message)
        {
            PanelError.Visible = true;
            lblErrorMsg.Text = message;
        }
    }
}
