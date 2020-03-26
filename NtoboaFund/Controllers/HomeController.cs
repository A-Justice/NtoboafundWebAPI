using Microsoft.AspNetCore.Mvc;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.DTO_s;
using NtoboaFund.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NtoboaFund.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly NtoboaFundDbContext dbContext;
        public HomeController(NtoboaFundDbContext context)
        {
            dbContext = context;
        }

        [HttpGet("allwinners")]
        public IEnumerable<ParticipantDTO> GetAllWinners()
        {

            List<ParticipantDTO> winners = new List<ParticipantDTO>();
            var lCount = dbContext.LuckyMes.Where(l => l.Status == "won").Count();
            var bCount = dbContext.Businesses.Where(l => l.Status == "won").Count();
            var sCount = dbContext.Scholarships.Where(l => l.Status == "won").Count();

            var lSkip = lCount > 10 ? lCount - 10 : 0;
            var bSkip = bCount > 10 ? bCount - 10 : 0;
            var sSkip = sCount > 10 ? sCount - 10 : 0;

            var luckyMeWinners = dbContext.LuckyMes.Where(l => l.Status == "won").Skip(lSkip).Select(i => new ParticipantDTO
            {
                Id = i.Id,
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString("0.##"),
                AmountToWin = i.AmountToWin.ToString("0.##"),
                Status = i.Status,
                DateDeclared = i.DateDeclared

            });
            var scholarshipWinners = dbContext.Scholarships.Where(l => l.Status == "won").Skip(sSkip).Select(i => new ParticipantDTO
            {
                Id = i.Id,
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString("0.##"),
                AmountToWin = i.AmountToWin.ToString("0.##"),
                Status = i.Status,
                DateDeclared = i.DateDeclared

            });
            var businessWinners = dbContext.Businesses.Where(l => l.Status == "won").Skip(bSkip).Select(i => new ParticipantDTO
            {
                Id = i.Id,
                UserName = i.User.FirstName + " " + i.User.LastName,
                UserId = i.UserId,
                AmountStaked = i.Amount.ToString("0.##"),
                AmountToWin = i.AmountToWin.ToString("0.##"),
                Status = i.Status,
                DateDeclared = i.DateDeclared

            });

            winners.AddRange(luckyMeWinners);
            winners.AddRange(scholarshipWinners);
            winners.AddRange(businessWinners);

            var orderedWinners = winners.OrderByDescending(i => DateTime.Parse(i.DateDeclared)).ToList();


            return orderedWinners;
        }

        [HttpGet("resetpts")]
        public IActionResult ResetPoints()
        {
            foreach (var item in dbContext.Users)
            {
                item.Points = 0.00M;
                dbContext.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
            foreach (var item in dbContext.LuckyMes.Where(i => i.User.UserType == 0 && i.Status != "pending" && i.Status != "failed"))
            {
                item.User.Points += (item.Amount * Constants.PointConstant);
                dbContext.Entry(item.User).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
            foreach (var item in dbContext.Businesses.Where(i => i.User.UserType == 0 && i.Status != "pending" && i.Status != "failed"))
            {
                item.User.Points += (item.Amount * Constants.PointConstant);
                dbContext.Entry(item.User).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
            foreach (var item in dbContext.Scholarships.Where(i => i.User.UserType == 0 && i.Status != "pending" && i.Status != "failed"))
            {
                item.User.Points += (item.Amount * Constants.PointConstant);
                dbContext.Entry(item.User).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }


            dbContext.SaveChanges();
            return Ok();
        }
    }
}