namespace NtoboaFund.Data.DTO_s
{
    public class ReddeRecieveWebhookCallback
    {
        public string Reason { get; set; }
        public string Clienttransid { get; set; }
        public string Clientreference { get; set; }
        public string Transactionid { get; set; }
        public string Statusdate { get; set; }
        public string Status { get; set; }
    }

    public class ReddeCashoutWebhookCallback
    {
        public string Reason { get; set; }
        public string Clienttransid { get; set; }
        public string Clientreference { get; set; }
        public int Transactionid { get; set; }
        public string Statusdate { get; set; }
        public string Status { get; set; }

        public string Telcotransid { get; set; }
    }

}
