using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NtoboaFund.Data.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public string TransactionId { get; set; }

        public string PayerId { get; set; }

        public string ItemPayedFor { get; set; }

        public int ItemPayedForId { get; set; }

        public DateTime DatePayed { get; set; }

        public string UserPayedId { get;set;}
    }
}
