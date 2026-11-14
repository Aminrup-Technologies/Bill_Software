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

        private void BindNotifications()
        {
            string userId = Session["USERID"] != null ? Session["USERID"].ToString() : null;
            if (string.IsNullOrEmpty(userId))
                return;

            using (SqlConnection con =
                new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetActiveNotifications", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserId", SqlDbType.VarChar, 50).Value = userId;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    rptNotifications.DataSource = dt;
                    rptNotifications.DataBind();

                    this.Visible = dt.Rows.Count > 0;
                }
            }
        }

        protected void rptNotifications_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Dismiss")
            {
                int notificationId = Convert.ToInt32(e.CommandArgument);
                string userId = Session["USERID"] != null ? Session["USERID"].ToString() : null;
                if (string.IsNullOrEmpty(userId))
                    return;

                using (SqlConnection con =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                using (SqlCommand cmd = new SqlCommand("sp_MarkNotificationRead", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@NotificationId", SqlDbType.Int).Value = notificationId;
                    cmd.Parameters.Add("@UserId", SqlDbType.VarChar, 50).Value = userId;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                BindNotifications();
            }
        }

    }
}
