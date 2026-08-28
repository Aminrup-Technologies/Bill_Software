/*
-----------------------------------------------------------------------------------------
-- File Name: daily_rpt.aspx.cs
-- When:      2026-03-22
-- Why:       To fix a bug where `DateTime.TryParse` failed due to URL-decoded timezone 
--            offsets (the '+' turning into a space). Added string sanitization and a 
--            fallback to ensure the datetime-local fields never load empty.
-- What:      Added `.Substring(0, 16)` extraction to `startQs` and `endQs` inside the 
--            `Page_Load` method to cleanly parse "yyyy-MM-ddTHH:mm". Added an `else` 
--            block for Parse failures.
-----------------------------------------------------------------------------------------
*/
using System;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;

namespace Bill_Software.corporate.business.app
{
    public partial class daily_rpt : System.Web.UI.Page
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
                GetAdminName();
                string mode = Request.QueryString["mode"] ?? "plan";

                // Backward compatibility for 'date' param, but prefer 'start' and 'end'
                string startQs = Request.QueryString["start"] ?? Request.QueryString["date"];
                string endQs = Request.QueryString["end"];

                if (!string.IsNullOrEmpty(startQs))
                {
                    // Sanitize strings to handle URL timezone encoding issues 
                    // (e.g. "+05:30" becoming " 05:30" and breaking TryParse)
                    // We extract just the "yyyy-MM-ddTHH:mm" portion (16 characters)
                    if (startQs.Contains("T") && startQs.Length >= 16)
                    {
                        startQs = startQs.Substring(0, 16);
                    }

                    DateTime parsedStart;
                    if (DateTime.TryParse(startQs, out parsedStart))
                    {
                        // If only a date was sent (Month view click), default to 9:00 AM
                        if (startQs.Length <= 10 && !startQs.Contains("T"))
                        {
                            parsedStart = parsedStart.AddHours(9);
                        }
                        txtVisitStart.Text = parsedStart.ToString("yyyy-MM-ddTHH:mm");

                        // Parse End Time
                        if (!string.IsNullOrEmpty(endQs))
                        {
                            if (endQs.Contains("T") && endQs.Length >= 16)
                            {
                                endQs = endQs.Substring(0, 16);
                            }

                            DateTime parsedEnd;
                            if (DateTime.TryParse(endQs, out parsedEnd))
                            {
                                // If Month view, FullCalendar sends end date as the NEXT day at 00:00.
                                // We override this to just be a 1-hour block on the chosen start day.
                                if (endQs.Length <= 10 || (parsedEnd.Hour == 0 && parsedEnd.Minute == 0))
                                {
                                    txtVisitEnd.Text = parsedStart.AddHours(1).ToString("yyyy-MM-ddTHH:mm");
                                }
                                else
                                {
                                    txtVisitEnd.Text = parsedEnd.ToString("yyyy-MM-ddTHH:mm");
                                }
                            }
                            else
                            {
                                txtVisitEnd.Text = parsedStart.AddHours(1).ToString("yyyy-MM-ddTHH:mm");
                            }
                        }
                        else
                        {
                            // Fallback 1-hour duration if no end string is passed
                            txtVisitEnd.Text = parsedStart.AddHours(1).ToString("yyyy-MM-ddTHH:mm");
                        }
                    }
                    else
                    {
                        // Safe Fallback: If parsing entirely fails, don't leave it blank
                        txtVisitStart.Text = DateTime.Now.ToString("yyyy-MM-ddTHH:mm");
                        txtVisitEnd.Text = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:mm");
                    }
                }
                else
                {
                    // Default to current time, 1-hour duration for manual entries
                    DateTime now = DateTime.Now;
                    txtVisitStart.Text = now.ToString("yyyy-MM-ddTHH:mm");
                    txtVisitEnd.Text = now.AddHours(1).ToString("yyyy-MM-ddTHH:mm");
                }

