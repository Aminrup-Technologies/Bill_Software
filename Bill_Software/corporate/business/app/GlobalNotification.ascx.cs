using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class GlobalNotification : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var uid = Session["USERID"];

            if (uid == null)
            {
                this.Visible = false;
                return;
            }

            if (!IsPostBack)
                BindNotifications();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            throw new Exception("GlobalNotification code-behind executed");
        }


        private void BindNotifications()
        {
            string userId = Session["USERID"]?.ToString();
            if (string.IsNullOrEmpty(userId))
                return;

            using (SqlConnection con =
                new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_GetActiveNotifications", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptNotifications.DataSource = dt;
                rptNotifications.DataBind();

                this.Visible = dt.Rows.Count > 0;
            }
        }

        protected void rptNotifications_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Dismiss")
            {
                int notificationId = Convert.ToInt32(e.CommandArgument);
                string userId = Session["USERID"]?.ToString();

                using (SqlConnection con =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_MarkNotificationRead", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NotificationId", notificationId);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                BindNotifications();
            }
        }

        protected void btnDismiss_Click(object sender, EventArgs e)
        {
            string userId = Session["USERID"]?.ToString();
            if (string.IsNullOrEmpty(userId)) return;

            // Example: mark all visible notifications as read
            // OR hide control
            this.Visible = false;
        }

    }
}