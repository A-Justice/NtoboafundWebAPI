using System.ComponentModel.DataAnnotations.Schema;

namespace NtoboaFund.Data.Models
{
    public class LuckyMe : IStakeType
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public string Date { get; set; }

        public string Period { get; set; }

        public string Status { get; set; }


        public decimal AmountToWin { get; set; }

        public string DateDeclared { get; set; }

        public int? TransferId { get; set; }

        public string TxRef { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

    }
}
