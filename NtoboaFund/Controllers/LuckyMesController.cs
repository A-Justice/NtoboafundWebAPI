using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.DTO_s;
using NtoboaFund.Data.Models;
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
        private readonly NtoboaFundDbContext _context;

        public LuckyMesController(NtoboaFundDbContext context)
        {
            _context = context;
        }

        // GET: api/LuckyMes
        [HttpGet]
        public IEnumerable<LuckyMe> GetLuckyMes()
        {
            return _context.LuckyMes;
        }

        [HttpGet("bytype/{type}")]
        public IEnumerable<LuckyMe> GetLuckyMesByType(string type)
        {
            if(type.ToLower() == "all")
                return _context.LuckyMes;
            else if(type.ToLower() == "2")
                return _context.LuckyMes.Where(i=>i.User.UserType.ToString() == type.ToLower());
            else
                return _context.LuckyMes.Where(i => i.User.UserType != 2);
        }


        [HttpGet("withusers")]
        public async Task<IEnumerable<LuckyMe>> GetLuckyMesWithUsers()
        {
            return await _context.LuckyMes.Include("User").ToListAsync();
        }

        [HttpGet("foruser/{userId}")]
        public IEnumerable<LuckyMe> GetLuckyMes([FromRoute]string userId)
        {
            return _context.LuckyMes.Where(l => l.UserId == userId);
        }

        [HttpGet("bystatus/{status}")]
        public IEnumerable<LuckyMe> GetLuckyMesByStatus([FromRoute] string status)
        {
            if(status.ToLower() == "all")
                return GetLuckyMes();
            return _context.LuckyMes.Where(i=>i.Status.ToLower() == status);
        }

        [AllowAnonymous]
        [HttpGet("winners")]
        public IEnumerable<LuckyMe> Winners()
        {
            return _context.LuckyMes.Where(l => l.Status == "won").Include("User");
        }

        // GET: api/LuckyMes/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLuckyMe([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var luckyMe = await _context.LuckyMes.FindAsync(id);

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

            _context.Entry(luckyMe).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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
        [HttpPost]
        public async Task<IActionResult> PostLuckyMe([FromBody] LuckyMe luckyMe)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.LuckyMes.Add(luckyMe);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLuckyMe", new { id = luckyMe.Id }, luckyMe);
        }

        // DELETE: api/LuckyMes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLuckyMe([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var luckyMe = await _context.LuckyMes.FindAsync(id);
            if (luckyMe == null)
            {
                return NotFound();
            }

            _context.LuckyMes.Remove(luckyMe);
            await _context.SaveChangesAsync();

            return Ok(luckyMe);
        }

        private bool LuckyMeExists(int id)
        {
            return _context.LuckyMes.Any(e => e.Id == id);
        }
    }
}