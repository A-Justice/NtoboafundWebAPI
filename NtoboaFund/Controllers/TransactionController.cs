using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.DTO_s;
using NtoboaFund.Data.Models;
using NtoboaFund.Helpers;
using NtoboaFund.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NtoboaFund.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {

        private readonly AppSettings AppSettings;
        private NtoboaFundDbContext context;

        public IHubContext<StakersHub> StakersHub { get; }

        public TransactionController(IOptions<AppSettings> appSettings, NtoboaFundDbContext _context, IHubContext<StakersHub> stakersHub)
        {
            AppSettings = appSettings.Value;
            context = _context;
            StakersHub = stakersHub;
        }

        [HttpPost("verifyLuckymePayment/{txRef}")]
        public async Task<IActionResult> VerifyLuckymePayment(string txRef, [FromBody]LuckyMe LuckyMe)
        {
            LuckyMe.Date = DateTime.Now.ToLongDateString();
            LuckyMe.AmountToWin = LuckyMe.Amount * Settings.LuckymeStakeOdds;
            LuckyMe.Status = "pending";
            LuckyMe.User = await context.Users.FindAsync(LuckyMe.UserId);
            LuckyMe.TxRef = txRef;
            context.LuckyMes.Add(LuckyMe);

            string resultString = null;
            string errorString = null;
            try
            {
                if (VerifyPayment(txRef))
                    LuckyMe.Status = "paid";
                else
                    LuckyMe.Status = "pending";

                await context.SaveChangesAsync();

                if (LuckyMe.User != null && LuckyMe.Status.ToLower() == "paid") //send the currently added participant to all clients
                {
                    if (LuckyMe.Period.ToLower() == "daily")
                    {
                        await StakersHub.Clients.All.SendAsync("adddailyluckymeparticipant",
                        new LuckyMeParticipantDTO
                        {
                            Id = LuckyMe.Id,
                            UserId = LuckyMe.User.Id,
                            UserName = LuckyMe.User.FirstName + " " + LuckyMe.User.LastName,
                            AmountStaked = LuckyMe.Amount.ToString(),
                            AmountToWin = LuckyMe.AmountToWin.ToString(),
                            Status = LuckyMe.Status.ToLower()

                        });
                    }
                    else if (LuckyMe.Period.ToLower() == "weekly")
                    {
                        await StakersHub.Clients.All.SendAsync("addweeklyluckymeparticipant",
                           new LuckyMeParticipantDTO
                           {
                               Id = LuckyMe.Id,
                               UserId = LuckyMe.User.Id,
                               UserName = LuckyMe.User.FirstName + " " + LuckyMe.User.LastName,
                               AmountStaked = LuckyMe.Amount.ToString(),
                               AmountToWin = LuckyMe.AmountToWin.ToString(),
                               Status = LuckyMe.Status.ToLower()
                           });
                    }
                    else if (LuckyMe.Period.ToLower() == "monthly")
                    {
                        await StakersHub.Clients.All.SendAsync("addmonthlyluckymeparticipant",
                           new LuckyMeParticipantDTO
                           {
                               Id = LuckyMe.Id,
                               UserId = LuckyMe.User.Id,
                               UserName = LuckyMe.User.FirstName + " " + LuckyMe.User.LastName,
                               AmountStaked = LuckyMe.Amount.ToString(),
                               AmountToWin = LuckyMe.AmountToWin.ToString(),
                               Status = LuckyMe.Status.ToLower()
                           });
                    }

                }

                // resultString = GenerateHubtelUrl(LuckyMe.Id, LuckyMe.Amount, "luckyme");
            }
            catch (Exception ex)
            {
                errorString = ex.Message;
            }



            //  return Ok(hubtelresponse?.data?.checkoutUrl);
            return Ok(new { LuckyMe, resultString, errorString });
        }


        [HttpPost("verifyScholarshipPayment/{txRef}")]
        public async Task<IActionResult> VerifyScholarshipPayment(string txRef, [FromBody]Scholarship Scholarship)
        {


            Scholarship.Amount = Settings.ScholarshipStakeAmount;
            Scholarship.Date = DateTime.Now.ToLongDateString();
            Scholarship.AmountToWin = (Scholarship.Amount * Settings.ScholarshipStakeOdds);
            Scholarship.Status= "Pending";
            Scholarship.Period = "quaterly";
            Scholarship.TxRef = txRef;
            Scholarship.User = context.Users.Find(Scholarship.UserId);

            if (ModelState.IsValid)
            {
                //Scholarship.UserId = null;
                context.Scholarships.Add(Scholarship);
            }
            else
                BadRequest("Submitted Form is Invalid");

            string resultString = null;
            string errorString = null;

            try
            {
                if (VerifyPayment(txRef))
                    Scholarship.Status = "paid";
                else
                    Scholarship.Status = "pending";

                await context.SaveChangesAsync();
                //Find the current user associated with the scholarship


                //send the currently added participant to all clients
                if (Scholarship.User != null && Scholarship.Status.ToLower() == "paid")
                {
                    await StakersHub.Clients.All.SendAsync("addscholarshipparticipant",
                       new ScholarshipParticipantDTO
                       {
                           Id = Scholarship.Id,
                           UserId = Scholarship.User.Id,
                           UserName = Scholarship.User.FirstName + " " + Scholarship.User.LastName,
                           AmountStaked = Scholarship.Amount.ToString(),
                           AmountToWin = Scholarship.AmountToWin.ToString(),
                           Status = Scholarship.Status.ToLower()
                       });
                }
                else if (Scholarship.Status.ToLower() != "paid")
                {
                    errorString = "Payment Could not be verfied";
                }

            }
            catch (Exception ex)
            {
                errorString = ex.Message;
            }



            //  return Ok(hubtelresponse?.data?.checkoutUrl);
            return Ok(new { Scholarship, resultString, errorString });
        }


        [HttpPost("verifyBusinessPayment/{txRef}")]
        public async Task<IActionResult> VerifyBusinessPayment(string txRef, [FromBody]Business business)
        {
            business.Date = DateTime.Now.ToLongDateString();
            business.AmountToWin = (business.Amount * Settings.BusinessStakeOdds);
            business.Status = "Pending";
            business.Period = "monthly";
            business.TxRef = txRef;
            business.User = context.Users.Find(business.UserId);

            if (ModelState.IsValid)
            {
                // business.UserId = null;
                context.Businesses.Add(business);

            }
            else
                BadRequest("Submitted Form is Invalid");

            string resultString = null;
            string errorString = null;

            try
            {

                if (VerifyPayment(txRef))
                    business.Status = "paid";
                else
                    business.Status = "pending";

                await context.SaveChangesAsync();
                //Find the current user associated with the business

                if (business.User != null && business.Status.ToLower() == "paid") //send the currently added participant to all clients
                    await StakersHub.Clients.All.SendAsync("addbusinessparticipant",
                         new BusinessParticipantDTO
                         {
                             Id = business.Id,
                             UserId = business.User.Id,
                             UserName = business.User.FirstName + " " + business.User.LastName,
                             AmountStaked = business.Amount.ToString(),
                             AmountToWin = business.AmountToWin.ToString(),
                             Status = business.Status.ToLower()
                         });
                //resultString = GenerateHubtelUrl(business.Id, business.Amount, "business");
            }
            catch (Exception ex)
            {
                errorString = ex.Message;
            }



            //  return Ok(hubtelresponse?.data?.checkoutUrl);
            return Ok(new { business, resultString, errorString });
        }

        [HttpPost("addpayment")]
        public async Task<IActionResult> AddPayment([FromBody]Payment payment)
        {
            payment.DatePayed = DateTime.Now;
            context.Payments.Add(payment);
            await context.SaveChangesAsync();
            return Ok("Payment Made Successfully");
        }


        bool VerifyPayment(string txRef)
        {

            var data = new { txref = txRef, SECKEY = AppSettings.FlatterWaveSettings.GetApiSecret() };
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var responseMessage = client.PostAsJsonAsync("https://api.ravepay.co/flwv3-pug/getpaidx/api/v2/verify", data).Result;
            //please make sure to change this to production url when you go live
            var responseStr = responseMessage.Content.ReadAsStringAsync().Result;
            var response = JsonConvert.DeserializeObject<RaveResponse>(responseStr);
            if (response.data.status == "successful" && response.data.chargecode == "00")
            {

                return true;

            }

            return false;

        }

        public string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }


        string GenerateHubtelUrl(int id, decimal amount, string name)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://api.hubtel.com/v2/pos/onlinecheckout/items/initiate");
            request.PreAuthenticate = true;
            request.ContentType = "application/json";
            request.Method = "POST";

            //var authDetails = $"Basic {Base64Encode(AppSettings.RaveApiSettings.ApiKey + ":" + AppSettings.RaveApiSettings.ApiSecret)}";
            //request.Headers.Add("Authorization", authDetails);


            using (var streamwriter = new StreamWriter(request.GetRequestStream()))
            {
                //Get Algorithm to calculate amount to win

                string json = JsonConvert.SerializeObject(new
                {

                    items = new List<object>()
                    {
                        new
                        {
                            name = name,
                            quantity=1,
                            unitPrice = amount
                        }
                    },
                    totalAmount = amount,
                    description = "ntuboa",
                    callbackUrl = "https://ntoboafund.gear.host/transaction/hubtelcallback",
                    returnUrl = $"https://ntoboafund.herokuapp.com/{name}",
                    merchantBusinessLogoUrl = "http://ntoboafund.herokuapp.com/assets/images/ntlog.png",
                    merchantAccountNumber = "HM2706190002",
                    cancellationUrl = $"https://ntoboafund.herokuapp.com/{name}",
                    clientReference = name + id
                });

                streamwriter.Write(json);
            }

            RaveResponse raveResponse = null;
            string resultString = null;

            var response = (HttpWebResponse)request.GetResponse();

            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                resultString = streamReader.ReadToEnd();
                raveResponse = JsonConvert.DeserializeObject<RaveResponse>(resultString);

            }
            response.Close();


            return resultString;
        }


    }



    public class RaveResponse
    {
        public RaveResponseData data { get; set; }
    }

    public class RaveResponseData
    {
        public string txid { get; set; }
        public string txref { get; set; }

        public decimal amount { get; set; }

        public string currency { get; set; }

        //successful
        public string status { get; set; }

        public string chargecode { get; set; }
    }





}