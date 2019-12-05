namespace NtoboaFund.Data.DTO_s
{
    public class ScholarshipParticipantDTO : ParticipantDTO
    {
    }

    public class BusinessParticipantDTO : ParticipantDTO
    {
    }

    public class LuckyMeParticipantDTO : ParticipantDTO
    {
    }

    public class ParticipantDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        public string UserId { get; set; }

        public string AmountStaked { get; set; }

        public string AmountToWin { get; set; }

        public string Status { get; set; }
        public string DateDeclared { get; set; }
        public string TxRef { get; set; }

    }
}
