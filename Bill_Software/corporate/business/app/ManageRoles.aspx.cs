using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class ManageRoles : System.Web.UI.Page
    {
        private string ConnString => ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        // Store all permissions in memory for quick data binding of the nested repeaters
        private DataTable AllPermissionsTable
        {
            get { return (DataTable)ViewState["AllPermissionsTable"]; }
            set { ViewState["AllPermissionsTable"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["USERID"] == null || Session["SessionToken"] == null)
            {
                Response.Redirect("~/index.aspx", false);
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
            using (var cmd = new SqlCommand("SELECT PermissionId, PermissionKey, ModuleName, SubModuleName, FeatureName FROM dbo.Permissions ORDER BY ModuleName, SubModuleName, FeatureName", cn))
            {
                var dt = new DataTable();
                var da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                AllPermissionsTable = dt;

                // 1. Get Distinct Module Names for the Outer Repeater
                var modules = dt.AsEnumerable()
                                .Select(row => row.Field<string>("ModuleName"))
                                .Distinct()
                                .OrderBy(m => m)
                                .ToList();

                rptModules.DataSource = modules;
                rptModules.DataBind();
            }
        }

        protected void rptModules_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                string moduleName = (string)e.Item.DataItem;
                Repeater rptSubModules = (Repeater)e.Item.FindControl("rptSubModules");

                // 2. Get Distinct Sub-Module Names for this specific Module
                var subModules = AllPermissionsTable.AsEnumerable()
                                    .Where(row => row.Field<string>("ModuleName") == moduleName)
                                    .Select(row => row.Field<string>("SubModuleName"))
                                    .Distinct()
                                    .OrderBy(sm => sm)
                                    .ToList();

                // Bind the inner repeater but pass the ModuleName down via a custom class or Dictionary if needed. 
                // For simplicity, we can pass just the SubModule name and query both in the next level.
                // To be safe, let's pass an object containing both so we don't mix up identical sub-modules in different modules.
                var subModuleData = subModules.Select(sm => new { ModuleName = moduleName, SubModuleName = sm }).ToList();

                rptSubModules.DataSource = subModuleData;
                rptSubModules.DataBind();
            }
        }

        protected void rptSubModules_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                // Retrieve our anonymous object
                dynamic dataItem = e.Item.DataItem;
                string moduleName = dataItem.ModuleName;
                string subModuleName = dataItem.SubModuleName;

                CheckBoxList chkFeatures = (CheckBoxList)e.Item.FindControl("chkFeatures");

                // 3. Filter the exact features for this Module + SubModule
                var features = AllPermissionsTable.AsEnumerable()
                                    .Where(row => row.Field<string>("ModuleName") == moduleName &&
                                                  row.Field<string>("SubModuleName") == subModuleName)
                                    .CopyToDataTable();

                chkFeatures.DataSource = features;
                chkFeatures.DataBind();
            }
        }

        protected void ddlRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 1. Uncheck all checkboxes first
            ToggleAllCheckboxes(false);

            if (ddlRoles.SelectedValue == "0") return;

            // 2. Load existing permissions for this role
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("SELECT PermissionId FROM dbo.RolePermissions WHERE RoleId = @RoleId", cn))
            {
                cmd.Parameters.AddWithValue("@RoleId", ddlRoles.SelectedValue);
                cn.Open();

                var assignedPermissionIds = new HashSet<string>();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        assignedPermissionIds.Add(rdr.GetInt32(0).ToString());
                    }
                }

                // 3. Check the appropriate boxes
                foreach (RepeaterItem modItem in rptModules.Items)
                {
                    Repeater rptSub = (Repeater)modItem.FindControl("rptSubModules");
                    foreach (RepeaterItem subItem in rptSub.Items)
                    {
                        CheckBoxList chkFeatures = (CheckBoxList)subItem.FindControl("chkFeatures");
                        foreach (ListItem chk in chkFeatures.Items)
                        {
                            if (assignedPermissionIds.Contains(chk.Value))
                            {
                                chk.Selected = true;
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

                            // Loop through the nested hierarchy to find all checked boxes
                            foreach (RepeaterItem modItem in rptModules.Items)
                            {
                                Repeater rptSub = (Repeater)modItem.FindControl("rptSubModules");
                                foreach (RepeaterItem subItem in rptSub.Items)
                                {
                                    CheckBoxList chkFeatures = (CheckBoxList)subItem.FindControl("chkFeatures");
                                    foreach (ListItem chk in chkFeatures.Items)
                                    {
                                        if (chk.Selected)
                                        {
                                            cmdIns.Parameters["@PermId"].Value = Convert.ToInt32(chk.Value);
                                            cmdIns.ExecuteNonQuery();
                                        }
                                    }
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
                LoadRoles();
            }
            catch (Exception ex)
            {
                ShowError("Error creating role: " + ex.Message);
            }
        }

        private void ToggleAllCheckboxes(bool isChecked)
        {
            foreach (RepeaterItem modItem in rptModules.Items)
            {
                Repeater rptSub = (Repeater)modItem.FindControl("rptSubModules");
                foreach (RepeaterItem subItem in rptSub.Items)
                {
                    CheckBoxList chkFeatures = (CheckBoxList)subItem.FindControl("chkFeatures");
                    foreach (ListItem chk in chkFeatures.Items)
                    {
                        chk.Selected = isChecked;
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