using System;

namespace NtoboaFund.Data.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public string PayerId { get; set; }

        public long TransactionId { get; set; }

        public string ItemPayedFor { get; set; }

        public int ItemPayedForId { get; set; }

        public DateTime? DatePayed { get; set; }

        public bool IsPaid { get; set; }
    }
}
