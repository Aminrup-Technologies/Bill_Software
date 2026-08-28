using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm68 : System.Web.UI.Page
    {
        string ConnString { get { return ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
                return;
            }
            if (!IsPostBack)
                Binddata();
        }

        private void Binddata()
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT id, ProductOrServiceCat FROM tbl_NewparentProduct WHERE CompanyID = @CompanyID ORDER BY id", conn))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    DataList1.DataSource = rdr;
                    DataList1.DataBind();
                }
            }
        }

        private bool CheckDuplicate(string categoryName, SqlConnection conn, SqlTransaction trans)
        {
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT COUNT(1) FROM tbl_NewparentProduct
                  WHERE CompanyID = @CompanyID AND LTRIM(RTRIM(ProductOrServiceCat)) = @Name", conn, trans))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cmd.Parameters.AddWithValue("@Name", categoryName);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private void InsertSystemNotification(string title, string message, string severity, SqlConnection conn, SqlTransaction trans)
        {
            string sql = @"INSERT INTO tbl_SystemNotification
                           (Title, Message, ModuleCode, Severity, StartDate, EndDate, IsActive, CreatedBy, CompanyID)
                           VALUES
                           (@Title, @Msg, @Mod, @Severity, GETDATE(), DATEADD(day, 30, GETDATE()), 1, @User, @Comp)";
            using (SqlCommand cmd = new SqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Msg", message);
                cmd.Parameters.AddWithValue("@Mod", "CATEGORY");
                cmd.Parameters.AddWithValue("@Severity", severity);
                cmd.Parameters.AddWithValue("@User", Session["USERID"] != null ? Session["USERID"].ToString() : "System");
                cmd.Parameters.AddWithValue("@Comp", CompanyContext.CurrentCompanyID);
                cmd.ExecuteNonQuery();
            }
        }

        private void ShowOk(string msg)
        {
            PanelError.Visible = true;
            PanelError.Style["display"] = "none";
            PanelOK.Visible = true;
            PanelOK.Style["display"] = "block";
            lblOk.Text = msg;
        }

        private void ShowErr(string msg)
        {
            PanelOK.Visible = true;
            PanelOK.Style["display"] = "none";
            PanelError.Visible = true;
            PanelError.Style["display"] = "block";
            lblErrorMsg.Text = msg;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string name = (txtParentProducts.Text ?? string.Empty).Trim();
            if (name.Length == 0)
            {
                ShowErr("Provide Products Name.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        if (CheckDuplicate(name, conn, trans))
                        {
                            trans.Rollback();
                            ShowErr("Category already exists for this company.");
                            return;
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            @"INSERT INTO tbl_NewparentProduct (ProductOrServiceCat, CompanyID)
                              VALUES (@ProductOrServiceCat, @CompanyID)", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@ProductOrServiceCat", name);
                            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                            cmd.ExecuteNonQuery();
                        }

                        InsertSystemNotification(
                            "Category Created",
                            "Category '" + name + "' was created.",
                            "Success",
                            conn, trans);

                        trans.Commit();
                        txtParentProducts.Text = string.Empty;
                        ShowOk("Data Save Successfully...");
                        Binddata();
                    }
                    catch (Exception ex)
                    {
                        try { trans.Rollback(); } catch { }
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        ShowErr("An error occurred while saving the record. Please try again.");
                    }
                }
            }
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Id = Convert.ToString(e.CommandArgument);
            if (e.CommandName != "Delete")
            {
                Binddata();
                return;
            }

            int idVal;
            if (!int.TryParse(Id, out idVal))
            {
                ShowErr("Invalid category id.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string catName = string.Empty;
                        using (SqlCommand cmdLookup = new SqlCommand(
                            "SELECT ProductOrServiceCat FROM tbl_NewparentProduct WHERE id = @Id AND CompanyID = @CompanyID", conn, trans))
                        {
                            cmdLookup.Parameters.AddWithValue("@Id", idVal);
                            cmdLookup.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                            object o = cmdLookup.ExecuteScalar();
                            if (o == null || o == DBNull.Value)
                            {
                                trans.Rollback();
                                ShowErr("Category not found for this company.");
                                return;
                            }
                            catName = o.ToString();
                        }

                        int affected;
                        using (SqlCommand cmd = new SqlCommand(
                            "DELETE FROM tbl_NewparentProduct WHERE id = @Id AND CompanyID = @CompanyID", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@Id", idVal);
                            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                            affected = cmd.ExecuteNonQuery();
                        }

                        if (affected == 0)
                        {
                            trans.Rollback();
                            ShowErr("Category not found for this company.");
                            return;
                        }

                        InsertSystemNotification(
                            "Category Deleted",
                            "Category '" + catName + "' was deleted.",
                            "Warning",
                            conn, trans);

                        trans.Commit();
                        ShowOk("Data Deleted Successfully...");
                        Binddata();
                    }
                    catch (Exception ex)
                    {
                        try { trans.Rollback(); } catch { }
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        ShowErr("An error occurred while deleting the record. Please try again.");
                    }
                }
            }
        }
    }
}
