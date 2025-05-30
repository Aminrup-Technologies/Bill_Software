using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Text;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm11 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        public DataTable first_datatable;
        public static DataTable Dt = new DataTable("Table");
        public static double tota_purchesrate1 = 0;
        public static double total_tax_rate_details = 0;

        private List<string> vatRates;
        private List<string> serviceTaxRates;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                
                tota_purchesrate1 = 0;
                total_tax_rate_details = 0;
                Dt = new DataTable("Table");
                DbCL.FillCombo(cmbState, "select State_Name from tbl_State order by State_Name");
                DbCL.FillCombo(cmbcity, "select City_Name from tbl_City order by City_Name");
                DbCL.FillCombo(cmbvendor, "select Vendor_Name from tbl_Vendor order by Vendor_Name");
                BindShippedToDropdown();
                txtPurchesDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtcashDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtdddate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtneftdate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtpaymentdate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }
        }


        protected void BindShippedToDropdown()
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ToString()))
            {
                string query = "SELECT StoreId, StoreName, StoreAddress FROM Stores WHERE IsActive = 1";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    row["StoreName"] = $"{row["StoreName"]} [{row["StoreAddress"]}]";
                }

                DDL_ShippedTo.DataSource = dt;
                DDL_ShippedTo.DataTextField = "StoreName";
                DDL_ShippedTo.DataValueField = "StoreId";
                DDL_ShippedTo.DataBind();

                DDL_ShippedTo.Items.Insert(0, new ListItem("-- Select Store --", ""));
            }
        }


        private void LoadTaxRates()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            // Fetch VAT Rates
            string vatQuery = "Select Vat_Rate from tbl_Vat_Master";
            SqlCommand vatCmd = new SqlCommand(vatQuery, DbCL.Conn);
            SqlDataReader vatRdr = vatCmd.ExecuteReader();

            vatRates = new List<string> { "NA" }; // Initialize with "NA"
            while (vatRdr.Read())
            {
                vatRates.Add(vatRdr[0].ToString());
            }
            vatRdr.Close();

            // Fetch Service Tax Rates
            string serviceTaxQuery = "Select Service_tax from tbl_Service_master";
            SqlCommand serviceTaxCmd = new SqlCommand(serviceTaxQuery, DbCL.Conn);
            SqlDataReader serviceTaxRdr = serviceTaxCmd.ExecuteReader();

            serviceTaxRates = new List<string> { "NA" }; // Initialize with "NA"
            while (serviceTaxRdr.Read())
            {
                serviceTaxRates.Add(serviceTaxRdr[0].ToString());
            }
            serviceTaxRdr.Close();

            DbCL.Conn.Close();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Label1.Visible = false;
            RadioButtonList1.Visible = false;
            Button1.Visible = false;
            Panel1.Visible = true;
            BindListitem();
        }

        private void BindListitem()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                //cmdstring = "select Product_Name from tbl_Product order by Product_Name";
                cmdstring = "select ProductOrServiceCat from tbl_NewparentProduct order by Id";
            }
            else
            {
                cmdstring = "select Service_name  from tbl_Service order by Service_name";
            }
            cmbproduct_service.Items.Clear();
            cmbproduct_service.Items.Add("--Select--");
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            while (re.Read())
            {
                cmbproduct_service.Items.Add(re.GetValue(0).ToString());
            }
            DbCL.Conn.Close();

        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            LoadTaxRates();

            Panel2.Visible = true;
            if (RadioButtonList1.SelectedIndex == 0)
            {
                //string cmdstring = "select Product_code,Product_Name from tbl_Product where Product_Name='" + cmbproduct_service.Text + "'";
                string cmdstring = "select ProductID, ProductName from tbl_NewProduct where ProductOrServiceCat='" + cmbproduct_service.Text + "'";
                Binddata1(cmdstring);

                if (DDL_vat_parsentage.Items.Count == 0)
                {
                    DDL_vat_parsentage.Items.AddRange(vatRates.Select(rate => new ListItem(rate)).ToArray());
                }

                //if (DDL_tcspercent.Items.Count == 0)
                //{
                //    DDL_tcspercent.Items.AddRange(vatRates.Select(rate => new ListItem(rate)).ToArray());
                //}

            }
            else
            {
                string cmdstring = "select Service_code,Service_name  from tbl_Service where Service_name='" + cmbproduct_service.Text + "'";
                Binddata1(cmdstring);
            }

            cmbproduct_service.SelectedIndex = 0;

            //string listProduct_Service1 = null;
            //for (int i = 0; i <= listProduct_Service.Items.Count - 1; i++)
            //{
            //    if (listProduct_Service.Items[i].Selected)
            //    {

            //        listProduct_Service1 = listProduct_Service.Items[i].Text;
            //        if (RadioButtonList1.SelectedIndex == 0)
            //        {
            //            string cmdstring = "select Product_code,Product_Name,Purches_Rate,Sail_Rate,Tax_Rate from tbl_Product where Product_Name='" + listProduct_Service1.ToString() + "'";
            //            Binddata1(cmdstring);
            //        }
            //        else
            //        {
            //            string cmdstring = "select Service_code,Service_name,Purches_rate,Sail_rate,Tax_rate  from tbl_Service where Service_name='" + listProduct_Service1.ToString() + "'";
            //            Binddata1(cmdstring);
            //        }

            //    }
            //}
            gd_Service_Product.DataSource = Dt;
            gd_Service_Product.DataBind();
            ViewState["dt"] = Dt;
            
        }

        private void Binddata1New(SqlCommand cmd)
        {
            // Assuming DbCL already has methods to handle connection management
            DbCL.ConnectDb(); // Establish the database connection
            cmd.Connection = DbCL.Conn; // Assign the connection to the SqlCommand
            // Initialize a DataTable and fill it directly using SqlDataAdapter
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt); // Fill the DataTable with data from the command

            if (dt.Rows.Count > 0) // Check if there is any data
            {
                first_datatable = dt;

                if (Label2.Text == "1")
                {
                    newgrid1();
                }
                else
                {
                    newgrid();
                }

                Label2.Text = (Convert.ToInt32(Label2.Text) + 1).ToString();
            }
            DbCL.Conn.Close(); // Close the connection
        }

        private void Binddata1(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            try
            {
                // Create and configure the SqlCommand
                SqlCommand com1 = new SqlCommand(cmdstring, DbCL.Conn);

                // Use SqlDataAdapter to fill the DataTable directly
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(com1);
                da.Fill(dt); // Fill the DataTable with data

                // Check if the DataTable has rows to process
                if (dt.Rows.Count > 0)
                {
                    first_datatable = dt;

                    // Call the appropriate grid function based on Label2.Text
                    if (Label2.Text == "1")
                    {
                        newgrid1();
                    }
                    else
                    {
                        newgrid();
                    }

                    // Update Label2 to ensure the function executes only once
                    Label2.Text = (Convert.ToInt32(Label2.Text) + 1).ToString();
                }
            }
            finally
            {
                // Close the database connection in the finally block to ensure it always closes
                DbCL.Conn.Close();
            }
        }


        private void Binddata1Org(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand com1 = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataAdapter da = new SqlDataAdapter(com1);
            SqlDataReader dr = com1.ExecuteReader();

            if (dr.Read())
            {
                DataTable dt = DbCL.GetDataTable(cmdstring);
                first_datatable = dt;
                if (Label2.Text == "1")
                {
                    newgrid1();
                }
                else
                {
                    newgrid();
                }
                Label2.Text = (Convert.ToInt32(Label2.Text) + 1).ToString();
            }
            DbCL.Conn.Close();
        }

        private void newgrid1_old()
        {
            DataTable dt;
            dt = first_datatable;

            DataRow dr = null;
            DataColumn Ser_pro_code = new DataColumn("Ser_pro_code", typeof(string));
            Dt.Columns.Add(Ser_pro_code);

            DataColumn Ser_pro_Name = new DataColumn("Ser_pro_Name", typeof(string));
            Dt.Columns.Add(Ser_pro_Name);
            //DataColumn Vendor_rate = new DataColumn("Vendor_rate", typeof(string));
            //Dt.Columns.Add(Vendor_rate);
            //DataColumn Sale_rate = new DataColumn("Sale_rate", typeof(string));
            //Dt.Columns.Add(Sale_rate);
            //DataColumn service_Tax_Rate = new DataColumn("service_Tax_Rate", typeof(string));
            //Dt.Columns.Add(service_Tax_Rate);

            for (int i = 0; i <= dt.Rows.Count - 1; i++)
            {
                string Ser_pro_code1 = (String)first_datatable.Rows[i][0];
                string Ser_pro_Name1 = (String)first_datatable.Rows[i][1]; ;
                //string Vendor_rate1 = (String)first_datatable.Rows[i][2];
                //string Sale_rate1 = (String)first_datatable.Rows[i][3];
                //string service_Tax_Rate1 = (String)first_datatable.Rows[i][4];
                dr = Dt.NewRow();
                dr["Ser_pro_code"] = Ser_pro_code1.ToString();
                dr["Ser_pro_Name"] = Ser_pro_Name1.ToString();
                //dr["Vendor_rate"] = Vendor_rate1.ToString();
                //dr["Sale_rate"] = Sale_rate1.ToString();
                //dr["service_Tax_Rate"] = service_Tax_Rate1.ToString();
                Dt.Rows.Add(dr);
            }
        }

        private void newgrid1()
        {
            DataTable dt = first_datatable;

            // Ensure Dt is initialized
            if (Dt == null)
                Dt = new DataTable();

            // Clear existing columns and rows if needed
            Dt.Clear();
            Dt.Columns.Clear();

            // Add necessary columns
            Dt.Columns.Add("Ser_pro_code", typeof(string));
            Dt.Columns.Add("Ser_pro_Name", typeof(string));

            //// Add the "Order" column here
            if (!Dt.Columns.Contains("Order"))
            {
                Dt.Columns.Add("Order", typeof(int));
            }

            // Add rows from first_datatable
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = Dt.NewRow();
                dr["Ser_pro_code"] = dt.Rows[i][0].ToString();
                dr["Ser_pro_Name"] = dt.Rows[i][1].ToString();

                // Initialize order, e.g., by default assign sequential order
                //dr["Order"] = i + 1;

                Dt.Rows.Add(dr);
            }
        }




        private void newgrid1New()
        {
            DataTable dt = first_datatable; // Get the existing data table

            // Create a new DataTable to hold the results
            DataTable Dt = new DataTable();

            // Add columns to the new DataTable
            Dt.Columns.Add(new DataColumn("Ser_pro_code", typeof(string)));
            Dt.Columns.Add(new DataColumn("Ser_pro_Name", typeof(string)));
            // Uncomment these if you decide to use them in the future
            // Dt.Columns.Add(new DataColumn("Vendor_rate", typeof(string)));
            // Dt.Columns.Add(new DataColumn("Sale_rate", typeof(string)));
            // Dt.Columns.Add(new DataColumn("service_Tax_Rate", typeof(string)));

            // Iterate over rows in the first DataTable
            foreach (DataRow row in dt.Rows)
            {
                DataRow dr = Dt.NewRow(); // Create a new row for the new DataTable
                dr["Ser_pro_code"] = row[0]?.ToString(); // Use null-conditional operator for safety
                dr["Ser_pro_Name"] = row[1]?.ToString(); // Use null-conditional operator for safety

                // Uncomment these if you decide to use them in the future
                // dr["Vendor_rate"] = row[2]?.ToString();
                // dr["Sale_rate"] = row[3]?.ToString();
                // dr["service_Tax_Rate"] = row[4]?.ToString();

                Dt.Rows.Add(dr); // Add the new row to the new DataTable
            }

            // Now, you can use Dt as needed
        }


        private void newgrid_old()
        {
            DataTable dt;
            dt = first_datatable;
            DataRow dr = null;
            for (int i = 0; i <= dt.Rows.Count - 1; i++)
            {
                string Ser_pro_code1 = (String)first_datatable.Rows[i][0];
                string Ser_pro_Name1 = (String)first_datatable.Rows[i][1]; ;
                //string Vendor_rate1 = (String)first_datatable.Rows[i][2];
                //string Sale_rate1 = (String)first_datatable.Rows[i][3];
                //string service_Tax_Rate1 = (String)first_datatable.Rows[i][4];
                dr = Dt.NewRow();
                dr["Ser_pro_code"] = Ser_pro_code1.ToString();
                dr["Ser_pro_Name"] = Ser_pro_Name1.ToString();
                //dr["Vendor_rate"] = Vendor_rate1.ToString();
                //dr["Sale_rate"] = Sale_rate1.ToString();
                //dr["service_Tax_Rate"] = service_Tax_Rate1.ToString();
                Dt.Rows.Add(dr);



            }
        }

        private void newgrid()
        {
            DataTable dt = first_datatable;

            if (Dt == null)
                Dt = new DataTable();

            Dt.Clear();
            Dt.Columns.Clear();

            // Add columns
            Dt.Columns.Add("Ser_pro_code", typeof(string));
            Dt.Columns.Add("Ser_pro_Name", typeof(string));

            // Add the Order column
            Dt.Columns.Add("Order", typeof(int));

            // Fill rows with default Order values (1-based index)
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = Dt.NewRow();
                dr["Ser_pro_code"] = dt.Rows[i][0].ToString();
                dr["Ser_pro_Name"] = dt.Rows[i][1].ToString();
                //dr["Order"] = i + 1;  // default ordering sequence
                Dt.Rows.Add(dr);
            }
        }

        protected void btnApplyOrder_Click(object sender, EventArgs e)
        {
            if (ViewState["dt"] == null)
            {
                // No data to work with
                return;
            }

            DataTable dt = (DataTable)ViewState["dt"];

            // Read order numbers from TextBoxes
            for (int i = 0; i < gd_Service_Product.Rows.Count; i++)
            {
                GridViewRow row = gd_Service_Product.Rows[i];
                TextBox txtOrder = (TextBox)row.FindControl("txtOrder");
                string orderValue = txtOrder.Text.Trim();

                // You can validate and use this orderValue for your logic
                // For example, store it in a new column or a dictionary for later processing

                // Example: just print/debug
                System.Diagnostics.Debug.WriteLine($"Row {i}, Order entered: {orderValue}");
            }

            // Now you can use this ordering data as you want (e.g., sort dt, reorder rows, save order in DB, etc.)
        }




        protected void gd_Service_Product_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                // Null check on ViewState["dt"]
                if (ViewState["dt"] == null)
                {
                    throw new Exception("ViewState[\"dt\"] is null.");
                }

                DataTable dt = ViewState["dt"] as DataTable;

                if (dt == null || dt.Rows.Count == 0)
                {
                    throw new Exception("DataTable is null or empty.");
                }

                int index;
                // Parse and validate index
                if (!int.TryParse(e.CommandArgument.ToString(), out index))
                {
                    throw new Exception("CommandArgument is not a valid integer.");
                }

                if (index < 0 || index >= dt.Rows.Count)
                {
                    throw new Exception($"Index {index} is out of bounds.");
                }

                // Swap logic
                if (e.CommandName == "MoveUp" && index > 0)
                {
                    SwapRows(dt, index, index - 1);
                }
                else if (e.CommandName == "MoveDown" && index < dt.Rows.Count - 1)
                {
                    SwapRows(dt, index, index + 1);
                }

                // Rebind to grid
                ViewState["dt"] = dt;
                gd_Service_Product.DataSource = dt;
                gd_Service_Product.DataBind();
            }
            catch (Exception ex)
            {
                // Optional: Display on UI or log
                // lblError.Text = "Grid operation failed: " + ex.Message;
                throw new Exception("Grid bind failed: " + ex.Message, ex);
            }
        }


        private void SwapRows(DataTable table, int index1, int index2)
        {
            if (table == null || index1 < 0 || index2 < 0 || index1 >= table.Rows.Count || index2 >= table.Rows.Count)
                throw new ArgumentOutOfRangeException("Index out of range in SwapRows.");

            DataRow temp = table.NewRow();
            temp.ItemArray = table.Rows[index1].ItemArray;
            table.Rows[index1].ItemArray = table.Rows[index2].ItemArray;
            table.Rows[index2].ItemArray = temp.ItemArray;
        }




        protected void gd_Service_Product_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //if (e.Row.RowType == DataControlRowType.DataRow)
            //{
            //    //DropDownList dp = (DropDownList)e.Row.Cells[6].FindControl("service_Tax_Rate");
            //    DropDownList dp1 = (DropDownList)e.Row.Cells[4].FindControl("vat_parsentage");


            //    DbCL.Sqlconnection();

            //    DbCL.ConnectDb();
            //    string cmdString = "";
            //    if (RadioButtonList1.SelectedIndex == 0)
            //    {
            //        cmdString = "Select Vat_Rate from tbl_Vat_Master";
            //    }
            //    else
            //    {
            //        cmdString = "Select Service_tax from tbl_Service_master";
            //    }
            //    SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn);

            //    SqlDataReader Rdr;
            //    Rdr = cmd.ExecuteReader();
            //    dp1.Items.Add("NA");
            //    while (Rdr.Read())
            //    {
            //        dp1.Items.Add(Rdr[0].ToString());
            //    }

            //    DbCL.Conn.Close();
            //    //DbCL.Sqlconnection();

            //    //DbCL.ConnectDb();
            //    //string cmdString1 = "Select Service_tax from tbl_Service_master";
            //    //SqlCommand cmd1 = new SqlCommand(cmdString1, DbCL.Conn);

            //    //SqlDataReader Rdr1;
            //    //Rdr1 = cmd1.ExecuteReader();

            //    //while (Rdr1.Read())
            //    //{
            //    //    dp.Items.Add(Rdr1["Service_tax"].ToString());
            //    //}

            //    //DbCL.Conn.Close();
            //}

            //Above whole code is commented to use single databse query and read from the list

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList dp1 = (DropDownList)e.Row.Cells[4].FindControl("vat_parsentage");

                // Clear existing items
                dp1.Items.Clear();

                if (RadioButtonList1.SelectedIndex == 0) // VAT Rates
                {
                    dp1.Items.AddRange(vatRates.Select(rate => new ListItem(rate)).ToArray());
                }
                else // Service Tax Rates
                {
                    dp1.Items.AddRange(serviceTaxRates.Select(rate => new ListItem(rate)).ToArray());
                }
            }
        }

        protected void RadioButtonList2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RadioButtonList2.SelectedIndex == 0)
            {
                First.Visible = true;
                Second.Visible = false;
                Third.Visible = false;

            }
            else if (RadioButtonList2.SelectedIndex == 3)
            {
                First.Visible = false;
                Second.Visible = false;
                Third.Visible = true;

            }
            else
            {
                First.Visible = false;
                Second.Visible = true;
                Third.Visible = false;

            }
        }

        //protected void Button3_Click(object sender, EventArgs e)
        //{
        //    string purchesid = findpurchesId();
        //    int i = 0;
        //    DataTable dt1 = (DataTable)ViewState["dt"];
        //    if (dt1 == null) return;

        //    using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ToString()))
        //    {
        //        conn.Open();
        //        using (SqlTransaction trans = conn.BeginTransaction())
        //        {
        //            try
        //            {
        //                Int32 sl = 0;
        //                double tota_purchesrate1 = 0, total_tax_rate_details = 0;
        //                List<string> errorMessages = new List<string>();
        //                for (i = 0; i < dt1.Rows.Count; i++)
        //                {
        //                    SqlCommand cmd = new SqlCommand()
        //                    {
        //                        CommandType = CommandType.Text,
        //                        Connection = conn,
        //                        Transaction = trans
        //                    };

        //                    string Ser_pro_code = ((Label)gd_Service_Product.Rows[i].FindControl("Ser_pro_code")).Text;
        //                    string Ser_pro_Name = ((Label)gd_Service_Product.Rows[i].FindControl("Ser_pro_Name")).Text;
        //                    string sepecification = ((TextBox)gd_Service_Product.Rows[i].FindControl("sepecification")).Text;
        //                    string Quantity = ((TextBox)gd_Service_Product.Rows[i].FindControl("Quantity")).Text;
        //                    string Vendor_rate = ((TextBox)gd_Service_Product.Rows[i].FindControl("Vendor_rate")).Text;
        //                    string tax_app = ((RadioButtonList)gd_Service_Product.Rows[i].FindControl("RadioButtonList1")).Text;
        //                    string vat_parsentage = ((DropDownList)gd_Service_Product.Rows[i].FindControl("vat_parsentage")).Text;



        //                    double qty = 0;
        //                    double vndr = 0;

        //                    bool isQuantityValid = !string.IsNullOrEmpty(Quantity) && double.TryParse(Quantity, out qty) && qty > 0;
        //                    bool isVendorRateValid = !string.IsNullOrEmpty(Vendor_rate) && double.TryParse(Vendor_rate, out vndr);
        //                    bool isTaxAppValid = !string.IsNullOrEmpty(tax_app);
        //                    bool isVatValid = !string.IsNullOrEmpty(vat_parsentage) && vat_parsentage != "NA";

        //                    bool isAnyFieldFilled = isQuantityValid || isVendorRateValid || isTaxAppValid || isVatValid;

        //                    if (isAnyFieldFilled)
        //                    {
        //                        List<string> missingFields = new List<string>();

        //                        if (!isQuantityValid) missingFields.Add("Quantity");
        //                        if (!isVendorRateValid) missingFields.Add("Vendor Rate");
        //                        if (!isTaxAppValid) missingFields.Add("Tax Applicable");

        //                        if (tax_app == "Yes" && !isVatValid)
        //                        {
        //                            missingFields.Add("VAT Percentage");
        //                        }

        //                        if (missingFields.Count > 0)
        //                        {
        //                            errorMessages.Add($"Row {i + 1}: {string.Join(", ", missingFields)} is required.");
        //                            continue;
        //                        }
        //                    }

        //                    sl++;
        //                    double parches_rate = Convert.ToDouble(Vendor_rate) * Convert.ToDouble(Quantity);
        //                    double tax_rete = (tax_app == "Yes") ? (parches_rate * Convert.ToDouble(vat_parsentage)) / 100 : 0;
        //                    double total_purches_rate = tax_rete + parches_rate;
        //                    tota_purchesrate1 += total_purches_rate;
        //                    total_tax_rate_details += tax_rete;

        //                    cmd.CommandText = "insert into tbl_purches_details(sl_no,Purches_id,Product_id,Product_name,vendor_rate,tax_applicable,tax_rate,Quantity,purches_rate,total_purches_rate,vat_amount,specification,Purches_date,Client_id)" +
        //                                      "values(@sl_no, @Purches_id, @Product_id, @Product_name, @vendor_rate, @tax_applicable, @tax_rate, @Quantity, @purches_rate, @total_purches_rate, @vat_amount, @specification, @Purches_date, @Client_id)";
        //                    cmd.Parameters.AddWithValue("@sl_no", sl.ToString());
        //                    cmd.Parameters.AddWithValue("@Purches_id", purchesid);
        //                    cmd.Parameters.AddWithValue("@Product_id", Ser_pro_code);
        //                    cmd.Parameters.AddWithValue("@Product_name", Ser_pro_Name);
        //                    cmd.Parameters.AddWithValue("@vendor_rate", Vendor_rate);
        //                    cmd.Parameters.AddWithValue("@tax_applicable", tax_app);
        //                    cmd.Parameters.AddWithValue("@tax_rate", vat_parsentage);
        //                    cmd.Parameters.AddWithValue("@Quantity", Quantity);
        //                    cmd.Parameters.AddWithValue("@purches_rate", parches_rate);
        //                    cmd.Parameters.AddWithValue("@total_purches_rate", total_purches_rate);
        //                    cmd.Parameters.AddWithValue("@vat_amount", tax_rete);
        //                    cmd.Parameters.AddWithValue("@specification", sepecification.ToString());
        //                    cmd.Parameters.AddWithValue("@Purches_date", txtPurchesDate.Text);
        //                    cmd.Parameters.AddWithValue("@Client_id", lblvendor_id.Text);
        //                    cmd.ExecuteNonQuery();

        //                    updatestock(Ser_pro_code, Ser_pro_Name, Quantity, Vendor_rate, vat_parsentage);
        //                }

        //                decimal invAmount = 0.0m, tcsAmount = 0.00m, deliveryAmount = 0.00m, otherAmount1 = 0.00m, otherAmount2 = 0.00m;
        //                decimal.TryParse(txt_tcs_amnt.Text.Trim(), out tcsAmount);
        //                decimal.TryParse(txt_delivery_amnt.Text.Trim(), out deliveryAmount);
        //                decimal.TryParse(txt_othr_amnt1.Text.Trim(), out otherAmount1);
        //                decimal.TryParse(txt_othr_amnt2.Text.Trim(), out otherAmount2);
        //                decimal totalAmount = invAmount + tcsAmount + deliveryAmount + otherAmount1 + otherAmount2;
        //                string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";

        //                SqlCommand cmdMain = new SqlCommand("INSERT INTO tbl_Purches (Purches_Id, Client_Id, Total_purches_rate, Total_Tax_rate, Purches_date, Purches_Type, Invoice_No, Stock_Add_Date, Narration, InvoiceAmnt, TCS_Amount, TCS_Rate, Delivery_Amount, Delivery_Rate, otherAmount1_name, otherAmount1, otherAmount2_name, otherAmount2, AddedById, CreatedDate, TimeStamp) " +
        //                    "VALUES (@Purches_Id, @Client_Id, @Total_purches_rate, @Total_Tax_rate, @Purches_date, @Purches_Type, @Invoice_No, @Stock_Add_Date, @Narration, @InvoiceAmnt, @TCS_Amount, @TCS_Rate, @Delivery_Amount, @Delivery_Rate, @otherAmount1_name, @otherAmount1, @otherAmount2_name, @otherAmount2, @AddedById, @CreatedDate, @TimeStamp);", conn, trans);

        //                cmdMain.Parameters.AddWithValue("@Purches_Id", purchesid);
        //                cmdMain.Parameters.AddWithValue("@Client_Id", lblvendor_id.Text);
        //                cmdMain.Parameters.AddWithValue("@Total_purches_rate", tota_purchesrate1);
        //                cmdMain.Parameters.AddWithValue("@Total_Tax_rate", total_tax_rate_details);
        //                cmdMain.Parameters.AddWithValue("@Purches_date", txtPurchesDate.Text);
        //                cmdMain.Parameters.AddWithValue("@Purches_Type", RadioButtonList1.SelectedValue);
        //                cmdMain.Parameters.AddWithValue("@Invoice_No", txt_invno.Text);
        //                cmdMain.Parameters.AddWithValue("@Stock_Add_Date", txt_stockadddate.Text);
        //                cmdMain.Parameters.AddWithValue("@Narration", txt_narration.Text);
        //                cmdMain.Parameters.AddWithValue("@InvoiceAmnt", invAmount);
        //                cmdMain.Parameters.AddWithValue("@TCS_Amount", tcsAmount);
        //                //cmdMain.Parameters.AddWithValue("@TCS_Rate", DDL_tcspercent.SelectedValue); // Assuming you have a field for TCS Rate 
        //                cmdMain.Parameters.AddWithValue("@TCS_Rate", txt_tcs_percent.Text);
        //                cmdMain.Parameters.AddWithValue("@Delivery_Amount", deliveryAmount);
        //                cmdMain.Parameters.AddWithValue("@Delivery_Rate", DDL_vat_parsentage.SelectedValue);
        //                cmdMain.Parameters.AddWithValue("@otherAmount1_name", TextBox1.Text); // Other Charge-1 Name
        //                cmdMain.Parameters.AddWithValue("@otherAmount1", otherAmount1);
        //                cmdMain.Parameters.AddWithValue("@otherAmount2_name", TextBox2.Text); // Other Charge-2 Name
        //                cmdMain.Parameters.AddWithValue("@otherAmount2", otherAmount2);
        //                cmdMain.Parameters.AddWithValue("@AddedById", userId);
        //                cmdMain.Parameters.AddWithValue("@CreatedDate", DateTime.Now.Date); // Setting CreatedDate to current date
        //                cmdMain.Parameters.AddWithValue("@TimeStamp", DateTime.Now); // Setting Timestamp to current date & time

        //                cmdMain.ExecuteNonQuery();

        //                SqlCommand cmdDue = new SqlCommand("INSERT INTO tbl_purches_due (Purches_Id, Due_amount) VALUES (@Purches_Id, @Due_amount);", conn, trans);
        //                cmdDue.Parameters.AddWithValue("@Purches_Id", purchesid);
        //                cmdDue.Parameters.AddWithValue("@Due_amount", tota_purchesrate1);
        //                cmdDue.ExecuteNonQuery();

        //                trans.Commit();
        //                lblOk.Text = "Data Saved Successfully.....";
        //                PanelOK.Visible = true;
        //                PanelError.Visible = false;

        //                gridtable.Visible = false;
        //            }
        //            catch (Exception ex)
        //            {
        //                trans.Rollback();
        //                ShowErrorMessage(ex.Message);
        //            }
        //        }
        //    }
        //}


        //protected void Button3_Click(object sender, EventArgs e)
        //{
        //    string purchesid = findpurchesId();
        //    int i = 0;
        //    DataTable dt1 = (DataTable)ViewState["dt"];
        //    if (dt1 == null) return;

        //    using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ToString()))
        //    {
        //        conn.Open();
        //        using (SqlTransaction trans = conn.BeginTransaction())
        //        {
        //            try
        //            {
        //                Int32 sl = 0;
        //                double tota_purchesrate1 = 0, total_tax_rate_details = 0;
        //                List<string> errorMessages = new List<string>();

        //                // Loop to insert details into tbl_purches_details
        //                for (i = 0; i < dt1.Rows.Count; i++)
        //                {
        //                    SqlCommand cmd = new SqlCommand()
        //                    {
        //                        CommandType = CommandType.Text,
        //                        Connection = conn,
        //                        Transaction = trans
        //                    };

        //                    string Ser_pro_code = ((Label)gd_Service_Product.Rows[i].FindControl("Ser_pro_code")).Text;
        //                    string Ser_pro_Name = ((Label)gd_Service_Product.Rows[i].FindControl("Ser_pro_Name")).Text;
        //                    string sepecification = ((TextBox)gd_Service_Product.Rows[i].FindControl("sepecification")).Text;
        //                    string Quantity = ((TextBox)gd_Service_Product.Rows[i].FindControl("Quantity")).Text;
        //                    string Vendor_rate = ((TextBox)gd_Service_Product.Rows[i].FindControl("Vendor_rate")).Text;
        //                    string tax_app = ((RadioButtonList)gd_Service_Product.Rows[i].FindControl("RadioButtonList1")).Text;
        //                    string vat_parsentage = ((DropDownList)gd_Service_Product.Rows[i].FindControl("vat_parsentage")).Text;

        //                    // New Fields for Discount and Taxable Amount
        //                    string discountPercent = ((TextBox)gd_Service_Product.Rows[i].FindControl("DiscountPercent")).Text;
        //                    string discountAmount = ((TextBox)gd_Service_Product.Rows[i].FindControl("DiscountAmount")).Text;
        //                    string taxableAmount = ((TextBox)gd_Service_Product.Rows[i].FindControl("TaxableAmount")).Text;

        //                    double qty = 0;
        //                    double vndr = 0;
        //                    double discPercent = 0, discAmount = 0, taxableAmt = 0;

        //                    bool isQuantityValid = !string.IsNullOrEmpty(Quantity) && double.TryParse(Quantity, out qty) && qty > 0;
        //                    bool isVendorRateValid = !string.IsNullOrEmpty(Vendor_rate) && double.TryParse(Vendor_rate, out vndr);
        //                    bool isTaxAppValid = !string.IsNullOrEmpty(tax_app);
        //                    bool isVatValid = !string.IsNullOrEmpty(vat_parsentage) && vat_parsentage != "NA";
        //                    bool isDiscountValid = double.TryParse(discountPercent, out discPercent) && discPercent >= 0;
        //                    bool isDiscountAmountValid = double.TryParse(discountAmount, out discAmount);
        //                    bool isTaxableAmountValid = double.TryParse(taxableAmount, out taxableAmt);

        //                    bool isAnyFieldFilled = isQuantityValid || isVendorRateValid || isTaxAppValid || isVatValid;

        //                    if (isAnyFieldFilled)
        //                    {
        //                        List<string> missingFields = new List<string>();

        //                        if (!isQuantityValid) missingFields.Add("Quantity");
        //                        if (!isVendorRateValid) missingFields.Add("Vendor Rate");
        //                        if (!isTaxAppValid) missingFields.Add("Tax Applicable");

        //                        if (tax_app == "Yes" && !isVatValid)
        //                        {
        //                            missingFields.Add("VAT Percentage");
        //                        }

        //                        if (missingFields.Count > 0)
        //                        {
        //                            errorMessages.Add($"Row {i + 1}: {string.Join(", ", missingFields)} is required.");
        //                            continue;
        //                        }
        //                    }

        //                    sl++;
        //                    double parches_rate = Convert.ToDouble(Vendor_rate) * Convert.ToDouble(Quantity); ///Total Purchase without Discount Calculations
        //                    double discountAmountCalculated = (discPercent > 0) ? (parches_rate * discPercent) / 100 : discAmount;
        //                    double taxableAmountCalculated = parches_rate - discountAmountCalculated;


        //                    double tax_rete = (tax_app == "Yes") ? (taxableAmountCalculated * Convert.ToDouble(vat_parsentage)) / 100 : 0;
        //                    double total_purches_rate = tax_rete + taxableAmountCalculated;

        //                    tota_purchesrate1 += total_purches_rate;
        //                    total_tax_rate_details += tax_rete;

        //                    // Insert into tbl_purches_details including new columns
        //                    cmd.CommandText = "insert into tbl_purches_details(sl_no,Purches_id,Product_id,Product_name,vendor_rate,tax_applicable,tax_rate,Quantity,purches_rate,total_purches_rate,vat_amount,specification,DiscountPercent,DiscountAmount,TaxableAmount,ShippedToLoc,ShippedDate,Purches_date,Client_id)" +
        //                                      "values(@sl_no, @Purches_id, @Product_id, @Product_name, @vendor_rate, @tax_applicable, @tax_rate, @Quantity, @purches_rate, @total_purches_rate, @vat_amount, @specification, @DiscountPercent, @DiscountAmount, @TaxableAmount, @ShippedToLoc, @ShippedDate, @Purches_date, @Client_id)";
        //                    cmd.Parameters.AddWithValue("@sl_no", sl.ToString());
        //                    cmd.Parameters.AddWithValue("@Purches_id", purchesid);
        //                    cmd.Parameters.AddWithValue("@Product_id", Ser_pro_code);
        //                    cmd.Parameters.AddWithValue("@Product_name", Ser_pro_Name);
        //                    cmd.Parameters.AddWithValue("@vendor_rate", Vendor_rate);
        //                    cmd.Parameters.AddWithValue("@tax_applicable", tax_app);
        //                    cmd.Parameters.AddWithValue("@tax_rate", vat_parsentage);
        //                    cmd.Parameters.AddWithValue("@Quantity", Quantity);
        //                    cmd.Parameters.AddWithValue("@purches_rate", parches_rate); //Purchase Rate w/o Discount
        //                    cmd.Parameters.AddWithValue("@total_purches_rate", total_purches_rate);
        //                    cmd.Parameters.AddWithValue("@vat_amount", tax_rete);
        //                    cmd.Parameters.AddWithValue("@specification", sepecification.ToString());
        //                    cmd.Parameters.AddWithValue("@DiscountPercent", discPercent);
        //                    cmd.Parameters.AddWithValue("@DiscountAmount", discountAmountCalculated);
        //                    cmd.Parameters.AddWithValue("@TaxableAmount", taxableAmountCalculated); //Purchase Rate w Discount
        //                    cmd.Parameters.AddWithValue("@ShippedToLoc", DDL_ShippedTo.SelectedValue);
        //                    cmd.Parameters.AddWithValue("@ShippedDate", txt_stockadddate.Text);
        //                    cmd.Parameters.AddWithValue("@Purches_date", txtPurchesDate.Text);
        //                    cmd.Parameters.AddWithValue("@Client_id", lblvendor_id.Text);
        //                    cmd.ExecuteNonQuery();

        //                    updatestock(Ser_pro_code, Ser_pro_Name, Quantity, Vendor_rate, vat_parsentage, DDL_ShippedTo.SelectedValue, DDL_ShippedTo.SelectedItem.Text, txt_stockadddate.Text);
        //                }

        //                // Main insertion into tbl_Purches
        //                decimal invAmount = 0.0m, tcsAmount = 0.00m, deliveryAmount = 0.00m, otherAmount1 = 0.00m, otherAmount2 = 0.00m;
        //                decimal.TryParse(txt_tcs_amnt.Text.Trim(), out tcsAmount);
        //                decimal.TryParse(txt_delivery_amnt.Text.Trim(), out deliveryAmount);
        //                decimal.TryParse(txt_othr_amnt1.Text.Trim(), out otherAmount1);
        //                decimal.TryParse(txt_othr_amnt2.Text.Trim(), out otherAmount2);
        //                decimal totalAmount = invAmount + tcsAmount + deliveryAmount + otherAmount1 + otherAmount2;
        //                string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";

        //                SqlCommand cmdMain = new SqlCommand("INSERT INTO tbl_Purches (Purches_Id, Client_Id, Total_purches_rate, Total_Tax_rate, Purches_date, Purches_Type, Invoice_No, Stock_Add_Date, BuyerOrderNo, OrderDate, Narration, InvoiceAmnt, TCS_Amount, TCS_Rate, Delivery_Amount, Delivery_Rate, otherAmount1_name, otherAmount1, otherAmount2_name, otherAmount2, AddedById, CreatedDate, TimeStamp, ShippedToStoreId, ShippedToStoreName) " +
        //                    "VALUES (@Purches_Id, @Client_Id, @Total_purches_rate, @Total_Tax_rate, @Purches_date, @Purches_Type, @Invoice_No, @Stock_Add_Date, @BuyerOrderNo, @OrderDate, @Narration, @InvoiceAmnt, @TCS_Amount, @TCS_Rate, @Delivery_Amount, @Delivery_Rate, @otherAmount1_name, @otherAmount1, @otherAmount2_name, @otherAmount2, @AddedById, @CreatedDate, @TimeStamp, @ShippedToStoreId, @ShippedToStoreName );", conn, trans);

        //                cmdMain.Parameters.AddWithValue("@Purches_Id", purchesid);
        //                cmdMain.Parameters.AddWithValue("@Client_Id", lblvendor_id.Text);
        //                cmdMain.Parameters.AddWithValue("@Total_purches_rate", tota_purchesrate1);
        //                cmdMain.Parameters.AddWithValue("@Total_Tax_rate", total_tax_rate_details);
        //                cmdMain.Parameters.AddWithValue("@Purches_date", txtPurchesDate.Text);
        //                cmdMain.Parameters.AddWithValue("@Purches_Type", RadioButtonList1.SelectedValue);
        //                cmdMain.Parameters.AddWithValue("@Invoice_No", txt_invno.Text);
        //                cmdMain.Parameters.AddWithValue("@Stock_Add_Date", txt_stockadddate.Text);

        //                cmdMain.Parameters.AddWithValue("@BuyerOrderNo", string.IsNullOrEmpty(txt_reforder.Text) ? (object)DBNull.Value : txt_reforder.Text);
        //                cmdMain.Parameters.AddWithValue("@OrderDate", string.IsNullOrEmpty(txt_refordrdate.Text) ? (object)DBNull.Value : txt_refordrdate.Text);

        //                cmdMain.Parameters.AddWithValue("@Narration", txt_narration.Text);
        //                cmdMain.Parameters.AddWithValue("@InvoiceAmnt", invAmount);
        //                cmdMain.Parameters.AddWithValue("@TCS_Amount", tcsAmount);
        //                cmdMain.Parameters.AddWithValue("@TCS_Rate", txt_tcs_percent.Text);
        //                cmdMain.Parameters.AddWithValue("@Delivery_Amount", deliveryAmount);
        //                cmdMain.Parameters.AddWithValue("@Delivery_Rate", DDL_vat_parsentage.SelectedValue);
        //                cmdMain.Parameters.AddWithValue("@otherAmount1_name", TextBox1.Text);
        //                cmdMain.Parameters.AddWithValue("@otherAmount1", otherAmount1);
        //                cmdMain.Parameters.AddWithValue("@otherAmount2_name", TextBox2.Text);
        //                cmdMain.Parameters.AddWithValue("@otherAmount2", otherAmount2);
        //                cmdMain.Parameters.AddWithValue("@AddedById", userId);
        //                cmdMain.Parameters.AddWithValue("@CreatedDate", DateTime.Now.Date);
        //                cmdMain.Parameters.AddWithValue("@TimeStamp", DateTime.Now);
        //                cmdMain.Parameters.AddWithValue("@ShippedToStoreId", DDL_ShippedTo.SelectedValue);
        //                cmdMain.Parameters.AddWithValue("@ShippedToStoreName", DDL_ShippedTo.SelectedItem.Text);

        //                cmdMain.ExecuteNonQuery();

        //                SqlCommand cmdDue = new SqlCommand("INSERT INTO tbl_purches_due (Purches_Id, Due_amount) VALUES (@Purches_Id, @Due_amount);", conn, trans);
        //                cmdDue.Parameters.AddWithValue("@Purches_Id", purchesid);
        //                cmdDue.Parameters.AddWithValue("@Due_amount", tota_purchesrate1);
        //                cmdDue.ExecuteNonQuery();

        //                trans.Commit();
        //                lblOk.Text = "Data Saved Successfully.....";
        //                PanelOK.Visible = true;
        //                PanelError.Visible = false;
        //                gridtable.Visible = false;
        //            }
        //            catch (Exception ex)
        //            {
        //                trans.Rollback();
        //                ShowErrorMessage(ex.Message);
        //            }
        //        }
        //    }
        //}


        private T FindControlSafe<T>(GridViewRow row, string controlId, StringBuilder logBuilder, int rowIndex) where T : Control
        {
            T control = row.FindControl(controlId) as T;
            if (control == null)
            {
                logBuilder.AppendLine($"[ROW {rowIndex + 1}] Missing control: {controlId}");
            }
            return control;
        }

        private bool ValidateFormFields(out string errorMessage)
        {
            List<string> missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(txtPurchesDate.Text)) missingFields.Add("Purchase Date");
            if (string.IsNullOrWhiteSpace(txt_stockadddate.Text)) missingFields.Add("Stock Add Date");
            if (string.IsNullOrWhiteSpace(lblvendor_id.Text)) missingFields.Add("Vendor ID");
            if (DDL_ShippedTo.SelectedIndex <= 0) missingFields.Add("Shipped To Location");

            if (missingFields.Count > 0)
            {
                errorMessage = "Missing required form fields: " + string.Join(", ", missingFields);
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            StringBuilder logBuilder = new StringBuilder();
            string purchesid = findpurchesId();
            logBuilder.AppendLine($"[INFO] Starting purchase entry. Purchase ID: {purchesid}");

            string topFormError ="";
            if (!ValidateFormFields(out topFormError))
            {
                ShowErrorMessage(topFormError);
                return;
            }

            // Step 1: Validate and collect data
            DataTable dt = ((DataTable)ViewState["dt"]);
            if (dt == null)
            {
                ShowErrorMessage("[ERROR] No data found in ViewState.");
                return;
            }

            // Step 2: Update Order column and validate
            List<string> validationErrors = new List<string>();
            List<DataRow> validRows = new List<DataRow>();

            for (int i = 0; i < gd_Service_Product.Rows.Count; i++)
            {
                GridViewRow row = gd_Service_Product.Rows[i];
                string rowHeader = $"[ROW {i + 1}]";

                TextBox txtOrder = FindControlSafe<TextBox>(row, "txtOrder", logBuilder, i);
                int orderVal = 0;
                if (txtOrder != null && int.TryParse(txtOrder.Text.Trim(), out orderVal))
                    dt.Rows[i]["Order"] = orderVal;
                else
                    dt.Rows[i]["Order"] = 9999;
            }

            DataView sortedView = dt.DefaultView;
            sortedView.Sort = "Order ASC";
            DataTable sortedTable = sortedView.ToTable();

            List<ValidatedRow> parsedRows = new List<ValidatedRow>();
            for (int i = 0; i < sortedTable.Rows.Count; i++)
            {
                GridViewRow row = gd_Service_Product.Rows[i];
                string rowHeader = $"[ROW {i + 1}]";

                var lblCode = FindControlSafe<Label>(row, "Ser_pro_code", logBuilder, i);
                var lblName = FindControlSafe<Label>(row, "Ser_pro_Name", logBuilder, i);
                var txtSpec = FindControlSafe<TextBox>(row, "sepecification", logBuilder, i);
                var txtQty = FindControlSafe<TextBox>(row, "Quantity", logBuilder, i);
                var txtRate = FindControlSafe<TextBox>(row, "Vendor_rate", logBuilder, i);
                var rblTax = FindControlSafe<RadioButtonList>(row, "RadioButtonList1", logBuilder, i);
                var ddlVat = FindControlSafe<DropDownList>(row, "vat_parsentage", logBuilder, i);
                var txtDiscPct = FindControlSafe<TextBox>(row, "DiscountPercent", logBuilder, i);
                var txtDiscAmt = FindControlSafe<TextBox>(row, "DiscountAmount", logBuilder, i);
                var txtTaxAmt = FindControlSafe<TextBox>(row, "TaxableAmount", logBuilder, i);
                var txtOrder = FindControlSafe<TextBox>(row, "txtOrder", logBuilder, i);

                if (lblCode == null || lblName == null || txtSpec == null || txtQty == null || txtRate == null ||
                    rblTax == null || ddlVat == null || txtDiscPct == null || txtDiscAmt == null || txtTaxAmt == null || txtOrder == null)
                {
                    validationErrors.Add($"{rowHeader} Missing one or more required fields.");
                    continue;
                }

                // Skip row if either Quantity or Rate is blank
                if (string.IsNullOrWhiteSpace(txtQty.Text) || string.IsNullOrWhiteSpace(txtRate.Text))
                {
                    //logBuilder.AppendLine($"{rowHeader} Skipped (Quantity or Rate not provided).");
                    continue;
                }
                double qty=0; double rate=0; double discPct=0; double discAmt=0;
                if (!double.TryParse(txtQty.Text, out qty) || qty <= 0 ||
                    !double.TryParse(txtRate.Text, out rate))
                {
                    validationErrors.Add($"{rowHeader} Invalid Quantity or Vendor Rate.");
                    continue;
                }
                
                double.TryParse(txtDiscPct.Text, out discPct);
                double.TryParse(txtDiscAmt.Text, out discAmt);
                double vat = 0;
                if (rblTax.SelectedValue == "Yes")
                {
                    if (!double.TryParse(ddlVat.SelectedValue, out vat))
                    {
                        validationErrors.Add($"{rowHeader} VAT required but invalid.");
                        continue;
                    }
                }

                double purchesRate = qty * rate;
                double discount = discPct > 0 ? purchesRate * discPct / 100 : discAmt;
                double taxable = purchesRate - discount;
                double tax = rblTax.SelectedValue == "Yes" ? taxable * vat / 100 : 0;
                double totalRate = taxable + tax;

                parsedRows.Add(new ValidatedRow
                {
                    RowIndex = i,
                    Code = lblCode.Text,
                    Name = lblName.Text,
                    Specification = txtSpec.Text,
                    Quantity = qty,
                    Rate = rate,
                    TaxApplicable = rblTax.SelectedValue,
                    VatRate = ddlVat.SelectedValue,
                    DiscountPercent = discPct,
                    DiscountAmount = discount,
                    TaxableAmount = taxable,
                    TaxAmount = tax,
                    TotalPurchaseRate = totalRate,
                    Order = txtOrder.Text
                });
            }

            if (validationErrors.Count > 0)
            {
                ShowErrorMessage(string.Join("<br/>", validationErrors));
                return;
            }

            // Step 3: Insert inside transaction
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ToString()))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    double grandTotal = 0, grandTax = 0;
                    foreach (var item in parsedRows)
                    {
                        SqlCommand cmd = new SqlCommand(@"INSERT INTO tbl_purches_details 
                (sl_no, Purches_id, Product_id, Product_name, vendor_rate, tax_applicable, tax_rate, Quantity, purches_rate, total_purches_rate, vat_amount, specification, DiscountPercent, DiscountAmount, TaxableAmount, ShippedToLoc, ShippedDate, Purches_date, Client_id) 
                VALUES 
                (@sl_no, @Purches_id, @Product_id, @Product_name, @vendor_rate, @tax_applicable, @tax_rate, @Quantity, @purches_rate, @total_purches_rate, @vat_amount, @specification, @DiscountPercent, @DiscountAmount, @TaxableAmount, @ShippedToLoc, @ShippedDate, @Purches_date, @Client_id)", conn, trans);

                        cmd.Parameters.AddWithValue("@sl_no", item.Order);
                        cmd.Parameters.AddWithValue("@Purches_id", purchesid);
                        cmd.Parameters.AddWithValue("@Product_id", item.Code);
                        cmd.Parameters.AddWithValue("@Product_name", item.Name);
                        cmd.Parameters.AddWithValue("@vendor_rate", item.Rate);
                        cmd.Parameters.AddWithValue("@tax_applicable", item.TaxApplicable);
                        cmd.Parameters.AddWithValue("@tax_rate", item.VatRate);
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@purches_rate", item.Quantity * item.Rate);
                        cmd.Parameters.AddWithValue("@total_purches_rate", item.TotalPurchaseRate);
                        cmd.Parameters.AddWithValue("@vat_amount", item.TaxAmount);
                        cmd.Parameters.AddWithValue("@specification", item.Specification);
                        cmd.Parameters.AddWithValue("@DiscountPercent", item.DiscountPercent);
                        cmd.Parameters.AddWithValue("@DiscountAmount", item.DiscountAmount);
                        cmd.Parameters.AddWithValue("@TaxableAmount", item.TaxableAmount);
                        cmd.Parameters.AddWithValue("@ShippedToLoc", DDL_ShippedTo.SelectedValue);
                        cmd.Parameters.AddWithValue("@ShippedDate", txt_stockadddate.Text);
                        cmd.Parameters.AddWithValue("@Purches_date", txtPurchesDate.Text);
                        cmd.Parameters.AddWithValue("@Client_id", lblvendor_id.Text);
                        cmd.ExecuteNonQuery();
                        logBuilder.AppendLine("[SUCCESS] Purchase Records Added successfully.");
                        updatestock(item.Code, item.Name, item.Quantity.ToString(), item.Rate.ToString(), item.VatRate, DDL_ShippedTo.SelectedValue, DDL_ShippedTo.SelectedItem.Text, txt_stockadddate.Text);
                        logBuilder.AppendLine("[SUCCESS] Sock Records Added / Updated successfully.");
                        grandTotal += item.TotalPurchaseRate;
                        grandTax += item.TaxAmount;
                    }


                    SqlCommand cmdMain = new SqlCommand(@"INSERT INTO tbl_Purches (Purches_Id, Client_Id, Total_purches_rate, Total_Tax_rate, Purches_date, Purches_Type, Invoice_No, Stock_Add_Date, BuyerOrderNo, OrderDate, Narration, InvoiceAmnt, TCS_Amount, TCS_Rate, Delivery_Amount, Delivery_Rate, otherAmount1_name, otherAmount1, otherAmount2_name, otherAmount2, AddedById, CreatedDate, TimeStamp, ShippedToStoreId, ShippedToStoreName) VALUES 
                    (@Purches_Id, @Client_Id, @Total_purches_rate, @Total_Tax_rate, @Purches_date, @Purches_Type, @Invoice_No, @Stock_Add_Date, @BuyerOrderNo, @OrderDate, @Narration, @InvoiceAmnt, @TCS_Amount, @TCS_Rate, @Delivery_Amount, @Delivery_Rate, @otherAmount1_name, @otherAmount1, @otherAmount2_name, @otherAmount2, @AddedById, @CreatedDate, @TimeStamp, @ShippedToStoreId, @ShippedToStoreName);", conn, trans);

                    decimal tcsAmount = 0.00m, deliveryAmount = 0.00m, otherAmount1 = 0.00m, otherAmount2 = 0.00m;
                    string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";

                    decimal.TryParse(txt_tcs_amnt.Text, out tcsAmount);
                    decimal.TryParse(txt_delivery_amnt.Text, out deliveryAmount);
                    decimal.TryParse(txt_othr_amnt1.Text, out otherAmount1);
                    decimal.TryParse(txt_othr_amnt2.Text, out otherAmount2);

                    cmdMain.Parameters.AddWithValue("@Purches_Id", string.IsNullOrWhiteSpace(purchesid) ? Guid.NewGuid().ToString() : purchesid);
                    cmdMain.Parameters.AddWithValue("@Client_Id", string.IsNullOrWhiteSpace(lblvendor_id.Text) ? (object)DBNull.Value : lblvendor_id.Text);
                    cmdMain.Parameters.AddWithValue("@Total_purches_rate", grandTotal > 0 ? grandTotal : 0);
                    cmdMain.Parameters.AddWithValue("@Total_Tax_rate", grandTax >= 0 ? grandTax : 0);
                    cmdMain.Parameters.AddWithValue("@Purches_date", txtPurchesDate.Text);
                    cmdMain.Parameters.AddWithValue("@Purches_Type", string.IsNullOrWhiteSpace(RadioButtonList1.SelectedValue) ? "Product" : RadioButtonList1.SelectedValue);
                    cmdMain.Parameters.AddWithValue("@Invoice_No", string.IsNullOrWhiteSpace(txt_invno.Text) ? "NA" : txt_invno.Text);
                    cmdMain.Parameters.AddWithValue("@Stock_Add_Date", string.IsNullOrWhiteSpace(txt_stockadddate.Text) ? (object)DBNull.Value : txt_stockadddate.Text);
                    cmdMain.Parameters.AddWithValue("@BuyerOrderNo", string.IsNullOrWhiteSpace(txt_reforder.Text) ? (object)DBNull.Value : txt_reforder.Text);
                    cmdMain.Parameters.AddWithValue("@OrderDate", string.IsNullOrWhiteSpace(txt_refordrdate.Text) ? (object)DBNull.Value : txt_refordrdate.Text);
                    cmdMain.Parameters.AddWithValue("@Narration", string.IsNullOrWhiteSpace(txt_narration.Text) ? "NA" : txt_narration.Text);
                    cmdMain.Parameters.AddWithValue("@InvoiceAmnt", string.IsNullOrWhiteSpace(txt_inv_amount.Text) ? "0.00" : txt_inv_amount.Text);
                    cmdMain.Parameters.AddWithValue("@TCS_Amount", tcsAmount);
                    cmdMain.Parameters.AddWithValue("@TCS_Rate", string.IsNullOrWhiteSpace(txt_tcs_percent.Text) ? "0" : txt_tcs_percent.Text);
                    cmdMain.Parameters.AddWithValue("@Delivery_Amount", deliveryAmount);
                    cmdMain.Parameters.AddWithValue("@Delivery_Rate", string.IsNullOrWhiteSpace(DDL_vat_parsentage.SelectedValue) ? "0" : DDL_vat_parsentage.SelectedValue);
                    cmdMain.Parameters.AddWithValue("@otherAmount1_name", string.IsNullOrWhiteSpace(TextBox1.Text) ? "" : TextBox1.Text);
                    cmdMain.Parameters.AddWithValue("@otherAmount1", otherAmount1);
                    cmdMain.Parameters.AddWithValue("@otherAmount2_name", string.IsNullOrWhiteSpace(TextBox2.Text) ? "" : TextBox2.Text);
                    cmdMain.Parameters.AddWithValue("@otherAmount2", otherAmount2);
                    cmdMain.Parameters.AddWithValue("@AddedById", userId);
                    cmdMain.Parameters.AddWithValue("@CreatedDate", DateTime.Now.Date);
                    cmdMain.Parameters.AddWithValue("@TimeStamp", DateTime.Now);
                    cmdMain.Parameters.AddWithValue("@ShippedToStoreId", string.IsNullOrWhiteSpace(DDL_ShippedTo.SelectedValue) ? "STR001" : DDL_ShippedTo.SelectedValue);
                    cmdMain.Parameters.AddWithValue("@ShippedToStoreName", string.IsNullOrWhiteSpace(DDL_ShippedTo.SelectedItem?.Text) ? "Central Warehouse [Jamshedpur]" : DDL_ShippedTo.SelectedItem.Text);
                    cmdMain.ExecuteNonQuery();
                    logBuilder.AppendLine("[SUCCESS] Purchase Register Updated successfully.");
                    SqlCommand cmdDue = new SqlCommand("INSERT INTO tbl_purches_due (Purches_Id, Due_amount) VALUES (@Purches_Id, @Due_amount)", conn, trans);
                    cmdDue.Parameters.AddWithValue("@Purches_Id", purchesid);
                    cmdDue.Parameters.AddWithValue("@Due_amount", grandTotal);
                    cmdDue.ExecuteNonQuery();

                    trans.Commit();
                    logBuilder.AppendLine("[SUCCESS] All Purchase data committed successfully.");
                    lblOk.Text = logBuilder.ToString().Replace("\n", "<br/>");
                    PanelOK.Visible = true;
                    PanelError.Visible = false;
                    gridtable.Visible = false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    ShowErrorMessage("[ERROR] Transaction failed: " + ex.Message);
                }
            }
        }

        class ValidatedRow
        {
            public int RowIndex;
            public string Code;
            public string Name;
            public string Specification;
            public double Quantity;
            public double Rate;
            public string TaxApplicable;
            public string VatRate;
            public double DiscountPercent;
            public double DiscountAmount;
            public double TaxableAmount;
            public double TaxAmount;
            public double TotalPurchaseRate;
            public string Order;
        }


        private void ShowLogMessage(string message, bool success, string purchesId = "")
        {
            lblLog.Text = "<pre>" + message + "</pre>";
            lblOk.Text = success ? $"✅ Purchase saved successfully. ID: {purchesId}" : "❌ Failed to save purchase.";
            PanelOK.Visible = success;
            PanelError.Visible = !success;
            gridtable.Visible = false;
        }


        private void ShowErrorMessage(string message)
        {
            PanelError.Visible = true;
            lblErrorMsg.Text = message;
        }

        //private void updatestock(string Ser_pro_code, string Ser_pro_Name, string Quantity1, string Sale_rate, string service_Tax_Rate)
        //{
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    string cmdstring = "select Product_id from tbl_stock where  Product_id='" + Ser_pro_code + "'";
        //    SqlCommand cmd10 = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataReader re = cmd10.ExecuteReader();
        //    if (re.Read())
        //    {
        //        //DbCL.executeRdr("UPDATE tbl_stock SET Quantity = CAST(CAST(Quantity AS DECIMAL(18,3)) + " + Quantity1 + " AS VARCHAR(50)), Sail_Rate = '" + Sale_rate + "', Service_tax_rate = '" + service_Tax_Rate + "' WHERE Product_id = '" + Ser_pro_code + "' AND Product_name = '" + Ser_pro_Name + "'");

        //        //DbCL.executeRdr("UPDATE tbl_NewProduct SET Quantity = CAST(CAST(Quantity AS DECIMAL(18,3)) + " + Quantity1 + " AS VARCHAR(50)), Sail_Rate = '" + Sale_rate + "', Tax_Rate = '" + service_Tax_Rate + "' WHERE ProductID = '" + Ser_pro_code + "' AND ProductName = '" + Ser_pro_Name + "'");

        //        DbCL.executeRdr(
        //            "UPDATE tbl_stock " +
        //            "SET Quantity = CAST(CAST(Quantity AS DECIMAL(18,3)) + " + Quantity1 + " AS VARCHAR(50)), " +
        //            "Sail_Rate = '" + Sale_rate + "', " +
        //            "Service_tax_rate = '" + service_Tax_Rate + "' " +
        //            "WHERE Product_id = '" + Ser_pro_code + "' AND Product_name = '" + Ser_pro_Name + "' " +
        //            "AND ISNUMERIC(Quantity) = 1"
        //        );

        //        DbCL.executeRdr(
        //            "UPDATE tbl_NewProduct " +
        //            "SET Quantity = CAST(CAST(Quantity AS DECIMAL(18,3)) + " + Quantity1 + " AS NVARCHAR(100)), " +
        //            "Sail_Rate = '" + Sale_rate + "', " +
        //            "Tax_Rate = '" + service_Tax_Rate + "' " +
        //            "WHERE ProductID = '" + Ser_pro_code + "' AND ProductName = '" + Ser_pro_Name + "' " +
        //            "AND ISNUMERIC(Quantity) = 1"
        //        );


        //    }
        //    else
        //    {
        //        //DbCL.executeRdr("insert into tbl_stock(Product_id,Product_name,Quantity,Sail_Rate,Service_tax_rate)values('" + Ser_pro_code + "','" + Ser_pro_Name + "','" + Quantity1 + "','" + Sale_rate + "','" + service_Tax_Rate + "')");

        //        //DbCL.executeRdr("INSERT INTO tbl_NewProduct (ProductID, ProductName, Quantity, Sail_Rate, Tax_Rate) VALUES ('" + Ser_pro_code + "', '" + Ser_pro_Name + "', '" + Quantity1 + "', '" + Sale_rate + "', '" + service_Tax_Rate + "')");

        //        DbCL.executeRdr(
        //            "INSERT INTO tbl_stock (Product_id, Product_name, Quantity, Sail_Rate, Service_tax_rate) " +
        //            "VALUES ('" + Ser_pro_code + "', '" + Ser_pro_Name + "', '" + Quantity1 + "', '" + Sale_rate + "', '" + service_Tax_Rate + "')"
        //        );

        //        DbCL.executeRdr(
        //            "INSERT INTO tbl_NewProduct (ProductID, ProductName, Quantity, Sail_Rate, Tax_Rate) " +
        //            "VALUES ('" + Ser_pro_code + "', '" + Ser_pro_Name + "', '" + Quantity1 + "', '" + Sale_rate + "', '" + service_Tax_Rate + "')"
        //        );
        //    }
        //    DbCL.Conn.Close();
        //}

        private void updatestock(string Ser_pro_code, string Ser_pro_Name, string Quantity1, string Sale_rate, string service_Tax_Rate, string ShippedToStoreId, string ShippedToStoreName, string ShippedDate)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "SELECT Product_id FROM tbl_stock WHERE Product_id = '" + Ser_pro_code + "' and ShippedToStoreId = '"+ ShippedToStoreId + "'";
            SqlCommand cmd10 = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd10.ExecuteReader();

            if (re.Read())
            {
                //// If product exists, update
                //DbCL.executeRdr(
                //    "UPDATE tbl_stock " +
                //    "SET Quantity = CAST(CAST(Quantity AS DECIMAL(18,3)) + " + Quantity1 + " AS VARCHAR(50)), " +
                //    "Sail_Rate = '" + Sale_rate + "', " +
                //    "Service_tax_rate = '" + service_Tax_Rate + "', " +
                //    "ShippedToStoreId = '" + ShippedToStoreId + "', " +
                //    "ShippedToStoreName = '" + ShippedToStoreName + "', " +
                //    "ShippedDate = '" + ShippedDate + "' " +
                //    "WHERE Product_id = '" + Ser_pro_code + "' AND Product_name = '" + Ser_pro_Name + "' " +
                //    "AND ISNUMERIC(Quantity) = 1"
                //);

                //DbCL.executeRdr(
                //    "UPDATE tbl_NewProduct " +
                //    "SET Quantity = CAST(CAST(Quantity AS DECIMAL(18,3)) + " + Quantity1 + " AS NVARCHAR(100)), " +
                //    "Sail_Rate = '" + Sale_rate + "', " +
                //    "Tax_Rate = '" + service_Tax_Rate + "' " +
                //    "WHERE ProductID = '" + Ser_pro_code + "' AND ProductName = '" + Ser_pro_Name + "' " +
                //    "AND ISNUMERIC(Quantity) = 1"
                //);

                // Update tbl_stock based on combination of Product_id (Ser_pro_code) and ShippedToStoreId
                DbCL.executeRdr(
                    "UPDATE tbl_stock " +
                    "SET Quantity = CAST(CAST(Quantity AS DECIMAL(18,3)) + " + Quantity1 + " AS VARCHAR(50)), " +
                    "Sail_Rate = '" + Sale_rate + "', " +
                    "Service_tax_rate = '" + service_Tax_Rate + "', " +
                    "ShippedToStoreId = '" + ShippedToStoreId + "', " +
                    "ShippedToStoreName = '" + ShippedToStoreName + "', " +
                    "ModifiedOn = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                    "ModifiedByUserId = '" + Session["USERID"]?.ToString() + "', " +
                    "ShippedDate = '" + ShippedDate + "' " +
                    "WHERE Product_id = '" + Ser_pro_code + "' AND ShippedToStoreId = '" + ShippedToStoreId + "' " +
                    "AND ISNUMERIC(Quantity) = 1"
                );

                // Update tbl_NewProduct with the same logic
                DbCL.executeRdr(
                    "UPDATE tbl_NewProduct " +
                    "SET Quantity = CAST(CAST(Quantity AS DECIMAL(18,3)) + " + Quantity1 + " AS NVARCHAR(100)), " +
                    "Sail_Rate = '" + Sale_rate + "', " +
                    "Tax_Rate = '" + service_Tax_Rate + "', " +
                    "ModifiedOn = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                    "ModifiedByUserId = '" + Session["USERID"]?.ToString() + "' " +
                    "WHERE ProductID = '" + Ser_pro_code + "' AND ProductName = '" + Ser_pro_Name + "' " +
                    "AND ISNUMERIC(Quantity) = 1"
                );
            }
            else
            {
                // If product doesn't exist, insert new
                //DbCL.executeRdr(
                //    "INSERT INTO tbl_stock (Product_id, Product_name, Quantity, Sail_Rate, Service_tax_rate, ShippedToStoreId, ShippedToStoreName, ShippedDate) " +
                //    "VALUES ('" + Ser_pro_code + "', '" + Ser_pro_Name + "', '" + Quantity1 + "', '" + Sale_rate + "', '" + service_Tax_Rate + "', '" + ShippedToStoreId + "', '" + ShippedToStoreName + "', '" + ShippedDate + "')"
                //);

                // If no record found, insert a new record for tbl_stock
                DbCL.executeRdr(
                    "INSERT INTO tbl_stock (Product_id, Product_name, Quantity, Sail_Rate, Service_tax_rate, ShippedToStoreId, ShippedToStoreName, ShippedDate) " +
                    "VALUES ('" + Ser_pro_code + "', '" + Ser_pro_Name + "', '" + Quantity1 + "', '" + Sale_rate + "', '" + service_Tax_Rate + "', '" + ShippedToStoreId + "', '" + ShippedToStoreName + "', '" + ShippedDate + "')"
                );

                DbCL.executeRdr(
                    "UPDATE tbl_NewProduct " +
                    "SET Quantity = CAST(CAST(Quantity AS DECIMAL(18,3)) + " + Quantity1 + " AS NVARCHAR(100)), " +
                    "Sail_Rate = '" + Sale_rate + "', " +
                    "Tax_Rate = '" + service_Tax_Rate + "', " +
                    "ModifiedOn = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                    "ModifiedByUserId = '" + Session["USERID"]?.ToString() + "' " +
                    "WHERE ProductID = '" + Ser_pro_code + "' AND ProductName = '" + Ser_pro_Name + "' " +
                    "AND ISNUMERIC(Quantity) = 1"
                );

                //DbCL.executeRdr(
                //    "INSERT INTO tbl_NewProduct (ProductID, ProductName, Quantity, Sail_Rate, Tax_Rate) " +
                //    "VALUES ('" + Ser_pro_code + "', '" + Ser_pro_Name + "', '" + Quantity1 + "', '" + Sale_rate + "', '" + service_Tax_Rate + "')"
                //);
            }

            DbCL.Conn.Close();
        }


        private string findpurchesId()
        {
            string PurID = "";
            string aa = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdString1 = "select ID,Purches_Id from tbl_Purches where ID=(select max(ID)from tbl_Purches)";
            SqlCommand com1 = new SqlCommand(cmdString1, DbCL.Conn);
            SqlDataReader DR1 = com1.ExecuteReader();
            if (DR1.Read())
            {
                aa = DR1.GetValue(1).ToString();
                string bb = aa.Substring(4);
                int k = Convert.ToInt32(bb);
                k = k + 1;
                string q = Convert.ToString(k);
                PurID = "PR00" + q;
            }
            else
            {
                PurID = "PR001";
            }

            DbCL.Conn.Close();
            return PurID;
        }

        protected void cmbvendor_SelectedIndexChanged(object sender, EventArgs e)
        {
            Label1.Visible = true;
            RadioButtonList1.Visible = true;
            Button1.Visible = true;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Vendor where Vendor_Name='" + cmbvendor.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblvendor_id.Text = re["Vendor_Id"].ToString();
                txtAddress1.Text = re["Address1"].ToString();
                txtAddress2.Text = re["Address2"].ToString();
                cmbcity.Text = re["City"].ToString();
                txtPin.Text = re["pin"].ToString();
                cmbState.Text = re["State"].ToString();
                txtWebsite.Text = re["Com_web_site"].ToString();
                txtEmail.Text = re["Com_email"].ToString();
                txtPhone.Text = re["Com_phone"].ToString();
                txtFax.Text = re["Com_Fax"].ToString();
                txtRepresentativeName.Text = re["Rep_Name"].ToString();
                txtRepresantativeDesig.Text = re["Rep_Desig"].ToString();
                txtRepresentativePhone.Text = re["Rep_phone"].ToString();
                txtRepresentativeEmail.Text = re["Rep_email"].ToString();
                txtservicetaxNo.Text = re["Service_tax_No"].ToString();
                txtpanNo.Text = re["Pan_No"].ToString();
                txtvat.Text = re["Vat_No"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnpurchess_save_Click(object sender, EventArgs e)
        {
            if (Convert.ToDouble(lblpaayment_amount.Text) < Convert.ToDouble(txtpaymentamount.Text))
            {
                lblErrorMsg.Text = "Due Amount Is less Than Given Amount...";
                PanelError.Visible = true;
            }
            else
            {
                InserttotalDate();
                PanelOK.Visible = true;
                lblOk.Text = "Data Save Successfully...";
                btnpurchess_save.Visible = false;
                PanelError.Visible = false;
            }
        }

        private void InserttotalDate()
        {
            string paymentid = BindpaymentId();

            string comma = ",";
            string dated = " Dated:";
            string date1 = "";
            string no = "";
            string bank = "";
            double due = Convert.ToDouble(lblpaayment_amount.Text) - Convert.ToDouble(txtpaymentamount.Text);
            string due1 = due.ToString();

            if (RadioButtonList2.SelectedIndex == 0)
            {

                date1 = dated + txtcashDate.Text;

            }
            else if (RadioButtonList2.SelectedIndex == 3)
            {
                date1 = dated + txtneftdate.Text;
                no = txtneftnumber.Text + comma;
                bank = txtbankname1.Text;

            }
            else
            {
                date1 = dated + txtdddate.Text;
                no = txtDDno.Text + comma;
                bank = txtBankName.Text;
            }
            string cmdstring = "insert into tbl_Purchess_payment(Payment_ID,Payment_Date,Purchess_ID,Purchess_Date,Client_Id,Net_amount,Given_amount,type,Ch_no,Ch_bank,Ch_date,Due_amount)values(@Payment_ID,@Payment_Date,@Purchess_ID,@Purchess_Date,@Client_Id,@Net_amount,@Given_amount,@type,@Ch_no,@Ch_bank,@Ch_date,@Due_amount)";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 0;
            cmd.Parameters.AddWithValue("@Payment_ID", paymentid.ToString());
            cmd.Parameters.AddWithValue("@Payment_Date", txtpaymentdate.Text);
            cmd.Parameters.AddWithValue("@Purchess_ID", lblpuechess_id.Text);
            cmd.Parameters.AddWithValue("@Purchess_Date", txtPurchesDate.Text);
            cmd.Parameters.AddWithValue("@Client_Id", lblvendor_id.Text);
            cmd.Parameters.AddWithValue("@Net_amount", lblpaayment_amount.Text);
            cmd.Parameters.AddWithValue("@Given_amount", txtpaymentamount.Text);
            cmd.Parameters.AddWithValue("@type", RadioButtonList2.Text);
            cmd.Parameters.AddWithValue("@Ch_no", no.ToString());
            cmd.Parameters.AddWithValue("@Ch_bank", bank.ToString());
            cmd.Parameters.AddWithValue("@Ch_date", date1.ToString());
            cmd.Parameters.AddWithValue("@Due_amount", due1.ToString());

            cmd.ExecuteNonQuery();
            DbCL.Conn.Close();
            DbCL.executeRdr("update tbl_purches_due set Due_amount='" + due1.ToString() + "' where Purches_Id='" + lblpuechess_id.Text + "'");
        }
        private string BindpaymentId()
        {
            string paymentIDdetai = "";
            string aa = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdString1 = "select ID,Payment_ID from tbl_Purchess_payment where ID=(select max(ID)from tbl_Purchess_payment)";
            SqlCommand com1 = new SqlCommand(cmdString1, DbCL.Conn);
            SqlDataReader DR1 = com1.ExecuteReader();
            if (DR1.Read())
            {
                aa = DR1.GetValue(1).ToString();
                string bb = aa.Substring(3);
                int k = Convert.ToInt32(bb);
                k = k + 1;
                string q = Convert.ToString(k);
                paymentIDdetai = "PN0" + q;
            }
            else
            {
                paymentIDdetai = "PN01";
            }

            DbCL.Conn.Close();
            return paymentIDdetai;
        }
    }
}