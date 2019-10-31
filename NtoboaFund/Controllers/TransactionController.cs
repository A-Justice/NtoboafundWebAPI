using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.DTO_s;
using NtoboaFund.Data.Models;
using NtoboaFund.Helpers;
using NtoboaFund.Services;
using NtoboaFund.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private NtoboaFundDbContext dbContext;
        public MessagingService MessagingService { get; set; }

        public IHubContext<StakersHub> StakersHub { get; }

        public DummyService DummyService { get; set; }

        public StakersHub DataHub { get; set; }

        public TransactionController(IOptions<AppSettings> appSettings,
            NtoboaFundDbContext _context, DummyService dummyService, StakersHub dataHub,
            IHubContext<StakersHub> stakersHub, MessagingService messagingService)
        {
            AppSettings = appSettings.Value;
            dbContext = _context;
            StakersHub = stakersHub;
            MessagingService = messagingService;
            DataHub = dataHub;
            DataHub.Groups = StakersHub.Groups;

        }


        [HttpPost("verifyLuckymePayment/{txRef}")]
        public async Task<IActionResult> VerifyLuckymePayment(string txRef /*, [FromBody]LuckyMe luckyMe*/)
        {
            var luckyMe = dbContext.LuckyMes.Where(i => i.TxRef == txRef && i.Status.ToLower() == "pending").Include("User").FirstOrDefault();

            luckyMe.User.Points += luckyMe.Amount * Constants.PointConstant;

            if (luckyMe == null)
            {
                return BadRequest(new { errorString = "LuckyMe stake was not found" });
            }

            string resultString = null;
            string errorString = null;
            try
            {
                if (VerifyPayment(txRef))
                    luckyMe.Status = "paid";
                else
                    luckyMe.Status = "pending";

                await dbContext.SaveChangesAsync();

                if (luckyMe.User != null && luckyMe.Status.ToLower() == "paid") //send the currently added participant to all clients
                {
                    if (luckyMe.Period.ToLower() == "daily")
                    {
                        //Send as sms to the user
                        await MessagingService.SendSms(luckyMe.User.PhoneNumber, Misc.GetStakedUserSmsMessage(EntityTypes.Luckyme, Period.Daily, luckyMe.Amount));

                        await StakersHub.Clients.All.SendAsync("adddailyluckymeparticipant",
                        new LuckyMeParticipantDTO
                        {
                            Id = luckyMe.Id,
                            UserId = luckyMe.User.Id,
                            UserName = luckyMe.User.FirstName + " " + luckyMe.User.LastName,
                            AmountStaked = luckyMe.Amount.ToString("0.##"),
                            AmountToWin = luckyMe.AmountToWin.ToString("0.##"),
                            Status = luckyMe.Status.ToLower(),
                            DateDeclared = luckyMe.DateDeclared
                        });

                        foreach (var dummy in DataHub.ManageDummies(EntityTypes.Luckyme, Period.Daily))
                        {
                            var luckymeDailyDummy = dummy as LuckyMe;
                            await StakersHub.Clients.All.SendAsync("adddailyluckymeparticipant",
                            new LuckyMeParticipantDTO
                            {
                                Id = luckymeDailyDummy.Id,
                                UserId = luckymeDailyDummy.User.Id,
                                UserName = luckymeDailyDummy.User.FirstName + " " + luckymeDailyDummy.User.LastName,
                                AmountStaked = luckymeDailyDummy.Amount.ToString("0.##"),
                                AmountToWin = luckymeDailyDummy.AmountToWin.ToString("0.##"),
                                Status = luckymeDailyDummy.Status.ToLower(),
                                DateDeclared = luckymeDailyDummy.DateDeclared
                            });
                        }
                    }
                    else if (luckyMe.Period.ToLower() == "weekly")
                    {
                        //Send as sms to the user
                        await MessagingService.SendSms(luckyMe.User.PhoneNumber, Misc.GetStakedUserSmsMessage(EntityTypes.Luckyme, Period.Weekly, luckyMe.Amount));


                        await StakersHub.Clients.All.SendAsync("addweeklyluckymeparticipant",
                           new LuckyMeParticipantDTO
                           {
                               Id = luckyMe.Id,
                               UserId = luckyMe.User.Id,
                               UserName = luckyMe.User.FirstName + " " + luckyMe.User.LastName,
                               AmountStaked = luckyMe.Amount.ToString("0.##"),
                               AmountToWin = luckyMe.AmountToWin.ToString("0.##"),
                               Status = luckyMe.Status.ToLower(),
                               DateDeclared = luckyMe.DateDeclared
                           });

                        //InsertLuckyMe dummies and Propagate them to user
                        foreach (var dummy in DataHub.ManageDummies(EntityTypes.Luckyme, Period.Weekly))
                        {
                            var luckymeWeeklyDummy = dummy as LuckyMe;
                            await StakersHub.Clients.All.SendAsync("addweeklyluckymeparticipant",
                            new LuckyMeParticipantDTO
                            {
                                Id = luckymeWeeklyDummy.Id,
                                UserId = luckymeWeeklyDummy.User.Id,
                                UserName = luckymeWeeklyDummy.User.FirstName + " " + luckymeWeeklyDummy.User.LastName,
                                AmountStaked = luckymeWeeklyDummy.Amount.ToString("0.##"),
                                AmountToWin = luckymeWeeklyDummy.AmountToWin.ToString("0.##"),
                                Status = luckymeWeeklyDummy.Status.ToLower(),
                                DateDeclared = luckymeWeeklyDummy.DateDeclared
                            });
                        }

                    }
                    else if (luckyMe.Period.ToLower() == "monthly")
                    {
                        //Send as sms to the user
                        await MessagingService.SendSms(luckyMe.User.PhoneNumber, Misc.GetStakedUserSmsMessage(EntityTypes.Luckyme, Period.Monthly, luckyMe.Amount));

                        await StakersHub.Clients.All.SendAsync("addmonthlyluckymeparticipant",
                           new LuckyMeParticipantDTO
                           {
                               Id = luckyMe.Id,
                               UserId = luckyMe.User.Id,
                               UserName = luckyMe.User.FirstName + " " + luckyMe.User.LastName,
                               AmountStaked = luckyMe.Amount.ToString("0.##"),
                               AmountToWin = luckyMe.AmountToWin.ToString("0.##"),
                               Status = luckyMe.Status.ToLower(),
                               DateDeclared = luckyMe.DateDeclared
                           });
                        //InsertLuckyMe dummies and Propagate them to user
                        foreach (var dummy in DataHub.ManageDummies(EntityTypes.Luckyme, Period.Monthly))
                        {
                            var luckymeMonthlyDummy = dummy as LuckyMe;
                            await StakersHub.Clients.All.SendAsync("addmonthlyluckymeparticipant",
                            new LuckyMeParticipantDTO
                            {
                                Id = luckymeMonthlyDummy.Id,
                                UserId = luckymeMonthlyDummy.User.Id,
                                UserName = luckymeMonthlyDummy.User.FirstName + " " + luckymeMonthlyDummy.User.LastName,
                                AmountStaked = luckymeMonthlyDummy.Amount.ToString("0.##"),
                                AmountToWin = luckymeMonthlyDummy.AmountToWin.ToString("0.##"),
                                Status = luckymeMonthlyDummy.Status.ToLower(),
                                DateDeclared = luckymeMonthlyDummy.DateDeclared
                            });
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                errorString = ex.Message;
            }

            return Ok(new { luckyMe, resultString, errorString });
        }


        [HttpPost("verifyScholarshipPayment/{txRef}")]
        public async Task<IActionResult> VerifyScholarshipPayment(string txRef/*, [FromBody]Scholarship scholarship*/)
        {
            var scholarship = dbContext.Scholarships.Where(i => i.TxRef == txRef && i.Status.ToLower() == "pending").Include("User").FirstOrDefault();

            scholarship.User.Points += scholarship.Amount * Constants.PointConstant;

            if (scholarship == null)
            {
                return BadRequest(new { errorString = "Scholarship stake was not found" });
            }
            string resultString = null;
            string errorString = null;

            try
            {
                if (VerifyPayment(txRef))
                    scholarship.Status = "paid";
                else
                    scholarship.Status = "pending";

                await dbContext.SaveChangesAsync();
                //Find the current user associated with the scholarship


                //send the currently added participant to all clients
                if (scholarship.User != null && scholarship.Status.ToLower() == "paid")
                {
                    //Send as sms to the user
                    await MessagingService.SendSms(scholarship.User.PhoneNumber, Misc.GetStakedUserSmsMessage(EntityTypes.Scholarship, null, scholarship.Amount));

                    await StakersHub.Clients.All.SendAsync("addscholarshipparticipant",
                       new ScholarshipParticipantDTO
                       {
                           Id = scholarship.Id,
                           UserId = scholarship.User.Id,
                           UserName = scholarship.User.FirstName + " " + scholarship.User.LastName,
                           AmountStaked = scholarship.Amount.ToString("0.##"),
                           AmountToWin = scholarship.AmountToWin.ToString("0.##"),
                           Status = scholarship.Status.ToLower()
                       });
                    //Insert Scholarship dummies and Propagate them to user
                    foreach (var dummy in DataHub.ManageDummies(EntityTypes.Scholarship, null))
                    {
                        var ScholarshipDummy = dummy as Scholarship;
                        await StakersHub.Clients.All.SendAsync("addscholarshipparticipant",
                        new ScholarshipParticipantDTO
                        {
                            Id = ScholarshipDummy.Id,
                            UserId = ScholarshipDummy.User.Id,
                            UserName = ScholarshipDummy.User.FirstName + " " + ScholarshipDummy.User.LastName,
                            AmountStaked = ScholarshipDummy.Amount.ToString("0.##"),
                            AmountToWin = ScholarshipDummy.AmountToWin.ToString("0.##"),
                            Status = ScholarshipDummy.Status.ToLower(),
                            DateDeclared = ScholarshipDummy.DateDeclared
                        });
                    }

                }
                else if (scholarship.Status.ToLower() != "paid")
                {
                    errorString = "Payment Could not be verfied";
                }

            }
            catch (Exception ex)
            {
                errorString = ex.Message;
            }



            //  return Ok(hubtelresponse?.data?.checkoutUrl);
            return Ok(new { scholarship, resultString, errorString });
        }


        [HttpPost("verifyBusinessPayment/{txRef}")]
        public async Task<IActionResult> VerifyBusinessPayment(string txRef/*, [FromBody]Business business*/)
        {
            var business = dbContext.Businesses.Where(i => i.TxRef == txRef && i.Status.ToLower() == "pending").Include("User").FirstOrDefault();

            business.User.Points += business.Amount * Constants.PointConstant;

            if (business == null)
            {
                return BadRequest(new { errorString = "Business stake was not found" });
            }

            string resultString = null;
            string errorString = null;

            try
            {

                if (VerifyPayment(txRef))
                    business.Status = "paid";
                else
                    business.Status = "pending";

                await dbContext.SaveChangesAsync();
                //Find the current user associated with the business

                if (business.User != null && business.Status.ToLower() == "paid") //send the currently added participant to all clients

                    //Send as sms to the user
                    await MessagingService.SendSms(business.User.PhoneNumber, Misc.GetStakedUserSmsMessage(EntityTypes.Business, null, business.Amount));

                await StakersHub.Clients.All.SendAsync("addbusinessparticipant",
                         new BusinessParticipantDTO
                         {
                             Id = business.Id,
                             UserId = business.User.Id,
                             UserName = business.User.FirstName + " " + business.User.LastName,
                             AmountStaked = business.Amount.ToString("0.##"),
                             AmountToWin = business.AmountToWin.ToString("0.##"),
                             Status = business.Status.ToLower()
                         });

                //Insert Business dummies and Propagate them to user
                foreach (var dummy in DataHub.ManageDummies(EntityTypes.Business, null))
                {
                    var BusinessDummy = dummy as Business;
                    await StakersHub.Clients.All.SendAsync("addbusinessparticipant",
                    new BusinessParticipantDTO
                    {
                        Id = BusinessDummy.Id,
                        UserId = BusinessDummy.User.Id,
                        UserName = BusinessDummy.User.FirstName + " " + BusinessDummy.User.LastName,
                        AmountStaked = BusinessDummy.Amount.ToString("0.##"),
                        AmountToWin = BusinessDummy.AmountToWin.ToString("0.##"),
                        Status = BusinessDummy.Status.ToLower(),
                        DateDeclared = BusinessDummy.DateDeclared
                    });
                }


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
            dbContext.Payments.Add(payment);
            await dbContext.SaveChangesAsync();
            return Ok("Payment Made Successfully");
        }

        [HttpPost("ravehook")]
        public async Task<IActionResult> RaveWebHook(WebhookCallback response)
        {
            if (await dbContext.LuckyMes.AnyAsync(i => i.TxRef == response.txRef))
            {

                await VerifyLuckymePayment(response.txRef);

            }
            else if (await dbContext.Businesses.AnyAsync(i => i.TxRef == response.txRef))
            {
                await VerifyBusinessPayment(response.txRef);
            }
            else if (await dbContext.Scholarships.AnyAsync(i => i.TxRef == response.txRef))
            {
                await VerifyScholarshipPayment(response.txRef);
            }

            return Ok();
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