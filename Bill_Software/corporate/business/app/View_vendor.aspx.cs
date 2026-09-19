using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm13 : SecurePage
    {
        protected override string RequiredPermissionKey { get { return "View_vendor"; } }

        DB_UTILITY DbCL = new DB_UTILITY();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                PopulateTargetCompanyDropdown();
                BindGrid();
            }
        }

        // --- 1. AJAX WEBMETHOD FOR SMART SEARCH ---
        [WebMethod(EnableSession = true)]
        public static List<string> GetVendorNames(string prefix)
        {
            AuthGuard.EnsureWebMethodPermission("View_vendor");
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
            string cmdstring = @"SELECT Id, Vendor_Id, Vendor_Name, PrincipleVndrCode,
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

        private void PopulateTargetCompanyDropdown()
        {
            ddlTargetCompanyGlobal.Items.Clear();

            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT c.ID, c.Name
                FROM dbo.tbl_Company c
                INNER JOIN dbo.UserCompanyAccess a ON a.CompanyID = c.ID
                INNER JOIN dbo.tbl_login u ON u.Id = a.UserId
                WHERE u.User_Id = @UserId
                  AND a.IsActive = 1
                  AND (c.IsActive = 1 OR c.IsActive IS NULL)
                  AND c.ID <> @CompanyID
                ORDER BY c.Name", conn))
            {
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 100)
                {
                    Value = Session["USERID"] != null ? Session["USERID"].ToString() : string.Empty
                });
                cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int)
                {
                    Value = CompanyContext.CurrentCompanyID
                });
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }

            // Membership can list only the current tenant. Other active companies
            // are still valid duplication targets and must appear in the dropdown.
            if (dt.Rows.Count == 0)
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT ID, Name
                    FROM dbo.tbl_Company
                    WHERE (IsActive = 1 OR IsActive IS NULL)
                      AND ID <> @CompanyID
                    ORDER BY Name", conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int)
                    {
                        Value = CompanyContext.CurrentCompanyID
                    });
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        da.Fill(dt);
                }
            }

            ddlTargetCompanyGlobal.DataSource = dt;
            ddlTargetCompanyGlobal.DataTextField = "Name";
            ddlTargetCompanyGlobal.DataValueField = "ID";
            ddlTargetCompanyGlobal.DataBind();
            ddlTargetCompanyGlobal.Items.Insert(0, new ListItem("-- Select Target Company --", ""));
        }

        private void BindGrid1()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string cmdstring = @"SELECT Id, Vendor_Id, Vendor_Name, PrincipleVndrCode,
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

        protected void DataList1_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item &&
                e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            DataRowView row = e.Item.DataItem as DataRowView;
            HiddenField sourceId = e.Item.FindControl("hfVendorId") as HiddenField;
            if (row != null && sourceId != null)
                sourceId.Value = Convert.ToString(row["Id"]);
        }

        protected void btnBulkDuplicateVendor_Click(object sender, EventArgs e)
        {
            btnConfirmDuplicateVendor_Click(sender, e);
        }

        protected void btnConfirmDuplicateVendor_Click(object sender, EventArgs e)
        {
            try
            {
                string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "System";
                int sourceCompanyId = CompanyContext.CurrentCompanyID;
                int targetCompanyId = 0;

                if (!int.TryParse(ddlTargetCompanyGlobal.SelectedValue, out targetCompanyId) || targetCompanyId <= 0)
                {
                    ShowMessage("Please select a valid target company from the list.", false);
                    return;
                }

                if (targetCompanyId == sourceCompanyId)
                {
                    ShowMessage("Source and target companies must be different. Please select another company.", false);
                    return;
                }

                if (!AuthGuard.UserCanAccessCompany(targetCompanyId))
                {
                    ShowMessage("You do not have access to the selected target company.", false);
                    return;
                }

                // Check if this is a bulk operation
                string bulkIdsRaw = hfBulkVendorIds.Value;
                if (!string.IsNullOrWhiteSpace(bulkIdsRaw))
                {
                    HandleBulkVendorDuplication(bulkIdsRaw, targetCompanyId, userId);
                    return;
                }

                // Single vendor duplication
                string vendorId = hfPendingVendorId.Value;
                if (string.IsNullOrWhiteSpace(vendorId))
                {
                    ShowMessage("No vendor selected for duplication. Please try again.", false);
                    return;
                }

                vendorId = vendorId.Trim();
                if (vendorId.Length < 3 || !vendorId.StartsWith("AA", StringComparison.OrdinalIgnoreCase))
                {
                    ShowMessage("Invalid vendor identifier. Please refresh and try again.", false);
                    return;
                }

                int sourceId = ResolveVendorId(vendorId, sourceCompanyId);
                if (sourceId <= 0)
                {
                    ShowMessage("Vendor not found in your current company.", false);
                    return;
                }

                bool success = DuplicationService.DuplicateVendor(sourceId, targetCompanyId, userId);

                if (success)
                {
                    string targetCompanyName = ddlTargetCompanyGlobal.SelectedItem != null
                        ? ddlTargetCompanyGlobal.SelectedItem.Text : "target company";
                    ShowMessage(
                        string.Format("Vendor '{0}' duplicated successfully to '{1}'.", vendorId, targetCompanyName),
                        true);
                    BindGrid();
                }
                else
                {
                    ShowMessage("Duplication failed. The vendor could not be created in the target company.", false);
                }
            }
            catch (InvalidOperationException ex)
            {
                ShowMessage("Duplication could not be completed: " + ex.Message, false);
            }
            catch (UnauthorizedAccessException)
            {
                ShowMessage("You do not have permission to perform this action.", false);
            }
            catch (Exception)
            {
                ShowMessage("An unexpected error occurred during duplication. Please try again or contact support.", false);
            }
            finally
            {
                hfPendingVendorId.Value = string.Empty;
                hfBulkVendorIds.Value = string.Empty;
                ddlTargetCompanyGlobal.SelectedIndex = 0;
                btnConfirmDuplicateVendor.Enabled = true;
                btnConfirmDuplicateVendor.Text = "Confirm Duplicate";
            }
        }

        private void HandleBulkVendorDuplication(string bulkIdsRaw, int targetCompanyId, string userId)
        {
            string[] parts = bulkIdsRaw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var sourceIds = new System.Collections.Generic.List<int>();
            foreach (string part in parts)
            {
                int id;
                if (int.TryParse(part.Trim(), out id) && id > 0)
                    sourceIds.Add(id);
            }

            if (sourceIds.Count == 0)
            {
                ShowMessage("No valid vendors selected for duplication.", false);
                return;
            }

            string targetCompanyName = ddlTargetCompanyGlobal.SelectedItem != null
                ? ddlTargetCompanyGlobal.SelectedItem.Text : "target company";

            var result = DuplicationService.BulkDuplicateVendors(sourceIds.ToArray(), targetCompanyId, userId);

            string message;
            bool isSuccess;
            if (result.FailedCount > 0 && result.SuccessCount == 0)
            {
                message = string.Format(
                    "Bulk duplication to '{0}' failed: {1}",
                    targetCompanyName,
                    result.FailureReason ?? "All vendors failed.");
                isSuccess = false;
            }
            else
            {
                message = string.Format(
                    "{0} duplicated, {1} skipped, {2} failed.",
                    result.SuccessCount,
                    result.SkippedCount,
                    result.FailedCount);
                isSuccess = result.FailedCount == 0;
            }

            BindGrid();
            ShowMessage(message, isSuccess);
        }

        private void ShowMessage(string text, bool isSuccess)
        {
            lblRecordCount.Text = text;
            lblRecordCount.ForeColor = isSuccess
                ? System.Drawing.Color.Green
                : System.Drawing.Color.Red;
        }

        private int ResolveVendorId(string vendorId, int companyId)
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Id FROM tbl_Vendor WHERE Vendor_Id = @VendorId AND CompanyID = @CompanyID", conn))
                {
                    cmd.Parameters.AddWithValue("@VendorId", vendorId);
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        return Convert.ToInt32(result);
                }
            }
            return 0;
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