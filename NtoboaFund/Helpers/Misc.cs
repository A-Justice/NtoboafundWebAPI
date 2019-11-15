using System;
using System.Text;

namespace NtoboaFund.Helpers
{
    public static class Misc
    {

        public static string GetRegisteredUserSmsMessage()
        {
            return "Welcome to Ntoboafund, your registration was successful.You can dial *714*50# to invest on any mobile phone";
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
                $"ntoboa of {amount.ToString("0.##")} cedi(s), your draw will happen on {drawDate}." +
                $" Stay tuned.";
        }


        public static string FormatGhanaianPhoneNumber(string phoneNumber)
        {
            if (phoneNumber.StartsWith("0") && phoneNumber.Length == 10)
            {
                return "+233" + phoneNumber.Substring(1);
            }
            else if (!phoneNumber.StartsWith("0") && phoneNumber.Length == 9)
            {
                return "+233" + phoneNumber;
            }
            else if (phoneNumber.StartsWith("233") && phoneNumber.Length == 12)
            {
                return "+" + phoneNumber;
            }

            return phoneNumber;
        }

        public static bool IsCorrectGhanaianNumber(string phoneNumber)
        {
            if (phoneNumber.StartsWith("0") && phoneNumber.Length == 10)
            {
                return true;
            }
            else if (phoneNumber.StartsWith("+233") && phoneNumber.Length == 13)
            {
                return true;
            }
            else if (phoneNumber.StartsWith("00233") && phoneNumber.Length == 14)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the phoneNumber in a specific format..
        /// </summary>
        /// <param name="phoneNumber">The phoneNumber to normalize</param>
        /// <returns></returns>
        public static string NormalizePhoneNumber(string phoneNumber)
        {
            if (phoneNumber == null)
                return null;

            if (phoneNumber.StartsWith("0") && phoneNumber.Length == 10)
            {
                return phoneNumber.Substring(1);
            }
            else if (!phoneNumber.StartsWith("0") && phoneNumber.Length == 9)
            {
                return phoneNumber;
            }
            else if (phoneNumber.StartsWith("233") && phoneNumber.Length == 12)
            {
                return phoneNumber.Substring(3);
            }

            return phoneNumber.Substring(4);
        }

        public static string getNetwork(string phoneNumber)
        {
            var phone = Misc.FormatGhanaianPhoneNumber(phoneNumber);
            var networkDeterminants = phone.Substring(5, 1);
            if (networkDeterminants == "4" || networkDeterminants == "5")
                return "MTN";
            else if (networkDeterminants == "0")
                return "VODAFONE";
            else if (networkDeterminants == "6" || networkDeterminants == "7")
                return "TIGO";

            return null;
        }


        static string getDateString(DateTime date)
        {
            return date.ToLongDateString();
        }
        public static string GetDrawMessage(string type, string amount, string period, string momoNumber = null)
        {
            var amountStaked = Convert.ToDecimal(amount);
            switch (type)
            {
                case "lkm":
                    return GetDrawMessage(EntityTypes.Luckyme, amountStaked, period, momoNumber);
                case "bus":
                    return GetDrawMessage(EntityTypes.Luckyme, amountStaked, period, momoNumber);
                case "sch":
                    return GetDrawMessage(EntityTypes.Luckyme, amountStaked, period, momoNumber);
                default:
                    return "";
            }
        }

        public static string GetDrawMessage(EntityTypes type, decimal amountStaked, string period, string momoNumber = null)
        {
            var sBuilder = new StringBuilder();

            //double amountStaked = Convert.ToDouble(amount);


            var c = amountStaked > 1 ? "Cedis" : "Cedi";

            if (type == EntityTypes.Luckyme)
            {

                sBuilder.AppendLine($"Thank you for your {amountStaked.ToString("0.##")} {c} {period} lucky me stake.");
                sBuilder.AppendLine($"Your Potential Returns is {(amountStaked * Constants.LuckymeStakeOdds).ToString("0.##")} Cedis.");
                if (period == "daily")
                {
                    sBuilder.AppendLine($"A winner will be anounced at 6:00 pm on {DateTime.Now.DailyStakeEndDate().ToLongDateString()}.");
                }
                else if (period == "weekly")
                {
                    sBuilder.AppendLine($"A winner will be anounced at 6:00 pm on {DateTime.Now.EndOfWeek(18, 0, 0, 0).ToLongDateString()}.");

                }
                else if (period == "monthly")
                {
                    sBuilder.AppendLine($"A winner will be anounced at 6:00 pm on {DateTime.Now.EndOfMonth(18, 0, 0, 0).ToLongDateString()}.");

                }
                sBuilder.AppendLine($"You've been added to the current LuckyMe {period} participants. Stay tuned");

            }
            else if (type == EntityTypes.Business)
            {
                sBuilder.AppendLine($"Thank you for your {amountStaked.ToString("0.##")} Cedis Business stake.");
                sBuilder.AppendLine($"Your Potential Return is {(amountStaked * Constants.BusinessStakeOdds).ToString("0.##")} Cedis.");
                sBuilder.AppendLine($"A winner will be anounced at 6:00 pm on {DateTime.Now.EndOfMonth(18, 0, 0, 0).ToLongDateString()}.");
                sBuilder.AppendLine($"You've been added to the current business participants. Stay tuned");
            }
            else if (type == EntityTypes.Scholarship)
            {
                sBuilder.AppendLine($"Thank you for your {amountStaked.ToString("0.##")} Cedis Scholarship stake.");
                sBuilder.AppendLine($"Your Potential Return is {(amountStaked * Constants.ScholarshipStakeOdds).ToString("0.##")} Cedis.");
                sBuilder.AppendLine($"A winner will be anounced at 6:00 pm on {DateTime.Now.NextQuater(18, 0, 0, 0).ToLongDateString()}.");
                sBuilder.AppendLine($"You've been added to the current scholarship participants. Stay tuned");
            }

            return sBuilder.ToString();
        }

        public static string GetUssdPreStakeMessage(string type, string amount, string period)
        {
            var sBuilder = new StringBuilder();
            var amountStaked = Convert.ToDecimal(amount);
            var c = amountStaked > 1 ? "Cedis" : "Cedi";
            if (type == "lkm")
            {
                sBuilder.AppendLine($"NtoboaType : LuckyMe");
                sBuilder.AppendLine($"InvestedAmount : {amount} {c}");
                sBuilder.AppendLine($"PotentialReturns : {amountStaked * Constants.LuckymeStakeOdds} Cedis");
                if (period == "daily")
                {
                    sBuilder.AppendLine($"DrawDate {DateTime.Now.DailyStakeEndDate().ToLongDateString()}.");
                }
                else if (period == "weekly")
                {
                    sBuilder.AppendLine($"DrawDate {DateTime.Now.EndOfWeek(18, 0, 0, 0).ToLongDateString()}.");

                }
                else if (period == "monthly")
                {
                    sBuilder.AppendLine($"DrawDate {DateTime.Now.EndOfMonth(18, 0, 0, 0).ToLongDateString()}.");

                }
            }
            if (type == "bus")
            {
                sBuilder.AppendLine($"NtoboaType : Business");
                sBuilder.AppendLine($"InvestedAmount : {amount} {c}");
                sBuilder.AppendLine($"PotentialReturns : {amountStaked * Constants.BusinessStakeOdds} Cedis");
                sBuilder.AppendLine($"DrawDate {DateTime.Now.EndOfMonth(18, 0, 0, 0).ToLongDateString()}.");

            }
            if (type == "sch")
            {
                sBuilder.AppendLine($"NtoboaType : Scholarship");
                sBuilder.AppendLine($"InvestedAmount : {amount} {c}");
                sBuilder.AppendLine($"PotentialReturns : {amountStaked * Constants.ScholarshipStakeOdds} Cedis");
                sBuilder.AppendLine($"DrawDate {DateTime.Now.NextQuater(18, 0, 0, 0).ToLongDateString()}.");
            }

            return sBuilder.ToString();
        }

        public static string GetUssdPreStakeMessageForScholarship(string amount, string Institution, string Program, string StudentId, string PlayerType)
        {
            var sBuilder = new StringBuilder();

            sBuilder.AppendLine($"Scholarship");
            sBuilder.AppendLine($"{Institution}");
            sBuilder.AppendLine($"{Program}");
            sBuilder.AppendLine($"{StudentId}");
            sBuilder.AppendLine($"{PlayerType}");
            sBuilder.AppendLine($"{amount} Cedis");
            sBuilder.AppendLine($"Returns : {Convert.ToDecimal(amount) * Constants.ScholarshipStakeOdds} Cedis");
            sBuilder.AppendLine($"DrawDate : {DateTime.Now.NextQuater(18, 0, 0, 0).ToLongDateString()}.");

            return sBuilder.ToString();
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