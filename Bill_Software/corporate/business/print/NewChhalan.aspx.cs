using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Globalization;
using System.Threading;

namespace Bill_Software.corporate.business.print
{
   
    public partial class NewChhalan : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public string vatno = "";
        public string gstno = "";
        DataTable dtChPro = new DataTable();
        DataTable dtRep = new DataTable();
        DataTable dtChadd = new DataTable();
        StringBuilder strProduct = new StringBuilder();
        public int TQ = 0;

        CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
        protected void Page_Load(object sender, EventArgs e)
        {
            string Chalan_No = Request.QueryString["Chalan_No"];
            lblChano.Text = Chalan_No.ToString();

            buindalldata();
            Buindamount(Chalan_No);
            deliveryAddress(Chalan_No);
        }

        private void deliveryAddress(string chalan_No)
        {
            string query = "select SiteAddress from tbl_ChaSiteAddress where Cha_no=@Cha_no order by id";
            SqlParameter[] pram = {
                new SqlParameter("@Cha_no",chalan_No)
            };
            dtChadd = DbCL.SPreturn_dt(query, pram);
            if (dtChadd.Rows.Count > 0)
            {
                string SiteAddress = "";
                for (int i = 0; i < dtChadd.Rows.Count; i++)
                {
                    string add = dtChadd.Rows[i]["SiteAddress"].ToString();
                    SiteAddress += add + "<br>";
                }
                lblAddress.Text = SiteAddress.ToString();
            }
        }

        private void buindalldata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "SELECT c.Chalan_No, c.Chalan_Date, c.Quotation_No, c.Quotation_Date, c.Client_ID, c.addressfor, q.DO_Number, q.PO_Number, q.PO_Date FROM tbl_Chalan c, tbl_Quotation q where c.Quotation_No=q.Quotation_no and c.Chalan_No='" + lblChano.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblChadate.Text = re["Chalan_Date"].ToString();
                //lblqnumber.Text = re["Quotation_No"].ToString();
                string qno = re["Quotation_No"].ToString();
                lblpnumber.Text = re["PO_Number"].ToString();
                lbldonumber.Text = re["DO_Number"].ToString();
                lblpdate.Text = re["PO_Date"].ToString();

