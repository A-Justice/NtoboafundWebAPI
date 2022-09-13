using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NtoboaFund.Data;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.DTO_s;
using NtoboaFund.Data.Models;
using NtoboaFund.Helpers;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NtoboaFund.Services
{
    public class ReddePaymentService
    {
        public NtoboaFundDbContext dbContext { get; set; }
        public AppSettings Settings { get; set; }
        public IServiceScopeFactory serviceFactory { get; set; }

        public IUserService UserService { get; set; }

        public ReddePaymentService(IOptions<AppSettings> _appSettings, IServiceScopeFactory _serviceFactory, IUserService userService)
        {
            Settings = _appSettings.Value;
            serviceFactory = _serviceFactory;
            UserService = userService;
        }

        public async Task PersistUssdData(string requestString)
        {

            using (var scope = serviceFactory.CreateScope())
            {
                try
                {

                    dbContext = scope.ServiceProvider.GetRequiredService<NtoboaFundDbContext>();
                    //types : luckyme,business,scholarship
                    var match = Regex.Match(requestString, @"(?<type>\w+)-(?<amount>\d+)-(?<period>\w+)-(?<momonumber>\w+)-(?<voucher>[\w\s]+)-(?<mobilenumber>\w+)");
                    var type = match.Groups["type"].ToString();
                    var amount = Convert.ToDecimal(match.Groups["amount"].ToString());
                    var period = match.Groups["period"].ToString();
                    var momoNumber = "0" + Misc.NormalizePhoneNumber(match.Groups["momonumber"].ToString());
                    var voucher = match.Groups["voucher"].ToString();
                    var mobileNumber = "0" + Misc.NormalizePhoneNumber(match.Groups["mobilenumber"].ToString());


                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    var messagingService = scope.ServiceProvider.GetRequiredService<MessagingService>();

                    var _user = await getMatchedUser(dbContext, mobileNumber, userManager, roleManager, messagingService);

                    //dbContext = scope.ServiceProvider.GetRequiredService<NtoboaFundDbContext>();

                    var user = await dbContext.Users.FindAsync(_user.Id);

                    var txRef = Misc.getTxRef(mobileNumber);
                    IStakeType stakeType = null;
                    EntityTypes entityType;



                    if (type == "luckyme")
                    {
                        entityType = EntityTypes.Luckyme;
                        stakeType = new LuckyMe
                        {
                            Amount = amount,
                            Date = DateTime.Now.ToLongDateString(),
                            AmountToWin = amount * Constants.LuckymeStakeOdds,
                            Status = "pending",
                            TxRef = txRef,
                            Period = period,
                            User = user
                        };

                        dbContext.LuckyMes.Add(stakeType as LuckyMe);

                    }
                    else if (type == "business")
                    {
                        entityType = EntityTypes.Business;
                        stakeType = new Business
                        {
                            Amount = amount,
                            Date = DateTime.Now.ToLongDateString(),
                            AmountToWin = amount * Constants.BusinessStakeOdds,
                            Status = "pending",
                            TxRef = txRef,
                            Period = period,
                            User = user
                        };

                        dbContext.Businesses.Add(stakeType as Business);

                    }




                    await dbContext.SaveChangesAsync();

                    int? transactionId = null;
                    if (Constants.PaymentGateway == PaymentGateway.redde)
                        transactionId  = await Misc.GenerateAndSendReddeMomoInvoice(EntityTypes.Scholarship, stakeType, Settings.ReddeSettings, $"{Misc.FormatGhanaianPhoneNumberWp(momoNumber)}*{voucher}");
                    if (type == "luckyme")
                    {
                        var luckyMe = stakeType as LuckyMe;
                        luckyMe.TransferId = transactionId;
                        dbContext.Entry(luckyMe).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        await dbContext.SaveChangesAsync();
                        if (Constants.PaymentGateway == PaymentGateway.theTeller)
                        {
                            await Misc.GenerateAndSendTellerInvoice(luckyMe, Settings.TellerSettings, $"{Misc.FormatGhanaianPhoneNumberWp(momoNumber)}*{voucher}", "Payment for luckyme", EntityTypes.Luckyme);
                            await Misc.PostRequest<dynamic>($"{Constants.BackUrl}/transaction/verifyLuckymePayment/{luckyMe.TxRef}?paymentType=MobileMoney", new { });
                        }   
                    }
                    else if (type == "business")
                    {
                        var business = stakeType as Business;
                        business.TransferId = transactionId;
                        dbContext.Entry(business).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        await dbContext.SaveChangesAsync();
                        if (Constants.PaymentGateway == PaymentGateway.theTeller)
                        {
                            await Misc.GenerateAndSendTellerInvoice(business, Settings.TellerSettings, $"{Misc.FormatGhanaianPhoneNumberWp(momoNumber)}*{voucher}", "Payment for Business", EntityTypes.Business);
                            await Misc.PostRequest<dynamic>($"{Constants.BackUrl}transaction/verifyBusinessPayment/{business.TxRef}?paymentType=MobileMoney", new { });
                        }
                    }

                }
                catch (Exception ex)
                {

                }
            }


        }

        public async Task PersistScholarshipUssdData(string requestString)
        {
            using (var scope = serviceFactory.CreateScope())
            {
                dbContext = scope.ServiceProvider.GetRequiredService<NtoboaFundDbContext>();

                //types : luckyme,business,scholarship
                var regex = new Regex(@"^(?<type>\w+)\*(?<amount>\d+)-(?<period>\w+)-(?<institution>[\w\s]+)-(?<program>[\w\s]+)-(?<studentid>[\w\s]+)-(?<playertype>\d+)-(?<momonumber>\d+)-(?<voucher>[\w\s]+)-(?<usernumber>\d+)$").Match(requestString);
                var type = regex.Groups["type"].ToString();
                var amount = regex.Groups["amount"].ToString();
                var period = regex.Groups["period"].ToString();
                var institution = regex.Groups["institution"].ToString();
                var program = regex.Groups["program"].ToString();
                var studentId = regex.Groups["studentid"].ToString();
                var playerType = regex.Groups["playertype"].ToString();
                var voucher = regex.Groups["voucher"].ToString();
                var mobileNumber = "0" + Misc.NormalizePhoneNumber(regex.Groups["usernumber"].ToString());
                var momoNumber = "0" + Misc.NormalizePhoneNumber(regex.Groups["momonumber"].ToString());

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var messagingService = scope.ServiceProvider.GetRequiredService<MessagingService>();

                var user = await getMatchedUser(dbContext, mobileNumber, userManager,roleManager,messagingService);
                var txRef = Misc.getTxRef(mobileNumber);

                var scholarship = new Scholarship
                {
                    Amount = Convert.ToDecimal(amount),
                    Date = DateTime.Now.ToLongDateString(),
                    AmountToWin = Convert.ToDecimal(amount) * Constants.ScholarshipStakeOdds,
                    Status = "pending",
                    TxRef = txRef,
                    Period = period,
                    User = user,
                    Institution = institution,
                    Program = program,
                    StudentId = studentId,
                    PlayerType = Misc.getPlayerType(playerType)
                };




                try
                {
                    int? transactionId = null;
                    if (Constants.PaymentGateway == PaymentGateway.theTeller)
                        transactionId = await Misc.GenerateAndSendReddeMomoInvoice(EntityTypes.Scholarship, scholarship, Settings.ReddeSettings, $"{Misc.FormatGhanaianPhoneNumberWp(momoNumber)}*{voucher}");
                    scholarship.TransferId = transactionId;
                    dbContext.Scholarships.Add(scholarship);
                    await dbContext.SaveChangesAsync();
                    if (Constants.PaymentGateway == PaymentGateway.theTeller)
                    {
                        await Misc.GenerateAndSendTellerInvoice(scholarship, Settings.TellerSettings, $"{Misc.FormatGhanaianPhoneNumberWp(momoNumber)}*{voucher}", "Payment for Business", EntityTypes.Scholarship);
                        await Misc.PostRequest<dynamic>($"{Constants.BackUrl}/transaction/verifyScholarshipPayment/{scholarship.TxRef}?paymentType=MobileMoney", new { });
                    }

                }
                catch (Exception ex)
                {

                }
            }


        }

        async Task<ApplicationUser> getMatchedUser(NtoboaFundDbContext localContext, string phoneNumber, UserManager<ApplicationUser> _userManager = null, RoleManager<IdentityRole> _roleManager = null, MessagingService _messagingService = null)
        {
            var nomalizedPhoneNumber = Misc.NormalizePhoneNumber(phoneNumber);

            var user = localContext.Users.FirstOrDefault(i => i.PhoneNumber.Contains(nomalizedPhoneNumber));

            if (user != null)
                return user;

            var regDTO = new RegistrationDTO
            {
                FirstName = phoneNumber,
                LastName = "",
                PhoneNumber = phoneNumber,
                Email = $"ntoboafund.{phoneNumber}@gmail.com",
                Password = phoneNumber,
                ConfirmPassword = phoneNumber
            };

            var result = await UserService.Register(regDTO, true, localContext,_userManager,_roleManager,_messagingService);
            //dbContext.Users.Add(user);
            //await dbContext.SaveChangesAsync();

            return result.Item1;
        }

    }
}
