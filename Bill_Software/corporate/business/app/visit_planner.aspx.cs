/*
-----------------------------------------------------------------------------------------
-- File Name: visit_planner.aspx.cs
-- When:      2026-03-22
-- Why:       To output accurate start/end times to the calendar and establish the 
--            ParentVisitId linkage for automated follow-up visits.
-- What:      1. Updated `GetCalendarEvents` to return `end` properties.
--            2. Modified `btnSubmitExecution_Click` to insert `ParentVisitId` and 
--               `VisitEndDate` into `tbl_SalesVisitReport`.
-----------------------------------------------------------------------------------------
*/
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.Services;
using System.Web.Script.Serialization;
using System.IO;

namespace Bill_Software.corporate.business.app
{
    public partial class visit_planner : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
        }

        [WebMethod(EnableSession = true)]
        public static string GetCalendarEvents()
        {
            string userId = HttpContext.Current.Session["USERID"]?.ToString();
            if (string.IsNullOrEmpty(userId)) return "[]";

            List<CalendarEvent> eventsList = new List<CalendarEvent>();
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT Id, VisitDate, VisitEndDate, CustomerName, VisitPhase 
                                 FROM tbl_SalesVisitReport 
                                 WHERE CreatedByCode = @UserId AND CompanyID = @CompanyID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            string phase = rdr["VisitPhase"].ToString();
                            DateTime vDate = Convert.ToDateTime(rdr["VisitDate"]);

                            // Handle potential NULL end dates gracefully
                            DateTime? vEndDate = rdr["VisitEndDate"] != DBNull.Value
                                ? Convert.ToDateTime(rdr["VisitEndDate"])
                                : (DateTime?)null;

                            eventsList.Add(new CalendarEvent
                            {
                                id = rdr["Id"].ToString(),
                                title = rdr["CustomerName"].ToString(),
                                start = vDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                end = vEndDate?.ToString("yyyy-MM-ddTHH:mm:ss"), // Passes End Time to calendar
                                className = (phase == "Planned") ? "event-planned" : "event-executed",
                                visitPhase = phase
                            });
                        }
                    }
                }
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(eventsList);
        }

        public class CalendarEvent
        {
            public string id { get; set; }
            public string title { get; set; }
            public string start { get; set; }
            public string end { get; set; } // Added End property for duration spanning
            public string className { get; set; }
            public string visitPhase { get; set; }
        }

        protected void btnSubmitExecution_Click(object sender, EventArgs e)
        {
            try
            {
                int visitId = Convert.ToInt32(hfExecuteVisitId.Value);
                string latitude = hfLatitude.Value;
                string longitude = hfLongitude.Value;

                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    // 1. Update Current Execution
                    // 2. Automatically spawn the next Follow-Up visit with ParentVisitId linkage
                    string query = @"
                UPDATE tbl_SalesVisitReport 
                SET VisitPhase = 'Executed', 
                    ExecutionDateTime = GETDATE(), 
                    Latitude = @Latitude, 
                    Longitude = @Longitude,
                    DiscussionPoints = @DiscussionPoints,
                    Status = @Status,
                    FollowUpRequired = @FollowUpRequired,
                    NextFollowUpDate = @NextFollowUpDate,
                    AttachmentName = ISNULL(@AttachmentName, AttachmentName)
                WHERE Id = @Id;

                -- AUTO FOLLOW-UP LOGIC WITH PARENT LINKAGE
                IF @FollowUpRequired = 'Yes' AND @NextFollowUpDate IS NOT NULL
                BEGIN
                    INSERT INTO tbl_SalesVisitReport (
                        VisitDate, VisitEndDate, Salesperson, CustomerName, Department, ContactPerson, 
                        VisitType, DiscussionPoints, VisitPhase, Status, FollowUpRequired, 
                        CreatedDate, CreatedByCode, ParentVisitId, CompanyID
                    )
                    SELECT 
                        @NextFollowUpDate, DATEADD(hour, 1, @NextFollowUpDate), Salesperson, CustomerName, Department, ContactPerson, 
                        VisitType, 'Automated Follow-up regarding: ' + @DiscussionPoints, 
                        'Planned', 'Pending', 'No', GETDATE(), CreatedByCode, @Id, @CompanyID
                    FROM tbl_SalesVisitReport 
                    WHERE Id = @Id AND CompanyID = @CompanyID;
                END";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", visitId);
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        cmd.Parameters.AddWithValue("@Latitude", string.IsNullOrEmpty(latitude) ? (object)DBNull.Value : Convert.ToDecimal(latitude));
                        cmd.Parameters.AddWithValue("@Longitude", string.IsNullOrEmpty(longitude) ? (object)DBNull.Value : Convert.ToDecimal(longitude));

                        cmd.Parameters.AddWithValue("@DiscussionPoints", txtExecDiscussion.Text.Trim());
                        cmd.Parameters.AddWithValue("@Status", ddlExecStatus.SelectedValue);
                        cmd.Parameters.AddWithValue("@FollowUpRequired", ddlExecFollowUp.SelectedValue);

                        if (!string.IsNullOrEmpty(txtExecNextDate.Text))
                            cmd.Parameters.AddWithValue("@NextFollowUpDate", Convert.ToDateTime(txtExecNextDate.Text.Trim()));
                        else
                            cmd.Parameters.AddWithValue("@NextFollowUpDate", DBNull.Value);

                        string fileName = null;
                        if (fileExecAttachment.HasFile)
                        {
                            string datePrefix = DateTime.Now.ToString("yyyyMMddHHmmss");
                            fileName = datePrefix + "_" + Path.GetFileName(fileExecAttachment.FileName);
                            string uploadPath = Server.MapPath("~/Uploads/");
                            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                            fileExecAttachment.SaveAs(Path.Combine(uploadPath, fileName));
                        }
                        cmd.Parameters.AddWithValue("@AttachmentName", (object)fileName ?? DBNull.Value);

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
                            notifCmd.Parameters.AddWithValue("@Title", "Visit Executed");
                            notifCmd.Parameters.AddWithValue("@Message", $"Visit #{visitId} was executed by {Session["USERID"]}. Follow-up: {ddlExecFollowUp.SelectedValue}.");
                            notifCmd.Parameters.AddWithValue("@Module", "Sales Visit");
                            notifCmd.Parameters.AddWithValue("@Type", "Success");
                            notifCmd.Parameters.AddWithValue("@UserId", Session["USERID"]);
                            logConn.Open();
                            notifCmd.ExecuteNonQuery();
                        }
                    }
                }
                catch { /* Soft catch: don't crash main transaction if logging fails */ }

                txtExecDiscussion.Text = "";
                txtExecNextDate.Text = "";
                Response.Redirect(Request.RawUrl);
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('An error occurred while saving the visit. Please try again.');</script>");
            }
        }

        [System.Web.Services.WebMethod(EnableSession = true)]
        public static string GetVisitDetails(int visitId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT CustomerName, VisitDate, ExecutionDateTime, DiscussionPoints, Status, 
                                Salesperson, Department, ContactPerson, VisitType, FollowUpRequired, 
                                NextFollowUpDate, AttachmentName, Latitude, Longitude
                         FROM tbl_SalesVisitReport WHERE Id = @Id AND CompanyID = @CompanyID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", visitId);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            var visit = new
                            {
                                CustomerName = rdr["CustomerName"].ToString(),
                                Salesperson = rdr["Salesperson"].ToString(),
                                Department = rdr["Department"].ToString(),
                                ContactPerson = rdr["ContactPerson"].ToString(),
                                VisitType = rdr["VisitType"].ToString(),
                                VisitDate = Convert.ToDateTime(rdr["VisitDate"]).ToString("dd-MMM-yyyy hh:mm tt"),
                                ExecutionDate = rdr["ExecutionDateTime"] != DBNull.Value ? Convert.ToDateTime(rdr["ExecutionDateTime"]).ToString("dd-MMM-yyyy hh:mm tt") : "N/A",
                                DiscussionPoints = rdr["DiscussionPoints"].ToString(),
                                FollowUpRequired = rdr["FollowUpRequired"].ToString(),
                                NextFollowUpDate = rdr["NextFollowUpDate"] != DBNull.Value ? Convert.ToDateTime(rdr["NextFollowUpDate"]).ToString("dd-MMM-yyyy hh:mm tt") : "N/A",
                                Status = rdr["Status"].ToString(),
                                AttachmentName = rdr["AttachmentName"].ToString(),
                                Latitude = rdr["Latitude"] != DBNull.Value ? rdr["Latitude"].ToString() : "",
                                Longitude = rdr["Longitude"] != DBNull.Value ? rdr["Longitude"].ToString() : ""
                            };

                            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
                            return js.Serialize(visit);
                        }
                    }
                }
            }
            return "{}";
        }
    }
}