using Microsoft.Extensions.Options;
using NtoboaFund.Helpers;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace NtoboaFund.Services
{
    public class MessagingService
    {

        public MessagingService(IOptions<AppSettings> appSettings)
        {
            AppSetting = appSettings.Value;
        }

        public AppSettings AppSetting { get; }

        public async Task SendMail(string userName, string To, string MailSubject, string MailBody)
        {
            await SendSendGridEmail(userName, To, MailSubject, MailBody);
        }

        public async Task SendTwilioSms()
        {
            const string accountSid = "AC5a1532337b7f246e808adaa1addffa60";
            const string authToken = "ac0a00e27cad3fcffc2a745d9f18f258";

            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                body: "Join Earth's mightiest heroes. Like Kevin Bacon.",
                from: new Twilio.Types.PhoneNumber("+15017122661"),
                to: new Twilio.Types.PhoneNumber("+15558675310")
            );

            Console.WriteLine(message.Sid);
        }

        public async Task SendSms(string phoneNumber, string message, string senderId = "Ntoboafund")
        {
            //return;

            if (string.IsNullOrEmpty(phoneNumber))
                return;

            await SendAdroitSms(phoneNumber, message, senderId);
        }

        public async Task<int> SendAdroitSms(string phoneNumber, string message, string senderId)
        {
            message = message.Replace("#", "%23");
            phoneNumber = Misc.FormatGhanaianPhoneNumber(phoneNumber);
            //string url = $"https://apps.mnotify.net/smsapi?key={AppSetting.MNotifySettings.ApiKey}&to={phoneNumber}&msg={message}&sender_id={senderId}";

            string url = $"http://sms.adroit360gh.com/sms/api?action=send-sms&api_key=YWRtaW46YWRtaW4ucGFzc3dvcmQ=&to={phoneNumber}&from={senderId}&sms={message}";

            var httpClient = new HttpClient();

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);

                //string m =  await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

   

        public async Task SendSendGridEmail(string userName, string To, string MailSubject, string MailBody) {
            if (string.IsNullOrEmpty(To))
                return;

            try
            {
                string From = "info@ntoboafund.com";
                string FromPassword = "u8v@Dlh!Ew%;";
                var client = new SendGridClient(AppSetting.SendGridSettings.ApiKey);
                var from = new EmailAddress(From, "NtoboaFund");
                var subject = MailSubject;
                var to = new EmailAddress(To, userName);
                var plainTextContent = MailBody;
                var htmlContent = MailBody;
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("New User Registration");
                Console.WriteLine(ex.Message);
            }
        }

        public async Task SendMailChimpEmail(string userName, string To, string MailSubject, string MailBody)
        {
            if (string.IsNullOrEmpty(To))
                return;

            try
            {
               
                string url = $"https://mandrillapp.com/api/1.0/messages/send";


                var httpClient = new HttpClient();

                try
                {
                    HttpResponseMessage response = httpClient.PostAsJsonAsync(url,new {

                            key = "",
                            message = new
                            {
                                html = "",
                                subject = "",
                                from_email = "info@ntoboafund.com",
                                from_name = "NTOBOAFUND",
                                to = new Object[] { new { email = To } },
                                async = false
                            }

                }).Result;

                    string m = response.Content.ReadAsStringAsync().Result;
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("New User Registration");
                Console.WriteLine(ex.Message);
            }
        }

        public void SendMNotifySms(string phoneNumber, string message, string senderId)
        {
            phoneNumber = Misc.FormatGhanaianPhoneNumber(phoneNumber);
            string url = $"https://apps.mnotify.net/smsapi?key={AppSetting.MNotifySettings.ApiKey}&to={phoneNumber}&msg={message}&sender_id={senderId}";


            var httpClient = new HttpClient();

            try
            {
                HttpResponseMessage response = httpClient.GetAsync(url).Result;

                string m = response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {

            }
        }
    }
}
