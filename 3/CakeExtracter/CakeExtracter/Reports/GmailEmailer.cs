using ClientPortal.Data.Contexts;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;

// TODO: separate project
namespace CakeExtracter.Reports
{
    public class GmailEmailer
    {
        public NetworkCredential Credential { get; set; }

        public GmailEmailer(NetworkCredential credential)
        {
            this.Credential = credential;
        }

        private void SendMailMessage(MailMessage message)
        {
            var client = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                Timeout = 50000,
                EnableSsl = true,
                Credentials = this.Credential
            };
            client.Send(message);
        }

        public void GenerateAndSendSimpleReport(SimpleReport simpleReport, IReport iReport, string overrideEmail = null)
        {
            string email = overrideEmail ?? simpleReport.Email;
            string emailCC = (overrideEmail == null ? simpleReport.EmailCC : null);

            var htmlView = iReport.GenerateView();
            Attachment attachment = null;
            if (simpleReport.IncludeAttachment)
            {
                attachment = iReport.GenerateSpreadsheetAttachment();
            }
            SendEmail(
                this.Credential.UserName,
                new[] { email },
                emailCC == null ? null : new[] { emailCC },
                iReport.Subject,
                new[] { htmlView },
                attachment);
        }

        public void SendEmail(string fromAddress, string[] toAddresses, string[] ccAddresses, string subject, string body, bool isHTML, Attachment attachment = null)
        {
            var message = CreateMailMessage(fromAddress, toAddresses, ccAddresses, subject, attachment);
            message.Body = body;
            message.IsBodyHtml = isHTML;

            SendMailMessage(message);
        }
        public void SendEmail(string fromAddress, string[] toAddresses, string[] ccAddresses, string subject, AlternateView[] alternateViews, Attachment attachment = null)
        {
            var message = CreateMailMessage(fromAddress, toAddresses, ccAddresses, subject, attachment);
            foreach (var alternateView in alternateViews)
            {
                message.AlternateViews.Add(alternateView);
            }
            SendMailMessage(message);
        }


        private MailMessage CreateMailMessage(string fromAddress, string[] toAddresses, string[] ccAddresses, string subject, Attachment attachment)
        {
            MailMessage message = new MailMessage
            {
                Subject = subject,
                From = new MailAddress(fromAddress),
            };

            foreach (var item in toAddresses.SelectMany(c => c.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)))
            {
                message.To.Add(item.Trim());
            }
            if (ccAddresses != null)
            {
                foreach (var ccAddress in ccAddresses.SelectMany(c => c.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)))
                {
                    message.CC.Add(ccAddress);
                }
            }
            if (attachment != null)
                message.Attachments.Add(attachment);

            return message;
        }

    }
}
