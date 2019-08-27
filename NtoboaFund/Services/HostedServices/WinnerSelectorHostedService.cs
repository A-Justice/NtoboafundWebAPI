using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.DTO_s;
using NtoboaFund.Data.Models;
using NtoboaFund.Helpers;
using NtoboaFund.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NtoboaFund.Services.HostedServices
{

    public interface IScopedProcessingService
    {
        void DoWork();
    }

    public class ScopedProcessingService : IScopedProcessingService
    {
        public readonly NtoboaFundDbContext context = null;
        int[] quaterlyMonths = new int[] { 3, 6, 9, 12 };
        public IHubContext<CountdownHub> CountdownHub { get; }
        public IHubContext<StakersHub> StakersHub { get; }
        public IHubContext<WinnerSelectionHub> WinnerSelectionHub { get; }
        public PaymentService PaymentService { get; }

        public ScopedProcessingService(NtoboaFundDbContext _context,
            IHubContext<StakersHub> _stakersHub,
            IHubContext<CountdownHub> _countdownHub,
            IHubContext<WinnerSelectionHub> _winnerSelectionHub, PaymentService paymentService)
        {
            context = _context;
            CountdownHub = _countdownHub;
            StakersHub = _stakersHub;
            WinnerSelectionHub = _winnerSelectionHub;
            PaymentService = paymentService;
        }

        public void DoWork()
        {
            StartChoosingWinner(null);
        }

        public async Task StartChoosingWinner(Object _selectedPeriod)
        {
            await chooseDailyWinner();
            await chooseWeeklyWinner();
            await chooseMonthlyWinner();
            await chooseQuaterlyWinner();
        }

        public async Task chooseDailyWinner()
        {
            //var ddaydiff = DateTime.Now.DailyStakeEndDate() - DateTime.Now;
            var ddaydiff = DateTime.Now.NextFiveMinutes() - DateTime.Now;

            if (ddaydiff.TotalMinutes >= 0 && ddaydiff.TotalMinutes <= 1)
            {
                if ((int)Math.Floor(ddaydiff.TotalSeconds) == 0)
                {
                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingDailyDraw", false);

                    var luckymeWinnerId = await ChooseWinnerAsync("daily", EntityTypes.Luckyme);

                    var selectedLuckyMe = await context.LuckyMes.FindAsync(luckymeWinnerId);

                    if (selectedLuckyMe != null)
                    {
                        //send the selected daily luckyme winner to the UI
                        await WinnerSelectionHub.Clients.All.SendAsync("dailyLuckymeWinner", new LuckyMeParticipantDTO
                        {
                            UserName = selectedLuckyMe.User.FirstName + " " + selectedLuckyMe.User.LastName,
                            UserId = selectedLuckyMe.UserId,
                            AmountStaked = selectedLuckyMe.Amount.ToString(),
                            AmountToWin = selectedLuckyMe.AmountToWin.ToString()
                        });
                        //clear all current daily luckyme participants
                        await StakersHub.Clients.All.SendAsync("getCurrentDailyLuckymeParticipants", new List<LuckyMeParticipantDTO>());
                    }
                    else
                    {
                        //send a blank winner
                        await WinnerSelectionHub.Clients.All.SendAsync("dailyLuckymeWinner", new LuckyMeParticipantDTO
                        {
                            UserName = "No Participants",
                            UserId = "",
                            AmountStaked = "",
                            AmountToWin = ""
                        });
                    }
                }
                else
                {
                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingDailyDraw", true);
                }
            }
            else
            {
                await CountdownHub.Clients.All.SendAsync("getDailyTime", ddaydiff.Hours, ddaydiff.Minutes - 1, ddaydiff.Seconds);
            }

        }

        public async Task chooseWeeklyWinner()
        {
            //  var wdaydiff = DateTime.Now.EndOfWeek(18, 0, 0, 0) - DateTime.Now;
            var wdaydiff = DateTime.Now.NextFiveMinutes() - DateTime.Now;

            if (wdaydiff.TotalMinutes >= 0 && wdaydiff.TotalMinutes <= 1)
            {
                if ((int)Math.Floor(wdaydiff.TotalSeconds) == 0)
                {
                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingWeeklyDraw", false);

                    var luckymeWeeklyWinnerId = await ChooseWinnerAsync("weekly", EntityTypes.Luckyme);

                    var selectedLuckyMe = await context.LuckyMes.FindAsync(luckymeWeeklyWinnerId);

                    if (selectedLuckyMe != null)
                    {
                        //send the selected daily luckyme winner to the UI
                        await WinnerSelectionHub.Clients.All.SendAsync("weeklyLuckymeWinner", new LuckyMeParticipantDTO
                        {
                            UserName = selectedLuckyMe.User.FirstName + " " + selectedLuckyMe.User.LastName,
                            UserId = selectedLuckyMe.UserId,
                            AmountStaked = selectedLuckyMe.Amount.ToString(),
                            AmountToWin = selectedLuckyMe.AmountToWin.ToString()
                        });
                        //clear all current daily luckyme participants
                        await StakersHub.Clients.All.SendAsync("getCurrentWeeklyLuckymeParticipants", new List<LuckyMeParticipantDTO>());
                    }
                    else
                    {
                        //send a blank winner
                        await WinnerSelectionHub.Clients.All.SendAsync("weeklyLuckymeWinner", new LuckyMeParticipantDTO
                        {
                            UserName = "No Participants",
                            UserId = "",
                            AmountStaked = "",
                            AmountToWin = ""
                        });
                    }
                }
                else
                {
                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingWeeklyDraw", true);
                }
            }
            else
            {
                await CountdownHub.Clients.All.SendAsync("getWeeklyTime", wdaydiff.Days, wdaydiff.Hours, wdaydiff.Minutes - 1, wdaydiff.Seconds);
            }
        }

        public async Task chooseMonthlyWinner()
        {
            // var mdaydiff = DateTime.Now.EndOfMonth(18, 0, 0, 0) - DateTime.Now;
            var mdaydiff = DateTime.Now.NextFiveMinutes() - DateTime.Now;

            if (mdaydiff.TotalMinutes >= 0 && mdaydiff.TotalMinutes <= 1)
            {
                if ((int)Math.Floor(mdaydiff.TotalSeconds) == 0)
                {
                    //Perform Winner Selection at the 0th second

                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingMonthlyDraw", false);

                    var businessWinnerId = await ChooseWinnerAsync("monthly", EntityTypes.Business);
                    var monthlyLuckymeWinnerId = await ChooseWinnerAsync("monthly", EntityTypes.Luckyme);

                    var selectedBusiness = await context.Businesses.FindAsync(businessWinnerId);
                    var selectedmonthlyLuckyMe = await context.LuckyMes.FindAsync(monthlyLuckymeWinnerId);

                    if (selectedBusiness != null)
                    {
                        //send the selected scholarship to the UI
                        await WinnerSelectionHub.Clients.All.SendAsync("businessWinner", new BusinessParticipantDTO
                        {
                            UserName = selectedBusiness.User.FirstName + " " + selectedBusiness.User.LastName,
                            UserId = selectedBusiness.UserId,
                            AmountStaked = selectedBusiness.Amount.ToString(),
                            AmountToWin = selectedBusiness.AmountToWin.ToString()
                        });
                        //clear all current participants
                        await StakersHub.Clients.All.SendAsync("getCurrentBusinessParticipants", new List<BusinessParticipantDTO>());
                    }
                    else
                    {
                        //send a blank winner
                        await WinnerSelectionHub.Clients.All.SendAsync("businessWinner", new BusinessParticipantDTO
                        {
                            UserName = "No Participants",
                            UserId = "",
                            AmountStaked = "",
                            AmountToWin = ""
                        });
                    }

                    if (selectedmonthlyLuckyMe != null)
                    {
                        //send the selected scholarship to the UI
                        await WinnerSelectionHub.Clients.All.SendAsync("monthlyLuckymeWinner", new LuckyMeParticipantDTO
                        {
                            UserName = selectedmonthlyLuckyMe.User.FirstName + " " + selectedmonthlyLuckyMe.User.LastName,
                            UserId = selectedmonthlyLuckyMe.UserId,
                            AmountStaked = selectedmonthlyLuckyMe.Amount.ToString(),
                            AmountToWin = selectedmonthlyLuckyMe.AmountToWin.ToString()
                        });
                        //clear all current participants
                        await StakersHub.Clients.All.SendAsync("getCurrentMonthlyLuckymeParticipants", new List<LuckyMeParticipantDTO>());
                    }
                    else
                    {
                        //send a blank winner
                        await WinnerSelectionHub.Clients.All.SendAsync("monthlyLuckymeWinner", new LuckyMeParticipantDTO
                        {
                            UserName = "No Participants",
                            UserId = "",
                            AmountStaked = "",
                            AmountToWin = ""
                        });
                    }


                }
                else
                {
                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingMonthlyDraw", true);
                }

            }
            else
            {
                //Make sure the countdown does not run to negatives
                if (mdaydiff.TotalSeconds >= 0)
                    await CountdownHub.Clients.All.SendAsync("getMonthlyTime", mdaydiff.Days, mdaydiff.Hours, mdaydiff.Minutes - 1, mdaydiff.Seconds);
            }
        }

        public async Task chooseQuaterlyWinner()
        {
            // var qdaydiff = DateTime.Now.NextQuater(18, 2, 0, 0) - DateTime.Now;
            var qdaydiff = DateTime.Now.NextFiveMinutes() - DateTime.Now;

            //Drawing Takes a maximum of two minutes
            if (qdaydiff.TotalMinutes >= 0 && qdaydiff.TotalMinutes <= 1)
            {

                if ((int)Math.Floor(qdaydiff.TotalSeconds) == 0)
                {
                    //stop ongoingQuaterlyDraw
                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingQuaterlyDraw", false);
                    //Get current scholarship winner Id
                    var winnerId = await ChooseWinnerAsync("quaterly", EntityTypes.Scholarship);
                    //Get the winning scholarship object
                    var selectedScholarship = await context.Scholarships.FindAsync(winnerId);

                    if (selectedScholarship != null)
                    {
                        //send the selected scholarship to the UI
                        await WinnerSelectionHub.Clients.All.SendAsync("scholarshipWinner", new ScholarshipParticipantDTO
                        {
                            UserName = selectedScholarship.User.FirstName + " " + selectedScholarship.User.LastName,
                            UserId = selectedScholarship.UserId,
                            AmountStaked = selectedScholarship.Amount.ToString(),
                            AmountToWin = selectedScholarship.AmountToWin.ToString()
                        });
                        //clear all current participants
                        await StakersHub.Clients.All.SendAsync("getCurrentScholarshipParticipants", new List<ScholarshipParticipantDTO>());
                    }
                    else
                    {
                        //send a blank winner
                        await WinnerSelectionHub.Clients.All.SendAsync("scholarshipWinner", new ScholarshipParticipantDTO
                        {
                            UserName = "No Participants",
                            UserId = "",
                            AmountStaked = "",
                            AmountToWin = ""
                        });
                    }

                }
                else
                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingQuaterlyDraw", true);
            }
            else
            {
                //Make sure the countdown does not run to negatives
                if (qdaydiff.TotalSeconds >= 0)
                {
                    //subtract the 2minutes draw time from the countdown time
                    await CountdownHub.Clients.All.SendAsync("getQuaterlyTime", qdaydiff.Days, qdaydiff.Hours, qdaydiff.Minutes - 1, qdaydiff.Seconds);

                }
            }
        }
        /// <summary>
        /// chooses the Winner for the selected period and returns the user's Id
        /// </summary>
        /// <param name="_selectedPeriod"></param>
        /// <returns></returns>
        public async Task<int?> ChooseWinnerAsync(string _selectedPeriod, EntityTypes entityType)
        {
            string selectedPeriod = _selectedPeriod;
            int? selectedWinnerId = null;
            if (String.IsNullOrEmpty(selectedPeriod))
            {
                return null;
            }


            if (_selectedPeriod == "daily")
            {
                //If The CurrentHours is not 6 O'clock dont run the algorithm
                //if (DateTime.Now.TimeOfDay.Hours != 18)
                //{
                //    return null;
                //}

                //assume a period from 6:00 yesterday to now ie. a time past 6
                //var DateLimit = (DateTime.Now - TimeSpan.FromDays(1));

                //Get the Current Users who are eligible for a draw
                //the eligible users date of stake should fall within this time
                //&& Convert.ToDateTime(i.Date) >= DateLimit
                var EligibleLuckyMeWinners = context.LuckyMes.Where(i => i.Status.ToLower() == "paid" && i.Period.ToLower() == selectedPeriod).ToList();

                //Run the Draw to choose a winner
                selectedWinnerId = await RunLuckyMeAlgorithm(EligibleLuckyMeWinners);
            }
            else if (_selectedPeriod == "weekly")
            {
                //Check if Today is saturday and the current hours is past 6 before you continue
                //if (DateTime.Now.DayOfWeek != DayOfWeek.Saturday && DateTime.Now.TimeOfDay.Hours != 18)
                //{
                //    return null;
                //}
                //assume a period of 7days from 6:00 Last Week to now ie. a time past 6
                //the eligible users date of stake should fall within this time
                //var DateLimit = (DateTime.Now - TimeSpan.FromDays(7));
                //&& Convert.ToDateTime(i.Date) >= DateLimit

                if (entityType == EntityTypes.Luckyme)
                {
                    ////Get the Current Users who are eligible for a draw
                    var EligibleLuckyMeWinners = context.LuckyMes.Where(i => i.Status.ToLower() == "paid" && i.Period.ToLower() == selectedPeriod).ToList();

                    selectedWinnerId = await RunLuckyMeAlgorithm(EligibleLuckyMeWinners);
                }

            }
            else if (_selectedPeriod == "monthly")
            {
                //Get the Number of days in the current month
                int daysInCurrentMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

                //check if today is the last day of the month and the time is 6:00 ///Deprecated
                //if (DateTime.Now.Day != daysInCurrentMonth && DateTime.Now.TimeOfDay.Hours != 18)
                //{
                //    return null;
                //}
                //var oneMonthAgo = DateTime.Now.AddMonths(-1);

                if (entityType == EntityTypes.Luckyme)
                {
                    var EligibleLuckyMeWinners = context.LuckyMes.Where(i => i.Status.ToLower() == "paid" && i.Period.ToLower() == selectedPeriod).ToList();
                    selectedWinnerId = await RunLuckyMeAlgorithm(EligibleLuckyMeWinners);
                }
                else if (entityType == EntityTypes.Business)
                {
                    var EligibleBusinessWinners = context.Businesses.Where(i => i.Status.ToLower() == "paid" && i.Period.ToLower() == selectedPeriod).ToList();
                    selectedWinnerId = await RunBusinessAlgorithm(EligibleBusinessWinners);
                }

            }
            else if (_selectedPeriod == "quaterly")
            {
                var eligibleScholarshipWinners = context.Scholarships.Where(i => i.Status.ToLower() == "paid" && i.Period.ToLower() == selectedPeriod/*&& Convert.ToDateTime(i.Date) >= threeMonthsAgo*/).ToList();

                selectedWinnerId = await RunScholarshipAlgorithm(eligibleScholarshipWinners);

            }

            context.SaveChanges();

            return selectedWinnerId;
        }


        /// <summary>
        /// Returns the Id of the Lucky Me That won
        /// </summary>
        /// <param name="eligibleLuckyMeWinners"></param>
        /// <returns></returns>
        public async Task<int?> RunLuckyMeAlgorithm(List<LuckyMe> eligibleLuckyMeWinners)
        {

            if (eligibleLuckyMeWinners == null || eligibleLuckyMeWinners.Count < 1) return null;
            //Repeat the luckyme's occurance in the selection list based on the amount staked.
            //The Selection List contains Id's of hte Lucky Me;
            List<int> SelectionList = new List<int>();

            foreach (var item in eligibleLuckyMeWinners)
            {
                //Divide staked amount by minimum possible stake amount
                var flooredAmount = item.Amount / 5;
                for (var i = flooredAmount; i > 0; i--)
                {
                    SelectionList.Add(item.Id);
                }
            }

            //Choose a winner randomly
            //Get the list length
            int selectionListLength = SelectionList.Count;
            //choose a random index between 0 and the list count inclusive
            if (selectionListLength < 1) return null;

            int selectedWinnerIndex = new Random().Next(0, selectionListLength - 1);
            //get selected Id;
            int winnerId = SelectionList[selectedWinnerIndex];
            //Set Winner and set Loosers
            foreach (var item in eligibleLuckyMeWinners)
            {
                item.DateDeclared = DateTime.Now.ToLongDateString();
                if (item.Id == winnerId)
                {
                    item.Status = "won";
                    //change the won amount later
                    item.AmountToWin = 1;

                    var user = context.Users.Where(u => u.Id == item.UserId).Include("BankDetails").Include("MomoDetails").FirstOrDefault();


                    try
                    {
                        if (user.PreferedMoneyReceptionMethod == "momo")
                        {
                            PaymentService.MomoTransfer(user, item.AmountToWin, "luckyme");
                            Operations.SendMail(user.Email, "Ntoboafund Winner", $"Dear {user.FirstName} {user.LastName}, your {item.Period} ntoboa of {item.Amount} on {item.Date} has yielded an amount of {item.AmountToWin} which would be paid directly into your Mobile Money Account");
                        }
                    }
                    catch
                    {

                    }
                    continue;
                }

                item.Status = "lost";

            }
            return winnerId;
        }


        /// <summary>
        /// Returns the Id of the scholarship that won
        /// </summary>
        /// <param name="eligibleScholarshipWinners"></param>
        /// <returns></returns>
        public async Task<int?> RunScholarshipAlgorithm(List<Scholarship> eligibleScholarshipWinners)
        {

            if (eligibleScholarshipWinners == null || eligibleScholarshipWinners.Count < 1) return null;
            //Repeat the luckyme's occurance in the selection list based on the amount staked.
            //The Selection List;
            List<int> SelectionList = new List<int>();

            foreach (var item in eligibleScholarshipWinners)
            {
                //Divide staked amount by minimum possible stake amount
                //var flooredAmount = item.Amount / 5;
                //for (var i = flooredAmount; i > 0; i--)
                //{
                //Rund only this since all amount is the same
                SelectionList.Add(item.Id);
                //}
            }

            //Choose a winner randomly
            //Get the list length
            int selectionListLength = SelectionList.Count;
            //choose a random index between 0 and the list count inclusive
            if (selectionListLength < 1) return null;

            int selectedWinnerIndex = new Random().Next(0, selectionListLength - 1);
            //get selected Id;
            int winnerId = SelectionList[selectedWinnerIndex];
            //Set Winner and set Loosers
            foreach (var item in eligibleScholarshipWinners)
            {
                item.DateDeclared = DateTime.Now.ToLongDateString();
                if (item.Id == winnerId)
                {
                    item.Status = "won";
                    //change the won amount later
                    item.AmountToWin = 1;
                    var user = context.Users.Where(u => u.Id == item.UserId).Include("BankDetails").Include("MomoDetails").FirstOrDefault();

                    try
                    {
                        if (user.PreferedMoneyReceptionMethod == "momo")
                        {
                            PaymentService.MomoTransfer(user, item.AmountToWin, "Scholarship");
                            Operations.SendMail(user.Email, "Ntofund Winner", $"Dear {user.FirstName} {user.LastName}, your Scholarhip ntoboa of {item.Amount} on {item.Date} has yielded an amount of {item.AmountToWin} which would be paid directly into your Mobile Money Account");
                        }
                        //await StakersHub.Clients.All.SendAsync("addscholarshipwinner", new ScholarshipParticipantDTO[]{
                        //    new ScholarshipParticipantDTO
                        //    {
                        //        UserId = user.Id,
                        //        UserName = user.FirstName + " " + user.LastName,
                        //        AmountStaked = item.Amount.ToString(),
                        //        AmountToWin = item.AmountToWin.ToString()

                        //    }
                        //});
                    }
                    catch
                    {

                    }
                    continue;
                }

                item.Status = "lost";

            }
            return winnerId;
        }


        public async Task<int?> RunBusinessAlgorithm(List<Business> eligibleBusinessWinners)
        {

            if (eligibleBusinessWinners == null || eligibleBusinessWinners.Count < 1) return null;
            //Repeat the luckyme's occurance in the selection list based on the amount staked.
            //The Selection List;
            List<int> SelectionList = new List<int>();

            foreach (var item in eligibleBusinessWinners)
            {
                //Divide staked amount by minimum possible stake amount
                //var flooredAmount = item.Amount / 5;
                //for (var i = flooredAmount; i > 0; i--)
                //{
                //Rund only this since all amount is the same
                SelectionList.Add(item.Id);
                //}
            }

            //Choose a winner randomly
            //Get the list length
            int selectionListLength = SelectionList.Count;
            //choose a random index between 0 and the list count inclusive
            if (selectionListLength < 1) return null;

            int selectedWinnerIndex = new Random().Next(0, selectionListLength - 1);
            //get selected Id;
            int winnerId = SelectionList[selectedWinnerIndex];
            //Set Winner and set Loosers
            foreach (var item in eligibleBusinessWinners)
            {
                item.DateDeclared = DateTime.Now.ToLongDateString();
                if (item.Id == winnerId)
                {
                    item.Status = "won";
                    //change the won amount later
                    item.AmountToWin = 1;
                    var user = context.Users.Where(u => u.Id == item.UserId).Include("BankDetails").Include("MomoDetails").FirstOrDefault();

                    try
                    {
                        if (user.PreferedMoneyReceptionMethod == "momo")
                        {
                            PaymentService.MomoTransfer(user, item.AmountToWin, "luckyme");
                            Operations.SendMail(user.Email, "Ntoboafund Winner", $"Dear {user.FirstName} {user.LastName}, your Business ntoboa of {item.Amount} on {item.Date} has yielded an amount of {item.AmountToWin} which would be paid directly into your Mobile Money Account");
                        }
                        //await StakersHub.Clients.All.SendAsync("addscholarshipwinner", new ScholarshipParticipantDTO[]{
                        //    new ScholarshipParticipantDTO
                        //    {
                        //        UserId = user.Id,
                        //        UserName = user.FirstName + " " + user.LastName,
                        //        AmountStaked = item.Amount.ToString(),
                        //        AmountToWin = item.AmountToWin.ToString()

                        //    }
                        //});
                    }
                    catch
                    {

                    }
                    continue;
                }

                item.Status = "lost";

            }
            return winnerId;
        }


        int GetDays(int seconds)
        {
            return (int)Math.Floor(seconds / 86400.0);
        }

        int GetHours(int totalSeconds, int days)
        {
            return (int)Math.Floor((totalSeconds - (days * 86400)) / 3600.0);
        }

        int GetMinutes(int totalSeconds, int days, int hours)
        {
            return (int)Math.Floor((totalSeconds - (days * 86400) - (hours * 3600)) / 60.0);
        }

        int GetSeconds(int totalSeconds, int days, int hours, int minutes)
        {
            return totalSeconds - (days * 86400) - (hours * 3600) - (minutes * 60);
        }

    }

    public class WinnerSelectorHostedService : IHostedService, IDisposable
    {

        private Timer _timer;
        public IServiceProvider Services { get; }

        public WinnerSelectorHostedService(IServiceProvider services)
        {
            Services = services;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            ////call every hour
            // _timer = new Timer(DoWork, null,0, 3600000);

            ////call every miniute
            _timer = new Timer(DoWork, null, 0, 1000);

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IScopedProcessingService>();

                scopedProcessingService.DoWork();
            }

        }




        public Task StopAsync(CancellationToken cancellationToken)
        {

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }


    }
}



//Reenact scholarship algo
//Reset Timer Time to 3600000

#region QuaterlyJunk

////get the months representing quaterly periods
//var quaterlyMonths = new int[] {3,6,9,12};

////The Year For the Draw would always be the current year
//int nextDrawYear = DateTime.Now.Year;

////Get the 
//int nextDrawMonth  = -1;
//foreach(int month in quaterlyMonths)
//{
//    //check if the current month iterated month in the quaterly period is greater the the current month
//    //only then do we choose a month and proceed
//    //NB: if no month is chosen, dont ever proceed.. 
//    if (month >= DateTime.Now.Month)
//    {
//        nextDrawMonth = month;
//        break;
//    }
//}

////If not month was chosen the we are in december the waiting for a new year
//if(nextDrawMonth < 0)
//    return;

////get the number of days in the draw month;
//var daysInDMonth = DateTime.DaysInMonth(nextDrawYear,nextDrawMonth);

////Get the date of three months ago
//var threeMonthsAgo = DateTime.Now.AddMonths(-3);

////If We are not on the draw date.. dont continue
//if (DateTime.Now.Month != nextDrawMonth && DateTime.Now.Day != daysInDMonth && DateTime.Now.TimeOfDay.Hours != 18)
//{
//    return;
//}
#endregion