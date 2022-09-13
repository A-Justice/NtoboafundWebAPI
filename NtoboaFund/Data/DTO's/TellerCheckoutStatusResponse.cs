using System;
namespace NtoboaFund.Data.DTO_s
{
    public class TellerCheckoutStatusResponse
    {
        public string Code { get; set; }

        public string Status { get; set; }

        public string Reason { get; set; }

        public string Transaction_id { get; set; }
    }
}
