using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Text;

namespace Bill_Software.corporate.business.app
{
    public partial class vw_dailyrpts : System.Web.UI.Page
    {
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

        // ==========================================
        // MAIN GRID BINDING
        // ==========================================
        private void BindSalesVisits()
        {
            string user = HttpContext.Current.Session["USERID"]?.ToString() ?? "";
            string query = @"
            SELECT Id, VisitDate, CustomerName, VisitType, Status, ApprovalStatus 
            FROM tbl_SalesVisitReport
            WHERE CreatedByCode = @CreatedByCode
            ORDER BY VisitDate DESC";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@CreatedByCode", user);
                con.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvSalesVisits.DataSource = dt;
                    gvSalesVisits.DataBind();
                }
            }
        }

        protected string GetApprovalClass(object statusObj)
        {
            string status = Convert.ToString(statusObj ?? "").Trim();
            if (string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase)) return "approval-chip approval-approved";
            if (string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase)) return "approval-chip approval-pending";
            if (string.Equals(status, "Rejected", StringComparison.OrdinalIgnoreCase)) return "approval-chip approval-rejected";
            return "approval-chip approval-default";
        }

        // ==========================================
        // MEGA MODAL LOGIC
        // ==========================================
        protected void gvSalesVisits_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "OpenMegaModal")
            {
                string visitId = e.CommandArgument.ToString();
                hfMegaVisitId.Value = visitId;

                PanelOK.Visible = false;
                PanelError.Visible = false;

                LoadMegaModal(visitId);
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowMega", "showMegaModal('tabDetails');", true);
            }
        }

        private void LoadMegaModal(string visitId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                // 1. Load Core Details
                // FIX: Added a subquery to count Manager Comments directly in this initial query to prevent DataReader collisions!
                string query = @"
                    SELECT v.*, 
                           (SELECT COUNT(*) FROM tbl_SalesVisitResponses r WHERE r.VisitId = v.Id AND r.RespondentRole = 'Manager') AS MgrCommentCount
                    FROM tbl_SalesVisitReport v 
                    WHERE v.Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id", visitId);
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            lblMegaHeaderTitle.Text = $"#{visitId} - {rdr["CustomerName"]}";

                            // Populate Form Fields
                            txtVisitDate.Text = Convert.ToDateTime(rdr["VisitDate"]).ToString("dd-MMM-yyyy");
                            txtCustomerName.Text = rdr["CustomerName"].ToString();
                            txtDepartment.Text = rdr["Department"].ToString();
                            txtContactPerson.Text = rdr["ContactPerson"].ToString();
                            txtDiscussion.Text = rdr["DiscussionPoints"].ToString();

                            if (ddlVisitType.Items.FindByValue(rdr["VisitType"].ToString()) != null) ddlVisitType.SelectedValue = rdr["VisitType"].ToString();
                            if (ddlFollowUp.Items.FindByValue(rdr["FollowUpRequired"].ToString()) != null) ddlFollowUp.SelectedValue = rdr["FollowUpRequired"].ToString();
                            if (ddlStatus.Items.FindByValue(rdr["Status"].ToString()) != null) ddlStatus.SelectedValue = rdr["Status"].ToString();

                            txtNextFollowUp.Text = rdr["NextFollowUpDate"] != DBNull.Value ? Convert.ToDateTime(rdr["NextFollowUpDate"]).ToString("dd-MMM-yyyy") : "";

                            if (rdr["AttachmentName"] != DBNull.Value && rdr["AttachmentName"].ToString() != "")
                            {
                                hlCurrentAttachment.Text = "📎 View Current Attachment";
                                hlCurrentAttachment.NavigateUrl = "~/Uploads/" + rdr["AttachmentName"].ToString();
                            }
                            else
                            {
                                hlCurrentAttachment.Text = "";
                                hlCurrentAttachment.NavigateUrl = "";
                            }

                            // 🚨 EDIT LOCK LOGIC (FIXED) 🚨
                            DateTime visitDate = Convert.ToDateTime(rdr["VisitDate"]);
                            string approvalStatus = rdr["ApprovalStatus"].ToString();
                            bool isEditable = true;
                            string warningMsg = "";

                            if (approvalStatus != "Pending")
                            {
                                isEditable = false;
                                warningMsg = $"This file is locked because it has been {approvalStatus}.";
                            }
                            else if ((DateTime.Now - visitDate).TotalDays > 45)
                            {
                                isEditable = false;
                                warningMsg = "This file is locked because it is older than 45 days.";
                            }
                            else
                            {
                                // FIX: We now just read the count from our modified SELECT query above instead of opening a 2nd DataReader!
                                int mgrComments = Convert.ToInt32(rdr["MgrCommentCount"]);
                                if (mgrComments > 0)
                                {
                                    isEditable = false;
                                    warningMsg = "Editing is locked because a Manager has already reviewed and commented on this file.";
                                }
                            }

                            // Apply Lock States
                            pnlEditForm.Enabled = isEditable;
                            btnUpdateVisit.Visible = isEditable;
                            lblEditWarning.Visible = !isEditable;
                            lblEditWarning.Text = warningMsg;

                            // Tab 2: Map
                            string lat = rdr["Latitude"] != DBNull.Value ? rdr["Latitude"].ToString() : "";
                            string lon = rdr["Longitude"] != DBNull.Value ? rdr["Longitude"].ToString() : "";
                            if (!string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lon))
                            {
                                string mapUrl = $"https://maps.google.com/maps?q={lat},{lon}&hl=en&z=15&output=embed";
                                megaMapContainer.InnerHtml = $"<iframe width='100%' height='100%' frameborder='0' scrolling='no' src='{mapUrl}'></iframe>";
                            }
                            else
                            {
                                megaMapContainer.InnerHtml = "<div style='display:flex; height:100%; align-items:center; justify-content:center;'><span style='color: #888; font-style: italic;'>Location was not captured.</span></div>";
                            }
                        }
                    }
                }

                // 2. Fetch Expenses (Read-Only)
                using (SqlCommand cmdExp = new SqlCommand("SELECT ExpenseDate, ExpenseCategory, Description, Amount, ApprovalStatus FROM tbl_Expenses WHERE VisitId = @Id", con))
                {
                    cmdExp.Parameters.AddWithValue("@Id", visitId);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmdExp))
                    {
                        DataTable dtExp = new DataTable();
                        da.Fill(dtExp);
                        gvMegaExpenses.DataSource = dtExp;
                        gvMegaExpenses.DataBind();
                    }
                }

                // 3. Load Chat
                BindMegaComments(visitId, con);
            }
        }

        protected void btnUpdateVisit_Click(object sender, EventArgs e)
        {
            string visitId = hfMegaVisitId.Value;
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = @"
                    UPDATE tbl_SalesVisitReport
                    SET VisitDate = @VisitDate, CustomerName = @CustomerName, Department = @Department, ContactPerson = @ContactPerson,
                        VisitType = @VisitType, DiscussionPoints = @DiscussionPoints, FollowUpRequired = @FollowUpRequired, 
                        NextFollowUpDate = @NextFollowUpDate, Status = @Status, AttachmentName = COALESCE(@AttachmentName, AttachmentName)
                    WHERE Id = @Id 
                      AND ApprovalStatus = 'Pending' 
                      AND NOT EXISTS (SELECT 1 FROM tbl_SalesVisitResponses WHERE VisitId = tbl_SalesVisitReport.Id AND RespondentRole = 'Manager')";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", visitId);
                        cmd.Parameters.AddWithValue("@VisitDate", Convert.ToDateTime(txtVisitDate.Text));
                        cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Department", txtDepartment.Text.Trim());
                        cmd.Parameters.AddWithValue("@ContactPerson", txtContactPerson.Text.Trim());
                        cmd.Parameters.AddWithValue("@VisitType", ddlVisitType.SelectedValue);
                        cmd.Parameters.AddWithValue("@DiscussionPoints", txtDiscussion.Text.Trim());
                        cmd.Parameters.AddWithValue("@FollowUpRequired", ddlFollowUp.SelectedValue);
                        cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);

                        if (!string.IsNullOrEmpty(txtNextFollowUp.Text)) cmd.Parameters.AddWithValue("@NextFollowUpDate", Convert.ToDateTime(txtNextFollowUp.Text));
                        else cmd.Parameters.AddWithValue("@NextFollowUpDate", DBNull.Value);

                        // File Upload Logic
                        string fileName = null;
                        if (fileAttachment.HasFile)
                        {
                            fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Path.GetFileName(fileAttachment.FileName);
                            string uploadPath = Server.MapPath("~/Uploads/");
                            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                            fileAttachment.SaveAs(Path.Combine(uploadPath, fileName));
                        }
                        cmd.Parameters.AddWithValue("@AttachmentName", (object)fileName ?? DBNull.Value);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            lblOk.Text = "Visit file updated successfully!";
                            PanelOK.Visible = true;
                            BindSalesVisits();
                            ScriptManager.RegisterStartupScript(this, GetType(), "HideMega", "hideMegaModal();", true);
                        }
                        else
                        {
                            lblErrorMsg.Text = "Update failed. The file may be locked by a Manager.";
                            PanelError.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error updating file: " + ex.Message;
                PanelError.Visible = true;
            }
        }

        // ==========================================
        // CHAT & EMAILS
        // ==========================================
        private void BindMegaComments(string visitId, SqlConnection con)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT r.RespondentCode, r.RespondentRole, r.ResponseText, r.ResponseDate, u.Name
                FROM tbl_SalesVisitResponses r
                INNER JOIN tbl_login u ON r.RespondentCode = u.User_Id
                WHERE r.VisitId = @VisitId ORDER BY r.ResponseDate ASC", con))
            {
                cmd.Parameters.AddWithValue("@VisitId", visitId);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    StringBuilder sb = new StringBuilder();
                    while (dr.Read())
                    {
                        // 🟢 FLIPPED COLORS: Since current user = Salesperson, their bubbles are Right (Green). Managers are Left (White).
                        bool isSalesperson = dr["RespondentRole"].ToString().Equals("Salesperson", StringComparison.OrdinalIgnoreCase);
                        string bubbleClass = isSalesperson ? "chat-right" : "chat-left";

                        string senderName = dr["Name"].ToString();
                        string msgText = dr["ResponseText"].ToString().Replace(Environment.NewLine, "<br/>");
                        string timeString = Convert.ToDateTime(dr["ResponseDate"]).ToString("dd-MMM hh:mm tt");

                        sb.AppendFormat(
                            "<div class='chat-message {0}'><span class='chat-sender'>{1}</span><span class='chat-text'>{2}</span><span class='chat-time'>{3}</span></div>",
                            bubbleClass, senderName, msgText, timeString
                        );
                    }
                    litMegaComments.Text = sb.ToString();
                }
            }
        }

        protected void btnMegaSendChat_Click(object sender, EventArgs e)
        {
            string visitId = hfMegaVisitId.Value;
            string comment = txtMegaNewComment.Text.Trim();

            if (!string.IsNullOrEmpty(comment) && !string.IsNullOrEmpty(visitId))
            {
                string userCode = Session["USERID"].ToString();

                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO tbl_SalesVisitResponses (VisitId, RespondentRole, RespondentCode, ResponseText, ResponseDate) VALUES (@VisitId, 'Salesperson', @Code, @Text, GETDATE())", con))
                    {
                        cmd.Parameters.AddWithValue("@VisitId", visitId);
                        cmd.Parameters.AddWithValue("@Code", userCode);
                        cmd.Parameters.AddWithValue("@Text", comment);
                        cmd.ExecuteNonQuery();
                    }
                }
                SendChatEmailNotification(visitId, comment, userCode);
            }

            txtMegaNewComment.Text = "";
            LoadMegaModal(visitId);
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowMegaChat", "showMegaModal('tabAction');", true);
        }

        private string GetVisitEmailBody(string visitId, SqlConnection con)
        {
            string htmlDetails = "";
            string visitQuery = "SELECT v.*, mgr.Name AS ApprovedByName FROM tbl_SalesVisitReport v LEFT JOIN tbl_login mgr ON v.ApprovedBy = mgr.User_Id WHERE v.Id = @Id";

            using (SqlCommand cmd = new SqlCommand(visitQuery, con))
            {
                cmd.Parameters.AddWithValue("@Id", visitId);
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        string attachmentLink = string.IsNullOrEmpty(rdr["AttachmentName"].ToString()) ? "N/A" : $"<a href='https://www.exc.aagroupindia.com/Uploads/{rdr["AttachmentName"]}' target='_blank'>📎 View Document</a>";
                        htmlDetails = $@"
                            <table border='1' cellspacing='0' cellpadding='8' style='border-collapse:collapse; width:100%; max-width: 800px; font-family:Arial,sans-serif; font-size:14px; color:#333; border: 1px solid #ccc;'>
                                <tr style='background-color:#f4f8fb;'><td style='width:35%;'><b>Visit Date</b></td><td>{Convert.ToDateTime(rdr["VisitDate"]):dd-MMM-yyyy}</td></tr>
                                <tr><td><b>Salesperson</b></td><td>{rdr["Salesperson"]}</td></tr>
                                <tr style='background-color:#f4f8fb;'><td><b>Customer Name</b></td><td>{rdr["CustomerName"]}</td></tr>
                                <tr><td><b>Department</b></td><td>{rdr["Department"]}</td></tr>
                                <tr style='background-color:#f4f8fb;'><td><b>Contact Person</b></td><td>{rdr["ContactPerson"]}</td></tr>
                                <tr><td><b>Visit Type</b></td><td>{rdr["VisitType"]}</td></tr>
                                <tr style='background-color:#f4f8fb;'><td><b>Discussion Points</b></td><td>{rdr["DiscussionPoints"].ToString().Replace(Environment.NewLine, "<br/>")}</td></tr>
                                <tr><td><b>Follow-Up Required</b></td><td>{rdr["FollowUpRequired"]}</td></tr>
                                <tr style='background-color:#f4f8fb;'><td><b>Next Follow-Up Date</b></td><td>{(rdr["NextFollowUpDate"] == DBNull.Value ? "N/A" : Convert.ToDateTime(rdr["NextFollowUpDate"]).ToString("dd-MMM-yyyy"))}</td></tr>
                                <tr><td><b>Status</b></td><td><b style='color:#19658A;'>{rdr["Status"]}</b></td></tr>
                                <tr style='background-color:#f4f8fb;'><td><b>Manager Remarks</b></td><td>{rdr["ManagerRemarks"]}</td></tr>
                                <tr><td><b>Approved Date</b></td><td>{(rdr["ApprovedDate"] == DBNull.Value ? "N/A" : Convert.ToDateTime(rdr["ApprovedDate"]).ToString("dd-MMM-yyyy HH:mm tt"))}</td></tr>
                                <tr style='background-color:#f4f8fb;'><td><b>Approved By</b></td><td>{rdr["ApprovedByName"]}</td></tr>
                                <tr><td><b>Attachment</b></td><td>{attachmentLink}</td></tr>
                            </table>";
                    }
                }
            }

            string commentQuery = "SELECT r.RespondentRole, u.Name AS RespondentName, r.ResponseText, r.ResponseDate FROM tbl_SalesVisitResponses r INNER JOIN tbl_login u ON r.RespondentCode = u.User_Id WHERE r.VisitId = @VisitId ORDER BY r.ResponseDate ASC";
            using (SqlCommand cmdComments = new SqlCommand(commentQuery, con))
            {
                cmdComments.Parameters.AddWithValue("@VisitId", visitId);
                using (SqlDataReader rdrC = cmdComments.ExecuteReader())
                {
                    if (rdrC.HasRows)
                    {
                        htmlDetails += "<br/><h4 style='font-family:Arial,sans-serif; color:#19658A; margin-bottom: 5px; border-bottom: 1px solid #eee; padding-bottom: 5px; max-width: 800px;'>Conversations:</h4><div style='font-family:Arial,sans-serif; font-size:14px; max-width: 800px;'>";
                        while (rdrC.Read())
                        {
                            bool isManager = rdrC["RespondentRole"].ToString().Equals("Manager", StringComparison.OrdinalIgnoreCase);
                            string align = isManager ? "right" : "left";
                            string bgColor = isManager ? "#e1f5fe" : "#fce4ec";
                            htmlDetails += $@"<div style='text-align:{align}; margin:8px 0;'><div style='display:inline-block; text-align:left; background-color:{bgColor}; padding:10px 15px; border-radius:8px; max-width:75%; border: 1px solid #ddd;'><b style='color:#19658A; font-size:12px;'>{rdrC["RespondentName"]}</b> <small style='color:#666;'>({Convert.ToDateTime(rdrC["ResponseDate"]):dd-MMM HH:mm})</small><br/><div style='margin-top:4px; color:#333; line-height: 1.4;'>{rdrC["ResponseText"].ToString().Replace(Environment.NewLine, "<br/>")}</div></div></div>";
                        }
                        htmlDetails += "</div>";
                    }
                }
            }
            return htmlDetails;
        }

        private void SendChatEmailNotification(string visitId, string commentText, string senderUserCode)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT SVR.CustomerName, Manager.Email AS ManagerEmail
                        FROM tbl_SalesVisitReport SVR 
                        INNER JOIN tbl_login Creator ON Creator.User_Id = SVR.CreatedByCode 
                        LEFT JOIN tbl_login Manager ON Manager.User_Id = Creator.ReportingManagerId
                        WHERE SVR.Id = @Id";

                    string emailTo = "";
                    string customerName = "";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", visitId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                customerName = reader["CustomerName"]?.ToString();
                                emailTo = reader["ManagerEmail"]?.ToString();
                            }
                        }
                    }

                    if (string.IsNullOrWhiteSpace(emailTo)) return;

                    string dataRichHtml = GetVisitEmailBody(visitId, conn);
                    string body = $@"<html><body style='font-family: Arial, sans-serif; color: #333;'>
                            <h2 style='color: #0056b3; margin-bottom: 15px;'>Sales Visit Report &ndash; Salesperson Reply</h2>
                            <div style='margin-bottom: 20px;'>{dataRichHtml}</div>
                            <div style='border: 2px solid #0056b3; padding: 15px; margin-top: 20px; max-width: 800px;'>
                                <h3 style='color: #0056b3; margin-top: 0; margin-bottom: 15px;'>Salesperson Reply</h3>
                                <p style='margin: 0 0 10px 0;'><b>Reply Date:</b> {DateTime.Now.ToString("dd-MMM-yyyy HH:mm tt")}</p>
                                <p style='margin: 0;'>{commentText.Replace(Environment.NewLine, "<br/>")}</p>
                            </div>
                            </body></html>";

                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress("it.support@aminruptechnologies.co.in", "Flame-Ex : Sales Reporting Mailer");
                        mail.To.Add(emailTo);
                        mail.Subject = $"Sales Visit Report - Salesperson Reply";
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
            catch (Exception) { /* Fail silently */ }
        }
    }
}