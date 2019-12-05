namespace NtoboaFund.Data.DTO_s
{
    public class ReddeCheckoutRequest
    {
        public decimal Amount { get; set; }
        public string Apikey { get; set; }
        public string Appid { get; set; }
        public string Description { get; set; }
        public string Failurecallback { get; set; }
        public string Logolink { get; set; }
        public string Merchantname { get; set; }
        public string Clienttransid { get; set; }
        public string Successcallback { get; set; }
    }
}
