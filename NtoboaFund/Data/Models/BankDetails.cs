using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NtoboaFund.Data.Models
{
    public class BankDetails
    {
        [ForeignKey("User")]
        [Key]
        public string BankDetailsId { get; set; }

        public string BankName { get; set; }

        public string AccountNumber { get; set; }

        public string SwiftCode { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
