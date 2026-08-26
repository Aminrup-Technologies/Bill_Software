using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Bill_Software
{
    public class DB_UTILITY
    {
       
        public SqlDataReader dr;
        public SqlCommand cmd;
        public SqlDataAdapter da;
        public DataTable dt;
        public DataSet ds;


        public SqlConnection Conn;
        int flag = 0;
        public int Sqlconnection()
        {
            string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ToString();
            Conn = new SqlConnection(cnnString);
            flag = 1;
            return flag;
        }
        public void executeRdr(String SqlString)
        {
            try
            {
                Sqlconnection();
                ConnectDb();
                SqlCommand cmd = new SqlCommand(SqlString, Conn);
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
            }
            catch (Exception exp)
            {

                throw new Exception(exp.Message);
            }
            finally
            {
                Conn.Close();
            }
        }

        public DataTable ReturnDataTable(string cmdstring)
        {
            DataTable dt = new DataTable();
            try
            {
                // Ensure connection object is initialized (Method from your DB_UTILITY class)
                Sqlconnection();

                // Open the connection (Method from your DB_UTILITY class)
                ConnectDb();

                using (SqlCommand cmd = new SqlCommand(cmdstring, Conn))
                {
                    // Optional: Increase timeout for heavy queries
                    cmd.CommandTimeout = 180;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                // It is good practice to log the error here if you have a logging mechanism
                throw ex;
            }
            finally
            {
                // Ensure connection is closed even if an error occurs
                if (Conn.State == System.Data.ConnectionState.Open)
                {
                    Conn.Close();
                }
            }
            return dt;
        }

        public void ExecuteNonQuery(string sql, SqlParameter[] parameters)
        {
            try
            {
                Sqlconnection();
                ConnectDb();
                using (SqlCommand cmd = new SqlCommand(sql, Conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception exp)
            {
                throw new Exception("ExecuteNonQuery Error: " + exp.Message);
            }
            finally
            {
                if (Conn != null && Conn.State == ConnectionState.Open)
                    Conn.Close();
            }
        }



        public void executeRdrNew(string sql, Dictionary<string, object> parameters)
        {
            try
            {
                Sqlconnection();
                ConnectDb();
                SqlCommand cmd = new SqlCommand(sql, Conn);
                cmd.CommandTimeout = 0;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                cmd.ExecuteNonQuery();
            }
            catch (Exception exp)
            {
                throw new Exception(exp.Message);
            }
            finally
            {
                Conn.Close();
            }
        }

        public object ExecuteScalar(string query, SqlParameter[] parameters)
        {
            object result = null;
            try
            {
                Sqlconnection();
                ConnectDb();
                using (SqlCommand cmd = new SqlCommand(query, Conn))
                {
                    cmd.Parameters.AddRange(parameters);
                    cmd.CommandTimeout = 0;
                    result = cmd.ExecuteScalar(); // Gets a single value
                }
            }
            catch (Exception exp)
            {
                throw new Exception(exp.Message);
            }
            finally
            {
                Conn.Close();
            }
            return result;
        }


        public void ExecuteQuery(string sqlString, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ToString()))  // Use your actual connection string
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sqlString, conn))
                    {
                        cmd.CommandTimeout = 0;

                        // Add parameters if available
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception exp)
            {
                throw new Exception("Database error: " + exp.Message);
            }
        }


        public void ConnectDb()
        {
            try
            {
                if (Conn.State != ConnectionState.Open)
                    Conn.Open();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void DisconnectDb()
        {
            try
            {
                Conn.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DataTable GetDataTable(String cmdString)
        {
            Sqlconnection();
            SqlCommand cmd = new SqlCommand(cmdString, Conn);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            Conn.Close();
            return dt;

        }

        public void FillCombo(DropDownList cmbName, string cmdString)
        {
            cmbName.Items.Clear();
            Sqlconnection();
            ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdString, Conn);
            SqlDataReader Rdr;
            Rdr = cmd.ExecuteReader();
            cmbName.Items.Add("--Select--");
            while (Rdr.Read())
            {
                cmbName.Items.Add(Rdr[0].ToString());
            }
            Conn.Close();
        }

        public void FillComboNew(DropDownList cmbName, string cmdString)
        {
            cmbName.Items.Clear();
            Sqlconnection();
            ConnectDb();

            SqlCommand cmd = new SqlCommand(cmdString, Conn);
            SqlDataReader Rdr = cmd.ExecuteReader();

            cmbName.Items.Add(new ListItem("--Select--", ""));

            while (Rdr.Read())
            {
                string text = Rdr[1].ToString();  // Assuming Client_Name is at index 1
                string value = Rdr[0].ToString(); // Assuming Client_ID is at index 0
                cmbName.Items.Add(new ListItem(text, value));
            }

            Rdr.Close();
            Conn.Close();
        }


        public void FillCombo1(DropDownList cmbName, string cmdString)
        {
            cmbName.Items.Clear();
            Sqlconnection();
            ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdString, Conn);
            SqlDataReader Rdr;
            Rdr = cmd.ExecuteReader();

            while (Rdr.Read())
            {
                cmbName.Items.Add(Rdr[0].ToString());
            }
            Conn.Close();
        }
        public void FillCombo10(DropDownList cmbName, string cmdString)
        {
            
            Sqlconnection();
            ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdString, Conn);
            SqlDataReader Rdr;
            Rdr = cmd.ExecuteReader();

            while (Rdr.Read())
            {
                cmbName.Items.Add(Rdr[0].ToString());
            }
            Conn.Close();
        }
        public void FillCombo2(DropDownList cmbName, string cmdString)
        {

            Sqlconnection();
            ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdString, Conn);
            SqlDataReader Rdr;
            Rdr = cmd.ExecuteReader();

            while (Rdr.Read())
            {
                cmbName.Items.Add(Rdr[0].ToString());
            }
            Conn.Close();
        }
        public void calmonth(DropDownList cmbM1)
        {
            cmbM1.Items.Add("January");
            cmbM1.Items.Add("February");
            cmbM1.Items.Add("March");
            cmbM1.Items.Add("April");
            cmbM1.Items.Add("May");
            cmbM1.Items.Add("June");
            cmbM1.Items.Add("July");
            cmbM1.Items.Add("August");
            cmbM1.Items.Add("September");
            cmbM1.Items.Add("October");
            cmbM1.Items.Add("November");
            cmbM1.Items.Add("December");

        }

        public void CalDateCombo(DropDownList cmbD1, DropDownList cmbM1, DropDownList cmbY1)
        {


            int dd, yyend;
            string dt;
            cmbD1.Items.Clear();
            cmbM1.Items.Clear();
            cmbY1.Items.Clear();
            for (dd = 1; dd <= 31; dd++)
            {
                dt = dd.ToString();
                if (dt.Length == 1)
                { cmbD1.Items.Add("0" + dt); }
                else { cmbD1.Items.Add(dt); }
            }

            yyend = (DateTime.Now.Year);
            cmbY1.Items.Add((yyend - 14).ToString());
            cmbY1.Items.Add((yyend - 13).ToString());
            cmbY1.Items.Add((yyend - 12).ToString());
            cmbY1.Items.Add((yyend - 11).ToString());
            cmbY1.Items.Add((yyend - 10).ToString());
            cmbY1.Items.Add((yyend - 9).ToString());
            cmbY1.Items.Add((yyend - 8).ToString());
            cmbY1.Items.Add((yyend - 7).ToString());
            cmbY1.Items.Add((yyend - 6).ToString());
            cmbY1.Items.Add((yyend - 5).ToString());
            cmbY1.Items.Add((yyend - 4).ToString());
            cmbY1.Items.Add((yyend - 3).ToString());
            cmbY1.Items.Add((yyend - 2).ToString());
            cmbY1.Items.Add((yyend - 1).ToString());
            cmbY1.Items.Add(yyend.ToString());
            for (int i = 1; i <= 20; i++)
            {
                cmbY1.Items.Add((yyend + i).ToString());
            }
            cmbM1.Items.Add("01");
            cmbM1.Items.Add("02");
            cmbM1.Items.Add("03");
            cmbM1.Items.Add("04");
            cmbM1.Items.Add("05");
            cmbM1.Items.Add("06");
            cmbM1.Items.Add("07");
            cmbM1.Items.Add("08");
            cmbM1.Items.Add("09");
            cmbM1.Items.Add("10");
            cmbM1.Items.Add("11");
            cmbM1.Items.Add("12");
            DateTime now = DateTime.Now;
            cmbM1.Text = (now.ToString("MM"));
            cmbY1.Text = (now.ToString("yyyy"));
            cmbD1.Text = (now.ToString("dd"));

        }
        public void CalDateCombo8(DropDownList cmbM1, DropDownList cmbY1)
        {


            int yyend;


            cmbM1.Items.Clear();
            cmbY1.Items.Clear();


            yyend = (DateTime.Now.Year);
            for (int j = 10; j >= 1; j--)
            {


                cmbY1.Items.Add((yyend - j).ToString());
            }
            cmbY1.Items.Add(yyend.ToString());
            for (int i = 1; i <= 20; i++)
            {
                cmbY1.Items.Add((yyend + i).ToString());
            }

            cmbM1.Items.Add("Jan");
            cmbM1.Items.Add("Feb");
            cmbM1.Items.Add("Mar");
            cmbM1.Items.Add("Apr");
            cmbM1.Items.Add("May");
            cmbM1.Items.Add("Jun");
            cmbM1.Items.Add("Jul");
            cmbM1.Items.Add("Aug");
            cmbM1.Items.Add("Sep");
            cmbM1.Items.Add("Oct");
            cmbM1.Items.Add("Nov");
            cmbM1.Items.Add("Dec");
            DateTime now = DateTime.Now;
            cmbM1.Text = (now.ToString("MMM"));
            cmbY1.Text = (now.ToString("yyyy"));


        }
        public void CalDateCombo1(DropDownList cmbD1, DropDownList cmbM1, DropDownList cmbY1)
        {


            int dd, yyend;
            string dt;
            cmbD1.Items.Clear();
            cmbM1.Items.Clear();
            cmbY1.Items.Clear();
            for (dd = 1; dd <= 31; dd++)
            {
                dt = dd.ToString();
                if (dt.Length == 1)
                { cmbD1.Items.Add("0" + dt); }
                else { cmbD1.Items.Add(dt); }
            }

            yyend = (DateTime.Now.Year);
            for (int j = 15; j >= 1; j--)
            {


                cmbY1.Items.Add((yyend - j).ToString());
            }
            cmbY1.Items.Add(yyend.ToString());
            for (int i = 1; i <= 20; i++)
            {
                cmbY1.Items.Add((yyend + i).ToString());
            }
            cmbM1.Items.Add("01");
            cmbM1.Items.Add("02");
            cmbM1.Items.Add("03");
            cmbM1.Items.Add("04");
            cmbM1.Items.Add("05");
            cmbM1.Items.Add("06");
            cmbM1.Items.Add("07");
            cmbM1.Items.Add("08");
            cmbM1.Items.Add("09");
            cmbM1.Items.Add("10");
            cmbM1.Items.Add("11");
            cmbM1.Items.Add("12");
            DateTime now = DateTime.Now;
            cmbM1.Text = (now.ToString("MM"));
            cmbY1.Text = (now.ToString("yyyy"));
            cmbD1.Text = (now.ToString("dd"));

        }

        public void CalDateCombo5(DropDownList cmbD1, DropDownList cmbM1, DropDownList cmbY1)
        {


            int dd, yyend;
            string dt;
            cmbD1.Items.Clear();
            cmbM1.Items.Clear();
            cmbY1.Items.Clear();
            for (dd = 1; dd <= 31; dd++)
            {
                dt = dd.ToString();
                if (dt.Length == 1)
                { cmbD1.Items.Add("0" + dt); }
                else { cmbD1.Items.Add(dt); }
            }

            yyend = (DateTime.Now.Year);
            for (int j = 10; j >= 1; j--)
            {


                cmbY1.Items.Add((yyend - j).ToString());
            }
            cmbY1.Items.Add(yyend.ToString());
            for (int i = 1; i <= 20; i++)
            {
                cmbY1.Items.Add((yyend + i).ToString());
            }
            cmbM1.Items.Add("Month");
            cmbM1.Items.Add("Jan");
            cmbM1.Items.Add("Feb");
            cmbM1.Items.Add("Mar");
            cmbM1.Items.Add("Apr");
            cmbM1.Items.Add("May");
            cmbM1.Items.Add("Jun");
            cmbM1.Items.Add("Jul");
            cmbM1.Items.Add("Aug");
            cmbM1.Items.Add("Sep");
            cmbM1.Items.Add("Oct");
            cmbM1.Items.Add("Nov");
            cmbM1.Items.Add("Dec");
            DateTime now = DateTime.Now;
            cmbM1.Text = (now.ToString("MMM"));
            cmbY1.Text = (now.ToString("yyyy"));
            cmbD1.Text = (now.ToString("dd"));

        }

        public void Dob1DateCombo(DropDownList cmbD1, DropDownList cmbM1, DropDownList cmbY1)
        {
            int dd, yy, yyend;
            string dt;
            cmbD1.Items.Clear();
            cmbM1.Items.Clear();
            cmbY1.Items.Clear();

            cmbD1.Items.Add("Day");
            for (dd = 1; dd <= 31; dd++)
            {
                dt = dd.ToString();
                if (dt.Length == 1)
                { cmbD1.Items.Add("0" + dt); }
                else { cmbD1.Items.Add(dt); }
            }

            dd = (DateTime.Now.Year) - 70;
            yyend = (DateTime.Now.Year);
            cmbY1.Items.Add("Year");
            for (yy = dd; yy <= yyend; yy++)
            {
                cmbY1.Items.Add(yy.ToString());
            }

            cmbM1.Items.Add("Month");
            cmbM1.Items.Add("Jan");
            cmbM1.Items.Add("Feb");
            cmbM1.Items.Add("Mar");
            cmbM1.Items.Add("Apr");
            cmbM1.Items.Add("May");
            cmbM1.Items.Add("Jun");
            cmbM1.Items.Add("Jul");
            cmbM1.Items.Add("Aug");
            cmbM1.Items.Add("Sep");
            cmbM1.Items.Add("Oct");
            cmbM1.Items.Add("Nov");
            cmbM1.Items.Add("Dec");

        }


        public SqlDataReader SPReturnRdr(String SPName, SqlParameter[] SPParameter)
        {
            Sqlconnection();
            ConnectDb();
            cmd = new SqlCommand(SPName, Conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 0;
            if (SPParameter != null)
            {
                foreach (SqlParameter p in SPParameter)
                {
                    cmd.Parameters.Add(p);
                }
            }
            dr = cmd.ExecuteReader();
            return dr;
        }

        public int SPExecDB(String SPName, SqlParameter[] SPParameter)
        {
            Sqlconnection();
            ConnectDb();
            cmd = new SqlCommand(SPName, Conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 0;
            if (SPParameter != null)
            {
                foreach (SqlParameter p in SPParameter)
                {
                    cmd.Parameters.Add(p);
                }
            }
            int retVal = cmd.ExecuteNonQuery();
            if (retVal > 0)
            {
                return 1;
            }
            else
            {
                return 0;

            }

        }

        public int ExecDB(String strSql)
        {
            int retVal = 0;
            cmd = new SqlCommand();
            Sqlconnection();
            ConnectDb();
            cmd.Connection = Conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = strSql;
            retVal = cmd.ExecuteNonQuery();
            Conn.Close();
            if (retVal > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }
        public DataSet SPreturn_dataset(string s1, SqlParameter[] SPParameter)
        {
            Sqlconnection();
            ConnectDb();
            cmd = new SqlCommand(s1, Conn);
            cmd.CommandType = CommandType.Text;
            if (SPParameter != null)
            {
                foreach (SqlParameter p in SPParameter)
                {
                    cmd.Parameters.Add(p);
                }
            }
            SqlDataAdapter da = new SqlDataAdapter(cmd);

            ds = new DataSet();
            da.Fill(ds);
            Conn.Close();
            return ds;
        }

        public DataTable SPreturn_dt(string s1, SqlParameter[] SPParameter)
        {
            Sqlconnection();
            ConnectDb();
            cmd = new SqlCommand(s1, Conn);
            cmd.CommandType = CommandType.Text;
            if (SPParameter != null)
            {
                foreach (SqlParameter p in SPParameter)
                {
                    cmd.Parameters.Add(p);
                }
            }
            SqlDataAdapter da = new SqlDataAdapter(cmd);

            dt = new DataTable();
            da.Fill(dt);
            Conn.Close();
            return dt;
        }

        public void Sppopulate_Combo(String strsql, SqlParameter[] SPParameter, System.Web.UI.WebControls.DropDownList ddlDropdown)
        {
            try
            {
                ddlDropdown.Items.Clear();
                Sqlconnection();
                ConnectDb();
                SqlCommand cmd = new SqlCommand(strsql, Conn);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                if (SPParameter != null)
                {
                    foreach (SqlParameter p in SPParameter)
                    {
                        cmd.Parameters.Add(p);
                    }
                }
                dr = cmd.ExecuteReader();

                ddlDropdown.Items.Add("--Select--");

                while (dr.Read())
                {
                    ddlDropdown.Items.Add(dr.GetValue(0).ToString());
                }
                Conn.Close();
            }
            catch
            {

            }
        }

    }
}