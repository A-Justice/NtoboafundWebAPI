//using hubtelapi_dotnet_v1.Hubtel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NtoboaFund.Data.DTO_s;
using NtoboaFund.Data.Models;
using NtoboaFund.Services;
using System;
using System.Threading.Tasks;

namespace NtoboaFund.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("getuser/{Id}")]
        public IActionResult GetUser(string Id)
        {
            return Ok(_userService.GetUser(Id));
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]AuthViewModel credentials)
        {
            var user = _userService.Authenticate(credentials.Email, credentials.Password);

            if (user == null)
                return BadRequest("Username or password is incorrect");

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromForm]RegistrationDTO regDTO)
        {
            Tuple<ApplicationUser, string> result = null;
            if (ModelState.IsValid)
            {
                result = _userService.Register(regDTO);

            }
            else
            {
                return BadRequest("Form Contains Invalid Fields");
            }

            if (result.Item2 == null)
                return Ok(result.Item1);

            return BadRequest(result.Item2);
        }

        // PUT: api/User/5
        [HttpPut("update")]
        public async Task<IActionResult> EditUser([FromForm]UserEditDTO regDTO)
        {
            ApplicationUser result = null;
            if (ModelState.IsValid)
            {
                result = _userService.EditUser(regDTO);
                if (result != null)
                {
                    return Ok(result);
                }
                else
                    return BadRequest("");
            }

            return NoContent();
        }


        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [AllowAnonymous]
        [HttpGet("getimage/{email}")]
        public IActionResult GetImage(string email)
        {
            try
            {
                var imageFileStream = System.IO.File.OpenRead(_userService.GetImagePath(email));
                return File(imageFileStream, "image/jpeg");
            }
            catch (Exception)
            {
                return NotFound();
            }

        }

    }

    public class AuthViewModel
    {
        public string Email { get; set; }


        public string Password { get; set; }
    }
}
