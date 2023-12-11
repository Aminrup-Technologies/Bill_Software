using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm71 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dt = new DataTable();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                DbCL.FillCombo(cmbClient, "select Client_Name from tbl_Client order by Client_Name");
                txtquotationDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            
            if (ViewState["CurrentData"] != null)
            {
                dt = (DataTable)ViewState["CurrentData"];
                int count = dt.Rows.Count + 1;
                BindGrid(count);
            }
            else
            {
                //dt.Clear();
                //dt.Dispose();
                BindGrid(1);
            }

            txtDescription.Text = "";
            txtSize.Text = "";
            txtQnty.Text = "";
            txtRate.Text = "";
        }

        private void BindGrid(int count)
        {
            //dt.Clear();
            DataRow dr;
            if (count == 1)
            {
                dt.Columns.Add(new System.Data.DataColumn("Description", typeof(String)));
                dt.Columns.Add(new System.Data.DataColumn("Size", typeof(String)));
                dt.Columns.Add(new System.Data.DataColumn("Qnty", typeof(int)));
                dt.Columns.Add(new System.Data.DataColumn("Rate", typeof(float)));
                dt.Columns.Add(new System.Data.DataColumn("Amount", typeof(float)));
            }

            if (ViewState["CurrentData"] != null)
            {
                for (int i = 0; i < dt.Rows.Count + 1; i++)
                {
                    dt = (DataTable)ViewState["CurrentData"];
                    if (dt.Rows.Count > 0)
                    {
                        dr = dt.NewRow();
                        dr[0] = dt.Rows[0][0].ToString();
                    }
                }
                dr = dt.NewRow();

                string Description = txtDescription.Text;
                string Size = txtSize.Text;
                int Qnty =Convert.ToInt32(txtQnty.Text);
                double rate = Convert.ToDouble(txtRate.Text);
                double amount =Math.Round(Qnty * rate);

                dr[0] = Description;
                dr[1] = Size;
                dr[2] = Qnty;
                dr[3] = rate;
                dr[4] = amount;

                dt.Rows.Add(dr);

            }
            else
            {
                dr = dt.NewRow();

                string Description = txtDescription.Text;
                string Size = txtSize.Text;
                int Qnty = Convert.ToInt32(txtQnty.Text);
                double rate = Convert.ToDouble(txtRate.Text);
                double amount = Math.Round(Qnty * rate);

                dr[0] = Description;
                dr[1] = Size;
                dr[2] = Qnty;
                dr[3] = rate;
                dr[4] = amount;

                dt.Rows.Add(dr);

            }

            // If ViewState has a data then use the value as the DataSource
            if (ViewState["CurrentData"] != null)
            {
                repredataGrid.DataSource = (DataTable)ViewState["CurrentData"];
                repredataGrid.DataBind();
            }
            else
            {
                // Bind GridView with the initial data assocaited in the DataTable
                repredataGrid.DataSource = dt;
                repredataGrid.DataBind();

            }
            // Store the DataTable in ViewState to retain the values
            ViewState["CurrentData"] = dt;
        }

        protected void repredataGrid_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            repredataGrid.PageIndex = e.NewPageIndex;
            repredataGrid.DataBind();

            if (ViewState["CurrentData"] != null)
            {
                dt = (DataTable)ViewState["CurrentData"];
                repredataGrid.DataSource = dt;
                repredataGrid.DataBind();
            }
        }

        protected void repredataGrid_RowCommand(object sender, GridViewCommandEventArgs e)
        {

        }

        protected void repredataGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Cells[0].HorizontalAlign = HorizontalAlign.Center;
                e.Row.Cells[1].HorizontalAlign = HorizontalAlign.Center;
                e.Row.Cells[2].HorizontalAlign = HorizontalAlign.Center;
                e.Row.Cells[3].HorizontalAlign = HorizontalAlign.Center;
            }
        }

        protected void repredataGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            if (ViewState["CurrentData"] != null)
            {
                dt = (DataTable)ViewState["CurrentData"];
                dt.Rows[e.RowIndex].Delete();
                repredataGrid.DataSource = dt;
                repredataGrid.DataBind();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string reqno = BindRequino();
            insertMainTable(reqno);
            foreach (GridViewRow gvadd in repredataGrid.Rows)
            {
                //CheckBox chk = (CheckBox)gvadd.Cells[0].FindControl("chkAllRow");
                //if (chk.Checked)
                //{
                    string Description = "";
                    string Size = "";
                    int Qnty = 0;
                    double Rate = 0;
                    double amount =0;
                    Description = gvadd.Cells[1].Text.ToString();
                    Size = gvadd.Cells[2].Text.ToString();
                    Qnty =Convert.ToInt32(gvadd.Cells[3].Text);
                    Rate =Convert.ToDouble(gvadd.Cells[4].Text);
                    amount = Convert.ToDouble(gvadd.Cells[5].Text);

                    string query = "insert into tbl_RequisitionNew (Clientname,Description,Size,Qnty,Rate,amount,date,gstrate,ReqNo) values (@Clientname,@Description,@Size,@Qnty,@Rate,@amount,@date,@gstrate,@ReqNo)";
                    SqlParameter[] pram = {
                        new SqlParameter("@Clientname",cmbClient.Text),
                        new SqlParameter("@Description",Description),
                        new SqlParameter("@Size",Size),
                        new SqlParameter("@Qnty",Qnty),
                        new SqlParameter("@Rate",Rate),
                        new SqlParameter("@amount",amount),
                        new SqlParameter("@date",txtquotationDate.Text),
                        new SqlParameter("@gstrate",cmdGst.Text),
                        new SqlParameter("@ReqNo",reqno),
                    };

                    int row = DbCL.SPExecDB(query, pram);
                    if (row > 0)
                    {
                        PanelOK.Visible = true;
                        lblOk.Text = "Data Save Successfully...";
                    }
                    else
                    {
                        PanelOK.Visible = false;
                    }
                //}
            }
        }

        
        private string BindRequino()
        {
            string p = null;
            string c = cmbClient.Text.Trim();
            string f = c.Substring(0, 1);
            string tt;
            for (int i = 0; i < c.Length; i++)
            {
                p = c.Substring(i, 1);
                if (p == " ")
                {
                    tt = c.Substring((i + 1), 1);
                    if (tt == "(")
                    {
                        tt = c.Substring((i + 2), 1);
                    }
                    f = f + tt;
                }
            }
            f = "REQ/" + f + "/";
            int j = idreturn();
            j = j + 1;
            f = f + j.ToString();
            return f;
        }

        private int idreturn()
        {
            string a = null;
            int b = 0;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string query = "select id from tbl_RequisitionMain where id=(select Max(id) from tbl_RequisitionMain)";

            SqlCommand cmd = new SqlCommand(query, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                a = re["id"].ToString();
                b = Convert.ToInt32(a);
            }
            else
            {
                b = 0;
            }
            DbCL.Conn.Close();
            return b;
        }

        private void insertMainTable(string reqno)
        {

            string query = "insert into tbl_RequisitionMain (clientName,CheckNo,IssueDate,BankName,IFSCode,GstRate,Date,Address,ReqNo,Vendor) values (@clientName,@CheckNo,@IssueDate,@BankName,@IFSCode,@GstRate,@Date,@Address,@ReqNo,@Vendor)";
            SqlParameter[] pram = {
                        new SqlParameter("@clientName",cmbClient.Text),
                        new SqlParameter("@CheckNo",txtCheckno.Text),
                        new SqlParameter("@IssueDate",txtIssueDate.Text),
                        new SqlParameter("@BankName",txtBankName.Text),
                        new SqlParameter("@IFSCode",txtIFSCode.Text),
                        new SqlParameter("@GstRate",Convert.ToDouble(cmdGst.Text)),
                        new SqlParameter("@Date",txtquotationDate.Text),
                        new SqlParameter("@Address",""),
                        new SqlParameter("@ReqNo",reqno.ToString()),
                        new SqlParameter("@Vendor",txtVendor.Text.ToString())
                    };
            int row = DbCL.SPExecDB(query, pram);
            if (row > 0)
            {
                PanelOK.Visible = true;
                lblOk.Text = "Data Save Successfully...";
            }
            else
            {
                PanelOK.Visible = false;
            }
        }
    }
}