using System;
namespace NtoboaFund.Data.DTOs
{
    public class TellerRestRequest
    {
        public string amount { get; set; }
        public string processing_code { get; set; }
        public string desc { get; set; }
        public string transaction_id { get; set; }
        public string merchant_id { get; set; }
        public string subscriber_number { get; set; }
        public string r_switch { get; set; }
        public string voucher_code { get; set; }

    }
}
