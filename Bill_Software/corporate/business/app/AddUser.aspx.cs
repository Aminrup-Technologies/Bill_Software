using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm79 : System.Web.UI.Page
    {
        private string ConnString => ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Standard Single Session Check
            if (Session["USERID"] == null || Session["SessionToken"] == null)
            {
                Response.Redirect("~/index.aspx", false);
                return;
            }

            if (!IsPostBack)
            {
                Binddata();
            }
        }

        private void Binddata()
        {
            using (var cn = new SqlConnection(ConnString))
            {
                // Notice we no longer select 'Password' as it's not shown in the grid anymore
                string cmdstring = "SELECT id, User_Id, Name, Phone_no, Email FROM tbl_login WHERE User_Id NOT IN ('admin', 'AT01') AND IsActive = 1 ORDER BY id DESC;";
                using (var cmd = new SqlCommand(cmdstring, cn))
                {
                    cn.Open();
                    DataList1.DataSource = cmd.ExecuteReader();
                    DataList1.DataBind();
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string idvalue = getidvalue();
                idvalue = "FLM0" + idvalue;
                string tempPassword = txtPass.Text.Trim();

                // FIX: Declare the byte arrays first for older C# versions
                byte[] passwordHash;
                byte[] passwordSalt;

                // Then pass them to the method
                CreatePasswordHash(tempPassword, out passwordHash, out passwordSalt);

                // 2. Insert into tbl_login. 
                // CRUCIAL: MustChangePassword = 1 and EmailVerified = 0 triggers the OTP flow on first login
                string query = @"INSERT INTO tbl_login 
                        (User_Id, Name, Phone_no, Email, PasswordHash, PasswordSalt, MustChangePassword, EmailVerified, IsActive, CreatedAt) 
                         VALUES 
                        (@User_Id, @Name, @Phone_no, @Email, @PasswordHash, @PasswordSalt, 1, 0, 1, sysutcdatetime())";

                using (var cn = new SqlConnection(ConnString))
                {
                    using (var cmd = new SqlCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@User_Id", idvalue);
                        cmd.Parameters.AddWithValue("@Name", txtEmployee.Text.Trim());
                        cmd.Parameters.AddWithValue("@Phone_no", txtPhno.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        cmd.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, 256).Value = passwordHash;
                        cmd.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary, 128).Value = passwordSalt;

                        cn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                PanelOK.Visible = true;
                lblOk.Text = "User created successfully! They will be asked to verify their email on first login.";
                PanelError.Visible = false;

                // Clear fields
                txtEmployee.Text = "";
                txtPhno.Text = "";
                txtEmail.Text = "";
                txtPass.Text = "";

                Binddata();
            }
            catch (Exception ex)
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "Error saving user: " + ex.Message;
                PanelOK.Visible = false;
            }
        }

        private string getidvalue()
        {
            string idvalue = "1";
            string query = "SELECT ISNULL(MAX(id), 0) + 1 FROM tbl_login";
            try
            {
                using (var cn = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(query, cn))
                {
                    cn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        idvalue = result.ToString();
                    }
                }
            }
            catch { }
            return idvalue;
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string ID = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Inactivate")
            {
                using (var cn = new SqlConnection(ConnString))
                {
                    string query = "UPDATE tbl_login SET IsActive = 0 WHERE Id = @Id";
                    using (var cmd = new SqlCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@Id", Convert.ToInt32(ID));
                        cn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                PanelOK.Visible = true;
                lblOk.Text = "User marked as inactive successfully...";
                PanelError.Visible = false;
            }

            Binddata();
        }

        #region Security Helpers
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                passwordSalt = new byte[128 / 8]; // 16 bytes
                rng.GetBytes(passwordSalt);
            }

            // 100,000 iterations for PBKDF2
            using (var derive = new Rfc2898DeriveBytes(password, passwordSalt, 100000))
            {
                passwordHash = derive.GetBytes(256 / 8); // 32 bytes
            }
        }
        #endregion
    }
}


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.UI;
//using System.Web.UI.WebControls;
//using System.Data;
//using System.Data.SqlClient;

