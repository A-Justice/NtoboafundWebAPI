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

        public async Task PersistReddeUssdData(string requestString)
        {

            using (var scope = serviceFactory.CreateScope())
            {
                dbContext = scope.ServiceProvider.GetRequiredService<NtoboaFundDbContext>();
                //types : lkm,bus,sch
                var match = Regex.Match(requestString, @"(?<type>\w+)-(?<amount>\d+)-(?<period>\w+)-(?<momonumber>\w+)-(?<voucher>[\w\s]+)-(?<mobilenumber>\w+)");
                var type = match.Groups["type"].ToString();
                var amount = Convert.ToDecimal(match.Groups["amount"].ToString());
                var period = match.Groups["period"].ToString();
                var momoNumber = "0" + Misc.NormalizePhoneNumber(match.Groups["momonumber"].ToString());
                var voucher = match.Groups["voucher"].ToString();
                var mobileNumber = "0" + Misc.NormalizePhoneNumber(match.Groups["mobilenumber"].ToString());
                var user = await getMatchedUser(dbContext, mobileNumber);

                var txRef = Misc.getTxRef(mobileNumber);
                IStakeType stakeType = null;
                EntityTypes entityType;



                if (type == "lkm")
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
                else if (type == "bus")
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

                try
                {
                    var transactionId = await Misc.GenerateAndSendReddeMomoInvoice(EntityTypes.Scholarship, stakeType, Settings.ReddeSettings, $"{Misc.FormatGhanaianPhoneNumberWp(momoNumber)}*{voucher}");
                    if (type == "lkm")
                    {
                        var luckyMe = stakeType as LuckyMe;
                        luckyMe.TransferId = transactionId;
                        dbContext.Entry(luckyMe).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        await dbContext.SaveChangesAsync();
                    }
                    else if (type == "bus")
                    {
                        var business = stakeType as Business;
                        business.TransferId = transactionId;
                        dbContext.Entry(business).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        await dbContext.SaveChangesAsync();
                    }

                }
                catch (Exception ex)
                {

                }
            }


        }

        public async Task PersistScholarshipReddeUssdData(string requestString)
        {
            using (var scope = serviceFactory.CreateScope())
            {
                dbContext = scope.ServiceProvider.GetRequiredService<NtoboaFundDbContext>();

                //types : lkm,bus,sch
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


                var user = await getMatchedUser(dbContext, mobileNumber);
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
                    var transactionId = await Misc.GenerateAndSendReddeMomoInvoice(EntityTypes.Scholarship, scholarship, Settings.ReddeSettings, $"{Misc.FormatGhanaianPhoneNumberWp(momoNumber)}*{voucher}");
                    scholarship.TransferId = transactionId;
                    dbContext.Scholarships.Add(scholarship);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {

                }
            }


        }


        async Task<ApplicationUser> getMatchedUser(NtoboaFundDbContext localContext, string phoneNumber)
        {
            phoneNumber = "0" + Misc.NormalizePhoneNumber(phoneNumber);

            var user = localContext.Users.FirstOrDefault(i => Misc.NormalizePhoneNumber(i.PhoneNumber) == Misc.NormalizePhoneNumber(phoneNumber));

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
            var result = await UserService.Register(regDTO, true);
            //dbContext.Users.Add(user);
            //await dbContext.SaveChangesAsync();

            return result.Item1;
        }

    }
}