                //string qno = lblqnumber.Text;
                bindplaceodsupply(qno);
                //lblQdate.Text = re["Quotation_Date"].ToString();
                string clientid = re["Client_ID"].ToString();
                //lblClientCode.Text = clientid;
                representative(clientid);
                string addressfor = re["addressfor"].ToString();
                Bindclientdetails(clientid);
            }
        }

        private void bindplaceodsupply(string qno)
        {
            string query = "select PlaceofSupply from tbl_Quotation where Quotation_no=@Quotation_no";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_no",qno)
            };
            SqlDataReader rdr = DbCL.SPReturnRdr(query, pram);
            if (rdr.Read())
            {
                string placeofsupply = rdr["PlaceofSupply"].ToString();
                lblplaceofsup1.Text = "Place Of Supply";
                lblplaceofsup2.Text = ":";
                lblplaceofsup3.Text = placeofsupply;
            }
        }

        private void representative(string clientid)
        {
            string query = "select Representative_name,Designation,Phone_no,Email,RepTitle,RepLastName from tbl_representative where Copany_Id=@Copany_Id";
            SqlParameter[] pram = {
                new SqlParameter("@Copany_Id",clientid)
            };
            dtRep = DbCL.SPreturn_dt(query, pram);
            if (dtRep.Rows.Count > 0)
            {
                string RepTitle = dtRep.Rows[0]["RepTitle"].ToString();
                string repname = dtRep.Rows[0]["Representative_name"].ToString();
                string RepLastName = dtRep.Rows[0]["RepLastName"].ToString();
                string Designation = dtRep.Rows[0]["Designation"].ToString();

                //lblrename.Text = RepTitle +" " + repname+" " + RepLastName;
                //lbldeg.Text = Designation;
            }
        }

        private void Buindamount(string Chalan_No)
        {
            //string query = "select Sl_no,Challan_no,Product_id,Product_code,Product_name,Quantity from tbl_Challan_details where Challan_no=@Challan_no order by Product_name";

            string query = "SELECT d.Sl_no, q.Product_id as Product_code, q.Product_name, d.Quantity, q.Product_code as Product_id, q.specification, q.ItemNo, q.MaterialNo, q.PackSize, q.Unit, q.Department FROM tbl_Chalan c INNER JOIN tbl_Challan_details d ON c.Chalan_No = d.Challan_no INNER JOIN tbl_Quotaion_details q ON c.Quotation_No = q.Quotation_no AND d.Product_id =q.Product_Code and d.ItemNo = q.ItemNo where Challan_no=@Challan_no and IsDeleted=0 and IsLatest=1 order by CAST(d.Sl_no as int)";
            SqlParameter[] pram = {
                new SqlParameter("@Challan_no",Chalan_No)
            };
            dtChPro = DbCL.SPreturn_dt(query, pram);

            //if (dtChPro.Rows.Count>0)
            //{
            //    strProduct.Append("<table class='' style='border:0' width='100%'>");
            //    strProduct.Append("<tr>");
            //    strProduct.Append("<td style='width:7%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>S.NO</td>");
            //    strProduct.Append("<td style='width:63%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>PARTICULARS</td>");
            //    strProduct.Append("<td style='width:15%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>HSN CODE</td>");
            //    strProduct.Append("<td style='width:15%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>QUANTITY<br>(PCS)</td>");
            //    strProduct.Append("</tr>");
            //    strProduct.Append("</table>");

            //    for (int i = 0; i < dtChPro.Rows.Count; i++)
            //    {
            //        string Sl_no = dtChPro.Rows[i]["Sl_no"].ToString();
            //        string Challan_no = dtChPro.Rows[i]["Challan_no"].ToString();
            //        string Product_id = dtChPro.Rows[i]["Product_id"].ToString();
            //        string HSN = dtChPro.Rows[i]["Product_code"].ToString();
            //        string Product_name = dtChPro.Rows[i]["Product_name"].ToString();
            //        int Quantity =Convert.ToInt32(dtChPro.Rows[i]["Quantity"]);
            //        string hsncode= dtChPro.Rows[i]["Product_id"].ToString(); ;
            //        TQ = TQ + Quantity;

            //        strProduct.Append("<table class='' style='border:0' width='100%'>");
            //        strProduct.Append("<tr>");
            //        strProduct.Append("<td style='width:7%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;  border-right:none;'>" + Sl_no + "</td>");
            //        strProduct.Append("<td style='width:63%; border: 1px solid #bfbfbf; font-weight: bold; text-align: left;   border-right:none;'>" + Product_name + "</td>");
            //        strProduct.Append("<td style='width:15%; border: 1px solid #bfbfbf; font-weight: bold; text-align: center;   border-right:none;'>" + HSN + "</td>");
            //        strProduct.Append("<td style='width:15%; border: 1px solid #bfbfbf; font-weight: bold; text-align: center;'>" + Quantity + "</td>");
            //        strProduct.Append("</tr>");
            //        strProduct.Append("</table>");
            //    }

            //    strProduct.Append("<table class='' style='border:0' width='100%'>");
            //    strProduct.Append("<tr>");
            //    strProduct.Append("<td style='width:70%; border:none; font-weight: bold;  text-align: center;' colspan='2'></td>");
            //    strProduct.Append("<td style='width:15%; border: 1px solid #bfbfbf; background-color: #e31e24; color: white; border-right:none; font-weight: bold; text-align: center;'>Total Quantity</td>");
            //    strProduct.Append("<td style='width:15%; border: 1px solid #bfbfbf; background-color: #e31e24; color: white; font-weight: bold; text-align: center;'>" + TQ.ToString() + "</td>");
            //    strProduct.Append("</tr>");
            //    strProduct.Append("</table>");

            //    lblProductDetails.Text = strProduct.ToString();
            //}

            if (dtChPro.Rows.Count > 0)
            {
                strProduct.Append("<table class='' style='border:0' width='100%'>");
                strProduct.Append("<tr>");
                strProduct.Append("<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>S.NO</td>");
                strProduct.Append("<td style='width:15%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>PRODUCT ID</td>");
                strProduct.Append("<td style='width:45%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>PARTICULARS</td>");               
                strProduct.Append("<td style='width:10%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>HSN CODE</td>");
                strProduct.Append("<td style='width:10%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>QUANTITY<br>(PCS)</td>");
                strProduct.Append("<td style='width:10%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>UNIT</td>");
                strProduct.Append("</tr>");

                for (int i = 0; i < dtChPro.Rows.Count; i++)
                {
                    string Sl_no = dtChPro.Rows[i]["Sl_no"].ToString();
                    string Product_id = dtChPro.Rows[i]["Product_id"].ToString();
                    string HSN = dtChPro.Rows[i]["Product_code"].ToString();
                    string Product_name = dtChPro.Rows[i]["Product_name"].ToString();
                    int Quantity = Convert.ToInt32(dtChPro.Rows[i]["Quantity"]);
                    string Unit = dtChPro.Rows[i]["Unit"].ToString();
                    string Specification = dtChPro.Rows[i]["specification"].ToString();
                    string ItemNo = dtChPro.Rows[i]["ItemNo"].ToString();
                    string MaterialNo = dtChPro.Rows[i]["MaterialNo"].ToString();
                    string PackSize = dtChPro.Rows[i]["PackSize"].ToString();
                    string Dept = dtChPro.Rows[i]["Department"].ToString();

                    TQ += Quantity;
                    string particulars = $"<b>{Product_name}</b><br/>"
                                       + $"<span style='font-size: 11px;'>Spec: {Specification}</span><br/>"
                                       + $"<span style='font-size: 11px;'>Item No: {ItemNo}, Material No: {MaterialNo}, Pack: {PackSize}</span><br/>"
                                       + $"<span style='font-size: 11px;'>Department: {Dept}</span>";

                    strProduct.Append("<tr>");
                    strProduct.Append("<td style='border: 1px solid #bfbfbf; text-align: center;'>" + Sl_no + "</td>");
                    strProduct.Append("<td style='border: 1px solid #bfbfbf; text-align: center;'>" + Product_id + "</td>");
                    strProduct.Append("<td style='border: 1px solid #bfbfbf; text-align: left;'>" + particulars + "</td>");               
                    strProduct.Append("<td style='border: 1px solid #bfbfbf; text-align: center;'>" + HSN + "</td>");
                    strProduct.Append("<td style='border: 1px solid #bfbfbf; text-align: center;'>" + Quantity + "</td>");
                    strProduct.Append("<td style='border: 1px solid #bfbfbf; text-align: center;'>" + Unit + "</td>");
                    strProduct.Append("</tr>");
                }

                strProduct.Append("<tr>");
                strProduct.Append("<td colspan='3' style='border:none'></td>");
                strProduct.Append("<td style='border: 1px solid #bfbfbf; background-color: #e31e24; color: white; font-weight: bold; text-align: center;'>Total</td>");
                strProduct.Append("<td style='border: 1px solid #bfbfbf; background-color: #e31e24; color: white; font-weight: bold; text-align: center;'>" + TQ + "</td>");
                strProduct.Append("<td style='border:none'></td>");
                strProduct.Append("</tr>");
                strProduct.Append("</table>");

                lblProductDetails.Text = strProduct.ToString();
            }

        }

        private void Bindclientdetails(string clientid)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Name,Address1,Address2,City,pin,State,Vat_no,Service_tax_no,Pan_no,PlaceofSupply from tbl_Client where Client_Id='" + clientid + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                clientName.Text = re["Client_Name"].ToString();

                string Address1 = re["Address1"].ToString();
                string Address2 = re["Address2"].ToString();
                string add = "";
                if (Address1 == Address2)
                {
                    add = Address1;
                }
                else
                {
                    add = Address1 + " " + Address2;
                }

                txtaddres.Text = add.ToString();
                
                lblcity.Text = re["City"].ToString();
                lblpincode.Text = re["pin"].ToString();
                

                gstno = re["Service_tax_no"].ToString();
                vatno = re["Vat_no"].ToString();
                lblClientPan.Text = re["Pan_no"].ToString();
                lblGstno.Text = gstno;

                //string placeofsupply = re["PlaceofSupply"].ToString();
                //lblplaceofsup1.Text = "Place Of Supply";
                //lblplaceofsup2.Text = ":";
                //lblplaceofsup3.Text = placeofsupply;
            }
            DbCL.Conn.Close();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {

        }

        protected void Button2_Click(object sender, EventArgs e)
        {

        }
    }
}