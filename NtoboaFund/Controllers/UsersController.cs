//using hubtelapi_dotnet_v1.Hubtel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.DTO_s;
using NtoboaFund.Data.Models;
using NtoboaFund.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace NtoboaFund.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private NtoboaFundDbContext dbContext;
        public UserManager<ApplicationUser> UserManager { get; }
        public UsersController(IUserService userService,
            NtoboaFundDbContext _context,
            UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            dbContext = _context;
            UserManager = userManager;
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
            try
            {
                var user = _userService.Authenticate(credentials.PhoneNumber, credentials.Password);

                if (user == null)
                    return BadRequest("Username or password is incorrect");

                return Ok(user);
            }
            catch
            {
                return BadRequest("Username or password is incorrect");
            }
           
        }

        [AllowAnonymous]
        [HttpGet("resetpassword/initiate/{email}")]
        public IActionResult InitiateResetPassword(string email)
        {
            var user = _userService.GetUserWithEmail(email);
            if (user == null)
            {
                return BadRequest(new { message = "Sorry, this email address does not exist in our database." });
            }

            var code = _userService.SendPasswordResetMessage(user);

            return Ok(new { message = "Password Reset Email Sent",code });

        }

        [AllowAnonymous]
        [HttpPost("resetpassword/change")]
        public async Task<IActionResult> ResetPassword([FromBody]PasswordResetModel prModel)
        {
            int lid = prModel.resetToken.LastIndexOf(".");
            var userId = prModel.resetToken.Substring(0, lid);
            var user = await UserManager.FindByIdAsync(userId);
            string token = prModel.resetToken.Substring(lid + 1, prModel.resetToken.Length - (lid + 1));
            token = HttpUtility.UrlDecode(token);

            try
            {

                token = token.Replace(" ", "+");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

            try
            {

                var result = await UserManager.ResetPasswordAsync(user, token, prModel.newPassword);

                //var result = await _userService.ResetPasswordAsync(user, token, prModel.newPassword);

                if (result.Succeeded)
                    return Ok(new { email = user.Email, password = prModel.newPassword, message = "Password Changed Successfully" });

                string errString = "";
                foreach (var item in result.Errors)
                {
                    errString += (" " + item.Description);
                }

                return BadRequest(new { message = "Sorry an error occured while resetting password", errString, token });

            }
            catch(Exception ex)
            {
                return BadRequest(new { ex, prModel,user, token });
            }
           
        }

        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword([FromForm]ChangePasswordModel cpModel)
        {
            try
            {
                //var user = _userService.GetUser(cpModel.userId);
                //var user = dbContext.Users.Where(i => i.Id == cpModel.userId).ToList().FirstOrDefault();
                //this.user
                var identity = new ClaimsIdentity(new Claim[]
                {
                    new Claim(new IdentityOptions().ClaimsIdentity.UserIdClaimType, cpModel.userId)
                });
                var principal = new ClaimsPrincipal(identity);
                var  user = await UserManager.GetUserAsync(principal);
                var result =  UserManager.ChangePasswordAsync(user, cpModel.currentPassword, cpModel.newPassword).Result;

                //var result = await _userService.ChangePasswordAsync(user, cpModel.currentPassword, cpModel.newPassword);
                //var result = await _userService.ChangePasswordAsync(user, cpModel.currentPassword, cpModel.newPassword);


                if (result.Succeeded)
                    return Ok(new { message = "Password Changed Successfully" });


                return BadRequest(new { message = (result.Errors as List<IdentityError>)[0] });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Internal Server Error" });
            }
           
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

        [AllowAnonymous]
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
        public string PhoneNumber { get; set; }


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
