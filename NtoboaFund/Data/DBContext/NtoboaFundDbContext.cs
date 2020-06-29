using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NtoboaFund.Data.Models;

namespace NtoboaFund.Data.DBContext
{
    public class NtoboaFundDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ApplicationUser> Users { get; set; }


        public DbSet<LuckyMe> LuckyMes { get; set; }

        public DbSet<Scholarship> Scholarships { get; set; }

        public DbSet<Business> Businesses { get; set; }

        public DbSet<BankDetails> BankDetails { get; set; }


        public DbSet<MobileMoneyDetails> MobileMoneyDetails { get; set; }

        public DbSet<Status> Statuses { get; set; }

        public DbSet<Transfer> Transfers { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<UserBuilder> UserBuilders { get; set; }

        public DbSet<ContactUs> ContactUs { get; set; }

        public NtoboaFundDbContext(DbContextOptions<NtoboaFundDbContext> options)
            : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

    }
}
