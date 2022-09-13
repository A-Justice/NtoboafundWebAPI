using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.DTOs;
using NtoboaFund.Data.Models;
using NtoboaFund.Helpers;

namespace NtoboaFund.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CrowdfundController: ControllerBase
    {
        private readonly NtoboaFundDbContext dbContext;
        private readonly AppSettings AppSettings;
        public IMapper Mapper { get; set; }
        public CrowdfundController(NtoboaFundDbContext context, IOptions<AppSettings> appSettings, IMapper mapper)
        {
            dbContext = context;
            AppSettings = appSettings.Value;
            Mapper = mapper;
        }

        
        [HttpPost("add")]
        public async Task<IActionResult> Add([FromForm]CrowdFund _fund)
        {
            try
            {
                var fund = await updateImages(_fund);
                dbContext.CrowdFunds.Add(fund);
                dbContext.SaveChanges();
                return new CreatedAtActionResult("Add","CrowdFund","",fund);
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message,stackTrace = ex.StackTrace });
            }
           
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromForm]CrowdFund _fund)
        {
            try
            {
                var crowdfund = dbContext.CrowdFunds.Find(_fund.Id);
                var fund = await updateImages(_fund);
                crowdfund.Title = fund.Title;
                crowdfund.MainImageUrl = fund.MainImageUrl;
                crowdfund.SecondImageUrl = fund.SecondImageUrl;
                crowdfund.ThirdImageUrl = fund.ThirdImageUrl;
                crowdfund.MainImageUrl = fund.MainImageUrl;
                crowdfund.videoUrl = fund.videoUrl;
                crowdfund.EndDate = fund.EndDate;
                dbContext.Entry(crowdfund).State = EntityState.Modified;
                dbContext.SaveChanges();
                return  Ok(crowdfund);
            }
            catch (Exception ex)
            {
                return new BadRequestResult();
            }

        }

        async Task<CrowdFund> updateImages(CrowdFund fund)
        {
            var imageDirectory = Directory.GetCurrentDirectory() + "/wwwroot/images";
            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);
            }
            string combinedTitle = fund.Title.Replace(' ', '_') + DateTime.Now.Ticks.ToString();
            if (fund?.MainImage?.Length > 0)
            {
                var filePath = $"{imageDirectory}/{combinedTitle}.MainImage{Path.GetExtension(fund.MainImage.FileName)}";

                using (var stream = System.IO.File.Create(filePath))
                {
                    await fund.MainImage.CopyToAsync(stream);
                }
                fund.MainImageUrl = $"/images/{combinedTitle}.MainImage{Path.GetExtension(fund.MainImage.FileName)}";
            }
            if (fund?.SecondImage?.Length > 0)
            {
                var filePath = $"{imageDirectory}/{combinedTitle}.SecondImage{Path.GetExtension(fund.SecondImage.FileName)}";

                using (var stream = System.IO.File.Create(filePath))
                {
                    await fund.SecondImage.CopyToAsync(stream);
                }
                fund.SecondImageUrl = $"/images/{combinedTitle}.SecondImage{Path.GetExtension(fund.SecondImage.FileName)}";
            }
            if (fund?.ThirdImage?.Length > 0)
            {
                var filePath = $"{imageDirectory}/{combinedTitle}.ThirdImage{Path.GetExtension(fund.ThirdImage.FileName)}";

                using (var stream = System.IO.File.Create(filePath))
                {
                    await fund.ThirdImage.CopyToAsync(stream);
                }
                fund.ThirdImageUrl = $"/images/{combinedTitle}.ThirdImage{Path.GetExtension(fund.ThirdImage.FileName)}";
            }
            if (fund?.Video?.Length > 0)
            {
                var videoDirectory = Directory.GetCurrentDirectory() + "/wwwroot/videos";
                if (!Directory.Exists(videoDirectory))
                {
                    Directory.CreateDirectory(videoDirectory);
                }

                var filePath = $"{imageDirectory}/{combinedTitle}.Video{Path.GetExtension(fund.Video.FileName)}";

                using (var stream = System.IO.File.Create(filePath))
                {
                    await fund.Video.CopyToAsync(stream);
                }
                fund.videoUrl = $"/videos/{combinedTitle}.Video{Path.GetExtension(fund.ThirdImage.FileName)}";
            }

            return fund;
        }

        [AllowAnonymous]
        [HttpGet("single/{id}")]
        public ActionResult GetSingle(int id)
        {
            try
            {
                var crowdfund = dbContext.CrowdFunds.Where(i => i.Id == id).Include("CrowdFundType").Include("User").FirstOrDefault();

                var crowdfundForReturn = Mapper.Map<CrowdFundForReturnDTO>(crowdfund);

                var usersCount = dbContext.Donations.Where(i => i.CrowdFundId == id && i.paid == true).Select(i => i.UserId).Distinct().Count();

                crowdfundForReturn.PeopleContributed = usersCount;

                if (crowdfundForReturn != null)
                {
                    return Ok(crowdfundForReturn);
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [AllowAnonymous]
        [HttpGet("donations/forfund/{crowdfundId}")]
        public ActionResult GetDonationsForFund(int crowdfundId)
        {
            try
            {
                var donations = dbContext.Donations.Where(i => i.CrowdFundId == crowdfundId && i.paid == true).Include("User").ToList();
                var donationsForReturn = Mapper.Map<List<DonationForReturnDTO>>(donations);
                return Ok(donationsForReturn);
            }
            catch
            {
                return BadRequest(new { message = "An unknown error occured" });
            }
           
        }

        [AllowAnonymous]
        [HttpGet("totalpeoplecontributed/{crowdfundId}")]
        public ActionResult GetTotalPeopleContributed(int crowdfundId)
        {
            try
            {
                var usersCount = dbContext.Donations.Where(i => i.CrowdFundId == crowdfundId && i.paid == true).Select(i =>i.UserId).Distinct().Count();
                return Ok(new { usersCount });
            }
            catch
            {
                return BadRequest(new { message = "An unknown error occured" });
            }

        }

        [AllowAnonymous]
        [HttpGet("all/{take}/{skip}")]
        public ActionResult All(int take,int skip)
        {
            try
            {
                var crowdfunds = dbContext.CrowdFunds.OrderByDescending(i=>i.Id).Skip(skip).Take(take).ToList();


                if(crowdfunds != null)
                {
                    return Ok(crowdfunds);
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }


        [HttpDelete("delete/{id}")]
        public ActionResult Get(int id)
        {
            try
            {
                var crowdfund = dbContext.CrowdFunds.Find(id);


                if (crowdfund != null)
                {
                    dbContext.Remove(crowdfund);
                    dbContext.SaveChanges();
                    return Ok(crowdfund);
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [AllowAnonymous]
        [HttpGet("types/all")]
        public ActionResult categories()
        {
            try
            {
                var crowdfundTypes = dbContext.CrowdFundTypes.ToList();


                return Ok(crowdfundTypes);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("foruser/{userId}")]
        public ActionResult GetUserCrowdFunds(string userId)
        {
            try
            {
                var crowdfunds = dbContext.CrowdFunds.Where(i=>i.UserId == userId);

                if (crowdfunds != null)
                {
                    return Ok(crowdfunds);
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPost("Donate")]
        public async Task<ActionResult> Donate([FromBody] Donation donation)
        {
            try
            {
                donation.Date = DateTime.Now.ToLongDateString();
                donation.paid = false;
                dbContext.Donations.Add(donation);
                await dbContext.SaveChangesAsync();
                return Ok(new { message = "Donation added" });
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
           
        }
    }
}
