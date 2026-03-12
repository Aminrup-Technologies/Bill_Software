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

                // Auto-fill the date passed from the calendar
                if (Request.QueryString["date"] != null)
                {
                    txtVisitDate.Text = Convert.ToDateTime(Request.QueryString["date"]).ToString("yyyy-MM-dd");
                }
                else
                {
                    txtVisitDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                }

                // UI Configuration based on Mode
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
            string cmdString = "select Name from tbl_login where User_Id='" + UserName + "'";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn);
            SqlDataReader Rdr = cmd.ExecuteReader();
            if (Rdr.Read())
            {
                txtSalesperson.Text = Rdr["Name"].ToString();
            }
            DbCL.Conn.Close();
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
                        (VisitDate, Salesperson, CustomerName, Department, ContactPerson, VisitType, DiscussionPoints, 
                         VisitPhase, Status, FollowUpRequired, NextFollowUpDate, AttachmentName, ExecutionDateTime, CreatedDate, CreatedByCode) 
                        VALUES 
                        (@VisitDate, @Salesperson, @CustomerName, @Department, @ContactPerson, @VisitType, @DiscussionPoints, 
                         @VisitPhase, @Status, @FollowUpRequired, @NextFollowUpDate, @AttachmentName, @ExecutionDateTime, @CreatedDate, @CreatedByCode)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@VisitDate", txtVisitDate.Text.Trim());
                        cmd.Parameters.AddWithValue("@Salesperson", txtSalesperson.Text.Trim());
                        cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Department", txtDepartment.Text.Trim());
                        cmd.Parameters.AddWithValue("@ContactPerson", txtContactPerson.Text.Trim());
                        cmd.Parameters.AddWithValue("@VisitType", ddlVisitType.SelectedValue);
                        cmd.Parameters.AddWithValue("@DiscussionPoints", txtDiscussion.Text.Trim());
                        cmd.Parameters.AddWithValue("@VisitPhase", visitPhase);

                        if (mode == "past")
                        {
                            // It is a past record, fill out the execution columns
                            cmd.Parameters.AddWithValue("@ExecutionDateTime", txtVisitDate.Text.Trim()); // Treat visit date as execution date
                            cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                            cmd.Parameters.AddWithValue("@FollowUpRequired", ddlFollowUp.SelectedValue);

                            if (!string.IsNullOrEmpty(txtNextFollowUp.Text))
                                cmd.Parameters.AddWithValue("@NextFollowUpDate", txtNextFollowUp.Text.Trim());
                            else
                                cmd.Parameters.AddWithValue("@NextFollowUpDate", DBNull.Value);

                            // Handle Attachment
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
                            // Planning mode: send nulls for execution metrics
                            cmd.Parameters.AddWithValue("@ExecutionDateTime", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Status", "Pending Execution");
                            cmd.Parameters.AddWithValue("@FollowUpRequired", "");
                            cmd.Parameters.AddWithValue("@NextFollowUpDate", DBNull.Value);
                            cmd.Parameters.AddWithValue("@AttachmentName", DBNull.Value);
                        }

                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Today);
                        string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";
                        cmd.Parameters.AddWithValue("@CreatedByCode", userId);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ScriptManager.RegisterStartupScript(this, GetType(), "redirect", "alert('Record saved successfully!'); window.location='visit_planner.aspx';", true);
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "An error occurred: " + ex.Message;
                PanelError.Visible = true;
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            Response.Redirect("visit_planner.aspx");
        }
    }
}