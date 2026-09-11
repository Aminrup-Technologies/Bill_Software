using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class SwitchUser : Page
    {
        private string ConnString
        {
            get { return ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!ImpersonationGovernance.IsSwitchUserEnabled)
            {
                AuthGuard.EnsurePage(this, false, null);
                pnlDisabled.Visible = true;
                pnlActive.Visible = false;
                return;
            }

            bool impersonating = Session[ImpersonationGovernance.SessionLinkKey] != null;
            if (!AuthGuard.EnsurePage(this, true, impersonating ? null : ImpersonationGovernance.PermissionKey))
                return;

            pnlDisabled.Visible = false;
            pnlActive.Visible = true;

            if (impersonating)
            {
                ShowStatus("Nested impersonation is rejected (INV-13). Use End impersonation on the banner.", Color.DarkRed);
                rptUsers.Visible = false;
                txtSearch.Visible = false;
                return;
            }

            if (!IsPostBack)
                BindUsers(null);
        }

        protected void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (!ImpersonationGovernance.IsSwitchUserEnabled)
                return;
            BindUsers(txtSearch.Text);
        }

        protected void btnSwitch_Click(object sender, EventArgs e)
        {
            if (!ImpersonationGovernance.IsSwitchUserEnabled)
            {
                ShowStatus("SwitchUser is disabled.", Color.DarkRed);
                return;
            }

            Button btn = sender as Button;
            int targetUserId;
            if (btn == null || !int.TryParse(btn.CommandArgument, out targetUserId) || targetUserId <= 0)
            {
                ShowStatus("Target user is required.", Color.DarkRed);
                return;
            }

            ImpersonationResult issued = ImpersonationRuntime.IssueIntent(targetUserId);
            if (issued.Status != ImpersonationStatus.Issued || string.IsNullOrEmpty(issued.Token))
            {
                ShowStatus(FormatResult(issued), Color.DarkRed);
                return;
            }

            ImpersonationResult started = ImpersonationRuntime.Start(issued.Token);
            if (started.Status != ImpersonationStatus.Started)
            {
                ShowStatus(FormatResult(started), Color.DarkRed);
                return;
            }

            Response.Redirect("~/corporate/business/app/home.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void BindUsers(string search)
        {
            int currentUserId;
            if (Session["UserDbId"] == null || !int.TryParse(Convert.ToString(Session["UserDbId"]), out currentUserId) || currentUserId <= 0)
            {
                lblNoResults.Visible = true;
                rptUsers.DataSource = null;
                rptUsers.DataBind();
                return;
            }

            string term = search == null ? string.Empty : search.Trim();
            const string sql = @"
                SELECT TOP 50 u.Id, u.User_Id, u.Name, ISNULL(r.RoleName, '') AS RoleName
                FROM dbo.tbl_login u
                INNER JOIN dbo.UserCompanyAccess a
                    ON a.UserId = u.Id AND a.CompanyID = @CompanyID AND a.IsActive = 1
                LEFT JOIN dbo.Roles r ON r.RoleId = u.RoleId AND r.CompanyID = @CompanyID
                WHERE u.CompanyID = @CompanyID
                  AND u.IsActive = 1
                  AND u.Id <> @CurrentUserId
                  AND (u.LockoutEnd IS NULL OR u.LockoutEnd < SYSUTCDATETIME())
                  AND (@Search = N'' OR u.Name LIKE @SearchLike OR u.User_Id LIKE @SearchLike)
                ORDER BY u.Name";

            try
            {
                using (var cn = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = CompanyContext.CurrentCompanyID;
                    cmd.Parameters.Add("@CurrentUserId", SqlDbType.Int).Value = currentUserId;
                    cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = term;
                    cmd.Parameters.Add("@SearchLike", SqlDbType.NVarChar, 110).Value = "%" + term + "%";
                    cn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(rdr);
                        rptUsers.Visible = true;
                        rptUsers.DataSource = dt;
                        rptUsers.DataBind();
                        lblNoResults.Visible = dt.Rows.Count == 0;
                    }
                }
            }
            catch (SqlException)
            {
                rptUsers.DataSource = null;
                rptUsers.DataBind();
                lblNoResults.Visible = true;
            }
        }

        private static string FormatResult(ImpersonationResult result)
        {
            if (result == null)
                return "Start failed.";
            if (!string.IsNullOrEmpty(result.Invariant) && !string.IsNullOrEmpty(result.Message))
                return result.Invariant + ": " + result.Message;
            if (!string.IsNullOrEmpty(result.Message))
                return result.Message;
            return result.Status.ToString();
        }

        private void ShowStatus(string message, Color color)
        {
            lblStatus.Visible = true;
            lblStatus.ForeColor = color;
            lblStatus.Text = message;
        }
    }
}
