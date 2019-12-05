using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class PaymentsController : ControllerBase
    {
        private readonly NtoboaFundDbContext dbContext;

        public PaymentsController(NtoboaFundDbContext context)
        {
            dbContext = context;
        }

        // GET: api/Payments
        [HttpGet]
        public IEnumerable<Payment> GetPayments()
        {
            return dbContext.Payments;
        }

        // GET: api/Payments/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var payment = await dbContext.Payments.FindAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }


        [HttpGet("bydetails/{itemPayedFor}/{itemPayedForId}")]
        public async Task<IActionResult> GetPaymentByDetails([FromRoute]string itemPayedFor, [FromRoute]int itemPayedForId)
        {
            var oldSamePayment = dbContext.Payments.Where(i => i.ItemPayedFor == itemPayedFor && i.ItemPayedForId == itemPayedForId).FirstOrDefault();

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
                dbContext.Entry(payment).State = EntityState.Modified;
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                //Console.WriteLine(ex.Message);
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
            var oldSamePayment = dbContext.Payments.Where(i => i.ItemPayedFor == payment.ItemPayedFor && i.ItemPayedForId == payment.ItemPayedForId).FirstOrDefault();
            if (oldSamePayment == null)
            {
                dbContext.Payments.Add(payment);
            }
            else
            {
                dbContext.Entry(oldSamePayment).State = EntityState.Modified;
            }

            await dbContext.SaveChangesAsync();
            return CreatedAtAction("GetPayment", new { id = payment.PaymentId }, payment);

        }


        [HttpGet("congratmsg/{type}/{txRef}")]
        public async Task<ActionResult<string>> GetCongratulatoryMessage(string type, string txRef)
        {
            string message = "";
            if (type == "lkm")
            {
                var luckyMe = dbContext.LuckyMes.Where(i => i.TxRef == txRef).FirstOrDefault();
                message = Misc.GetDrawMessage(EntityTypes.Luckyme, luckyMe.Amount, luckyMe.Period);

            }
            else if (type == "bus")
            {
                var business = dbContext.Businesses.Where(i => i.TxRef == txRef).FirstOrDefault();
                message = Misc.GetDrawMessage(EntityTypes.Business, business.Amount, business.Period);

            }
            else if (type == "sch")
            {
                var scholarship = dbContext.Scholarships.Where(i => i.TxRef == txRef).FirstOrDefault();
                message = Misc.GetDrawMessage(EntityTypes.Scholarship, scholarship.Amount, scholarship.Period);

            }
            return Ok(new { message = message });
        }

        // DELETE: api/Payments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var payment = await dbContext.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            dbContext.Payments.Remove(payment);
            await dbContext.SaveChangesAsync();

            return Ok(payment);
        }

        private bool PaymentExists(int id)
        {
            return dbContext.Payments.Any(e => e.PaymentId == id);
        }
    }
}