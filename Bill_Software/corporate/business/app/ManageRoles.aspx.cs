using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class ManageRoles : System.Web.UI.Page
    {
        private string ConnString => ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadRoles();
                LoadAllPermissions();
            }
        }

        private void LoadRoles()
        {
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("SELECT RoleId, RoleName FROM dbo.Roles ORDER BY RoleName", cn))
            {
                var dt = new DataTable();
                var da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                ddlRoles.DataSource = dt;
                ddlRoles.DataTextField = "RoleName";
                ddlRoles.DataValueField = "RoleId";
                ddlRoles.DataBind();
                ddlRoles.Items.Insert(0, new ListItem("-- Select a Role --", "0"));
            }
        }

        private void LoadAllPermissions()
        {
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("SELECT PermissionId, PermissionKey, Description FROM dbo.Permissions ORDER BY PermissionKey", cn))
            {
                var dt = new DataTable();
                var da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                gvPermissions.DataSource = dt;
                gvPermissions.DataBind();
            }
        }

        protected void btnCreateRole_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRoleName.Text))
            {
                ShowError("Role name cannot be empty.");
                return;
            }

            try
            {
                using (var cn = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand("INSERT INTO dbo.Roles (RoleName, Description) VALUES (@RoleName, @Desc)", cn))
                {
                    cmd.Parameters.AddWithValue("@RoleName", txtRoleName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Desc", txtRoleDesc.Text.Trim());
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowOk("Role created successfully!");
                txtRoleName.Text = "";
                txtRoleDesc.Text = "";
                LoadRoles(); // Refresh the dropdown
            }
            catch (Exception ex)
            {
                ShowError("Error creating role: " + ex.Message);
            }
        }

        protected void ddlRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            // First, clear all checkboxes
            foreach (GridViewRow row in gvPermissions.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null) chk.Checked = false;
            }

            if (ddlRoles.SelectedValue == "0") return;

            // Load existing permissions for this role
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("SELECT PermissionId FROM dbo.RolePermissions WHERE RoleId = @RoleId", cn))
            {
                cmd.Parameters.AddWithValue("@RoleId", ddlRoles.SelectedValue);
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        int permId = rdr.GetInt32(0);

                        // Check the corresponding box in the GridView
                        foreach (GridViewRow row in gvPermissions.Rows)
                        {
                            int rowPermId = Convert.ToInt32(gvPermissions.DataKeys[row.RowIndex].Value);
                            if (rowPermId == permId)
                            {
                                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                                if (chk != null) chk.Checked = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        protected void btnSavePermissions_Click(object sender, EventArgs e)
        {
            if (ddlRoles.SelectedValue == "0")
            {
                ShowError("Please select a role first.");
                return;
            }

            int roleId = Convert.ToInt32(ddlRoles.SelectedValue);

            using (var cn = new SqlConnection(ConnString))
            {
                cn.Open();
                using (var transaction = cn.BeginTransaction())
                {
                    try
                    {
                        // 1. Delete old permissions for this role
                        using (var cmdDel = new SqlCommand("DELETE FROM dbo.RolePermissions WHERE RoleId = @RoleId", cn, transaction))
                        {
                            cmdDel.Parameters.AddWithValue("@RoleId", roleId);
                            cmdDel.ExecuteNonQuery();
                        }

                        // 2. Insert checked permissions
                        using (var cmdIns = new SqlCommand("INSERT INTO dbo.RolePermissions (RoleId, PermissionId) VALUES (@RoleId, @PermId)", cn, transaction))
                        {
                            cmdIns.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;
                            cmdIns.Parameters.Add("@PermId", SqlDbType.Int);

                            foreach (GridViewRow row in gvPermissions.Rows)
                            {
                                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                                if (chk != null && chk.Checked)
                                {
                                    int permId = Convert.ToInt32(gvPermissions.DataKeys[row.RowIndex].Value);
                                    cmdIns.Parameters["@PermId"].Value = permId;
                                    cmdIns.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                        ShowOk("Permissions updated successfully for " + ddlRoles.SelectedItem.Text);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ShowError("Failed to save permissions: " + ex.Message);
                    }
                }
            }
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