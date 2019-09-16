using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NtoboaFund.Services;

namespace NtoboaFund.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class AnalysisController : ControllerBase
    {
        public AnalysisController(AnalysisService analysisService)
        {
            AnalysisService = analysisService;
        }

        public AnalysisService AnalysisService { get; }

        [HttpGet()]
        public async Task<IActionResult> GetAnalysis()
        {
            var analysis = AnalysisService.GetAnalysis();
            
            return Ok(analysis);
        }
    }
}