using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class ManagePermissions : System.Web.UI.Page
    {
        private string ConnString => ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["USERID"] == null || Session["SessionToken"] == null)
            {
                Response.Redirect("~/index.aspx", false);
                return;
            }

            if (!IsPostBack)
            {
                BindGrid();
            }
        }

        private void BindGrid()
        {
            using (var cn = new SqlConnection(ConnString))
            {
                string sql = "SELECT PermissionId, PermissionKey, ModuleName, SubModuleName, FeatureName, Description FROM dbo.Permissions ORDER BY ModuleName, SubModuleName, FeatureName";
                using (var cmd = new SqlCommand(sql, cn))
                {
                    var dt = new DataTable();
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    gvPermissions.DataSource = dt;
                    gvPermissions.DataBind();
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string permKey = txtPermissionKey.Text.Trim();
            string module = txtModuleName.Text.Trim();
            string subModule = txtSubModuleName.Text.Trim();
            string feature = txtFeatureName.Text.Trim();
            string desc = txtDescription.Text.Trim();

            if (string.IsNullOrEmpty(permKey) || string.IsNullOrEmpty(module) || string.IsNullOrEmpty(subModule) || string.IsNullOrEmpty(feature))
            {
                ShowError("HTML ID, Module, Sub-Module, and Feature Name are strictly required.");
                return;
            }

            int editId = Convert.ToInt32(hfEditPermissionId.Value);

            try
            {
                using (var cn = new SqlConnection(ConnString))
                {
                    cn.Open();
                    if (editId == 0)
                    {
                        // INSERT NEW
                        // First check if key already exists to prevent duplicate constraint errors
                        using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Permissions WHERE PermissionKey = @Key", cn))
                        {
                            checkCmd.Parameters.AddWithValue("@Key", permKey);
                            if ((int)checkCmd.ExecuteScalar() > 0)
                            {
                                ShowError("A permission with this HTML ID already exists.");
                                return;
                            }
                        }

                        string sqlIns = @"INSERT INTO dbo.Permissions (PermissionKey, ModuleName, SubModuleName, FeatureName, Description) 
                                          VALUES (@Key, @Mod, @SubMod, @Feat, @Desc)";
                        using (var cmd = new SqlCommand(sqlIns, cn))
                        {
                            cmd.Parameters.AddWithValue("@Key", permKey);
                            cmd.Parameters.AddWithValue("@Mod", module);
                            cmd.Parameters.AddWithValue("@SubMod", subModule);
                            cmd.Parameters.AddWithValue("@Feat", feature);
                            cmd.Parameters.AddWithValue("@Desc", desc);
                            cmd.ExecuteNonQuery();
                        }
                        ShowSuccess("New permission successfully registered!");
                    }
                    else
                    {
                        // UPDATE EXISTING
                        string sqlUpd = @"UPDATE dbo.Permissions 
                                          SET PermissionKey = @Key, ModuleName = @Mod, SubModuleName = @SubMod, FeatureName = @Feat, Description = @Desc 
                                          WHERE PermissionId = @Id";
                        using (var cmd = new SqlCommand(sqlUpd, cn))
                        {
                            cmd.Parameters.AddWithValue("@Key", permKey);
                            cmd.Parameters.AddWithValue("@Mod", module);
                            cmd.Parameters.AddWithValue("@SubMod", subModule);
                            cmd.Parameters.AddWithValue("@Feat", feature);
                            cmd.Parameters.AddWithValue("@Desc", desc);
                            cmd.Parameters.AddWithValue("@Id", editId);
                            cmd.ExecuteNonQuery();
                        }
                        ShowSuccess("Permission details successfully updated!");
                    }
                }
                ClearForm();
                BindGrid();
            }
            catch (Exception ex)
            {
                ShowError("An error occurred: " + ex.Message);
            }
        }

        protected void gvPermissions_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int permId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditPerm")
            {
                LoadForEdit(permId);
            }
            else if (e.CommandName == "DeletePerm")
            {
                DeletePermission(permId);
            }
        }

        private void LoadForEdit(int permId)
        {
            using (var cn = new SqlConnection(ConnString))
            {
                string sql = "SELECT PermissionKey, ModuleName, SubModuleName, FeatureName, Description FROM dbo.Permissions WHERE PermissionId = @Id";
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Id", permId);
                    cn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            hfEditPermissionId.Value = permId.ToString();
                            txtPermissionKey.Text = rdr["PermissionKey"].ToString();
                            txtModuleName.Text = rdr["ModuleName"].ToString();
                            txtSubModuleName.Text = rdr["SubModuleName"].ToString();
                            txtFeatureName.Text = rdr["FeatureName"].ToString();
                            txtDescription.Text = rdr["Description"].ToString();

                            btnSave.Text = "Update Permission";
                            PanelError.Visible = false;
                            PanelOK.Visible = false;
                        }
                    }
                }
            }
        }

        private void DeletePermission(int permId)
        {
            using (var cn = new SqlConnection(ConnString))
            {
                cn.Open();
                using (var transaction = cn.BeginTransaction())
                {
                    try
                    {
                        // Must delete Role mapping first due to Foreign Key constraints
                        using (var cmd1 = new SqlCommand("DELETE FROM dbo.RolePermissions WHERE PermissionId = @Id", cn, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@Id", permId);
                            cmd1.ExecuteNonQuery();
                        }

                        // Delete the permission itself
                        using (var cmd2 = new SqlCommand("DELETE FROM dbo.Permissions WHERE PermissionId = @Id", cn, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@Id", permId);
                            cmd2.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        ShowSuccess("Permission permanently deleted.");
                        BindGrid();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ShowError("Failed to delete permission: " + ex.Message);
                    }
                }
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            PanelOK.Visible = false;
            PanelError.Visible = false;
        }

        private void ClearForm()
        {
            hfEditPermissionId.Value = "0";
            txtPermissionKey.Text = "";
            txtModuleName.Text = "";
            txtSubModuleName.Text = "";
            txtFeatureName.Text = "";
            txtDescription.Text = "";
            btnSave.Text = "Save Permission";
        }

        private void ShowSuccess(string msg)
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