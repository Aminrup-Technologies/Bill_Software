using System;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Data;

namespace Bill_Software.corporate.business.app
{
    public partial class expense_entry : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }

            if (!IsPostBack)
            {
                txtExpenseDate.Text = DateTime.Now.ToString("yyyy-MM-dd");

                if (Request.QueryString["visitId"] != null)
                {
                    int visitId;
                    if (int.TryParse(Request.QueryString["visitId"], out visitId))
                    {
                        LoadLinkedVisitDetails(visitId);
                        BindExpenses(visitId); // Load previously added expenses
                    }
                }
            }
        }

        private void LoadLinkedVisitDetails(int visitId)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = "SELECT CustomerName, VisitDate, DiscussionPoints FROM tbl_SalesVisitReport WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", visitId);
                        conn.Open();
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                trVisitInfo.Visible = true;
                                hfVisitId.Value = visitId.ToString();
                                lblLinkedCustomer.Text = rdr["CustomerName"].ToString();
                                lblLinkedDate.Text = Convert.ToDateTime(rdr["VisitDate"]).ToString("dd-MMM-yyyy");
                                lblLinkedOutcome.Text = rdr["DiscussionPoints"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error loading visit details: " + ex.Message;
                PanelError.Visible = true;
            }
        }

        private void BindExpenses(int visitId)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = @"SELECT ExpenseDate, ExpenseCategory, Description, Amount, ApprovalStatus 
                                     FROM tbl_Expenses WHERE VisitId = @VisitId ORDER BY Id DESC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@VisitId", visitId);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        trExpenseGrid.Visible = true;
                        gvExpenses.DataSource = dt;
                        gvExpenses.DataBind();
                    }
                }
            }
            catch (Exception)
            {
                // Optionally handle silently
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            PanelOK.Visible = false;
            PanelError.Visible = false;

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
                string userId = HttpContext.Current.Session["USERID"].ToString();

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = @"INSERT INTO tbl_Expenses 
                                    (UserCode, ExpenseDate, VisitId, ExpenseCategory, Amount, Description, AttachmentName, CreatedDate) 
                                     VALUES 
                                    (@UserCode, @ExpenseDate, @VisitId, @ExpenseCategory, @Amount, @Description, @AttachmentName, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserCode", userId);
                        cmd.Parameters.AddWithValue("@ExpenseDate", txtExpenseDate.Text.Trim());
                        cmd.Parameters.AddWithValue("@ExpenseCategory", ddlCategory.SelectedValue);
                        cmd.Parameters.AddWithValue("@Amount", Convert.ToDecimal(txtAmount.Text.Trim()));
                        cmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());

                        if (!string.IsNullOrEmpty(hfVisitId.Value))
                            cmd.Parameters.AddWithValue("@VisitId", Convert.ToInt32(hfVisitId.Value));
                        else
                            cmd.Parameters.AddWithValue("@VisitId", DBNull.Value);

                        string fileName = null;
                        if (fileReceipt.HasFile)
                        {
                            string datePrefix = DateTime.Now.ToString("yyyyMMddHHmmss");
                            fileName = "EXP_" + datePrefix + "_" + Path.GetFileName(fileReceipt.FileName);
                            string uploadPath = Server.MapPath("~/Uploads/Expenses/");
                            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                            fileReceipt.SaveAs(Path.Combine(uploadPath, fileName));
                        }
                        cmd.Parameters.AddWithValue("@AttachmentName", (object)fileName ?? DBNull.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // Success message tells them they can keep going
                lblOk.Text = "Expense saved! You can add another expense for this visit below.";
                PanelOK.Visible = true;

                // Clear the input fields so they can add the next one
                txtAmount.Text = "";
                txtDescription.Text = "";
                ddlCategory.SelectedIndex = 0;

                // Refresh the grid to show the newly added expense instantly
                if (!string.IsNullOrEmpty(hfVisitId.Value))
                {
                    BindExpenses(Convert.ToInt32(hfVisitId.Value));
                }
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "An error occurred: " + ex.Message;
                PanelError.Visible = true;
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("visit_planner.aspx");
        }
    }
}