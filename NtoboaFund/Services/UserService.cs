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

namespace NtoboaFund.Services
{
    public interface IUserService
    {
        ApplicationUser Authenticate(string username, string password);
        Tuple<ApplicationUser, string> Register(RegistrationDTO user);

        ApplicationUser EditUser(UserEditDTO user);

        IEnumerable<ApplicationUser> GetAll();



        string GetImagePath(string userEmail);

        ApplicationUser GetUser(string Id);
    }

    public class UserService : IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<ApplicationUser> _users = new List<ApplicationUser>
        {
            //  new ApplicationUser { Id = 1, FirstName = "Test", LastName = "ApplicationUser", Username = "test", Password = "test" }
        };

        private readonly AppSettings _appSettings;
        private NtoboaFundDbContext context;
        public static IHostingEnvironment _environment;

        public UserManager<ApplicationUser> UserManager { get; }
        public RoleManager<IdentityRole> RoleManager { get; }
        public SignInManager<ApplicationUser> SignInManager { get; }
        public MessagingService MessagingService { get; }

        public UserService(IOptions<AppSettings> appSettings, NtoboaFundDbContext _context,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager
            , SignInManager<ApplicationUser> signInManager,MessagingService messagingService,IHostingEnvironment environment)
        {
            RoleManager = roleManager;
            SignInManager = signInManager;
            MessagingService = messagingService;
            UserManager = userManager;
            _appSettings = appSettings.Value;
            context = _context;
            _environment = environment;
        }

        public ApplicationUser Authenticate(string username, string password)
        {
            var signInResult = SignInManager.PasswordSignInAsync(username, password, false, false).Result;


            // return null if ApplicationUser not found
            if (!signInResult.Succeeded)
                return null;

            var user = context.Users.Include("BankDetails").Include("MomoDetails").FirstOrDefault(i => i.UserName.ToLower() == username.ToLower());

            user = GenerateTokenForUser(user);


            return user;
        }

        public Tuple<ApplicationUser, string> Register(RegistrationDTO regUser)
        {
            ApplicationUser user = null;

            var User = context.Users.FirstOrDefault(x => x.Email == regUser.Email);

            if (User != null)
            {
                return Tuple.Create<ApplicationUser, string>(null, "User With Same Email Exists");
            }

            if (regUser.Password != regUser.ConfirmPassword)
            {
                return Tuple.Create<ApplicationUser, string>(null, "Passwords Do Not Match");
            }
            var momoDetails = new MobileMoneyDetails();
            context.MobileMoneyDetails.Add(momoDetails);
            var bankDetails = new BankDetails();
            context.BankDetails.Add(bankDetails);

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
                var errorString =string.Join(@"\n", result.Errors.Select(i=> $"{i.Code}\n{i.Description}" ));
                return Tuple.Create<ApplicationUser, string>(null, errorString);
            }


            user = GenerateTokenForUser(user);

            //send Hubtel Message
            
            //send Email
            try
            {
                sendHubtelMessage(cookNumber(regUser.PhoneNumber));
                string path = _environment.WebRootPath + "\\files\\html.txt";
                string html = File.ReadAllText(path);
                MessagingService.SendMail($"{user.FirstName} {user.LastName}",regUser.Email, "Registration Successfull", html);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            context.SaveChanges();

            return Tuple.Create<ApplicationUser, string>(user, null);
        }

        public ApplicationUser EditUser(UserEditDTO regUser)
        {
            var user = context.Users.Find(regUser.Id);
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
                context.Entry(user).State = EntityState.Modified;
                context.Entry(user.BankDetails).State = EntityState.Modified;
                context.Entry(user.MomoDetails).State = EntityState.Modified;
                context.SaveChanges();
                return user;
            }
            catch
            {
                return null;
            }
        }
        public IEnumerable<ApplicationUser> GetAll()
        {
            // return users without passwords
            return context.Users.ToList();
        }

        public ApplicationUser GetUser(string Id)
        {
            return context.Users.Where(i=>i.Id == Id).Include("BankDetails").Include("MomoDetails").FirstOrDefault();
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
