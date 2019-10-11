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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NtoboaFund
{
    //When Switch between production and test
    //Change Connection String
    //change currentsettings in RaveApiSettingsDTO
    //change Draw Time

    /// <summary>
    /// 
    /// </summary>
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
            //DefaultConnectiona
            //Azure
            services.AddDbContext<NtoboaFundDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), i => i.EnableRetryOnFailure()));

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
            services.AddScoped<MessagingService>();
            services.AddScoped<StakersHub>();
            services.AddScoped<DummyService>();
            services.AddScoped<AnalysisService>();
            services.AddHostedService<WinnerSelectorHostedService>();

            //services.AddTransient(typeof(StakersHub));
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.KeepAliveInterval = TimeSpan.FromHours(1);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, NtoboaFundDbContext dbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
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
            //app.UseHttpsRedirection();
            app.UseSignalR(options =>
            {
                options.MapHub<StakersHub>("/stakers", hubOptions =>
                {
                    hubOptions.WebSockets.CloseTimeout = TimeSpan.FromHours(1);
                    hubOptions.LongPolling.PollTimeout = TimeSpan.FromHours(1);
                });
                options.MapHub<CountdownHub>("/countdown", hubOptions =>
                {
                    hubOptions.WebSockets.CloseTimeout = TimeSpan.FromHours(1);
                    hubOptions.LongPolling.PollTimeout = TimeSpan.FromHours(1);
                });
                options.MapHub<WinnerSelectionHub>("/winnerselection", hubOptions =>
                {
                    hubOptions.WebSockets.CloseTimeout = TimeSpan.FromHours(1);
                    hubOptions.LongPolling.PollTimeout = TimeSpan.FromHours(1);
                });
            });

            app.UseMvc();

            CreateDefaultUserBuilder(dbContext);
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
                user.FirstName = "Administrator";
                user.LastName = "";
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

        private static void CreateDefaultUserBuilder(NtoboaFundDbContext dbContext)
        {
            if (dbContext.UserBuilders.All(i => i.FirstName != "Abraham"))
            {
                var userBuilders = new List<UserBuilder>
                {
                    new UserBuilder
                    {
                        FirstName = "Abraham",
                         LastName = "Adofo"
                    },
                    new UserBuilder
                    {
                        FirstName = "Gyifa",
                        LastName  = "Mensah"
                    },
                    new UserBuilder
                    {
                        FirstName = "Daniel",
                        LastName  = "Adjei"
                    },
                    new UserBuilder
                    {
                        FirstName = "Samuel",
                        LastName  = "Owusu"
                    },
                    new UserBuilder
                    {
                        FirstName = "Emmanuel",
                        LastName  = "Doe"
                    },
                    new UserBuilder
                    {
                        FirstName = "Yvonne",
                        LastName  = "Owusu"
                    },
                    new UserBuilder
                    {
                        FirstName = "Michael",
                        LastName  = "Ato"
                    },
                    new UserBuilder
                    {
                        FirstName = "David",
                        LastName  = "Gbadago"
                    },
                    new UserBuilder
                    {
                        FirstName = "Samuel",
                        LastName  = "Gadagidi"
                    },
                    new UserBuilder
                    {
                        FirstName = "Ralph",
                        LastName  = "Davids"
                    },
                    new UserBuilder
                    {
                        FirstName = "Joshua",
                        LastName  = "Ayi"
                    },
                    new UserBuilder
                    {
                        FirstName = "Emmanuel",
                        LastName  = "Osa"
                    },
                    new UserBuilder
                    {
                        FirstName = "Emelia",
                        LastName  = "Opare"
                    },
                    new UserBuilder
                    {
                        FirstName = "Sena",
                        LastName  = "Agbo"
                    },
                    new UserBuilder
                    {
                        FirstName = "Mercy",
                        LastName  = "Chinwe"
                    },
                    new UserBuilder
                    {
                        FirstName = "Annette",
                        LastName  = "Larry"
                    },
                    new UserBuilder
                    {
                        FirstName = "Chris",
                        LastName  = "Stapleton"
                    },
                    new UserBuilder
                    {
                        FirstName = "Emelia",
                        LastName  = "Clark"
                    },
                    new UserBuilder
                    {
                        FirstName = "Regina",
                        LastName  = "Hayford"
                    },
                    new UserBuilder
                    {
                        FirstName = "Kwame",
                        LastName  = "Ansah"
                    },
                    new UserBuilder
                    {
                        FirstName = "Emmanuel",
                        LastName  = "Asante"
                    },
                    new UserBuilder
                    {
                        FirstName = "Kwesi",
                        LastName  = "Ampah"
                    },
                    new UserBuilder
                    {
                        FirstName = "Lisa",
                        LastName  = "Ofori"
                    },
                    new UserBuilder
                    {
                        FirstName = "Raphael",
                        LastName  = "Musa"
                    },
                    new UserBuilder
                    {
                        FirstName = "Alhasan",
                        LastName  = "Kura"
                    },
                    new UserBuilder
                    {
                        FirstName = "Fuseini",
                        LastName  = "Mamba"
                    }

                };
                dbContext.UserBuilders.AddRange(userBuilders);
                dbContext.SaveChanges();
            }
        }
    }

}
