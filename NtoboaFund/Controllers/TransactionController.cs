using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NtoboaFund.Data;
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
        static readonly object locker = new object();
        public MessagingService MessagingService { get; set; }

        public IHubContext<StakersHub> StakersHub { get; }

        public DummyService DummyService { get; set; }

        public StakersHub DataHub { get; set; }

        public  TransactionController(IOptions<AppSettings> appSettings,
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

        /// <summary>
        /// Makes Luckyme Payment On Slide Pay
        /// </summary>
        /// <param name="txRef">The reference of that transaction</param>
        /// <param name="paymentType">The type of payment as a string</param>
        /// <param name="momoStarVoucherOrEmail">The Mobile Money Number to be used for the transaction</param>
        /// <returns></returns>
        [HttpPost("payforluckyme/{txRef}/{paymentType}/{momoStarVoucherOrEmail}/{isredirected}")]
        public async Task<IActionResult> PayForLuckyMe(string txRef, string paymentType, string momoStarVoucherOrEmail, bool isredirected = false)
        {
            var luckyMe = dbContext.LuckyMes.Where(i => i.TxRef == txRef).Include("User").FirstOrDefault();
            if (luckyMe == null)
            {
                return BadRequest(new { message = "This Lucky Me Transaction Was Not Found" });
            }
            string paymentToken = null;

            if (paymentType == "MobileMoney" && momoStarVoucherOrEmail != null)
            {
                if (Constants.PaymentGateway == PaymentGateway.slydepay)
                {
                    paymentToken = await Misc.GenerateAndSendSlydePayMomoInvoice(EntityTypes.Luckyme, luckyMe, AppSettings.SlydePaySettings, momoStarVoucherOrEmail);
                }
                else if (Constants.PaymentGateway == PaymentGateway.redde)
                {
                    var payResults = await Misc.GenerateAndSendReddeMomoInvoice(EntityTypes.Luckyme, luckyMe, AppSettings.ReddeSettings, momoStarVoucherOrEmail);
                    paymentToken = payResults.ToString();
                    luckyMe.TransferId = payResults;
                }


            }
            else if (paymentType == "CreditCard")
            {
                if (Constants.PaymentGateway == PaymentGateway.slydepay)
                {
                    if (!isredirected)
                        paymentToken = await Misc.GenerateAndSendSlydePayCardInvoice(EntityTypes.Luckyme, luckyMe, AppSettings.SlydePaySettings, momoStarVoucherOrEmail);
                    else
                        paymentToken = await Misc.GenerateSlydePayToken(EntityTypes.Luckyme, luckyMe, AppSettings.SlydePaySettings);

                }
                else if (Constants.PaymentGateway == PaymentGateway.redde)
                {
                    if (!isredirected)
                    {
                        // paymentToken = await Misc.GenerateAndSendSlydePayCardInvoice(EntityTypes.Luckyme, luckyMe, AppSettings.SlydePaySettings, momoStarVoucherOrEmail);

                    }
                    else
                    {
                        var payResults = await Misc.GenerateReddeToken(EntityTypes.Luckyme, luckyMe, AppSettings.ReddeSettings);
                        paymentToken = payResults.token;
                        luckyMe.TransferId = payResults.checkoutransId;
                    }

                }

            }
          
            else if (paymentType == "SlydePay" && Constants.PaymentGateway == PaymentGateway.slydepay)
                paymentToken = await Misc.GenerateAndSendSlydePayAnkasaInvoice(EntityTypes.Luckyme, luckyMe, AppSettings.SlydePaySettings);




            if (paymentToken == null)
                return BadRequest(new { message = "Payment Failed" });
            //else
            //    await Misc.ConfirmSlydePayTransaction(EntityTypes.Luckyme, luckyMe, AppSettings.SlydePaySettings);

            dbContext.Entry(luckyMe).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();

            return Ok(new { txRef, paymentToken });
        }

        [HttpPost("payforbusiness/{txRef}/{paymentType}/{momoStarVoucherOrEmail}/{isredirected}")]
        public async Task<IActionResult> PayForBusiness(string txRef, string paymentType, string momoStarVoucherOrEmail, bool isredirected = false)
        {
            var business = dbContext.Businesses.Where(i => i.TxRef == txRef).Include("User").FirstOrDefault();
            if (business == null)
            {
                return BadRequest(new { message = "This Business Me Transaction Was Not Found" });
            }
            string paymentToken = null;

            if (paymentType == "MobileMoney" && momoStarVoucherOrEmail != null)
            {
                if (Constants.PaymentGateway == PaymentGateway.slydepay)
                    paymentToken = await Misc.GenerateAndSendSlydePayMomoInvoice(EntityTypes.Business, business, AppSettings.SlydePaySettings, momoStarVoucherOrEmail);
                else if (Constants.PaymentGateway == PaymentGateway.redde)
                {
                    var payResult = await Misc.GenerateAndSendReddeMomoInvoice(EntityTypes.Business, business, AppSettings.ReddeSettings, momoStarVoucherOrEmail);
                    paymentToken = paymentToken.ToString();
                    business.TransferId = payResult;
                }

            }
            else if (paymentType == "CreditCard")
            {
                if (Constants.PaymentGateway == PaymentGateway.slydepay)
                {
                    if (!isredirected)
                        paymentToken = await Misc.GenerateAndSendSlydePayCardInvoice(EntityTypes.Business, business, AppSettings.SlydePaySettings, momoStarVoucherOrEmail);
                    else
                        paymentToken = await Misc.GenerateSlydePayToken(EntityTypes.Business, business, AppSettings.SlydePaySettings);

                }
                else if (Constants.PaymentGateway == PaymentGateway.redde)
                {
                    if (!isredirected) { }
                    //paymentToken = await Misc.GenerateAndSendSlydePayCardInvoice(EntityTypes.Business, business, AppSettings.SlydePaySettings, momoStarVoucherOrEmail);
                    else
                    {
                        var payResult = await Misc.GenerateReddeToken(EntityTypes.Business, business, AppSettings.ReddeSettings);
                        paymentToken = payResult.token;
                        business.TransferId = payResult.checkoutransId;
                    }
                }

            }
            else if (paymentType == "SlydePay" && Constants.PaymentGateway == PaymentGateway.slydepay)
                paymentToken = await Misc.GenerateAndSendSlydePayAnkasaInvoice(EntityTypes.Business, business, AppSettings.SlydePaySettings);

            if (paymentToken == null)
                return BadRequest(new { message = "Payment Failed" });
            //else
            //    await Misc.ConfirmSlydePayTransaction(EntityTypes.Business, business, AppSettings.SlydePaySettings);
            dbContext.Entry(business).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();


            return Ok(new { txRef, paymentToken });
        }

        [HttpPost("payforscholarship/{txRef}/{paymentType}/{momoStarVoucherOrEmail}/{isredirected}")]
        public async Task<IActionResult> PayForScholarship(string txRef, string paymentType, string momoStarVoucherOrEmail, bool isredirected = false)
        {
            var scholarship = dbContext.Scholarships.Where(i => i.TxRef == txRef).Include("User").FirstOrDefault();
            if (scholarship == null)
            {
                return BadRequest(new { message = "This Scholarship Transaction Was Not Found" });
            }
            string paymentToken = null;

            if (paymentType == "MobileMoney" && momoStarVoucherOrEmail != null)
            {
                if (Constants.PaymentGateway == PaymentGateway.slydepay)
                    paymentToken = await Misc.GenerateAndSendSlydePayMomoInvoice(EntityTypes.Scholarship, scholarship, AppSettings.SlydePaySettings, momoStarVoucherOrEmail);
                else if (Constants.PaymentGateway == PaymentGateway.redde)
                {
                    var payResult = await Misc.GenerateAndSendReddeMomoInvoice(EntityTypes.Scholarship, scholarship, AppSettings.ReddeSettings, momoStarVoucherOrEmail);
                    paymentToken = paymentToken.ToString();
                    scholarship.TransferId = payResult;
                }

            }
            else if (paymentType == "CreditCard")
            {
                if (Constants.PaymentGateway == PaymentGateway.slydepay)
                {
                    if (!isredirected)
                        paymentToken = await Misc.GenerateAndSendSlydePayCardInvoice(EntityTypes.Scholarship, scholarship, AppSettings.SlydePaySettings, momoStarVoucherOrEmail);
                    else
                        paymentToken = await Misc.GenerateSlydePayToken(EntityTypes.Scholarship, scholarship, AppSettings.SlydePaySettings);

                }
                else if (Constants.PaymentGateway == PaymentGateway.redde)
                {
                    if (!isredirected) { }
                    //paymentToken = await Misc.GenerateAndSendSlydePayCardInvoice(EntityTypes.Scholarship, scholarship, AppSettings.SlydePaySettings, momoStarVoucherOrEmail);
                    else
                    {
                        var payResult = await Misc.GenerateReddeToken(EntityTypes.Scholarship, scholarship, AppSettings.ReddeSettings);
                        paymentToken = payResult.token;
                        scholarship.TransferId = payResult.checkoutransId;
                    }

                }
            }
            else if (paymentType == "SlydePay" && Constants.PaymentGateway == PaymentGateway.slydepay)
                paymentToken = await Misc.GenerateAndSendSlydePayAnkasaInvoice(EntityTypes.Scholarship, scholarship, AppSettings.SlydePaySettings);

            if (paymentToken == null)
                return BadRequest(new { message = "Payment Failed" });
            //else
            //    await Misc.ConfirmSlydePayTransaction(EntityTypes.Scholarship, scholarship, AppSettings.SlydePaySettings);

            dbContext.Entry(scholarship).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();

            return Ok(new { txRef, paymentToken });
        }


        /// <summary>
        /// Verifies a payment transaction
        /// </summary>
        /// <param name="txRef">The Unique reference of the transaction</param>
        /// <param name="paymentType">The type of payment momo or card</param>
        /// <param name="status">An already known status of the transaction</param>
        /// <returns>An Http Status Code</returns>
        [HttpPost("verifyLuckymePayment/{txRef}")]
        public async Task<IActionResult> VerifyLuckymePayment(string txRef, string paymentType = null, string status = null /*, [FromBody]LuckyMe luckyMe*/)
        {
            lock (locker)
            {
                var luckyMe = dbContext.LuckyMes.Where(i => i.TxRef == txRef).FirstOrDefault();
                ApplicationUser luckyMeUser = null;
                if (luckyMe == null)
                {
                    return BadRequest(new { status = "", errorString = "luckyMe stake was not found" });
                }
                else if (luckyMe.Status.ToLower() == "paid" || luckyMe.Status.ToLower() == "won" || luckyMe.Status.ToLower() == "wins")
                {
                    return Ok(new { luckyMe.Status, message = "LuckyMe Already Verified" });
                }

                luckyMeUser = dbContext.Users.Find(luckyMe.UserId);

                string errorString = null;

                try
                {
                    var VResult = VerifyPayment(luckyMe.TransferId, paymentType, status).Result;
                    if (VResult.isConfirmed)
                    {
                        if (luckyMe.Status.ToLower() == "paid")
                            return Ok(new { luckyMe, errorString });

                        luckyMe.Status = "paid";

                        luckyMeUser.Points += luckyMe.Amount * Constants.PointConstant;
                        luckyMe.DateDeclared = Misc.GetDrawDate(EntityTypes.Luckyme, luckyMe.Period);
                        if (Constants.PaymentGateway == PaymentGateway.slydepay)
                        {
                            Task.Run(async () =>
                        {
                            await Misc.ConfirmSlydePayTransaction(EntityTypes.Luckyme, luckyMe, AppSettings.SlydePaySettings);

                        });
                        }
                    }
                    else
                    {
                        if (VResult.message == "PENDING" || VResult.message == "PROGRESS" || VResult.message == "NEW")
                        {

                        }
                        else
                            luckyMe.Status = "failed";
                    }
                    dbContext.Entry(luckyMeUser).State = EntityState.Modified;
                    dbContext.Entry(luckyMe).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    if (luckyMeUser != null && luckyMe.Status.ToLower() == "paid") //send the currently added participant to all clients
                    {
                        if (luckyMe.Period.ToLower() == "daily")
                        {
                            //Send an Email to the User
                            MessagingService.SendMail(luckyMeUser.FirstName + luckyMeUser.LastName, luckyMeUser.Email, "Successful Ntoboa Investment", Misc.GetDrawMessage(EntityTypes.Luckyme, luckyMe.Amount, luckyMe.Period));

                            //Send as sms to the user
                            MessagingService.SendSms(luckyMeUser.PhoneNumber, Misc.GetDrawMessage(EntityTypes.Luckyme, luckyMe.Amount, luckyMe.Period));

                            StakersHub.Clients.All.SendAsync("adddailyluckymeparticipant",
                           new LuckyMeParticipantDTO
                           {
                               Id = luckyMe.Id,
                               UserId = luckyMeUser.Id,
                               UserName = luckyMeUser.FirstName + " " + luckyMeUser.LastName,
                               AmountStaked = luckyMe.Amount.ToString("0.##"),
                               AmountToWin = luckyMe.AmountToWin.ToString("0.##"),
                               Status = luckyMe.Status.ToLower(),
                               DateDeclared = luckyMe.DateDeclared,
                               TxRef = luckyMe.TxRef
                           });

                            foreach (var dummy in DataHub.ManageDummies(EntityTypes.Luckyme, Period.Daily))
                            {
                                var luckymeDailyDummy = dummy as LuckyMe;
                                StakersHub.Clients.All.SendAsync("adddailyluckymeparticipant",
                                new LuckyMeParticipantDTO
                                {
                                    Id = luckymeDailyDummy.Id,
                                    UserId = luckymeDailyDummy.User.Id,
                                    UserName = luckymeDailyDummy.User.FirstName + " " + luckymeDailyDummy.User.LastName,
                                    AmountStaked = luckymeDailyDummy.Amount.ToString("0.##"),
                                    AmountToWin = luckymeDailyDummy.AmountToWin.ToString("0.##"),
                                    Status = luckymeDailyDummy.Status.ToLower(),
                                    DateDeclared = luckymeDailyDummy.DateDeclared,
                                    TxRef = luckymeDailyDummy.TxRef
                                });
                            }
                        }
                        else if (luckyMe.Period.ToLower() == "weekly")
                        {
                            //Send as sms to the user
                            MessagingService.SendMail(luckyMeUser.FirstName + luckyMeUser.LastName, luckyMeUser.Email, "Successful Ntoboa Investment", Misc.GetDrawMessage(EntityTypes.Luckyme, luckyMe.Amount, luckyMe.Period));
                            MessagingService.SendSms(luckyMeUser.PhoneNumber, Misc.GetDrawMessage(EntityTypes.Luckyme, luckyMe.Amount, luckyMe.Period));


                            StakersHub.Clients.All.SendAsync("addweeklyluckymeparticipant",
                              new LuckyMeParticipantDTO
                              {
                                  Id = luckyMe.Id,
                                  UserId = luckyMeUser.Id,
                                  UserName = luckyMeUser.FirstName + " " + luckyMeUser.LastName,
                                  AmountStaked = luckyMe.Amount.ToString("0.##"),
                                  AmountToWin = luckyMe.AmountToWin.ToString("0.##"),
                                  Status = luckyMe.Status.ToLower(),
                                  DateDeclared = luckyMe.DateDeclared,
                                  TxRef = luckyMe.TxRef
                              });

                            //InsertLuckyMe dummies and Propagate them to user
                            foreach (var dummy in DataHub.ManageDummies(EntityTypes.Luckyme, Period.Weekly))
                            {
                                var luckymeWeeklyDummy = dummy as LuckyMe;
                                StakersHub.Clients.All.SendAsync("addweeklyluckymeparticipant",
                               new LuckyMeParticipantDTO
                               {
                                   Id = luckymeWeeklyDummy.Id,
                                   UserId = luckymeWeeklyDummy.User.Id,
                                   UserName = luckymeWeeklyDummy.User.FirstName + " " + luckymeWeeklyDummy.User.LastName,
                                   AmountStaked = luckymeWeeklyDummy.Amount.ToString("0.##"),
                                   AmountToWin = luckymeWeeklyDummy.AmountToWin.ToString("0.##"),
                                   Status = luckymeWeeklyDummy.Status.ToLower(),
                                   DateDeclared = luckymeWeeklyDummy.DateDeclared,
                                   TxRef = luckymeWeeklyDummy.TxRef

                               });
                            }

                        }
                        else if (luckyMe.Period.ToLower() == "monthly")
                        {
                            //Send an Email to the User
                            MessagingService.SendMail(luckyMeUser.FirstName + luckyMeUser.LastName, luckyMeUser.Email, "Successful Ntoboa Investment", Misc.GetDrawMessage(EntityTypes.Luckyme, luckyMe.Amount, luckyMe.Period));

                            //Send as sms to the user
                            MessagingService.SendSms(luckyMeUser.PhoneNumber, Misc.GetDrawMessage(EntityTypes.Luckyme, luckyMe.Amount, luckyMe.Period));

                            StakersHub.Clients.All.SendAsync("addmonthlyluckymeparticipant",
                              new LuckyMeParticipantDTO
                              {
                                  Id = luckyMe.Id,
                                  UserId = luckyMeUser.Id,
                                  UserName = luckyMeUser.FirstName + " " + luckyMeUser.LastName,
                                  AmountStaked = luckyMe.Amount.ToString("0.##"),
                                  AmountToWin = luckyMe.AmountToWin.ToString("0.##"),
                                  Status = luckyMe.Status.ToLower(),
                                  DateDeclared = luckyMe.DateDeclared,
                                  TxRef = luckyMe.TxRef
                              });
                            //InsertLuckyMe dummies and Propagate them to user
                            foreach (var dummy in DataHub.ManageDummies(EntityTypes.Luckyme, Period.Monthly))
                            {
                                var luckymeMonthlyDummy = dummy as LuckyMe;
                                StakersHub.Clients.All.SendAsync("addmonthlyluckymeparticipant",
                               new LuckyMeParticipantDTO
                               {
                                   Id = luckymeMonthlyDummy.Id,
                                   UserId = luckymeMonthlyDummy.User.Id,
                                   UserName = luckymeMonthlyDummy.User.FirstName + " " + luckymeMonthlyDummy.User.LastName,
                                   AmountStaked = luckymeMonthlyDummy.Amount.ToString("0.##"),
                                   AmountToWin = luckymeMonthlyDummy.AmountToWin.ToString("0.##"),
                                   Status = luckymeMonthlyDummy.Status.ToLower(),
                                   DateDeclared = luckymeMonthlyDummy.DateDeclared,
                                   TxRef = luckymeMonthlyDummy.TxRef

                               });
                            }

                        }

                    }


                }
                catch (Exception ex)
                {
                    errorString = ex.Message;
                }

                return Ok(new { luckyMe.Status, errorString });

            }
        }

        /// <summary>
        /// Verifies a payment transaction
        /// </summary>
        /// <param name="txRef">The Unique reference of the transaction</param>
        /// <param name="paymentType">The type of payment momo or card</param>
        /// <param name="status">An already known status of the transaction</param>
        /// <returns>An Http Status Code</returns>
        [HttpPost("verifyScholarshipPayment/{txRef}")]
        public async Task<IActionResult> VerifyScholarshipPayment(string txRef, string paymentType = null, string status = null/*, [FromBody]Scholarship scholarship*/)
        {
            lock (locker)
            {
                var scholarship = dbContext.Scholarships.Where(i => i.TxRef == txRef).FirstOrDefault();
                ApplicationUser scholarshipUser = null;
                if (scholarship == null)
                {
                    return BadRequest(new { status = "", errorString = "Scholarship stake was not found" });
                }
                else if (scholarship.Status.ToLower() == "paid" || scholarship.Status.ToLower() == "won" || scholarship.Status.ToLower() == "wins")
                {
                    return Ok(new { scholarship.Status, message = "Scholarship Already Verified" });
                }

                scholarshipUser = dbContext.Users.Find(scholarship.UserId);


                //string resultString = null;
                string errorString = null;

                try
                {
                    var VResult = VerifyPayment(scholarship.TransferId, paymentType, status).Result;
                    if (VResult.isConfirmed)
                    {
                        if (scholarship.Status.ToLower() == "paid")
                            return Ok(new { scholarship.Status, errorString });

                        scholarship.Status = "paid";
                        scholarshipUser.Points += scholarship.Amount * Constants.PointConstant;
                        scholarship.DateDeclared = Misc.GetDrawDate(EntityTypes.Scholarship, scholarship.Period);
                        if (Constants.PaymentGateway == PaymentGateway.slydepay)
                        {
                            Task.Run(async () =>
                            {
                                await Misc.ConfirmSlydePayTransaction(EntityTypes.Scholarship, scholarship, AppSettings.SlydePaySettings);

                            });
                        }
                    }
                    else
                    {
                        if (VResult.message == "PENDING" || VResult.message == "PROGRESS" || VResult.message == "NEW")
                        {

                        }
                        else
                            scholarship.Status = "failed";
                    }
                    dbContext.Entry(scholarshipUser).State = EntityState.Modified;
                    dbContext.Entry(scholarship).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    //Find the current user associated with the scholarship


                    //send the currently added participant to all clients
                    if (scholarshipUser != null && scholarship.Status.ToLower() == "paid")
                    {
                        //Send an Email to the User
                        MessagingService.SendMail(scholarshipUser.FirstName + scholarshipUser.LastName, scholarshipUser.Email, "Successful Ntoboa Investment", Misc.GetDrawMessage(EntityTypes.Scholarship, scholarship.Amount, scholarship.Period));

                        //Send as sms to the user
                        MessagingService.SendSms(scholarshipUser.PhoneNumber, Misc.GetDrawMessage(EntityTypes.Scholarship, scholarship.Amount, scholarship.Period));

                        StakersHub.Clients.All.SendAsync("addscholarshipparticipant",
                          new ScholarshipParticipantDTO
                          {
                              Id = scholarship.Id,
                              UserId = scholarshipUser.Id,
                              UserName = scholarshipUser.FirstName + " " + scholarshipUser.LastName,
                              AmountStaked = scholarship.Amount.ToString("0.##"),
                              AmountToWin = scholarship.AmountToWin.ToString("0.##"),
                              Status = scholarship.Status.ToLower(),
                              DateDeclared = scholarship.DateDeclared,
                              TxRef = scholarship.TxRef

                          });
                        //Insert Scholarship dummies and Propagate them to user
                        foreach (var dummy in DataHub.ManageDummies(EntityTypes.Scholarship, null))
                        {
                            var ScholarshipDummy = dummy as Scholarship;
                            StakersHub.Clients.All.SendAsync("addscholarshipparticipant",
                           new ScholarshipParticipantDTO
                           {
                               Id = ScholarshipDummy.Id,
                               UserId = ScholarshipDummy.User.Id,
                               UserName = ScholarshipDummy.User.FirstName + " " + ScholarshipDummy.User.LastName,
                               AmountStaked = ScholarshipDummy.Amount.ToString("0.##"),
                               AmountToWin = ScholarshipDummy.AmountToWin.ToString("0.##"),
                               Status = ScholarshipDummy.Status.ToLower(),
                               DateDeclared = ScholarshipDummy.DateDeclared,
                               TxRef = ScholarshipDummy.TxRef
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
                return Ok(new { scholarship.Status, errorString });
            }
        }

        /// <summary>
        /// Verifies a payment transaction
        /// </summary>
        /// <param name="txRef">The Unique reference of the transaction</param>
        /// <param name="paymentType">The type of payment momo or card</param>
        /// <param name="status">An already known status of the transaction</param>
        /// <returns>An Http Status Code</returns>
        [HttpPost("verifyBusinessPayment/{txRef}")]
        public async Task<IActionResult> VerifyBusinessPayment(string txRef, string paymentType = null, string status = null/*, [FromBody]Business business*/)
        {
            lock (locker)
            {
                var business = dbContext.Businesses.Where(i => i.TxRef == txRef).FirstOrDefault();
                ApplicationUser businessUser = null;
                if (business == null)
                {
                    return BadRequest(new { status = "", errorString = "Business stake was not found" });
                }
                else if (business.Status.ToLower() == "paid" || business.Status.ToLower() == "won" || business.Status.ToLower() == "wins")
                {
                    return Ok(new { business.Status, message = "Business Already Verified" });
                }

                businessUser = dbContext.Users.Find(business.UserId);


                // string resultString = null;
                string errorString = null;

                try
                {
                    var VResult = VerifyPayment(business.TransferId, paymentType, status).Result;
                    if (VResult.isConfirmed)
                    {
                        if (business.Status.ToLower() == "paid")
                            return Ok(new { business.Status, errorString });

                        business.Status = "paid";
                        businessUser.Points += business.Amount * Constants.PointConstant;
                        business.DateDeclared = Misc.GetDrawDate(EntityTypes.Business, business.Period);
                        if (Constants.PaymentGateway == PaymentGateway.slydepay)
                        {
                            Task.Run(async () =>
                            {
                                await Misc.ConfirmSlydePayTransaction(EntityTypes.Business, business, AppSettings.SlydePaySettings);

                            });
                        }

                    }
                    else
                    {
                        if (VResult.message == "PENDING" || VResult.message == "PROGRESS" || VResult.message == "NEW")
                        {

                        }
                        else
                            business.Status = "failed";
                    }


                    dbContext.Entry(businessUser).State = EntityState.Modified;
                    dbContext.Entry(business).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    //Find the current user associated with the business

                    if (businessUser != null && business.Status.ToLower() == "paid") //send the currently added participant to all clients
                    {
                        //Send an Email to the User
                        MessagingService.SendMail(businessUser.FirstName + businessUser.LastName, businessUser.Email, "Successful Ntoboa Investment", Misc.GetDrawMessage(EntityTypes.Business, business.Amount, business.Period));

                        //Send as sms to the user
                        MessagingService.SendSms(businessUser.PhoneNumber, Misc.GetDrawMessage(EntityTypes.Business, business.Amount, business.Period));

                        StakersHub.Clients.All.SendAsync("addbusinessparticipant",
                                new BusinessParticipantDTO
                                {
                                    Id = business.Id,
                                    UserId = businessUser.Id,
                                    UserName = businessUser.FirstName + " " + businessUser.LastName,
                                    AmountStaked = business.Amount.ToString("0.##"),
                                    AmountToWin = business.AmountToWin.ToString("0.##"),
                                    Status = business.Status.ToLower(),
                                    DateDeclared = business.DateDeclared,
                                    TxRef = business.TxRef
                                });

                        //Insert Business dummies and Propagate them to user
                        foreach (var dummy in DataHub.ManageDummies(EntityTypes.Business, null))
                        {
                            var BusinessDummy = dummy as Business;
                            StakersHub.Clients.All.SendAsync("addbusinessparticipant",
                           new BusinessParticipantDTO
                           {
                               Id = BusinessDummy.Id,
                               UserId = BusinessDummy.User.Id,
                               UserName = BusinessDummy.User.FirstName + " " + BusinessDummy.User.LastName,
                               AmountStaked = BusinessDummy.Amount.ToString("0.##"),
                               AmountToWin = BusinessDummy.AmountToWin.ToString("0.##"),
                               Status = BusinessDummy.Status.ToLower(),
                               DateDeclared = BusinessDummy.DateDeclared,
                               TxRef = BusinessDummy.TxRef
                           });
                        }

                    }
                    else if (business.Status.ToLower() != "paid")
                    {
                        errorString = "Payment Could not be verfied";
                    }

                    //resultString = GenerateHubtelUrl(business.Id, business.Amount, "business");
                }
                catch (Exception ex)
                {
                    errorString = ex.Message;
                }

                //  return Ok(hubtelresponse?.data?.checkoutUrl);
                return Ok(new { business.Status, errorString });
            }
        }


        [HttpPost("cancelluckymetransaction/{txRef}")]
        public async Task<IActionResult> CancelLuckymeTransaction(string txRef)
        {
            var luckyMe = dbContext.LuckyMes.Where(i => i.TxRef == txRef).Include("User").FirstOrDefault();

            if (luckyMe == null)
            {
                return BadRequest("Luckyme not Found");
            }
            if (luckyMe.Status != "paid")
            {
                luckyMe.Status = "failed";
                return Ok("CANCELLED");
            }
            else
            {
                var result = await Misc.CancelSlydePayTransaction(EntityTypes.Luckyme, luckyMe, AppSettings.SlydePaySettings);

                if (result.Result == "CANCELLED")
                    return Ok(result.Result);
                else
                    return BadRequest(result.ErrorCode);
            }


        }

        [HttpPost("cancelbusinesstransaction/{txRef}")]
        public async Task<IActionResult> CancelBusinessTransaction(string txRef)
        {
            var business = dbContext.Businesses.Where(i => i.TxRef == txRef).Include("User").FirstOrDefault();

            if (business == null)
            {
                return BadRequest("Business not Found");
            }
            if (business.Status != "paid")
            {
                business.Status = "failed";
                return Ok("CANCELLED");
            }
            else
            {
                var result = await Misc.CancelSlydePayTransaction(EntityTypes.Business, business, AppSettings.SlydePaySettings);

                if (result.Result == "CANCELLED")
                    return Ok(result.Result);
                else
                    return BadRequest(result.ErrorCode);
            }


        }

        [HttpPost("cancelscholarshiptransaction/{txRef}")]
        public async Task<IActionResult> CancelScholarshipTransaction(string txRef)
        {
            var scholarship = dbContext.Scholarships.Where(i => i.TxRef == txRef).Include("User").FirstOrDefault();

            if (scholarship == null)
            {
                return BadRequest("Scholarship not Found");
            }
            if (scholarship.Status != "paid")
            {
                scholarship.Status = "failed";
                return Ok("CANCELLED");
            }
            else
            {
                var result = await Misc.CancelSlydePayTransaction(EntityTypes.Scholarship, scholarship, AppSettings.SlydePaySettings);

                if (result.Result == "CANCELLED")
                    return Ok(result.Result);
                else
                    return BadRequest(result.ErrorCode);
            }


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
        public async Task<IActionResult> RaveWebHook(RaveWebhookCallback response)
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

        [HttpGet("slydepayhook")]
        public async Task<IActionResult> SlydePayWebHook([FromQuery] SlydePayWebhookCallback response)
        {
            if (await dbContext.LuckyMes.AnyAsync(i => i.TxRef == response.Cust_ref))
            {
                await VerifyLuckymePayment(response.Cust_ref);
            }
            else if (await dbContext.Businesses.AnyAsync(i => i.TxRef == response.Cust_ref))
            {
                await VerifyBusinessPayment(response.Cust_ref);
            }
            else if (await dbContext.Scholarships.AnyAsync(i => i.TxRef == response.Cust_ref))
            {
                await VerifyScholarshipPayment(response.Cust_ref);
            }

            return Ok();
        }

        [HttpPost("redderecievehook")]
        public async Task<IActionResult> ReddeRecieveWebHook(ReddeRecieveWebhookCallback response)
        {
            if (await dbContext.LuckyMes.AnyAsync(i => i.TxRef == response.Clienttransid))
            {
                await VerifyLuckymePayment(response.Clienttransid, "MobileMoney", response.Status);
            }
            else if (await dbContext.Businesses.AnyAsync(i => i.TxRef == response.Clienttransid))
            {
                await VerifyBusinessPayment(response.Clienttransid, "MobileMoney", response.Status);
            }
            else if (await dbContext.Scholarships.AnyAsync(i => i.TxRef == response.Clienttransid))
            {
                await VerifyScholarshipPayment(response.Clienttransid, "MobileMoney", response.Status);
            }

            return Ok();
        }

        [HttpPost("reddecashouthook")]
        public async Task<IActionResult> ReddeCashoutHook(ReddeCashoutWebhookCallback response)
        {
            if (await dbContext.LuckyMes.AnyAsync(i => i.TransferId.ToString() == response.Clienttransid))
            {
                //Passing MobileMoney as payment type to indicate the use of the rest api status check endpoint
                await VerifyReddeCashout(EntityTypes.Luckyme, "MobileMoney", response);
            }
            else if (await dbContext.Businesses.AnyAsync(i => i.TransferId.ToString() == response.Clienttransid))
            {
                //Passing MobileMoney as payment type to indicate the use of the rest api status check endpoint
                await VerifyReddeCashout(EntityTypes.Business, "MobileMoney", response);
            }
            else if (await dbContext.Scholarships.AnyAsync(i => i.TransferId.ToString() == response.Clienttransid))
            {
                //Passing MobileMoney as payment type to indicate the use of the rest api status check endpoint
                await VerifyReddeCashout(EntityTypes.Scholarship, "MobileMoney", response);
            }

            return Ok();
        }

        [HttpPost("addpaymentrecord")]
        public async Task<ActionResult<string>> AddReddeCashoutRecord(ReddeCashoutRecord record)
        {
            (string message, bool isConfirmed)? result = null;

            if (record.StakeTypeShortName == "lkm")
            {
                //Passing MobileMoney as payment type to indicate the use of the rest api status check endpoint
                result = await VerifyReddeCashoutRecord(EntityTypes.Luckyme, "MobileMoney", record);
            }
            else if (record.StakeTypeShortName == "bus")
            {
                //Passing MobileMoney as payment type to indicate the use of the rest api status check endpoint
                result = await VerifyReddeCashoutRecord(EntityTypes.Business, "MobileMoney", record);
            }
            else if (record.StakeTypeShortName == "sch")
            {
                //Passing MobileMoney as payment type to indicate the use of the rest api status check endpoint
                result = await VerifyReddeCashoutRecord(EntityTypes.Scholarship, "MobileMoney", record);
            }
            if (result.Value.isConfirmed)
                return Ok(new { result.Value.message });
            else
                return BadRequest(result.Value.message);

        }

        [HttpPost("failedravehook")]
        public async Task<IActionResult> FailedRaveWebHook(RaveWebhookCallback response)
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


        async Task<(string message, bool isConfirmed)> VerifyPayment(int? paymentId, string paymentType, string status = null)
        {
            if (Constants.PaymentGateway == PaymentGateway.flutterwave)
            {
                var data = new { txref = paymentId, SECKEY = AppSettings.FlatterWaveSettings.GetApiSecret() };
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var responseMessage = client.PostAsJsonAsync("https://api.ravepay.co/flwv3-pug/getpaidx/api/v2/verify", data).Result;
                //please make sure to change this to production url when you go live
                var responseStr = responseMessage.Content.ReadAsStringAsync().Result;
                try
                {
                    var response = JsonConvert.DeserializeObject<RaveResponse>(responseStr);
                    if (response.data.status == "successful" && response.data.chargecode == "00")
                    {
                        return (response.data.status, true);
                    }

                    return (response.data.status, false);
                }
                catch
                {
                    return ("failed", false);
                }

            }
            else if (Constants.PaymentGateway == PaymentGateway.slydepay)
            {
                var request = new
                {
                    EmailOrMobileNumber = AppSettings.SlydePaySettings.MerchantEmail,
                    MerchantKey = AppSettings.SlydePaySettings.Merchantkey,
                    OrderCode = paymentId,
                    ConfirmTransaction = true
                };

                var httpClient = new HttpClient();


                var data = JsonConvert.SerializeObject(request, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                var stringContent = new StringContent(data);
                stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var responseMessage = await httpClient.PostAsync("https://app.slydepay.com.gh/api/merchant/invoice/checkstatus", stringContent);

                var contentString = await responseMessage.Content.ReadAsStringAsync();

                try
                {
                    SlydePayPaymentStatusResponse response = JsonConvert.DeserializeObject<SlydePayPaymentStatusResponse>(contentString);

                    if (response.Result == "PAID")
                        return ("PAID", true);
                    else return (response.Result, false);
                }
                catch
                {
                    return (null, false);
                }



            }
            else if (Constants.PaymentGateway == PaymentGateway.redde)
            {
                if (status?.ToLower() == "paid")
                    return ("PAID", true);
                else if (status != null)
                    return (status, false);
                else
                {


                    var httpClient = new HttpClient();

                    httpClient.DefaultRequestHeaders.Add("apikey", AppSettings.ReddeSettings.ApiKey);
                    httpClient.DefaultRequestHeaders.Add("appid", AppSettings.ReddeSettings.AppId);
                    //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));



                    string requestUrl = null;

                    if (paymentType == "MobileMoney")
                        requestUrl = $"https://api.reddeonline.com/v1/status/{paymentId}";
                    else if (paymentType == "CreditCard")
                        requestUrl = $"https://api.reddeonline.com/v1/checkoutstatus/{paymentId}";


                    var req = new HttpRequestMessage();
                    req.Content = new StringContent("{}",
                                    Encoding.UTF8,
                                    "application/json");
                    //req.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                    req.Method = HttpMethod.Get;
                    req.RequestUri = new Uri(requestUrl);



                    var responseMessage = httpClient.SendAsync(req).Result;

                    var contentString = responseMessage.Content.ReadAsStringAsync().Result;

                    try
                    {
                        ReddeStatusResponse response = JsonConvert.DeserializeObject<ReddeStatusResponse>(contentString);

                        if (response.Status.ToLower() == "paid")
                            return ("PAID", true);
                        else return (response.Status, false);
                    }
                    catch
                    {
                        return (null, false);
                    }

                }
            }
            else return ("UNKNOWN", false);
        }
        async Task<(string message, bool isConfirmed)> VerifyReddeCashout(EntityTypes entityType, string paymentType, ReddeCashoutWebhookCallback response = null)
        {
            IStakeType stakeType = null;

            switch (entityType)
            {
                case EntityTypes.Luckyme:
                    stakeType = dbContext.LuckyMes.Where(i => i.TxRef == response.Clienttransid).FirstOrDefault();
                    break;
                case EntityTypes.Business:
                    stakeType = dbContext.Businesses.Where(i => i.TxRef == response.Clienttransid).FirstOrDefault();
                    break;
                case EntityTypes.Scholarship:
                    stakeType = dbContext.Scholarships.Where(i => i.TxRef == response.Clienttransid).FirstOrDefault();
                    break;
                default:
                    break;
            }

            var result = await VerifyPayment(response.Transactionid, paymentType, response.Status);

            var payment = await dbContext.Payments.Where(i => i.TransactionId == response.Transactionid).FirstOrDefaultAsync();

            if (payment == null)
            {
                payment = new Payment
                {
                    TransactionId = response.Transactionid,
                    ItemPayedFor = entityType.ToString(),
                    ItemPayedForId = stakeType.Id

                };
                dbContext.Payments.Add(payment);
            }

            if (response.Status.ToLower() == "paid")
            {
                payment.DatePayed = DateTime.Now;
                payment.IsPaid = true;
            }
            await dbContext.SaveChangesAsync();

            return result;
        }

        async Task<(string message, bool isConfirmed)> VerifyReddeCashoutRecord(EntityTypes entityType, string paymentType, ReddeCashoutRecord record)
        {

            try
            {
                IStakeType stakeType = null;

                switch (entityType)
                {
                    case EntityTypes.Luckyme:
                        stakeType = dbContext.LuckyMes.Where(i => i.TransferId == record.TransferId && i.User.UserType == 0 && i.Status == "won").Include("User").FirstOrDefault();
                        break;
                    case EntityTypes.Business:
                        stakeType = dbContext.Businesses.Where(i => i.TransferId == record.TransferId && i.User.UserType == 0 && i.Status == "won").Include("User").FirstOrDefault();
                        break;
                    case EntityTypes.Scholarship:
                        stakeType = dbContext.Scholarships.Where(i => i.TransferId == record.TransferId && i.User.UserType == 0 && i.Status == "won").Include("User").FirstOrDefault();
                        break;
                    default:
                        break;
                }

                if (stakeType == null)
                    return ("Invalid Stake Type", false);

                if ((int)stakeType.AmountToWin != Convert.ToInt32(record.Amount))
                    return ("Invalid Amount Specified", false);



                var result = await VerifyPayment((int)record.TransactionId, paymentType);

                var payment = await dbContext.Payments.Where(i => i.TransactionId == record.TransactionId).FirstOrDefaultAsync();

                if (!result.isConfirmed)
                {
                    //It means there is not existing payment for this transaction 
                    //So don't worry urself to create a new payment.
                    return ("Payment Verification Failed", result.isConfirmed);
                }

                if (payment == null)
                {
                    payment = new Payment
                    {
                        TransactionId = (int)record.TransactionId,
                        ItemPayedFor = entityType.ToString(),
                        ItemPayedForId = stakeType.Id,
                        PayerId = record.PayerId

                    };
                    dbContext.Payments.Add(payment);
                }
                else
                {
                    if (payment.ItemPayedForId != stakeType.Id)
                    {
                        return ("Transaction Id already recorded for another", false);
                    }
                    else
                    {
                        payment.PayerId = record.PayerId;
                        payment.TransactionId = (int)record.TransactionId;
                    }
                }


                payment.DatePayed = DateTime.Now;
                payment.IsPaid = true;
                stakeType.Status = "completed";

                var user = stakeType.User;

                await MessagingService.SendSms(Constants.MasterNumber, $"{ user.FirstName} { user.LastName} has been paid {stakeType.AmountToWin} Cedi(s) as returns for {stakeType.Period} {entityType.ToString()} Ntoboa of {stakeType.Amount} Cedi(s) with Transfer Id {stakeType.TransferId} and Transaction Id of {record.TransactionId}", "NTB Payouts");

                await dbContext.SaveChangesAsync();
                return ("Successfully Recorded Payment", result.isConfirmed);
            }
            catch
            {
                return ("Unknown Error", false);
            }

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

    public class ReddeCashoutRecord
    {
        /// <summary>
        /// Use this to determine what entityType i am dealing with
        /// </summary>
        public string StakeTypeShortName { get; set; }

        /// <summary>
        /// The Amount for the transaction Added to the new payment object
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// The transferId Used to get the particular entityType
        /// </summary>
        public long? TransferId { get; set; }

        /// <summary>
        /// The transaction Id of the current cashout
        /// </summary>
        public long? TransactionId { get; set; }

        public string PayerId { get; set; }
    }
}