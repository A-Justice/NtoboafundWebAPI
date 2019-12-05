using System;

namespace NtoboaFund.Data.DTO_s
{
    public class Transfer
    {
        public int id { get; set; }
        public string account_number { get; set; }
        public string bank_code { get; set; }
        public string fullname { get; set; }
        public DateTime date_created { get; set; }
        public string currency { get; set; }
        public object debit_currency { get; set; }
        public double amount { get; set; }
        public int fee { get; set; }
        public string status { get; set; }
        public string reference { get; set; }
        public object meta { get; set; }
        public string narration { get; set; }
        public object approver { get; set; }
        public string complete_message { get; set; }
        public int requires_approval { get; set; }
        public int is_approved { get; set; }
        public string bank_name { get; set; }
    }

    public class MomoTransferResponse
    {
        public string event_type { get; set; }
        public Transfer transfer { get; set; }
    }
}
