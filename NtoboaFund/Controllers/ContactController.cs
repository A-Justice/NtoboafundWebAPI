using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.Models;

namespace NtoboaFund.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ContactUsController : ControllerBase
    {
        public ContactUsController(NtoboaFundDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public NtoboaFundDbContext DbContext { get; }

        [HttpPost("postmessage")]
        public async Task<IActionResult> PostContactUsRequest(ContactUs contactUs)
        {
            try
            {
                await DbContext.ContactUs.AddAsync(contactUs);
                await DbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}