using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NtoboaFund.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NtoboaFund.Data.DBContext
{
    public class NtoboaFundDbContext:IdentityDbContext<ApplicationUser>
    {
        public DbSet<ApplicationUser> Users { get; set; }


        public DbSet<LuckyMe> LuckyMes { get; set; }

        public DbSet<Scholarship> Scholarships { get; set; }

        public DbSet<Business> Businesses { get;set;}

        public NtoboaFundDbContext(DbContextOptions<NtoboaFundDbContext> options)
            :base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
