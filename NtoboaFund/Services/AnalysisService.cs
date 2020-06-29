using Microsoft.AspNetCore.SignalR;
using NtoboaFund.Data.DBContext;
using NtoboaFund.SignalR;
using System;
using System.Linq;

namespace NtoboaFund.Services
{
    public class AnalysisService
    {
        public AnalysisService(NtoboaFundDbContext _context, IHubContext<StakersHub> stakersHubContext)
        {
            dbContext = _context;
            StakersHubContext = stakersHubContext;
        }

        public NtoboaFundDbContext dbContext { get; }
        public IHubContext<StakersHub> StakersHubContext { get; }

        public AnalysisModel GetAnalysis()
        {


            //Users : 1 = Normal , 2 = Dummy ,0 = Staff

            //NB :Users with Transactions with status complete are said to have been paid their winnings
            var usersCount = dbContext.Users.Where(i => i.UserType != 2).Count();
            var dummiesCount = dbContext.Users.Where(i => i.UserType == 2).Count();

            var luckymefunds = dbContext.LuckyMes.Where(i => i.User.UserType == 1 && (i.Status == "complete" || i.Status == "lost")).Sum(i => i.Amount);
            var businessfunds = dbContext.Businesses.Where(i => i.User.UserType == 1 && (i.Status == "complete" || i.Status == "lost")).Sum(i => i.Amount);
            var scholarshipfunds = dbContext.Scholarships.Where(i => i.User.UserType != 1 && (i.Status == "complete" || i.Status == "lost")).Sum(i => i.Amount);
            var totalfundsCollected = luckymefunds + businessfunds + scholarshipfunds;

            var luckymePayouts = dbContext.LuckyMes.Where(i => i.User.UserType == 1 && i.Status == "complete").Sum(i => i.AmountToWin);
            var businessPayouts = dbContext.Businesses.Where(i => i.User.UserType == 1 && i.Status == "complete").Sum(i => i.AmountToWin);
            var scholarshipPayouts = dbContext.Scholarships.Where(i => i.User.UserType == 1 && i.Status == "complete").Sum(i => i.AmountToWin);
            var totalPayouts = luckymePayouts = businessPayouts + scholarshipPayouts;

            var profitMade = totalfundsCollected - totalPayouts;
            var totalProfits = profitMade < 0 ? 0 : profitMade;

            var luckyMeprofitsGroup = dbContext.LuckyMes.Where(i => i.User.UserType == 1 && i.Status == "complete").ToList().Select(i => new ProfitOrLossData { Year = Convert.ToDateTime(i.Date).Year, Amount = i.Amount });
            var businessprofitsGroup = dbContext.Businesses.Where(i => i.User.UserType == 1 && i.Status == "complete").ToList().Select(i => new ProfitOrLossData { Year = Convert.ToDateTime(i.Date).Year, Amount = i.Amount });
            var scholarshipprofitsGroup = dbContext.Scholarships.Where(i => i.User.UserType == 1 && i.Status == "complete").ToList().Select(i => new ProfitOrLossData { Year = Convert.ToDateTime(i.Date).Year, Amount = i.Amount });
            var profitsGroup = luckyMeprofitsGroup.Concat(businessprofitsGroup).Concat(scholarshipprofitsGroup);
            var mProfitsGroup = profitsGroup.Sum(i => i.Amount);

            var luckyMelossesGroup = dbContext.LuckyMes.Where(i => i.User.UserType == 1 && i.Status == "lost").ToList().Select(i => new ProfitOrLossData { Year = Convert.ToDateTime(i.Date).Year, Amount = i.Amount });
            var businesslossesGroup = dbContext.Businesses.Where(i => i.User.UserType == 1 && i.Status == "lost").ToList().Select(i => new ProfitOrLossData { Year = Convert.ToDateTime(i.Date).Year, Amount = i.Amount });
            var scholarshiplossesGroup = dbContext.Scholarships.Where(i => i.User.UserType == 1 && i.Status == "lost").ToList().Select(i => new ProfitOrLossData { Year = Convert.ToDateTime(i.Date).Year, Amount = i.Amount });
            var lossesGroup = luckyMelossesGroup.Concat(businesslossesGroup).Concat(scholarshiplossesGroup);
            var mLossesGroup = lossesGroup.GroupBy(i => i.Year);

            var lossesMade = totalPayouts - totalfundsCollected;
            var totalLosses = lossesMade < 0 ? 0 : lossesMade;


            var analysisModel = new AnalysisModel();
            analysisModel.UsersCount = usersCount;
            analysisModel.DummiesCount = dummiesCount;
            analysisModel.TotalFundsCollected = totalfundsCollected;
            analysisModel.TotalFundsPaid = totalPayouts;
            analysisModel.TotalProfits = totalProfits;
            analysisModel.TotalLosses = totalLosses;
            analysisModel.ProfitsGroup = mProfitsGroup;
            analysisModel.LossesGroup = mLossesGroup;

            return analysisModel;
        }
    }

    public class AnalysisModel
    {
        public int UsersCount { get; set; }

        public int DummiesCount { get; set; }

        public decimal TotalFundsCollected { get; set; }

        public decimal TotalFundsPaid { get; set; }
        public decimal TotalProfits { get; set; }

        public decimal TotalLosses { get; set; }

        public object LossesGroup { get; set; }

        public object ProfitsGroup { get; set; }
    }

    public class ProfitOrLossData
    {
        public decimal Amount { get; set; }
        public int Year { get; set; }
    }
}
