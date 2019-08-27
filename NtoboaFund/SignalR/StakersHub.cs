using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.DTO_s;
using System.Linq;
using System.Threading.Tasks;

namespace NtoboaFund.SignalR
{
    public class StakersHub : Hub
    {
        public StakersHub(NtoboaFundDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public NtoboaFundDbContext dbContext { get; }

        public async Task GetCurrentScholarshipParticipants()
        {
            var scholarshipParticipants = dbContext.Scholarships.Where(i => i.Status == "paid").Include("User").Select(i => new ScholarshipParticipantDTO
            {
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString(),
                AmountToWin = i.AmountToWin.ToString()

            });
            await Clients.Caller.SendAsync("getCurrentScholarshipParticipants", scholarshipParticipants.ToList());
        }

        public async Task GetCurrentBusinessParticipants()
        {
            var scholarshipParticipants = dbContext.Businesses.Where(i => i.Status == "paid").Include("User").Select(i => new BusinessParticipantDTO
            {
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString(),
                AmountToWin = i.AmountToWin.ToString()

            });
            await Clients.Caller.SendAsync("getCurrentBusinessParticipants", scholarshipParticipants.ToList());
        }

        public async Task GetCurrentDailyLuckymeParticipants()
        {
            var dailyLuckymeParticipants = dbContext.LuckyMes.Where(i => i.Status == "paid" && i.Period == "daily").Include("User").Select(i => new LuckyMeParticipantDTO
            {
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString(),
                AmountToWin = i.AmountToWin.ToString()

            });
            await Clients.Caller.SendAsync("getCurrentDailyLuckymeParticipants", dailyLuckymeParticipants.ToList());
        }

        public async Task GetCurrentWeeklyLuckymeParticipants()
        {
            var weeklyLuckymeParticipants = dbContext.LuckyMes.Where(i => i.Status == "paid" && i.Period == "weekly").Include("User").Select(i => new LuckyMeParticipantDTO
            {
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString(),
                AmountToWin = i.AmountToWin.ToString()

            });
            await Clients.Caller.SendAsync("getCurrentWeeklyLuckymeParticipants", weeklyLuckymeParticipants.ToList());
        }

        public async Task GetCurrentMonthlyLuckymeParticipants()
        {
            var monthlyLuckymeParticipants = dbContext.LuckyMes.Where(i => i.Status == "paid" && i.Period == "monthly").Include("User").Select(i => new LuckyMeParticipantDTO
            {
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString(),
                AmountToWin = i.AmountToWin.ToString()

            });
            await Clients.Caller.SendAsync("getCurrentMonthlyLuckymeParticipants", monthlyLuckymeParticipants.ToList());
        }

    }
}
