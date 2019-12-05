using System.Collections.Generic;

namespace NtoboaFund.Data.DTO_s
{
    public class Result
    {
        public string orderCode { get; set; }
        public string paymentCode { get; set; }
        public string payToken { get; set; }
        public object description { get; set; }
        public string qrCodeUrl { get; set; }
        public double fullDiscountAmount { get; set; }
        public List<object> discounts { get; set; }
    }

    public class SlydePayCreateInvoiceResponse
    {
        public bool success { get; set; }
        public Result result { get; set; }
        public string errorMessage { get; set; }
        public object errorCode { get; set; }
    }
}
