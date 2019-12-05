using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.Models;
using NtoboaFund.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NtoboaFund.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class LuckyMesController : ControllerBase
    {
        private readonly NtoboaFundDbContext dbContext;
        private readonly AppSettings AppSettings;
        public LuckyMesController(NtoboaFundDbContext context, IOptions<AppSettings> appSettings)
        {
            dbContext = context;
            AppSettings = appSettings.Value;
        }

        // GET: api/LuckyMes
        [HttpGet]
        public IEnumerable<LuckyMe> GetLuckyMes()
        {
            return dbContext.LuckyMes;
        }

        [HttpGet("bytype/{type}")]
        public IEnumerable<LuckyMe> GetLuckyMesByType(string type)
        {
            if (type.ToLower() == "all")
                return dbContext.LuckyMes;
            else if (type.ToLower() == "2")
                return dbContext.LuckyMes.Where(i => i.User.UserType.ToString() == type.ToLower());
            else
                return dbContext.LuckyMes.Where(i => i.User.UserType != 2);
        }


        [HttpGet("withusers")]
        public async Task<IEnumerable<LuckyMe>> GetLuckyMesWithUsers()
        {
            return await dbContext.LuckyMes.Include("User").ToListAsync();
        }

        [HttpGet("foruser/{userId}")]
        public IEnumerable<LuckyMe> GetLuckyMes([FromRoute]string userId)
        {
            return dbContext.LuckyMes.Where(l => l.UserId == userId);
        }

        [HttpGet("bystatus/{status}")]
        public IEnumerable<LuckyMe> GetLuckyMesByStatus([FromRoute] string status)
        {
            if (status.ToLower() == "all")
                return GetLuckyMes();
            return dbContext.LuckyMes.Where(i => i.Status.ToLower() == status);
        }

        [AllowAnonymous]
        [HttpGet("winners")]
        public IEnumerable<LuckyMe> Winners()
        {
            return dbContext.LuckyMes.Where(l => l.Status == "won").Include("User");
        }

        // GET: api/LuckyMes/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLuckyMe([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var luckyMe = await dbContext.LuckyMes.FindAsync(id);

            if (luckyMe == null)
            {
                return NotFound();
            }

            return Ok(luckyMe);
        }

        // PUT: api/LuckyMes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLuckyMe([FromRoute] int id, [FromBody] LuckyMe luckyMe)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != luckyMe.Id)
            {
                return BadRequest();
            }

            dbContext.Entry(luckyMe).State = EntityState.Modified;

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LuckyMeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/LuckyMes
        [HttpPost("addnew")]
        public async Task<IActionResult> PostLuckyMe([FromBody] LuckyMe luckyMe)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            luckyMe.Date = DateTime.Now.ToLongDateString();
            luckyMe.AmountToWin = luckyMe.Amount * Constants.LuckymeStakeOdds;
            luckyMe.Status = "pending";
            luckyMe.User = await dbContext.Users.FindAsync(luckyMe.UserId);
            dbContext.LuckyMes.Add(luckyMe);

            await dbContext.SaveChangesAsync();

            //if (Constants.PaymentGateway == PaymentGateway.slydepay)
            //    return Ok(new { id = luckyMe.Id, paymentToken = await Misc.GenerateSlydePayToken(EntityTypes.Luckyme, luckyMe, AppSettings.SlydePaySettings) });
            //else
            return Ok(new { id = luckyMe.Id });
        }

        // DELETE: api/LuckyMes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLuckyMe([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var luckyMe = await dbContext.LuckyMes.FindAsync(id);
            if (luckyMe == null)
            {
                return NotFound();
            }

            dbContext.LuckyMes.Remove(luckyMe);
            await dbContext.SaveChangesAsync();

            return Ok(luckyMe);
        }

        private bool LuckyMeExists(int id)
        {
            return dbContext.LuckyMes.Any(e => e.Id == id);
        }
    }
}