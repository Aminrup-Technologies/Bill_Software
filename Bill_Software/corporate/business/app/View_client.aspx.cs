using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Web.Services;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm16 : System.Web.UI.Page
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
                BindGrid(); // Load all clients initially
            }
        }

        // 1. AJAX WebMethod to fetch keyword suggestions
        [WebMethod(EnableSession = true)]
        public static List<string> GetClientNames(string prefix)
        {
            List<string> clients = new List<string>();
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Strict Tenant Segregation inside the WebMethod
                string query = "SELECT Client_Name FROM tbl_Client WHERE Client_Name LIKE '%' + @SearchText + '%' AND CompanyID = @CompanyID ORDER BY Client_Name ASC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SearchText", prefix.Trim());
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                    conn.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            clients.Add(sdr["Client_Name"].ToString());
                        }
                    }
                }
            }
            return clients;
        }

        // 2. Search & Reset Button Logic
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtClientSearch.Text))
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
            txtClientSearch.Text = "";
            BindGrid();
        }

        // 3. Grid Binders
        private void BindGrid()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = @"SELECT Client_Id, Client_Name, Industry, 
                            Address1, City, State, pin, PlaceofSupply,
                            Com_phone, Com_email, Com_web_site, 
                            Service_tax_no, Pan_no,
                            CreatedBy, CreatedOn, UpdatedBy, UpdatedOn
                     FROM tbl_Client 
                     WHERE CompanyID = @CompanyID 
                     ORDER BY Id DESC";

            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            DbCL.Conn.Close();

            // Set Feedback Message
            if (dt.Rows.Count > 0)
            {
                lblRecordCount.Text = $"Total Clients: {dt.Rows.Count} record(s) found.";
                lblRecordCount.ForeColor = System.Drawing.Color.FromArgb(25, 101, 138); // Match your Theme Blue
            }
            else
            {
                lblRecordCount.Text = "No clients found in the database.";
                lblRecordCount.ForeColor = System.Drawing.Color.Red;
            }

            DataList1.DataSource = dt;
            DataList1.DataBind();
        }

        // 4. Grid Binders (Load Filtered Search)
        private void BindGrid1()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string cmdstring = @"SELECT Client_Id, Client_Name, Industry, 
                            Address1, City, State, pin, PlaceofSupply,
                            Com_phone, Com_email, Com_web_site, 
                            Service_tax_no, Pan_no,
                            CreatedBy, CreatedOn, UpdatedBy, UpdatedOn
                     FROM tbl_Client 
                     WHERE Client_Name LIKE '%' + @ClientName + '%' AND CompanyID = @CompanyID";

            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.Parameters.AddWithValue("@ClientName", txtClientSearch.Text.Trim());
            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            DbCL.Conn.Close();

            // Set Feedback Message
            if (dt.Rows.Count > 0)
            {
                lblRecordCount.Text = $"Search Results: {dt.Rows.Count} matching record(s) found for '{txtClientSearch.Text.Trim()}'.";
                lblRecordCount.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                lblRecordCount.Text = $"No matching records found for '{txtClientSearch.Text.Trim()}'.";
                lblRecordCount.ForeColor = System.Drawing.Color.Red;
            }

            DataList1.DataSource = dt;
            DataList1.DataBind();
        }

        // 4. Action Buttons (Edit, Factories, Reps)
        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Client_Id = Convert.ToString(e.CommandArgument);
            if (e.CommandName == "Edit")
            {
                Response.Redirect("Update_client.aspx?Client_Id=" + Client_Id);
            }
            else if (e.CommandName == "Representative")
            {
                Response.Redirect("Representative.aspx?Client_Id=" + Client_Id);
            }
            else if (e.CommandName == "Factory")
            {
                Response.Redirect("AddFactory.aspx?Client_Id=" + Client_Id);
            }
        }

        protected void btnDownloadExcel_Click(object sender, EventArgs e)
        {
            string exportType = ddlExportType.SelectedValue;
            string searchTerm = txtClientSearch.Text.Trim();
            string query = "";
            string fileName = "";

            // 1. Determine which specialized query to run based on user selection
            switch (exportType)
            {
                case "Master":
                    fileName = "Client_Master_Report";
                    query = @"SELECT Client_Id AS [Client ID], Client_Name AS [Client Name], Industry, 
                             Address1 AS [Corporate Address], City, State, pin AS [PIN Code], PlaceofSupply AS [Place of Supply],
                             Com_phone AS [Phone], Com_email AS [Email], Com_web_site AS [Website], 
                             Service_tax_no AS [GST No], Pan_no AS [PAN No],
                             CreatedBy AS [Added By], CreatedOn AS [Added Date]
                      FROM tbl_Client 
                      WHERE CompanyID = @CompanyID";
                    break;

                case "Reps":
                    fileName = "Client_Representatives_Report";
                    query = @"SELECT c.Client_Id AS [Client ID], c.Client_Name AS [Client Name], 
                             r.RepTitle AS [Title], r.Representative_name AS [First Name], r.RepLastName AS [Last Name], 
                             r.Designation, r.Phone_no AS [Rep Phone], r.Email AS [Rep Email]
                      FROM tbl_Client c
                      LEFT JOIN tbl_representative r ON c.Client_Id = r.Copany_Id AND r.CompanyID = c.CompanyID
                      WHERE c.CompanyID = @CompanyID";
                    break;

                case "Factories":
                    fileName = "Client_Factories_Report";
                    query = @"SELECT c.Client_Id AS [Client ID], c.Client_Name AS [Client Name], 
                             f.Factory_name AS [Unit Name], f.Address1 AS [Unit Address 1], f.Address2 AS [Unit Address 2], 
                             f.city AS [City], f.State AS [State], f.pin AS [PIN Code]
                      FROM tbl_Client c
                      LEFT JOIN tbl_Factory f ON c.Client_Id = f.Client_id AND f.CompanyID = c.CompanyID
                      WHERE c.CompanyID = @CompanyID";
                    break;
            }

            // Apply Search Filter if user had typed something
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // "c.Client_Name" handles the alias if it's a JOIN query, or regular query
                query += query.Contains("FROM tbl_Client c")
                         ? " AND c.Client_Name LIKE '%' + @SearchText + '%'"
                         : " AND Client_Name LIKE '%' + @SearchText + '%'";
            }

            query += query.Contains("FROM tbl_Client c") ? " ORDER BY c.Client_Name ASC" : " ORDER BY Client_Name ASC";

            // 2. Fetch the Data
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
                // 3. Proactive Audit Logging: Record that a user downloaded data
                string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "System";
                try
                {
                    string notifQuery = @"INSERT INTO tbl_SystemNotification (CompanyID, Title, Message, Module, Type, UserId, CreatedOn) 
                                  VALUES (@CompanyID, 'Data Export', @Message, 'Client Management', 'Audit', @UserId, GETDATE())";
                    SqlParameter[] notifParam = {
                new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID),
                new SqlParameter("@Message", $"{userId} exported {dtExport.Rows.Count} records via {fileName}."),
                new SqlParameter("@UserId", userId)
            };
                    DbCL.SPExecDB(notifQuery, notifParam);
                }
                catch { /* Soft catch for audit logs */ }

                // 4. Generate & Download CSV
                string attachment = $"attachment; filename={fileName}_{DateTime.Now.ToString("yyyyMMdd")}.csv";
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", attachment);
                Response.ContentType = "text/csv";

                // Write Column Headers
                string[] columnNames = new string[dtExport.Columns.Count];
                for (int i = 0; i < columnNames.Length; i++)
                {
                    // Wrap in quotes to handle commas in column names (standard CSV practice)
                    columnNames[i] = "\"" + dtExport.Columns[i].ColumnName + "\"";
                }
                Response.Write(string.Join(",", columnNames) + "\r\n");

                // Write Data Rows
                foreach (DataRow row in dtExport.Rows)
                {
                    string[] fields = new string[dtExport.Columns.Count];
                    for (int i = 0; i < dtExport.Columns.Count; i++)
                    {
                        string field = row[i].ToString();
                        // Escape double quotes and wrap in quotes to handle commas/newlines in data
                        field = field.Replace("\"", "\"\"");
                        fields[i] = "\"" + field + "\"";
                    }
                    Response.Write(string.Join(",", fields) + "\r\n");
                }

                Response.End();
            }
            else
            {
                // If export yields no data, alert the user
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('No data found to export based on current search filter.'); document.getElementById('exportModal').style.display='none';", true);
            }
        }
    }
}