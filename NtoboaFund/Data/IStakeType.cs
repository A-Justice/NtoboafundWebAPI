using NtoboaFund.Data.Interfaces;
using NtoboaFund.Data.Models;

namespace NtoboaFund.Data
{
    public interface IStakeType : ITransactionItem
    {
        string Period { get; set; }

        string Status { get; set; }

        decimal AmountToWin { get; set; }

        string DateDeclared { get; set; }

        int? TransferId { get; set; }

        bool deleted { get; set; }
    }
}
