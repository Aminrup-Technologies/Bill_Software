using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    /// <summary>
    /// Provides cross-tenant duplication infrastructure for Vendor and Customer entities.
    /// All operations use modern ADO.NET with explicit transactions.
    /// </summary>
    public static class DuplicationService
    {
        private static string ConnString
        {
            get { return ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString; }
        }

        public static void ExecuteWithinTransaction(SqlConnection conn, SqlTransaction tran, Action<SqlConnection, SqlTransaction> action)
        {
            action(conn, tran);
        }

        // ───────────────────────────────────────────────────────────
        //  VENDOR DUPLICATION
        // ───────────────────────────────────────────────────────────

        /// <summary>
        /// Generates the next unique Vendor_Id (AA prefix) using a MAX() counter
        /// within the active transaction.
        /// </summary>
        public static string GenerateNextVendorCode(SqlConnection conn, SqlTransaction tran)
        {
            string lastCode = null;
            using (var cmd = new SqlCommand(
                "SELECT TOP 1 [Vendor_Id] FROM [tbl_Vendor] WHERE CompanyID = @CompanyID ORDER BY Id DESC", conn, tran))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read() && !r.IsDBNull(0))
                        lastCode = r.GetString(0);
                }
            }

            int num = 1;
            if (!string.IsNullOrEmpty(lastCode) && lastCode.Length > 2)
            {
                int.TryParse(lastCode.Substring(2), out num);
            }
            num++;
            return "AA" + num.ToString("D2");
        }

        /// <summary>
        /// Duplicates a single vendor from one tenant to another within one SQL transaction.
        /// Copies master data only; skips purchase history.
        /// Generates a new Vendor_Id using the AA prefix.
        /// Writes an audit entry to tbl_SystemNotification scoped to the target company.
        /// </summary>
        public static bool DuplicateVendor(int sourceId, int targetCompanyId, string userName)
        {
            if (sourceId <= 0) return false;
            if (targetCompanyId <= 0) return false;
            if (string.IsNullOrWhiteSpace(userName)) userName = "System";

            using (var conn = new SqlConnection(ConnString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        DataTable dtVendor = new DataTable();
                        using (var cmd = new SqlCommand(
                            "SELECT * FROM tbl_Vendor WHERE Id = @Id AND CompanyID = @CompanyID", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@Id", sourceId);
                            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                            using (var da = new SqlDataAdapter(cmd))
                                da.Fill(dtVendor);
                        }

                        if (dtVendor.Rows.Count == 0)
                        {
                            tran.Rollback();
                            return false;
                        }

                        DataRow src = dtVendor.Rows[0];
                        string newVendorId = GenerateNextVendorCode(conn, tran);

                        using (var cmd = new SqlCommand(@"
                            INSERT INTO tbl_Vendor
                            (Vendor_Id, Vendor_Name, Address1, Address2, City, pin, State,
                             Com_web_site, Com_email, Com_phone, Com_Fax,
                             Rep_Name, Rep_Desig, Rep_phone, Rep_email,
                             Service_tax_No, Pan_No, Vat_No, PrincipleVndrCode,
                             BankAccNo, BankIfscCode, AccountName,
                             CompanyID, CreatedBy, CreatedOn)
                            VALUES
                            (@Vendor_Id, @Vendor_Name, @Address1, @Address2, @City, @pin, @State,
                             @Com_web_site, @Com_email, @Com_phone, @Com_Fax,
                             @Rep_Name, @Rep_Desig, @Rep_phone, @Rep_email,
                             @Service_tax_No, @Pan_No, @Vat_No, @PrincipleVndrCode,
                             @BankAccNo, @BankIfscCode, @AccountName,
                             @CompanyID, @CreatedBy, GETDATE())", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@Vendor_Id", newVendorId);
                            cmd.Parameters.AddWithValue("@Vendor_Name", RowStr(src, "Vendor_Name"));
                            cmd.Parameters.AddWithValue("@Address1", RowStr(src, "Address1"));
                            cmd.Parameters.AddWithValue("@Address2", RowStr(src, "Address2"));
                            cmd.Parameters.AddWithValue("@City", RowStr(src, "City"));
                            cmd.Parameters.AddWithValue("@pin", RowStr(src, "pin"));
                            cmd.Parameters.AddWithValue("@State", RowStr(src, "State"));
                            cmd.Parameters.AddWithValue("@Com_web_site", RowStr(src, "Com_web_site"));
                            cmd.Parameters.AddWithValue("@Com_email", RowStr(src, "Com_email"));
                            cmd.Parameters.AddWithValue("@Com_phone", RowStr(src, "Com_phone"));
                            cmd.Parameters.AddWithValue("@Com_Fax", RowStr(src, "Com_Fax"));
                            cmd.Parameters.AddWithValue("@Rep_Name", RowStr(src, "Rep_Name"));
                            cmd.Parameters.AddWithValue("@Rep_Desig", RowStr(src, "Rep_Desig"));
                            cmd.Parameters.AddWithValue("@Rep_phone", RowStr(src, "Rep_phone"));
                            cmd.Parameters.AddWithValue("@Rep_email", RowStr(src, "Rep_email"));
                            cmd.Parameters.AddWithValue("@Service_tax_No", RowStr(src, "Service_tax_No"));
                            cmd.Parameters.AddWithValue("@Pan_No", RowStr(src, "Pan_No"));
                            cmd.Parameters.AddWithValue("@Vat_No", RowStr(src, "Vat_No"));
                            cmd.Parameters.AddWithValue("@PrincipleVndrCode", RowStr(src, "PrincipleVndrCode"));
                            cmd.Parameters.AddWithValue("@BankAccNo", RowStr(src, "BankAccNo"));
                            cmd.Parameters.AddWithValue("@BankIfscCode", RowStr(src, "BankIfscCode"));
                            cmd.Parameters.AddWithValue("@AccountName", RowStr(src, "AccountName"));
                            cmd.Parameters.AddWithValue("@CompanyID", targetCompanyId);
                            cmd.Parameters.AddWithValue("@CreatedBy", userName);
                            cmd.ExecuteNonQuery();
                        }

                        // Audit trail — scoped to TARGET company (live tbl_SystemNotification schema)
                        string sourceVendorId = RowStr(src, "Vendor_Id");
                        WriteDuplicationAudit(
                            conn,
                            tran,
                            targetCompanyId,
                            "Vendor Duplicated",
                            "Vendor",
                            string.Format(
                                "Vendor '{0}' duplicated from company {1} to company {2}: source code '{3}' (Id {4}) → new code '{5}' by user '{6}'.",
                                RowStr(src, "Vendor_Name"),
                                CompanyContext.CurrentCompanyID,
                                targetCompanyId,
                                sourceVendorId,
                                sourceId,
                                newVendorId,
                                userName),
                            userName);

                        tran.Commit();
                        return true;
                    }
                    catch
                    {
                        try { tran.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        // ───────────────────────────────────────────────────────────
        //  CUSTOMER DUPLICATION
        // ───────────────────────────────────────────────────────────

        /// <summary>
        /// Generates the next unique Client_Id (AD prefix) using a MAX() counter
        /// within the active transaction.
        /// </summary>
        public static string GenerateNextClientCode(SqlConnection conn, SqlTransaction tran)
        {
            string lastCode = null;
            using (var cmd = new SqlCommand(
                "SELECT TOP 1 [Client_Id] FROM [tbl_Client] WHERE CompanyID = @CompanyID ORDER BY Id DESC", conn, tran))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read() && !r.IsDBNull(0))
                        lastCode = r.GetString(0);
                }
            }

            int num = 1;
            if (!string.IsNullOrEmpty(lastCode) && lastCode.Length > 2)
            {
                int.TryParse(lastCode.Substring(2), out num);
            }
            num++;
            return "AD" + num.ToString("D2");
        }

        /// <summary>
        /// Duplicates a customer and child records (ClientRegAddress, Factory, Representative)
        /// from one tenant to another within one SQL transaction.
        /// Excludes downstream transactional tables (Quotation, Invoice, Chalan, Proforma, Purchases).
        /// </summary>
        public static bool DuplicateCustomer(int sourceId, int targetCompanyId, string userName)
        {
            if (sourceId <= 0) return false;
            if (targetCompanyId <= 0) return false;
            if (string.IsNullOrWhiteSpace(userName)) userName = "System";

            using (var conn = new SqlConnection(ConnString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        // Read source client data
                        DataTable dtClient = new DataTable();
                        using (var cmd = new SqlCommand(
                            "SELECT * FROM tbl_Client WHERE Id = @Id AND CompanyID = @CompanyID", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@Id", sourceId);
                            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                            using (var da = new SqlDataAdapter(cmd))
                                da.Fill(dtClient);
                        }

                        if (dtClient.Rows.Count == 0)
                        {
                            tran.Rollback();
                            return false;
                        }

                        DataRow src = dtClient.Rows[0];
                        string srcClientId = RowStr(src, "Client_Id");
                        string newClientId = GenerateNextClientCode(conn, tran);

                        // Insert the duplicated client master
                        using (var cmd = new SqlCommand(@"
                            INSERT INTO tbl_Client
                            (Client_Id, Client_Name, Industry, Address1, State, City, pin,
                             Com_phone, Com_Fax, Com_web_site, Com_email,
                             Service_tax_no, Pan_no, PlaceofSupply,
                             CompanyID, CreatedBy, CreatedOn)
                            VALUES
                            (@Client_Id, @Client_Name, @Industry, @Address1, @State, @City, @pin,
                             @Com_phone, @Com_Fax, @Com_web_site, @Com_email,
                             @Service_tax_no, @Pan_no, @PlaceofSupply,
                             @CompanyID, @CreatedBy, GETDATE())", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@Client_Id", newClientId);
                            cmd.Parameters.AddWithValue("@Client_Name", RowStr(src, "Client_Name"));
                            cmd.Parameters.AddWithValue("@Industry", RowStr(src, "Industry"));
                            cmd.Parameters.AddWithValue("@Address1", RowStr(src, "Address1"));
                            cmd.Parameters.AddWithValue("@State", RowStr(src, "State"));
                            cmd.Parameters.AddWithValue("@City", RowStr(src, "City"));
                            cmd.Parameters.AddWithValue("@pin", RowStr(src, "pin"));
                            cmd.Parameters.AddWithValue("@Com_phone", RowStr(src, "Com_phone"));
                            cmd.Parameters.AddWithValue("@Com_Fax", RowStr(src, "Com_Fax"));
                            cmd.Parameters.AddWithValue("@Com_web_site", RowStr(src, "Com_web_site"));
                            cmd.Parameters.AddWithValue("@Com_email", RowStr(src, "Com_email"));
                            cmd.Parameters.AddWithValue("@Service_tax_no", RowStr(src, "Service_tax_no"));
                            cmd.Parameters.AddWithValue("@Pan_no", RowStr(src, "Pan_no"));
                            cmd.Parameters.AddWithValue("@PlaceofSupply", RowStr(src, "PlaceofSupply"));
                            cmd.Parameters.AddWithValue("@CompanyID", targetCompanyId);
                            cmd.Parameters.AddWithValue("@CreatedBy", userName);
                            cmd.ExecuteNonQuery();
                        }

                        // Clone child entities
                        CopyClientRegAddress(conn, tran, srcClientId, newClientId);
                        CopyFactoryRecords(conn, tran, srcClientId, newClientId, targetCompanyId, userName);
                        CopyRepresentativeRecords(conn, tran, srcClientId, newClientId, targetCompanyId, userName);

                        // Audit trail — scoped to TARGET company (live tbl_SystemNotification schema)
                        WriteDuplicationAudit(
                            conn,
                            tran,
                            targetCompanyId,
                            "Customer Duplicated",
                            "Client",
                            string.Format(
                                "Customer '{0}' duplicated from company {1} to company {2}: source code '{3}' (Id {4}) → new code '{5}' by user '{6}'.",
                                RowStr(src, "Client_Name"),
                                CompanyContext.CurrentCompanyID,
                                targetCompanyId,
                                srcClientId,
                                sourceId,
                                newClientId,
                                userName),
                            userName);

                        tran.Commit();
                        return true;
                    }
                    catch
                    {
                        try { tran.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        // ───────────────────────────────────────────────────────────
        //  CHILD-ENTITY CLONING
        // ───────────────────────────────────────────────────────────

        private static void CopyClientRegAddress(SqlConnection conn, SqlTransaction tran, string sourceClientId, string targetClientId)
        {
            DataTable dt = new DataTable();
            using (var cmd = new SqlCommand("SELECT * FROM tbl_ClientRegAddress WHERE Client_Id = @ClientId", conn, tran))
            {
                cmd.Parameters.AddWithValue("@ClientId", sourceClientId);
                using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
            }
            foreach (DataRow r in dt.Rows)
            {
                using (var cmd = new SqlCommand(@"
                    INSERT INTO tbl_ClientRegAddress
                    (Client_Id, Address1, Address2, City, State, pin, ContactPerson, Phone, Email)
                    VALUES
                    (@Client_Id, @Address1, @Address2, @City, @State, @pin, @ContactPerson, @Phone, @Email)", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@Client_Id", targetClientId);
                    cmd.Parameters.AddWithValue("@Address1", RowStr(r, "Address1"));
                    cmd.Parameters.AddWithValue("@Address2", RowStr(r, "Address2"));
                    cmd.Parameters.AddWithValue("@City", RowStr(r, "City"));
                    cmd.Parameters.AddWithValue("@State", RowStr(r, "State"));
                    cmd.Parameters.AddWithValue("@pin", RowStr(r, "pin"));
                    cmd.Parameters.AddWithValue("@ContactPerson", RowStr(r, "ContactPerson"));
                    cmd.Parameters.AddWithValue("@Phone", RowStr(r, "Phone"));
                    cmd.Parameters.AddWithValue("@Email", RowStr(r, "Email"));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void CopyFactoryRecords(SqlConnection conn, SqlTransaction tran, string sourceClientId, string targetClientId, int targetCompanyId, string userName)
        {
            DataTable dt = new DataTable();
            using (var cmd = new SqlCommand("SELECT * FROM tbl_Factory WHERE Client_id = @ClientId", conn, tran))
            {
                cmd.Parameters.AddWithValue("@ClientId", sourceClientId);
                using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
            }
            foreach (DataRow r in dt.Rows)
            {
                using (var cmd = new SqlCommand(@"
                    INSERT INTO tbl_Factory
                    (Client_id, Factory_name, Address1, Address2, city, State, pin, CompanyID, CreatedBy, CreatedOn)
                    VALUES
                    (@Client_id, @Factory_name, @Address1, @Address2, @city, @State, @pin, @CompanyID, @CreatedBy, GETDATE())", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@Client_id", targetClientId);
                    cmd.Parameters.AddWithValue("@Factory_name", RowStr(r, "Factory_name"));
                    cmd.Parameters.AddWithValue("@Address1", RowStr(r, "Address1"));
                    cmd.Parameters.AddWithValue("@Address2", RowStr(r, "Address2"));
                    cmd.Parameters.AddWithValue("@city", RowStr(r, "city"));
                    cmd.Parameters.AddWithValue("@State", RowStr(r, "State"));
                    cmd.Parameters.AddWithValue("@pin", RowStr(r, "pin"));
                    cmd.Parameters.AddWithValue("@CompanyID", targetCompanyId);
                    cmd.Parameters.AddWithValue("@CreatedBy", userName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void CopyRepresentativeRecords(SqlConnection conn, SqlTransaction tran, string sourceClientId, string targetClientId, int targetCompanyId, string userName)
        {
            DataTable dt = new DataTable();
            using (var cmd = new SqlCommand("SELECT * FROM tbl_representative WHERE Copany_Id = @ClientId", conn, tran))
            {
                cmd.Parameters.AddWithValue("@ClientId", sourceClientId);
                using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
            }
            foreach (DataRow r in dt.Rows)
            {
                using (var cmd = new SqlCommand(@"
                    INSERT INTO tbl_representative
                    (Copany_Id, RepTitle, Representative_name, RepLastName, Designation,
                     Phone_no, Email, CompanyID, CreatedBy, CreatedOn)
                    VALUES
                    (@Copany_Id, @RepTitle, @Representative_name, @RepLastName, @Designation,
                     @Phone_no, @Email, @CompanyID, @CreatedBy, GETDATE())", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@Copany_Id", targetClientId);
                    cmd.Parameters.AddWithValue("@RepTitle", RowStr(r, "RepTitle"));
                    cmd.Parameters.AddWithValue("@Representative_name", RowStr(r, "Representative_name"));
                    cmd.Parameters.AddWithValue("@RepLastName", RowStr(r, "RepLastName"));
                    cmd.Parameters.AddWithValue("@Designation", RowStr(r, "Designation"));
                    cmd.Parameters.AddWithValue("@Phone_no", RowStr(r, "Phone_no"));
                    cmd.Parameters.AddWithValue("@Email", RowStr(r, "Email"));
                    cmd.Parameters.AddWithValue("@CompanyID", targetCompanyId);
                    cmd.Parameters.AddWithValue("@CreatedBy", userName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ───────────────────────────────────────────────────────────
        //  INTERNAL HELPERS
        // ───────────────────────────────────────────────────────────

        /// <summary>
        /// Live tbl_SystemNotification columns: Title, Message, ModuleCode, Severity,
        /// StartDate, EndDate, IsActive, CreatedBy, CompanyID (not Module/Type/UserId).
        /// </summary>
        private static void WriteDuplicationAudit(
            SqlConnection conn,
            SqlTransaction tran,
            int targetCompanyId,
            string title,
            string moduleCode,
            string message,
            string userName)
        {
            using (var cmd = new SqlCommand(@"
                INSERT INTO dbo.tbl_SystemNotification
                (Title, Message, ModuleCode, Severity, StartDate, EndDate, IsActive, CreatedBy, CompanyID)
                VALUES
                (@Title, @Message, @ModuleCode, @Severity, GETDATE(), DATEADD(DAY, 30, GETDATE()), 1, @CreatedBy, @CompanyID)",
                conn, tran))
            {
                cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = title ?? string.Empty });
                cmd.Parameters.Add(new SqlParameter("@Message", SqlDbType.NVarChar, -1) { Value = message ?? string.Empty });
                cmd.Parameters.Add(new SqlParameter("@ModuleCode", SqlDbType.VarChar, 50) { Value = moduleCode ?? string.Empty });
                cmd.Parameters.Add(new SqlParameter("@Severity", SqlDbType.VarChar, 20) { Value = "Success" });
                cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.VarChar, 50)
                {
                    Value = string.IsNullOrEmpty(userName) ? (object)DBNull.Value : userName
                });
                cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = targetCompanyId });
                cmd.ExecuteNonQuery();
            }
        }

        private static string RowStr(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value)
                return string.Empty;
            return row[column].ToString().Trim();
        }
    }
}
