using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NtoboaFund.Data;
using NtoboaFund.Data.DTO_s;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        /// <summary>
        /// Returns the Ghanaian Phone Number in InternationalSyntax
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
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
        /// Return the International Ghanaian phone Number without the initial plus symbol
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public static string FormatGhanaianPhoneNumberWp(string phoneNumber)
        {
            return FormatGhanaianPhoneNumber(phoneNumber).Substring(1);
        }
        /// <summary>
        /// Returns the The Phone Number without the Country Code
        /// </summary>
        /// <param name="phoneNumber">The phoneNumber to normalize</param>
        /// <returns></returns>
        public static string NormalizePhoneNumber(string phoneNumber)
        {
            try
            {
                if (phoneNumber == null)
                    return phoneNumber;

                if (phoneNumber.Length < 9)
                    return phoneNumber;

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
            catch
            {
                return phoneNumber;
            }

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

        /// <summary>
        /// Get
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public static string getSlydePayOption(string phoneNumber)
        {
            var phone = Misc.FormatGhanaianPhoneNumber(phoneNumber);
            var networkDeterminants = phone.Substring(5, 1);
            if (networkDeterminants == "4" || networkDeterminants == "5")
                return "MTN_MONEY";
            else if (networkDeterminants == "0")
                return "VODAFONE_CASH";
            else if (networkDeterminants == "6" || networkDeterminants == "7")
                return "AIRTEL_MONEY";

            return null;
        }
        public static string getReddePayOption(string phoneNumber)
        {
            var phone = Misc.FormatGhanaianPhoneNumber(phoneNumber);
            var networkDeterminants = phone.Substring(5, 1);
            if (networkDeterminants == "4" || networkDeterminants == "5")
                return "MTN";
            else if (networkDeterminants == "0")
                return "VODAFONE";
            else if (networkDeterminants == "6" || networkDeterminants == "7")
                return "AIRTELTIGO";

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
                sBuilder.AppendLine($"You have been added to the current LuckyMe {period} participants. Stay tuned");

            }
            else if (type == EntityTypes.Business)
            {
                sBuilder.AppendLine($"Thank you for your {amountStaked.ToString("0.##")} Cedis Business stake.");
                sBuilder.AppendLine($"Your Potential Returns is {(amountStaked * Constants.BusinessStakeOdds).ToString("0.##")} Cedis.");
                sBuilder.AppendLine($"A winner will be anounced at 6:00 pm on {DateTime.Now.EndOfMonth(18, 0, 0, 0).ToLongDateString()}.");
                sBuilder.AppendLine($"You have been added to the current business participants. Stay tuned");
            }
            else if (type == EntityTypes.Scholarship)
            {
                sBuilder.AppendLine($"Thank you for your {amountStaked.ToString("0.##")} Cedis Scholarship stake.");
                sBuilder.AppendLine($"Your Potential Returns is {(amountStaked * Constants.ScholarshipStakeOdds).ToString("0.##")} Cedis.");
                sBuilder.AppendLine($"A winner will be anounced at 6:00 pm on {DateTime.Now.NextQuater(18, 0, 0, 0).ToLongDateString()}.");
                sBuilder.AppendLine($"You have been added to the current scholarship participants. Stay tuned");
            }

            return sBuilder.ToString();
        }

        public static string GetDrawDate(EntityTypes type, string period)
        {
            if (type == EntityTypes.Luckyme)
            {

                if (period == "daily")
                {
                    return DateTime.Now.DailyStakeEndDate().ToLongDateString();
                }
                else if (period == "weekly")
                {
                    return DateTime.Now.EndOfWeek(18, 0, 0, 0).ToLongDateString();

                }
                else if (period == "monthly")
                {
                    return DateTime.Now.EndOfMonth(18, 0, 0, 0).ToLongDateString();

                }
            }
            else if (type == EntityTypes.Business)
            {
                return DateTime.Now.EndOfMonth(18, 0, 0, 0).ToLongDateString();
            }
            else if (type == EntityTypes.Scholarship)
            {
                return DateTime.Now.NextQuater(18, 0, 0, 0).ToLongDateString();
            }

            return null;
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

        public static JsonSerializerSettings getDefaultResolverJsonSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver()
            };
        }


        public static async Task<string> GenerateSlydePayToken(EntityTypes entityType, IStakeType stakeType, SlydePayApiSettingsDTO slydePayApiSettings)
        {

            try
            {
                SlydePayOrderItem item = new SlydePayOrderItem();
                item.ItemCode = stakeType.Id.ToString();
                item.ItemName = $"{entityType.ToString()} Investment";
                item.Quantity = 1;
                item.SubTotal = stakeType.Amount;
                item.UnitPrice = stakeType.Amount;

                SlydePayOrderItem[] items = { item };

                SlydePayCreateInvoiceRequest request = new SlydePayCreateInvoiceRequest
                {
                    EmailOrMobileNumber = slydePayApiSettings.MerchantEmail,
                    MerchantKey = slydePayApiSettings.Merchantkey,
                    Amount = stakeType.Amount,
                    OrderCode = stakeType.TxRef,
                    OrderItems = items
                };

                //PayliveConnector connector = new PayliveConnector(slydePayApiSettings.ApiVersion, slydePayApiSettings.MerchantEmail, slydePayApiSettings.Merchantkey, slydePayApiSettings.ServiceType, slydePayApiSettings.IntegrationMode);

                var httpClient = new HttpClient();


                var data = JsonConvert.SerializeObject(request, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                var stringContent = new StringContent(data);
                stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var responseMessage = await httpClient.PostAsync("https://app.slydepay.com.gh/api/merchant/invoice/create", stringContent);

                var contentString = await responseMessage.Content.ReadAsStringAsync();
                SlydePayCreateInvoiceResponse response = JsonConvert.DeserializeObject<SlydePayCreateInvoiceResponse>(contentString);

                return response.result.payToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GenerateSlydePayToken {ex.Message}");
                return null;
            }

        }

        #region SlydePayMethods

        public static async Task<string> GenerateAndSendSlydePayMomoInvoice(EntityTypes entityType, IStakeType stakeType, SlydePayApiSettingsDTO slydePayApiSettings, string MomoNumber)
        {
            if (string.IsNullOrEmpty(MomoNumber))
                return null;

            return await GenerateAndSendSlydePayInvoice(entityType, stakeType, slydePayApiSettings, getSlydePayOption(MomoNumber), MomoNumber);
        }
        public static async Task<string> GenerateAndSendSlydePayCardInvoice(EntityTypes entityType, IStakeType stakeType, SlydePayApiSettingsDTO slydePayApiSettings, string email)
        {
            return await GenerateAndSendSlydePayInvoice(entityType, stakeType, slydePayApiSettings, "VISA", null, email);

        }

        public static async Task<string> GenerateAndSendSlydePayAnkasaInvoice(EntityTypes entityType, IStakeType stakeType, SlydePayApiSettingsDTO slydePayApiSettings)
        {
            return await GenerateAndSendSlydePayInvoice(entityType, stakeType, slydePayApiSettings, "SLYDEPAY");

        }

        private static async Task<string> GenerateAndSendSlydePayInvoice(EntityTypes entityType, IStakeType stakeType, SlydePayApiSettingsDTO slydePayApiSettings, string PayOption, string MomoNumber = null, string email = null)
        {
            string customerMobileNumber = MomoNumber ?? stakeType.User.PhoneNumber;
            string customerEmail = email == "default" ? stakeType.User.Email : email;

            try
            {
                SlydePayOrderItem item = new SlydePayOrderItem();
                item.ItemCode = stakeType.Id.ToString();
                item.ItemName = $"{entityType.ToString()} Investment";
                item.Quantity = 1;
                item.SubTotal = stakeType.Amount;
                item.UnitPrice = stakeType.Amount;

                SlydePayOrderItem[] items = { item };

                SlydePayCreateInvoiceRequest request = new SlydePayCreateInvoiceRequest
                {
                    EmailOrMobileNumber = slydePayApiSettings.MerchantEmail,
                    MerchantKey = slydePayApiSettings.Merchantkey,
                    Amount = stakeType.Amount,
                    OrderCode = stakeType.TxRef,
                    OrderItems = items,
                    SendInvoice = true,
                    PayOption = PayOption,
                    CustomerName = stakeType.User.FirstName + " " + stakeType.User.FirstName,
                    CustomerEmail = customerEmail,
                    CustomerMobileNumber = customerMobileNumber
                };

                var httpClient = new HttpClient();


                var data = JsonConvert.SerializeObject(request, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                var stringContent = new StringContent(data);
                stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var responseMessage = await httpClient.PostAsync("https://app.slydepay.com.gh/api/merchant/invoice/create", stringContent);

                var contentString = await responseMessage.Content.ReadAsStringAsync();
                SlydePayCreateInvoiceResponse response = JsonConvert.DeserializeObject<SlydePayCreateInvoiceResponse>(contentString);

                return response.result.payToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GenerateSlydePayToken {ex.Message}");
                return null;
            }

        }

        public static async Task<SlydePayPaymentStatusResponse> ConfirmSlydePayTransaction(EntityTypes entityType, IStakeType stakeType, SlydePayApiSettingsDTO slydePayApiSettings)
        {
            try
            {
                SlydePayCreateInvoiceRequest request = new SlydePayCreateInvoiceRequest
                {
                    EmailOrMobileNumber = slydePayApiSettings.MerchantEmail,
                    MerchantKey = slydePayApiSettings.Merchantkey,
                    OrderCode = stakeType.TxRef
                };

                var httpClient = new HttpClient();


                var data = JsonConvert.SerializeObject(request, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                var stringContent = new StringContent(data);
                stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var responseMessage = await httpClient.PostAsync("https://app.slydepay.com.gh/api/merchant/transaction/confirm", stringContent);

                var contentString = await responseMessage.Content.ReadAsStringAsync();
                SlydePayPaymentStatusResponse response = JsonConvert.DeserializeObject<SlydePayPaymentStatusResponse>(contentString);

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Confirm Slydepay Transaction {ex.Message}");
                return null;
            }

        }


        public static async Task<SlydePayPaymentStatusResponse> CancelSlydePayTransaction(EntityTypes entityType, IStakeType stakeType, SlydePayApiSettingsDTO slydePayApiSettings)
        {
            try
            {

                SlydePayCreateInvoiceRequest request = new SlydePayCreateInvoiceRequest
                {
                    EmailOrMobileNumber = slydePayApiSettings.MerchantEmail,
                    MerchantKey = slydePayApiSettings.Merchantkey,
                    OrderCode = stakeType.TxRef
                };

                var httpClient = new HttpClient();


                var data = JsonConvert.SerializeObject(request, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                var stringContent = new StringContent(data);
                stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var responseMessage = await httpClient.PostAsync("https://app.slydepay.com.gh/api/merchant/transaction/cancel", stringContent);

                var contentString = await responseMessage.Content.ReadAsStringAsync();
                SlydePayPaymentStatusResponse response = JsonConvert.DeserializeObject<SlydePayPaymentStatusResponse>(contentString);

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GenerateSlydePayToken {ex.Message}");
                return null;
            }

        }

        #endregion

        #region ReddeMethods
        /// <summary>
        /// Sends the momo invoice to users phone
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="stakeType"></param>
        /// <param name="redSettings"></param>
        /// <param name="momoAndVoucher"></param>
        /// <returns>Transaction Id</returns>
        public static async Task<int?> GenerateAndSendReddeMomoInvoice(EntityTypes entityType, IStakeType stakeType, ReddeSettingsDTO redSettings, string momoAndVoucher)
        {
            var mandV = momoAndVoucher.Split('*');
            var MomoNumber = mandV[0];
            var voucher = mandV[1];
            if (string.IsNullOrEmpty(MomoNumber))
                return null;

            return await GenerateAndSendReddeInvoice(entityType, stakeType, redSettings, getReddePayOption(MomoNumber), MomoNumber, voucher);
        }

        /// <summary>
        /// Generate the token used to redirect the user to pay
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="stakeType"></param>
        /// <param name="reddeSettings"></param>
        /// <param name="MomoNumber"></param>
        /// <returns>The generated token and checkout Tranaction id</returns>
        public static async Task<(string token, int? checkoutransId)> GenerateReddeToken(EntityTypes entityType, IStakeType stakeType, ReddeSettingsDTO reddeSettings, string MomoNumber = null)
        {
            string customerMobileNumber = MomoNumber ?? stakeType.User.PhoneNumber;
            //string customerEmail = email == "default" ? stakeType.User.Email : email;
            try
            {

                ReddeCheckoutRequest request = new ReddeCheckoutRequest
                {
                    Amount = stakeType.Amount,
                    Apikey = reddeSettings.ApiKey,
                    Appid = reddeSettings.AppId,
                    Description = entityType.GetType().Name + " Investment",
                    Failurecallback = "https://ntoboasuccess.com",
                    Logolink = "https://ntoboafund.com/assets/images/ntlog.png",
                    Merchantname = reddeSettings.NickName,
                    Clienttransid = stakeType.Id,
                    Successcallback = "https://ntoboafailure.com",
                };

                var httpClient = new HttpClient();


                var data = JsonConvert.SerializeObject(request, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                var stringContent = new StringContent(data);
                httpClient.DefaultRequestHeaders.Add("apikey", reddeSettings.ApiKey);
                stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var responseMessage = await httpClient.PostAsync("https://api.reddeonline.com/v1/checkout/", stringContent);

                var contentString = await responseMessage.Content.ReadAsStringAsync();
                ReddeCheckoutResponse response = JsonConvert.DeserializeObject<ReddeCheckoutResponse>(contentString);

                return (response.Responsetoken, response.Checkouttransid);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GenerateReddeToken {ex.Message}");
                return (null, null);
            }

        }

        private static async Task<int?> GenerateAndSendReddeInvoice(EntityTypes entityType, IStakeType stakeType, ReddeSettingsDTO reddeSettings, string PayOption, string MomoNumber = null, string voucher = null)
        {
            string customerMobileNumber = MomoNumber ?? stakeType.User.PhoneNumber;
            //string customerEmail = email == "default" ? stakeType.User.Email : email;
            try
            {

                ReddeRequest request = new ReddeRequest
                {
                    Amount = stakeType.Amount,
                    Appid = reddeSettings.AppId,
                    Clientreference = stakeType.TxRef,
                    Clienttransid = stakeType.TxRef,
                    Description = entityType.ToString() + " Investment",
                    Nickname = reddeSettings.NickName,
                    Paymentoption = PayOption,
                    Vouchercode = voucher,
                    Walletnumber = FormatGhanaianPhoneNumberWp(customerMobileNumber)
                };

                var httpClient = new HttpClient();


                var data = JsonConvert.SerializeObject(request, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                var stringContent = new StringContent(data);
                httpClient.DefaultRequestHeaders.Add("apikey", reddeSettings.ApiKey);
                stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var responseMessage = await httpClient.PostAsync("https://api.reddeonline.com/v1/receive", stringContent);

                var contentString = await responseMessage.Content.ReadAsStringAsync();
                ReddeInitialResponse response = JsonConvert.DeserializeObject<ReddeInitialResponse>(contentString);

                return response.Transactionid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GenerateReddeToken {ex.Message}");
                return null;
            }

        }


        #endregion

        public static string getTxRef(string phoneNumber)
        {
            var match = Regex.Match(phoneNumber, @"^(\w{2}).*(\w{2})$");

            var userCode = match.Groups[1].ToString() + match.Groups[2].ToString();
            var timeStamp = DateTime.Now.TimeOfDay.ToString();
            return $"inv.{ userCode}.{timeStamp}";
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