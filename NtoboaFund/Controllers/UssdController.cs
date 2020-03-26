using DalSoft.Hosting.BackgroundQueue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.Models;
using NtoboaFund.Helpers;
using NtoboaFund.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
        List<int> scholarshipPrices = new List<int>() { 20, 50, 100 };

        List<string> luckymePeriods = new List<string>() { "daily", "weekly", "monthly" };
        List<string> businessPeriods = new List<string>() { "monthly" };
        List<string> scholarshipPeriods = new List<string>() { "quarterly" };

        public NtoboaFundDbContext dbContext { get; set; }

        public AppSettings Settings { get; set; }
        public BackgroundQueue BackgroundQueue { get; set; }
        public ReddePaymentService reddePaymentService { get; set; }
        public UssdController(NtoboaFundDbContext _dbContext, IOptions<AppSettings> appSettings, BackgroundQueue backgroundQueue, ReddePaymentService _reddePaymentService)
        {
            dbContext = _dbContext;

            Settings = appSettings.Value;

            BackgroundQueue = backgroundQueue;

            reddePaymentService = _reddePaymentService;


            ussdResponse = new Dictionary<string, UssdResponse>()
            {
                #region Initiation
                {
                    "null",new UssdResponse{
                        Message = "Welcome To Ntoboafund\nChoose your option\n 1. Luckyme\n 2. Business\n 3. Scholarship\n 4. Check my live Investments\n5.Terms and conditions",
                        Type="Response",
                        ClientState = "1"
                    }
                },
                #endregion

                #region Categories
                {
                    "1-1",new UssdResponse
                    {
                        Message = "Choose the luckyme amount you want to invest with\n 1. 1 GHS\n 2. 5 GHS\n 3. 10 GHS\n 4. 20 GHS\n 5. 50 GHS\n 6. 100 GHS\n 7. 500 GHS\n 0. Go Back",
                        Type = "Response",
                        ClientState = "lkm"
                    }
                },
                {
                    "1-2",new UssdResponse
                    {
                        Message = "Choose the business amount you want to invest with\n 1. 100 GHS\n 2. 500 GHS\n 3. 1000 GHS\n 4. 2000 GHS\n 0. Go Back",
                        Type = "Response",
                        ClientState = "bus"
                    }
                },
                {
                    "1-3",new UssdResponse
                    {
                        Message = "Choose the scholarship amount you want to invest with\n 1. 20 GHS\n 2. 50 GHS\n 3. 100 GHS\n 0. Go Back",
                        Type = "Response",
                        ClientState = "sch"
                    }
                },
                {
                    "1-4",new UssdResponse
                    {
                        Message = $"",
                        Type = "Response",
                        ClientState = "cls"
                    }
                },
                {
                    "1-5",new UssdResponse
                    {
                        Message = $"Visit https://ntoboafund.com/#/terms to view our terms and conditions.\n 0. Go Back",
                        Type = "Response",
                        ClientState = "terms"
                    }
                },
                #endregion

                {
                    "lkm",new UssdResponse
                    {
                        Message = "Choose your investment period for luckyme\n 1. Daily \n 2. Weekly\n 3. Monthly\n 0. Go Back",
                        Type = "Response"
                    }
                },
                {
                    "bus",new UssdResponse
                    {
                        Message = "Choose your investment period for Business\n 1. Monthly\n 0. Go Back",
                        Type = "Response"
                    }
                },
                {
                    "sch",new UssdResponse
                    {
                        Message = "Choose your ntoboa period for scholarship\n 1. Quaterly\n 0. Go Back",
                        Type = "Response"
                    }
                },


                {
                    "schInstitution",new UssdResponse
                    {
                        Message = "Which institution do you belong to ?\n\n 0. Go Back",
                        Type = "Response"
                    }
                },
                {
                    "schProgram",new UssdResponse
                    {
                        Message = "Which program do you offer ?\n\n 0. Go Back",
                        Type = "Response"
                    }
                },
                {
                    "schStudentId",new UssdResponse
                    {
                        Message = "What is your student id number ?\n\n 0. Go Back",
                        Type = "Response"
                    }
                },
                {
                    "schPlayerType",new UssdResponse
                    {
                        Message = "Select your player type \n 1. Student \n 2.Parent/Guardian \n 0. Go Back",
                        Type = "Response"
                    }
                },

                {
                    "number",new UssdResponse
                    {
                        Message = "Enter the phone number you want to make the transaction with, if different.\n1. This Number\n 0. Go Back",
                        Type = "Response"
                    }
                },
                {
                    "incorrectNumber",new UssdResponse
                    {
                        Message = "Incorrect PhoneNumber \nEnter the phone number you want to make the transaction with, if different.\n 0. Go Back",
                        Type = "Response"
                    }
                },
                {
                    "voucher",new UssdResponse
                    {
                        Message = "Enter the voucher for the transaction (Only for vodafone cash) \n0. Go Back",
                        Type = "Response"
                    }
                },

                {
                    "drawMessage",new UssdResponse
                    {
                        Message = "1.Confirm\n0. Go Back",
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
                    case null://Return Various Stake Options
                        return Json(ussdResponse["null"]);

                    case "1": //Returns the Stake Amounts for chosen Stake Option
                        try
                        {
                            var response1 = ussdResponse[$"1-{ussdRequest.Message}"];
                            if (ussdRequest.Message == "4")
                            {
                                response1.Message = getLiveStakesForUser($"0{Misc.NormalizePhoneNumber(ussdRequest.Mobile)}");
                            }
                            return Json(response1);
                        }
                        catch
                        {
                            var response = ussdResponse["null"];
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }

                    case "lkm": //Returns LuckyMe Periods Page
                        try
                        {
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
                        }
                        catch
                        {
                            var response = ussdResponse["1-1"];
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }

                    case "bus": //Returns Business Periods Page
                        try
                        {
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
                        }
                        catch
                        {
                            var response = ussdResponse["1-2"];
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }

                    case "sch": //Returns Scholarship Periods Page
                        try
                        {
                            if (ussdRequest.Message == "0")
                                return Json(ussdResponse["null"]);
                            //get the ussd response string showing user to choose period after choosing money
                            var responsesch = ussdResponse["sch"];
                            //Get the user's response for the money choice made
                            var responseschMessage = Convert.ToInt32(ussdRequest.Message);
                            //get the actual correspondig amount and append to the client state
                            responsesch.ClientState = $"sch*{scholarshipPrices[responseschMessage - 1]}";
                            //return the formated response
                            return Json(responsesch);
                        }
                        catch
                        {
                            var response = ussdResponse["1-3"];
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }
                    case "terms":
                        try
                        {
                            if (ussdRequest.Message == "0")
                                return Json(ussdResponse["null"]);
                            else
                                throw new Exception();
                        }
                        catch
                        {
                            var response = ussdResponse["1-5"];
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }
                    case var lkmx when new Regex(@"^lkm-(\d+)$").IsMatch(lkmx): //Returns Number Entry Page for LuckyMe
                        try
                        {
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
                        }
                        catch
                        {
                            var response = ussdResponse["lkm"];
                            response.ClientState = ussdRequest.ClientState;
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }

                    case var busx when new Regex(@"^bus-(\d+)$").IsMatch(busx)://Returns Number Entry Page for Business
                        try
                        {
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
                        }
                        catch
                        {
                            var response = ussdResponse["bus"];
                            response.ClientState = ussdRequest.ClientState;
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }

                    case var schInstitution when new Regex(@"^sch\*(\d+)$").IsMatch(schInstitution)://show Institution Entry Page for Scholarship
                                                                                                    //Get the user's response for the period choice made
                        try
                        {
                            if (ussdRequest.Message == "0")
                                return Json(ussdResponse["1-3"]);

                            var selectedPeriodIndex = Convert.ToInt32(ussdRequest.Message);

                            //get the ussd response string showing user to enter Institution after Choosing Period
                            var responseInstitution = ussdResponse["schInstitution"];
                            //get the actual correspondig amount and append to the client state
                            responseInstitution.ClientState = $"{ussdRequest.ClientState}-{scholarshipPeriods[selectedPeriodIndex - 1]}";
                            //return the formated response
                            return Json(responseInstitution);
                        }
                        catch
                        {
                            var response = ussdResponse["sch"];
                            response.ClientState = ussdRequest.ClientState;
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }

                    case var schProgram when new Regex(@"^sch\*(\d+)-(\w+)$").IsMatch(schProgram)://show Program Entry Page Page for Scholarship

                        try
                        {
                            var regexschProg = new Regex(@"^(?<type>\w+)\*(?<amount>\d+)-(?<period>\w+)$").Match(schProgram);
                            var typeschProg = regexschProg.Groups["type"].ToString();
                            var amountschProg = regexschProg.Groups["amount"].ToString();
                            var periodschProg = regexschProg.Groups["period"].ToString();
                            //Get the user's response for the Institution Entered
                            var enteredInstitution = ussdRequest.Message;

                            if (ussdRequest.Message == "0")
                            {
                                var response = ussdResponse["sch"];
                                response.ClientState = $"sch*{amountschProg}";
                                return Json(response);
                            }
                            //get the ussd response string showing user to Enter Program
                            var responseProgram = ussdResponse["schProgram"];

                            //append the entered Institution to the client state
                            responseProgram.ClientState = $"{ussdRequest.ClientState}-{enteredInstitution}";
                            //return the formated response
                            return Json(responseProgram);
                        }
                        catch
                        {
                            var response = ussdResponse["schInstitution"];
                            response.ClientState = ussdRequest.ClientState;
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }

                    case var schStudentId when new Regex(@"^sch\*(\d+)-(\w+)-([\w\s]+)$").IsMatch(schStudentId)://show StudentId Entry Page for Scholarship
                        try
                        {
                            var regexschStId = new Regex(@"^(?<type>\w+)\*(?<amount>\d+)-(?<period>\w+)-(?<institution>[\w\s]+)$").Match(schStudentId);
                            var typeschStId = regexschStId.Groups["type"].ToString();
                            var amountschStId = regexschStId.Groups["amount"].ToString();
                            var periodschStId = regexschStId.Groups["period"].ToString();
                            var InstitutionschStId = regexschStId.Groups["institution"].ToString();
                            //Get the user's response for the Institution Entered
                            var enteredProgram = ussdRequest.Message;

                            if (ussdRequest.Message == "0")
                            {
                                var response = ussdResponse["schInstitution"];
                                response.ClientState = $"sch*{amountschStId}-{periodschStId}";
                                return Json(response);
                            }

                            //get the ussd response string showing user to choose period after choosing money
                            var responseStudentId = ussdResponse["schStudentId"];
                            //get the actual correspondig amount and append to the client state
                            responseStudentId.ClientState = $"{ussdRequest.ClientState}-{enteredProgram}";
                            //return the formated response
                            return Json(responseStudentId);
                        }
                        catch
                        {
                            var response = ussdResponse["schProgram"];
                            response.ClientState = ussdRequest.ClientState;
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }

                    case var schPlayerType when new Regex(@"^sch\*(\d+)-(\w+)-([\w\s]+)-([\w\s]+)$").IsMatch(schPlayerType)://show PlayerType Choice Page for Scholarship
                        try
                        {
                            var regexschPltp = new Regex(@"^(?<type>\w+)\*(?<amount>\d+)-(?<period>\w+)-(?<institution>[\w\s]+)-(?<program>[\w\s]+)$").Match(schPlayerType);
                            var typeschPltp = regexschPltp.Groups["type"].ToString();
                            var amountschPltp = regexschPltp.Groups["amount"].ToString();
                            var periodschPltp = regexschPltp.Groups["period"].ToString();
                            var InstitutionschPltp = regexschPltp.Groups["institution"].ToString();
                            var ProgramschPltp = regexschPltp.Groups["program"].ToString();
                            //Get the user's response for the Institution Entered
                            var enteredStudentId = ussdRequest.Message;

                            if (ussdRequest.Message == "0")
                            {
                                var response = ussdResponse["schProgram"];
                                response.ClientState = $"sch*{amountschPltp}-{periodschPltp}-{InstitutionschPltp}";
                                return Json(response);
                            }
                            //get chosen player type choice
                            var responsePlayerType = ussdResponse["schPlayerType"];
                            //get the actual correspondig amount and append to the client state
                            responsePlayerType.ClientState = $"{ussdRequest.ClientState}-{enteredStudentId}";
                            //return the formated response
                            return Json(responsePlayerType);
                        }
                        catch
                        {
                            var response = ussdResponse["schStudentId"];
                            response.ClientState = ussdRequest.ClientState;
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }

                    case var schx when new Regex(@"^sch\*(\d+)-(\w+)-([\w\s]+)-([\w\s]+)-([\w\s]+)$").IsMatch(schx)://Returns Number Entry Page for Scholarship
                        try
                        {
                            if (ussdRequest.Message != "0" && ussdRequest.Message != "1" && ussdRequest.Message != "2")
                                throw new Exception("Invalid Player Type");

                            var selectedPlayerType = ussdRequest.Message;

                            var regexschNum = new Regex(@"^(?<type>\w+)\*(?<amount>\d+)-(?<period>\w+)-(?<institution>[\w\s]+)-(?<program>[\w\s]+)-(?<studentid>[\w\s]+)$").Match(schx);
                            var typeschNum = regexschNum.Groups["type"].ToString();
                            var amountschNum = regexschNum.Groups["amount"].ToString();
                            var periodschNum = regexschNum.Groups["period"].ToString();
                            var InstitutionschNum = regexschNum.Groups["institution"].ToString();
                            var ProgramschNum = regexschNum.Groups["program"].ToString();
                            var StudentIdschNum = regexschNum.Groups["studentid"].ToString();
                            //Get the user's response for the Institution Entered


                            if (ussdRequest.Message == "0")
                            {
                                var response = ussdResponse["schStudentId"];
                                response.ClientState = $"sch*{amountschNum}-{periodschNum}-{InstitutionschNum}-{ProgramschNum}";
                                return Json(response);
                            }
                            //get the ussd response string showing user to choose period after choosing money
                            //var responseschxMessage = Convert.ToInt32(ussdRequest.Message);

                            //var regexschx = new Regex(@"^(?<type>\w+)-(?<amount>\d+)$").Match(schx);
                            //var typeschx = regexschx.Groups["type"].ToString();
                            //var amountschx = regexschx.Groups["amount"].ToString();
                            //var periodschx = scholarshipPeriods[responseschxMessage - 1];

                            var responseschNum = ussdResponse["number"];

                            //Get the user's response for the period choice made
                            //get the actual correspondig amount and append to the client state
                            responseschNum.ClientState = $"{ussdRequest.ClientState}-{selectedPlayerType}";
                            //return the formated response
                            return Json(responseschNum);
                        }
                        catch
                        {
                            var response = ussdResponse["schPlayerType"];
                            response.ClientState = ussdRequest.ClientState;
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }

                    //Gets Entered Number
                    case var vouchersch when new Regex(@"^sch\*(\d+)-(\w+)-([\w\s]+)-([\w\s]+)-([\w\s]+)-(\d+)$").IsMatch(vouchersch):// returns Voucher Entry Page for scholarships
                        try
                        {
                            var regexschVoucher = new Regex(@"^(?<type>\w+)\*(?<amount>\d+)-(?<period>\w+)-(?<institution>[\w\s]+)-(?<program>[\w\s]+)-(?<studentid>[\w\s]+)-(?<playertype>\d+)$").Match(vouchersch);
                            var typeschVoucher = regexschVoucher.Groups["type"].ToString();
                            var amountschVoucher = regexschVoucher.Groups["amount"].ToString();
                            var periodschVoucher = regexschVoucher.Groups["period"].ToString();
                            var InstitutionschVoucher = regexschVoucher.Groups["institution"].ToString();
                            var ProgramschVoucher = regexschVoucher.Groups["program"].ToString();
                            var StudentIdschVoucher = regexschVoucher.Groups["studentid"].ToString();
                            var selectedPlayerTypeVoucher = regexschVoucher.Groups["playertype"].ToString();

                            if (ussdRequest.Message == "0")
                            {
                                var response = ussdResponse["schPlayerType"];
                                response.ClientState = $"sch*{amountschVoucher}-{periodschVoucher}-{InstitutionschVoucher}-{ProgramschVoucher}-{StudentIdschVoucher}";
                                return Json(response);
                            }
                            else if (ussdRequest.Message == "1")
                            {

                            }
                            else if (!Misc.IsCorrectGhanaianNumber(ussdRequest.Message))
                            {
                                var response = ussdResponse["incorrectNumber"];
                                response.ClientState = $"sch*{amountschVoucher}-{periodschVoucher}-{InstitutionschVoucher}-{ProgramschVoucher}-{StudentIdschVoucher}-{selectedPlayerTypeVoucher}";
                                return Json(response);
                            }
                            //get the ussd response string showing user to choose period after choosing money
                            string responseMomoNumberVoucher = null;
                            if (ussdRequest.Message == "1")
                                responseMomoNumberVoucher = ussdRequest.Mobile;
                            else
                                responseMomoNumberVoucher = ussdRequest.Message;

                            var isVodafone = false;
                            UssdResponse responsenumbVoucher = null;
                            if (Constants.PaymentGateway == PaymentGateway.redde)
                                isVodafone = Misc.getReddePayOption(responseMomoNumberVoucher) == "VODAFONE";

                            if (isVodafone)
                            {
                                responsenumbVoucher = ussdResponse["voucher"];
                                responsenumbVoucher.ClientState = $"{ussdRequest.ClientState}-{responseMomoNumberVoucher}";
                            }
                            else
                            {
                                responsenumbVoucher = ussdResponse["drawMessage"];
                                var drawMessage_Voucher = Misc.GetUssdPreStakeMessageForScholarship(amountschVoucher, InstitutionschVoucher, ProgramschVoucher, StudentIdschVoucher, getPlayerType(selectedPlayerTypeVoucher));
                                responsenumbVoucher.Message = drawMessage_Voucher + responsenumbVoucher.Message;
                                //The passed xxx represents a dummy voucher number
                                responsenumbVoucher.ClientState = $"{ussdRequest.ClientState}-{responseMomoNumberVoucher}-xxx";

                            }
                            return Json(responsenumbVoucher);
                        }
                        catch
                        {
                            var response = ussdResponse["voucher"];
                            response.ClientState = ussdRequest.ClientState;
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }

                    //Gets Entered Number
                    case var voucher when new Regex(@"^(\w+)-(\d+)-(\w+)$").IsMatch(voucher):// returns Voucher Entry Page for scholarships
                        try
                        {
                            var regex_numb = new Regex(@"^(?<type>\w+)-(?<amount>\d+)-(?<period>\w+)$").Match(voucher);
                            var type_numb = regex_numb.Groups["type"].ToString();
                            var amount_numb = regex_numb.Groups["amount"].ToString();
                            var period_numb = regex_numb.Groups["period"].ToString();


                            if (ussdRequest.Message == "0")
                            {
                                var response = ussdResponse[type_numb];
                                response.ClientState = $"{type_numb}-{amount_numb}";
                                return Json(response);
                            }
                            else if (ussdRequest.Message == "1")
                            {

                            }
                            else if (!Misc.IsCorrectGhanaianNumber(ussdRequest.Message))
                            {
                                var response = ussdResponse["incorrectNumber"];
                                response.ClientState = $"{type_numb}-{amount_numb}";
                                return Json(response);
                            }
                            //get the ussd response string showing user to choose period after choosing money
                            string responseMomoNumberVoucher = null;
                            if (ussdRequest.Message == "1")
                                responseMomoNumberVoucher = ussdRequest.Mobile;
                            else
                                responseMomoNumberVoucher = ussdRequest.Message;

                            var isVodafone = false;
                            UssdResponse responsenumbVoucher = null;
                            if (Constants.PaymentGateway == PaymentGateway.redde)
                                isVodafone = Misc.getReddePayOption(responseMomoNumberVoucher) == "VODAFONE";

                            if (isVodafone)
                            {
                                responsenumbVoucher = ussdResponse["voucher"];
                                responsenumbVoucher.ClientState = $"{ussdRequest.ClientState}-{responseMomoNumberVoucher}";
                            }
                            else
                            {
                                responsenumbVoucher = ussdResponse["drawMessage"];
                                var drawMessage_Voucher = Misc.GetUssdPreStakeMessage(type_numb, amount_numb, period_numb);
                                responsenumbVoucher.Message = drawMessage_Voucher + responsenumbVoucher.Message;
                                //The passed xxx represents a dummy voucher number
                                responsenumbVoucher.ClientState = $"{ussdRequest.ClientState}-{responseMomoNumberVoucher}-xxx";

                            }
                            return Json(responsenumbVoucher);
                        }
                        catch
                        {
                            var response = ussdResponse["voucher"];
                            response.ClientState = ussdRequest.ClientState;
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }


                    //Gets The Voucher for scholarship
                    case var drawSch when new Regex(@"^sch\*(\d+)-(\w+)-([\w\s]+)-([\w\s]+)-([\w\s]+)-(\d+)-(\d+)$").IsMatch(drawSch)://Returns DrawMessage Page ForScholarship
                        try
                        {
                            var regexschDraw = new Regex(@"^(?<type>\w+)\*(?<amount>\d+)-(?<period>\w+)-(?<institution>[\w\s]+)-(?<program>[\w\s]+)-(?<studentid>[\w\s]+)-(?<playertype>\d+)-(?<momonumber>[\d]+)$").Match(drawSch);
                            var typeschDraw = regexschDraw.Groups["type"].ToString();
                            var amountschDraw = regexschDraw.Groups["amount"].ToString();
                            var periodschDraw = regexschDraw.Groups["period"].ToString();
                            var InstitutionschDraw = regexschDraw.Groups["institution"].ToString();
                            var ProgramschDraw = regexschDraw.Groups["program"].ToString();
                            var StudentIdschDraw = regexschDraw.Groups["studentid"].ToString();
                            var selectedPlayerTypeDraw = regexschDraw.Groups["playertype"].ToString();
                            var momoNumber = regexschDraw.Groups["momonumber"].ToString();

                            if (ussdRequest.Message == "0")
                            {
                                var response = ussdResponse["schPlayerType"];
                                response.ClientState = $"sch*{amountschDraw}-{periodschDraw}-{InstitutionschDraw}-{ProgramschDraw}-{StudentIdschDraw}";
                                return Json(response);
                            }
                            else if (ussdRequest.Message == "1")
                            {

                            }
                            //get the ussd response string showing user to choose period after choosing money
                            var responsenumbDraw = ussdResponse["drawMessage"];

                            var drawMessage_Draw = Misc.GetUssdPreStakeMessageForScholarship(amountschDraw, InstitutionschDraw, ProgramschDraw, StudentIdschDraw, getPlayerType(selectedPlayerTypeDraw));

                            responsenumbDraw.Message = drawMessage_Draw + responsenumbDraw.Message;

                            //get the actual correspondig amount and append to the client state
                            responsenumbDraw.ClientState = $"{ussdRequest.ClientState}-{ussdRequest.Message}";


                            return Json(responsenumbDraw);
                        }
                        catch
                        {
                            var response = ussdResponse["number"];
                            response.ClientState = ussdRequest.ClientState;
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }

                    //Gets the Voucher for regular
                    case var draw when new Regex(@"^(\w+)-(\d+)-(\w+)-(\d+)$").IsMatch(draw)://Returns DrawMessage Page
                        try
                        {
                            var regex_numb = new Regex(@"^(?<type>\w+)-(?<amount>\d+)-(?<period>\w+)-(?<momonumber>\d+)$").Match(draw);
                            var type_numb = regex_numb.Groups["type"].ToString();
                            var amount_numb = regex_numb.Groups["amount"].ToString();
                            var period_numb = regex_numb.Groups["period"].ToString();
                            var momonumber_numb = regex_numb.Groups["momonumber"].ToString();

                            if (ussdRequest.Message == "0")
                            {
                                var response = ussdResponse["number"];
                                response.ClientState = $"{type_numb}-{amount_numb}-{period_numb}";
                                return Json(response);
                            }
                            else if (ussdRequest.Message == "1")
                            {

                            }
                            //get the ussd response string showing user to choose period after choosing money
                            var responsenumb = ussdResponse["drawMessage"];


                            var drawMessage = Misc.GetUssdPreStakeMessage(type_numb, amount_numb, period_numb);

                            responsenumb.Message = drawMessage + responsenumb.Message;


                            responsenumb.ClientState = $"{ussdRequest.ClientState}-{ussdRequest.Message}";


                            return Json(responsenumb);
                        }
                        catch
                        {
                            var response = ussdResponse["number"];
                            response.ClientState = ussdRequest.ClientState;
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }

                    case var msgProceedForScholarship when new Regex(@"^sch\*(\d+)-(\w+)-([\w\s]+)-([\w\s]+)-([\w\s]+)-(\d+)-(\d+)-([\w\s]+)").IsMatch(msgProceedForScholarship):
                        try
                        {
                            var regexsch_proceed = new Regex(@"^(?<type>\w+)\*(?<amount>\d+)-(?<period>\w+)-(?<institution>[\w\s]+)-(?<program>[\w\s]+)-(?<studentid>[\w\s]+)-(?<playertype>\d+)-(?<momonumber>\d+)-(?<voucher>[\w\s]+)$").Match(msgProceedForScholarship);
                            var typesch_proceed = regexsch_proceed.Groups["type"].ToString();
                            var amountsch_proceed = regexsch_proceed.Groups["amount"].ToString();
                            var periodsch_proceed = regexsch_proceed.Groups["period"].ToString();
                            var Institutionsch_proceed = regexsch_proceed.Groups["institution"].ToString();
                            var Programsch_proceed = regexsch_proceed.Groups["program"].ToString();
                            var StudentIdsch_proceed = regexsch_proceed.Groups["studentid"].ToString();
                            var selectedPlayerType_proceed = regexsch_proceed.Groups["playertype"].ToString();
                            var voucher_proceed = regexsch_proceed.Groups["voucher"].ToString();
                            var momonumbersch_proceed = regexsch_proceed.Groups["momonumber"].ToString();

                            if (ussdRequest.Message == "0")
                            {
                                if (Misc.getReddePayOption(momonumbersch_proceed) == "VODAFONE")
                                {
                                    var response = ussdResponse["voucher"];
                                    response.ClientState = $"sch*{amountsch_proceed}-{periodsch_proceed}-{Institutionsch_proceed}-{Programsch_proceed}-{StudentIdsch_proceed}-{selectedPlayerType_proceed}-{momonumbersch_proceed}";
                                    return Json(response);
                                }
                                else
                                {
                                    var response = ussdResponse["number"];
                                    response.ClientState = $"sch*{amountsch_proceed}-{periodsch_proceed}-{Institutionsch_proceed}-{Programsch_proceed}-{StudentIdsch_proceed}-{selectedPlayerType_proceed}";
                                    return Json(response);

                                }

                            }
                            else if (ussdRequest.Message != "1")
                            {
                                throw new Exception("Invalid Input");
                            }

                            var responsesch_proceed = ussdResponse["complete"];

                            if (Constants.PaymentGateway == PaymentGateway.flutterwave)
                            {
                                responsesch_proceed.Message = getMomoApprovalDirections(Misc.getNetwork(momonumbersch_proceed));

                                //await PersistRaveUssdData($"{msgProceedForScholarship}-{ussdRequest.Mobile}");
                            }
                            else if (Constants.PaymentGateway == PaymentGateway.slydepay)
                            {
                                responsesch_proceed.Message = "Wait For Mobile Money Prompt.\nNB: Mobile money users won't get prompt on insufficient balance.";

                                //await PersistScholarshipSlydepayUssdData($"{msgProceedForScholarship}-{ussdRequest.Mobile}");
                            }
                            else if (Constants.PaymentGateway == PaymentGateway.redde)
                            {
                                responsesch_proceed.Message = "Wait For Mobile Money Prompt.\nNB: Mobile money users won't get prompt on insufficient balance.";

                                BackgroundQueue.Enqueue(async cancellationToken =>
                                {
                                    reddePaymentService.PersistScholarshipReddeUssdData($"{msgProceedForScholarship}-{ussdRequest.Mobile}");
                                });

                            }
                            return Json(responsesch_proceed);
                        }
                        catch
                        {
                            var response = ussdResponse["drawMessage"];
                            response.ClientState = ussdRequest.ClientState;
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }


                    case var msgProceed when new Regex(@"^(\w+)-(\d+)-(\w+)-(\d+)-([\w\s]+)").IsMatch(msgProceed):
                        try
                        {
                            var regex_proceed = new Regex(@"^(?<type>\w+)-(?<amount>\d+)-(?<period>\w+)-(?<momonumber>\d+)-(?<voucher>[\w\s]+)").Match(msgProceed);
                            var type_proceed = regex_proceed.Groups["type"].ToString();
                            var amount_proceed = regex_proceed.Groups["amount"].ToString();
                            var period__proceed = regex_proceed.Groups["period"].ToString();
                            var momonumber_proceed = regex_proceed.Groups["momonumber"].ToString();
                            var voucher_proceed = regex_proceed.Groups["voucher"].ToString();

                            if (ussdRequest.Message == "0")
                            {
                                if (Misc.getReddePayOption(momonumber_proceed) == "VODAFONE")
                                {
                                    var response = ussdResponse["voucher"];
                                    response.ClientState = $"{type_proceed}-{amount_proceed}-{period__proceed}-{momonumber_proceed}";
                                    return Json(response);

                                }
                                else
                                {
                                    var response = ussdResponse["number"];
                                    response.ClientState = $"{type_proceed}-{amount_proceed}-{period__proceed}";
                                    return Json(response);

                                }
                            }
                            else if (ussdRequest.Message != "1")
                            {
                                throw new Exception("Invalid Input");
                            }


                            var response_proceed = ussdResponse["complete"];

                            if (Constants.PaymentGateway == PaymentGateway.flutterwave)
                            {
                                response_proceed.Message = getMomoApprovalDirections(Misc.getNetwork(momonumber_proceed));

                                //await PersistRaveUssdData($"{msgProceed}-{ussdRequest.Mobile}");

                            }
                            else if (Constants.PaymentGateway == PaymentGateway.slydepay)
                            {
                                response_proceed.Message = "Wait For Mobile Money Prompt.\nNB: Mobile money users won't get prompt on insufficient balance.";
                                //await PersistSlydepayUssdData($"{msgProceed}-{ussdRequest.Mobile}");
                            }
                            else if (Constants.PaymentGateway == PaymentGateway.redde)
                            {
                                response_proceed.Message = "Wait For Mobile Money Prompt.\nNB: Mobile money users won't get prompt on insufficient balance.";
                                BackgroundQueue.Enqueue(async cancellationToken =>
                                {
                                    reddePaymentService.PersistReddeUssdData($"{msgProceed}-{ussdRequest.Mobile}");

                                });
                            }

                            return Json(response_proceed);
                        }
                        catch
                        {
                            var response = ussdResponse["drawMessage"];
                            response.ClientState = ussdRequest.ClientState;
                            response.Message = "Invalid Input\n" + response.Message;
                            return Json(response);
                        }

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

        #region Slydepay
        //async Task PersistSlydepayUssdData(string requestString)
        //{
        //    //types : lkm,bus,sch
        //    var match = Regex.Match(requestString, @"(?<type>\w+)-(?<amount>\d+)-(?<period>\w+)-(?<mobilenumber>\w+)-(?<momonumber>\w+)");
        //    var type = match.Groups["type"].ToString();
        //    var amount = Convert.ToDecimal(match.Groups["amount"].ToString());
        //    var period = match.Groups["period"].ToString();
        //    var mobileNumber = "0" + Misc.NormalizePhoneNumber(match.Groups["mobilenumber"].ToString());
        //    var momoNumber = "0" + Misc.NormalizePhoneNumber(match.Groups["momonumber"].ToString());
        //    var user = await getMatchedUser(mobileNumber);

        //    var txRef = Misc.getTxRef(mobileNumber);
        //    IStakeType stakeType = null;
        //    EntityTypes entityType;



        //    if (type == "lkm")
        //    {
        //        entityType = EntityTypes.Luckyme;
        //        stakeType = new LuckyMe
        //        {
        //            Amount = amount,
        //            Date = DateTime.Now.ToLongDateString(),
        //            AmountToWin = amount * Constants.LuckymeStakeOdds,
        //            Status = "pending",
        //            TxRef = txRef,
        //            Period = period,
        //            User = user
        //        };

        //        dbContext.LuckyMes.Add(stakeType as LuckyMe);

        //    }
        //    else if (type == "bus")
        //    {
        //        entityType = EntityTypes.Business;
        //        stakeType = new Business
        //        {
        //            Amount = amount,
        //            Date = DateTime.Now.ToLongDateString(),
        //            AmountToWin = amount * Constants.BusinessStakeOdds,
        //            Status = "pending",
        //            TxRef = txRef,
        //            Period = period,
        //            User = user
        //        };

        //        dbContext.Businesses.Add(stakeType as Business);

        //    }


        //    await dbContext.SaveChangesAsync();

        //    try
        //    {
        //        var token = await Misc.GenerateAndSendSlydePayMomoInvoice(EntityTypes.Scholarship, stakeType, Settings.SlydePaySettings, Misc.FormatGhanaianPhoneNumberWp(momoNumber));
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //async Task PersistScholarshipSlydepayUssdData(string requestString)
        //{
        //    //types : lkm,bus,sch
        //    var regex = new Regex(@"^(?<type>\w+)\*(?<amount>\d+)-(?<period>\w+)-(?<institution>[\w\s]+)-(?<program>[\w\s]+)-(?<studentid>[\w\s]+)-(?<playertype>\d+)-(?<usernumber>\d+)-(?<momonumber>\d+)$").Match(requestString);
        //    var type = regex.Groups["type"].ToString();
        //    var amount = regex.Groups["amount"].ToString();
        //    var period = regex.Groups["period"].ToString();
        //    var institution = regex.Groups["institution"].ToString();
        //    var program = regex.Groups["program"].ToString();
        //    var studentId = regex.Groups["studentid"].ToString();
        //    var playerType = regex.Groups["playertype"].ToString();
        //    var mobileNumber = "0" + Misc.NormalizePhoneNumber(regex.Groups["usernumber"].ToString());
        //    var momoNumber = "0" + Misc.NormalizePhoneNumber(regex.Groups["momonumber"].ToString());


        //    var user = await getMatchedUser(mobileNumber);
        //    var txRef = Misc.getTxRef(mobileNumber);

        //    var scholarship = new Scholarship
        //    {
        //        Amount = Convert.ToDecimal(amount),
        //        Date = DateTime.Now.ToLongDateString(),
        //        AmountToWin = Convert.ToDecimal(amount) * Constants.ScholarshipStakeOdds,
        //        Status = "pending",
        //        TxRef = txRef,
        //        Period = period,
        //        User = user,
        //        Institution = institution,
        //        Program = program,
        //        StudentId = studentId,
        //        PlayerType = getPlayerType(playerType)
        //    };

        //    dbContext.Scholarships.Add(scholarship);


        //    await dbContext.SaveChangesAsync();

        //    try
        //    {
        //        var token = await Misc.GenerateAndSendSlydePayMomoInvoice(EntityTypes.Scholarship, scholarship, Settings.SlydePaySettings, Misc.FormatGhanaianPhoneNumberWp(momoNumber));

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        #endregion

        #region Redded
        //async Task PersistReddeUssdData(string requestString)
        //{
        //    //types : lkm,bus,sch
        //    var match = Regex.Match(requestString, @"(?<type>\w+)-(?<amount>\d+)-(?<period>\w+)-(?<momonumber>\w+)-(?<voucher>[\w\s]+)-(?<mobilenumber>\w+)");
        //    var type = match.Groups["type"].ToString();
        //    var amount = Convert.ToDecimal(match.Groups["amount"].ToString());
        //    var period = match.Groups["period"].ToString();
        //    var momoNumber = "0" + Misc.NormalizePhoneNumber(match.Groups["momonumber"].ToString());
        //    var voucher = match.Groups["voucher"].ToString();
        //    var mobileNumber = "0" + Misc.NormalizePhoneNumber(match.Groups["mobilenumber"].ToString());
        //    var user = await getMatchedUser(mobileNumber);

        //    var txRef = Misc.getTxRef(mobileNumber);
        //    IStakeType stakeType = null;
        //    EntityTypes entityType;



        //    if (type == "lkm")
        //    {
        //        entityType = EntityTypes.Luckyme;
        //        stakeType = new LuckyMe
        //        {
        //            Amount = amount,
        //            Date = DateTime.Now.ToLongDateString(),
        //            AmountToWin = amount * Constants.LuckymeStakeOdds,
        //            Status = "pending",
        //            TxRef = txRef,
        //            Period = period,
        //            User = user
        //        };

        //        dbContext.LuckyMes.Add(stakeType as LuckyMe);

        //    }
        //    else if (type == "bus")
        //    {
        //        entityType = EntityTypes.Business;
        //        stakeType = new Business
        //        {
        //            Amount = amount,
        //            Date = DateTime.Now.ToLongDateString(),
        //            AmountToWin = amount * Constants.BusinessStakeOdds,
        //            Status = "pending",
        //            TxRef = txRef,
        //            Period = period,
        //            User = user
        //        };

        //        dbContext.Businesses.Add(stakeType as Business);

        //    }


        //    await dbContext.SaveChangesAsync();

        //    try
        //    {
        //        var transactionId = await Misc.GenerateAndSendReddeMomoInvoice(EntityTypes.Scholarship, stakeType, Settings.ReddeSettings, $"{Misc.FormatGhanaianPhoneNumberWp(momoNumber)}*{voucher}");
        //        if (type == "lkm")
        //        {
        //            var luckyMe = stakeType as LuckyMe;
        //            luckyMe.TransferId = transactionId;
        //            dbContext.Entry(luckyMe).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        //            await dbContext.SaveChangesAsync();
        //        }
        //        else if (type == "bus")
        //        {
        //            var business = stakeType as Business;
        //            business.TransferId = transactionId;
        //            dbContext.Entry(business).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        //            await dbContext.SaveChangesAsync();
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //async Task PersistScholarshipReddeUssdData(string requestString)
        //{
        //    //types : lkm,bus,sch
        //    var regex = new Regex(@"^(?<type>\w+)\*(?<amount>\d+)-(?<period>\w+)-(?<institution>[\w\s]+)-(?<program>[\w\s]+)-(?<studentid>[\w\s]+)-(?<playertype>\d+)-(?<momonumber>\d+)-(?<voucher>[\w\s]+)-(?<usernumber>\d+)$").Match(requestString);
        //    var type = regex.Groups["type"].ToString();
        //    var amount = regex.Groups["amount"].ToString();
        //    var period = regex.Groups["period"].ToString();
        //    var institution = regex.Groups["institution"].ToString();
        //    var program = regex.Groups["program"].ToString();
        //    var studentId = regex.Groups["studentid"].ToString();
        //    var playerType = regex.Groups["playertype"].ToString();
        //    var voucher = regex.Groups["voucher"].ToString();
        //    var mobileNumber = "0" + Misc.NormalizePhoneNumber(regex.Groups["usernumber"].ToString());
        //    var momoNumber = "0" + Misc.NormalizePhoneNumber(regex.Groups["momonumber"].ToString());


        //    var user = await getMatchedUser(mobileNumber);
        //    var txRef = Misc.getTxRef(mobileNumber);

        //    var scholarship = new Scholarship
        //    {
        //        Amount = Convert.ToDecimal(amount),
        //        Date = DateTime.Now.ToLongDateString(),
        //        AmountToWin = Convert.ToDecimal(amount) * Constants.ScholarshipStakeOdds,
        //        Status = "pending",
        //        TxRef = txRef,
        //        Period = period,
        //        User = user,
        //        Institution = institution,
        //        Program = program,
        //        StudentId = studentId,
        //        PlayerType = getPlayerType(playerType)
        //    };




        //    try
        //    {
        //        var transactionId = await Misc.GenerateAndSendReddeMomoInvoice(EntityTypes.Scholarship, scholarship, Settings.ReddeSettings, $"{Misc.FormatGhanaianPhoneNumberWp(momoNumber)}*{voucher}");
        //        scholarship.TransferId = transactionId;
        //        dbContext.Scholarships.Add(scholarship);
        //        await dbContext.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        #endregion

        #region Rave
        //async Task PersistRaveUssdData(string requestString)
        //{
        //    //types : lkm,bus,sch
        //    var match = Regex.Match(requestString, @"(?<type>\w+)-(?<amount>\d+)-(?<period>\w+)-(?<mobilenumber>\w+)-(?<momonumber>\w+)");
        //    var type = match.Groups["type"].ToString();
        //    var amount = Convert.ToDecimal(match.Groups["amount"].ToString());
        //    var period = match.Groups["period"].ToString();
        //    var mobileNumber = "0" + Misc.NormalizePhoneNumber(match.Groups["mobilenumber"].ToString());
        //    var momoNumber = "0" + Misc.NormalizePhoneNumber(match.Groups["momonumber"].ToString());
        //    var user = await getMatchedUser(mobileNumber);

        //    var raveRequest = new RaveRequestDTO()
        //    {
        //        amount = amount.ToString(),
        //        txRef = Misc.getTxRef(mobileNumber),
        //        orderRef = getOrderRef(mobileNumber),
        //        email = user.Email,
        //        currency = "GHS",
        //        PBFPubKey = Settings.FlatterWaveSettings.GetPublicApiKey(),
        //        phonenumber = momoNumber,
        //        is_mobile_money_gh = 1,
        //        country = "GH",
        //        payment_type = "mobilemoneygh",
        //        network = Misc.getNetwork(momoNumber),
        //        firstname = user.FirstName,
        //        lastname = user.LastName
        //    };


        //    if (type == "lkm")
        //    {

        //        var luckyme = new LuckyMe
        //        {
        //            Amount = amount,
        //            Date = DateTime.Now.ToLongDateString(),
        //            AmountToWin = amount * Constants.LuckymeStakeOdds,
        //            Status = "pending",
        //            TxRef = raveRequest.txRef,
        //            Period = period,
        //            User = user
        //        };

        //        dbContext.LuckyMes.Add(luckyme);

        //    }
        //    else if (type == "bus")
        //    {
        //        var business = new Business
        //        {
        //            Amount = amount,
        //            Date = DateTime.Now.ToLongDateString(),
        //            AmountToWin = amount * Constants.BusinessStakeOdds,
        //            Status = "pending",
        //            TxRef = raveRequest.txRef,
        //            Period = period,
        //            User = user
        //        };

        //        dbContext.Businesses.Add(business);

        //    }
        //    else if (type == "sch")
        //    {
        //        var scholarship = new Scholarship
        //        {
        //            Amount = amount,
        //            Date = DateTime.Now.ToLongDateString(),
        //            AmountToWin = amount * Constants.ScholarshipStakeOdds,
        //            Status = "pending",
        //            TxRef = raveRequest.txRef,
        //            Period = period,
        //            User = user
        //        };

        //        dbContext.Scholarships.Add(scholarship);

        //    }

        //    await dbContext.SaveChangesAsync();

        //    var httpClient = new HttpClient();

        //    try
        //    {
        //        var data = JsonConvert.SerializeObject(raveRequest, Misc.getDefaultResolverJsonSettings());
        //        var encryptionKey = RaveEncryption.GetEncryptionKey(Settings.FlatterWaveSettings.GetApiSecret());
        //        var encryptedRequest = RaveEncryption.EncryptData(encryptionKey, data);
        //        var payloadData = new
        //        {
        //            PBFPubKey = Settings.FlatterWaveSettings.GetPublicApiKey(),
        //            client = encryptedRequest,
        //            alg = "3DES-24"
        //        };
        //        var payload = JsonConvert.SerializeObject(payloadData, Misc.getDefaultResolverJsonSettings());
        //        var stringContent = new StringContent(payload);
        //        stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        //        Task.Run(() =>
        //        {
        //            var m = httpClient.PostAsync("https://api.ravepay.co/flwv3-pug/getpaidx/api/charge", stringContent).Result;
        //        });

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //async Task PersistScholarshipRaveUssdData(string requestString)
        //{
        //    //types : lkm,bus,sch
        //    var regex = new Regex(@"^(?<type>\w+)\*(?<amount>\d+)-(?<period>\w+)-(?<institution>[\w\s]+)-(?<program>[\w\s]+)-(?<studentid>[\w\s]+)-(?<playertype>\d+)-(?<usernumber>\d+)-(?<momonumber>\d+)$").Match(requestString);
        //    var type = regex.Groups["type"].ToString();
        //    var amount = regex.Groups["amount"].ToString();
        //    var period = regex.Groups["period"].ToString();
        //    var institution = regex.Groups["institution"].ToString();
        //    var program = regex.Groups["program"].ToString();
        //    var studentId = regex.Groups["studentid"].ToString();
        //    var playerType = regex.Groups["playertype"].ToString();
        //    var mobileNumber = "0" + Misc.NormalizePhoneNumber(regex.Groups["usernumber"].ToString());
        //    var momoNumber = "0" + Misc.NormalizePhoneNumber(regex.Groups["momonumber"].ToString());


        //    var user = await getMatchedUser(mobileNumber);

        //    var raveRequest = new RaveRequestDTO()
        //    {
        //        amount = amount.ToString(),
        //        txRef = Misc.getTxRef(mobileNumber),
        //        orderRef = getOrderRef(mobileNumber),
        //        email = user.Email,
        //        currency = "GHS",
        //        PBFPubKey = Settings.FlatterWaveSettings.GetPublicApiKey(),
        //        phonenumber = momoNumber,
        //        is_mobile_money_gh = 1,
        //        country = "GH",
        //        payment_type = "mobilemoneygh",
        //        network = Misc.getNetwork(momoNumber),
        //        firstname = user.FirstName,
        //        lastname = user.LastName
        //    };

        //    var scholarship = new Scholarship
        //    {
        //        Amount = Convert.ToDecimal(amount),
        //        Date = DateTime.Now.ToLongDateString(),
        //        AmountToWin = Convert.ToDecimal(amount) * Constants.ScholarshipStakeOdds,
        //        Status = "pending",
        //        TxRef = raveRequest.txRef,
        //        Period = period,
        //        User = user,
        //        Institution = institution,
        //        Program = program,
        //        StudentId = studentId,
        //        PlayerType = getPlayerType(playerType)
        //    };

        //    dbContext.Scholarships.Add(scholarship);


        //    await dbContext.SaveChangesAsync();

        //    var httpClient = new HttpClient();

        //    try
        //    {
        //        var data = JsonConvert.SerializeObject(raveRequest, Misc.getDefaultResolverJsonSettings());
        //        var encryptionKey = RaveEncryption.GetEncryptionKey(Settings.FlatterWaveSettings.GetApiSecret());
        //        var encryptedRequest = RaveEncryption.EncryptData(encryptionKey, data);
        //        var payloadData = new
        //        {
        //            PBFPubKey = Settings.FlatterWaveSettings.GetPublicApiKey(),
        //            client = encryptedRequest,
        //            alg = "3DES-24"
        //        };
        //        var payload = JsonConvert.SerializeObject(payloadData, Misc.getDefaultResolverJsonSettings());
        //        var stringContent = new StringContent(payload);
        //        stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        //        Task.Run(() =>
        //        {
        //            var m = httpClient.PostAsync("https://api.ravepay.co/flwv3-pug/getpaidx/api/charge", stringContent).Result;
        //        });

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        #endregion


        /// <summary>
        /// Get the steps to take before approving the momo transaction
        /// </summary>
        /// <param name="network"></param>
        /// <returns></returns>
        string getMomoApprovalDirections(string network)
        {
            StringBuilder sBuilder = new StringBuilder();
            if (network == "MTN")
                return sBuilder.AppendLine("Dial *170#\nChoose option: 6) Wallet\nChoose option: 3) My Approvals\nEnter your MOMO pin to retrieve your pending approval list\nChoose a pending transaction\nChoose option 1 to approve\nTap button to continue").ToString();
            if (network == "VODAFONE")
                return sBuilder.AppendLine("Dial *110# to generate your transaction voucher.\nSelect option) 6 to generate the voucher.\nEnter your PIN in next prompt.\nInput the voucher generated in the payment modal.\n").ToString();
            if (network == "TIGO")
                return sBuilder.AppendLine("Dial 501*5# to approve your transaction.\nSelect the transaction to approve and click on send.\nSelect YES to confirm your payment.\n").ToString();

            return null;
        }

        string getLiveStakesForUser(string phoneNumber)
        {
            ApplicationUser user = dbContext.Users.Where(i => i.PhoneNumber == phoneNumber).FirstOrDefault();
            var luckyMes = dbContext.LuckyMes.Where(i => i.Status.ToLower() == "paid" && i.User.PhoneNumber == phoneNumber);
            var businesses = dbContext.Businesses.Where(i => i.Status.ToLower() == "paid" && i.User.PhoneNumber == phoneNumber);
            var scholarships = dbContext.Scholarships.Where(i => i.Status.ToLower() == "paid" && i.User.PhoneNumber == phoneNumber);
            StringBuilder sbuilder = new StringBuilder();

            foreach (var item in luckyMes)
            {
                sbuilder.AppendLine($"LuckyMe {(int)item.Amount} Cedi(s) {item.Period}");
            }
            foreach (var item in businesses)
            {
                sbuilder.AppendLine($"Business {(int)item.Amount} Cedi(s)");
            }
            foreach (var item in scholarships)
            {
                sbuilder.AppendLine($"Scholarhip {(int)item.Amount} Cedi(s)");
            }

            if (string.IsNullOrEmpty(sbuilder.ToString()))
            {

                sbuilder.AppendLine("You have no live investments");
            }
            sbuilder.Insert(0, $"You have {user.Points} Points\n");

            return sbuilder.ToString();
        }


        string getOrderRef(string phoneNumber)
        {
            var match = Regex.Match(phoneNumber, @"^(\w{2}).*(\w{2})$");

            var userCode = match.Groups[1].ToString() + match.Groups[2].ToString();
            var timeStamp = DateTime.Now.TimeOfDay.ToString();
            return $"inv_{ userCode}_{timeStamp}";
        }

        //async Task<ApplicationUser> getMatchedUser(string phoneNumber)
        //{
        //    phoneNumber = "0" + Misc.NormalizePhoneNumber(phoneNumber);

        //    var user = dbContext.Users.FirstOrDefault(i => Misc.NormalizePhoneNumber(i.PhoneNumber) == Misc.NormalizePhoneNumber(phoneNumber));

        //    if (user != null)
        //        return user;

        //    var regDTO = new RegistrationDTO
        //    {
        //        FirstName = phoneNumber,
        //        LastName = "",
        //        PhoneNumber = phoneNumber,
        //        Email = $"ntoboafund.{phoneNumber}@gmail.com",
        //        Password = phoneNumber,
        //        ConfirmPassword = phoneNumber
        //    };
        //    var result = await UserService.Register(regDTO, true);
        //    //dbContext.Users.Add(user);
        //    //await dbContext.SaveChangesAsync();

        //    return result.Item1;
        //}

        string getPlayerType(string index)
        {
            if (index == "1")
                return "student";
            else if (index == "2")
                return "parent";

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns>MTN or VODAFONE or TIGO</returns>
        JsonResult Json(object result)
        {
            return new JsonResult(result, Misc.getDefaultResolverJsonSettings());
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