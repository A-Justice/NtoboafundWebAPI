namespace NtoboaFund.Data.DTO_s
{
    public class RaveRequestDTO
    {
        public string amount { get; set; }

        public string customer_email { get; set; }

        public string customer_phone { get; set; }

        public string currency { get; set; }

        public string txRef { get; set; }

        public string PBFPubKey { get; set; }

        public string redirect_url { get; set; }

        public string payment_plan { get; set; }

        public string payment_options { get; set; }

        public string country { get; set; }

    }
}
