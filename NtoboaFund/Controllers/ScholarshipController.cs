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
    public class ScholarshipsController : ControllerBase
    {
        private readonly NtoboaFundDbContext dbContext;
        private readonly AppSettings AppSettings;
        public ScholarshipsController(IOptions<AppSettings> appSettings, NtoboaFundDbContext context)
        {
            AppSettings = appSettings.Value;
            dbContext = context;
        }

        // GET: api/Scholarships
        [HttpGet]
        public IEnumerable<Scholarship> GetScholarships()
        {
            return dbContext.Scholarships.Where(i=>i.deleted == false);
        }

        [HttpGet("foruser/{userId}")]
        public IEnumerable<Scholarship> GetScholarships([FromRoute]string userId)
        {
            return dbContext.Scholarships.Where(i=>i.deleted == false).Where(l => l.UserId == userId);
        }

        [HttpGet("bystatus/{status}")]
        public IEnumerable<Scholarship> GetScholarshipsByStatus([FromRoute] string status)
        {
            if (status.ToLower() == "all")
                return GetScholarships();

            return dbContext.Scholarships.Where(i=>i.deleted == false).Where(i => i.Status.ToLower() == status);
        }

        [HttpGet("unpaidwinnerscount")]
        public async Task<int> GetUnpaidWinnersCount()
        {
            return await dbContext.Scholarships.Where(i=>i.deleted == false).Where(i => i.Status.ToLower() == "won" && i.User.UserType == 0).CountAsync();
        }


        [HttpGet("bytype/{type}")]
        public IEnumerable<Scholarship> GetScholarshipsByType(string type)
        {
            if (type.ToLower() == "all")
                return dbContext.Scholarships.Where(i=>i.deleted == false);
            else if (type.ToLower() == "2")
                return dbContext.Scholarships.Where(i=>i.deleted == false).Where(i => i.User.UserType.ToString() == type.ToLower());
            else
                return dbContext.Scholarships.Where(i=>i.deleted == false).Where(i => i.User.UserType != 2);
        }



        [AllowAnonymous]
        [HttpGet("winners")]
        public IEnumerable<Scholarship> Winners()
        {
            //Request
            return dbContext.Scholarships.Where(i=>i.deleted == false).Where(l => l.Status == "won").Include("User");
        }

        // GET: api/Scholarships/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetScholarship([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var Scholarship = await dbContext.Scholarships.FindAsync(id);

            if (Scholarship == null)
            {
                return NotFound();
            }

            return Ok(Scholarship);
        }

        // PUT: api/Scholarships/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutScholarship([FromRoute] int id, [FromBody] Scholarship Scholarship)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != Scholarship.Id)
            {
                return BadRequest();
            }

            dbContext.Entry(Scholarship).State = EntityState.Modified;

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScholarshipExists(id))
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

        // POST: api/Scholarships
        [HttpPost("addnew")]
        public async Task<IActionResult> PostScholarship([FromBody] Scholarship scholarship)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //scholarship.Amount = scholarship.Amount;
            scholarship.Date = DateTime.Now.ToLongDateString();
            scholarship.AmountToWin = (scholarship.Amount * Constants.ScholarshipStakeOdds);
            scholarship.Status = "Pending";
            scholarship.Period = "quaterly";
            scholarship.User = dbContext.Users.Find(scholarship.UserId);


            dbContext.Scholarships.Add(scholarship);
            await dbContext.SaveChangesAsync();

            //if (Constants.PaymentGateway == PaymentGateway.slydepay)
            //    return Ok(new { id = scholarship.Id, paymentToken = Misc.GenerateSlydePayToken(EntityTypes.Scholarship, scholarship, AppSettings.SlydePaySettings) });
            //else
            return Ok(new { id = scholarship.Id });
        }

        // DELETE: api/Scholarships/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScholarship([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var Scholarship = await dbContext.Scholarships.FindAsync(id);
            if (Scholarship == null)
            {
                return NotFound();
            }

            dbContext.Scholarships.Remove(Scholarship);
            await dbContext.SaveChangesAsync();

            return Ok(Scholarship);
        }

        private bool ScholarshipExists(int id)
        {
            return dbContext.Scholarships.Where(i=>i.deleted == false).Any(e => e.Id == id);
        }
    }
}