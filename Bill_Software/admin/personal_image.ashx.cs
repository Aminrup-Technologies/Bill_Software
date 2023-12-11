using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace Bill_Software.admin
{
    /// <summary>
    /// Summary description for personal_image
    /// </summary>
    public class personal_image : IHttpHandler
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public void ProcessRequest(HttpContext context)
        {
            string uin;

            if (context.Request.QueryString["ID"] != null)
                uin = Convert.ToString(context.Request.QueryString["ID"]);
            else
                throw new ArgumentException("No parameter specified");

            context.Response.ContentType = "image/jpeg";
            Stream strm = ShowEmpImage(uin);
            if (strm != null)
            {
                byte[] buffer = new byte[4096];
                int byteSeq = strm.Read(buffer, 0, 4096);

                while (byteSeq > 0)
                {
                    context.Response.OutputStream.Write(buffer, 0, byteSeq);
                    byteSeq = strm.Read(buffer, 0, 4096);
                }
            }
        }

        public Stream ShowEmpImage(string uin)
        {
            SqlConnection connection = new SqlConnection();
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            //connection.ConnectionString = @"Data Source=199.79.62.22;Initial Catalog=mailmantra_quantum; Persist Security Info=True;User ID=quantumsystem; Password=/*-Abc@123";
            string sql = "SELECT imgdata FROM tbl_employee WHERE ID =@ID";
            SqlCommand cmd = new SqlCommand(sql, DbCL.Conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@ID", uin);
            //connection.Open();
            object img = cmd.ExecuteScalar();
            try
            {
                return new MemoryStream((byte[])img);
            }
            catch
            {
                return null;
            }
            finally
            {
                DbCL.Conn.Close();
            }
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