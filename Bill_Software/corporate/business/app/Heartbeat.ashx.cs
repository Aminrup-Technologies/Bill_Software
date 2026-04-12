using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.SessionState; // Ensure this is explicitly included

namespace Bill_Software.corporate.business.app
{
    public class Heartbeat : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            if (context.Session["SessionToken"] == null)
            {
                context.Response.Write("{\"status\":\"logout\", \"reason\":\"missing_session\"}");
                return;
            }

            // 1. C# 5.0 Compatible OUT parameter parsing
            Guid sessionToken;
            if (!Guid.TryParse(context.Session["SessionToken"].ToString(), out sessionToken))
            {
                KillLocalSession(context);
                context.Response.Write("{\"status\":\"logout\", \"reason\":\"invalid_token_format\"}");
                return;
            }

            string connString = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            using (SqlConnection cn = new SqlConnection(connString))
            {
                cn.Open();

                bool isActive = false;
                bool isFound = false;
                DateTimeOffset lastHeartbeat = DateTimeOffset.MinValue;

                // Step 1: Read the session status
                string sql = "SELECT IsActive, LastHeartbeat FROM dbo.ActiveSessions WHERE SessionToken = @Token";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@Token", SqlDbType.UniqueIdentifier).Value = sessionToken;

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            isFound = true;
                            isActive = rdr.GetBoolean(0);
                            lastHeartbeat = rdr.GetDateTimeOffset(1);
                        }
                    }
                }

                // Step 2: Evaluate the status and take action
                if (!isFound)
                {
                    KillLocalSession(context);
                    context.Response.Write("{\"status\":\"logout\", \"reason\":\"invalid\"}");
                    return;
                }

                if (!isActive)
                {
                    KillLocalSession(context);
                    context.Response.Write("{\"status\":\"logout\", \"reason\":\"superseded\"}");
                    return;
                }

                // 30-minute idle timeout
                if ((DateTimeOffset.UtcNow - lastHeartbeat.ToUniversalTime()).TotalMinutes > 30)
                {
                    KillLocalSession(context);

                    using (SqlCommand cmdKill = new SqlCommand("UPDATE dbo.ActiveSessions SET IsActive = 0 WHERE SessionToken = @Token", cn))
                    {
                        cmdKill.Parameters.Add("@Token", SqlDbType.UniqueIdentifier).Value = sessionToken;
                        cmdKill.ExecuteNonQuery();
                    }

                    context.Response.Write("{\"status\":\"logout\", \"reason\":\"timeout\"}");
                    return;
                }

                // Step 3: Session is valid! Update LastHeartbeat
                using (SqlCommand cmdUpd = new SqlCommand("UPDATE dbo.ActiveSessions SET LastHeartbeat = SYSDATETIMEOFFSET() WHERE SessionToken = @Token", cn))
                {
                    cmdUpd.Parameters.Add("@Token", SqlDbType.UniqueIdentifier).Value = sessionToken;
                    cmdUpd.ExecuteNonQuery();
                }

                context.Response.Write("{\"status\":\"ok\"}");
            }
        }

        private void KillLocalSession(HttpContext context)
        {
            context.Session.Clear();
            context.Session.Abandon();
        }

        // FIX: C# 5.0 / ASP.NET 4.5.2 compatible property getter
        public bool IsReusable
        {
            get { return false; }
        }
    }
}