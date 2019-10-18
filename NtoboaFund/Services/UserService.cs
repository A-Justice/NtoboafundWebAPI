using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.DTO_s;
using NtoboaFund.Data.Models;
using NtoboaFund.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NtoboaFund.Services
{
    public interface IUserService
    {
        ApplicationUser Authenticate(string username, string password);

        Task<Tuple<ApplicationUser, string>> Register(RegistrationDTO user);

        ApplicationUser EditUser(UserEditDTO user);

        IEnumerable<ApplicationUser> GetAll();

        Task<string> GetUserRole(string userId);

        string GetImagePath(string userEmail);

        ApplicationUser GetUser(string Id);
        ApplicationUser GetFullUser(string Id);

        Task<bool> UserExists(string userId);

        UserStatistics GetUserStatistics(ApplicationUser user);

        ApplicationUser GetUserWithEmail(string email);

        Task SendPasswordResetMessage(ApplicationUser user);

        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);

        Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
    }

    public class UserService : IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<ApplicationUser> _users = new List<ApplicationUser>
        {
            //  new ApplicationUser { Id = 1, FirstName = "Test", LastName = "ApplicationUser", Username = "test", Password = "test" }
        };

        private readonly AppSettings _appSettings;
        private NtoboaFundDbContext dbContext;
        public static IHostingEnvironment _environment;

        public UserManager<ApplicationUser> UserManager { get; }
        public RoleManager<IdentityRole> RoleManager { get; }
        public SignInManager<ApplicationUser> SignInManager { get; }
        public MessagingService MessagingService { get; }

        public UserService(IOptions<AppSettings> appSettings, NtoboaFundDbContext _context,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager
            , SignInManager<ApplicationUser> signInManager, MessagingService messagingService, IHostingEnvironment environment)
        {
            RoleManager = roleManager;
            SignInManager = signInManager;
            MessagingService = messagingService;
            UserManager = userManager;
            _appSettings = appSettings.Value;
            dbContext = _context;
            _environment = environment;
        }

        public ApplicationUser Authenticate(string username, string password)
        {
            var signInResult = SignInManager.PasswordSignInAsync(username, password, false, false).Result;

            // return null if ApplicationUser not found
            if (!signInResult.Succeeded)
                return null;

            var user = dbContext.Users.Include("BankDetails").Include("MomoDetails").FirstOrDefault(i => i.UserName.ToLower() == username.ToLower());

            user = GenerateTokenForUser(user);


            return user;
        }

        public async Task<Tuple<ApplicationUser, string>> Register(RegistrationDTO regUser)
        {
            ApplicationUser user = null;

            var User = dbContext.Users.FirstOrDefault(x => x.Email == regUser.Email);

            if (User != null)
            {
                return Tuple.Create<ApplicationUser, string>(null, "User With Same Email Exists");
            }

            if (regUser.Password != regUser.ConfirmPassword)
            {
                return Tuple.Create<ApplicationUser, string>(null, "Passwords Do Not Match");
            }

            var momoDetails = new MobileMoneyDetails();
            dbContext.MobileMoneyDetails.Add(momoDetails);
            var bankDetails = new BankDetails();
            dbContext.BankDetails.Add(bankDetails);

            user = new ApplicationUser
            {
                UserName = regUser.Email,
                FirstName = regUser.FirstName,
                LastName = regUser.LastName,
                PhoneNumber = regUser.PhoneNumber,
                Email = regUser.Email,
                MomoDetails = momoDetails,
                BankDetails = bankDetails
            };

            if (regUser.Images?.Length > 0)
            {
                try
                {
                    if (!Directory.Exists(_environment.WebRootPath + "\\uploads\\"))
                    {
                        Directory.CreateDirectory(_environment.WebRootPath + "\\uploads\\");
                    }
                    using (FileStream filestream = File.Create(_environment.WebRootPath + "\\uploads\\" + regUser.Email.Split('@')[0] + ".jpeg"))
                    {
                        regUser.Images.CopyTo(filestream);
                        filestream.Flush();
                        //return "\\uploads\\" + regUser.Images.FileName;
                    }
                }
                catch (Exception ex)
                {
                    // return ex.ToString();
                }
            }


            IdentityResult result = UserManager.CreateAsync
            (user, regUser.Password).Result;
            var userRole = regUser.Role ?? "User";

            if (result.Succeeded)
            {
                if (RoleManager.RoleExistsAsync(userRole).Result)
                    UserManager.AddToRoleAsync(user, userRole).Wait();
            }
            else
            {
                var errorString = string.Join(@"\n", result.Errors.Select(i => $"{i.Code}\n{i.Description}"));
                return Tuple.Create<ApplicationUser, string>(null, errorString);
            }


            user = GenerateTokenForUser(user);

            //send Hubtel Message

            //send Email
            try
            {
                //sendHubtelMessage(cookNumber(regUser.PhoneNumber));
                string path = _environment.WebRootPath + "\\files\\html.txt";
                string html = File.ReadAllText(path);
                await MessagingService.SendMail($"{user.FirstName} {user.LastName}", regUser.Email, "Registration Successfull", html);
                await MessagingService.SendSms(user.PhoneNumber, Misc.GetRegisteredUserSmsMessage());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            dbContext.SaveChanges();

            return Tuple.Create<ApplicationUser, string>(user, null);
        }

        public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
        {
            return await UserManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {
            return await UserManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task SendPasswordResetMessage(ApplicationUser user)
        {
            string path = _environment.WebRootPath + "\\files\\resetpassword.txt";
            string html = File.ReadAllText(path);

            var uniqueCode = await GetUniqueCodeForMailReset(user);

            html = html.Replace("resetpasswordform", $"resetpasswordform?token={uniqueCode}");

            await MessagingService.SendMail($"{user.FirstName} {user.LastName}", user.Email, "Password Reset", html);

        }

        async Task<string> GetUniqueCodeForMailReset(ApplicationUser user)
        {

            //var f = user.FirstName.Substring(0, 2);
            //var l = user.LastName.Substring(0, 2);
            //var em = user.Email.Substring(0, 4);
            //var d = DateTime.Now.AddHours(1).ToString();

            //var resetToken = $"{f}{l}{em}.{user.Id}.{d}";

            //setUserResetToken(user, resetToken);


            return $"{user.Id}.{await UserManager.GeneratePasswordResetTokenAsync(user)}";
        }

        public async Task<string> GetUserRole(string userId)
        {
            string userRole = null;
            try
            {
                var user = dbContext.Users.Find(userId);

                userRole = UserManager.GetRolesAsync(user).Result[0];
            }
            catch (Exception ex)
            {
                userRole = null;
            }
            return userRole;
        }

        public ApplicationUser EditUser(UserEditDTO regUser)
        {
            var user = dbContext.Users.Find(regUser.Id);
            user.FirstName = regUser.FirstName == "null" ? null : regUser.FirstName;
            user.LastName = regUser.LastName == "null" ? null : regUser.LastName;
            user.Email = regUser.Email == "null" ? null : regUser.Email;
            user.PhoneNumber = regUser.PhoneNumber == "null" ? null : regUser.PhoneNumber;

            if (user.MomoDetails == null)
                user.MomoDetails = new MobileMoneyDetails();
            user.MomoDetails.Country = regUser.Country == "null" ? null : regUser.Country;
            user.MomoDetails.Number = regUser.MobileMoneyNumber == "null" ? null : regUser.MobileMoneyNumber;
            user.MomoDetails.Network = regUser.Network == "null" ? null : regUser.Network;
            user.MomoDetails.Currency = regUser.Currency == "null" ? null : regUser.Currency;

            if (user.BankDetails == null)
                user.BankDetails = new BankDetails();
            user.BankDetails.BankName = regUser.BankName == "null" ? null : regUser.BankName;
            user.BankDetails.AccountNumber = regUser.AccountNumber == "null" ? null : regUser.AccountNumber;
            user.BankDetails.SwiftCode = regUser.SwiftCode == "null" ? null : regUser.SwiftCode;

            user.PreferedMoneyReceptionMethod = regUser.PreferredReceptionMethod == "null" ? null : regUser.PreferredReceptionMethod;

            try
            {
                dbContext.Entry(user).State = EntityState.Modified;
                dbContext.Entry(user.BankDetails).State = EntityState.Modified;
                dbContext.Entry(user.MomoDetails).State = EntityState.Modified;
                dbContext.SaveChanges();
                return user;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the current statistics of the user with the specified userId
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns></returns>
        public UserStatistics GetUserStatistics(ApplicationUser user)
        {
            var s = new UserStatistics();

            s.TotalPoints = user.Points;
            s.TotalLuckymeStakes = dbContext.LuckyMes.Where(i => i.UserId == user.Id && i.Status != "pending").Count();
            s.TotalScholarshipStakes = dbContext.Scholarships.Where(i => i.UserId == user.Id && i.Status != "pending").Count();
            s.TotalBusinessStakes = dbContext.Businesses.Where(i => i.UserId == user.Id && i.Status != "pending").Count();
            s.TotalStakes = s.TotalLuckymeStakes + s.TotalScholarshipStakes + s.TotalBusinessStakes;

            return s;
        }

        public IEnumerable<ApplicationUser> GetAll()
        {
            // return users without passwords
            return dbContext.Users.ToList();
        }

        public ApplicationUser GetUser(string Id)
        {
            return dbContext.Users.Where(i => i.Id == Id).Include("BankDetails").Include("MomoDetails").FirstOrDefault();
        }
        public ApplicationUser GetFullUser(string Id)
        {
            return dbContext.Users.Where(i => i.Id == Id).Include("LuckyMes").Include("Scholarships").Include("Businesses").Include("BankDetails").Include("MomoDetails").FirstOrDefault();
        }

        public Task<bool> UserExists(string userId)
        {
            return dbContext.Users.AnyAsync(i => i.Id == userId);
        }

        public ApplicationUser GetUserWithEmail(string email)
        {
            return dbContext.Users.Where(i => i.Email == email).Include("BankDetails").Include("MomoDetails").FirstOrDefault();

        }

        public string GetImagePath(string imageName)
        {
            var avatarPath = Path.Combine(_environment.WebRootPath + "\\uploads\\", $"{imageName}.jpeg");

            return avatarPath;
        }

        public ApplicationUser GenerateTokenForUser(ApplicationUser User)
        {
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Settings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, User.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            User.Token = tokenHandler.WriteToken(token);
            return User;
        }

        public void sendHubtelMessage(string phoneNumber)
        {
            //string clientId = _appSettings.RaveApiSettings.ApiKey;
            //string clientSecret = _appSettings.RaveApiSettings.ApiSecret;

            //try
            //{
            //    var host = new ApiHost(new BasicAuth(clientId, clientSecret));
            //    var messageApi = new MessagingApi(host);
            //    MessageResponse msg = messageApi.SendQuickMessage("Ntoba-fund", phoneNumber, "Thank you for registering with ntoboa fund", true);

            //    //Console.WriteLine(msg.Status);
            //}
            //catch (Exception e)
            //{
            //    if (e.GetType() == typeof(HttpRequestException))
            //    {
            //        var ex = e as HttpRequestException;
            //        if (ex != null && ex.HttpResponse != null)
            //        {
            //            Console.WriteLine("Error Status Code " + ex.HttpResponse.Status);
            //        }
            //    }
            //}
            //Console.ReadKey();
        }


        public void SendMail(string To, string MailSubject, string MailBody)
        {
            string From = "info@ntoboafund.com";
            string FromPassword = "u8v@Dlh!Ew%;";
            SmtpClient client = new SmtpClient("relay-hosting.secureserver.net", 25);

            //client.Port = 587;
            // client.DeliveryMethod = SmtpDeliveryMethod.Network;
            // client.UseDefaultCredentials = false;
            System.Net.NetworkCredential credentials =
                new System.Net.NetworkCredential(From, FromPassword);
            client.EnableSsl = false;
            client.Credentials = credentials;

            try
            {
                var mail = new MailMessage(From.Trim(), To.Trim());
                mail.IsBodyHtml = true;
                mail.Subject = MailSubject;
                mail.Body = MailBody;
                client.Send(mail);
                mail.Dispose();

            }
            catch (Exception ex)
            {

            }
        }

        public string cookNumber(string phoneNumber)
        {
            if (phoneNumber.Length == 10 && phoneNumber.StartsWith('0'))
            {
                phoneNumber = phoneNumber.TrimStart('0');
                phoneNumber = "+233" + phoneNumber;
            }
            return phoneNumber;
        }

    }
}
