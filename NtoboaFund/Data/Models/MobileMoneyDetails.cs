using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NtoboaFund.Data.Models
{
    public class MobileMoneyDetails
    {
        [ForeignKey("User")]
        [Key]
        public string MobileMoneyDetailsId { get; set; }

        public string Country { get; set; }

        public string Network { get; set; }

        public string Voucher { get; set; }

        public string Number { get; set; }

        public string Currency { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
