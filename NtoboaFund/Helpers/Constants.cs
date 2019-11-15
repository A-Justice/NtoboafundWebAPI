namespace NtoboaFund.Helpers
{
    public static class Constants
    {
        public static int ScholarshipStakeOdds { get; set; } = 100;
        public static int BusinessStakeOdds { get; set; } = 10;
        public static int LuckymeStakeOdds { get; set; } = 10;

        public static int[] LuckyMeStakes { get; set; } = { 1, 5, 10, 20, 50, 100, 500 };

        public static int[] ScholarshipStakes { get; set; } = { 100, 50, 20 };

        public static int[] BusinessStakes { get; set; } = { 100, 500, 1000, 2000 };

        /// <summary>
        /// Determines how long a dummy takes before it participates in the draw again
        /// </summary>
        public static int DaysForDummyToRepeat { get; set; } = 5;

        /// <summary>
        /// Depicts the treshold used to determine whether participants wins or not
        /// </summary>
        public static decimal WinnerTreshold { get; set; } = 1 / 3;


        public static decimal PointConstant { get; set; } = 1 / 5;
    }

}