//namespace Bill_Software.corporate.business.app
//{
//    public partial class WebForm79 : System.Web.UI.Page
//    {
//        DB_UTILITY DbCL = new DB_UTILITY();
//        protected void Page_Load(object sender, EventArgs e)
//        {
//            if (HttpContext.Current.Session["USERID"] == null)
//            {
//                Response.Redirect("~/index.aspx");
//            }
//            if (!IsPostBack)
//            {
//                Binddata();
//            }
//        }

//        private void Binddata()
//        {
//            DbCL.Sqlconnection();
//            DbCL.ConnectDb();
//            string cmdstring = "select id,User_Id,Password,Name,Phone_no,Email from tbl_login where User_Id not in ('admin', 'AT01') AND IsActive = 1;";
//            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
//            DataList1.DataSource = cmd.ExecuteReader();
//            DataList1.DataBind();
//            DbCL.Conn.Close();
//        }

//        protected void btnSave_Click(object sender, EventArgs e)
//        {
//            string idvalue = getidvalue();
//            idvalue = "FLM0" + idvalue;

//            string query = "insert into tbl_login(User_Id,Password,Name,Phone_no,Email) values (@User_Id,@Password,@Name,@Phone_no,@Email)";
//            SqlParameter[] pram = {
//                    new SqlParameter("@User_Id",idvalue),
//                    new SqlParameter("@Password",txtPass.Text),
//                    new SqlParameter("@Name",txtEmployee.Text),
//                    new SqlParameter("@Phone_no",txtPhno.Text),
//                    new SqlParameter("@Email",txtEmail.Text),
//            };
//            string query1 = "insert into tbl_Designation(User_Id,Name,Home,home1,settings,Dashboard,Data_Mastering,master_State,master_city,AddIndustry,PaymentPhase,AddPrimaryService,PrimaryServiceTerms,productparent,product_master,newproductparent,newproduct_master,Service_master,Vat_master,Service_Tax_Master,Expenses_Head,Vendor,New_vendor,View_vendor,Delete_vendor,Purches_exting_vendor,View_purches,seartch_purtch,Delete_purtches,Purchess_payment,add_payment_purchess,View_purchess_payment,Seartch_purchess_payments,Delete_purches_payment,Client,New_client,View_client,Delete_client,Representative,AddFactory,Quotatio,Create_quotation,View_quotation,Seartch_quotation,Delete_Quotation,Edit_quatation,challan,add_chalan,View_chalan,seartch_chalan,Delete_chalan,proforma,Add_proforma,View_proforma,Seartch_proforma,Delete_proforma,Invoice,Add_invoice,View_Invoice,seartch_invoice,Delete_invoice,Block_invoice,Payment,add_payment,View_payment,seartch_payment,Delete_payment,Epencess,general_expences,patty_cash_expences,view_expencess_head,view_patty_cash_expenses,Delete_general_expencess,Delete_patty_cash_expenses,Reports,Payment_due,Purchess_due,PurchaseRequisition,RequisitionManual,RequisitionManualView,RequisitionManualSearch,RequisitionManualDelete,Users,AddUser,ViewUser,SetQuatation,ProformaMail,InvoiceMail,PaymentMail,FinalPaymentInvoice,PaymentsDue) values (@User_Id,@Name,@Home,@home1,@settings,@Dashboard,@Data_Mastering,@master_State,@master_city,@AddIndustry,@PaymentPhase,@AddPrimaryService,@PrimaryServiceTerms,@productparent,@product_master,@newproductparent,@newproduct_master,@Service_master,@Vat_master,@Service_Tax_Master,@Expenses_Head,@Vendor,@New_vendor,@View_vendor,@Delete_vendor,@Purches_exting_vendor,@View_purches,@seartch_purtch,@Delete_purtches,@Purchess_payment,@add_payment_purchess,@View_purchess_payment,@Seartch_purchess_payments,@Delete_purches_payment,@Client,@New_client,@View_client,@Delete_client,@Representative,@AddFactory,@Quotatio,@Create_quotation,@View_quotation,@Seartch_quotation,@Delete_Quotation,@Edit_quatation,@challan,@add_chalan,@View_chalan,@seartch_chalan,@Delete_chalan,@proforma,@Add_proforma,@View_proforma,@Seartch_proforma,@Delete_proforma,@Invoice,@Add_invoice,@View_Invoice,@seartch_invoice,@Delete_invoice,@Block_invoice,@Payment,@add_payment,@View_payment,@seartch_payment,@Delete_payment,@Epencess,@general_expences,@patty_cash_expences,@view_expencess_head,@view_patty_cash_expenses,@Delete_general_expencess,@Delete_patty_cash_expenses,@Reports,@Payment_due,@Purchess_due,@PurchaseRequisition,@RequisitionManual,@RequisitionManualView,@RequisitionManualSearch,@RequisitionManualDelete,@Users,@AddUser,@ViewUser,@SetQuatation,@ProformaMail,@InvoiceMail,@PaymentMail,@FinalPaymentInvoice,@PaymentsDue)";
//            SqlParameter[] pram1 = {
//                    new SqlParameter("@User_Id",idvalue),
//                    new SqlParameter("@Name",txtEmployee.Text),
//                    new SqlParameter("@Home","Yes"),
//                    new SqlParameter("@home1","Yes"),
//                    new SqlParameter("@settings","Yes"),
//                    new SqlParameter("@Dashboard","No"),
//                    new SqlParameter("@Data_Mastering","No"),
//                    new SqlParameter("@master_State","No"),
//                    new SqlParameter("@master_city","No"),
//                    new SqlParameter("@AddIndustry","No"),
//                    new SqlParameter("@PaymentPhase","No"),
//                    new SqlParameter("@AddPrimaryService","No"),
//                    new SqlParameter("@PrimaryServiceTerms","No"),
//                    new SqlParameter("@productparent","No"),
//                    new SqlParameter("@product_master","No"),
//                    new SqlParameter("@newproductparent","No"),
//                    new SqlParameter("@newproduct_master","No"),
//                    new SqlParameter("@Service_master","No"),
//                    new SqlParameter("@Vat_master","No"),
//                    new SqlParameter("@Service_Tax_Master","No"),
//                    new SqlParameter("@Expenses_Head","No"),
//                    new SqlParameter("@Vendor","No"),
//                    new SqlParameter("@New_vendor","No"),
//                    new SqlParameter("@View_vendor","No"),
//                    new SqlParameter("@Delete_vendor","No"),
//                    new SqlParameter("@Purches_exting_vendor","No"),
//                    new SqlParameter("@View_purches","No"),
//                    new SqlParameter("@seartch_purtch","No"),
//                    new SqlParameter("@Delete_purtches","No"),
//                    new SqlParameter("@Purchess_payment","No"),
//                    new SqlParameter("@add_payment_purchess","No"),
//                    new SqlParameter("@View_purchess_payment","No"),
//                    new SqlParameter("@Seartch_purchess_payments","No"),
//                    new SqlParameter("@Delete_purches_payment","No"),
//                    new SqlParameter("@Client","No"),
//                    new SqlParameter("@New_client","No"),
//                    new SqlParameter("@View_client","No"),
//                    new SqlParameter("@Delete_client","No"),
//                    new SqlParameter("@Representative","No"),
//                    new SqlParameter("@AddFactory","No"),
//                    new SqlParameter("@Quotatio","No"),
//                    new SqlParameter("@Create_quotation","No"),
//                    new SqlParameter("@View_quotation","No"),
//                    new SqlParameter("@Seartch_quotation","No"),
//                    new SqlParameter("@Delete_Quotation","No"),
//                    new SqlParameter("@Edit_quatation","No"),
//                    new SqlParameter("@challan","No"),
//                    new SqlParameter("@add_chalan","No"),
//                    new SqlParameter("@View_chalan","No"),
//                    new SqlParameter("@seartch_chalan","No"),
//                    new SqlParameter("@Delete_chalan","No"),
//                    new SqlParameter("@proforma","No"),
//                    new SqlParameter("@Add_proforma","No"),
//                    new SqlParameter("@View_proforma","No"),
//                    new SqlParameter("@Seartch_proforma","No"),
//                    new SqlParameter("@Delete_proforma","No"),
//                    new SqlParameter("@Invoice","No"),
//                    new SqlParameter("@Add_invoice","No"),
//                    new SqlParameter("@View_Invoice","No"),
//                    new SqlParameter("@seartch_invoice","No"),
//                    new SqlParameter("@Delete_invoice","No"),
//                    new SqlParameter("@Block_invoice","No"),
//                    new SqlParameter("@Payment","No"),
//                    new SqlParameter("@add_payment","No"),
//                    new SqlParameter("@View_payment","No"),
//                    new SqlParameter("@seartch_payment","No"),
//                    new SqlParameter("@Delete_payment","No"),
//                    new SqlParameter("@Epencess","No"),
//                    new SqlParameter("@general_expences","No"),
//                    new SqlParameter("@patty_cash_expences","No"),
//                    new SqlParameter("@view_expencess_head","No"),
//                    new SqlParameter("@view_patty_cash_expenses","No"),
//                    new SqlParameter("@Delete_general_expencess","No"),
//                    new SqlParameter("@Delete_patty_cash_expenses","No"),
//                    new SqlParameter("@Reports","No"),
//                    new SqlParameter("@Payment_due","No"),
//                    new SqlParameter("@Purchess_due","No"),
//                    new SqlParameter("@PurchaseRequisition","No"),
//                    new SqlParameter("@RequisitionManual","No"),
//                    new SqlParameter("@RequisitionManualView","No"),
//                    new SqlParameter("@RequisitionManualSearch","No"),
//                    new SqlParameter("@RequisitionManualDelete","No"),

