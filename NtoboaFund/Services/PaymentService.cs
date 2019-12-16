
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NtoboaFund.Data;
using NtoboaFund.Data.DBContext;
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
        NtoboaFundDbContext dbContext;
        public PaymentService(IOptions<AppSettings> appSettings, NtoboaFundDbContext _dbContext)
        {
            AppSettings = appSettings.Value;
            dbContext = _dbContext;
        }

        public AppSettings AppSettings { get; }

        public async void MomoTransfer(IStakeType stakeType)
        {
            if (Constants.PaymentGateway == PaymentGateway.flutterwave)
            {
                try
                {
                    var data = new
                    {
                        account_bank = stakeType.User.MomoDetails.Network,
                        account_number = Misc.FormatGhanaianPhoneNumberWp(stakeType.User.MomoDetails.Number),
                        amount = stakeType.AmountToWin,
                        seckey = AppSettings.FlatterWaveSettings.GetApiSecret(),
                        narration = "Ntoboa " + stakeType + " Winner",
                        currency = stakeType.User.MomoDetails.Currency,
                        reference = stakeType.User.Id + DateTime.Now.ToShortDateString() + DateTime.Now.ToShortTimeString() + "_PMCK",
                        beneficiary_name = stakeType.User.FirstName + " " + stakeType.User.LastName,
                        callback_url = "https://32d4015e.ngrok.io/transaction/momohook"
                    };
                    var payload = JsonConvert.SerializeObject(data, Misc.getDefaultResolverJsonSettings());
                    var stringContent = new StringContent(payload);
                    stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var httpClient = new HttpClient();
                    HttpResponseMessage response = await httpClient.PostAsync("https://api.ravepay.co/v2/gpx/transfers/create", stringContent);

                    var responseStr = response.Content.ReadAsStringAsync().Result;
                    var responseObject = JsonConvert.DeserializeObject<TransferResponse>(responseStr);
                    if (responseObject.status == "success")
                    {
                        //If transfer is successfull add a new payment
                        var payment = new Payment
                        {
                            // Reference = responseObject.data.reference,

                        };

                    }
                    else
                    {

                    }
                }
                catch (Exception ex)
                {

                }
            }
            else if (Constants.PaymentGateway == PaymentGateway.redde)
            {
                try
                {
                    var txRef = Misc.getTxRef(stakeType.User.MomoDetails.Number);
                    ReddeRequest request = new ReddeRequest
                    {
                        Amount = stakeType.AmountToWin,
                        Appid = AppSettings.ReddeSettings.AppId,
                        Clientreference = txRef,
                        Clienttransid = txRef,
                        Description = "",
                        Nickname = AppSettings.ReddeSettings.NickName,
                        Paymentoption = Misc.getReddePayOption(stakeType.User.MomoDetails.Number),
                        Walletnumber = Misc.FormatGhanaianPhoneNumberWp(stakeType.User.MomoDetails.Number)
                    };

                    var httpClient = new HttpClient();

                    var data = JsonConvert.SerializeObject(request, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                    var stringContent = new StringContent(data);
                    httpClient.DefaultRequestHeaders.Add("apikey", AppSettings.ReddeSettings.ApiKey);
                    stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var responseMessage = await httpClient.PostAsync("https://api.reddeonline.com/v1/cashout", stringContent);

                    var contentString = await responseMessage.Content.ReadAsStringAsync();
                    ReddeInitialResponse response = JsonConvert.DeserializeObject<ReddeInitialResponse>(contentString);

                    if (response.Status == "OK")
                    {
                        dbContext.Payments.Add(new Payment
                        {
                            ItemPayedFor = stakeType.GetType().Name + " winning",
                            ItemPayedForId = stakeType.Id,
                            DatePayed = DateTime.Now
                        });
                        await dbContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"MomoTransfer {ex.Message}");
                }

            }


        }
    }


}
