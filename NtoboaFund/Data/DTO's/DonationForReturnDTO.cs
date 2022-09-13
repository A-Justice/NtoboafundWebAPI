using System;
using NtoboaFund.Data.Models;

namespace NtoboaFund.Data.DTOs
{
    public class DonationForReturnDTO
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        //public virtual ApplicationUser User { get; set; }

        public string Username { get; set; }

        public decimal Amount { get; set; }

        public string TxRef { get; set; }

        public int CrowdFundId { get; set; }

        public bool paid { get; set; } = false;

        public string Date { get; set; }

        public virtual CrowdFund CrowdFund { get; set; }
    }
}
