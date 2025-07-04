using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Bill_Software.corporate.business.app
{
    public partial class srch_dailyrpts : System.Web.UI.Page
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
                DbCL.FillCombo(cmbvendor, "select Name from tbl_login order by Name");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");

            }
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            Binder();
        }

        private void Binder()
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                cmdstring = "select * from tbl_SalesVisitReport where CreatedByCode='" + lblclientId.Text + "' order by VisitDate , TimeStamp desc";

                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                cmdstring = "select * from tbl_SalesVisitReport where VisitDate between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by VisitDate , TimeStamp desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                cmdstring = "select * from tbl_SalesVisitReport where CreatedByCode='" + lblclientId.Text + "' and VisitDate between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by VisitDate , TimeStamp desc";
                Buinddatagrid(cmdstring);
            }
            btnSertch.Visible = false;
        }

        private void Buinddatagrid_old(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                Buinddatagrid1(cmdstring);
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "No Data Found...";

            }
            DbCL.Conn.Close();
        }

        private void Buinddatagrid(string cmdstring)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmdstring, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        DataList2.DataSource = dt;
                        DataList2.DataBind();
                    }
                    else
                    {
                        DataList2.DataSource = null;
                        DataList2.DataBind();
                        lblOk.Text = "No records found for the selected criteria.";
                        PanelError.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error loading data: " + ex.Message;
                PanelError.Visible = true;
            }
        }


        private void Buinddatagrid1(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            SqlCommand cmd1 = new SqlCommand(cmdstring, DbCL.Conn);
            DataList2.DataSource = cmd1.ExecuteReader();
            DataList2.DataBind();
            DbCL.Conn.Close();

        }

        private void BuindCompanyId()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select User_Id from tbl_login where Name='" + cmbvendor.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclientId.Text = re["User_Id"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/srch_dailyrpts.aspx");
        }

        //protected void DataList2_ItemCommand(object source, DataListCommandEventArgs e)
        //{
        //    string Id = e.CommandArgument.ToString();
        //    string remarks = ((TextBox)e.Item.FindControl("txtManagerRemarks"))?.Text ?? "";
        //    string status = e.CommandName == "Approve" ? "Approved" : "Rejected";

        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
        //        {
        //            conn.Open();
        //            string query = "UPDATE tbl_SalesVisitReport SET ApprovalStatus = @Status, ManagerRemarks = @Remarks, ApprovedDate = GETDATE(), ApprovedBy = @User WHERE Id = @Id";

        //            SqlCommand cmd = new SqlCommand(query, conn);
        //            cmd.Parameters.AddWithValue("@Status", status);
        //            cmd.Parameters.AddWithValue("@Remarks", remarks);
        //            cmd.Parameters.AddWithValue("@User", Session["UserName"] ?? "Manager");
        //            cmd.Parameters.AddWithValue("@Id", Id);

        //            cmd.ExecuteNonQuery();
        //            lblOk.Text = $"Visit ID {Id} marked as {status}.";
        //            PanelOK.Visible = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        lblErrorMsg.Text = "Error: " + ex.Message;
        //        PanelError.Visible = true;
        //    }

        //}

        protected void DataList2_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string id = e.CommandArgument.ToString();
            string remarks = ((TextBox)e.Item.FindControl("txtManagerRemarks"))?.Text.Trim() ?? "";
            string status = e.CommandName == "Approve" ? "Approved" : "Rejected";
            string user = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";

            PanelOK.Visible = false;
            PanelError.Visible = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    conn.Open();

                    string query = @"
                UPDATE tbl_SalesVisitReport SET 
                    ApprovalStatus = @Status,
                    ManagerRemarks = @Remarks,
                    ApprovedDate = GETDATE(),
                    ApprovedBy = @User WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@Remarks", remarks);
                        cmd.Parameters.AddWithValue("@User", user);
                        cmd.Parameters.AddWithValue("@Id", id);

                        cmd.ExecuteNonQuery();
                    }

                    lblOk.Text = $"Visit ID {id} successfully marked as <b>{status}</b>.";
                    PanelOK.Visible = true;
                    Binder();
                }
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error occurred: " + ex.Message;
                PanelError.Visible = true;
            }
        }

        protected void DataList2_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var lblStatus = (Label)e.Item.FindControl("lblApprovalStatus");
                var btnApprove = (Button)e.Item.FindControl("btnApprove");
                var btnReject = (Button)e.Item.FindControl("btnReject");

                if (lblStatus != null && lblStatus.Text != "Pending")
                {
                    if (btnApprove != null) btnApprove.Enabled = false;
                    if (btnReject != null) btnReject.Enabled = false;
                }
            }
        }
    }
}