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
            var scholarshipParticipants = dbContext.Scholarships.Where(i => i.Status == "won").Include("User").Select(i => new ScholarshipParticipantDTO
            {
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString(),
                AmountToWin = i.AmountToWin.ToString()

            });
            await Clients.Caller.SendAsync("getCurrentScholarshipWinners", scholarshipParticipants.ToList());
        }

        public async Task GetCurrentBusinessWinners()
        {
            var scholarshipParticipants = dbContext.Businesses.Where(i => i.Status == "won").Include("User").Select(i => new BusinessParticipantDTO
            {
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString(),
                AmountToWin = i.AmountToWin.ToString()

            });
            await Clients.Caller.SendAsync("getCurrentBusinessWinners", scholarshipParticipants.ToList());
        }


        public async Task GetCurrentMonthlyLuckymeWinners()
        {
            var monthlyLuckymeWinners = dbContext.LuckyMes.Where(i => i.Status == "won" && i.Period.ToLower() == "monthly").Include("User").Select(i => new BusinessParticipantDTO
            {
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString(),
                AmountToWin = i.AmountToWin.ToString()

            });
            await Clients.Caller.SendAsync("getCurrentMonthlyLuckymeWinners", monthlyLuckymeWinners.ToList());
        }

        public async Task GetCurrentWeeklyLuckymeWinners()
        {
            var weeklyLuckymeWinners = dbContext.LuckyMes.Where(i => i.Status == "won" && i.Period.ToLower() == "weekly").Include("User").Select(i => new LuckyMeParticipantDTO
            {
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString(),
                AmountToWin = i.AmountToWin.ToString()

            });
            await Clients.Caller.SendAsync("getCurrentWeeklyLuckymeWinners", weeklyLuckymeWinners.ToList());
        }

        public async Task GetCurrentDailyLuckymeWinners()
        {
            var dailyLuckymeWinners = dbContext.LuckyMes.Where(i => i.Status == "won" && i.Period.ToLower() == "daily").Include("User").Select(i => new LuckyMeParticipantDTO
            {
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString(),
                AmountToWin = i.AmountToWin.ToString()

            });
            await Clients.Caller.SendAsync("getCurrentDailyLuckymeWinners", dailyLuckymeWinners.ToList());
        }

    }
}
