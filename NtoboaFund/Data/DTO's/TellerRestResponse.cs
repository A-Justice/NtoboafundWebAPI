using System;
namespace NtoboaFund.Data.DTOs
{
    public class TellerRestResponse
    {
        public string transaction_id { get; set; }
        public string status { get; set; }
        public int code { get; set; }
        public string reason { get; set; }
    }
}
