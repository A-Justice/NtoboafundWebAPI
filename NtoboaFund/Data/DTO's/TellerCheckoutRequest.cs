using System;
namespace NtoboaFund.Data.DTO_s
{
    public class TellerCheckoutRequest
    {
        public string merchant_id { get; set; }

        public string transaction_id { get; set; }

        public string desc { get; set; }

        public string amount { get; set; }

        public string redirect_url { get; set; }

        public string email { get; set; }

        public string API_Key { get; set; }

        public string apiuser { get; set; }
    }
}
