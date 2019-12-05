using NtoboaFund.Data.Models;

namespace NtoboaFund.Data
{
    public interface IStakeType
    {
        int Id { get; set; }

        decimal Amount { get; set; }

        string Date { get; set; }

        string Period { get; set; }

        string Status { get; set; }


        decimal AmountToWin { get; set; }

        string DateDeclared { get; set; }

        int? TransferId { get; set; }

        string TxRef { get; set; }

        string UserId { get; set; }

        ApplicationUser User { get; set; }

    }
}
