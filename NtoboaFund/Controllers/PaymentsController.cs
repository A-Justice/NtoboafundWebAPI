using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.Models;

namespace NtoboaFund.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly NtoboaFundDbContext _context;

        public PaymentsController(NtoboaFundDbContext context)
        {
            _context = context;
        }

        // GET: api/Payments
        [HttpGet]
        public IEnumerable<Payment> GetPayments()
        {
            return _context.Payments;
        }

        // GET: api/Payments/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var payment = await _context.Payments.FindAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }

        [HttpGet("bydetails/{itemPayedFor}/{itemPayedForId}")]
        public async Task<IActionResult> GetPaymentByDetails([FromRoute]string itemPayedFor, [FromRoute]int itemPayedForId)
        {
            var oldSamePayment = _context.Payments.Where(i => i.ItemPayedFor == itemPayedFor && i.ItemPayedForId == itemPayedForId).FirstOrDefault();

            return Ok(oldSamePayment);

        }
        // PUT: api/Payments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPayment([FromRoute] int id, [FromBody] Payment payment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != payment.PaymentId)
            {
                return BadRequest();
            }


            try
            {
                _context.Entry(payment).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine(ex.Message);
                if (!PaymentExists(id))
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

        // POST: api/Payments
        [HttpPost]
        public async Task<IActionResult> PostPayment([FromBody] Payment payment)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            payment.DatePayed = DateTime.Now;
            var oldSamePayment = _context.Payments.Where(i=>i.ItemPayedFor==payment.ItemPayedFor && i.ItemPayedForId == payment.ItemPayedForId).FirstOrDefault();
            if(oldSamePayment == null)
            {
                _context.Payments.Add(payment);
            }
            else
            {
                oldSamePayment.TransactionId = payment.TransactionId;
                oldSamePayment.DatePayed = payment.DatePayed;
                oldSamePayment.PayerId = payment.PayerId;
                _context.Entry(oldSamePayment).State = EntityState.Modified;
            }

                await _context.SaveChangesAsync();
                return CreatedAtAction("GetPayment", new { id = payment.PaymentId }, payment);
           
        }

        // DELETE: api/Payments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return Ok(payment);
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.PaymentId == id);
        }
    }
}