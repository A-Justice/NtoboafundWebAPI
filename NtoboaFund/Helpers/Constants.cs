using System;

namespace NtoboaFund.Helpers
{
    public enum PaymentGateway
    {
        flutterwave,
        slydepay,
        redde,
    }
    public static class Constants
    {
        public static PaymentGateway PaymentGateway { get; set; } = PaymentGateway.redde;
        public static int ScholarshipStakeOdds { get; set; } = 100;
        public static int BusinessStakeOdds { get; set; } = 10;
        public static int LuckymeStakeOdds { get; set; } = 10;

        public static int[] LuckyMeStakes { get; set; } = { 1, 5, 10, 20, 50, 100, 500 };

        public static int[] ScholarshipStakes { get; set; } = { 20, 50, 100 };

        public static int[] BusinessStakes { get; set; } = { 100, 500, 1000, 2000 };

        public static string MasterNumber { get; set; } = "233557560016";

        /// <summary>
        /// Determines how long a dummy takes before it participates in the draw again
        /// </summary>
        public static int DaysForDummyToRepeat { get; set; } = 5;

        /// <summary>
        /// Depicts the treshold used to determine whether participants wins or not
        /// </summary>
        public static decimal WinnerTreshold { get; set; } = Convert.ToDecimal(1.0 / 3.0);


        public static decimal PointConstant { get; set; } = Convert.ToDecimal(3.0);
    }

}
