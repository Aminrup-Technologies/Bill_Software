using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm13 : System.Web.UI.Page
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
                BindGrid();
            }
        }

        // --- 1. AJAX WEBMETHOD FOR SMART SEARCH ---
        [WebMethod(EnableSession = true)]
        public static List<string> GetVendorNames(string prefix)
        {
            List<string> vendors = new List<string>();
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Strict Tenant Segregation inside the WebMethod
                string query = "SELECT Vendor_Name FROM tbl_Vendor WHERE Vendor_Name LIKE '%' + @SearchText + '%' AND CompanyID = @CompanyID ORDER BY Vendor_Name ASC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SearchText", prefix.Trim());
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                    conn.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            vendors.Add(sdr["Vendor_Name"].ToString());
                        }
                    }
                }
            }
            return vendors;
        }

        // --- 2. SEARCH & RESET BUTTON LOGIC ---
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtVendorSearch.Text))
            {
                BindGrid();
            }
            else
            {
                BindGrid1();
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtVendorSearch.Text = "";
            BindGrid();
        }

        // --- 3. GRID BINDING LOGIC (ALL & FILTERED) ---
        private void BindGrid()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = @"SELECT Vendor_Id, Vendor_Name, PrincipleVndrCode, 
                                        Address1, City, State, pin, 
                                        Com_phone, Com_email, 
                                        Service_tax_No, Pan_No, BankAccNo, BankIfscCode,
                                        CreatedBy, CreatedOn, UpdatedBy, UpdatedOn
                                 FROM tbl_Vendor 
                                 WHERE CompanyID = @CompanyID 
                                 ORDER BY Id DESC";

            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            DbCL.Conn.Close();

            if (dt.Rows.Count > 0)
            {
                lblRecordCount.Text = $"Total Vendors: {dt.Rows.Count} record(s) found.";
                lblRecordCount.ForeColor = System.Drawing.Color.FromArgb(25, 101, 138);
            }
            else
            {
                lblRecordCount.Text = "No vendors found in the database.";
                lblRecordCount.ForeColor = System.Drawing.Color.Red;
            }

            DataList1.DataSource = dt;
            DataList1.DataBind();
        }

        private void BindGrid1()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string cmdstring = @"SELECT Vendor_Id, Vendor_Name, PrincipleVndrCode, 
                                        Address1, City, State, pin, 
                                        Com_phone, Com_email, 
                                        Service_tax_No, Pan_No, BankAccNo, BankIfscCode,
                                        CreatedBy, CreatedOn, UpdatedBy, UpdatedOn
                                 FROM tbl_Vendor 
                                 WHERE Vendor_Name LIKE '%' + @VendorName + '%' AND CompanyID = @CompanyID";

            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.Parameters.AddWithValue("@VendorName", txtVendorSearch.Text.Trim());
            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            DbCL.Conn.Close();

            if (dt.Rows.Count > 0)
            {
                lblRecordCount.Text = $"Search Results: {dt.Rows.Count} matching record(s) found for '{txtVendorSearch.Text.Trim()}'.";
                lblRecordCount.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                lblRecordCount.Text = $"No matching records found for '{txtVendorSearch.Text.Trim()}'.";
                lblRecordCount.ForeColor = System.Drawing.Color.Red;
            }

            DataList1.DataSource = dt;
            DataList1.DataBind();
        }

        // --- 4. ACTION BUTTONS (Edit) ---
        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Vendor_Id = Convert.ToString(e.CommandArgument);
            if (e.CommandName == "Edit")
            {
                Response.Redirect("Update_vendor.aspx?Vendor_Id=" + Vendor_Id);
            }
        }

        // --- 5. SMART EXPORT LOGIC ---
        protected void btnDownloadExcel_Click(object sender, EventArgs e)
        {
            string exportType = ddlExportType.SelectedValue;
            string searchTerm = txtVendorSearch.Text.Trim();
            string query = "";
            string fileName = "";

            switch (exportType)
            {
                case "Master":
                    fileName = "Vendor_Basic_Details";
                    query = @"SELECT Vendor_Id AS [Vendor ID], Vendor_Name AS [Vendor Name], PrincipleVndrCode AS [Principle Code],
                                     Address1 AS [Address], City, State, pin AS [PIN Code],
                                     Com_phone AS [Phone], Com_email AS [Email], Com_web_site AS [Website],
                                     Rep_Name AS [Rep Name], Rep_Desig AS [Rep Designation], Rep_phone AS [Rep Phone]
                              FROM tbl_Vendor WHERE CompanyID = @CompanyID";
                    break;
                case "Banking":
                    fileName = "Vendor_Tax_Banking_Details";
                    query = @"SELECT Vendor_Id AS [Vendor ID], Vendor_Name AS [Vendor Name], 
                                     Service_tax_No AS [GST No], Pan_No AS [PAN No],
                                     AccountName AS [Bank Account Name], BankAccNo AS [Bank Account Number], BankIfscCode AS [IFSC Code]
                              FROM tbl_Vendor WHERE CompanyID = @CompanyID";
                    break;
                case "Full":
                    fileName = "Vendor_Full_Dump";
                    query = @"SELECT * FROM tbl_Vendor WHERE CompanyID = @CompanyID";
                    break;
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query += " AND Vendor_Name LIKE '%' + @SearchText + '%'";
            }
            query += " ORDER BY Vendor_Name ASC";

            DataTable dtExport = new DataTable();
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            using (SqlCommand cmd = new SqlCommand(query, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchText", searchTerm);
                }

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dtExport);
                }
            }
            DbCL.Conn.Close();

            if (dtExport.Rows.Count > 0)
            {
                // Proactive Audit Logging
                string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "System";
                try
                {
                    string notifQuery = @"INSERT INTO tbl_SystemNotification (CompanyID, Title, Message, Module, Type, UserId, CreatedOn) 
                                          VALUES (@CompanyID, 'Data Export', @Message, 'Vendor Management', 'Audit', @UserId, GETDATE())";
                    SqlParameter[] notifParam = {
                        new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID),
                        new SqlParameter("@Message", $"{userId} exported {dtExport.Rows.Count} vendor records ({fileName})."),
                        new SqlParameter("@UserId", userId)
                    };
                    DbCL.SPExecDB(notifQuery, notifParam);
                }
                catch { }

                string attachment = $"attachment; filename={fileName}_{DateTime.Now.ToString("yyyyMMdd")}.csv";
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", attachment);
                Response.ContentType = "text/csv";

                string[] columnNames = new string[dtExport.Columns.Count];
                for (int i = 0; i < columnNames.Length; i++)
                {
                    columnNames[i] = "\"" + dtExport.Columns[i].ColumnName + "\"";
                }
                Response.Write(string.Join(",", columnNames) + "\r\n");

                foreach (DataRow row in dtExport.Rows)
                {
                    string[] fields = new string[dtExport.Columns.Count];
                    for (int i = 0; i < dtExport.Columns.Count; i++)
                    {
                        fields[i] = "\"" + row[i].ToString().Replace("\"", "\"\"") + "\"";
                    }
                    Response.Write(string.Join(",", fields) + "\r\n");
                }
                Response.End();
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('No data found to export.'); document.getElementById('exportModal').style.display='none';", true);
            }
        }
    }
}