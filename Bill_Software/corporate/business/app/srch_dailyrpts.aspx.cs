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

        private void Buinddatagrid_old(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                Buinddatagrid1(cmdstring);
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "No Data Found...";

            }
            DbCL.Conn.Close();
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
                        lblErrorMsg.Text = "No records found for Current Date";
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


        private void Buinddatagrid1(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            SqlCommand cmd1 = new SqlCommand(cmdstring, DbCL.Conn);
            DataList2.DataSource = cmd1.ExecuteReader();
            DataList2.DataBind();
            DbCL.Conn.Close();

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

        //protected void DataList2_ItemCommand(object source, DataListCommandEventArgs e)
        //{
        //    string Id = e.CommandArgument.ToString();
        //    string remarks = ((TextBox)e.Item.FindControl("txtManagerRemarks"))?.Text ?? "";
        //    string status = e.CommandName == "Approve" ? "Approved" : "Rejected";

        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
        //        {
        //            conn.Open();
        //            string query = "UPDATE tbl_SalesVisitReport SET ApprovalStatus = @Status, ManagerRemarks = @Remarks, ApprovedDate = GETDATE(), ApprovedBy = @User WHERE Id = @Id";

        //            SqlCommand cmd = new SqlCommand(query, conn);
        //            cmd.Parameters.AddWithValue("@Status", status);
        //            cmd.Parameters.AddWithValue("@Remarks", remarks);
        //            cmd.Parameters.AddWithValue("@User", Session["UserName"] ?? "Manager");
        //            cmd.Parameters.AddWithValue("@Id", Id);

        //            cmd.ExecuteNonQuery();
        //            lblOk.Text = $"Visit ID {Id} marked as {status}.";
        //            PanelOK.Visible = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        lblErrorMsg.Text = "Error: " + ex.Message;
        //        PanelError.Visible = true;
        //    }

        //}

        protected void DataList2_ItemCommand_old(object source, DataListCommandEventArgs e)
        {
            string id = e.CommandArgument.ToString();
            string remarks = ((TextBox)e.Item.FindControl("txtManagerRemarks"))?.Text.Trim() ?? "";
            string status = e.CommandName == "Approve" ? "Approved" : "Rejected";
            string user = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";

            PanelOK.Visible = false;
            PanelError.Visible = false;

            try
            {
                if (e.CommandName == "ViewComments")
                {
                    // 🔹 Store Visit ID in hidden field for use in popup
                    hfVisitId.Value = id;

                    // 🔹 Bind and show comments popup
                    BindComments(id);
                    ShowCommentsPopup(); // Your JS or server-side popup trigger
                    //Binder();
                    return; // 🚪 Stop here so we don't run approve/reject logic
                }

                string createdByCode = "";
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    conn.Open();

                    string query = @"UPDATE tbl_SalesVisitReport SET ApprovalStatus = @Status, ManagerRemarks = @Remarks, ApprovedDate = GETDATE(), ApprovedBy = @User WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@Remarks", remarks);
                        cmd.Parameters.AddWithValue("@User", user);
                        cmd.Parameters.AddWithValue("@Id", id);

                        cmd.ExecuteNonQuery();
                    }

                    // 2. Get CreatedByCode for email lookup
                    string fetchQuery = "SELECT CreatedByCode FROM tbl_SalesVisitReport WHERE Id = @Id";
                    using (SqlCommand cmd2 = new SqlCommand(fetchQuery, conn))
                    {
                        cmd2.Parameters.AddWithValue("@Id", id);
                        object result = cmd2.ExecuteScalar();
                        if (result != null)
                            createdByCode = result.ToString();
                    }


                    Binder();
                }

                // ✅ Call email sender after DB update
                SendApprovalNotification(id, status, remarks, user);

                lblOk.Text = $"Visit ID {id} successfully marked as <b>{status}</b>.";
                PanelOK.Visible = true;
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error occurred: " + ex.Message;
                PanelError.Visible = true;
            }
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
            string user = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";

            PanelOK.Visible = false;
            PanelError.Visible = false;

            try
            {
                // 🟢 Case 1: View Comments only
                if (e.CommandName == "ViewComments")
                {
                    hfVisitId.Value = id;              // Store Visit ID in hidden field for popup
                    BindComments(id);                  // Load comments from DB
                    ShowCommentsPopup();               // Trigger popup
                    Binder();                          // <--- Rebind your grid so nothing is lost
                    return;                            // stop further execution
                }

                // 🟢 Case 2: Approve/Reject Flow
                if (status != "")
                {
                    string createdByCode = "";

                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                    {
                        conn.Open();

                        // 1. Update approval status
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

                        // 2. Fetch CreatedByCode for email lookup
                        string fetchQuery = "SELECT CreatedByCode FROM tbl_SalesVisitReport WHERE Id = @Id";
                        using (SqlCommand cmd2 = new SqlCommand(fetchQuery, conn))
                        {
                            cmd2.Parameters.AddWithValue("@Id", id);
                            object result = cmd2.ExecuteScalar();
                            if (result != null)
                                createdByCode = result.ToString();
                        }
                    }

                    // 3. Refresh UI
                    Binder();

                    // 4. Trigger Email Notification (body format unchanged ✅)
                    SendApprovalNotification(id, status, remarks, user);

                    // 5. Show Success Message
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

        protected void btnSendComment_Click(object sender, EventArgs e)
        {
            string visitId = hfVisitId.Value;
            string comment = txtNewComment.Text.Trim();

            if (!string.IsNullOrEmpty(comment))
            {
                string userCode = Session["USERID"].ToString();
                string role = GetUserRole(userCode, Convert.ToInt16(visitId)); // You’ll need to implement this method

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

            // 2. Get approver email & visit details
            string approverEmail;
            string htmlDetails;
            string salespersonEmail;
            string reply = comment;
            DateTime replyDate;
            string role1 = "Manager";

            GetApproverEmailAndVisitDetails(Convert.ToInt16(visitId), out approverEmail, out salespersonEmail, out htmlDetails, out replyDate);

            // 3. Send Email
            if (!string.IsNullOrEmpty(approverEmail))
            {
                try
                {
                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress("it.support@aminruptechnologies.co.in", "Flame-Ex : Sales Reporting Mailer | Aminrup Technologies");
                        mail.CC.Add(approverEmail);
                        mail.To.Add(salespersonEmail);
                        mail.Subject = "Sales Visit Response Submitted";

                        mail.Body = $@"
                            <html>
                            <body style='font-family:Arial; font-size:14px; color:#333;'>
                                <h2 style='color:#0066cc;'>Sales Visit Report – {role1} Reply</h2>

                                <div style='border:1px solid #ddd; padding:10px; margin-bottom:15px;'>
                                    <h3 style='margin-top:0;'>Visit Details</h3>
                                    {htmlDetails}
                                </div>

                                <div style='border:1px solid #0066cc; background-color:#f9f9f9; padding:10px;'>
                                    <h3 style='margin-top:0;'>{role1} Reply</h3>
                                    <p><strong>Reply Date:</strong> {replyDate:dd-MMM-yyyy HH:mm tt}</p>
                                    <p>{reply}</p>
                                </div>
                            </body>
                            </html>";

                        mail.IsBodyHtml = true;

                        using (SmtpClient smtp = new SmtpClient("smtp.zoho.in", 587))
                        {
                            smtp.Credentials = new NetworkCredential("it.support@aminruptechnologies.co.in", "TPw800QrVMU2");
                            smtp.EnableSsl = true;
                            smtp.Send(mail);
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblErrorMsg.Text = "Email sending failed: " + ex.Message;
                    PanelError.Visible = true;
                }
            }
            else
            {
                lblErrorMsg.Text = "Approver email not found. Email not sent.";
                PanelError.Visible = true;
            }

            txtNewComment.Text = "";
            Binder();
            BindComments(visitId);
            ShowCommentsPopup(); // Keep popup open
            
        }

        private void BindComments(string visitId)
        {
            string currentUserId = Session["USERID"].ToString();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT r.RespondentCode,
                       r.RespondentRole,
                       r.ResponseText,
                       r.ResponseDate,
                       u.Name
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
                        // Align message to left or right depending on sender
                        string side = dr["RespondentCode"].ToString() == currentUserId ? "right" : "left";
                        var sideClass = dr["RespondentRole"].ToString().Equals("Manager", StringComparison.OrdinalIgnoreCase)
                            ? "comment-right"
                            : "comment-left";
                        //sb.AppendFormat(
                        //    "<div class='comment {0}' style='width: 100%;'><b>{1}</b> ({2}): {3} <br/><small>{4}</small></div>",
                        //    side,
                        //    dr["Name"],                // Person's name
                        //    dr["RespondentRole"],       // Role from the record
                        //    dr["ResponseText"],
                        //    Convert.ToDateTime(dr["ResponseDate"]).ToString("dd-MMM-yyyy hh:mm tt")
                        //);

                        sb.AppendFormat(
                            "<div class='comment {0}' style='display:block;width:100%;clear:both;box-sizing:border-box;'><b>{1}</b> ({2}): {3} <br/><small>{4}</small></div>",
                            sideClass,
                            dr["Name"],
                            dr["RespondentRole"],
                            dr["ResponseText"],
                            Convert.ToDateTime(dr["ResponseDate"]).ToString("dd-MMM-yyyy hh:mm tt")
                        );

                    }

                    litComments.Text = sb.ToString();

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "scrollComments", "scrollToBottom();", true);
                }
            }
        }

        //private void GetApproverEmailAndVisitDetails(int visitId, out string approverEmail, out string htmlDetails, out DateTime replyDate)
        //{
        //    approverEmail = "";
        //    htmlDetails = "";
        //    replyDate = DateTime.Now;

        //    string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
        //    using (SqlConnection con = new SqlConnection(connStr))
        //    {
        //        string query = @"
        //        SELECT 
        //            mgr.Email AS ApproverEmail,
        //            v.VisitDate,
        //            v.Salesperson,
        //            v.CustomerName,
        //            v.Department,
        //            v.ContactPerson,
        //            v.VisitType,
        //            v.DiscussionPoints,
        //            v.FollowUpRequired,
        //            v.NextFollowUpDate,
        //            v.Status,
        //            v.ManagerRemarks,
        //            v.ApprovedDate,
        //            mgr.Name AS ApprovedByName,
        //            v.AttachmentName,
        //            v.SalespersonReplyDate
        //        FROM tbl_SalesVisitReport v
        //        INNER JOIN tbl_login mgr ON v.ApprovedBy = mgr.User_Id
        //        WHERE v.Id = @Id";

        //        SqlCommand cmd = new SqlCommand(query, con);
        //        cmd.Parameters.AddWithValue("@Id", visitId);
        //        con.Open();

        //        using (SqlDataReader rdr = cmd.ExecuteReader())
        //        {
        //            if (rdr.Read())
        //            {
        //                approverEmail = rdr["ApproverEmail"].ToString();
        //                replyDate = rdr["SalespersonReplyDate"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(rdr["SalespersonReplyDate"]);

        //                string attachmentLink = "";
        //                if (!string.IsNullOrEmpty(rdr["AttachmentName"].ToString()))
        //                {
        //                    attachmentLink = $"<a href='https://yourdomain.com/uploads/salesvisits/{rdr["AttachmentName"]}' target='_blank'>{rdr["AttachmentName"]}</a>";
        //                }
        //                else
        //                {
        //                    attachmentLink = "N/A";
        //                }

        //                htmlDetails = $@"
        //                    <table border='1' cellspacing='0' cellpadding='6' style='border-collapse:collapse; width:100%;'>
        //                        <tr><td><b>Visit Date</b></td><td>{Convert.ToDateTime(rdr["VisitDate"]):dd-MMM-yyyy}</td></tr>
        //                        <tr><td><b>Salesperson</b></td><td>{rdr["Salesperson"]}</td></tr>
        //                        <tr><td><b>Customer Name</b></td><td>{rdr["CustomerName"]}</td></tr>
        //                        <tr><td><b>Department</b></td><td>{rdr["Department"]}</td></tr>
        //                        <tr><td><b>Contact Person</b></td><td>{rdr["ContactPerson"]}</td></tr>
        //                        <tr><td><b>Visit Type</b></td><td>{rdr["VisitType"]}</td></tr>
        //                        <tr><td><b>Discussion Points</b></td><td>{rdr["DiscussionPoints"]}</td></tr>
        //                        <tr><td><b>Follow-Up Required</b></td><td>{rdr["FollowUpRequired"]}</td></tr>
        //                        <tr><td><b>Next Follow-Up Date</b></td><td>{(rdr["NextFollowUpDate"] == DBNull.Value ? "N/A" : Convert.ToDateTime(rdr["NextFollowUpDate"]).ToString("dd-MMM-yyyy"))}</td></tr>
        //                        <tr><td><b>Status</b></td><td>{rdr["Status"]}</td></tr>
        //                        <tr><td><b>Manager Remarks</b></td><td>{rdr["ManagerRemarks"]}</td></tr>
        //                        <tr><td><b>Approved Date</b></td><td>{(rdr["ApprovedDate"] == DBNull.Value ? "N/A" : Convert.ToDateTime(rdr["ApprovedDate"]).ToString("dd-MMM-yyyy HH:mm tt"))}</td></tr>
        //                        <tr><td><b>Approved By</b></td><td>{rdr["ApprovedByName"]}</td></tr>
        //                        <tr><td><b>Attachment</b></td><td>{attachmentLink}</td></tr>
        //                    </table>
        //                ";
        //            }
        //        }
        //    }
        //}

        private void GetApproverEmailAndVisitDetails_OLD(int visitId, out string approverEmail, out string salespersonEmail, out string htmlDetails, out DateTime replyDate)
        {
            approverEmail = "";
            salespersonEmail = "";
            htmlDetails = "";
            replyDate = DateTime.Now;

            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                // First: Visit details
                string query = @"
                SELECT 
                    mgr.Email AS ApproverEmail,
                    sp.Email AS SalespersonEmail,
                    v.VisitDate,
                    v.Salesperson,
                    v.CustomerName,
                    v.Department,
                    v.ContactPerson,
                    v.VisitType,
                    v.DiscussionPoints,
                    v.FollowUpRequired,
                    v.NextFollowUpDate,
                    v.Status,
                    v.ManagerRemarks,
                    v.ApprovedDate,
                    mgr.Name AS ApprovedByName,
                    v.AttachmentName,
                    v.SalespersonReplyDate
                FROM tbl_SalesVisitReport v
                INNER JOIN tbl_login mgr ON v.ApprovedBy = mgr.User_Id
                INNER JOIN tbl_login sp ON v.Salesperson = sp.Name
                WHERE v.Id = @Id";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", visitId);
                con.Open();

                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        approverEmail = rdr["ApproverEmail"].ToString();
                        salespersonEmail = rdr["SalespersonEmail"].ToString();
                        replyDate = rdr["SalespersonReplyDate"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(rdr["SalespersonReplyDate"]);

                        string attachmentLink = string.IsNullOrEmpty(rdr["AttachmentName"].ToString())
                            ? "N/A"
                            : $"<a href='https://www.exc.aagroupindia.com/Uploads/{rdr["AttachmentName"]}' target='_blank'>{rdr["AttachmentName"]}</a>";

                        htmlDetails = $@"
                    <table border='1' cellspacing='0' cellpadding='6' style='border-collapse:collapse; width:100%;'>
                        <tr><td><b>Visit Date</b></td><td>{Convert.ToDateTime(rdr["VisitDate"]):dd-MMM-yyyy}</td></tr>
                        <tr><td><b>Salesperson</b></td><td>{rdr["Salesperson"]}</td></tr>
                        <tr><td><b>Customer Name</b></td><td>{rdr["CustomerName"]}</td></tr>
                        <tr><td><b>Department</b></td><td>{rdr["Department"]}</td></tr>
                        <tr><td><b>Contact Person</b></td><td>{rdr["ContactPerson"]}</td></tr>
                        <tr><td><b>Visit Type</b></td><td>{rdr["VisitType"]}</td></tr>
                        <tr><td><b>Discussion Points</b></td><td>{rdr["DiscussionPoints"]}</td></tr>
                        <tr><td><b>Follow-Up Required</b></td><td>{rdr["FollowUpRequired"]}</td></tr>
                        <tr><td><b>Next Follow-Up Date</b></td><td>{(rdr["NextFollowUpDate"] == DBNull.Value ? "N/A" : Convert.ToDateTime(rdr["NextFollowUpDate"]).ToString("dd-MMM-yyyy"))}</td></tr>
                        <tr><td><b>Status</b></td><td>{rdr["Status"]}</td></tr>
                        <tr><td><b>Manager Remarks</b></td><td>{rdr["ManagerRemarks"]}</td></tr>
                        <tr><td><b>Approved Date</b></td><td>{(rdr["ApprovedDate"] == DBNull.Value ? "N/A" : Convert.ToDateTime(rdr["ApprovedDate"]).ToString("dd-MMM-yyyy HH:mm tt"))}</td></tr>
                        <tr><td><b>Approved By</b></td><td>{rdr["ApprovedByName"]}</td></tr>
                        <tr><td><b>Attachment</b></td><td>{attachmentLink}</td></tr>
                    </table>
                ";
                    }
                }

                // Second: Chat-style comments
                string commentQuery = @"
                SELECT r.RespondentRole, u.Name AS RespondentName, r.ResponseText, r.ResponseDate
                FROM tbl_SalesVisitResponses r
                INNER JOIN tbl_login u ON r.RespondentCode = u.User_Id
                WHERE r.VisitId = @VisitId
                ORDER BY r.ResponseDate ASC";

                using (SqlCommand cmdComments = new SqlCommand(commentQuery, con))
                {
                    cmdComments.Parameters.AddWithValue("@VisitId", visitId);

                    using (SqlDataReader rdrC = cmdComments.ExecuteReader())
                    {
                        if (rdrC.HasRows)
                        {
                            htmlDetails += "<br/><b>Comments:</b><br/>";
                            htmlDetails += "<div style='font-family:Arial; font-size:14px;'>";

                            while (rdrC.Read())
                            {
                                bool isManager = rdrC["RespondentRole"].ToString().Equals("Manager", StringComparison.OrdinalIgnoreCase);
                                string align = isManager ? "right" : "left";
                                string bgColor = isManager ? "#e1f5fe" : "#fce4ec";
                                string name = rdrC["RespondentName"].ToString();
                                string text = rdrC["ResponseText"].ToString();
                                string date = Convert.ToDateTime(rdrC["ResponseDate"]).ToString("dd-MMM-yyyy HH:mm tt");

                                htmlDetails += $@"
                                    <div style='text-align:{align}; margin:5px 0;'>
                                        <div style='display:inline-block; background-color:{bgColor}; padding:8px 12px; border-radius:10px; max-width:70%;'>
                                            <b>{name}</b> <small>({date})</small><br/>
                                            {text}
                                        </div>
                                    </div>
                                ";
                            }

                            htmlDetails += "</div>";
                        }
                    }
                }
            }
        }

        private void GetApproverEmailAndVisitDetails(int visitId, out string approverEmail, out string salespersonEmail, out string htmlDetails, out DateTime replyDate)
        {
            approverEmail = "";
            salespersonEmail = "";
            htmlDetails = "";
            replyDate = DateTime.Now;

            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                // First: Visit details
                string query = @"
                SELECT 
                    mgr.Email AS ApproverEmail,
                    sp.Email AS SalespersonEmail,
                    v.VisitDate,
                    v.Salesperson,
                    v.CustomerName,
                    v.Department,
                    v.ContactPerson,
                    v.VisitType,
                    v.DiscussionPoints,
                    v.FollowUpRequired,
                    v.NextFollowUpDate,
                    v.Status,
                    v.ManagerRemarks,
                    v.ApprovedDate,
                    mgr.Name AS ApprovedByName,
                    v.AttachmentName,
                    v.SalespersonReplyDate,
                    v.ApprovedBy
                FROM tbl_SalesVisitReport v
                LEFT JOIN tbl_login mgr ON v.ApprovedBy = mgr.User_Id
                INNER JOIN tbl_login sp ON v.Salesperson = sp.Name
                WHERE v.Id = @Id";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", visitId);
                con.Open();

                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        approverEmail = rdr["ApproverEmail"]?.ToString();
                        salespersonEmail = rdr["SalespersonEmail"]?.ToString();
                        replyDate = rdr["SalespersonReplyDate"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(rdr["SalespersonReplyDate"]);

                        // 🔹 If approver email not found (record not yet approved), fallback to current logged-in user
                        if (string.IsNullOrEmpty(approverEmail))
                        {
                            string currentUserId = HttpContext.Current.Session["USERID"]?.ToString();
                            if (!string.IsNullOrEmpty(currentUserId))
                            {
                                approverEmail = GetUserEmail(currentUserId); // helper method below
                            }
                        }

                        string attachmentLink = string.IsNullOrEmpty(rdr["AttachmentName"].ToString())
                            ? "N/A"
                            : $"<a href='https://www.exc.aagroupindia.com/Uploads/{rdr["AttachmentName"]}' target='_blank'>{rdr["AttachmentName"]}</a>";

                        htmlDetails = $@"
                                <table border='1' cellspacing='0' cellpadding='6' style='border-collapse:collapse; width:100%;'>
                                    <tr><td><b>Visit Date</b></td><td>{Convert.ToDateTime(rdr["VisitDate"]):dd-MMM-yyyy}</td></tr>
                                    <tr><td><b>Salesperson</b></td><td>{rdr["Salesperson"]}</td></tr>
                                    <tr><td><b>Customer Name</b></td><td>{rdr["CustomerName"]}</td></tr>
                                    <tr><td><b>Department</b></td><td>{rdr["Department"]}</td></tr>
                                    <tr><td><b>Contact Person</b></td><td>{rdr["ContactPerson"]}</td></tr>
                                    <tr><td><b>Visit Type</b></td><td>{rdr["VisitType"]}</td></tr>
                                    <tr><td><b>Discussion Points</b></td><td>{rdr["DiscussionPoints"]}</td></tr>
                                    <tr><td><b>Follow-Up Required</b></td><td>{rdr["FollowUpRequired"]}</td></tr>
                                    <tr><td><b>Next Follow-Up Date</b></td><td>{(rdr["NextFollowUpDate"] == DBNull.Value ? "N/A" : Convert.ToDateTime(rdr["NextFollowUpDate"]).ToString("dd-MMM-yyyy"))}</td></tr>
                                    <tr><td><b>Status</b></td><td>{rdr["Status"]}</td></tr>
                                    <tr><td><b>Manager Remarks</b></td><td>{rdr["ManagerRemarks"]}</td></tr>
                                    <tr><td><b>Approved Date</b></td><td>{(rdr["ApprovedDate"] == DBNull.Value ? "N/A" : Convert.ToDateTime(rdr["ApprovedDate"]).ToString("dd-MMM-yyyy HH:mm tt"))}</td></tr>
                                    <tr><td><b>Approved By</b></td><td>{rdr["ApprovedByName"]}</td></tr>
                                    <tr><td><b>Attachment</b></td><td>{attachmentLink}</td></tr>
                                </table>
                                ";
                    }
                }

                // Second: Chat-style comments
                string commentQuery = @"
                        SELECT r.RespondentRole, u.Name AS RespondentName, r.ResponseText, r.ResponseDate
                        FROM tbl_SalesVisitResponses r
                        INNER JOIN tbl_login u ON r.RespondentCode = u.User_Id
                        WHERE r.VisitId = @VisitId
                        ORDER BY r.ResponseDate ASC";

                using (SqlCommand cmdComments = new SqlCommand(commentQuery, con))
                {
                    cmdComments.Parameters.AddWithValue("@VisitId", visitId);

                    using (SqlDataReader rdrC = cmdComments.ExecuteReader())
                    {
                        if (rdrC.HasRows)
                        {
                            htmlDetails += "<br/><b>Comments:</b><br/>";
                            htmlDetails += "<div style='font-family:Arial; font-size:14px;'>";

                            while (rdrC.Read())
                            {
                                bool isManager = rdrC["RespondentRole"].ToString().Equals("Manager", StringComparison.OrdinalIgnoreCase);
                                string align = isManager ? "right" : "left";
                                string bgColor = isManager ? "#e1f5fe" : "#fce4ec";
                                string name = rdrC["RespondentName"].ToString();
                                string text = rdrC["ResponseText"].ToString();
                                string date = Convert.ToDateTime(rdrC["ResponseDate"]).ToString("dd-MMM-yyyy HH:mm tt");

                                htmlDetails += $@"
                            <div style='text-align:{align}; margin:5px 0;'>
                                <div style='display:inline-block; background-color:{bgColor}; padding:8px 12px; border-radius:10px; max-width:70%;'>
                                    <b>{name}</b> <small>({date})</small><br/>
                                    {text}
                                </div>
                            </div>
                        ";
                            }

                            htmlDetails += "</div>";
                        }
                    }
                }
            }
        }

        // 🔹 Helper function to get user email by User_Id
        private string GetUserEmail(string userId)
        {
            string email = "";
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = "SELECT Email FROM tbl_login WHERE User_Id = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                        email = result.ToString();
                }
            }
            return email;
        }


        private void ShowCommentsPopup()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowComments", "showCommentsPopup();", true);
        }

        private void ShowReplyPopup()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowReply", "showReplyPopup(" + hfVisitId.Value + ");", true);
        }

        private string GetUserRole(string userId, Int32 visitId)
        {
            string role = "Salesperson";
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            string query = @"
                SELECT 
                    CASE 
                        WHEN ApprovedBy = @UserCode THEN 'Manager'
                        WHEN CreatedByCode = @UserCode THEN 'Salesperson'
                        ELSE 'Manager'
                    END
                FROM tbl_SalesVisitReport
                WHERE Id = @VisitId";

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserCode", userId);
                cmd.Parameters.AddWithValue("@VisitId", visitId);
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                    role = result.ToString();
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

                    // 1️⃣ Fetch full Sales Visit details
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

                        // 2️⃣ Prepare HTML Email Body
                        string subject = $"Sales Visit Report #{visitId} - {status}";
                        string body = $@"
                            <html>
                            <body style='font-family: Arial, sans-serif;'>
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
                                </table>
                                <br/>
                                Regards,<br/>
                                <b>Flame-Ex ERP System</b>
                            </body>
                            </html>";

                        // 3️⃣ Send Email
                        //using (System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.zoho.in"))
                        //{
                        //    smtp.Credentials = new System.Net.NetworkCredential("it.support@aminruptechnologies.co.in", "TPw800QrVMU2");
                        //    smtp.Port = 587; // or 25
                        //    smtp.EnableSsl = true;

                        //    System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                        //    mail.From = new System.Net.Mail.MailAddress("it.support@aminruptechnologies.co.in", "ERP Mailer | Aminrup Technologies");
                        //    mail.To.Add(emailTo);
                        //    mail.Subject = subject;
                        //    mail.Body = body;
                        //    mail.IsBodyHtml = true;

                        //    smtp.Send(mail);
                        //}

                        // Send email
                        //using (MailMessage mail = new MailMessage())
                        //{
                        //    mail.From = new MailAddress("it.support@aminruptechnologies.co.in", "ERP Mailer | Aminrup Technologies");
                        //    mail.To.Add(emailTo);
                        //    mail.Subject = $"Sales Visit Report - {status}";
                        //    mail.Body = body;
                        //    mail.IsBodyHtml = true;

                        //    using (SmtpClient smtp = new SmtpClient("smtp.zoho.in", 587))
                        //    {
                        //        smtp.Credentials = new NetworkCredential("it.support@aminruptechnologies.co.in", "TPw800QrVMU2");
                        //        smtp.EnableSsl = true; // SSL/TLS required
                        //        smtp.Send(mail);
                        //    }
                        //}

                        try
                        {
                            // Check if emailTo is provided and valid
                            if (string.IsNullOrWhiteSpace(emailTo))
                            {
                                lblErrorMsg.Text = "Email sending failed: Recipient email address is missing.";
                                PanelError.Visible = true;
                                return;
                            }
                            else if (!System.Text.RegularExpressions.Regex.IsMatch(emailTo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                            {
                                lblErrorMsg.Text = "Email sending failed: Invalid recipient email address format.";
                                PanelError.Visible = true;
                                return;
                            }

                            using (MailMessage mail = new MailMessage())
                            {
                                mail.From = new MailAddress("it.support@aminruptechnologies.co.in", "Flame-Ex : Sales Reporting Mailer | Aminrup Technologies"
                                );
                                mail.To.Add(emailTo);
                                mail.Subject = $"Sales Visit Report - {status}";
                                mail.Body = body;
                                mail.IsBodyHtml = true;

                                using (SmtpClient smtp = new SmtpClient("smtp.zoho.in", 587))
                                {
                                    smtp.Credentials = new NetworkCredential("it.support@aminruptechnologies.co.in", "TPw800QrVMU2");
                                    smtp.EnableSsl = true; // SSL/TLS required
                                    smtp.Send(mail);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            lblErrorMsg.Text = "Email sending failed: " + ex.Message;
                            PanelError.Visible = true;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Email sending failed: " + ex.Message;
                PanelError.Visible = true;
            }
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