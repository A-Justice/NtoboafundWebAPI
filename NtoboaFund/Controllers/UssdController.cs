using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.DTO_s;
using NtoboaFund.Data.Models;
using NtoboaFund.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NtoboaFund.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UssdController : ControllerBase
    {
        Dictionary<string, UssdResponse> ussdResponse = null;

        List<int> luckymePrices = new List<int>() { 1, 5, 10, 20, 50, 100, 500 };
        List<int> businessPrices = new List<int>() { 100, 500, 1000, 2000 };
        List<int> scholarshipPrices = new List<int>() { 100 };

        List<string> luckymePeriods = new List<string>() { "daily", "weekly", "monthly" };
        List<string> businessPeriods = new List<string>() { "monthly" };
        List<string> scholarshipPeriods = new List<string>() { "quarterly" };

        public NtoboaFundDbContext dbContext { get; set; }

        public AppSettings Settings { get; set; }
        public UssdController(NtoboaFundDbContext _dbContext, IOptions<AppSettings> appSettings)
        {
            dbContext = _dbContext;

            Settings = appSettings.Value;

            ussdResponse = new Dictionary<string, UssdResponse>()
            {
                #region Initiation
                {
                    "null",new UssdResponse{
                        Message = "Welcome To Ntoboafund\nChoose your stake option\n 1. Luckyme\n 2. Business\n 3. Scholarship\n 4. Check my live stakes",
                        Type="Response",
                        ClientState = "1"
                    }
                },
                #endregion

                #region Categories
                {
                    "1-1",new UssdResponse
                    {
                        Message = "Choose the luckyme amount you want to stake with\n 1. 1 GHS\n 2. 5 GHS\n 3. 10 GHS\n 4. 20 GHS\n 5. 50 GHS\n 6. 100 GHS\n 7. 500 GHS\n 0. Go Back",
                        Type = "Response",
                        ClientState = "lkm"
                    }
                },
                {
                    "1-2",new UssdResponse
                    {
                        Message = "Choose the business amount you want to stake with\n 1. 100 GHS\n 2. 500 GHS\n 3. 1000 GHS\n 4. 2000 GHS\n 0. Go Back",
                        Type = "Response",
                        ClientState = "bus"
                    }
                },
                {
                    "1-3",new UssdResponse
                    {
                        Message = "Choose the scholarship amount you want to stake with\n 1. 100 GHS\n 0. Go Back",
                        Type = "Response",
                        ClientState = "sch"
                    }
                },
                {
                    "1-4",new UssdResponse
                    {
                        Message = $"Your live stakes are : \n {0}",
                        Type = "Release",
                        ClientState = "cls"
                    }
                },
                #endregion

                {
                    "lkm",new UssdResponse
                    {
                        Message = "Choose your stake period for luckyme\n 1. Daily \n 2. Weekly\n 3. Monthly\n 0. Go Back",
                        Type = "Response"
                    }
                },
                {
                    "bus",new UssdResponse
                    {
                        Message = "Choose your stake period for luckyme\n 1. Monthly\n 0. Go Back",
                        Type = "Response"
                    }
                },
                {
                    "sch",new UssdResponse
                    {
                        Message = "Choose your stake period for luckyme\n 1. Quaterly\n 0. Go Back",
                        Type = "Response"
                    }
                },

                {
                    "number",new UssdResponse
                    {
                        Message = "Enter the phone number you want to make the transaction with\n 0. Go Back",
                        Type = "Response"
                    }
                },

                {
                    "complete",new UssdResponse
                    {
                        Message = "",
                        Type = "Release"
                    }
                },



            };
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody]UssdRequest ussdRequest)
        {
            try
            {
                switch (ussdRequest.ClientState)
                {
                    case null:
                        return Json(ussdResponse["null"]);

                    case "1":
                        var response1 = ussdResponse[$"1-{ussdRequest.Message}"];
                        return Json(response1);

                    case "lkm":
                        if (ussdRequest.Message == "0")
                            return Json(ussdResponse["null"]);
                        //get the ussd response string showing user to choose period after choosing money
                        var responselkm = ussdResponse["lkm"];
                        //Get the user's response for the money choice made
                        var responselkmMessage = Convert.ToInt32(ussdRequest.Message);
                        //get the actual correspondig amount and append to the client state
                        responselkm.ClientState = $"lkm-{luckymePrices[responselkmMessage - 1]}";
                        //return the formated response
                        return Json(responselkm);

                    case "bus":
                        if (ussdRequest.Message == "0")
                            return Json(ussdResponse["null"]);
                        //get the ussd response string showing user to choose period after choosing money
                        var responsebus = ussdResponse["bus"];
                        //Get the user's response for the money choice made
                        var responsebusMessage = Convert.ToInt32(ussdRequest.Message);
                        //get the actual correspondig amount and append to the client state
                        responsebus.ClientState = $"bus-{businessPrices[responsebusMessage - 1]}";
                        //return the formated response
                        return Json(responsebus);

                    case "sch":
                        if (ussdRequest.Message == "0")
                            return Json(ussdResponse["null"]);
                        //get the ussd response string showing user to choose period after choosing money
                        var responsesch = ussdResponse["sch"];
                        //Get the user's response for the money choice made
                        var responseschMessage = Convert.ToInt32(ussdRequest.Message);
                        //get the actual correspondig amount and append to the client state
                        responsesch.ClientState = $"sch-{luckymePrices[responseschMessage - 1]}";
                        //return the formated response
                        return Json(responsesch);

                    case var lkmx when new Regex(@"^lkm-(\d+)$").IsMatch(lkmx):
                        if (ussdRequest.Message == "0")
                            return Json(ussdResponse["1-1"]);
                        //get the ussd response string showing user to choose period after choosing money
                        var responselkmx = ussdResponse["number"];
                        //Get the user's response for the period choice made
                        var responselkmxMessage = Convert.ToInt32(ussdRequest.Message);
                        //get the actual correspondig amount and append to the client state
                        responselkmx.ClientState = $"{ussdRequest.ClientState}-{luckymePeriods[responselkmxMessage - 1]}";
                        //return the formated response
                        return Json(responselkmx);

                    case var busx when new Regex(@"^bus-(\d+)$").IsMatch(busx):
                        if (ussdRequest.Message == "0")
                            return Json(ussdResponse["1-2"]);
                        //get the ussd response string showing user to choose period after choosing money
                        var responsebusx = ussdResponse["number"];
                        //Get the user's response for the period choice made
                        var responsebusxMessage = Convert.ToInt32(ussdRequest.Message);
                        //get the actual correspondig amount and append to the client state
                        responsebusx.ClientState = $"{ussdRequest.ClientState}-{businessPeriods[responsebusxMessage - 1]}";
                        //return the formated response
                        return Json(responsebusx);

                    case var schx when new Regex(@"^sch-(\d+)$").IsMatch(schx):
                        if (ussdRequest.Message == "0")
                            return Json(ussdResponse["1-3"]);
                        //get the ussd response string showing user to choose period after choosing money
                        var responseschx = ussdResponse["number"];
                        //Get the user's response for the period choice made
                        var responseschxMessage = Convert.ToInt32(ussdRequest.Message);
                        //get the actual correspondig amount and append to the client state
                        responseschx.ClientState = $"{ussdRequest.ClientState}-{scholarshipPeriods[responseschxMessage - 1]}";
                        //return the formated response
                        return Json(responseschx);


                    case var numb when new Regex(@"^(\w+)-(\d+)-(\w+)$").IsMatch(numb):

                        var regex = new Regex(@"^(?<type>\w+)-(?<amount>\d+)-(?<period>\w+)$").Match(numb);
                        var type = regex.Groups["type"].ToString();
                        var amount = regex.Groups["amount"].ToString();
                        var period = regex.Groups["period"].ToString();

                        if (ussdRequest.Message == "0")
                        {
                            var response = ussdResponse[type];
                            response.ClientState = $"{type}-{amount}";
                            return Json(response);
                        }

                        //get the ussd response string showing user to choose period after choosing money
                        var responsenumb = ussdResponse["complete"];
                        responsenumb.Message = GetDrawMessage(type, amount, period, ussdRequest.Message);
                        //Get the user's response for the number entered
                        var responseMomoMessage = ussdRequest.Message;
                        //get the actual correspondig amount and append to the client state
                        responsenumb.ClientState = $"{ussdRequest.ClientState}-{ussdRequest.Mobile}-{responseMomoMessage}";

                        await PersistUssdData(responsenumb.ClientState);
                        //return the formated response
                        return Json(responsenumb);
                    default:
                        return Json(ussdResponse["null"]);
                }
            }
            catch
            {
                return Json(new UssdResponse
                {
                    Message = "Sorry and error ocured",
                    Type = "Release"
                });
            }
        }


        async Task PersistUssdData(string requestString)
        {
            //types : lkm,bus,sch
            var match = Regex.Match(requestString, @"(?<type>\w+)-(?<amount>\d+)-(?<period>\w+)-(?<mobilenumber>\w+)-(?<momonumber>\w+)");
            var type = match.Groups["type"].ToString();
            var amount = Convert.ToDecimal(match.Groups["amount"].ToString());
            var period = match.Groups["period"].ToString();
            var mobileNumber = match.Groups["mobilenumber"].ToString();
            var momoNumber = match.Groups["momonumber"].ToString();
            var user = await getMatchedUser(mobileNumber);

            var raveRequest = new RaveRequestDTO()
            {
                amount = amount.ToString(),
                txRef = getTxRef(mobileNumber),
                customer_email = user.Email,
                currency = "GHS",
                PBFPubKey = Settings.FlatterWaveSettings.LiveApiKey,
                customer_phone = momoNumber,
                payment_options = "mobilemoneyghana",
                country = "GH"
            };


            //if (type == "lkm")
            //{

            //    var luckyme = new LuckyMe
            //    {
            //        Date = DateTime.Now.ToLongDateString(),
            //        AmountToWin = amount * Constants.LuckymeStakeOdds,
            //        Status = "pending",
            //        TxRef = raveRequest.txRef,
            //        Period = period,
            //        User = user
            //    };

            //    dbContext.LuckyMes.Add(luckyme);

            //}
            //else if (type == "bus")
            //{
            //    var business = new Business
            //    {
            //        Date = DateTime.Now.ToLongDateString(),
            //        AmountToWin = amount * Constants.BusinessStakeOdds,
            //        Status = "pending",
            //        TxRef = raveRequest.txRef,
            //        Period = period,
            //        User = user
            //    };

            //    dbContext.Businesses.Add(business);

            //}
            //else if (type == "sch")
            //{
            //    var scholarship = new Scholarship
            //    {
            //        Date = DateTime.Now.ToLongDateString(),
            //        AmountToWin = amount * Constants.ScholarshipStakeOdds,
            //        Status = "pending",
            //        TxRef = raveRequest.txRef,
            //        Period = period,
            //        User = user
            //    };

            //    dbContext.Scholarships.Add(scholarship);

            //}

            //await dbContext.SaveChangesAsync();

            var httpClient = new HttpClient();

            try
            {
                var payload = JsonConvert.SerializeObject(raveRequest);

                HttpResponseMessage response = httpClient.PostAsync("https://api.ravepay.co/flwv3-pug/getpaidx/api/v2/hosted/pay", new StringContent(payload)).Result;

                Console.WriteLine(response);
            }
            catch (Exception ex)
            {

            }
        }

        string GetDrawMessage(string type, string amount, string period, string momoNumber)
        {
            var sBuilder = new StringBuilder();

            int amountStaked = 0;
            int.TryParse(amount, out amountStaked);


            var c = amountStaked > 1 ? "Cedis" : "Cedi";

            if (type == "lkm")
            {

                sBuilder.AppendLine($"Thank you for your {amount} {c} lucky me stake");
                sBuilder.AppendLine($"Your Potential Returns is {amountStaked * Constants.LuckymeStakeOdds} Cedis");
                if (period == "daily")
                    sBuilder.AppendLine($"A winner will be anounced at 6:00 pm on {DateTime.Now.DailyStakeEndDate().ToLongDateString()}");
                else if (period == "weekly")
                    sBuilder.AppendLine($"A winner will be anounced at 6:00 pm on {DateTime.Now.EndOfWeek(18, 0, 0, 0).ToLongDateString()}");
                else if (period == "monthly")
                    sBuilder.AppendLine($"A winner will be anounced at 6:00 pm on {DateTime.Now.EndOfMonth(18, 0, 0, 0).ToLongDateString()}");

            }
            else if (type == "bus")
            {
                sBuilder.AppendLine($"Thank you for your {amount} Cedis Business stake");
                sBuilder.AppendLine($"Your Potential Return is {amountStaked * Constants.BusinessStakeOdds} Cedis");
                sBuilder.AppendLine($"A winner will be anounced at 6:00 pm on {DateTime.Now.EndOfMonth(18, 0, 0, 0).ToLongDateString()}");
            }
            else if (type == "sch")
            {
                sBuilder.AppendLine($"Thank you for your {amount} Cedis Scholarship stake");
                sBuilder.AppendLine($"Your Potential Return is {amountStaked * Constants.ScholarshipStakeOdds} Cedis");
                sBuilder.AppendLine($"A winner will be anounced at 6:00 pm on {DateTime.Now.NextQuater(18, 0, 0, 0).ToLongDateString()}");

            }

            if (momoNumber != null)
            {
                //sBuilder.AppendLine("Step 1 Dial *170#\nStep 2 Choose option: 6) Wallet\nStep 3 Choose option: 3) My Approvals\nStep 4 Enter your MOMO pin to retrieve your pending approval list\nStep 5 Choose a pending transaction\nStep 6 Choose option 1 to approve\nStep 7 Tap button to continue");
            }


            return sBuilder.ToString();
        }


        string getTxRef(string phoneNumber)
        {
            var match = Regex.Match(phoneNumber, @"^(\w{2}).*(\w{2})$");

            var userCode = match.Groups[1].ToString() + match.Groups[2].ToString();
            var timeStamp = DateTime.Now.TimeOfDay.ToString();
            return $"inv.{ userCode}.{timeStamp}";
        }

        async Task<ApplicationUser> getMatchedUser(string phoneNumber)
        {
            var user = dbContext.Users.FirstOrDefault(i => i.PhoneNumber == phoneNumber);

            if (user != null)
                return user;

            user = new ApplicationUser
            {
                FirstName = phoneNumber,
                PhoneNumber = phoneNumber,
                Email = "ntoboafund.ussd@gmail.com"
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return user;
        }


        JsonResult Json(object result)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver()
            };
            return new JsonResult(result, settings);
        }
    }



    public class UssdRequest
    {
        public string Type { get; set; }

        public string Mobile { get; set; }

        public string SessionId { get; set; }

        public string ServiceCode { get; set; }

        public string Message { get; set; }

        public string Operator { get; set; }

        public string Sequence { get; set; }

        public string ClientState { get; set; }


    }

    public class UssdResponse
    {
        public string Message { get; set; }

        public string Type { get; set; }

        public string ClientState { get; set; }
    }
}