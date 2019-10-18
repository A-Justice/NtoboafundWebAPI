//using hubtelapi_dotnet_v1.Hubtel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NtoboaFund.Data.DTO_s;
using NtoboaFund.Data.Models;
using NtoboaFund.Services;
using System;
using System.Collections.Generic;
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

        [HttpGet("getrole/{userId}")]
        public async Task<IActionResult> GetUserRole(string userId)
        {
            var role = await _userService.GetUserRole(userId);
            if (role != null)
                return Ok(new { role });

            return BadRequest("Cannot find Role for User");
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
        [HttpGet("resetpassword/initiate/{email}")]
        public IActionResult InitiateResetPassword(string email)
        {
            var user = _userService.GetUserWithEmail(email);
            if (user == null)
            {
                return BadRequest(new { message = "Sorry, We dont know this email address." });
            }

            _userService.SendPasswordResetMessage(user);

            return Ok(new { message = "Password Reset Email Sent" });

        }

        [AllowAnonymous]
        [HttpPost("resetpassword/change")]
        public async Task<IActionResult> ResetPassword([FromBody]PasswordResetModel prModel)
        {
            int lid = prModel.resetToken.LastIndexOf(".");
            var userId = prModel.resetToken.Substring(0, lid);
            var token = prModel.resetToken.Substring(lid + 1, prModel.resetToken.Length - (lid + 1));
            token = token.Replace(" ", "+");
            var user = _userService.GetUser(userId);

            var result = await _userService.ResetPasswordAsync(user, token, prModel.newPassword);

            if (result.Succeeded)
                return Ok(new { email = user.Email, password = prModel.newPassword, message = "Password Changed Successfully" });

            return BadRequest(new { message = "Sorry an error occured while resetting password" });
        }

        [HttpPut("changepassword")]
        public async Task<IActionResult> ChangePassword([FromForm]ChangePasswordModel cpModel)
        {
            var user = _userService.GetUser(cpModel.userId);

            var result = await _userService.ChangePasswordAsync(user, cpModel.currentPassword, cpModel.newPassword);

            if (result.Succeeded)
                return Ok(new { message = "Password Changed Successfully" });


            return BadRequest(new { message = (result.Errors as List<IdentityError>)[0] });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm]RegistrationDTO regDTO)
        {
            Tuple<ApplicationUser, string> result = null;
            if (ModelState.IsValid)
            {
                result = await _userService.Register(regDTO);

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

        /// <summary>
        /// Chech wether user with the specified Id exists
        /// </summary>
        /// <param name="userId">The User Id</param>
        /// <returns></returns>
        [HttpGet("Exists/{userId}")]
        public async Task<bool> UserExists(string userId)
        {
            return await _userService.UserExists(userId);
        }


        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }


        [HttpGet("statistics/{userId}")]
        public IActionResult GetStakeStatistics(string userId)
        {
            var user = _userService.GetUser(userId);

            if (user == null)
                return BadRequest(new { Message = "User not found" });

            var userStatistics = _userService.GetUserStatistics(user);

            return Ok(userStatistics);
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

    public class PasswordResetModel
    {
        public string newPassword { get; set; }

        public string resetToken { get; set; }

    }

    public class ChangePasswordModel
    {
        public string userId { get; set; }
        public string currentPassword { get; set; }

        public string newPassword { get; set; }

    }

}
