using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace QLTL.Helpers
{
    public class MailHelper
    {
        private static string smtpServer = "smtp.mailtrap.io";
        private static int smtpPort = 587;
        private static string smtpUser = "e90d74ac5af6de"; // lấy trong Mailtrap
        private static string smtpPass = "87a02e3eb1bce9"; // lấy trong Mailtrap
        private static string fromEmail = "no-reply@yourapp.com";

        public static async Task SendMailAsync(string toEmail, string subject, string body)
        {
            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.EnableSsl = true;

                var mail = new MailMessage(fromEmail, toEmail, subject, body);
                mail.IsBodyHtml = true;

                await client.SendMailAsync(mail);
            }
        }
    }
}