//                    new SqlParameter("@Users","No"),
//                    new SqlParameter("@AddUser","No"),
//                    new SqlParameter("@ViewUser","No"),

//                    new SqlParameter("@SetQuatation","No"),
//                    new SqlParameter("@ProformaMail","No"),

//                    new SqlParameter("@InvoiceMail","No"),
//                    new SqlParameter("@PaymentMail","No"),
//                    new SqlParameter("@FinalPaymentInvoice","No"),
//                    new SqlParameter("@PaymentsDue","No"),

//            };

//            DbCL.SPExecDB(query, pram);
//            DbCL.SPExecDB(query1, pram1);

//            PanelOK.Visible = true;
//            lblOk.Text = "Data Save Successfully...";

//            Binddata();
//        }

//        private string getidvalue()
//        {
//            string idvalue = "";
//            string query = "select max(id)+1 as idvalue from tbl_login";
//            SqlDataReader rdr= DbCL.SPReturnRdr(query, null);
//            if (rdr.Read())
//            {
//                idvalue = rdr["idvalue"].ToString();
//            }
//            else
//            {
//                idvalue = "1";
//            }
//            return idvalue;
//        }

//        protected void DataList1_ItemCommand_OLD(object source, DataListCommandEventArgs e)
//        {
//            string ID = Convert.ToString(e.CommandArgument);

