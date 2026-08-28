using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm81 : System.Web.UI.Page
    {
        private string ConnString => ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        // We use ViewState to hold the numeric User ID across postbacks
        private int NumericUserId
        {
            get { return ViewState["NumericUserId"] != null ? (int)ViewState["NumericUserId"] : 0; }
            set { ViewState["NumericUserId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
                return;
            }

            if (!IsPostBack)
            {
                string userIdString = Request.QueryString["User_Id"]; // e.g., "FLM01" or "admin"
                if (!string.IsNullOrEmpty(userIdString))
                {
                    LoadAvailableRoles();
                    BindUserData(userIdString);
                }
                else
                {
                    ShowError("No User specified.");
                    btnSave.Enabled = false;
                }
            }
        }

        private void LoadAvailableRoles()
        {
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("SELECT RoleId, RoleName FROM dbo.Roles WHERE CompanyID = @CompanyID ORDER BY RoleName", cn))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                var dt = new DataTable();
                var da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                chkRoles.DataSource = dt;
                chkRoles.DataTextField = "RoleName";
                chkRoles.DataValueField = "RoleId";
                chkRoles.DataBind();
            }
        }

        private void BindUserData(string userIdString)
        {
            lblEmpId.Text = userIdString;

            using (var cn = new SqlConnection(ConnString))
            {
                cn.Open();

                // 1. Get the numeric Id and Name from tbl_login
                using (var cmdUser = new SqlCommand("SELECT Id, Name FROM dbo.tbl_login WHERE User_Id = @UserId AND CompanyID = @CompanyID", cn))
                {
                    cmdUser.Parameters.AddWithValue("@UserId", userIdString);
                    cmdUser.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    using (var rdr = cmdUser.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            NumericUserId = rdr.GetInt32(0);
                            lblEmpName.Text = rdr["Name"].ToString();
                        }
                        else
                        {
                            ShowError("User not found.");
                            btnSave.Enabled = false;
                            return;
                        }
                    }
                }

                // 2. See which roles this user already has and check the boxes
                using (var cmdRoles = new SqlCommand("SELECT RoleId FROM dbo.UserRoles WHERE UserId = @NumericId", cn))
                {
                    cmdRoles.Parameters.AddWithValue("@NumericId", NumericUserId);
                    using (var rdrRoles = cmdRoles.ExecuteReader())
                    {
                        while (rdrRoles.Read())
                        {
                            int roleId = rdrRoles.GetInt32(0);
                            ListItem item = chkRoles.Items.FindByValue(roleId.ToString());
                            if (item != null)
                            {
                                item.Selected = true;
                            }
                        }
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (NumericUserId == 0) return;

            using (var cn = new SqlConnection(ConnString))
            {
                cn.Open();
                using (var transaction = cn.BeginTransaction())
                {
                    try
                    {
                        // 1. Clear existing roles for this user
                        using (var cmdDel = new SqlCommand("DELETE FROM dbo.UserRoles WHERE UserId = @UserId", cn, transaction))
                        {
                            cmdDel.Parameters.AddWithValue("@UserId", NumericUserId);
                            cmdDel.ExecuteNonQuery();
                        }

                        // 2. Insert the newly selected roles
                        using (var cmdIns = new SqlCommand("INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)", cn, transaction))
                        {
                            cmdIns.Parameters.Add("@UserId", SqlDbType.Int).Value = NumericUserId;
                            cmdIns.Parameters.Add("@RoleId", SqlDbType.Int);

                            foreach (ListItem item in chkRoles.Items)
                            {
                                if (item.Selected)
                                {
                                    cmdIns.Parameters["@RoleId"].Value = Convert.ToInt32(item.Value);
                                    cmdIns.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                        ShowOk("Roles successfully updated for " + lblEmpName.Text);
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        // Ponytail Standard #3: Never expose raw exception details to client
                        ShowError("An unexpected error occurred while updating roles. Please try again.");
                    }
                }
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/ViewUser.aspx", false);
        }

        private void ShowOk(string msg)
        {
            PanelOK.Visible = true;
            PanelError.Visible = false;
            lblOk.Text = msg;
        }

        private void ShowError(string msg)
        {
            PanelOK.Visible = false;
            PanelError.Visible = true;
            lblErrorMsg.Text = msg;
        }
    }
}