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
                // Auto-fill the date if they clicked it on the calendar!
                if (Request.QueryString["date"] != null)
                {
                    txtVisitDate.Text = Convert.ToDateTime(Request.QueryString["date"]).ToString("dd-MMM-yyyy");
                }
                else
                {
                    txtVisitDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                }
                GetAdminName();
            }
        }

        private void GetAdminName()
        {
            string UserName = Session["USERID"].ToString();
            string cmdString = "select Name from tbl_login where User_Id='" + UserName + "'";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn);
            SqlDataReader Rdr;
            Rdr = cmd.ExecuteReader();
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
                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    // Updated Query: Setting VisitPhase to 'Planned' and safely inserting NULLs for execution fields
                    string query = @"INSERT INTO tbl_SalesVisitReport 
                        (VisitDate, Salesperson, CustomerName, Department, ContactPerson, VisitType, DiscussionPoints, 
                         VisitPhase, Status, FollowUpRequired, NextFollowUpDate, AttachmentName, CreatedDate, CreatedByCode) 
                        VALUES 
                        (@VisitDate, @Salesperson, @CustomerName, @Department, @ContactPerson, @VisitType, @DiscussionPoints, 
                         'Planned', 'Pending Execution', '', NULL, NULL, @CreatedDate, @CreatedByCode)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@VisitDate", txtVisitDate.Text.Trim());
                        cmd.Parameters.AddWithValue("@Salesperson", txtSalesperson.Text.Trim());
                        cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Department", txtDepartment.Text.Trim());
                        cmd.Parameters.AddWithValue("@ContactPerson", txtContactPerson.Text.Trim());
                        cmd.Parameters.AddWithValue("@VisitType", ddlVisitType.SelectedValue);
                        cmd.Parameters.AddWithValue("@DiscussionPoints", txtDiscussion.Text.Trim()); // This is now Agenda

                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Today);
                        string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";
                        cmd.Parameters.AddWithValue("@CreatedByCode", userId);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                //lblOk.Text = "Sales visit planned successfully!";
                //PanelOK.Visible = true;
                //ClearForm();

                ClearForm();
                ScriptManager.RegisterStartupScript(this, GetType(), "redirect",
                    "alert('Sales visit planned successfully!'); window.location='visit_planner.aspx';", true);
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "An error occurred. Please try again later.";
                PanelError.Visible = true;
                LogErrorToFile(ex);
            }
        }

        private void LogErrorToFile(Exception ex)
        {
            try
            {
                string logPath = Server.MapPath("~/Logs/");
                if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);
                string logFile = Path.Combine(logPath, "ErrorLog.txt");

                using (StreamWriter writer = new StreamWriter(logFile, true))
                {
                    writer.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]");
                    writer.WriteLine("Message: " + ex.Message);
                    writer.WriteLine("StackTrace: " + ex.StackTrace);
                    writer.WriteLine("----------------------------------------");
                }
            }
            catch (Exception)
            {
                lblErrorMsg.Text = "An error occurred: " + ex.Message;
                PanelError.Visible = true;
            }
        }

        private void ClearForm()
        {
            txtVisitDate.Text = string.Empty;
            // Kept Salesperson intact as it is read-only
            txtCustomerName.Text = string.Empty;
            txtDepartment.Text = string.Empty;
            txtContactPerson.Text = string.Empty;
            ddlVisitType.SelectedIndex = 0;
            txtDiscussion.Text = string.Empty;
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ClearForm();
            PanelOK.Visible = false;
            PanelError.Visible = false;
        }
    }
}