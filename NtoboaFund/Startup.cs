using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NtoboaFund.Data.DBContext;
using NtoboaFund.Data.Models;
using NtoboaFund.Helpers;
using NtoboaFund.Services;
using NtoboaFund.Services.HostedServices;
using NtoboaFund.SignalR;
using System.Text;

namespace NtoboaFund
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("NtuboaDefault", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

            //GearHost
            //DefaultConnection
            //Azure
            services.AddDbContext<NtoboaFundDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Azure")));

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("Personal");

            services.Configure<AppSettings>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();

            services.AddMvc(/*mvcOptions => mvcOptions.Filters.Add(new CorsHeaderFilter())*/)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<NtoboaFundDbContext>()
            .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }
            ).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    // ValidIssuer = Configuration["Issuer"],
                    ValidateAudience = false,
                    // ValidAudience = Configuration["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Settings.Secret))
                };
            });


            // configure DI for application services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IScopedProcessingService, ScopedProcessingService>();
            services.AddScoped<PaymentService>();
            services.AddHostedService<WinnerSelectorHostedService>();
            //services.AddTransient(typeof(StakersHub));
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            app.UseCors("NtuboaDefault");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseSignalR(options =>
            {
                options.MapHub<StakersHub>("/stakers");
                options.MapHub<CountdownHub>("/countdown");
                options.MapHub<WinnerSelectionHub>("/winnerselection");
            });
            app.UseMvc();

            CreateUserRoles(userManager, roleManager);
        }

        private static void CreateUserRoles(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {

            //Adding Admin Role
            if (!roleManager.RoleExistsAsync("Admin").Result)
            {
                IdentityResult roleResult = roleManager.CreateAsync(new IdentityRole("Admin")).Result;

            }

            else if (!roleManager.RoleExistsAsync("User").Result)
            {

                IdentityResult roleResult = roleManager.CreateAsync(new IdentityRole("User")).Result;
            }


            if (userManager.FindByNameAsync("admin@ntoboafund.com").Result == null)
            {
                ApplicationUser user = new ApplicationUser();
                user.UserName = "admin@ntoboafund.com";
                user.Email = "admin@ntoboafund.com";

                IdentityResult result = userManager.CreateAsync
                (user, ".ntobaofundadmin.").Result;

                if (result.Succeeded)
                {
                    //if (roleManager.RoleExistsAsync("Admin").Result)
                    userManager.AddToRoleAsync(user, "Admin").Wait();
                }

            }
            //Assign Admin role to the main User here we have given our newly registered 
            //login id for Admin management
            //ApplicationUser user = await UserManager.FindByEmailAsync("syedshanumcain@gmail.com");
            //var User = new ApplicationUser();
            //await UserManager.AddToRoleAsync(user, "Admin");
        }
    }
}
