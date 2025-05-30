using System;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Data;

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
                txtVisitDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
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
                    string query = @"INSERT INTO tbl_SalesVisitReport (VisitDate, Salesperson, CustomerName, Department, ContactPerson, VisitType, DiscussionPoints, FollowUpRequired, NextFollowUpDate, Status, AttachmentName, CreatedDate, CreatedByCode) VALUES (@VisitDate, @Salesperson, @CustomerName, @Department, @ContactPerson, @VisitType, @DiscussionPoints, @FollowUpRequired, @NextFollowUpDate, @Status, @AttachmentName, @CreatedDate, @CreatedByCode)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@VisitDate", txtVisitDate.Text.Trim());
                        cmd.Parameters.AddWithValue("@Salesperson", txtSalesperson.Text.Trim());
                        cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Department", ddlDepartment.SelectedValue);
                        cmd.Parameters.AddWithValue("@ContactPerson", txtContactPerson.Text.Trim());
                        cmd.Parameters.AddWithValue("@VisitType", ddlVisitType.SelectedValue);
                        cmd.Parameters.AddWithValue("@DiscussionPoints", txtDiscussion.Text.Trim());
                        cmd.Parameters.AddWithValue("@FollowUpRequired", ddlFollowUp.SelectedValue);

                        if (!string.IsNullOrEmpty(txtNextFollowUp.Text))
                            cmd.Parameters.AddWithValue("@NextFollowUpDate", txtNextFollowUp.Text.Trim());
                        else
                            cmd.Parameters.AddWithValue("@NextFollowUpDate", DBNull.Value);

                        cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);

                        //string fileName = null;
                        //if (fileAttachment.HasFile)
                        //{
                        //    fileName = Path.GetFileName(fileAttachment.FileName);
                        //    string uploadPath = Server.MapPath("~/Uploads/");
                        //    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                        //    fileAttachment.SaveAs(Path.Combine(uploadPath, fileName));
                        //}
                        //cmd.Parameters.AddWithValue("@AttachmentName", (object)fileName ?? DBNull.Value);

                        string fileName = null;

                        if (fileAttachment.HasFile)
                        {
                            string originalFileName = Path.GetFileName(fileAttachment.FileName);

                            // Add yyyymmdd date prefix
                            string datePrefix = DateTime.Now.ToString("yyyyMMdd");
                            fileName = datePrefix + "_" + originalFileName;

                            // Define upload path
                            string uploadPath = Server.MapPath("~/Uploads/");
                            if (!Directory.Exists(uploadPath))
                                Directory.CreateDirectory(uploadPath);

                            // Save file with new name
                            fileAttachment.SaveAs(Path.Combine(uploadPath, fileName));
                        }

                        // Pass to DB
                        cmd.Parameters.AddWithValue("@AttachmentName", (object)fileName ?? DBNull.Value);


                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Today);

                        string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";
                        cmd.Parameters.AddWithValue("@CreatedByCode", userId);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                lblOk.Text = "Sales visit report submitted successfully!";
                PanelOK.Visible = true;

                ClearForm();
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
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }

                string logFile = Path.Combine(logPath, "ErrorLog.txt");

                using (StreamWriter writer = new StreamWriter(logFile, true))
                {
                    writer.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]");
                    writer.WriteLine("Message: " + ex.Message);
                    writer.WriteLine("StackTrace: " + ex.StackTrace);
                    writer.WriteLine("----------------------------------------");
                }
            }
            catch (Exception )
            {
                lblErrorMsg.Text = "An error occurred :" + ex.Message;
                PanelError.Visible = true;
            }
        }

        private void ClearForm()
        {
            txtVisitDate.Text = string.Empty;
            txtSalesperson.Text = string.Empty;
            txtCustomerName.Text = string.Empty;
            ddlDepartment.SelectedIndex = 0;
            txtContactPerson.Text = string.Empty;
            ddlVisitType.SelectedIndex = 0;
            txtDiscussion.Text = string.Empty;
            ddlFollowUp.SelectedIndex = 0;
            txtNextFollowUp.Text = string.Empty;
            ddlStatus.SelectedIndex = 0;

            // Reset file upload by clearing file input (can't be done directly from server-side)
            // Suggest using JS for clearing file upload if needed visually

            // Hide success/error panels
            //PanelOK.Visible = false;
            //PanelError.Visible = false;

            //lblOk.Text = "";
            //lblErrorMsg.Text = "";
        }

        private void ClearFormNew()
        {
            txtVisitDate.Text = string.Empty;
            txtSalesperson.Text = string.Empty;
            txtCustomerName.Text = string.Empty;
            ddlDepartment.SelectedIndex = 0;
            txtContactPerson.Text = string.Empty;
            ddlVisitType.SelectedIndex = 0;
            txtDiscussion.Text = string.Empty;
            ddlFollowUp.SelectedIndex = 0;
            txtNextFollowUp.Text = string.Empty;
            ddlStatus.SelectedIndex = 0;

            // Reset file upload by clearing file input (can't be done directly from server-side)
            // Suggest using JS for clearing file upload if needed visually

            // Hide success/error panels
            PanelOK.Visible = false;
            PanelError.Visible = false;

            lblOk.Text = "";
            lblErrorMsg.Text = "";
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ClearFormNew();
        }
    }
}