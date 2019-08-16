using hubtelapi_dotnet_v1.Hubtel;
using Microsoft.AspNetCore.Hosting;
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
using System.Security.Claims;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NtoboaFund.Services
{
    public interface IUserService
    {
        ApplicationUser Authenticate(string username, string password);
        Tuple<ApplicationUser,string> Register(RegistrationDTO user);

        ApplicationUser EditUser(UserEditDTO user);

        IEnumerable<ApplicationUser> GetAll();

        

        string GetImagePath(string userEmail);
    }

    public class UserService:IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<ApplicationUser> _users = new List<ApplicationUser>
        {
          //  new ApplicationUser { Id = 1, FirstName = "Test", LastName = "ApplicationUser", Username = "test", Password = "test" }
        };

        private readonly AppSettings _appSettings;
        private NtoboaFundDbContext context;
        public static IHostingEnvironment _environment;

        public UserService(IOptions<AppSettings> appSettings,NtoboaFundDbContext _context, IHostingEnvironment environment)
        {
            _appSettings = appSettings.Value;
            context = _context;
            _environment = environment;
        }

        public ApplicationUser Authenticate(string username, string password)
        {
            var User = context.Users.SingleOrDefault(x => x.Email == username && x.Password == password);

            // return null if ApplicationUser not found
            if (User == null)
                return null;

           User = GenerateTokenForUser(User);

            // remove password before returning
            User.Password = null;

            return User;
        }

       public Tuple<ApplicationUser,string> Register(RegistrationDTO regUser)
        {
            ApplicationUser user  = null;
            
                var User = context.Users.FirstOrDefault(x => x.Email == regUser.Email);

                if (User != null)
                {
                    return Tuple.Create<ApplicationUser,string>(null,"User With Same Email Exists");
                }

                if (regUser.Password != regUser.ConfirmPassword)
                {
                    return Tuple.Create<ApplicationUser, string>(null, "Passwords Do Not Match"); 
                }
                user = new ApplicationUser
                {
                    FirstName = regUser.FirstName,
                    LastName = regUser.LastName,
                    PhoneNumber = regUser.PhoneNumber,
                    Email = regUser.Email,
                    Password = regUser.Password

                };

                if (regUser.Images?.Length > 0)
                {
                    try
                    {
                        if (!Directory.Exists(_environment.WebRootPath + "\\uploads\\"))
                        {
                            Directory.CreateDirectory(_environment.WebRootPath + "\\uploads\\");
                        }
                        using (FileStream filestream = File.Create(_environment.WebRootPath + "\\uploads\\" +regUser.Email.Split('@')[0]+".jpeg"))
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
                context.Users.Add(user);
                context.SaveChanges();
                user = GenerateTokenForUser(user);
                user.Password = null;

            sendHubtelMessage(cookNumber(regUser.PhoneNumber));
            
            try
            {
                string path = _environment.WebRootPath + "\\files\\html.txt";
                string html = File.ReadAllText(path);
                Operations.SendMail(regUser.Email, "Registration Successfull", html);
            }
            catch
            {
            }
          
            return Tuple.Create<ApplicationUser, string>(user, null);
        }

        public ApplicationUser EditUser(UserEditDTO regUser)
        {
            var user = context.Users.Find(regUser.Id);
            user.FirstName = regUser.FirstName;
            user.LastName = regUser.LastName;
            user.Email = regUser.Email;
            user.PhoneNumber = regUser.PhoneNumber;

            try
            {
                context.Entry(user).State = EntityState.Modified;
                context.SaveChangesAsync();
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
            return _users.Select(x => {
                x.Password = null;
                return x;
            });
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
            string clientId = _appSettings.HubtelApiSettings.ApiKey;
             string clientSecret = _appSettings.HubtelApiSettings.ApiSecret;

            try
            {
                var host = new ApiHost(new BasicAuth(clientId, clientSecret));
                var messageApi = new MessagingApi(host);
                MessageResponse msg = messageApi.SendQuickMessage("Ntoba-fund", phoneNumber, "Thank you for registering with ntoboa fund", true);

                //Console.WriteLine(msg.Status);
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(HttpRequestException))
                {
                    var ex = e as HttpRequestException;
                    if (ex != null && ex.HttpResponse != null)
                    {
                        Console.WriteLine("Error Status Code " + ex.HttpResponse.Status);
                    }
                }
            }
            //Console.ReadKey();
        }


        public void SendMail(string To, string MailSubject, string MailBody)
        {
            string From = "info@ntoboafund.com";
            string FromPassword = "u8v@Dlh!Ew%;";
            SmtpClient client = new SmtpClient("relay-hosting.secureserver.net",25);

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
            if(phoneNumber.Length == 10 && phoneNumber.StartsWith('0'))
            {
                phoneNumber = phoneNumber.TrimStart('0');
                phoneNumber = "+233" + phoneNumber;
            }
            return phoneNumber;
        }
      
    }
}
