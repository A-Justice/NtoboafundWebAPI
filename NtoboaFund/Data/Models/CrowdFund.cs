using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using NtoboaFund.Data.Interfaces;

namespace NtoboaFund.Data.Models
{
    public class CrowdFund
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string PhoneNumber { get; set; }

        public string DateCreated { get; set; }

        public string EndDate { get; set; }

        [NotMapped]
        public IFormFile MainImage { get; set; }

        public string MainImageUrl { get; set; } = "";

        [NotMapped]
        public IFormFile SecondImage { get; set; }

        public string SecondImageUrl { get; set; } = "";

        [NotMapped]
        public IFormFile ThirdImage { get; set; }

        public string ThirdImageUrl { get; set; } = "";

        [NotMapped]
        public IFormFile Video { get; set; }

        public string videoUrl { get; set; } = "";

        public decimal TotalAmount { get; set; }

        public decimal TotalAmountRecieved { get; set; }

        public int TypeId { get; set; }

        [ForeignKey("TypeId")]
        public virtual CrowdFundType CrowdFundType { get; set; }

        public virtual ICollection<Donation> Donations { get; set; }

        public string UserId
        {
            get; set;
        }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }


    public class CrowdFundType
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<CrowdFund> CrowdFunds { get; set; }
    }


    public class Donation : ITransactionItem
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public decimal Amount { get; set; }

        public string TxRef { get; set; }

        public int CrowdFundId { get; set; }

        public bool paid { get; set; } = false;

        public string Date { get; set; }

        [ForeignKey("CrowdFundId")]
        public virtual CrowdFund CrowdFund { get; set; }
    }
}
