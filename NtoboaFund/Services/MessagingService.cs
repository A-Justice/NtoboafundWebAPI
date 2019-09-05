using Microsoft.Extensions.Options;
using NtoboaFund.Helpers;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                var response =client.SendEmailAsync(msg);
            }
            catch(Exception ex)
            {
                Console.WriteLine("New User Registration");
                Console.WriteLine(ex.Message);
            }
            
        }
    }
}
