using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NtoboaFund.Helpers
{
    public static class Operations
    {
        public static void SendMail(string To, string MailSubject, string MailBody)
        {
            string From = "info@ntoboafund.com";
            string FromPassword = "u8v@Dlh!Ew%;";
            try
            {
                MailMessage msg = new MailMessage();
                msg.From = new MailAddress(From);
                msg.BodyEncoding = Encoding.UTF8;
                msg.To.Add(To);
                msg.Subject = MailSubject;
                msg.Body = MailBody;
                msg.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient("mail.ntoboafund.com", 587);

                smtp.Credentials = new System.Net.NetworkCredential(From, FromPassword);
                //smtp.Timeout = 60000;
                smtp.EnableSsl = false;

                // Sending the email
                smtp.Send(msg);
                // destroy the message after sent
                msg.Dispose();
            }
            catch
            {

            }
        }

    }
}
