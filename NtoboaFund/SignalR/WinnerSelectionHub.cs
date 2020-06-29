using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.DTO_s;
using System.Linq;
using System.Threading.Tasks;

namespace NtoboaFund.SignalR
{
    public class WinnerSelectionHub : Hub
    {

        public WinnerSelectionHub(NtoboaFundDbContext _context)
        {
            dbContext = _context;
        }

        public NtoboaFundDbContext dbContext { get; }

        public async Task GetCurrentScholarshipWinners()
        {
            var scholarshipParticipants = dbContext.Scholarships.Where(i => i.Status == "won").Include("User").OrderByDescending(i=>i.Id).Take(10).Select(i => new ScholarshipParticipantDTO
            {
                Id = i.Id,
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString("0.##"),
                AmountToWin = i.AmountToWin.ToString("0.##"),
                Status = i.Status,
                DateDeclared = i.DateDeclared

            });
            await Clients.Caller.SendAsync("getCurrentScholarshipWinners", scholarshipParticipants.ToList());
        }

        public async Task GetCurrentBusinessWinners()
        {
            var scholarshipParticipants = dbContext.Businesses.Where(i => i.Status == "won").Include("User").OrderByDescending(i => i.Id).Take(10).Select(i => new BusinessParticipantDTO
            {
                Id = i.Id,
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString("0.##"),
                AmountToWin = i.AmountToWin.ToString("0.##"),
                Status = i.Status,
                DateDeclared = i.DateDeclared

            });
            await Clients.Caller.SendAsync("getCurrentBusinessWinners", scholarshipParticipants.ToList());
        }


        public async Task GetCurrentMonthlyLuckymeWinners()
        {
            var monthlyLuckymeWinners = dbContext.LuckyMes.Where(i => i.Status == "won" && i.Period.ToLower() == "monthly").Include("User").OrderByDescending(i => i.Id).Take(10).Select(i => new BusinessParticipantDTO
            {
                Id = i.Id,
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString("0.##"),
                AmountToWin = i.AmountToWin.ToString("0.##"),
                Status = i.Status
                ,
                DateDeclared = i.DateDeclared

            });
            await Clients.Caller.SendAsync("getCurrentMonthlyLuckymeWinners", monthlyLuckymeWinners.ToList());
        }

        public async Task GetCurrentWeeklyLuckymeWinners()
        {
            var weeklyLuckymeWinners = dbContext.LuckyMes.Where(i => i.Status == "won" && i.Period.ToLower() == "weekly").Include("User").OrderByDescending(i => i.Id).Take(10).Select(i => new LuckyMeParticipantDTO
            {
                Id = i.Id,
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString("0.##"),
                AmountToWin = i.AmountToWin.ToString("0.##"),
                Status = i.Status,
                DateDeclared = i.DateDeclared

            });
            await Clients.Caller.SendAsync("getCurrentWeeklyLuckymeWinners", weeklyLuckymeWinners.ToList());
        }

        public async Task GetCurrentDailyLuckymeWinners()
        {
            var dailyLuckymeWinners = dbContext.LuckyMes.Where(i => i.Status == "won" && i.Period.ToLower() == "daily").Include("User").OrderByDescending(i => i.Id).Take(10).Select(i => new LuckyMeParticipantDTO
            {
                Id = i.Id,
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString("0.##"),
                AmountToWin = i.AmountToWin.ToString("0.##"),
                Status = i.Status,
                DateDeclared = i.DateDeclared

            });
            await Clients.Caller.SendAsync("getCurrentDailyLuckymeWinners", dailyLuckymeWinners.ToList());
        }

    }
}
