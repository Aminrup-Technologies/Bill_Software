using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using System.Data;
using System.Text;
using System.IO;

namespace Bill_Software.corporate.business.app
{
    public partial class vw_dailyrpts : System.Web.UI.Page
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
                BindSalesVisits();

            }
        }

        private void BindSalesVisits()
        {
            string user = HttpContext.Current.Session["USERID"]?.ToString() ?? "";

            string query = @"
            SELECT Id, VisitDate, Salesperson, CustomerName, Department, ContactPerson, VisitType,
                   DiscussionPoints, FollowUpRequired, NextFollowUpDate, Status, AttachmentName, 
                   ApprovalStatus, ManagerRemarks, SalespersonReply
            FROM tbl_SalesVisitReport
            WHERE CreatedByCode = @CreatedByCode
            ORDER BY VisitDate DESC";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@CreatedByCode", user);
                con.Open();
                gvSalesVisits.DataSource = cmd.ExecuteReader();
                gvSalesVisits.DataBind();
            }
        }

        private DataTable GetVisitMessages(int visitId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            string query = @"
            SELECT RespondentRole, RespondentCode, ResponseText, ResponseDate
            FROM tbl_SalesVisitResponses
            WHERE VisitId = @VisitId
            ORDER BY ResponseDate ASC";

            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@VisitId", visitId);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        protected void ShowConversation(int visitId)
        {
            var messages = GetVisitMessages(visitId);

            StringBuilder html = new StringBuilder();
            string currentUser = HttpContext.Current.Session["USERID"]?.ToString() ?? "";

            foreach (DataRow row in messages.Rows)
            {
                bool isCurrentUser = row["RespondentCode"].ToString() == currentUser;

                html.Append($@"
                    <div style='text-align:{(isCurrentUser ? "right" : "left")}; margin:10px;'>
                        <div style='display:inline-block; padding:10px; border-radius:10px;
                                    background-color:{(isCurrentUser ? "#d1e7dd" : "#f8d7da")}; 
                                    max-width:60%;'>
                            <strong>{row["RespondentRole"]}</strong><br/>
                            {row["ResponseText"]}<br/>
                            <small style='color:gray;'>{Convert.ToDateTime(row["ResponseDate"]).ToString("dd-MMM-yyyy HH:mm")}</small>
                        </div>
                    </div>
                ");
            }

            //litConversation.Text = html.ToString();
            hfVisitId.Value = visitId.ToString();
            //pnlConversation.Style["display"] = "block"; // Show popup
        }


        private void BindSalesVisits_1()
        {
            string query = @"SELECT Id, VisitDate, Salesperson, CustomerName, Department, ContactPerson, VisitType,
                     DiscussionPoints, FollowUpRequired, NextFollowUpDate, Status, AttachmentName, ApprovalStatus, 
                     ManagerRemarks, SalespersonReply
                     FROM tbl_SalesVisitReport
                     ORDER BY VisitDate DESC";
            //SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString);
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                con.Open();
                gvSalesVisits.DataSource = cmd.ExecuteReader();
                gvSalesVisits.DataBind();
            }
        }

        protected void gvSalesVisits_RowDataBound_OLD(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                object statusObj = DataBinder.Eval(e.Row.DataItem, "ApprovalStatus");
                string approvalStatus = statusObj != null ? statusObj.ToString() : string.Empty;

                Button btnEdit = (Button)e.Row.FindControl("btnEdit");
                Button btnReply = (Button)e.Row.FindControl("btnReply");

                if (btnEdit != null)
                {
                    // Edit visible only if Pending
                    if (!string.IsNullOrEmpty(approvalStatus) &&
                        approvalStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    {
                        btnEdit.Visible = true;
                        //btnEdit.Visible = false;
                    }
                    else
                    {
                        btnEdit.Visible = false;
                    }
                }

                if (btnReply != null)
                {
                    // Reply visible only if Approved
                    if (!string.IsNullOrEmpty(approvalStatus) &&
                        approvalStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                    {
                        //btnReply.Visible = true;
                        btnReply.Visible = false;
                    }
                    else
                    {
                        btnReply.Visible = false;
                    }
                }
            }

        }

        protected void gvSalesVisits_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                object statusObj = DataBinder.Eval(e.Row.DataItem, "ApprovalStatus");
                string approvalStatus = statusObj != null ? statusObj.ToString() : string.Empty;

                int visitId = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Id"));

                Button btnEdit = (Button)e.Row.FindControl("btnEdit");
                Button btnReply = (Button)e.Row.FindControl("btnReply");

                // ✅ Check if manager has commented
                bool managerCommentExists = false;
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    string query = @"
                SELECT COUNT(*) 
                FROM tbl_SalesVisitResponses
                WHERE VisitId = @VisitId
                  AND RespondentRole = 'Manager'";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@VisitId", visitId);
                        con.Open();
                        managerCommentExists = (int)cmd.ExecuteScalar() > 0;
                    }
                }

                // 📝 Button visibility rules
                if (btnEdit != null)
                {
                    // Edit visible only if Pending AND no manager comment yet
                    btnEdit.Visible =
                        !string.IsNullOrEmpty(approvalStatus) &&
                        approvalStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase) &&
                        !managerCommentExists;
                }

                if (btnReply != null)
                {
                    // Reply visible only if Approved
                    btnReply.Visible =
                        !string.IsNullOrEmpty(approvalStatus) &&
                        approvalStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase);
                }
            }
        }


        //protected void gvSalesVisits_RowCommand(object sender, GridViewCommandEventArgs e)
        //{
        //    if (e.CommandName == "EditVisit")
        //    {
        //        string visitId = e.CommandArgument.ToString();
        //        hfEditId.Value = visitId;

        //        GetVisitDetails(Convert.ToInt16(visitId)); // your DB fetch method

        //        upModify.Update(); // refresh update panel for edit form
        //        ShowEditPopup();   // call JavaScript
        //    }
        //    else if (e.CommandName == "ReplyVisit")
        //    {
        //        int id = Convert.ToInt32(e.CommandArgument);
        //        Response.Redirect("ReplyToManager.aspx?id=" + id);
        //    }
        //    else if (e.CommandName == "ViewComments")
        //    {
        //        string visitId = e.CommandArgument.ToString();
        //        //hfVisitId.Value = visitId;

        //        BindComments(visitId);
        //        hfVisitId.Value = visitId;
        //        upComments.Update(); // Refresh the UpdatePanel content
        //        ShowCommentsPopup(); // Trigger JavaScript after update
        //        //hfVisitId.Value = visitId;
        //    }
        //}

        protected void gvSalesVisits_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (string.IsNullOrEmpty(e.CommandArgument?.ToString()))
                return; // guard against nulls

            if (e.CommandName == "EditVisit")
            {
                int visitId = Convert.ToInt32(e.CommandArgument); // safer than Int16
                hfEditId.Value = visitId.ToString();

                GetVisitDetails(visitId); // DB fetch

                upModify.Update(); // refresh update panel
                ShowEditPopup();   // call JS (ensure ScriptManager is used)
            }
            else if (e.CommandName == "ReplyVisit")
            {
                int id = Convert.ToInt32(e.CommandArgument);
                Response.Redirect("ReplyToManager.aspx?id=" + id);
            }
            else if (e.CommandName == "ViewComments")
            {
                int visitId = Convert.ToInt32(e.CommandArgument);
                hfVisitId.Value = visitId.ToString();

                BindComments(visitId.ToString()); // or int if your method accepts int
                upComments.Update();
                ShowCommentsPopup();
            }
        }


        private void ShowEditPopup()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowEdit", "showEditPopup();", true);
        }

        private void HideEditPopup()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "HideEdit", "hideEditPopup();", true);
        }


        private SalesVisit GetVisitDetails(int visitId)
        {
            SalesVisit visit = null;
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT Id, VisitDate, Salesperson, CustomerName, Department, ContactPerson, 
                         VisitType, DiscussionPoints, FollowUpRequired, NextFollowUpDate, Status, AttachmentName
                         FROM tbl_SalesVisitReport
                         WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", visitId);
                    conn.Open();

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            visit = new SalesVisit
                            {
                                Id = (int)rdr["Id"],
                                VisitDate = Convert.ToDateTime(rdr["VisitDate"]),
                                Salesperson = rdr["Salesperson"].ToString(),
                                CustomerName = rdr["CustomerName"].ToString(),
                                Department = rdr["Department"].ToString(),
                                ContactPerson = rdr["ContactPerson"].ToString(),
                                VisitType = rdr["VisitType"].ToString(),
                                DiscussionPoints = rdr["DiscussionPoints"].ToString(),
                                FollowUpRequired = rdr["FollowUpRequired"].ToString(),
                                NextFollowUpDate = rdr["NextFollowUpDate"] != DBNull.Value
                                    ? Convert.ToDateTime(rdr["NextFollowUpDate"])
                                    : (DateTime?)null,
                                Status = rdr["Status"].ToString(),
                                AttachmentName = rdr["AttachmentName"].ToString()
                            };
                        }
                    }
                }
            }

            return visit;
        }


        public class SalesVisit
        {
            public int Id { get; set; }
            public DateTime VisitDate { get; set; }
            public string Salesperson { get; set; }
            public string CustomerName { get; set; }
            public string Department { get; set; }
            public string ContactPerson { get; set; }
            public string VisitType { get; set; }
            public string DiscussionPoints { get; set; }
            public string FollowUpRequired { get; set; }
            public DateTime? NextFollowUpDate { get; set; }
            public string Status { get; set; }
            public string AttachmentName { get; set; }
        }


        private void BindComments(string visitId)
        {
            hfVisitId.Value = visitId;
            HiddenField1.Value = visitId;
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


        private void BindComments_0(string visitId)
        {
            string currentUser = Session["USERID"].ToString();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT r.RespondentCode, r.ResponseText, r.ResponseDate, l.Name
                FROM tbl_SalesVisitResponses r
                INNER JOIN tbl_login l ON r.RespondentCode = l.User_Id
                WHERE r.VisitId = @VisitId
                ORDER BY r.ResponseDate", con))
            {
                cmd.Parameters.AddWithValue("@VisitId", visitId);
                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    while (dr.Read())
                    {
                        string respondentCode = dr["RespondentCode"].ToString();
                        string side = respondentCode == currentUser ? "right" : "left";

                        // Get role for THIS respondent (not the logged-in user)
                        string respondentRole = GetUserRole(respondentCode, Convert.ToInt16(visitId));
                        string respondentName = dr["Name"].ToString();

                        sb.AppendFormat(
                            "<div class='comment {0}'><b>{1}</b> ({2}): {3} <br/><small>{4}</small></div>",
                            side,
                            respondentName,
                            respondentRole,
                            dr["ResponseText"],
                            Convert.ToDateTime(dr["ResponseDate"]).ToString("dd-MMM-yyyy hh:mm tt")
                        );
                    }
                    litComments.Text = sb.ToString();
                }
            }
        }



        private string GetUserName(string userId)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT Name FROM tbl_login WHERE User_Id = @UserId", con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                con.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : "";
            }
        }


        private void BindComments_1(string visitId)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT RespondentCode, ResponseText, ResponseDate 
                FROM tbl_SalesVisitResponses 
                WHERE VisitId = @VisitId 
                ORDER BY ResponseDate", con))
            {
                cmd.Parameters.AddWithValue("@VisitId", visitId);
                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    while (dr.Read())
                    {
                        // Compare logged-in user with RespondentCode
                        string side = dr["RespondentCode"].ToString() == Session["USERID"].ToString() ? "right" : "left";

                        sb.AppendFormat(
                            "<div class='comment {0}'><b>{1}</b>: {2} <br/><small>{3}</small></div>",
                            side,
                            dr["RespondentCode"],
                            dr["ResponseText"],
                            Convert.ToDateTime(dr["ResponseDate"]).ToString("dd-MMM-yyyy HH:mm tt")
                        );
                    }
                    litComments.Text = sb.ToString();
                }
            }
        }


        //protected void btnSendComment_Click(object sender, EventArgs e)
        //{
        //    string visitId = hfVisitId.Value;
        //    string comment = txtNewComment.Text.Trim();
        //    if (!string.IsNullOrEmpty(comment))
        //    {
        //        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
        //        using (SqlCommand cmd = new SqlCommand("INSERT INTO tbl_Comments (VisitId, CommentText, CreatedBy, CreatedDate) VALUES (@VisitId, @CommentText, @CreatedBy, GETDATE())", con))
        //        {
        //            cmd.Parameters.AddWithValue("@VisitId", visitId);
        //            cmd.Parameters.AddWithValue("@CommentText", comment);
        //            cmd.Parameters.AddWithValue("@CreatedBy", Session["USERID"].ToString());
        //            con.Open();
        //            cmd.ExecuteNonQuery();
        //        }
        //    }

        //    txtNewComment.Text = "";
        //    BindComments(visitId);
        //    ShowCommentsPopup(); // Keep popup open
        //}

        protected void btnSendComment_Click(object sender, EventArgs e)
        {
            //string visitId = hfVisitId.Value;
            string visitId = HiddenField1.Value;

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
            string role1 = "Salesperson";

            GetApproverEmailAndVisitDetails(Convert.ToInt16(visitId), out approverEmail, out salespersonEmail, out htmlDetails, out replyDate);

            // 3. Send Email
            if (!string.IsNullOrEmpty(approverEmail))
            {
                try
                {
                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress("it.support@aminruptechnologies.co.in", "Flame-Ex : Sales Reporting Mailer | Aminrup Technologies");
                        mail.To.Add(approverEmail);
                        mail.CC.Add(salespersonEmail);
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
            BindComments(visitId);
            ShowCommentsPopup(); // Keep popup open
        }

        protected void btnSendComment_Click2(object sender, EventArgs e)
        {
            int visitId = Convert.ToInt32(ViewState["CurrentVisitId"]);
            string comment = txtNewComment.Text.Trim();
            string user = Session["USERID"].ToString();

            string role = GetUserRole(user, visitId);
            AddResponse(visitId, role, user, comment);
            GetVisitMessages(visitId);

            // Keep popup visible
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowPopup", "showCommentsPopup();", true);
        }

        protected void btnSaveReply_Click_1(object sender, EventArgs e)
        {
            try
            {
                int visitId = int.Parse(hfVisitId.Value);
                string reply = txtSalespersonReply.Text.Trim();

                if (string.IsNullOrEmpty(reply))
                {
                    lblErrorMsg.Text = "Please enter your response before submitting.";
                    PanelError.Visible = true;
                    return;
                }

                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                // 1. Update DB with reply and reply date
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    string query = @"
                        UPDATE tbl_SalesVisitReport 
                        SET SalespersonReply = @Reply, 
                            SalespersonReplyDate = GETDATE()
                        WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Reply", reply);
                    cmd.Parameters.AddWithValue("@Id", visitId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // 2. Get approver email & visit details
                string approverEmail;
                string salespersonEmail;
                string htmlDetails;
                DateTime replyDate;
                GetApproverEmailAndVisitDetails(visitId, out approverEmail, out salespersonEmail, out htmlDetails, out replyDate);

                // 3. Send Email
                if (!string.IsNullOrEmpty(approverEmail))
                {
                    try
                    {
                        using (MailMessage mail = new MailMessage())
                        {
                            mail.From = new MailAddress("it.support@aminruptechnologies.co.in", "Flame-Ex : Sales Reporting Mailer | Aminrup Technologies");
                            mail.To.Add(approverEmail);
                            mail.CC.Add(salespersonEmail);
                            mail.Subject = "Sales Visit Response Submitted";

                            // Email body with two boxes (details + reply)
                            mail.Body = $@"
                                <html>
                                <body style='font-family:Arial; font-size:14px; color:#333;'>
                                    <h2 style='color:#0066cc;'>Sales Visit Report – Salesperson Reply</h2>

                                    <div style='border:1px solid #ddd; padding:10px; margin-bottom:15px;'>
                                        <h3 style='margin-top:0;'>Visit Details</h3>
                                        {htmlDetails}
                                    </div>

                                    <div style='border:1px solid #0066cc; background-color:#f9f9f9; padding:10px;'>
                                        <h3 style='margin-top:0;'>Salesperson Reply</h3>
                                        <p><strong>Reply Date:</strong> {replyDate:dd-MMM-yyyy HH:mm tt}</p>
                                        <p>{reply}</p>
                                    </div>
                                </body>
                                </html>
                            ";

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

                // 4. Hide popup & refresh grid
                txtSalespersonReply.Text = "";
                pnlReply.Style["display"] = "none";
                BindSalesVisits();
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error saving reply: " + ex.Message;
                PanelError.Visible = true;
            }
        }

        protected void btnSaveReply_Click(object sender, EventArgs e)
        {
            try
            {
                int visitId = int.Parse(hfVisitId.Value);
                string reply = txtSalespersonReply.Text.Trim();
                string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "";
                string role = "Salesperson"; // Later you can set "Manager" when it's manager's reply

                if (string.IsNullOrEmpty(reply))
                {
                    lblErrorMsg.Text = "Please enter your response before submitting.";
                    PanelError.Visible = true;
                    return;
                }

                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                // 1. Insert new reply
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    string query = @"
                    INSERT INTO tbl_SalesVisitResponses 
                        (VisitId, RespondentRole, RespondentCode, ResponseText)
                    VALUES 
                        (@VisitId, @Role, @UserCode, @Reply)";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@VisitId", visitId);
                    cmd.Parameters.AddWithValue("@Role", role);
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    cmd.Parameters.AddWithValue("@Reply", reply);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // 2. Get approver email & visit details
                string approverEmail;
                string htmlDetails;
                string salespersonEmail;
                DateTime replyDate;
                GetApproverEmailAndVisitDetails(visitId, out approverEmail, out salespersonEmail, out htmlDetails, out replyDate);

                // 3. Send Email
                if (!string.IsNullOrEmpty(approverEmail))
                {
                    try
                    {
                        using (MailMessage mail = new MailMessage())
                        {
                            mail.From = new MailAddress("it.support@aminruptechnologies.co.in", "Flame-Ex : Sales Reporting Mailer | Aminrup Technologies");
                            mail.To.Add(approverEmail);
                            mail.CC.Add(salespersonEmail);
                            mail.Subject = "Sales Visit Response Submitted";

                            mail.Body = $@"
                            <html>
                            <body style='font-family:Arial; font-size:14px; color:#333;'>
                                <h2 style='color:#0066cc;'>Sales Visit Report – {role} Reply</h2>

                                <div style='border:1px solid #ddd; padding:10px; margin-bottom:15px;'>
                                    <h3 style='margin-top:0;'>Visit Details</h3>
                                    {htmlDetails}
                                </div>

                                <div style='border:1px solid #0066cc; background-color:#f9f9f9; padding:10px;'>
                                    <h3 style='margin-top:0;'>{role} Reply</h3>
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

                // 4. Hide popup & refresh grid
                txtSalespersonReply.Text = "";
                pnlReply.Style["display"] = "none";
                BindSalesVisits();
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error saving reply: " + ex.Message;
                PanelError.Visible = true;
            }
        }

        private void AddResponse(int visitId, string role, string userCode, string replyText)
        {
            string query = @"
            INSERT INTO tbl_SalesVisitResponses (VisitId, RespondentRole, RespondentCode, ResponseText)
            VALUES (@VisitId, @Role, @UserCode, @ReplyText)";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@VisitId", visitId);
                cmd.Parameters.AddWithValue("@Role", role);
                cmd.Parameters.AddWithValue("@UserCode", userCode);
                cmd.Parameters.AddWithValue("@ReplyText", replyText);
                con.Open();
                cmd.ExecuteNonQuery();
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
        //                    attachmentLink = $"<a href='https://www.exc.aagroupindia.com/Uploads/{rdr["AttachmentName"]}' target='_blank'>{rdr["AttachmentName"]}</a>";
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


        private void GetApproverEmailAndVisitDetails(
    int visitId,
    out string approverEmail,
    out string salespersonEmail,
    out string htmlDetails,
    out DateTime replyDate)
        {
            approverEmail = "";
            salespersonEmail = "";
            htmlDetails = "";
            replyDate = DateTime.Now;

            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT 
                        v.ApprovedBy,
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
                    LEFT JOIN tbl_login mgr ON v.ApprovedBy = mgr.User_Id
                    INNER JOIN tbl_login sp ON v.Salesperson = sp.Name -- ⚠ ideally should be User_Id
                    WHERE v.Id = @Id";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", visitId);
                con.Open();

                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        // Salesperson email
                        salespersonEmail = rdr["SalespersonEmail"].ToString();

                        // Approver email (fallback to current user)
                        if (rdr["ApproverEmail"] != DBNull.Value && !string.IsNullOrEmpty(rdr["ApproverEmail"].ToString()))
                        {
                            approverEmail = rdr["ApproverEmail"].ToString();
                        }
                        else
                        {
                            string currentUserId = HttpContext.Current.Session["USERID"]?.ToString() ?? "";
                            approverEmail = GetUserEmailById(currentUserId);
                        }

                        // Reply date
                        replyDate = rdr["SalespersonReplyDate"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(rdr["SalespersonReplyDate"]);

                        string attachmentLink = string.IsNullOrEmpty(rdr["AttachmentName"].ToString()) ? "N/A"
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
                            </table>";
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
                                string date = Convert.ToDateTime(rdrC["ResponseDate"]).ToString("dd-MMM-yyyy HH:mm");

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


        private string GetUserEmailById(string userId)
        {
            string email = "";
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Email FROM tbl_login WHERE User_Id = @UserId", con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    object result = cmd.ExecuteScalar();
                    if (result != null) email = result.ToString();
                }
            }
            return email;
        }




        //private string GetApproverEmailByVisitId(int visitId)
        //{
        //    string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
        //    using (SqlConnection con = new SqlConnection(connStr))
        //    {
        //        string query = @"SELECT u.Email FROM tbl_SalesVisitReport v INNER JOIN tbl_login u ON v.ApprovedBy = u.User_Id WHERE v.Id = @Id";
        //        SqlCommand cmd = new SqlCommand(query, con);
        //        cmd.Parameters.AddWithValue("@Id", visitId);
        //        con.Open();
        //        object result = cmd.ExecuteScalar();
        //        return result != DBNull.Value ? Convert.ToString(result) : string.Empty;
        //    }
        //}

        protected void btnSendMessage_Click(object sender, EventArgs e)
        {
            int visitId = int.Parse(hfVisitId.Value);
            //string reply = txtNewMessage.Text.Trim();
            string reply = "";
            string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "";
            string role = GetUserRole(userId, visitId); // Function to determine if it's Salesperson or Manager

            if (!string.IsNullOrEmpty(reply))
            {
                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
                string query = @"
                INSERT INTO tbl_SalesVisitResponses (VisitId, RespondentRole, RespondentCode, ResponseText)
                VALUES (@VisitId, @Role, @UserCode, @Reply)";

                using (SqlConnection con = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@VisitId", visitId);
                    cmd.Parameters.AddWithValue("@Role", role);
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    cmd.Parameters.AddWithValue("@Reply", reply);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                //txtNewMessage.Text = "";
                ShowConversation(visitId); // Refresh conversation
            }
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
                        ELSE 'Unknown'
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

        private void ShowCommentsPopup()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowComments", "showCommentsPopup();", true);
        }

        private void ShowReplyPopup()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowReply", "showReplyPopup(" + hfVisitId.Value + ");", true);
        }


        protected void btnSaveVisit_Click1(object sender, EventArgs e)
        {
            int visitId = Convert.ToInt32(hfVisitId.Value);

            //using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            //{
            //    string updateQuery = @"
            //    UPDATE tbl_SalesVisitReport
            //    SET CustomerName = @CustomerName,
            //        Department = @Department,
            //        ContactPerson = @ContactPerson,
            //        VisitType = @VisitType,
            //        DiscussionPoints = @DiscussionPoints,
            //        FollowUpRequired = @FollowUpRequired,
            //        NextFollowUpDate = @NextFollowUpDate,
            //        AttachmentName = @AttachmentName
            //    WHERE Id = @Id";

            //    SqlCommand cmd = new SqlCommand(updateQuery, con);
            //    cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text);
            //    cmd.Parameters.AddWithValue("@Department", txtDepartment.Text);
            //    cmd.Parameters.AddWithValue("@ContactPerson", txtContactPerson.Text);
            //    cmd.Parameters.AddWithValue("@VisitType", txtVisitType.Text);
            //    cmd.Parameters.AddWithValue("@DiscussionPoints", txtDiscussionPoints.Text);
            //    cmd.Parameters.AddWithValue("@FollowUpRequired", ddlFollowUpRequired.SelectedValue);
            //    cmd.Parameters.AddWithValue("@NextFollowUpDate", string.IsNullOrEmpty(txtNextFollowUpDate.Text) ? DBNull.Value : Convert.ToDateTime(txtNextFollowUpDate.Text));
            //    cmd.Parameters.AddWithValue("@AttachmentName", fuAttachment.HasFile ? fuAttachment.FileName : DBNull.Value);
            //    cmd.Parameters.AddWithValue("@Id", visitId);

            //    con.Open();
            //    cmd.ExecuteNonQuery();

            //    if (fuAttachment.HasFile)
            //    {
            //        string path = Server.MapPath("~/Uploads/") + fuAttachment.FileName;
            //        fuAttachment.SaveAs(path);
            //    }
            //}

            PanelOK.Visible = false;
            PanelError.Visible = false;

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = @"
                UPDATE tbl_SalesVisitReport
                SET VisitDate = @VisitDate,
                    Salesperson = @Salesperson,
                    CustomerName = @CustomerName,
                    Department = @Department,
                    ContactPerson = @ContactPerson,
                    VisitType = @VisitType,
                    DiscussionPoints = @DiscussionPoints,
                    FollowUpRequired = @FollowUpRequired,
                    NextFollowUpDate = @NextFollowUpDate,
                    Status = @Status,
                    AttachmentName = @AttachmentName
                WHERE Id = @Id
                  AND NOT EXISTS (
                      SELECT 1
                      FROM tbl_SalesVisitResponses
                      WHERE VisitId = tbl_SalesVisitReport.Id
                        AND RespondentRole = 'Manager'
                  )"; // 🚫 Only allow update if no manager comment exists

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", visitId); // hidden field from popup

                        //cmd.Parameters.AddWithValue("@VisitDate", txtVisitDate.Text.Trim());
                        //cmd.Parameters.AddWithValue("@Salesperson", txtSalesperson.Text.Trim());
                        cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Department", txtDepartment.Text.Trim());
                        cmd.Parameters.AddWithValue("@ContactPerson", txtContactPerson.Text.Trim());
                        //cmd.Parameters.AddWithValue("@VisitType", txtVisitType.SelectedValue);
                        cmd.Parameters.AddWithValue("@DiscussionPoints", txtDiscussion.Text.Trim());
                        //cmd.Parameters.AddWithValue("@FollowUpRequired", ddlFollowUp.SelectedValue);

                        //if (!string.IsNullOrEmpty(txtNextFollowUp.Text))
                        //    cmd.Parameters.AddWithValue("@NextFollowUpDate", txtNextFollowUp.Text.Trim());
                        //else
                        //    cmd.Parameters.AddWithValue("@NextFollowUpDate", DBNull.Value);

                        //cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);

                        //string fileName = null;
                        //if (fileAttachment.HasFile)
                        //{
                        //    string originalFileName = Path.GetFileName(fileAttachment.FileName);
                        //    string datePrefix = DateTime.Now.ToString("yyyyMMdd");
                        //    fileName = datePrefix + "_" + originalFileName;

                        //    string uploadPath = Server.MapPath("~/Uploads/");
                        //    if (!Directory.Exists(uploadPath))
                        //        Directory.CreateDirectory(uploadPath);

                        //    fileAttachment.SaveAs(Path.Combine(uploadPath, fileName));
                        //}

                        //cmd.Parameters.AddWithValue("@AttachmentName", (object)fileName ?? DBNull.Value);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            lblOk.Text = "Sales visit report updated successfully!";
                            PanelOK.Visible = true;
                        }
                        else
                        {
                            lblErrorMsg.Text = "Update not allowed. A manager has already added a comment.";
                            PanelError.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "An error occurred. Please try again later.";
                PanelError.Visible = true;
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"UPDATE tbl_SalesVisitReport
                         SET VisitDate=@VisitDate,
                             Salesperson=@Salesperson,
                             CustomerName=@CustomerName,
                             Department=@Department,
                             ContactPerson=@ContactPerson,
                             VisitType=@VisitType,
                             DiscussionPoints=@DiscussionPoints,
                             FollowUpRequired=@FollowUpRequired,
                             NextFollowUpDate=@NextFollowUpDate,
                             Status=@Status,
                             AttachmentName=@AttachmentName
                         WHERE Id=@Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", hfEditId.Value);
                    cmd.Parameters.AddWithValue("@VisitDate", txtVisitDate.Text.Trim());
                    cmd.Parameters.AddWithValue("@Salesperson", txtSalesperson.Text.Trim());
                    cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Department", txtDepartment.Text.Trim());
                    cmd.Parameters.AddWithValue("@ContactPerson", txtContactPerson.Text.Trim());
                    cmd.Parameters.AddWithValue("@VisitType", ddlVisitType.SelectedValue);
                    cmd.Parameters.AddWithValue("@DiscussionPoints", txtDiscussion.Text.Trim());
                    cmd.Parameters.AddWithValue("@FollowUpRequired", ddlFollowUp.SelectedValue);

                    if (!string.IsNullOrEmpty(txtNextFollowUp.Text))
                        cmd.Parameters.AddWithValue("@NextFollowUpDate", txtNextFollowUp.Text.Trim());
                    else
                        cmd.Parameters.AddWithValue("@NextFollowUpDate", DBNull.Value);

                    cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);

                    // Attachment logic (only replace if new file uploaded)
                    string fileName = null;
                    if (fileAttachment.HasFile)
                    {
                        string datePrefix = DateTime.Now.ToString("yyyyMMdd");
                        fileName = datePrefix + "_" + Path.GetFileName(fileAttachment.FileName);
                        string uploadPath = Server.MapPath("~/Uploads/");
                        if (!Directory.Exists(uploadPath))
                            Directory.CreateDirectory(uploadPath);
                        fileAttachment.SaveAs(Path.Combine(uploadPath, fileName));
                    }
                    cmd.Parameters.AddWithValue("@AttachmentName", (object)fileName ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            // Refresh the grid
            BindSalesVisits();
            // Update UpdatePanel
            upModify.Update();
            ScriptManager.RegisterStartupScript(this, GetType(), "HidePopup", "$('#editModal').modal('hide');", true);

        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int visitId = Convert.ToInt32(btn.CommandArgument);

            hfEditId.Value = visitId.ToString();

            SalesVisit visit = GetVisitDetails(visitId);
            if (visit != null)
            {
                txtVisitDate.Text = visit.VisitDate.ToString("yyyy-MM-dd");
                txtSalesperson.Text = visit.Salesperson;
                txtCustomerName.Text = visit.CustomerName;
                txtDepartment.Text = visit.Department;
                txtContactPerson.Text = visit.ContactPerson;
                ddlVisitType.SelectedValue = visit.VisitType;
                txtDiscussion.Text = visit.DiscussionPoints;
                ddlFollowUp.SelectedValue = visit.FollowUpRequired;
                txtNextFollowUp.Text = visit.NextFollowUpDate?.ToString("yyyy-MM-dd") ?? "";
                ddlStatus.SelectedValue = visit.Status;
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "Pop", "showEditModal();", true);
        }

    }
}