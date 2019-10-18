using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.DTO_s;
using NtoboaFund.Data.Models;
using NtoboaFund.Helpers;
using NtoboaFund.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NtoboaFund.SignalR
{
    public class StakersHub : Hub
    {
        private static int _userCount = 0;
        double WinnersCountPercentage = 0.5;
        public StakersHub(NtoboaFundDbContext _dbContext, DummyService dummyService)
        {
            dbContext = _dbContext;
            DummyService = dummyService;
        }

        public NtoboaFundDbContext dbContext { get; }
        public DummyService DummyService { get; }

        #region GetCurrentParticipants

        IQueryable<LuckyMe> DailyLuckymeParticipants()
        {
            return dbContext.LuckyMes.Where(i => (i.Status.ToLower() == "paid" || i.Status.ToLower() == "wins") && i.Period.ToLower() == "daily").Include("User");
        }

        IQueryable<LuckyMe> WeeklyLuckymeParticipants()
        {
            return dbContext.LuckyMes.Where(i => (i.Status.ToLower() == "paid" || i.Status.ToLower() == "wins") && i.Period.ToLower() == "weekly").Include("User");
        }

        IQueryable<LuckyMe> MonthlyLuckymeParticipants()
        {
            return dbContext.LuckyMes.Where(i => (i.Status.ToLower() == "paid" || i.Status.ToLower() == "wins") && i.Period.ToLower() == "monthly").Include("User");
        }

        IQueryable<Business> BusinessParticipants()
        {
            return dbContext.Businesses.Where(i => (i.Status.ToLower() == "paid" || i.Status.ToLower() == "wins")).Include("User");
        }

        IQueryable<Scholarship> ScholarshipParticipants()
        {
            return dbContext.Scholarships.Where(i => (i.Status.ToLower() == "paid" || i.Status.ToLower() == "wins")).Include("User");
        }

        #endregion

        #region GetCurrentDummies

        //Get the count of current dummies in a category
        public int CurrentDummyParticipantsCount(EntityTypes entityTypes, Period? period)
        {
            switch (entityTypes)
            {
                case EntityTypes.Luckyme:
                    if (period == Period.Daily)
                        return DailyDummyLuckymeParticipants().Count();
                    if (period == Period.Weekly)
                        return WeeklyDummyLuckymeParticipants().Count();
                    if (period == Period.Monthly)
                        return MonthlyDummyLuckymeParticipants().Count();
                    break;
                case EntityTypes.Business:
                    return DummyBusinessParticipants().Count();
                    break;
                case EntityTypes.Scholarship:
                    return DummyScholarshipParticipants().Count();
                    break;
                default:
                    break;
            }
            return 0;
        }

        public IQueryable<LuckyMe> DailyDummyLuckymeParticipants()
        {
            return DailyLuckymeParticipants().Where(i => i.User.UserType == 2);
        }

        public IQueryable<LuckyMe> WeeklyDummyLuckymeParticipants()
        {
            return WeeklyLuckymeParticipants().Where(i => i.User.UserType == 2);
        }
        public IQueryable<LuckyMe> MonthlyDummyLuckymeParticipants()
        {
            return MonthlyLuckymeParticipants().Where(i => i.User.UserType == 2);
        }
        public IQueryable<Business> DummyBusinessParticipants()
        {
            return BusinessParticipants().Where(i => i.User.UserType == 2);
        }
        public IQueryable<Scholarship> DummyScholarshipParticipants()
        {
            return ScholarshipParticipants().Where(i => i.User.UserType == 2);
        }

        #endregion

        #region HubMethods
        public List<LuckyMeParticipantDTO> GetDailyLuckymeParticipants()
        {
            var dailyLuckymeParticipants = DailyLuckymeParticipants().Select(i => new LuckyMeParticipantDTO
            {
                Id = i.Id,
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString(),
                AmountToWin = i.AmountToWin.ToString(),
                Status = i.Status

            }).ToList();

            return dailyLuckymeParticipants;

        }

        public List<LuckyMeParticipantDTO> GetWeeklyLuckymeParticipants()
        {
            var weeklyLuckymeParticipants = WeeklyLuckymeParticipants().Select(i => new LuckyMeParticipantDTO
            {
                Id = i.Id,
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString(),
                AmountToWin = i.AmountToWin.ToString(),
                Status = i.Status

            }).ToList();

            return weeklyLuckymeParticipants;

        }

        public List<LuckyMeParticipantDTO> GetMonthlyLuckymeParticipants()
        {
            var monthlyLuckymeParticipants = MonthlyLuckymeParticipants().Select(i => new LuckyMeParticipantDTO
            {
                Id = i.Id,
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString(),
                AmountToWin = i.AmountToWin.ToString(),
                Status = i.Status

            }).ToList();

            return monthlyLuckymeParticipants;

        }


        public List<ScholarshipParticipantDTO> GetScholarshipParticipants()
        {
            var scholarshipParticipants = ScholarshipParticipants().Select(i => new ScholarshipParticipantDTO
            {
                Id = i.Id,
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString(),
                AmountToWin = i.AmountToWin.ToString(),
                Status = i.Status

            }).ToList();

            return scholarshipParticipants;
        }

        public List<BusinessParticipantDTO> GetBusinessParticipants()
        {
            var businessParticipants = BusinessParticipants().Select(i => new BusinessParticipantDTO
            {
                Id = i.Id,
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString(),
                AmountToWin = i.AmountToWin.ToString(),
                Status = i.Status

            }).ToList();
            return businessParticipants;
        }

        public async Task GetCurrentScholarshipParticipants()
        {
            //var scholarshipParticipants = dbContext.Scholarships.Where(i => i.Status.ToLower() == "paid").Include("User").Select(i => new ScholarshipParticipantDTO
            //{
            //    UserName = i.User.FirstName + " " + i.User.LastName,
            //    UserId = i.UserId,
            //    AmountStaked = i.Amount.ToString(),
            //    AmountToWin = i.AmountToWin.ToString()

            //});
            await Clients.Caller.SendAsync("getCurrentScholarshipParticipants", GetScholarshipParticipants());
        }

        public async Task GetCurrentBusinessParticipants()
        {
            //var businessParticipants = dbContext.Businesses.Where(i => i.Status.ToLower() == "paid").Include("User").Select(i => new BusinessParticipantDTO
            //{
            //    UserName = i.User.FirstName + " " + i.User.LastName,
            //    UserId = i.UserId,
            //    AmountStaked = i.Amount.ToString(),
            //    AmountToWin = i.AmountToWin.ToString()

            //});
            await Clients.Caller.SendAsync("getCurrentBusinessParticipants", GetBusinessParticipants());
        }

        public async Task GetCurrentDailyLuckymeParticipants()
        {
            //var dailyLuckymeParticipants = dbContext.LuckyMes.Where(i => i.Status.ToLower() == "paid" && i.Period.ToLower() == "daily").Include("User").Select(i => new LuckyMeParticipantDTO
            //{
            //    UserName = i.User.FirstName + " " + i.User.LastName,
            //    UserId = i.UserId,
            //    AmountStaked = i.Amount.ToString(),
            //    AmountToWin = i.AmountToWin.ToString()

            //});
            await Clients.Caller.SendAsync("getCurrentDailyLuckymeParticipants", GetDailyLuckymeParticipants());
        }

        public async Task GetCurrentWeeklyLuckymeParticipants()
        {
            //var weeklyLuckymeParticipants = dbContext.LuckyMes.Where(i => i.Status.ToLower() == "paid" && i.Period.ToLower() == "weekly").Include("User").Select(i => new LuckyMeParticipantDTO
            //{
            //    UserName = i.User.FirstName + " " + i.User.LastName,
            //    UserId = i.UserId,
            //    AmountStaked = i.Amount.ToString(),
            //    AmountToWin = i.AmountToWin.ToString()

            //});
            await Clients.Caller.SendAsync("getCurrentWeeklyLuckymeParticipants", GetWeeklyLuckymeParticipants());
        }

        public async Task GetCurrentMonthlyLuckymeParticipants()
        {
            //var monthlyLuckymeParticipants = dbContext.LuckyMes.Where(i => i.Status.ToLower() == "paid" && i.Period.ToLower() == "monthly").Include("User").Select(i => new LuckyMeParticipantDTO
            //{
            //    UserName = i.User.FirstName + " " + i.User.LastName,
            //    UserId = i.UserId,
            //    AmountStaked = i.Amount.ToString(),
            //    AmountToWin = i.AmountToWin.ToString()

            //});
            await Clients.Caller.SendAsync("getCurrentMonthlyLuckymeParticipants", GetMonthlyLuckymeParticipants());
        }

        public async Task GetPotentialScholarshipWinnersCount()
        {
            await Clients.Caller.SendAsync("getPotentialScholarshipWinnersCount", GetCurrentPotentialWinnersCount(EntityTypes.Scholarship));
        }

        public async Task GetPotentialBusinessWinnersCount()
        {
            await Clients.Caller.SendAsync("getPotentialBusinessWinnersCount", GetCurrentPotentialWinnersCount(EntityTypes.Business));
        }

        public async Task GetPotentialDailyLuckymeWinnersCount()
        {
            await Clients.Caller.SendAsync("getPotentialDailyLuckymeWinnersCount", GetCurrentPotentialWinnersCount(EntityTypes.Luckyme, Period.Daily));
        }
        public async Task GetPotentialWeeklyLuckymeWinnersCount()
        {
            await Clients.Caller.SendAsync("getPotentialWeeklyLuckymeWinnersCount", GetCurrentPotentialWinnersCount(EntityTypes.Luckyme, Period.Weekly));
        }
        public async Task GetPotentialMonthlyLuckymeWinnersCount()
        {
            await Clients.Caller.SendAsync("getPotentialMonthlyLuckymeWinnersCount", GetCurrentPotentialWinnersCount(EntityTypes.Luckyme, Period.Monthly));
        }
        public async Task AddDummyParticipant(string entityType, string period)
        {
            if (entityType.ToLower() == "luckyme")
            {
                if (period.ToLower() == "daily")
                {
                    var luckyme = DummyService.FixLuckyMeDailyDummy();
                    await Clients.All.SendAsync("adddailyluckymeparticipant",
                       new LuckyMeParticipantDTO
                       {
                           Id = luckyme.Id,
                           UserId = luckyme.User.Id,
                           UserName = luckyme.User.FirstName + " " + luckyme.User.LastName,
                           AmountStaked = luckyme.Amount.ToString(),
                           AmountToWin = luckyme.AmountToWin.ToString(),
                           Status = luckyme.Status
                       });
                }
                else if (period.ToLower() == "weekly")
                {
                    var luckyme = DummyService.FixLuckyMeWeeklyDummy();
                    await Clients.All.SendAsync("addweeklyluckymeparticipant",
                      new LuckyMeParticipantDTO
                      {
                          Id = luckyme.Id,
                          UserId = luckyme.User.Id,
                          UserName = luckyme.User.FirstName + " " + luckyme.User.LastName,
                          AmountStaked = luckyme.Amount.ToString(),
                          AmountToWin = luckyme.AmountToWin.ToString(),
                          Status = luckyme.Status
                      });
                }
                else if (period.ToLower() == "monthly")
                {
                    var luckyme = DummyService.FixLuckyMeMonthlyDummy();
                    await Clients.All.SendAsync("addmonthlyluckymeparticipant",
                    new LuckyMeParticipantDTO
                    {
                        Id = luckyme.Id,
                        UserId = luckyme.User.Id,
                        UserName = luckyme.User.FirstName + " " + luckyme.User.LastName,
                        AmountStaked = luckyme.Amount.ToString(),
                        AmountToWin = luckyme.AmountToWin.ToString(),
                        Status = luckyme.Status
                    });
                }
            }
            else if (entityType.ToLower() == "business")
            {
                var business = DummyService.FixBusinessDummy();
                await Clients.All.SendAsync("addbusinessparticipant",
                new BusinessParticipantDTO
                {
                    Id = business.Id,
                    UserId = business.User.Id,
                    UserName = business.User.FirstName + " " + business.User.LastName,
                    AmountStaked = business.Amount.ToString(),
                    AmountToWin = business.AmountToWin.ToString(),
                    Status = business.Status
                });
            }
            else if (entityType.ToLower() == "scholarship")
            {
                var scholarship = DummyService.FixScholarshipDummy();
                await Clients.All.SendAsync("addscholarshipparticipant",
                new ScholarshipParticipantDTO
                {
                    Id = scholarship.Id,
                    UserId = scholarship.User.Id,
                    UserName = scholarship.User.FirstName + " " + scholarship.User.LastName,
                    AmountStaked = scholarship.Amount.ToString(),
                    AmountToWin = scholarship.AmountToWin.ToString(),
                    Status = scholarship.Status
                });
            }
        }

        public async Task FixWinner(string entityType, string period, int winnerId)
        {
            if (entityType.ToLower() == "luckyme")
            {
                if (period.ToLower() == "daily")
                {
                    foreach (var item in DailyLuckymeParticipants())
                    {
                        if (item.Id == winnerId && item.Status.ToLower() == "paid")
                            item.Status = "wins";
                        else if (item.Id != winnerId || item.Status.ToLower() == "wins")
                            item.Status = "paid";

                        dbContext.Entry(item).State = EntityState.Modified;
                    }
                }
                if (period.ToLower() == "weekly")
                {
                    foreach (var item in WeeklyLuckymeParticipants())
                    {
                        if (item.Id == winnerId && item.Status.ToLower() == "paid")
                            item.Status = "wins";
                        else if (item.Id != winnerId || item.Status.ToLower() == "wins")
                            item.Status = "paid";

                        dbContext.Entry(item).State = EntityState.Modified;
                    }
                }
                if (period.ToLower() == "monthly")
                {
                    foreach (var item in MonthlyLuckymeParticipants())
                    {
                        if (item.Id == winnerId && item.Status.ToLower() == "paid")
                            item.Status = "wins";
                        else if (item.Id != winnerId || item.Status.ToLower() == "wins")
                            item.Status = "paid";

                        dbContext.Entry(item).State = EntityState.Modified;
                    }
                }
            }
            if (entityType.ToLower() == "business")
            {
                foreach (var item in BusinessParticipants())
                {
                    if (item.Id == winnerId && item.Status.ToLower() == "paid")
                        item.Status = "wins";
                    else if (item.Id != winnerId || item.Status.ToLower() == "wins")
                        item.Status = "paid";

                    dbContext.Entry(item).State = EntityState.Modified;
                }
            }
            if (entityType.ToLower() == "scholarship")
            {
                foreach (var item in ScholarshipParticipants())
                {

                    if (item.Id == winnerId && item.Status.ToLower() == "paid")
                        item.Status = "wins";
                    else if (item.Id != winnerId || item.Status.ToLower() == "wins")
                        item.Status = "paid";

                    dbContext.Entry(item).State = EntityState.Modified;
                }
            }

            dbContext.SaveChanges();
        }

        public async Task UnfixWinner(string entityType, string period, int winnerId)
        {
            if (entityType.ToLower() == "luckyme")
            {
                if (period.ToLower() == "daily")
                {

                    var item = DailyLuckymeParticipants().Where(i => i.Id == winnerId).FirstOrDefault();

                    item.Status = "paid";

                    dbContext.Entry(item).State = EntityState.Modified;

                }
                if (period.ToLower() == "weekly")
                {
                    var item = WeeklyLuckymeParticipants().Where(i => i.Id == winnerId).FirstOrDefault();

                    item.Status = "paid";

                    dbContext.Entry(item).State = EntityState.Modified;

                }
                if (period.ToLower() == "monthly")
                {
                    var item = MonthlyLuckymeParticipants().Where(i => i.Id == winnerId).FirstOrDefault();

                    item.Status = "paid";

                    dbContext.Entry(item).State = EntityState.Modified;

                }
            }
            if (entityType.ToLower() == "business")
            {
                var item = BusinessParticipants().Where(i => i.Id == winnerId).FirstOrDefault();

                item.Status = "paid";

                dbContext.Entry(item).State = EntityState.Modified;

            }
            if (entityType.ToLower() == "scholarship")
            {
                var item = ScholarshipParticipants().Where(i => i.Id == winnerId).FirstOrDefault();

                item.Status = "paid";

                dbContext.Entry(item).State = EntityState.Modified;

            }

            dbContext.SaveChanges();
        }

        public async Task OnlineUsersCount()
        {
            await Clients.All.SendAsync("online", _userCount);
        }

        public override Task OnConnectedAsync()
        {
            _userCount++;
            Clients.All.SendAsync("online", _userCount);
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            _userCount--;
            Clients.All.SendAsync("online", _userCount);
            return base.OnDisconnectedAsync(exception);
        }

        #endregion

        public int GetCurrentPotentialWinnersCount(EntityTypes entityTypes, Period? period = Period.Daily)
        {
            //Hold the total participants in that group
            var participantsCount = 1;
            switch (entityTypes)
            {
                case EntityTypes.Luckyme:
                    if (period == Period.Daily)
                        participantsCount = DailyLuckymeParticipants().Count();
                    if (period == Period.Weekly)
                        participantsCount = WeeklyLuckymeParticipants().Count();
                    if (period == Period.Monthly)
                        participantsCount = MonthlyLuckymeParticipants().Count();
                    break;
                case EntityTypes.Business:
                    participantsCount = BusinessParticipants().Count();
                    break;
                case EntityTypes.Scholarship:
                    participantsCount = ScholarshipParticipants().Count();
                    break;
                default:
                    break;
            }
            var winnersCount = GetWinnersCount(participantsCount);

            return (int)winnersCount;
        }

        public int GetWinnersCount(int participantsCount)
        {
            return (int)Math.Ceiling(participantsCount * WinnersCountPercentage);
        }

        public List<object> ManageDummies(EntityTypes entityType, Period? period)
        {
            int currentPotentialWinnersCount = GetCurrentPotentialWinnersCount(entityType, period);

            int currentDummiesCount = CurrentDummyParticipantsCount(entityType, period);

            var dummyDifference = currentPotentialWinnersCount - currentDummiesCount;

            //Object to hold the number of dummies inserted
            List<object> dummyObjects = new List<object>();

            for (int i = 0; i < dummyDifference; i++)
            {
                switch (entityType)
                {
                    case EntityTypes.Luckyme:
                        if (period == Period.Daily)
                            dummyObjects.Add(DummyService.FixLuckyMeDailyDummy());
                        if (period == Period.Weekly)
                            dummyObjects.Add(DummyService.FixLuckyMeWeeklyDummy());
                        if (period == Period.Monthly)
                            dummyObjects.Add(DummyService.FixLuckyMeMonthlyDummy());
                        break;
                    case EntityTypes.Business:
                        dummyObjects.Add(DummyService.FixBusinessDummy());
                        break;
                    case EntityTypes.Scholarship:
                        dummyObjects.Add(DummyService.FixScholarshipDummy());
                        break;
                    default:
                        break;
                }
            }

            return dummyObjects;
        }

    }
}
