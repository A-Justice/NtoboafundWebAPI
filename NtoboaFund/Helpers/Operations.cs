using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace NtoboaFund.Helpers
{
    public static class Operations
    {

       


        public static string FormatGhanaianPhoneNumber(string phoneNumber)
        {
            if (phoneNumber.StartsWith("0") && phoneNumber.Length == 10)
            {
                return "233" + phoneNumber.Substring(1);
            }
            else if (!phoneNumber.StartsWith("0") && phoneNumber.Length == 9)
            {
                return "233" + phoneNumber;
            }

            return phoneNumber;
        }

    }
}


//public static void SendMail(string To, string MailSubject, string MailBody)
//{
//    string From = "info@ntoboafund.com";
//    string FromPassword = "u8v@Dlh!Ew%;";
//    try
//    {
//        MailMessage msg = new MailMessage();
//        msg.From = new MailAddress(From);
//        msg.BodyEncoding = Encoding.UTF8;
//        msg.To.Add(To);
//        msg.Subject = MailSubject;
//        msg.Body = MailBody;
//        msg.IsBodyHtml = true;
//        SmtpClient smtp = new SmtpClient("mail.ntoboafund.com", 587);

//        smtp.Credentials = new System.Net.NetworkCredential(From, FromPassword);
//        //smtp.Timeout = 60000;
//        smtp.EnableSsl = false;

//        // Sending the email
//        smtp.Send(msg);
//        // destroy the message after sent
//        msg.Dispose();
//    }
//    catch
//    {

//    }
//}