namespace NtoboaFund.Data.DTO_s
{
    public class RaveRequestDTO
    {
        public string PBFPubKey { get; set; }
        public string currency { get; set; }
        public string payment_type { get; set; }
        public string country { get; set; }
        public string amount { get; set; }

        public string email { get; set; }

        public string phonenumber { get; set; }

        public string network { get; set; }
        public string firstname { get; set; }

        public string lastname { get; set; }

        /// <summary>
        /// Only needed for vodafone users
        /// </summary>
        public string voucher { get; set; }
        public string IP { get; set; }
        public string txRef { get; set; }
        public string orderRef { get; set; }

        public int is_mobile_money_gh { get; set; }

        public string redirect_url { get; set; }

        public string device_fingerprint { get; set; }


    }
}
