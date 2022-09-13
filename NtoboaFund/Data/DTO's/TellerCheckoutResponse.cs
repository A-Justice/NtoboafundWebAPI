using System;
namespace NtoboaFund.Data.DTO_s

{
    public class TellerCheckoutResponse
    {
        public string Success { get; set; }

        public int Code { get; set; }

        public string Reason { get; set; }

        public string Token { get; set; }

        public string Checkout_url { get; set; }
    }
}
