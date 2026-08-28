using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Bill_Software.corporate.business.app
{
    public static class CommunicationGateway
    {
        // --- 1. Fetch Web.Config Settings Securely ---
        private static readonly string smtpFrom = ConfigurationManager.AppSettings["SmtpFrom"];
        private static readonly string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
        private static readonly string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];
        private static readonly string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
        private static readonly int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
        private static readonly bool smtpEnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["SmtpEnableSsl"] ?? "true");

        private static readonly string msg91AuthKey = ConfigurationManager.AppSettings["Msg91AuthKey"];
        private static readonly string msg91IntegratedNumber = ConfigurationManager.AppSettings["Msg91IntegratedNumber"];

        // --- 2. Fire-and-Forget Wrapper ---
        // This ensures the UI never freezes while waiting for Zoho or MSG91 to respond
        // UPDATED: Added optional ccEmail parameter
        public static void SendAlertsAsync(string email, string mobile, string subject, string message, string ccEmail = null)
        {
            Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(email)) SendEmail(email, subject, message, ccEmail);
                if (!string.IsNullOrEmpty(mobile)) SendWhatsApp(mobile, message);
            });
        }

        // --- 3. SMTP Execution (Zoho) ---
        // UPDATED: Added optional ccEmail parameter
        private static void SendEmail(string toEmail, string subject, string body, string ccEmail = null)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(smtpFrom, "Flame-Ex ERP");
                mail.To.Add(toEmail);

                // --- NEW: Inject CC if provided by the calling method ---
                if (!string.IsNullOrEmpty(ccEmail))
                {
                    mail.CC.Add(ccEmail);
                }

                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient(smtpHost, smtpPort);
                smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                smtp.EnableSsl = smtpEnableSsl;

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                // In a production environment, you could log this to tbl_ProcessLogs
                // For now, it fails silently so the user transaction isn't interrupted
            }
        }

        // --- 4. MSG91 WhatsApp/SMS Execution ---
        private static void SendWhatsApp(string mobileNumber, string message)
        {
            try
            {
                // Clean the mobile number (MSG91 usually expects pure country code + number, e.g., 919876543210)
                string cleanMobile = mobileNumber.Replace("+", "").Replace(" ", "").Trim();
                string encodedMessage = Uri.EscapeDataString(message);

                // Construct the MSG91 API URL (Adjust routing/template parameters based on your specific MSG91 plan)
                string msg91Url = $"https://api.msg91.com/api/sendhttp.php?authkey={msg91AuthKey}&mobiles={cleanMobile}&message={encodedMessage}&route=4&sender={msg91IntegratedNumber}";

                using (WebClient client = new WebClient())
                {
                    client.DownloadString(msg91Url);
                }
            }
            catch (Exception ex)
            {
                // Fail silently to protect user experience
            }
        }
    }
}