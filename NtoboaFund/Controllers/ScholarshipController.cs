using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.Models;

namespace NtoboaFund.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class ScholarshipsController : ControllerBase
    {
        private readonly NtoboaFundDbContext _context;

        public ScholarshipsController(NtoboaFundDbContext context)
        {
            _context = context;
        }

        // GET: api/Scholarships
        [HttpGet]
        public IEnumerable<Scholarship> GetScholarships()
        {
            return _context.Scholarships;
        }

        [HttpGet("foruser/{userId}")]
        public IEnumerable<Scholarship> GetScholarships([FromRoute]string userId)
        {
            return _context.Scholarships.Where(l=>l.UserId == userId);
        }

        [AllowAnonymous]
        [HttpGet("winners")]
        public IEnumerable<Scholarship> Winners()
        {
            //Request
            return _context.Scholarships.Where(l => l.Status == "won").Include("User");
        }

        // GET: api/Scholarships/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetScholarship([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var Scholarship = await _context.Scholarships.FindAsync(id);

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

            _context.Entry(Scholarship).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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
        [HttpPost]
        public async Task<IActionResult> PostScholarship([FromBody] Scholarship Scholarship)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Scholarships.Add(Scholarship);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetScholarship", new { id = Scholarship.Id }, Scholarship);
        }

        // DELETE: api/Scholarships/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScholarship([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var Scholarship = await _context.Scholarships.FindAsync(id);
            if (Scholarship == null)
            {
                return NotFound();
            }

            _context.Scholarships.Remove(Scholarship);
            await _context.SaveChangesAsync();

            return Ok(Scholarship);
        }

        private bool ScholarshipExists(int id)
        {
            return _context.Scholarships.Any(e => e.Id == id);
        }
    }
}