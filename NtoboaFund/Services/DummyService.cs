using Microsoft.EntityFrameworkCore;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.Models;
using NtoboaFund.Helpers;
using System;
using System.Linq;

namespace NtoboaFund.Services
{
    public class DummyService
    {
        public NtoboaFundDbContext context { get; set; }
        public DummyService(NtoboaFundDbContext dbContext)
        {
            context = dbContext;
        }

        static int GetRandomAmount(EntityTypes entityType)
        {
            var r = new Random();
            switch (entityType)
            {

                case EntityTypes.Luckyme:
                    var lstakeAmounts = Constants.LuckyMeStakes;
                    return lstakeAmounts[r.Next(0, lstakeAmounts.Length - 1)];
                    break;
                case EntityTypes.Business:
                    var bstakeAmounts = Constants.BusinessStakes;
                    return bstakeAmounts[r.Next(0, bstakeAmounts.Length - 1)];
                case EntityTypes.Scholarship:
                    var sstakeAmounts = Constants.ScholarshipStakes;
                    return sstakeAmounts[r.Next(0, sstakeAmounts.Length - 1)];
                default:
                    return 0;
            }

        }

        /// <summary>
        /// Checks if the user is participating in any current Draw
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        bool isUserParticipating(ApplicationUser user)
        {
            if (user.LuckyMes.Any(i => i.Status.ToLower() == "paid") || user.Businesses.Any(i => i.Status.ToLower() == "paid") || user.Scholarships.Any(i => i.Status.ToLower() == "paid"))
                return true;

            return false;
        }

        (ApplicationUser user, bool isNew) GetDummyUser(EntityTypes entityType)
        {
            var dummyUsers = context.Users.Where(i => i.UserType == 2);
            ApplicationUser user = null;

            if (entityType == EntityTypes.Luckyme)
            {
                dummyUsers = dummyUsers.Include("LuckyMes");

                foreach (var dummyUser in dummyUsers)
                {
                    if (isUserParticipating(dummyUser))
                        continue;

                    if (dummyUser.LuckyMes.All(i => Convert.ToDateTime(i.DateDeclared) < (DateTime.Now - TimeSpan.FromDays(Constants.DaysForDummyToRepeat))))
                    {
                        return (dummyUser, false);
                    }
                }
            }
            if (entityType == EntityTypes.Scholarship)
            {
                dummyUsers = dummyUsers.Include("Scholarships");
                foreach (var dummyUser in dummyUsers)
                {
                    if (isUserParticipating(dummyUser))
                        continue;

                    if (dummyUser.Scholarships.All(i => Convert.ToDateTime(i.DateDeclared) < (DateTime.Now - TimeSpan.FromDays(Constants.DaysForDummyToRepeat))))
                    {
                        return (dummyUser, false);
                    }
                }
            }
            if (entityType == EntityTypes.Business)
            {
                dummyUsers = dummyUsers.Include("Businesses");
                foreach (var dummyUser in dummyUsers)
                {
                    if (isUserParticipating(dummyUser))
                        continue;

                    if (dummyUser.Businesses.All(i => Convert.ToDateTime(i.DateDeclared) < (DateTime.Now - TimeSpan.FromDays(Constants.DaysForDummyToRepeat))))
                    {
                        return (dummyUser, false);
                    }
                }
            }

            var userBuilders = context.UserBuilders.ToList();
            var maxUserBuilders = context.UserBuilders.Count() - 1;
            var r = new Random();

            user = new ApplicationUser
            {
                FirstName = userBuilders[r.Next(0, maxUserBuilders)].FirstName,
                LastName = userBuilders[r.Next(0, maxUserBuilders)].LastName,
                UserType = 2
                // PhoneNumber = userBuilders[r.Next(0, maxUserBuilders)].PhoneNumber,
                //Email = userBuilders[r.Next(0, maxUserBuilders)].Email
            };

            return (user, true);
        }


        public LuckyMe FixLuckyMeDailyDummy()
        {

            //ForDaily
            var duser = GetDummyUser(EntityTypes.Luckyme);

            if (duser.isNew)
                context.Users.Add(duser.user);

            var damount = GetRandomAmount(EntityTypes.Luckyme);
            var luckyme = new LuckyMe
            {
                Amount = damount,
                User = duser.user,
                Date = DateTime.Now.ToLongDateString(),
                Period = "daily",
                Status = "paid",
                AmountToWin = Constants.LuckymeStakeOdds * damount
            };
            context.LuckyMes.Add(luckyme);
            context.SaveChanges();
            return luckyme;
        }
        public LuckyMe FixLuckyMeMonthlyDummy()
        {
            //ForMonthly
            var muser = GetDummyUser(EntityTypes.Luckyme);

            if (muser.isNew)
                context.Users.Add(muser.user);

            var mamount = GetRandomAmount(EntityTypes.Luckyme);
            var luckyme = new LuckyMe
            {
                Amount = mamount,
                User = muser.user,
                Date = DateTime.Now.ToLongDateString(),
                Period = "monthly",
                Status = "paid",
                AmountToWin = Constants.LuckymeStakeOdds * mamount
            };
            context.LuckyMes.Add(luckyme);
            context.SaveChanges();
            return luckyme;
        }
        public LuckyMe FixLuckyMeWeeklyDummy()
        {
            //For weekly
            var wuser = GetDummyUser(EntityTypes.Luckyme);
            if (wuser.isNew)
                context.Users.Add(wuser.user);

            var wamount = GetRandomAmount(EntityTypes.Luckyme);
            var luckyme = new LuckyMe
            {
                Amount = wamount,
                User = wuser.user,
                Date = DateTime.Now.ToLongDateString(),
                Period = "weekly",
                Status = "paid",
                AmountToWin = Constants.LuckymeStakeOdds * wamount
            };
            context.LuckyMes.Add(luckyme);
            context.SaveChanges();
            return luckyme;
        }
        public Scholarship FixScholarshipDummy()
        {
            var suser = GetDummyUser(EntityTypes.Scholarship);
            if (suser.isNew)
                context.Users.Add(suser.user);

            var amount = GetRandomAmount(EntityTypes.Scholarship);
            var scholarship = new Scholarship
            {
                Amount = amount,
                User = suser.user,
                Date = DateTime.Now.ToLongDateString(),
                Period = "quaterly",
                Status = "paid",
                AmountToWin = Constants.ScholarshipStakeOdds * amount,
                Institution = "",
                Program = "",
                StudentId = ""
            };
            context.Scholarships.Add(scholarship);
            context.SaveChanges();
            return scholarship;
        }
        public Business FixBusinessDummy()
        {
            var buser = GetDummyUser(EntityTypes.Business);
            if (buser.isNew)
                context.Users.Add(buser.user);
            var amount = GetRandomAmount(EntityTypes.Business);
            var business = new Business
            {
                Amount = amount,
                User = buser.user,
                Date = DateTime.Now.ToLongDateString(),
                Period = "monthly",
                Status = "paid",
                AmountToWin = Constants.BusinessStakeOdds * amount
            };
            context.Businesses.Add(business);
            context.SaveChanges();
            return business;

        }


    }
}