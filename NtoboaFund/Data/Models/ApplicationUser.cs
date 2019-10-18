using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace NtoboaFund.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            LuckyMes = new HashSet<LuckyMe>();
            Scholarships = new HashSet<Scholarship>();
            Businesses = new HashSet<Business>();
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }


        public string Token { get; set; }

        /// <summary>
        /// integers denoting type of user.. 1 standing for registered user 
        /// 2 standing for system generated user.9
        /// </summary>
        public int UserType { get; set; }

        public virtual BankDetails BankDetails { get; set; }

        public virtual MobileMoneyDetails MomoDetails { get; set; }

        public decimal Points { get; set; }

        /// <summary>
        /// Either Mobile Money or Bank Account
        /// </summary>
        public string PreferedMoneyReceptionMethod { get; set; }

        public virtual ICollection<LuckyMe> LuckyMes { get; set; }

        public virtual ICollection<Scholarship> Scholarships { get; set; }

        public virtual ICollection<Business> Businesses { get; set; }
    }
}
