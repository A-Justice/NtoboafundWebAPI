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
    [Route("[controller]")]
    [ApiController]
    public class BusinessesController : ControllerBase
    {

        private readonly NtoboaFundDbContext dbContext;

        private readonly AppSettings AppSettings;


        public BusinessesController(IOptions<AppSettings> appSettings, NtoboaFundDbContext context)
        {
            AppSettings = appSettings.Value;
            dbContext = context;
        }

        // GET: api/Businesses
        [HttpGet]
        public IEnumerable<Business> GetBusinesses()
        {
            return dbContext.Businesses;
        }

        [HttpGet("foruser/{userId}")]
        public IEnumerable<Business> GetBusinesses([FromRoute]string userId)
        {
            return dbContext.Businesses.Where(l => l.UserId == userId);
        }

        [HttpGet("bystatus/{status}")]
        public IEnumerable<Business> GetBusinessesByStatus([FromRoute] string status)
        {
            if (status.ToLower() == "all")
                return GetBusinesses();

            return dbContext.Businesses.Where(i => i.Status.ToLower() == status);
        }

        [HttpGet("bytype/{type}")]
        public IEnumerable<Business> GetBusinessesByType(string type)
        {
            if (type.ToLower() == "all")
                return dbContext.Businesses;
            else if (type.ToLower() == "2")
                return dbContext.Businesses.Where(i => i.User.UserType.ToString() == type.ToLower());
            else
                return dbContext.Businesses.Where(i => i.User.UserType != 2);
        }

        [HttpGet("unpaidwinnerscount")]
        public async Task<int> GetUnpaidWinnersCount()
        {
            return await dbContext.Businesses.Where(i => i.Status.ToLower() == "won" && i.User.UserType == 0).CountAsync();
        }



        [AllowAnonymous]
        [HttpGet("winners")]
        public IEnumerable<Business> Winners()
        {
            //Request
            return dbContext.Businesses.Where(l => l.Status == "won").Include("User");
        }

        // GET: api/Businesses/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBusiness([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var luckyMe = await dbContext.Businesses.FindAsync(id);

            if (luckyMe == null)
            {
                return NotFound();
            }

            return Ok(luckyMe);
        }

        // PUT: api/Businesses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBusiness([FromRoute] int id, [FromBody] Business luckyMe)
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
                if (!BusinessExists(id))
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

        // POST: api/Businesses
        [HttpPost("addnew")]
        public async Task<IActionResult> PostBusiness([FromBody] Business business)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            business.Date = DateTime.Now.ToLongDateString();
            business.AmountToWin = (business.Amount * Constants.BusinessStakeOdds);
            business.Status = "Pending";
            business.Period = "monthly";
            business.User = dbContext.Users.Find(business.UserId);

            dbContext.Businesses.Add(business);

            await dbContext.SaveChangesAsync();

            //if (Constants.PaymentGateway == PaymentGateway.slydepay)
            //    return Ok(new { id = business.Id, paymentToken = Misc.GenerateSlydePayToken(EntityTypes.Business,business, AppSettings.SlydePaySettings) });
            //else
            return Ok(new { id = business.Id });

        }

        // DELETE: api/Businesses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBusiness([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var luckyMe = await dbContext.Businesses.FindAsync(id);
            if (luckyMe == null)
            {
                return NotFound();
            }

            dbContext.Businesses.Remove(luckyMe);
            await dbContext.SaveChangesAsync();

            return Ok(luckyMe);
        }

        private bool BusinessExists(int id)
        {
            return dbContext.Businesses.Any(e => e.Id == id);
        }
    }
}