using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NtoboaFund.Data.DTO_s;
using NtoboaFund.Data.Models;
using NtoboaFund.Helpers;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace NtoboaFund.Services
{
    public class PaymentService
    {

        public PaymentService(IOptions<AppSettings> appSettings)
        {
            AppSettings = appSettings.Value;
        }

        public AppSettings AppSettings { get; }

        public void MomoTransfer(ApplicationUser user, decimal _amount, string stakeType)
        {
            try
            {
                var data = new
                {
                    account_bank = user.MomoDetails.Network,
                    account_number = Operations.FormatGhanaianPhoneNumber(user.MomoDetails.Number),
                    amount = _amount,
                    seckey = AppSettings.FlatterWaveSettings.GetApiSecret(),
                    narration = "Ntoboa " + stakeType + " Winner",
                    currency = user.MomoDetails.Currency,
                    reference = user.Id + DateTime.Now.ToShortDateString() + DateTime.Now.ToShortTimeString(),
                    beneficiary_name = user.FirstName + " " + user.LastName
                };
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var responseMessage = client.PostAsJsonAsync("https://api.ravepay.co/v2/gpx/transfers/create", data).Result;
                //please make sure to change this to production url when you go live
                var responseStr = responseMessage.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<TransferResponse>(responseStr);
                if (response.status == "success")
                {


                }
            }
            catch (Exception ex)
            {

            }

        }
    }


}