                if (mode == "past")
                {
                    lblPageTitle.Text = "Log Past Executed Visit";
                    lblDiscussionLabel.Text = "Visit Outcome / Discussion Points";
                    pnlExecution.Visible = true;
                }
                else
                {
                    lblPageTitle.Text = "Plan Future Sales Visit";
                    lblDiscussionLabel.Text = "Agenda / Purpose";
                    pnlExecution.Visible = false;
                }
            }
        }

        private void GetAdminName()
        {
            string UserName = Session["USERID"].ToString();
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("SELECT Name FROM tbl_login WHERE User_Id = @UserId", conn))
            {
                cmd.Parameters.AddWithValue("@UserId", UserName);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        txtSalesperson.Text = rdr["Name"].ToString();
                    }
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            PanelOK.Visible = false;
            PanelError.Visible = false;

            try
            {
                string mode = Request.QueryString["mode"] ?? "plan";
                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
                string visitPhase = (mode == "past") ? "Executed" : "Planned";

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = @"INSERT INTO tbl_SalesVisitReport 
                        (VisitDate, VisitEndDate, Salesperson, CustomerName, Department, ContactPerson, VisitType, DiscussionPoints, 
                         VisitPhase, Status, FollowUpRequired, NextFollowUpDate, AttachmentName, ExecutionDateTime, CreatedDate, CreatedByCode, CompanyID) 
                        VALUES 
                        (@VisitDate, @VisitEndDate, @Salesperson, @CustomerName, @Department, @ContactPerson, @VisitType, @DiscussionPoints, 
                         @VisitPhase, @Status, @FollowUpRequired, @NextFollowUpDate, @AttachmentName, @ExecutionDateTime, @CreatedDate, @CreatedByCode, @CompanyID)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@VisitDate", Convert.ToDateTime(txtVisitStart.Text.Trim()));
                        cmd.Parameters.AddWithValue("@VisitEndDate", Convert.ToDateTime(txtVisitEnd.Text.Trim()));

                        cmd.Parameters.AddWithValue("@Salesperson", txtSalesperson.Text.Trim());
                        cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Department", txtDepartment.Text.Trim());
                        cmd.Parameters.AddWithValue("@ContactPerson", txtContactPerson.Text.Trim());
                        cmd.Parameters.AddWithValue("@VisitType", ddlVisitType.SelectedValue);
                        cmd.Parameters.AddWithValue("@DiscussionPoints", txtDiscussion.Text.Trim());
                        cmd.Parameters.AddWithValue("@VisitPhase", visitPhase);

                        if (mode == "past")
                        {
                            // If retroactively logging, execution time is equal to the Visit Start time
                            cmd.Parameters.AddWithValue("@ExecutionDateTime", Convert.ToDateTime(txtVisitStart.Text.Trim()));
                            cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                            cmd.Parameters.AddWithValue("@FollowUpRequired", ddlFollowUp.SelectedValue);

                            if (!string.IsNullOrEmpty(txtNextFollowUp.Text))
                                cmd.Parameters.AddWithValue("@NextFollowUpDate", Convert.ToDateTime(txtNextFollowUp.Text.Trim()));
                            else
                                cmd.Parameters.AddWithValue("@NextFollowUpDate", DBNull.Value);

                            string fileName = null;
                            if (fileAttachment.HasFile)
                            {
                                string datePrefix = DateTime.Now.ToString("yyyyMMddHHmmss");
                                fileName = datePrefix + "_" + Path.GetFileName(fileAttachment.FileName);
                                string uploadPath = Server.MapPath("~/Uploads/");
                                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                                fileAttachment.SaveAs(Path.Combine(uploadPath, fileName));
                            }
                            cmd.Parameters.AddWithValue("@AttachmentName", (object)fileName ?? DBNull.Value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@ExecutionDateTime", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Status", "Pending Execution");
                            cmd.Parameters.AddWithValue("@FollowUpRequired", "");
                            cmd.Parameters.AddWithValue("@NextFollowUpDate", DBNull.Value);
                            cmd.Parameters.AddWithValue("@AttachmentName", DBNull.Value);
                        }

                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Today);
                        string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";
                        cmd.Parameters.AddWithValue("@CreatedByCode", userId);
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // Proactive notification logging
                try
                {
                    using (SqlConnection logConn = new SqlConnection(connStr))
                    {
                        string notifQuery = @"INSERT INTO tbl_SystemNotification 
                            (CompanyID, Title, Message, Module, Type, UserId, CreatedOn) 
                            VALUES (@CompanyID, @Title, @Message, @Module, @Type, @UserId, GETDATE())";
                        using (SqlCommand notifCmd = new SqlCommand(notifQuery, logConn))
                        {
                            notifCmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                            notifCmd.Parameters.AddWithValue("@Title", $"Visit {visitPhase}");
                            notifCmd.Parameters.AddWithValue("@Message", $"New {visitPhase.ToLower()} visit created for {txtCustomerName.Text.Trim()} by {userId}.");
                            notifCmd.Parameters.AddWithValue("@Module", "Sales Visit");
                            notifCmd.Parameters.AddWithValue("@Type", "Info");
                            notifCmd.Parameters.AddWithValue("@UserId", userId);
                            logConn.Open();
                            notifCmd.ExecuteNonQuery();
                        }
                    }
                }
                catch { /* Soft catch: audit logging failure must not crash main transaction */ }

                ScriptManager.RegisterStartupScript(this, GetType(), "redirect", "alert('Record saved successfully!'); window.location='visit_planner.aspx';", true);
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "An error occurred while saving the visit. Please try again.";
                PanelError.Visible = true;
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            Response.Redirect("visit_planner.aspx");
        }
    }
}