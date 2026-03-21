using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

namespace Bill_Software.corporate.business.app
{
    public partial class srch_dailyrpts : System.Web.UI.Page
    {
        // Assuming you have this utility class in your project based on your original code
        DB_UTILITY DbCL = new DB_UTILITY();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                DbCL.FillCombo(cmbvendor, "select Name from tbl_login order by Name");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");

                // Load data initially
                Binder();
            }
        }

        // ==========================================
        // MAIN SEARCH & GRID BINDING
        // ==========================================

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            Binder();
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("srch_dailyrpts.aspx");
        }

        private void Binder()
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0) // Only Person
            {
                BuindCompanyId();
                cmdstring = "SELECT * FROM tbl_SalesVisitReport WHERE Salesperson='" + cmbvendor.SelectedItem.Text + "' ORDER BY CAST(VisitDate as date) DESC";
            }
            else if (RadioButtonList1.SelectedIndex == 1) // Only Date
            {
                cmdstring = "SELECT * FROM tbl_SalesVisitReport WHERE VisitDate BETWEEN '" + txttodate.Text + "' AND '" + txtfromDate.Text + "' ORDER BY CAST(VisitDate as date) DESC";
            }
            else // Person & Date
            {
                BuindCompanyId();
                cmdstring = "SELECT * FROM tbl_SalesVisitReport WHERE Salesperson='" + cmbvendor.SelectedItem.Text + "' AND VisitDate BETWEEN '" + txttodate.Text + "' AND '" + txtfromDate.Text + "' ORDER BY CAST(VisitDate as date) DESC";
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmdstring, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        DataList2.DataSource = dt;
                        DataList2.DataBind();
                        PanelError.Visible = false;
                    }
                    else
                    {
                        DataList2.DataSource = null;
                        DataList2.DataBind();
                        lblErrorMsg.Text = "No records found for the selected criteria.";
                        PanelError.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error loading data: " + ex.Message;
                PanelError.Visible = true;
            }
        }

        private void BuindCompanyId()
        {
            try
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
            catch (Exception) { /* Handle Silently */ }
        }

        // ==========================================
        // MEGA MODAL: OPEN & LOAD DATA
        // ==========================================

        protected void DataList2_ItemCommand(object source, DataListCommandEventArgs e)
        {
            if (e.CommandName == "OpenMegaModal")
            {
                string visitId = e.CommandArgument.ToString();
                hfMegaVisitId.Value = visitId;

                PanelOK.Visible = false;
                PanelError.Visible = false;

                // Load all data for the 4 tabs
                LoadMegaModal(visitId);

                // Tell JS to open the Modal AND default to the 'tabDetails' tab
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowMega", "showMegaModal('tabDetails');", true);
            }
        }

        private void LoadMegaModal(string visitId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                // 1. Fetch Visit Details (For Tabs 1 & 2 & 4)
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tbl_SalesVisitReport WHERE Id = @Id", con))
                {
                    cmd.Parameters.AddWithValue("@Id", visitId);
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            // Header
                            lblMegaHeaderTitle.Text = $"#{visitId} - {rdr["CustomerName"]}";

                            // Tab 1: Details
                            lblMegaSalesperson.Text = rdr["Salesperson"].ToString();
                            lblMegaCustomer.Text = rdr["CustomerName"].ToString();
                            lblMegaContact.Text = $"{rdr["ContactPerson"]} / {rdr["Department"]}";
                            lblMegaPlanDate.Text = Convert.ToDateTime(rdr["VisitDate"]).ToString("dd-MMM-yyyy");
                            lblMegaExecDate.Text = rdr["ExecutionDateTime"] != DBNull.Value ? Convert.ToDateTime(rdr["ExecutionDateTime"]).ToString("dd-MMM-yyyy hh:mm tt") : "Not Executed";
                            lblMegaFollow.Text = $"{rdr["FollowUpRequired"]} (Next: {(rdr["NextFollowUpDate"] != DBNull.Value ? Convert.ToDateTime(rdr["NextFollowUpDate"]).ToString("dd-MMM-yyyy") : "N/A")})";
                            lblMegaNotes.Text = rdr["DiscussionPoints"].ToString();

                            // Attachment Logic
                            if (rdr["AttachmentName"] != DBNull.Value && rdr["AttachmentName"].ToString() != "")
                            {
                                hlMegaAttachment.Text = "📎 View File";
                                hlMegaAttachment.NavigateUrl = "~/Uploads/" + rdr["AttachmentName"].ToString();
                            }
                            else
                            {
                                hlMegaAttachment.Text = "No File Attached";
                                hlMegaAttachment.NavigateUrl = "";
                            }

                            // Tab 2: Map Logic
                            string lat = rdr["Latitude"] != DBNull.Value ? rdr["Latitude"].ToString() : "";
                            string lon = rdr["Longitude"] != DBNull.Value ? rdr["Longitude"].ToString() : "";
                            if (!string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lon))
                            {
                                string mapUrl = $"https://maps.google.com/maps?q={lat},{lon}&hl=en&z=15&output=embed";
                                megaMapContainer.InnerHtml = $"<iframe width='100%' height='100%' frameborder='0' scrolling='no' src='{mapUrl}'></iframe>";
                            }
                            else
                            {
                                megaMapContainer.InnerHtml = "<div style='display:flex; height:100%; align-items:center; justify-content:center;'><span style='color: #888; font-style: italic;'>Location not captured.</span></div>";
                            }

                            // Tab 4: Approval Panel Logic
                            string approvalStatus = rdr["ApprovalStatus"].ToString();
                            if (approvalStatus == "Pending")
                            {
                                pnlMegaAction.Visible = true;
                                pnlMegaAlreadyActioned.Visible = false;
                                txtMegaRemarks.Text = "";
                            }
                            else
                            {
                                pnlMegaAction.Visible = false;
                                pnlMegaAlreadyActioned.Visible = true;
                                lblMegaFinalStatus.Text = approvalStatus;
                                lblMegaFinalStatus.ForeColor = approvalStatus == "Approved" ? System.Drawing.Color.Green : System.Drawing.Color.Red;
                                lblMegaFinalBy.Text = rdr["ApprovedBy"].ToString();
                                lblMegaFinalDate.Text = rdr["ApprovedDate"] != DBNull.Value ? Convert.ToDateTime(rdr["ApprovedDate"]).ToString("dd-MMM-yyyy HH:mm") : "";
                            }
                        }
                    }
                }
                // REPLACE THE OLD EXPENSE FETCHING BLOCK WITH THIS SINGLE LINE:
                // 2. Fetch Expenses associated with this visit (For Tab 3)
                BindExpenses(visitId);

                // 3. Load Chat History (For Tab 4)
                BindMegaComments(visitId, con);
            }
        }

        // ==========================================
        // TAB 4: CHAT & COMMENTS LOGIC
        // ==========================================

        private void BindMegaComments(string visitId, SqlConnection con)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT r.RespondentCode, r.RespondentRole, r.ResponseText, r.ResponseDate, u.Name
                FROM tbl_SalesVisitResponses r
                INNER JOIN tbl_login u ON r.RespondentCode = u.User_Id
                WHERE r.VisitId = @VisitId ORDER BY r.ResponseDate", con))
            {
                cmd.Parameters.AddWithValue("@VisitId", visitId);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    StringBuilder sb = new StringBuilder();
                    while (dr.Read())
                    {
                        // Because this is the Manager's dashboard, Manager comments are on the Right (Green).
                        bool isManager = dr["RespondentRole"].ToString().Equals("Manager", StringComparison.OrdinalIgnoreCase);
                        string bubbleClass = isManager ? "chat-right" : "chat-left";

                        string senderName = dr["Name"].ToString();
                        string msgText = dr["ResponseText"].ToString().Replace(Environment.NewLine, "<br/>");
                        string timeString = Convert.ToDateTime(dr["ResponseDate"]).ToString("dd-MMM hh:mm tt");

                        // Build the WhatsApp Speech Bubble
                        sb.AppendFormat(
                            "<div class='chat-message {0}'>" +
                                "<span class='chat-sender'>{1}</span>" +
                                "<span class='chat-text'>{2}</span>" +
                                "<span class='chat-time'>{3}</span>" +
                            "</div>",
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
                string role = "Manager";

                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    con.Open();
                    // Identify if user is Manager or Salesperson
                    role = GetUserRole(userCode, Convert.ToInt32(visitId), con);

                    // Insert Chat
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO tbl_SalesVisitResponses (VisitId, RespondentRole, RespondentCode, ResponseText, ResponseDate) VALUES (@VisitId, @Role, @Code, @Text, GETDATE())", con))
                    {
                        cmd.Parameters.AddWithValue("@VisitId", visitId);
                        cmd.Parameters.AddWithValue("@Role", role);
                        cmd.Parameters.AddWithValue("@Code", userCode);
                        cmd.Parameters.AddWithValue("@Text", comment);
                        cmd.ExecuteNonQuery();
                    }
                }

                // 📧 NEW: Trigger the Chat Email Notification
                SendChatEmailNotification(visitId, comment, userCode, role);
            }

            // Refresh UI
            txtMegaNewComment.Text = "";
            Binder(); // Update the background grid
            LoadMegaModal(visitId); // Reload the modal data

            // Re-open the modal directly to the Action/Chat tab
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowMegaChat", "showMegaModal('tabAction');", true);
        }

        private void SendChatEmailNotification(string visitId, string commentText, string senderUserCode, string senderRole)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    conn.Open();

                    // Fetch Routing Emails
                    string query = @"
                        SELECT 
                            SVR.CustomerName, 
                            SVR.Salesperson, 
                            Creator.Email AS SalespersonEmail,
                            Manager.Email AS ManagerEmail
                        FROM tbl_SalesVisitReport SVR 
                        INNER JOIN tbl_login Creator ON Creator.User_Id = SVR.CreatedByCode 
                        LEFT JOIN tbl_login Manager ON Manager.User_Id = Creator.ReportingManagerId
                        WHERE SVR.Id = @Id";

                    string emailTo = "";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", visitId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (senderRole == "Manager")
                                {
                                    emailTo = reader["SalespersonEmail"]?.ToString();
                                }
                                else if (senderRole == "Salesperson")
                                {
                                    emailTo = reader["ManagerEmail"]?.ToString();
                                }
                            }
                        }
                    }

                    if (string.IsNullOrWhiteSpace(emailTo) || !System.Text.RegularExpressions.Regex.IsMatch(emailTo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        return;

                    // Fetch the Uniform HTML Data (The big table + chat history)
                    string dataRichHtml = GetVisitEmailBody(visitId, conn);

                    // Determine the headers based on who is sending it
                    string roleDisplay = senderRole == "Salesperson" ? "Salesperson Reply" : "Manager Reply";

                    // Build the exact Email Layout from the PDF/Screenshot
                    string body = $@"<html><body style='font-family: Arial, sans-serif; color: #333;'>
                            
                            <h2 style='color: #0056b3; margin-bottom: 15px;'>Sales Visit Report &ndash; {roleDisplay}</h2>
                            
                            <div style='margin-bottom: 20px;'>
                                {dataRichHtml}
                            </div>
                            
                            <div style='border: 2px solid #0056b3; padding: 15px; margin-top: 20px; max-width: 800px;'>
                                <h3 style='color: #0056b3; margin-top: 0; margin-bottom: 15px;'>{roleDisplay}</h3>
                                <p style='margin: 0 0 10px 0;'><b>Reply Date:</b> {DateTime.Now.ToString("dd-MMM-yyyy HH:mm tt")}</p>
                                <p style='margin: 0;'>{commentText.Replace(Environment.NewLine, "<br/>")}</p>
                            </div>
                            
                            </body></html>";

                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress("it.support@aminruptechnologies.co.in", "Flame-Ex : Sales Reporting Mailer");
                        mail.To.Add(emailTo);
                        mail.Subject = $"Sales Visit Report - {roleDisplay}";
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
            catch (Exception) { /* Fails silently to protect UI */ }
        }

        private string GetUserRole(string userId, int visitId, SqlConnection con)
        {
            string role = "Manager";
            using (SqlCommand cmdCheck = new SqlCommand("SELECT CreatedByCode FROM tbl_SalesVisitReport WHERE Id=@Id", con))
            {
                cmdCheck.Parameters.AddWithValue("@Id", visitId);
                object creator = cmdCheck.ExecuteScalar();
                if (creator != null && creator.ToString() == userId)
                {
                    role = "Salesperson";
                }
            }
            return role;
        }


        // ==========================================
        // TAB 4: APPROVAL / REJECTION LOGIC
        // ==========================================

        protected void btnMegaApprove_Click(object sender, EventArgs e)
        {
            ProcessApproval("Approved");
        }

        protected void btnMegaReject_Click(object sender, EventArgs e)
        {
            ProcessApproval("Rejected");
        }

        private void ProcessApproval(string status)
        {
            string visitId = hfMegaVisitId.Value;
            string remarks = txtMegaRemarks.Text.Trim();
            string user = Session["USERID"].ToString();

            if (string.IsNullOrEmpty(visitId)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    conn.Open();

                    // 1. Update the Visit Report ONLY
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
                        cmd.Parameters.AddWithValue("@Id", visitId);
                        cmd.ExecuteNonQuery();
                    }

                    // (REMOVED the bulk tbl_Expenses update from here)
                }

                // Send Email Notification
                SendApprovalNotification(visitId, status, remarks, user);

                // Refresh UI and Close Modal
                Binder();
                lblOk.Text = $"Visit #{visitId} has been successfully {status}.";
                PanelOK.Visible = true;
                ScriptManager.RegisterStartupScript(this, GetType(), "HideMega", "hideMegaModal();", true);
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error processing approval: " + ex.Message;
                PanelError.Visible = true;
            }
        }

        private void SendApprovalNotification(string visitId, string status, string remarks, string user)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    conn.Open();
                    string visitQuery = @"SELECT SVR.Salesperson, L.Email FROM tbl_SalesVisitReport SVR INNER JOIN tbl_login L ON L.User_Id = SVR.CreatedByCode WHERE SVR.Id = @Id";
                    string emailTo = "";
                    string salesperson = "";

                    using (SqlCommand cmd = new SqlCommand(visitQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", visitId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                emailTo = reader["Email"].ToString();
                                salesperson = reader["Salesperson"].ToString();
                            }
                        }
                    }

                    if (string.IsNullOrWhiteSpace(emailTo) || !System.Text.RegularExpressions.Regex.IsMatch(emailTo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        return;

                    // Fetch the Uniform HTML Data
                    string dataRichHtml = GetVisitEmailBody(visitId, conn);

                    string statusColor = status == "Approved" ? "green" : "red";
                    string body = $@"<html><body style='font-family: Arial, sans-serif;'>
                            <h2 style='color: #19658A;'>Sales Visit & Expenses: <span style='color:{statusColor};'>{status}</span></h2>
                            {dataRichHtml}
                            <p style='color:#666; font-size:13px;'><i>Note: Any expenses linked to this visit have also been updated to {status}.</i></p>
                            <br/>Regards,<br/><b>Flame-Ex ERP System</b></body></html>";

                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress("it.support@aminruptechnologies.co.in", "Flame-Ex : Sales Reporting Mailer");
                        mail.To.Add(emailTo);
                        mail.Subject = $"Sales Visit & Expenses - {status} (ID: {visitId})";
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
            catch (Exception) { /* Fails silently to protect UI */ }
        }

        protected void gvMegaExpenses_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ApproveExp" || e.CommandName == "RejectExp")
            {
                string expId = e.CommandArgument.ToString();
                string status = e.CommandName == "ApproveExp" ? "Approved" : "Rejected";
                string user = Session["USERID"].ToString();

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    conn.Open();
                    string query = "UPDATE tbl_Expenses SET ApprovalStatus = @Status, ApprovedBy = @User, ApprovedDate = GETDATE() WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@User", user);
                        cmd.Parameters.AddWithValue("@Id", expId);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Rebind just the expense grid so the UI updates instantly
                BindExpenses(hfMegaVisitId.Value);

                // Keep the modal open and stay on the Expenses tab
                ScriptManager.RegisterStartupScript(this, GetType(), "StayOnExpTab", "showMegaModal('tabExpenses');", true);
            }
        }

        private void BindExpenses(string visitId)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                // Added Id and ApprovalStatus to the query
                using (SqlCommand cmdExp = new SqlCommand("SELECT Id, ExpenseDate, ExpenseCategory, Description, Amount, AttachmentName, ApprovalStatus FROM tbl_Expenses WHERE VisitId = @Id", con))
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
            }
        }

        // NEW HELPER: Generates the uniform, data-rich HTML body for Visit Emails
        private string GetVisitEmailBody(string visitId, SqlConnection con)
        {
            string htmlDetails = "";

            // 1. Fetch Core Visit Details
            string visitQuery = @"
                SELECT v.*, mgr.Name AS ApprovedByName 
                FROM tbl_SalesVisitReport v 
                LEFT JOIN tbl_login mgr ON v.ApprovedBy = mgr.User_Id 
                WHERE v.Id = @Id";

            using (SqlCommand cmd = new SqlCommand(visitQuery, con))
            {
                cmd.Parameters.AddWithValue("@Id", visitId);
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        string attachmentLink = string.IsNullOrEmpty(rdr["AttachmentName"].ToString()) ? "N/A"
                            : $"<a href='https://www.exc.aagroupindia.com/Uploads/{rdr["AttachmentName"]}' target='_blank'>📎 View Document</a>";

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

            // 2. Fetch Conversation History
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
                        htmlDetails += "<br/><h4 style='font-family:Arial,sans-serif; color:#19658A; margin-bottom: 5px; border-bottom: 1px solid #eee; padding-bottom: 5px; max-width: 800px;'>Conversations / Comments:</h4>";
                        htmlDetails += "<div style='font-family:Arial,sans-serif; font-size:14px; max-width: 800px;'>";

                        while (rdrC.Read())
                        {
                            bool isManager = rdrC["RespondentRole"].ToString().Equals("Manager", StringComparison.OrdinalIgnoreCase);
                            string align = isManager ? "right" : "left";
                            string bgColor = isManager ? "#e1f5fe" : "#fce4ec";
                            string name = rdrC["RespondentName"].ToString();
                            string text = rdrC["ResponseText"].ToString().Replace(Environment.NewLine, "<br/>");
                            string date = Convert.ToDateTime(rdrC["ResponseDate"]).ToString("dd-MMM-yyyy HH:mm");

                            htmlDetails += $@"
                            <div style='text-align:{align}; margin:8px 0;'>
                                <div style='display:inline-block; text-align:left; background-color:{bgColor}; padding:10px 15px; border-radius:8px; max-width:75%; border: 1px solid #ddd;'>
                                    <b style='color:#19658A; font-size:12px;'>{name}</b> <small style='color:#666;'>({date})</small><br/>
                                    <div style='margin-top:4px; color:#333; line-height: 1.4;'>{text}</div>
                                </div>
                            </div>";
                        }
                        htmlDetails += "</div>";
                    }
                }
            }
            return htmlDetails;
        }
    }
}