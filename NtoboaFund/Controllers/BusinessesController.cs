using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NtoboaFund.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BusinessesController : ControllerBase
    {

        private readonly NtoboaFundDbContext _context;

        public BusinessesController(NtoboaFundDbContext context)
        {
            _context = context;
        }

        // GET: api/Businesses
        [HttpGet]
        public IEnumerable<Business> GetBusinesses()
        {
            return _context.Businesses;
        }

        [HttpGet("foruser/{userId}")]
        public IEnumerable<Business> GetBusinesses([FromRoute]string userId)
        {
            return _context.Businesses.Where(l => l.UserId == userId);
        }

        [AllowAnonymous]
        [HttpGet("winners")]
        public IEnumerable<Business> Winners()
        {
            //Request
            return _context.Businesses.Where(l => l.Status == "won").Include("User");
        }

        // GET: api/Businesses/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBusiness([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var luckyMe = await _context.Businesses.FindAsync(id);

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

            _context.Entry(luckyMe).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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
        [HttpPost]
        public async Task<IActionResult> PostBusiness([FromBody] Business luckyMe)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Businesses.Add(luckyMe);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBusiness", new { id = luckyMe.Id }, luckyMe);
        }

        // DELETE: api/Businesses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBusiness([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var luckyMe = await _context.Businesses.FindAsync(id);
            if (luckyMe == null)
            {
                return NotFound();
            }

            _context.Businesses.Remove(luckyMe);
            await _context.SaveChangesAsync();

            return Ok(luckyMe);
        }

        private bool BusinessExists(int id)
        {
            return _context.Businesses.Any(e => e.Id == id);
        }
    }
}