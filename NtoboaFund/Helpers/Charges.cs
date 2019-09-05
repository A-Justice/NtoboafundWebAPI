namespace NtoboaFund.Helpers
{
    public static class Settings
    {
        public static decimal ScholarshipStakeAmount { get; set; } = 100;

        public static int ScholarshipStakeOdds { get; set; } = 100;
        public static int BusinessStakeOdds { get; set; } = 10;
        public static int LuckymeStakeOdds { get; set; } = 10;

        public static int[] LuckyMeStakes { get; set; } = {1,5,10,20,50,100,500 };

        public static int[] ScholarshipStakes { get;set;} = {100};
        
        public static int[] BusinessStakes { get; set; } = {100,500,1000,2000 };

        public static int DaysForDummyToRepeat { get; set; } = 5;

        public static decimal WinnerTreshold { get; set; } = 1/3;
    }

}
