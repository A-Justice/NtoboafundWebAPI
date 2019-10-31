using System;

namespace NtoboaFund.Helpers
{
    public static class Misc
    {

        public static string GetRegisteredUserSmsMessage()
        {
            return "Welcome to Ntoboafund, your registration was successful.";
        }

        public static string GetStakedUserSmsMessage(EntityTypes entityType, Period? period, decimal amount)
        {

            string drawDate = null;
            switch (period)
            {
                case Period.Daily:
                    drawDate = (getDateString(DateTime.Now.DailyStakeEndDate()));
                    break;
                case Period.Monthly:
                    drawDate = (getDateString(DateTime.Now.EndOfWeek(18, 0, 0, 0)));
                    break;
                case Period.Weekly:
                    drawDate = (getDateString(DateTime.Now.EndOfMonth(18, 0, 0, 0)));
                    break;
                case Period.Quaterly:
                    drawDate = (getDateString(DateTime.Now.NextQuater(18, 0, 0, 0)));
                    break;
                default:
                    break;
            }
            return $"You have successfully made a {period.ToString()} {entityType.ToString()} " +
                $"ntoboa of {amount.ToString("0.##")}, your draw will happen on {drawDate}." +
                $" Stay tuned.";
        }


        public static string FormatPhoneNumber(string phoneNumber)
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


        static string getDateString(DateTime date)
        {
            return date.ToLongDateString();
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