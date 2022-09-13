using System;
using NtoboaFund.Data.Models;

namespace NtoboaFund.Data.Interfaces
{
    public interface ITransactionItem
    {
        int Id { get; set; }

        decimal Amount { get; set; }

        string Date { get; set; }

        string TxRef { get; set; }

        string UserId { get; set; }

        ApplicationUser User { get; set; }
    }
}
