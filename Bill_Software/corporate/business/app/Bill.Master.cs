using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace Bill_Software.corporate.business.app
{
    public partial class Bill : System.Web.UI.MasterPage
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        // Use your web.config connection string directly for the new RBAC queries
        private string ConnString => ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Dynamic year display
                int currentYear = DateTime.Now.Year;
                lbl_crntyr.Text = $"{currentYear - 1}-{currentYear}";

                GetMenuControl();
            }

            HttpContext.Current.Response.Cache.SetAllowResponseInBrowserHistory(false);
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Current.Response.Cache.SetNoStore();

            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx", false);
            }
            GetAdminName();
        }

        private void GetAdminName()
        {
            if (Session["USERID"] == null) return;

            string UserName = Session["USERID"].ToString();
            string cmdString = "SELECT Name FROM tbl_login WHERE User_Id=@UserId";

            try
            {
                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                using (SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserName);
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            lblName.Text = rdr["Name"].ToString();
                        }
                    }
                }
            }
            finally
            {
                DbCL.DisconnectDb(); // Ensures the connection is safely closed
            }
        }

        private void GetMenuControl()
        {
            if (Session["USERID"] == null) return;
            string UserName = Session["USERID"].ToString();

            List<string> allSystemPermissions = new List<string>();
            HashSet<string> userGrantedPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (var cn = new SqlConnection(ConnString))
            {
                cn.Open();

                // 1. Get ALL permissions in the system to know which UI elements we need to manage
                using (var cmdAll = new SqlCommand("SELECT PermissionKey FROM dbo.Permissions", cn))
                using (var rdrAll = cmdAll.ExecuteReader())
                {
                    while (rdrAll.Read())
                    {
                        allSystemPermissions.Add(rdrAll.GetString(0));
                    }
                }

                // 2. Get ONLY the permissions the current user has access to via their assigned roles
                string sqlUserPerms = @"
                    SELECT DISTINCT p.PermissionKey 
                    FROM dbo.Permissions p
                    INNER JOIN dbo.RolePermissions rp ON p.PermissionId = rp.PermissionId
                    INNER JOIN dbo.UserRoles ur ON rp.RoleId = ur.RoleId
                    INNER JOIN dbo.tbl_login u ON ur.UserId = u.Id
                    WHERE u.User_Id = @UserId";

                using (var cmdUser = new SqlCommand(sqlUserPerms, cn))
                {
                    cmdUser.Parameters.AddWithValue("@UserId", UserName);
                    using (var rdrUser = cmdUser.ExecuteReader())
                    {
                        while (rdrUser.Read())
                        {
                            userGrantedPermissions.Add(rdrUser.GetString(0));
                        }
                    }
                }
            }

            // 3. Loop through all system permissions, find their matching UI control, and show/hide it
            foreach (string menuId in allSystemPermissions)
            {
                Control menuControl = FindControlRecursive(this, menuId);

                if (menuControl != null)
                {
                    // If the user's granted permissions list contains this menuId, make it visible. 
                    // Otherwise, hide it.
                    menuControl.Visible = userGrantedPermissions.Contains(menuId);
                }
            }
        }

        // Helper function to deeply search the Master page for controls by ID
        private Control FindControlRecursive(Control rootControl, string controlID)
        {
            if (rootControl.ID == controlID) return rootControl;

            foreach (Control controlToSearch in rootControl.Controls)
            {
                Control controlToReturn = FindControlRecursive(controlToSearch, controlID);
                if (controlToReturn != null)
                {
                    return controlToReturn;
                }
            }
            return null;
        }

        protected void btnLogOut_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/index.aspx", false);
        }
    }
}

//using System;
 //using System.Data;
 //using System.Data.SqlClient;
 //using System.Web;
 //using System.Web.UI;

//namespace Bill_Software.corporate.business.app
//{
//    public partial class Bill : System.Web.UI.MasterPage
//    {
//        DB_UTILITY DbCL = new DB_UTILITY();
//        DataTable dtm = new DataTable();

//        protected void Page_Load(object sender, EventArgs e)
//        {
//            if (!IsPostBack)
//            {
//                // Dynamic year display
//                int currentYear = DateTime.Now.Year;
//                lbl_crntyr.Text = $"{currentYear - 1}-{currentYear}";

//                GetMenuControl();
//            }

//            HttpContext.Current.Response.Cache.SetAllowResponseInBrowserHistory(false);
//            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
//            HttpContext.Current.Response.Cache.SetNoStore();

//            if (HttpContext.Current.Session["USERID"] == null)
//            {
//                Response.Redirect("~/index.aspx", false);
//            }
//            GetAdminName();
//        }

//        private void GetAdminName()
//        {
//            if (Session["USERID"] == null) return;

//            string UserName = Session["USERID"].ToString();
//            string cmdString = "SELECT Name FROM tbl_login WHERE User_Id=@UserId";

//            try
//            {
//                DbCL.Sqlconnection();
//                DbCL.ConnectDb();
//                using (SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn))
//                {
//                    cmd.Parameters.AddWithValue("@UserId", UserName);
//                    using (SqlDataReader rdr = cmd.ExecuteReader())
//                    {
//                        if (rdr.Read())
//                        {
//                            lblName.Text = rdr["Name"].ToString();
//                        }
//                    }
//                }
//            }
//            finally
//            {
//                DbCL.DisconnectDb(); // Ensures the connection is safely closed
//            }
//        }

//        private void GetMenuControl()
//        {
//            if (Session["USERID"] == null) return;

//            string UserName = Session["USERID"].ToString();
//            string query = "SELECT * FROM vw_FullDesignation WHERE User_Id=@User_Id";
//            SqlParameter[] pram = { new SqlParameter("@User_Id", UserName) };

//            dtm = DbCL.SPreturn_dt(query, pram);

//            if (dtm != null && dtm.Rows.Count > 0)
//            {
//                DataRow row = dtm.Rows[0];

//                // Loop through every column returned from the database
//                foreach (DataColumn column in dtm.Columns)
//                {
//                    string menuId = column.ColumnName;

//                    // Find the HTML Control with the matching ID
//                    Control menuControl = FindControlRecursive(this, menuId);

//                    if (menuControl != null)
//                    {
//                        // Set visibility based on the "Yes" / "No" string in the database
//                        bool isVisible = row[menuId].ToString().Equals("Yes", StringComparison.OrdinalIgnoreCase);
//                        menuControl.Visible = isVisible;
//                    }
//                }
//            }
//        }

//        // Helper function to deeply search the Master page for controls by ID
//        private Control FindControlRecursive(Control rootControl, string controlID)
//        {
//            if (rootControl.ID == controlID) return rootControl;

//            foreach (Control controlToSearch in rootControl.Controls)
//            {
//                Control controlToReturn = FindControlRecursive(controlToSearch, controlID);
//                if (controlToReturn != null)
//                {
//                    return controlToReturn;
//                }
//            }
//            return null;
//        }

//        protected void btnLogOut_Click(object sender, EventArgs e)
//        {
//            Session.Clear();
//            Session.Abandon();
//            Response.Redirect("~/index.aspx", false);
//        }
//    }
//}