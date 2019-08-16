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

        [HttpPost("gethubtelurlforluckyme")]
        public async Task<IActionResult> GetHubtelUrlForLuckyMe([FromBody]LuckyMe LuckyMe)
        {
            LuckyMe.Date = DateTime.Now.ToLongDateString();
            LuckyMe.AmountToWin = 2000;
            LuckyMe.Status = "Pending";
            LuckyMe.User = await context.Users.FindAsync(LuckyMe.UserId);
            context.LuckyMes.Add(LuckyMe);

            string resultString = null;
            string errorString = null;
            try
            {

                await context.SaveChangesAsync();

                if (LuckyMe.User != null) //send the currently added participant to all clients
                {
                    if (LuckyMe.Period.ToLower() == "daily")
                    {
                        await StakersHub.Clients.All.SendAsync("adddailyluckymeparticipant",
                        new LuckyMeParticipantDTO
                        {
                            UserId = LuckyMe.User.Id,
                            UserName = LuckyMe.User.FirstName + " " + LuckyMe.User.LastName,
                            AmountStaked = LuckyMe.Amount.ToString(),
                            AmountToWin = LuckyMe.AmountToWin.ToString()
                        });
                    }
                    else if (LuckyMe.Period.ToLower() == "weekly")
                    {
                        await StakersHub.Clients.All.SendAsync("addweeklyluckymeparticipant",
                           new LuckyMeParticipantDTO
                           {
                               UserId = LuckyMe.User.Id,
                               UserName = LuckyMe.User.FirstName + " " + LuckyMe.User.LastName,
                               AmountStaked = LuckyMe.Amount.ToString(),
                               AmountToWin = LuckyMe.AmountToWin.ToString()
                           });
                    }
                    else if (LuckyMe.Period.ToLower() == "monthly")
                    {
                        await StakersHub.Clients.All.SendAsync("addmonthlyluckymeparticipant",
                           new LuckyMeParticipantDTO
                           {
                               UserId = LuckyMe.User.Id,
                               UserName = LuckyMe.User.FirstName + " " + LuckyMe.User.LastName,
                               AmountStaked = LuckyMe.Amount.ToString(),
                               AmountToWin = LuckyMe.AmountToWin.ToString()
                           });
                    }

                }



                resultString = GenerateHubtelUrl(LuckyMe.Id, LuckyMe.Amount, "luckyme");
            }
            catch (Exception ex)
            {
                errorString = ex.Message;
            }



            //  return Ok(hubtelresponse?.data?.checkoutUrl);
            return Ok(new { LuckyMe, resultString, errorString });
        }


        [HttpPost("gethubtelurlforscholarship")]
        public async Task<IActionResult> GetHubtelUrlForScholarship([FromBody]Scholarship Scholarship)
        {
            Scholarship.Amount = 100;
            Scholarship.Date = DateTime.Now.ToLongDateString();
            Scholarship.AmountToWin = 10000;
            Scholarship.Status = "Pending";
            Scholarship.Period = "quaterly";
            Scholarship.User = context.Users.Find(Scholarship.UserId);

            if (ModelState.IsValid)
            {
                Scholarship.UserId = null;
                context.Scholarships.Add(Scholarship);

            }
            else
                BadRequest("Submitted Form is Invalid");

            string resultString = null;
            string errorString = null;

            try
            {
                await context.SaveChangesAsync();
                //Find the current user associated with the scholarship

                if (Scholarship.User != null) //send the currently added participant to all clients
                    await StakersHub.Clients.All.SendAsync("addscholarshipparticipant",
                         new ScholarshipParticipantDTO
                         {
                             UserId = Scholarship.User.Id,
                             UserName = Scholarship.User.FirstName + " " + Scholarship.User.LastName,
                             AmountStaked = Scholarship.Amount.ToString(),
                             AmountToWin = Scholarship.AmountToWin.ToString()
                         });
                resultString = GenerateHubtelUrl(Scholarship.Id, Scholarship.Amount, "scholarship");
            }
            catch (Exception ex)
            {
                errorString = ex.Message;
            }



            //  return Ok(hubtelresponse?.data?.checkoutUrl);
            return Ok(new { Scholarship, resultString, errorString });
        }


        [HttpPost("gethubtelurlforbusiness")]
        public async Task<IActionResult> GetHubtelUrlForBusiness([FromBody]Business business)
        {
            business.Date = DateTime.Now.ToLongDateString();
            business.AmountToWin = 10000;
            business.Status = "Pending";
            business.Period = "monthly";
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
                await context.SaveChangesAsync();
                //Find the current user associated with the business

                if (business.User != null) //send the currently added participant to all clients
                    await StakersHub.Clients.All.SendAsync("addbusinessparticipant",
                         new BusinessParticipantDTO
                         {
                             UserId = business.User.Id,
                             UserName = business.User.FirstName + " " + business.User.LastName,
                             AmountStaked = business.Amount.ToString(),
                             AmountToWin = business.AmountToWin.ToString()
                         });
                resultString = GenerateHubtelUrl(business.Id, business.Amount, "business");
            }
            catch (Exception ex)
            {
                errorString = ex.Message;
            }



            //  return Ok(hubtelresponse?.data?.checkoutUrl);
            return Ok(new { business, resultString, errorString });
        }


        [HttpPost("hubtelcallback")]
        public IActionResult HubtleCallBack(HubtelResponse response)
        {
            return Ok(response);
        }

        [HttpPost("luckymepaid")]
        public IActionResult LuckyMePaid(HubtelResponse response)
        {
            return Ok(response);
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

            var authDetails = $"Basic {Base64Encode(AppSettings.HubtelApiSettings.ApiKey + ":" + AppSettings.HubtelApiSettings.ApiSecret)}";
            request.Headers.Add("Authorization", authDetails);


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

            HubtelResponse hubtelresponse = null;
            string resultString = null;

            var response = (HttpWebResponse)request.GetResponse();

            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                resultString = streamReader.ReadToEnd();
                hubtelresponse = JsonConvert.DeserializeObject<HubtelResponse>(resultString);

            }
            response.Close();


            return resultString;
        }


    }



    public class HubtelResponse
    {
        public string responseCode { get; set; }

        public string status { get; set; }

        public HubtelData data { get; set; }
    }
    public class HubtelData
    {
        public string checkoutUrl { get; set; }

        public string checkoutId { get; set; }

        public string clientReference { get; set; }

        public string message { get; set; }
    }


}