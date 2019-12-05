namespace NtoboaFund.Data.DTO_s
{
    public class SlydePayCreateInvoiceRequest
    {
        public string EmailOrMobileNumber { get; set; }

        public string MerchantKey { get; set; }

        public decimal Amount { get; set; }

        public string OrderCode { get; set; }

        public SlydePayOrderItem[] OrderItems { get; set; }

        public bool SendInvoice { get; set; }

        public string PayOption { get; set; }

        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }

        public string CustomerMobileNumber { get; set; }
    }
}
