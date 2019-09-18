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
        Task chooseDailyWinner(object obj);
        Task chooseWeeklyWinner(object obj);
        Task chooseMonthlyWinner(object obj);
        Task chooseQuaterlyWinner(object obj);
    }

    public class ScopedProcessingService : IScopedProcessingService
    {
        public readonly NtoboaFundDbContext context = null;
        int[] quaterlyMonths = new int[] { 3, 6, 9, 12 };
        public IHubContext<CountdownHub> CountdownHub { get; }
        public IHubContext<StakersHub> StakersHub { get; }
        public IHubContext<WinnerSelectionHub> WinnerSelectionHub { get; }
        public PaymentService PaymentService { get; }
        public MessagingService MessagingService { get; }
        public StakersHub DataHub { get; }
        public DummyService DummyService { get; }

        public ScopedProcessingService(NtoboaFundDbContext _context,IHubContext<StakersHub> _stakersHub,
            IHubContext<CountdownHub> _countdownHub,IHubContext<WinnerSelectionHub> _winnerSelectionHub,
            PaymentService paymentService,MessagingService messagingService,StakersHub dataHub,DummyService dummyService)
        {
            context = _context;
            CountdownHub = _countdownHub;
            StakersHub = _stakersHub;
            WinnerSelectionHub = _winnerSelectionHub;
            PaymentService = paymentService;
            MessagingService = messagingService;
            DataHub = dataHub;
            DummyService = dummyService;
        }

        public void DoWork()
        {
            StartChoosingWinner(null);
        }

        public void StartChoosingWinner(Object _selectedPeriod)
        {

            //await chooseDailyWinner();

            //await chooseWeeklyWinner();

            //await chooseMonthlyWinner();

            //await chooseQuaterlyWinner();


        }

        public async Task chooseDailyWinner(object obj)
        {
            var ddaydiff = DateTime.Now.DailyStakeEndDate() - DateTime.Now;
            //var ddaydiff = DateTime.Now.NextFiveMinutes() - DateTime.Now;

            if (ddaydiff.TotalMinutes >= 0 && ddaydiff.TotalMinutes <= 1)
            {
                if ((int)Math.Floor(ddaydiff.TotalSeconds) == 1)
                {

                    var luckymeWinnerId = await ChooseWinnerAsync("daily", EntityTypes.Luckyme);

                    var selectedLuckyMe = await context.LuckyMes.FindAsync(luckymeWinnerId);

                    //Put a dummy daily luckyme 
                    DummyService.FixLuckyMeDailyDummy();


                    if (selectedLuckyMe != null)
                    {
                        //send the selected daily luckyme winner to the UI
                        await WinnerSelectionHub.Clients.All.SendAsync("dailyLuckymeWinner", new LuckyMeParticipantDTO
                        {
                            Id = selectedLuckyMe.Id,
                            UserName = selectedLuckyMe.User.FirstName + " " + selectedLuckyMe.User.LastName,
                            UserId = selectedLuckyMe.UserId,
                            AmountStaked = selectedLuckyMe.Amount.ToString(),
                            AmountToWin = selectedLuckyMe.AmountToWin.ToString(),
                            Status = selectedLuckyMe.Status
                        });

                        
                        //clear all current daily luckyme participants
                        await StakersHub.Clients.All.SendAsync("getCurrentDailyLuckymeParticipants", DataHub.GetDailyLuckymeParticipants());
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

                   
                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingDailyDraw", false);
                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingDailyDraw", false);

                    
                    
                }
                else if ((int)Math.Floor(ddaydiff.TotalSeconds) > 1)
                {
                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingDailyDraw", true);
                }
            }
            else
            {
                await CountdownHub.Clients.All.SendAsync("getDailyTime", ddaydiff.Hours, ddaydiff.Add(TimeSpan.FromMinutes(-1)).Minutes, ddaydiff.Seconds);
            }

        }

        public async Task chooseWeeklyWinner(object obj)
        {
            var wdaydiff = DateTime.Now.EndOfWeek(18, 0, 0, 0) - DateTime.Now;
           // var wdaydiff = DateTime.Now.NextFiveMinutes() - DateTime.Now;

            if (wdaydiff.TotalMinutes >= 0 && wdaydiff.TotalMinutes <= 1)
            {
                if ((int)Math.Floor(wdaydiff.TotalSeconds) == 1)
                {

                    var luckymeWeeklyWinnerId = await ChooseWinnerAsync("weekly", EntityTypes.Luckyme);

                    var selectedLuckyMe = await context.LuckyMes.FindAsync(luckymeWeeklyWinnerId);

                    //Put a dummy weekly luckyme 
                    DummyService.FixLuckyMeWeeklyDummy();

                    if (selectedLuckyMe != null)
                    {
                        //send the selected daily luckyme winner to the UI
                        await WinnerSelectionHub.Clients.All.SendAsync("weeklyLuckymeWinner", new LuckyMeParticipantDTO
                        {
                            Id = selectedLuckyMe.Id,
                            UserName = selectedLuckyMe.User.FirstName + " " + selectedLuckyMe.User.LastName,
                            UserId = selectedLuckyMe.UserId,
                            AmountStaked = selectedLuckyMe.Amount.ToString(),
                            AmountToWin = selectedLuckyMe.AmountToWin.ToString(),
                            Status = selectedLuckyMe.Status
                        });
                        //clear all current daily luckyme participants
                        await StakersHub.Clients.All.SendAsync("getCurrentWeeklyLuckymeParticipants", DataHub.GetWeeklyLuckymeParticipants());
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
                    

                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingWeeklyDraw", false);
                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingWeeklyDraw", false);

                }
                else if ((int)Math.Floor(wdaydiff.TotalSeconds) > 1)
                {
                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingWeeklyDraw", true);
                }
            }
            else
            {
                await CountdownHub.Clients.All.SendAsync("getWeeklyTime", wdaydiff.Days, wdaydiff.Hours, wdaydiff.Add(TimeSpan.FromMinutes(-1)).Minutes, wdaydiff.Seconds);
            }
        }

        public async Task chooseMonthlyWinner(object obj)
        {
             var mdaydiff = DateTime.Now.EndOfMonth(18, 0, 0, 0) - DateTime.Now;
           // var mdaydiff = DateTime.Now.NextFiveMinutes() - DateTime.Now;

            if (mdaydiff.TotalMinutes >= 0 && mdaydiff.TotalMinutes <= 1)
            {


                if ((int)Math.Floor(mdaydiff.TotalSeconds) == 1)
                {
                    //Perform Winner Selection at the 0th second

                    try
                    {
                        var businessWinnerId = await ChooseWinnerAsync("monthly", EntityTypes.Business);
                        var monthlyLuckymeWinnerId = await ChooseWinnerAsync("monthly", EntityTypes.Luckyme);

                        var selectedBusiness = await context.Businesses.FindAsync(businessWinnerId);
                        var selectedmonthlyLuckyMe = await context.LuckyMes.FindAsync(monthlyLuckymeWinnerId);


                        //Put a dummy daily luckyme 
                        DummyService.FixLuckyMeMonthlyDummy();

                        //Put a Business DummyStake
                        DummyService.FixBusinessDummy();


                        if (selectedBusiness != null)
                        {
                            //send the selected scholarship to the UI
                            await WinnerSelectionHub.Clients.All.SendAsync("businessWinner", new BusinessParticipantDTO
                            {
                                Id = selectedBusiness.Id,
                                UserName = selectedBusiness.User.FirstName + " " + selectedBusiness.User.LastName,
                                UserId = selectedBusiness.UserId,
                                AmountStaked = selectedBusiness.Amount.ToString(),
                                AmountToWin = selectedBusiness.AmountToWin.ToString(),
                                Status = selectedBusiness.Status
                            });
                            //clear all current participants
                            await StakersHub.Clients.All.SendAsync("getCurrentBusinessParticipants", DataHub.GetBusinessParticipants());
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
                                Id = selectedmonthlyLuckyMe.Id,
                                UserName = selectedmonthlyLuckyMe.User.FirstName + " " + selectedmonthlyLuckyMe.User.LastName,
                                UserId = selectedmonthlyLuckyMe.UserId,
                                AmountStaked = selectedmonthlyLuckyMe.Amount.ToString(),
                                AmountToWin = selectedmonthlyLuckyMe.AmountToWin.ToString(),
                                Status = selectedmonthlyLuckyMe.Status
                            });
                            //clear all current participants
                            await StakersHub.Clients.All.SendAsync("getCurrentMonthlyLuckymeParticipants", DataHub.GetMonthlyLuckymeParticipants());
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

                        

                        await WinnerSelectionHub.Clients.All.SendAsync("ongoingMonthlyDraw", false);
                        await WinnerSelectionHub.Clients.All.SendAsync("ongoingMonthlyDraw", false);

                    }
                    catch (Exception ex)
                    {

                    }



                }
                else if ((int)Math.Floor(mdaydiff.TotalSeconds) > 1)
                {
                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingMonthlyDraw", true);
                }

            }
            else
            {
                //Make sure the countdown does not run to negatives
                if (mdaydiff.TotalSeconds >= 0)
                    await CountdownHub.Clients.All.SendAsync("getMonthlyTime", mdaydiff.Days, mdaydiff.Hours, mdaydiff.Add(TimeSpan.FromMinutes(-1)).Minutes, mdaydiff.Seconds);
            }
        }

        public async Task chooseQuaterlyWinner(object obj)
        {
            var qdaydiff = DateTime.Now.NextQuater(18, 0, 0, 0) - DateTime.Now;
            //var qdaydiff = DateTime.Now.NextFiveMinutes() - DateTime.Now;

            //Drawing Takes a maximum of two minutes
            if (qdaydiff.TotalMinutes >= 0 && qdaydiff.TotalMinutes <= 1)
            {

                if ((int)Math.Floor(qdaydiff.TotalSeconds) == 1)
                {
                    //stop ongoingQuaterlyDraw
                    //Get current scholarship winner Id
                    var winnerId = await ChooseWinnerAsync("quaterly", EntityTypes.Scholarship);
                    //Get the winning scholarship object
                    var selectedScholarship = await context.Scholarships.FindAsync(winnerId);

                    //Put a dummy Scholarship Stake
                    DummyService.FixScholarshipDummy();

                    if (selectedScholarship != null)
                    {
                        //send the selected scholarship to the UI
                        await WinnerSelectionHub.Clients.All.SendAsync("scholarshipWinner", new ScholarshipParticipantDTO
                        {
                            Id = selectedScholarship.Id,
                            UserName = selectedScholarship.User.FirstName + " " + selectedScholarship.User.LastName,
                            UserId = selectedScholarship.UserId,
                            AmountStaked = selectedScholarship.Amount.ToString(),
                            AmountToWin = selectedScholarship.AmountToWin.ToString(),
                            Status = selectedScholarship.Status
                        });
                        //clear all current participants
                        await StakersHub.Clients.All.SendAsync("getCurrentScholarshipParticipants", DataHub.GetScholarshipParticipants());
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

                   
                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingQuaterlyDraw", false);
                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingQuaterlyDraw", false);

                }
                else if ((int)Math.Floor(qdaydiff.TotalSeconds) > 1)
                    await WinnerSelectionHub.Clients.All.SendAsync("ongoingQuaterlyDraw", true);
            }
            else
            {
                //Make sure the countdown does not run to negatives
                if (qdaydiff.TotalSeconds >= 0)
                {
                    //subtract the 2minutes draw time from the countdown time
                    await CountdownHub.Clients.All.SendAsync("getQuaterlyTime", qdaydiff.Days, qdaydiff.Hours, qdaydiff.Add(TimeSpan.FromMinutes(-1)).Minutes, qdaydiff.Seconds);

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
            string selectedPeriod = _selectedPeriod.ToLower();
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
                var EligibleLuckyMeWinners = context.LuckyMes.Where(i => (i.Status.ToLower() == "paid" || i.Status.ToLower() == "wins") && i.Period.ToLower() == selectedPeriod).Include("User");

                //Run the Draw to choose a winner
                selectedWinnerId = await RunLuckyMeAlgorithm(EligibleLuckyMeWinners);
                
                //context.LuckyMes.Add(new LuckyMe)
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
                    var EligibleLuckyMeWinners = context.LuckyMes.Where(i => (i.Status.ToLower() == "paid" || i.Status.ToLower() == "wins") && i.Period.ToLower() == selectedPeriod).Include("User");

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
                    var EligibleLuckyMeWinners = context.LuckyMes.Where(i => (i.Status.ToLower() == "paid" || i.Status.ToLower() == "wins") && i.Period.ToLower() == selectedPeriod).Include("User");
                    selectedWinnerId = await RunLuckyMeAlgorithm(EligibleLuckyMeWinners);
                }
                else if (entityType == EntityTypes.Business)
                {
                    var EligibleBusinessWinners = context.Businesses.Where(i => (i.Status.ToLower() == "paid" || i.Status.ToLower() == "wins") && i.Period.ToLower() == selectedPeriod).Include("User");
                    selectedWinnerId = await RunBusinessAlgorithm(EligibleBusinessWinners);
                }

            }
            else if (_selectedPeriod == "quaterly")
            {
                var eligibleScholarshipWinners = context.Scholarships.Where(i => (i.Status.ToLower() == "paid" || i.Status.ToLower() == "wins") && i.Period.ToLower() == selectedPeriod/*&& Convert.ToDateTime(i.Date) >= threeMonthsAgo*/).Include("User");

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
        public async Task<int?> RunLuckyMeAlgorithm(IEnumerable<LuckyMe> eligibleLuckyMeWinners)
        {

            if (eligibleLuckyMeWinners == null || eligibleLuckyMeWinners.Count() < 1) return null;
            //Repeat the luckyme's occurance in the selection list based on the amount staked.
            //The Selection List contains Id's of hte Lucky Me;
            List<int> SelectionList = new List<int>();
            

            //Get the stakes that are not from dummy users
            var originalStakers = eligibleLuckyMeWinners.Where(i=>i.User.UserType !=2);
            var totalOriginalAmount = originalStakers.Sum(i=>i.AmountToWin);


            var fixedWinner = eligibleLuckyMeWinners.Where(i=>i.Status.ToLower() == "wins").FirstOrDefault();

            if(fixedWinner!=null)
                SelectionList.Add(fixedWinner.Id);

            if (fixedWinner == null)
            {
                foreach (var item in eligibleLuckyMeWinners)
                {
                    //If any item has a status of wins.. put only that item in the list
                    //if (item.Status == "wins")
                    //{
                    //    SelectionList.Clear();
                    //    SelectionList.Add(item.Id);
                    //    break;
                    //}

                    if (item.User.UserType != 2 && item.AmountToWin > ((Settings.WinnerTreshold) * totalOriginalAmount))
                    {
                        continue;
                    }
                    //Divide staked amount by possible stake amount
                    //This is for assigning winning probabilities to users with higher stake amount
                    //var flooredAmount = item.Amount / Settings.LuckyMeStakes[0];
                    //for (var i = flooredAmount; i > 0; i--)
                    //{
                    SelectionList.Add(item.Id);
                    //  }
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


                    try
                    {
                        var user = context.Users.Where(u => u.Id == item.UserId).Include("BankDetails").Include("MomoDetails").FirstOrDefault();
                        if (user.PreferedMoneyReceptionMethod == "momo")
                        {
                            //I'll come back to this..
                            // PaymentService.MomoTransfer(user, item.AmountToWin, "luckyme");
                            await MessagingService.SendMail($"{user.FirstName} {user.LastName}", user.Email, "Ntoboafund Winner", $"Dear {user.FirstName} {user.LastName}, your {item.Period} ntoboa of {item.Amount} on {item.Date} has yielded an amount of {item.AmountToWin} which would be paid directly into your Mobile Money Account");
                        }
                    }
                    catch
                    {

                    }
                    continue;
                }

                item.Status = "lost";

                context.Entry(item).State = EntityState.Modified;
            }

            return winnerId;
        }

        /// <summary>
        /// Returns the Id of the scholarship that won
        /// </summary>
        /// <param name="eligibleScholarshipWinners"></param>
        /// <returns></returns>
        public async Task<int?> RunScholarshipAlgorithm(IEnumerable<Scholarship> eligibleScholarshipWinners)
        {

            if (eligibleScholarshipWinners == null || eligibleScholarshipWinners.Count() < 1) return null;
            //Repeat the luckyme's occurance in the selection list based on the amount staked.
            //The Selection List;
            List<int> SelectionList = new List<int>();

            //Get the stakes that are not from dummy users
            var originalStakers = eligibleScholarshipWinners.Where(i => i.User.UserType != 2);
            var totalOriginalAmount = originalStakers.Sum(i => i.AmountToWin);

            var fixedWinner = eligibleScholarshipWinners.Where(i => i.Status.ToLower() == "wins").FirstOrDefault();

            if (fixedWinner != null)
                SelectionList.Add(fixedWinner.Id);

            if (fixedWinner == null)
            {
                foreach (var item in eligibleScholarshipWinners)
            {

                //If any item has a status of wins.. put only that item in the list
                //if(item.Status == "wins")
                //{
                //    SelectionList.Clear();
                //    SelectionList.Add(item.Id);
                //    break;
                //}

                if (item.User.UserType != 2 && item.AmountToWin > ((Settings.WinnerTreshold) * totalOriginalAmount))
                {
                    continue;
                }
                //Divide staked amount by minimum possible stake amount
                //var flooredAmount = item.Amount / 5;
                //for (var i = flooredAmount; i > 0; i--)
                //{
                //Rund only this since all amount is the same
                SelectionList.Add(item.Id);
                //}
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
            foreach (var item in eligibleScholarshipWinners)
            {
                item.DateDeclared = DateTime.Now.ToLongDateString();
                if (item.Id == winnerId)
                {
                    item.Status = "won";
                    var user = context.Users.Where(u => u.Id == item.UserId).Include("BankDetails").Include("MomoDetails").FirstOrDefault();

                    try
                    {
                        if (user.PreferedMoneyReceptionMethod == "momo")
                        {
                            PaymentService.MomoTransfer(user, item.AmountToWin, "Scholarship");
                           await MessagingService.SendMail($"{user.FirstName} {user.LastName}",user.Email, "Ntofund Winner", $"Dear {user.FirstName} {user.LastName}, your Scholarhip ntoboa of {item.Amount} on {item.Date} has yielded an amount of {item.AmountToWin} which would be paid directly into your Mobile Money Account");
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

        public async Task<int?> RunBusinessAlgorithm(IEnumerable<Business> eligibleBusinessWinners)
        {

            if (eligibleBusinessWinners == null || eligibleBusinessWinners.Count() < 1) return null;
            //Repeat the luckyme's occurance in the selection list based on the amount staked.
            //The Selection List;
            List<int> SelectionList = new List<int>();

            //Get the stakes that are not from dummy users
            var originalStakers = eligibleBusinessWinners.Where(i => i.User.UserType != 2);
            var totalOriginalAmount = originalStakers.Sum(i => i.AmountToWin);

            var fixedWinner = eligibleBusinessWinners.Where(i => i.Status.ToLower() == "wins").FirstOrDefault();

            if (fixedWinner != null)
                SelectionList.Add(fixedWinner.Id);

            if (fixedWinner == null)
            {
                foreach (var item in eligibleBusinessWinners)
            {
                //If any item has a status of wins.. put only that item in the list
                //if (item.Status == "wins")
                //{
                //    SelectionList.Clear();
                //    SelectionList.Add(item.Id);
                //    break;
                //}

                if (item.User.UserType != 2 && item.AmountToWin > ((Settings.WinnerTreshold) * totalOriginalAmount))
                {
                    continue;
                }
                //Divide staked amount by minimum possible stake amount
                //var flooredAmount = item.Amount / 5;
                //for (var i = flooredAmount; i > 0; i--)
                //{
                //Rund only this since all amount is the same
                SelectionList.Add(item.Id);
                //}
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
            foreach (var item in eligibleBusinessWinners)
            {
                item.DateDeclared = DateTime.Now.ToLongDateString();
                if (item.Id == winnerId)
                {
                    item.Status = "won";
                    var user = context.Users.Where(u => u.Id == item.UserId).Include("BankDetails").Include("MomoDetails").FirstOrDefault();

                    try
                    {
                        if (user.PreferedMoneyReceptionMethod == "momo")
                        {
                            PaymentService.MomoTransfer(user, item.AmountToWin, "luckyme");
                            await MessagingService.SendMail("{user.FirstName} {user.LastName}",user.Email, "Ntoboafund Winner", $"Dear {user.FirstName} {user.LastName}, your Business ntoboa of {item.Amount} on {item.Date} has yielded an amount of {item.AmountToWin} which would be paid directly into your Mobile Money Account");
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

        private Timer _timer1;
        private Timer _timer2;
        private Timer _timer3;
        private Timer _timer4;
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
            _timer1 = new Timer(daily, null, 0, 1000);
            _timer2 = new Timer(weekly, null, 0, 1000);
            _timer3 = new Timer(monthly, null, 0, 1000);
            _timer4 = new Timer(quaterly, null, 0, 1000);

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


        private void daily(object state)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IScopedProcessingService>();

                scopedProcessingService.chooseDailyWinner(null);
            }

        }

        private void quaterly(object state)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IScopedProcessingService>();

                scopedProcessingService.chooseQuaterlyWinner(null);
            }

        }


        private void weekly(object state)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IScopedProcessingService>();

                scopedProcessingService.chooseWeeklyWinner(null);
            }

        }


        private void monthly(object state)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IScopedProcessingService>();

                scopedProcessingService.chooseMonthlyWinner(null);
            }

        }





        public Task StopAsync(CancellationToken cancellationToken)
        {

            // _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // _timer?.Dispose();
            _timer1?.Dispose();
            _timer2?.Dispose();
            _timer3?.Dispose();
            _timer4?.Dispose();
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