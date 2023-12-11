using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Web.Script.Serialization;

namespace Bill_Software.corporate.business.app
{
    /// <summary>
    /// Summary description for clientHandlerAdmin
    /// </summary>
    public class clientHandlerAdmin : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string term = context.Request["term"] ?? "";
            List<string> listClientName = new List<string>();
            string cs = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("select Client_Name from tbl_Client where Client_Name like @term + '%'", con);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter prameter = new SqlParameter()
                {
                    ParameterName = "@term",
                    Value = term
                };
                cmd.Parameters.Add(prameter);
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    listClientName.Add(rdr["Client_Name"].ToString());
                }
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            context.Response.Write(js.Serialize(listClientName));
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}