using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;

namespace Bill_Software.corporate.business.app
{
    public class Heartbeat : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            if (context.Session["SessionToken"] == null)
            {
                context.Response.Write("{\"status\":\"logout\", \"reason\":\"missing_session\"}");
                return;
            }

            string sessionToken = context.Session["SessionToken"].ToString();
            string connString = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            using (var cn = new SqlConnection(connString))
            {
                cn.Open();

                string sql = "SELECT IsActive, LastHeartbeat FROM dbo.ActiveSessions WHERE SessionToken = @Token";
                using (var cmd = new SqlCommand(sql, cn))
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        bool isActive = rdr.GetBoolean(0);
                        DateTimeOffset lastHeartbeat = rdr.GetDateTimeOffset(1);

                        if (!isActive)
                        {
                            KillLocalSession(context);
                            context.Response.Write("{\"status\":\"logout\", \"reason\":\"superseded\"}");
                            return;
                        }

                        // 30-minute idle timeout
                        if ((DateTimeOffset.UtcNow - lastHeartbeat).TotalMinutes > 30)
                        {
                            KillLocalSession(context);
                            using (var cmdKill = new SqlCommand("UPDATE dbo.ActiveSessions SET IsActive = 0 WHERE SessionToken = @Token", cn))
                            {
                                cmdKill.Parameters.AddWithValue("@Token", sessionToken);
                                cmdKill.ExecuteNonQuery();
                            }
                            context.Response.Write("{\"status\":\"logout\", \"reason\":\"timeout\"}");
                            return;
                        }
                    }
                    else
                    {
                        KillLocalSession(context);
                        context.Response.Write("{\"status\":\"logout\", \"reason\":\"invalid\"}");
                        return;
                    }
                }

                // Session is valid! Update LastHeartbeat
                using (var cmdUpd = new SqlCommand("UPDATE dbo.ActiveSessions SET LastHeartbeat = sysutcdatetime() WHERE SessionToken = @Token", cn))
                {
                    cmdUpd.Parameters.AddWithValue("@Token", sessionToken);
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

        public bool IsReusable => false;
    }
}