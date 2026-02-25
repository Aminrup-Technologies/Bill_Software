using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.Services;
using System.Web.Script.Serialization; // Required for JSON serialization
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

        // The WebMethod attribute makes this callable from JavaScript AJAX
        [WebMethod(EnableSession = true)]
        public static string GetCalendarEvents()
        {
            string userId = HttpContext.Current.Session["USERID"]?.ToString();
            if (string.IsNullOrEmpty(userId)) return "[]";

            List<CalendarEvent> eventsList = new List<CalendarEvent>();
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Query planned and executed visits for the logged-in user
                string query = @"SELECT Id, VisitDate, CustomerName, VisitPhase 
                                 FROM tbl_SalesVisitReport 
                                 WHERE CreatedByCode = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            string phase = rdr["VisitPhase"].ToString();
                            DateTime vDate = Convert.ToDateTime(rdr["VisitDate"]);

                            // Create an event object that FullCalendar understands
                            eventsList.Add(new CalendarEvent
                            {
                                id = rdr["Id"].ToString(),
                                title = rdr["CustomerName"].ToString(),
                                start = vDate.ToString("yyyy-MM-dd"), // FullCalendar requires ISO format dates
                                className = (phase == "Planned") ? "event-planned" : "event-executed",
                                visitPhase = phase // Extended property to handle clicks in JS
                            });
                        }
                    }
                }
            }

            // Convert the C# List into a JSON string to send back to the browser
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(eventsList);
        }

        // A simple class mapping exactly to FullCalendar's required JSON properties
        public class CalendarEvent
        {
            public string id { get; set; }
            public string title { get; set; }
            public string start { get; set; }
            public string className { get; set; } // Used to assign CSS colors
            public string visitPhase { get; set; } // Custom property 
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
                    // We update the existing record with the execution details and GPS coords
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
                WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", visitId);
                        cmd.Parameters.AddWithValue("@Latitude", string.IsNullOrEmpty(latitude) ? (object)DBNull.Value : Convert.ToDecimal(latitude));
                        cmd.Parameters.AddWithValue("@Longitude", string.IsNullOrEmpty(longitude) ? (object)DBNull.Value : Convert.ToDecimal(longitude));

                        cmd.Parameters.AddWithValue("@DiscussionPoints", txtExecDiscussion.Text.Trim());
                        cmd.Parameters.AddWithValue("@Status", ddlExecStatus.SelectedValue);
                        cmd.Parameters.AddWithValue("@FollowUpRequired", ddlExecFollowUp.SelectedValue);

                        if (!string.IsNullOrEmpty(txtExecNextDate.Text))
                            cmd.Parameters.AddWithValue("@NextFollowUpDate", Convert.ToDateTime(txtExecNextDate.Text.Trim()));
                        else
                            cmd.Parameters.AddWithValue("@NextFollowUpDate", DBNull.Value);

                        // Handle File Upload securely
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

                // Optional: If Follow-Up is 'Yes', automatically create a new 'Planned' visit here!

                // Clear the form and reload the page to refresh the calendar
                txtExecDiscussion.Text = "";
                txtExecNextDate.Text = "";
                Response.Redirect(Request.RawUrl); // Reloads the page so the calendar fetches updated colors
            }
            catch (Exception ex)
            {
                // Handle your error logging here
                Response.Write("<script>alert('Error: " + ex.Message + "');</script>");
            }
        }

        [System.Web.Services.WebMethod(EnableSession = true)]
        public static string GetVisitDetails(int visitId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Added all relevant fields including Latitude and Longitude
                string query = @"SELECT CustomerName, VisitDate, ExecutionDateTime, DiscussionPoints, Status, 
                                Salesperson, Department, ContactPerson, VisitType, FollowUpRequired, 
                                NextFollowUpDate, AttachmentName, Latitude, Longitude
                         FROM tbl_SalesVisitReport WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", visitId);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            // Map database fields to an anonymous object
                            var visit = new
                            {
                                CustomerName = rdr["CustomerName"].ToString(),
                                Salesperson = rdr["Salesperson"].ToString(),
                                Department = rdr["Department"].ToString(),
                                ContactPerson = rdr["ContactPerson"].ToString(),
                                VisitType = rdr["VisitType"].ToString(),
                                VisitDate = Convert.ToDateTime(rdr["VisitDate"]).ToString("dd-MMM-yyyy"),
                                ExecutionDate = rdr["ExecutionDateTime"] != DBNull.Value ? Convert.ToDateTime(rdr["ExecutionDateTime"]).ToString("dd-MMM-yyyy hh:mm tt") : "N/A",
                                DiscussionPoints = rdr["DiscussionPoints"].ToString(),
                                FollowUpRequired = rdr["FollowUpRequired"].ToString(),
                                NextFollowUpDate = rdr["NextFollowUpDate"] != DBNull.Value ? Convert.ToDateTime(rdr["NextFollowUpDate"]).ToString("dd-MMM-yyyy") : "N/A",
                                Status = rdr["Status"].ToString(),
                                AttachmentName = rdr["AttachmentName"].ToString(),
                                // Handle Lat/Long safely
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