using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NtoboaFund.Data.DTO_s
{
    public class Customer
    {
        public int id { get; set; }
        public string phone { get; set; }
        public string fullName { get; set; }
        public object customertoken { get; set; }
        public string email { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public object deletedAt { get; set; }
        public int AccountId { get; set; }
    }

    public class Entity
    {
        public string id { get; set; }
    }

    public class WebhookCallback
    {
        public int id { get; set; }
        public string txRef { get; set; }
        public string flwRef { get; set; }
        public string orderRef { get; set; }
        public object paymentPlan { get; set; }
        public DateTime createdAt { get; set; }
        public int amount { get; set; }
        public int charged_amount { get; set; }
        public string status { get; set; }
        public string IP { get; set; }
        public string currency { get; set; }
        public Customer customer { get; set; }
        public Entity entity { get; set; }
    }
}
