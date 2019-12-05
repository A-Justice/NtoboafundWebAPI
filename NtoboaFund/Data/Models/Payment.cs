using System;

namespace NtoboaFund.Data.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public string Reference { get; set; }

        public string ItemPayedFor { get; set; }

        public int ItemPayedForId { get; set; }

        public decimal AmountPaid { get; set; }

        public string TelcoTransactionId { get; set; }

        public DateTime DatePayed { get; set; }

        public string UserId { get; set; }

        public bool IsPaid { get; set; }
    }
}
