using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

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
                txtPurchesDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtcashDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtdddate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtneftdate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtpaymentdate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
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

        private void newgrid1()
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


        private void newgrid()
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

        protected void Button3_Click(object sender, EventArgs e)
        {
            string purchesid = findpurchesId();
            int i = 0;
            DataTable dt1 = (DataTable)ViewState["dt"];
            if (dt1 == null) return;

            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ToString()))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        Int32 sl = 0;
                        double tota_purchesrate1 = 0, total_tax_rate_details = 0;
                        List<string> errorMessages = new List<string>();
                        for (i = 0; i < dt1.Rows.Count; i++)
                        {
                            SqlCommand cmd = new SqlCommand()
                            {
                                CommandType = CommandType.Text,
                                Connection = conn,
                                Transaction = trans
                            };

                            string Ser_pro_code = ((Label)gd_Service_Product.Rows[i].FindControl("Ser_pro_code")).Text;
                            string Ser_pro_Name = ((Label)gd_Service_Product.Rows[i].FindControl("Ser_pro_Name")).Text;
                            string Vendor_rate = ((TextBox)gd_Service_Product.Rows[i].FindControl("Vendor_rate")).Text;
                            string tax_app = ((RadioButtonList)gd_Service_Product.Rows[i].FindControl("RadioButtonList1")).Text;
                            string vat_parsentage = ((DropDownList)gd_Service_Product.Rows[i].FindControl("vat_parsentage")).Text;
                            string Quantity = ((TextBox)gd_Service_Product.Rows[i].FindControl("Quantity")).Text;
                            string sepecification = ((TextBox)gd_Service_Product.Rows[i].FindControl("sepecification")).Text;

                            double qty = 0;
                            double vndr = 0; 
                            // Convert quantity to numeric safely
                            bool isQuantityValid = !string.IsNullOrEmpty(Quantity) && double.TryParse(Quantity, out qty) && qty > 0;
                            bool isVendorRateValid = !string.IsNullOrEmpty(Vendor_rate) && double.TryParse(Vendor_rate, out vndr);
                            bool isTaxAppValid = !string.IsNullOrEmpty(tax_app);
                            bool isVatValid = !string.IsNullOrEmpty(vat_parsentage) && vat_parsentage != "NA";

                            // Check if any field is filled
                            bool isAnyFieldFilled = isQuantityValid || isVendorRateValid || isTaxAppValid || isVatValid;

                            // Check if all required fields are present when any field is filled
                            if (isAnyFieldFilled)
                            {
                                List<string> missingFields = new List<string>();

                                if (!isQuantityValid) missingFields.Add("Quantity");
                                if (!isVendorRateValid) missingFields.Add("Vendor Rate");
                                if (!isTaxAppValid) missingFields.Add("Tax Applicable");

                                // VAT check only if Tax is applicable
                                if (tax_app == "Yes" && !isVatValid)
                                {
                                    missingFields.Add("VAT Percentage");
                                }

                                if (missingFields.Count > 0)
                                {
                                    errorMessages.Add($"Row {i + 1}: {string.Join(", ", missingFields)} is required.");
                                    continue;
                                }

                                // ✅ Proceed with processing this row as all required fields are valid
                                // Your existing insert & stock update logic here...
                            }

                            sl++;
                            double parches_rate = Convert.ToDouble(Vendor_rate) * Convert.ToDouble(Quantity);
                            double tax_rete = (tax_app == "Yes") ? (parches_rate * Convert.ToDouble(vat_parsentage)) / 100 : 0;
                            double total_purches_rate = tax_rete + parches_rate;
                            tota_purchesrate1 += total_purches_rate;
                            total_tax_rate_details += tax_rete;

                            cmd.CommandText = "insert into tbl_purches_details(sl_no,Purches_id,Product_id,Product_name,vendor_rate,tax_applicable,tax_rate,Quantity,purches_rate,total_purches_rate,vat_amount,specification,Purches_date,Client_id)" +
                                              "values(@sl_no, @Purches_id, @Product_id, @Product_name, @vendor_rate, @tax_applicable, @tax_rate, @Quantity, @purches_rate, @total_purches_rate, @vat_amount, @specification, @Purches_date, @Client_id)";
                            cmd.Parameters.AddWithValue("@sl_no", sl.ToString());
                            cmd.Parameters.AddWithValue("@Purches_id", purchesid);
                            cmd.Parameters.AddWithValue("@Product_id", Ser_pro_code);
                            cmd.Parameters.AddWithValue("@Product_name", Ser_pro_Name);
                            cmd.Parameters.AddWithValue("@vendor_rate", Vendor_rate);
                            cmd.Parameters.AddWithValue("@tax_applicable", tax_app);
                            cmd.Parameters.AddWithValue("@tax_rate", vat_parsentage);
                            cmd.Parameters.AddWithValue("@Quantity", Quantity);
                            cmd.Parameters.AddWithValue("@purches_rate", parches_rate);
                            cmd.Parameters.AddWithValue("@total_purches_rate", total_purches_rate);
                            cmd.Parameters.AddWithValue("@vat_amount", tax_rete);
                            cmd.Parameters.AddWithValue("@specification", sepecification.ToString());
                            cmd.Parameters.AddWithValue("@Purches_date", txtPurchesDate.Text);
                            cmd.Parameters.AddWithValue("@Client_id", lblvendor_id.Text);
                            cmd.ExecuteNonQuery();

                            updatestock(Ser_pro_code, Ser_pro_Name, Quantity, Vendor_rate, vat_parsentage);
                        }

                        //// Show all error messages at once if any row had missing values
                        //if (errorMessages.Count > 0)
                        //{
                        //    ShowErrorMessage(string.Join("<br>", errorMessages)); // "<br>" works for web-based UI
                        //    return; // Stop further execution
                        //}

                        decimal invAmount = 0.0m, tcsAmount = 0.00m, deliveryAmount = 0.00m, otherAmount1 = 0.00m, otherAmount2 = 0.00m;
                        decimal.TryParse(txt_tcs_amnt.Text.Trim(), out tcsAmount);
                        decimal.TryParse(txt_delivery_amnt.Text.Trim(), out deliveryAmount);
                        decimal.TryParse(txt_othr_amnt1.Text.Trim(), out otherAmount1);
                        decimal.TryParse(txt_othr_amnt2.Text.Trim(), out otherAmount2);
                        decimal totalAmount = invAmount + tcsAmount + deliveryAmount + otherAmount1 + otherAmount2;
                        string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";

                        SqlCommand cmdMain = new SqlCommand("INSERT INTO tbl_Purches (Purches_Id, Client_Id, Total_purches_rate, Total_Tax_rate, Purches_date, Purches_Type, Invoice_No, Stock_Add_Date, Narration, InvoiceAmnt, TCS_Amount, TCS_Rate, Delivery_Amount, Delivery_Rate, otherAmount1_name, otherAmount1, otherAmount2_name, otherAmount2, AddedById, CreatedDate, TimeStamp) " +
                            "VALUES (@Purches_Id, @Client_Id, @Total_purches_rate, @Total_Tax_rate, @Purches_date, @Purches_Type, @Invoice_No, @Stock_Add_Date, @Narration, @InvoiceAmnt, @TCS_Amount, @TCS_Rate, @Delivery_Amount, @Delivery_Rate, @otherAmount1_name, @otherAmount1, @otherAmount2_name, @otherAmount2, @AddedById, @CreatedDate, @TimeStamp);", conn, trans);

                        cmdMain.Parameters.AddWithValue("@Purches_Id", purchesid);
                        cmdMain.Parameters.AddWithValue("@Client_Id", lblvendor_id.Text);
                        cmdMain.Parameters.AddWithValue("@Total_purches_rate", tota_purchesrate1);
                        cmdMain.Parameters.AddWithValue("@Total_Tax_rate", total_tax_rate_details);
                        cmdMain.Parameters.AddWithValue("@Purches_date", txtPurchesDate.Text);
                        cmdMain.Parameters.AddWithValue("@Purches_Type", RadioButtonList1.SelectedValue);
                        cmdMain.Parameters.AddWithValue("@Invoice_No", txt_invno.Text);
                        cmdMain.Parameters.AddWithValue("@Stock_Add_Date", txt_stockadddate.Text);
                        cmdMain.Parameters.AddWithValue("@Narration", txt_narration.Text);
                        cmdMain.Parameters.AddWithValue("@InvoiceAmnt", invAmount);
                        cmdMain.Parameters.AddWithValue("@TCS_Amount", tcsAmount);
                        //cmdMain.Parameters.AddWithValue("@TCS_Rate", DDL_tcspercent.SelectedValue); // Assuming you have a field for TCS Rate 
                        cmdMain.Parameters.AddWithValue("@TCS_Rate", txt_tcs_percent.Text);
                        cmdMain.Parameters.AddWithValue("@Delivery_Amount", deliveryAmount);
                        cmdMain.Parameters.AddWithValue("@Delivery_Rate", DDL_vat_parsentage.SelectedValue);
                        cmdMain.Parameters.AddWithValue("@otherAmount1_name", TextBox1.Text); // Other Charge-1 Name
                        cmdMain.Parameters.AddWithValue("@otherAmount1", otherAmount1);
                        cmdMain.Parameters.AddWithValue("@otherAmount2_name", TextBox2.Text); // Other Charge-2 Name
                        cmdMain.Parameters.AddWithValue("@otherAmount2", otherAmount2);
                        cmdMain.Parameters.AddWithValue("@AddedById", userId);
                        cmdMain.Parameters.AddWithValue("@CreatedDate", DateTime.Now.Date); // Setting CreatedDate to current date
                        cmdMain.Parameters.AddWithValue("@TimeStamp", DateTime.Now); // Setting Timestamp to current date & time

                        cmdMain.ExecuteNonQuery();

                        SqlCommand cmdDue = new SqlCommand("INSERT INTO tbl_purches_due (Purches_Id, Due_amount) VALUES (@Purches_Id, @Due_amount);", conn, trans);
                        cmdDue.Parameters.AddWithValue("@Purches_Id", purchesid);
                        cmdDue.Parameters.AddWithValue("@Due_amount", tota_purchesrate1);
                        cmdDue.ExecuteNonQuery();

                        trans.Commit();
                        lblOk.Text = "Data Saved Successfully.....";
                        PanelOK.Visible = true;
                        PanelError.Visible = false;

                        gridtable.Visible = false;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        ShowErrorMessage(ex.Message);
                    }
                }
            }
        }


        private void ShowErrorMessage(string message)
        {
            PanelError.Visible = true;
            lblErrorMsg.Text = message;
        }

        private void updatestock(string Ser_pro_code, string Ser_pro_Name, string Quantity1, string Sale_rate, string service_Tax_Rate)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Product_id from tbl_stock where  Product_id='" + Ser_pro_code + "'";
            SqlCommand cmd10 = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd10.ExecuteReader();
            if (re.Read())
            {
                DbCL.executeRdr("update tbl_stock set Quantity=(cast(Quantity as int)+'" + Quantity1.ToString() + "'),Sail_Rate='" + Sale_rate + "',Service_tax_rate='" + service_Tax_Rate + "' where Product_id='" + Ser_pro_code + "' and Product_name='" + Ser_pro_Name + "'");

                DbCL.executeRdr("UPDATE tbl_NewProduct SET Quantity=(CAST(Quantity AS INT) + '" + Quantity1.ToString() + "'), Sail_Rate='" + Sale_rate + "', Tax_Rate='" + service_Tax_Rate + "' WHERE ProductID='" + Ser_pro_code + "' AND ProductName='" + Ser_pro_Name + "'");
            }
            else
            {
                DbCL.executeRdr("insert into tbl_stock(Product_id,Product_name,Quantity,Sail_Rate,Service_tax_rate)values('" + Ser_pro_code + "','" + Ser_pro_Name + "','" + Quantity1 + "','" + Sale_rate + "','" + service_Tax_Rate + "')");

                DbCL.executeRdr("INSERT INTO tbl_NewProduct (ProductID, ProductName, Quantity, Sail_Rate, Tax_Rate) VALUES ('" + Ser_pro_code + "', '" + Ser_pro_Name + "', '" + Quantity1 + "', '" + Sale_rate + "', '" + service_Tax_Rate + "')");
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