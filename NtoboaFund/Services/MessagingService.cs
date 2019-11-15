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

        public async Task SendSms(string phoneNumber, string message)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return;

            SendMNotifySms(phoneNumber, message);
        }

        public void SendMNotifySms(string phoneNumber, string message)
        {
            phoneNumber = Misc.FormatGhanaianPhoneNumber(phoneNumber);
            string url = $"https://apps.mnotify.net/smsapi?key={AppSetting.MNotifySettings.ApiKey}&to={phoneNumber}&msg={message}&sender_id=NtoboaFund";


            var httpClient = new HttpClient();

            try
            {
                HttpResponseMessage response = httpClient.GetAsync(url).Result;

            }
            catch (Exception ex)
            {

            }
        }
    }
}