//            if (e.CommandName == "Delete")
//            {

//                string userid = getuser(ID);
//                DbCL.executeRdr("delete from tbl_login where Id='" + Convert.ToInt32(ID) + "'");
//                DbCL.executeRdr("delete from tbl_Designation where User_Id='" + userid + "'");
//                PanelOK.Visible = true;
//                lblOk.Text = "Data Deleted Successfully...";
//            }
//            Binddata();
//        }

//        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
//        {
//            string ID = Convert.ToString(e.CommandArgument);

//            if (e.CommandName == "Inactivate")   // NEW COMMAND NAME
//            {
//                // Mark user inactive instead of deleting
//                DbCL.executeRdr("UPDATE tbl_login SET IsActive = 0 WHERE Id = '" + Convert.ToInt32(ID) + "'");

//                PanelOK.Visible = true;
//                lblOk.Text = "User marked as inactive successfully...";
//            }

//            Binddata();
//        }

//        private string getuser(string id)
//        {
//            string userid = "";
//            string query = "select User_Id from tbl_login where Id=@id";
//            SqlParameter[] pram = { new SqlParameter("@id",Convert.ToInt32(id))};
//            SqlDataReader rdr = DbCL.SPReturnRdr(query, pram);
//            if (rdr.Read())
//            {
//                userid = rdr["User_Id"].ToString();
//            }
//            return userid;
//        }
//    }
//}