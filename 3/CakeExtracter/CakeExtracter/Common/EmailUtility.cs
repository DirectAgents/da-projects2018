using System;
using System.Net.Mail;

namespace CakeExtracter.Common
{
    public static class EmailUtility
    {
        public static void SendEmail(string from, string[] to, string[] cc, string subject, string body, bool isHTML)
        {
            var message = new MailMessage
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHTML,
                    From = new MailAddress(from),
                };
            Array.ForEach(to, c => message.To.Add(c));
            Array.ForEach(cc, c => message.CC.Add(c));
            var smtpClient = new SmtpClient
                {
                    Host = Host,
                    Port = Port,
                    Timeout = Timeout,
                    EnableSsl = EnableSsl,
                    Credentials = new System.Net.NetworkCredential(Login, Password)
                };
            smtpClient.Send(message);
        }

        public static string Login { get; set; }
        public static string Password { get; set; }
        public static string Host { get; set; }
        public static int Port { get; set; }
        public static int Timeout { get; set; }
        public static bool EnableSsl { get; set; }
    }
}
