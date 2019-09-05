namespace NtoboaFund.Data.DTO_s
{
    public class ScholarshipParticipantDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        public string UserId { get; set; }

        public string AmountStaked { get; set; }

        public string AmountToWin { get; set; }

        public string Status { get;set;}
    }

    public class BusinessParticipantDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        public string UserId { get; set; }

        public string AmountStaked { get; set; }

        public string AmountToWin { get; set; }
        public string Status { get; set; }
    }

    public class LuckyMeParticipantDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        public string UserId { get; set; }

        public string AmountStaked { get; set; }

        public string AmountToWin { get; set; }
        public string Status { get; set; }
    }
}
