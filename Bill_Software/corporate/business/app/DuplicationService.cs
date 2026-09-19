using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace Bill_Software.corporate.business.app
{
    /// <summary>
    /// Result returned by bulk duplication operations.
    /// </summary>
    public class BulkDuplicateResult
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }
        public string EntityType { get; set; }
        public string TargetCompanyName { get; set; }
        public string FailureReason { get; set; }
    }

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
        //  GENERALIZED BUSINESS-CODE GENERATION
        // ───────────────────────────────────────────────────────────

        /// <summary>
        /// Generates the next unique business code for the given prefix, scoped to the target company.
        /// Reads MAX(code) from the specified table/column, increments, and verifies no collision.
        /// Retries up to <paramref name="maxRetries"/> times.
        /// </summary>
        /// <param name="conn">Open connection inside an active transaction.</param>
        /// <param name="tran">The active transaction (uses UPDLOCK via IsolationLevel on the caller).</param>
        /// <param name="prefix">Two-character prefix, e.g. "AA" for Vendor, "AD" for Client.</param>
        /// <param name="tableName">Source table, e.g. "tbl_Vendor" or "tbl_Client".</param>
        /// <param name="codeColumn">Business-key column, e.g. "Vendor_Id" or "Client_Id".</param>
        /// <param name="targetCompanyId">CompanyID of the TARGET tenant.</param>
        /// <param name="maxRetries">Maximum collision-retry attempts (default 5).</param>
        private static string GenerateNextBusinessCode(
            SqlConnection conn, SqlTransaction tran,
            string prefix, string tableName, string codeColumn,
            int targetCompanyId, int maxRetries = 5)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                // Read the latest code for the TARGET company
                string lastCode = null;
                string query = string.Format(
                    "SELECT TOP 1 [{0}] FROM [{1}] WHERE CompanyID = @CompanyID ORDER BY Id DESC",
                    codeColumn, tableName);
                using (var cmd = new SqlCommand(query, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", targetCompanyId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read() && !r.IsDBNull(0))
                            lastCode = r.GetString(0);
                    }
                }

                int num = 1;
                if (!string.IsNullOrEmpty(lastCode) && lastCode.Length > prefix.Length)
                {
                    int.TryParse(lastCode.Substring(prefix.Length), out num);
                }
                num++;
                string candidate = prefix + num.ToString("D2");

                // Verify candidate does not already exist in the target company
                string checkQuery = string.Format(
                    "SELECT COUNT(*) FROM [{0}] WHERE [{1}] = @Code AND CompanyID = @CompanyID",
                    tableName, codeColumn);
                using (var cmd = new SqlCommand(checkQuery, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@Code", candidate);
                    cmd.Parameters.AddWithValue("@CompanyID", targetCompanyId);
                    int exists = Convert.ToInt32(cmd.ExecuteScalar());
                    if (exists == 0)
                        return candidate;
                }
            }

            throw new InvalidOperationException(
                string.Format("Unable to generate a unique code (prefix '{0}') for table {1} in company {2} after {3} attempts. Manual intervention required.",
                    prefix, tableName, targetCompanyId, maxRetries));
        }

        // ───────────────────────────────────────────────────────────
        //  GENERALIZED DUPLICATE-NAME RESOLUTION
        // ───────────────────────────────────────────────────────────

        /// <summary>
        /// Resolves a deterministic name for the target company.
        /// Returns the original name if no collision; otherwise appends (Copy), (Copy 2), etc.
        /// </summary>
        /// <param name="conn">Open connection.</param>
        /// <param name="tran">Active transaction.</param>
        /// <param name="tableName">Source table, e.g. "tbl_Vendor" or "tbl_Client".</param>
        /// <param name="nameColumn">Name column, e.g. "Vendor_Name" or "Client_Name".</param>
        /// <param name="originalName">The source entity's name.</param>
        /// <param name="targetCompanyId">CompanyID of the TARGET tenant.</param>
        private static string ResolveDuplicateName(
            SqlConnection conn, SqlTransaction tran,
            string tableName, string nameColumn,
            string originalName, int targetCompanyId)
        {
            // Check if original name already exists in target
            string baseQuery = string.Format(
                "SELECT COUNT(*) FROM [{0}] WHERE [{1}] = @Name AND CompanyID = @CompanyID",
                tableName, nameColumn);
            using (var cmd = new SqlCommand(baseQuery, conn, tran))
            {
                cmd.Parameters.AddWithValue("@Name", originalName);
                cmd.Parameters.AddWithValue("@CompanyID", targetCompanyId);
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    return originalName;
            }

            // Name exists — try (Copy), (Copy 2), (Copy 3), ...
            const int maxAttempts = 10;
            for (int i = 0; i < maxAttempts; i++)
            {
                string candidate = i == 0
                    ? originalName + " (Copy)"
                    : originalName + " (Copy " + (i + 1) + ")";

                using (var cmd = new SqlCommand(baseQuery, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@Name", candidate);
                    cmd.Parameters.AddWithValue("@CompanyID", targetCompanyId);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                        return candidate;
                }
            }

            // Fallback: append timestamp suffix (should be extremely rare)
            return originalName + " (Copy " + DateTime.Now.ToString("yyyyMMddHHmmss") + ")";
        }

        // ───────────────────────────────────────────────────────────
        //  VENDOR DUPLICATION (public API — backward compatible)
        // ───────────────────────────────────────────────────────────

        /// <summary>
        /// Generates the next unique Vendor_Id (AA prefix) scoped to the target company.
        /// Backward-compatible wrapper over the generalized code generator.
        /// </summary>
        public static string GenerateNextVendorCode(SqlConnection conn, SqlTransaction tran, int targetCompanyId)
        {
            return GenerateNextBusinessCode(conn, tran, "AA", "tbl_Vendor", "Vendor_Id", targetCompanyId);
        }

        /// <summary>
        /// Duplicates a single vendor from one tenant to another.
        /// Returns true on success; throws on unrecoverable failure.
        /// </summary>
        public static bool DuplicateVendor(int sourceId, int targetCompanyId, string userName)
        {
            if (sourceId <= 0)
                throw new ArgumentException("Invalid source vendor identifier.", "sourceId");
            if (targetCompanyId <= 0)
                throw new ArgumentException("Invalid target company identifier.", "targetCompanyId");
            if (string.IsNullOrWhiteSpace(userName))
                userName = "System";

            using (var conn = new SqlConnection(ConnString))
            {
                conn.Open();

                if (!VendorInCurrentCompany(conn, sourceId))
                    throw new InvalidOperationException(
                        "Source vendor not found or does not belong to your current company.");

                if (!UserCanAccessCompany(targetCompanyId))
                    throw new UnauthorizedAccessException(
                        "You do not have access to the selected target company.");

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
                        string srcVendorId = RowStr(src, "Vendor_Id");
                        string srcName = RowStr(src, "Vendor_Name");
                        if (string.IsNullOrEmpty(srcName)) srcName = "Vendor";

                        string targetName = ResolveDuplicateName(conn, tran, "tbl_Vendor", "Vendor_Name", srcName, targetCompanyId);
                        string newVendorId = GenerateNextVendorCode(conn, tran, targetCompanyId);

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
                            cmd.Parameters.AddWithValue("@Vendor_Name", targetName);
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
        /// Generates the next unique Client_Id (AD prefix) scoped to the target company.
        /// </summary>
        public static string GenerateNextClientCode(SqlConnection conn, SqlTransaction tran, int targetCompanyId)
        {
            return GenerateNextBusinessCode(conn, tran, "AD", "tbl_Client", "Client_Id", targetCompanyId);
        }

        /// <summary>
        /// Duplicates a customer and child records (ClientRegAddress, Factory, Representative).
        /// Returns true on success; throws on unrecoverable failure.
        /// </summary>
        /// <param name="sourceId">Integer PK (tbl_Client.Id) of the source customer.</param>
        /// <param name="targetCompanyId">CompanyID of the target tenant.</param>
        /// <param name="userName">Acting user's identity for audit.</param>
        public static bool DuplicateCustomer(int sourceId, int targetCompanyId, string userName)
        {
            if (sourceId <= 0)
                throw new ArgumentException("Invalid source client identifier.", "sourceId");
            if (targetCompanyId <= 0)
                throw new ArgumentException("Invalid target company identifier.", "targetCompanyId");
            if (string.IsNullOrWhiteSpace(userName))
                userName = "System";

            using (var conn = new SqlConnection(ConnString))
            {
                conn.Open();

                if (!ClientInCurrentCompany(conn, sourceId))
                    throw new InvalidOperationException(
                        "Source client not found or does not belong to your current company.");

                if (!UserCanAccessCompany(targetCompanyId))
                    throw new UnauthorizedAccessException(
                        "You do not have access to the selected target company.");

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
                        string srcName = RowStr(src, "Client_Name");
                        if (string.IsNullOrEmpty(srcName)) srcName = "Client";

                        // --- Name collision resolution (deterministic) ---
                        string targetName = ResolveDuplicateName(conn, tran, "tbl_Client", "Client_Name", srcName, targetCompanyId);

                        // --- Client_Id generation with collision retry ---
                        string newClientId = GenerateNextClientCode(conn, tran, targetCompanyId);

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
                            cmd.Parameters.AddWithValue("@Client_Name", targetName);
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
                        CopyClientRegAddress(conn, tran, srcClientId, newClientId, targetCompanyId, userName);
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

        private static void CopyClientRegAddress(SqlConnection conn, SqlTransaction tran, string sourceClientId, string targetClientId, int targetCompanyId, string userName)
        {
            DataTable dt = new DataTable();
            using (var cmd = new SqlCommand(
                "SELECT * FROM tbl_ClientRegAddress WHERE Client_Id = @ClientId AND CompanyID = @SourceCompanyID", conn, tran))
            {
                cmd.Parameters.AddWithValue("@ClientId", sourceClientId);
                cmd.Parameters.AddWithValue("@SourceCompanyID", CompanyContext.CurrentCompanyID);
                using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
            }
            foreach (DataRow r in dt.Rows)
            {
                using (var cmd = new SqlCommand(@"
                    INSERT INTO tbl_ClientRegAddress
                    (Client_Id, Address, State, City, Phno, pin, CompanyID, CreatedBy, CreatedOn)
                    VALUES
                    (@Client_Id, @Address, @State, @City, @Phno, @pin, @CompanyID, @CreatedBy, GETDATE())", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@Client_Id", targetClientId);
                    cmd.Parameters.AddWithValue("@Address", RowStr(r, "Address"));
                    cmd.Parameters.AddWithValue("@State", RowStr(r, "State"));
                    cmd.Parameters.AddWithValue("@City", RowStr(r, "City"));
                    cmd.Parameters.AddWithValue("@Phno", RowStr(r, "Phno"));
                    cmd.Parameters.AddWithValue("@pin", RowStr(r, "pin"));
                    cmd.Parameters.AddWithValue("@CompanyID", targetCompanyId);
                    cmd.Parameters.AddWithValue("@CreatedBy", userName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void CopyFactoryRecords(SqlConnection conn, SqlTransaction tran, string sourceClientId, string targetClientId, int targetCompanyId, string userName)
        {
            DataTable dt = new DataTable();
            using (var cmd = new SqlCommand(
                "SELECT * FROM tbl_Factory WHERE Client_id = @ClientId AND CompanyID = @SourceCompanyID", conn, tran))
            {
                cmd.Parameters.AddWithValue("@ClientId", sourceClientId);
                cmd.Parameters.AddWithValue("@SourceCompanyID", CompanyContext.CurrentCompanyID);
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
            using (var cmd = new SqlCommand(
                "SELECT * FROM tbl_representative WHERE Copany_Id = @ClientId AND CompanyID = @SourceCompanyID", conn, tran))
            {
                cmd.Parameters.AddWithValue("@ClientId", sourceClientId);
                cmd.Parameters.AddWithValue("@SourceCompanyID", CompanyContext.CurrentCompanyID);
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
        //  BULK VENDOR DUPLICATION
        // ───────────────────────────────────────────────────────────

        /// <summary>
        /// Duplicates multiple vendors inside a single transaction.
        /// Aborts and rolls back the entire batch on first failure.
        /// </summary>
        public static BulkDuplicateResult BulkDuplicateVendors(int[] sourceIds, int targetCompanyId, string userName)
        {
            if (sourceIds == null || sourceIds.Length == 0)
                throw new ArgumentException("No vendors selected for duplication.", "sourceIds");
            if (targetCompanyId <= 0)
                throw new ArgumentException("Invalid target company identifier.", "targetCompanyId");
            if (string.IsNullOrWhiteSpace(userName))
                userName = "System";

            var result = new BulkDuplicateResult { EntityType = "Vendor", SuccessCount = 0 };

            using (var conn = new SqlConnection(ConnString))
            {
                conn.Open();

                if (!UserCanAccessCompany(targetCompanyId))
                    throw new UnauthorizedAccessException(
                        "You do not have access to the selected target company.");

                using (var tran = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        foreach (int sourceId in sourceIds)
                        {
                            if (sourceId <= 0)
                            {
                                result.SkippedCount++;
                                continue;
                            }

                            if (!VendorInCurrentCompany(conn, sourceId))
                            {
                                result.SkippedCount++;
                                continue;
                            }

                            // Read source vendor data
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
                                result.SkippedCount++;
                                continue;
                            }

                            DataRow src = dtVendor.Rows[0];
                            string srcVendorId = RowStr(src, "Vendor_Id");
                            string srcName = RowStr(src, "Vendor_Name");
                            if (string.IsNullOrEmpty(srcName)) srcName = "Vendor";

                            string targetName = ResolveDuplicateName(conn, tran, "tbl_Vendor", "Vendor_Name", srcName, targetCompanyId);
                            string newVendorId = GenerateNextVendorCode(conn, tran, targetCompanyId);

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
                                cmd.Parameters.AddWithValue("@Vendor_Name", targetName);
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
                            WriteDuplicationAudit(
                                conn,
                                tran,
                                targetCompanyId,
                                "Vendor Duplicated",
                                "Vendor",
                                string.Format(
                                    "Vendor '{0}' duplicated from company {1} to company {2}: source code '{3}' as '{4}' (new code {5}) by user '{6}' (bulk).",
                                    srcName,
                                    CompanyContext.CurrentCompanyID,
                                    targetCompanyId,
                                    srcVendorId,
                                    targetName,
                                    newVendorId,
                                    userName),
                                userName);

                            result.SuccessCount++;
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        try { tran.Rollback(); } catch { }
                        result.FailedCount = result.SuccessCount + result.FailedCount + result.SkippedCount;
                        result.SuccessCount = 0;
                        result.FailureReason = "Batch aborted: one or more vendors failed to duplicate. The entire batch has been rolled back.";
                        throw;
                    }
                }
            }

            return result;
        }

        // ───────────────────────────────────────────────────────────
        //  BULK CUSTOMER DUPLICATION
        // ───────────────────────────────────────────────────────────

        /// <summary>
        /// Duplicates multiple customers and child records inside a single transaction.
        /// Aborts and rolls back the entire batch on first failure.
        /// </summary>
        public static BulkDuplicateResult BulkDuplicateCustomers(int[] sourceIds, int targetCompanyId, string userName)
        {
            if (sourceIds == null || sourceIds.Length == 0)
                throw new ArgumentException("No customers selected for duplication.", "sourceIds");
            if (targetCompanyId <= 0)
                throw new ArgumentException("Invalid target company identifier.", "targetCompanyId");
            if (string.IsNullOrWhiteSpace(userName))
                userName = "System";

            var result = new BulkDuplicateResult { EntityType = "Customer", SuccessCount = 0 };

            using (var conn = new SqlConnection(ConnString))
            {
                conn.Open();

                if (!UserCanAccessCompany(targetCompanyId))
                    throw new UnauthorizedAccessException(
                        "You do not have access to the selected target company.");

                using (var tran = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        foreach (int sourceId in sourceIds)
                        {
                            if (sourceId <= 0)
                            {
                                result.SkippedCount++;
                                continue;
                            }

                            if (!ClientInCurrentCompany(conn, sourceId))
                            {
                                result.SkippedCount++;
                                continue;
                            }

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
                                result.SkippedCount++;
                                continue;
                            }

                            DataRow src = dtClient.Rows[0];
                            string srcClientId = RowStr(src, "Client_Id");
                            string srcName = RowStr(src, "Client_Name");
                            if (string.IsNullOrEmpty(srcName)) srcName = "Client";

                            string targetName = ResolveDuplicateName(conn, tran, "tbl_Client", "Client_Name", srcName, targetCompanyId);
                            string newClientId = GenerateNextClientCode(conn, tran, targetCompanyId);

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
                                cmd.Parameters.AddWithValue("@Client_Name", targetName);
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

                            CopyClientRegAddress(conn, tran, srcClientId, newClientId, targetCompanyId, userName);
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
                                    "Customer '{0}' duplicated from company {1} to company {2}: source code '{3}' as '{4}' (new code {5}) by user '{6}' (bulk).",
                                    srcName,
                                    CompanyContext.CurrentCompanyID,
                                    targetCompanyId,
                                    srcClientId,
                                    targetName,
                                    newClientId,
                                    userName),
                                userName);

                            result.SuccessCount++;
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        try { tran.Rollback(); } catch { }
                        result.FailedCount = result.SuccessCount + result.FailedCount + result.SkippedCount;
                        result.SuccessCount = 0;
                        result.FailureReason = "Batch aborted: one or more customers failed to duplicate. The entire batch has been rolled back.";
                        throw;
                    }
                }
            }

            return result;
        }

        // ───────────────────────────────────────────────────────────
        //  INTERNAL HELPERS
        // ───────────────────────────────────────────────────────────

        private static bool VendorInCurrentCompany(SqlConnection conn, int vendorId)
        {
            using (var cmd = new SqlCommand(
                "SELECT TOP 1 1 FROM tbl_Vendor WHERE Id = @Id AND CompanyID = @CompanyID", conn))
            {
                cmd.Parameters.AddWithValue("@Id", vendorId);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                return cmd.ExecuteScalar() != null;
            }
        }

        private static bool ClientInCurrentCompany(SqlConnection conn, int clientId)
        {
            using (var cmd = new SqlCommand(
                "SELECT TOP 1 1 FROM tbl_Client WHERE Id = @Id AND CompanyID = @CompanyID", conn))
            {
                cmd.Parameters.AddWithValue("@Id", clientId);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                return cmd.ExecuteScalar() != null;
            }
        }

        private static bool UserCanAccessCompany(int companyId)
        {
            return AuthGuard.UserCanAccessCompany(companyId);
        }

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
