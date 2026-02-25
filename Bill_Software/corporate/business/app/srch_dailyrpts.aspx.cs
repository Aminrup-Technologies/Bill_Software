using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Net.Mail;
using System.Net;

namespace Bill_Software.corporate.business.app
{
    public partial class srch_dailyrpts : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                string query = "SELECT * FROM tbl_SalesVisitReport " +
                               "WHERE CAST(VisitDate AS DATE) = CAST(GETDATE() AS DATE) " +
                               "ORDER BY VisitDate DESC";
                Buinddatagrid(query);
                DbCL.FillCombo(cmbvendor, "select Name from tbl_login order by Name");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            Binder();
        }

        private void Binder()
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                cmdstring = "select * from tbl_SalesVisitReport where Salesperson='" + cmbvendor.SelectedItem.Text.ToString() + "' order by CAST(VisitDate as date) desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                cmdstring = "select * from tbl_SalesVisitReport where VisitDate between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by CAST(VisitDate as date) desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                cmdstring = "select * from tbl_SalesVisitReport where Salesperson='" + cmbvendor.SelectedItem.Text.ToString() + "' and VisitDate between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by CAST(VisitDate as date) desc";
                Buinddatagrid(cmdstring);
            }
            btnSertch.Visible = false;
        }

        private void Buinddatagrid(string cmdstring)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmdstring, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        DataList2.DataSource = dt;
                        DataList2.DataBind();
                    }
                    else
                    {
                        DataList2.DataSource = null;
                        DataList2.DataBind();
                        lblErrorMsg.Text = "No records found.";
                        PanelError.Visible = true;
                        lblOk.Text = "";
                        PanelOK.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error loading data: " + ex.Message;
                PanelError.Visible = true;
                lblOk.Text = "";
                PanelOK.Visible = false;
            }
        }

        private void BuindCompanyId()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select User_Id from tbl_login where Name='" + cmbvendor.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclientId.Text = re["User_Id"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/srch_dailyrpts.aspx");
        }

        protected void DataList2_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string id = e.CommandArgument?.ToString();
            if (string.IsNullOrEmpty(id))
            {
                lblErrorMsg.Text = "Invalid Visit ID.";
                PanelError.Visible = true;
                return;
            }

            string remarks = ((TextBox)e.Item.FindControl("txtManagerRemarks"))?.Text.Trim() ?? "";
            string status = (e.CommandName == "Approve") ? "Approved" : (e.CommandName == "Reject") ? "Rejected" : "";
            string user = HttpContext.Current.Session["USERID"]?.ToString() ?? "Manager";

            PanelOK.Visible = false;
            PanelError.Visible = false;
            try
            {
                // 🟢 Case 1: View Comments (Existing logic)
                if (e.CommandName == "ViewComments")
                {
                    hfVisitId.Value = id;
                    BindComments(id);
                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowComments", "showCommentsPopup();", true);
                    Binder();
                    return;
                }

                // 🟢 Case 2: NEW! View Details & Map Popup
                if (e.CommandName == "ViewDetails")
                {
                    BindVisitDetails(id);
                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowDet", "document.getElementById('viewDetailsModal').style.display='block';", true);
                    Binder(); // Rebind so DataList isn't lost
                    return;
                }

                // 🟢 Case 3: Approve/Reject Flow
                if (status != "")
                {
                    string createdByCode = "";
                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                    {
                        conn.Open();
                        string query = @"
                        UPDATE tbl_SalesVisitReport 
                        SET ApprovalStatus = @Status, 
                            ManagerRemarks = @Remarks, 
                            ApprovedDate = GETDATE(), 
                            ApprovedBy = @User 
                        WHERE Id = @Id";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Status", status);
                            cmd.Parameters.AddWithValue("@Remarks", remarks);
                            cmd.Parameters.AddWithValue("@User", user);
                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.ExecuteNonQuery();
                        }

                        string fetchQuery = "SELECT CreatedByCode FROM tbl_SalesVisitReport WHERE Id = @Id";
                        using (SqlCommand cmd2 = new SqlCommand(fetchQuery, conn))
                        {
                            cmd2.Parameters.AddWithValue("@Id", id);
                            object result = cmd2.ExecuteScalar();
                            if (result != null)
                                createdByCode = result.ToString();
                        }
                    }

                    Binder();
                    SendApprovalNotification(id, status, remarks, user);
                    lblOk.Text = $"Visit ID {id} successfully marked as <b>{status}</b>.";
                    PanelOK.Visible = true;
                }
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error occurred: " + ex.Message;
                PanelError.Visible = true;
            }
        }

        // 🟢 NEW METHOD to load full details into Map Popup
        private void BindVisitDetails(string visitId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = "SELECT * FROM tbl_SalesVisitReport WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id", visitId);
                    con.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            lblDetCustomer.Text = rdr["CustomerName"].ToString();
                            lblDetDept.Text = rdr["Department"].ToString();
                            lblDetContact.Text = rdr["ContactPerson"].ToString();
                            lblDetSalesperson.Text = rdr["Salesperson"].ToString();
                            lblDetVisitType.Text = rdr["VisitType"].ToString();
                            lblDetPlanDate.Text = Convert.ToDateTime(rdr["VisitDate"]).ToString("dd-MMM-yyyy");
                            lblDetExecDate.Text = rdr["ExecutionDateTime"] != DBNull.Value ? Convert.ToDateTime(rdr["ExecutionDateTime"]).ToString("dd-MMM-yyyy hh:mm tt") : "N/A";
                            lblDetStatus.Text = rdr["Status"].ToString();

                            lblDetNotes.Text = rdr["DiscussionPoints"].ToString();

                            // Inject Google Maps logic if coordinates exist
                            string lat = rdr["Latitude"] != DBNull.Value ? rdr["Latitude"].ToString() : "";
                            string lon = rdr["Longitude"] != DBNull.Value ? rdr["Longitude"].ToString() : "";

                            if (!string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lon))
                            {
                                string mapUrl = $"https://maps.google.com/maps?q={lat},{lon}&hl=en&z=15&output=embed";
                                mapContainer.InnerHtml = $"<iframe width='100%' height='100%' frameborder='0' scrolling='no' marginheight='0' marginwidth='0' src='{mapUrl}'></iframe>";
                            }
                            else
                            {
                                mapContainer.InnerHtml = "<div style='display:flex; height:100%; align-items:center; justify-content:center;'><span style='color: #888; font-style: italic;'>Location not captured during execution.</span></div>";
                            }
                        }
                    }
                }
            }
        }

        protected void btnSendComment_Click(object sender, EventArgs e)
        {
            string visitId = hfVisitId.Value;
            string comment = txtNewComment.Text.Trim();

            if (!string.IsNullOrEmpty(comment))
            {
                string userCode = Session["USERID"].ToString();
                string role = GetUserRole(userCode, Convert.ToInt32(visitId));

                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO tbl_SalesVisitResponses 
                    (VisitId, RespondentRole, RespondentCode, ResponseText, ResponseDate) 
                VALUES 
                (@VisitId, @RespondentRole, @RespondentCode, @ResponseText, GETDATE())", con))
                {
                    cmd.Parameters.AddWithValue("@VisitId", visitId);
                    cmd.Parameters.AddWithValue("@RespondentRole", role);
                    cmd.Parameters.AddWithValue("@RespondentCode", userCode);
                    cmd.Parameters.AddWithValue("@ResponseText", comment);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            // Keep popup open and refresh data
            txtNewComment.Text = "";
            Binder();
            BindComments(visitId);
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowComments", "showCommentsPopup();", true);
        }

        private void BindComments(string visitId)
        {
            string currentUserId = Session["USERID"].ToString();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT r.RespondentCode, r.RespondentRole, r.ResponseText, r.ResponseDate, u.Name
                FROM tbl_SalesVisitResponses r
                INNER JOIN tbl_login u ON r.RespondentCode = u.User_Id
                WHERE r.VisitId = @VisitId
                ORDER BY r.ResponseDate", con))
            {
                cmd.Parameters.AddWithValue("@VisitId", visitId);
                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    var sb = new System.Text.StringBuilder();
                    while (dr.Read())
                    {
                        var sideClass = dr["RespondentRole"].ToString().Equals("Manager", StringComparison.OrdinalIgnoreCase)
                            ? "comment-right" : "comment-left";

                        sb.AppendFormat(
                            "<div class='comment {0}' style='display:block;width:100%;clear:both;box-sizing:border-box;'><b>{1}</b> ({2}): {3} <br/><small>{4}</small></div>",
                            sideClass, dr["Name"], dr["RespondentRole"], dr["ResponseText"],
                            Convert.ToDateTime(dr["ResponseDate"]).ToString("dd-MMM-yyyy hh:mm tt")
                        );
                    }
                    litComments.Text = sb.ToString();
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "scrollComments", "scrollToBottom();", true);
                }
            }
        }

        private string GetUserRole(string userId, Int32 visitId)
        {
            string role = "Salesperson";
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            string query = @"SELECT CASE WHEN ApprovedBy = @UserCode THEN 'Manager' WHEN CreatedByCode = @UserCode THEN 'Salesperson' ELSE 'Manager' END FROM tbl_SalesVisitReport WHERE Id = @VisitId";
            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserCode", userId);
                cmd.Parameters.AddWithValue("@VisitId", visitId);
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null) role = result.ToString();
            }
            return role;
        }

        private void SendApprovalNotification(string visitId, string status, string remarks, string user)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    conn.Open();
                    string visitQuery = @"SELECT SVR.Id, SVR.VisitDate, SVR.CustomerName, SVR.Department, SVR.DiscussionPoints, SVR.CreatedByCode, SVR.Salesperson, L.Email FROM tbl_SalesVisitReport SVR INNER JOIN tbl_login L ON L.User_Id = SVR.CreatedByCode WHERE SVR.Id = @Id";
                    SqlCommand cmd = new SqlCommand(visitQuery, conn);
                    cmd.Parameters.AddWithValue("@Id", visitId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string emailTo = reader["Email"].ToString();
                        string salesperson = reader["Salesperson"].ToString();
                        string visitDate = Convert.ToDateTime(reader["VisitDate"]).ToString("dd-MMM-yyyy");
                        string clientName = reader["CustomerName"].ToString();
                        string dept = reader["Department"].ToString();
                        string discussion = reader["DiscussionPoints"].ToString();

                        string body = $@"<html><body style='font-family: Arial, sans-serif;'>
                                <h3>Sales Visit Report - {status}</h3>
                                <table border='1' cellpadding='6' cellspacing='0' style='border-collapse: collapse;'>
                                    <tr><td><b>Visit ID</b></td><td>{visitId}</td></tr>
                                    <tr><td><b>Salesperson</b></td><td>{salesperson}</td></tr>
                                    <tr><td><b>Visit Date</b></td><td>{visitDate}</td></tr>
                                    <tr><td><b>Client Name</b></td><td>{clientName}</td></tr>
                                    <tr><td><b>Department</b></td><td>{dept}</td></tr>
                                    <tr><td><b>Discussion Points</b></td><td>{discussion}</td></tr>
                                    <tr><td><b>Status</b></td><td>{status}</td></tr>
                                    <tr><td><b>Manager Remarks</b></td><td>{remarks}</td></tr>
                                    <tr><td><b>Approved/Rejected On</b></td><td>{DateTime.Now:dd-MMM-yyyy HH:mm tt}</td></tr>
                                    <tr><td><b>Approved/Rejected By</b></td><td>{user ?? "Manager"}</td></tr>
                                </table><br/>Regards,<br/><b>Flame-Ex ERP System</b></body></html>";

                        if (string.IsNullOrWhiteSpace(emailTo) || !System.Text.RegularExpressions.Regex.IsMatch(emailTo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                            return;

                        using (MailMessage mail = new MailMessage())
                        {
                            mail.From = new MailAddress("it.support@aminruptechnologies.co.in", "Flame-Ex : Sales Reporting Mailer | Aminrup Technologies");
                            mail.To.Add(emailTo);
                            mail.Subject = $"Sales Visit Report - {status}";
                            mail.Body = body;
                            mail.IsBodyHtml = true;

                            using (SmtpClient smtp = new SmtpClient("smtp.zoho.in", 587))
                            {
                                smtp.Credentials = new NetworkCredential("it.support@aminruptechnologies.co.in", "TPw800QrVMU2");
                                smtp.EnableSsl = true;
                                smtp.Send(mail);
                            }
                        }
                    }
                }
            }
            catch (Exception) { /* Handle Silently */ }
        }

        protected void DataList2_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var lblStatus = (Label)e.Item.FindControl("lblApprovalStatus");
                var btnApprove = (Button)e.Item.FindControl("btnApprove");
                var btnReject = (Button)e.Item.FindControl("btnReject");

                if (lblStatus != null && lblStatus.Text != "Pending")
                {
                    if (btnApprove != null) btnApprove.Enabled = false;
                    if (btnReject != null) btnReject.Enabled = false;
                }
            }
        }
    }
}