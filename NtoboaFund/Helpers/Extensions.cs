using System;

namespace NtoboaFund.Helpers
{
    public static class Extensions
    {
        public static DateTime ChangeTime(this DateTime dateTime, int hours, int minutes, int seconds, int milliseconds)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                hours,
                minutes,
                seconds,
                milliseconds,
                dateTime.Kind);
        }

        public static DateTime EndOfWeek(this DateTime dateTime, int hours, int minutes, int seconds, int milliseconds)
        {
            if (DateTime.Now.Day == dateTime.LastDayOfWeek().Day && DateTime.Now.Hour >= hours && DateTime.Now.Second > seconds)
            {
                dateTime = dateTime.AddDays(1);
            }
            return new DateTime(
                dateTime.LastDayOfWeek().Year,
                dateTime.LastDayOfWeek().Month,
                dateTime.LastDayOfWeek().Day,
                hours,
                minutes,
                seconds,
                milliseconds,
                dateTime.Kind);
        }


        public static DateTime EndOfMonth(this DateTime dateTime, int hours, int minutes, int seconds, int milliseconds)
        {
            var day = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);

            if (DateTime.Now.Day == day && DateTime.Now.Hour >= hours && DateTime.Now.Minute >= minutes && DateTime.Now.Second > seconds)
            {
                dateTime = dateTime.AddMonths(1);
                day = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
            }

            return new DateTime(
            dateTime.Year,
            dateTime.Month,
            day,
            hours,
            minutes,
            seconds,
            milliseconds,
            dateTime.Kind);
        }

        public static DateTime NextQuater(this DateTime dateTime, int hours, int minutes, int seconds, int milliseconds)
        {
            int quaterlyYear = dateTime.Year;
            //The list of months that are considered to be quaters
            int[] quaterlyMonths = new int[] { 3, 6, 9, 12 };
            //initialize the quaterly month;
            int quaterlyMonth = dateTime.Month;
            //initialize the quaterly day;
            int day = 0;
            //Loop through the list of quaterly months from least to highest
            foreach (var item in quaterlyMonths)
            {
                //if the current iteration is considered to be be higher or equal to the current month
                //it is suspected to be the closest quater
                if (dateTime.Month <= item)
                {
                    //get the total number of days in the month of the current iteration
                    day = DateTime.DaysInMonth(dateTime.Year, item);

                    //It can happen that we are in the last day of a quater month .. but the current draw the is completed

                    if (dateTime.Month == item && dateTime.Day == day && dateTime.Hour >= hours && dateTime.Minute >= minutes && dateTime.Second > seconds)
                    {
                        //If the above condition is true but we are in the last month of the year ..
                        // Then we have to change the current year to the next and choose it's first quater
                        if (item == 12)
                        {
                            quaterlyYear += 1;
                            quaterlyMonth = 3;

                            //set the day again because the month and year has changed
                            day = DateTime.DaysInMonth(quaterlyYear, quaterlyMonth);
                        }

                        //In that case continue the loop to find the closest quater
                        continue;
                    }

                    //assign the current iteration as the current month and
                    quaterlyMonth = item;
                    //break from the loop
                    break;
                }
            }


            return new DateTime(
                quaterlyYear,
                quaterlyMonth,
                day,
                hours,
                minutes,
                seconds,
                milliseconds,
                dateTime.Kind);
        }

        public static DateTime FirstDayOfWeek(this DateTime dt)
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var diff = dt.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;

            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-diff).Date;
        }

        public static DateTime LastDayOfWeek(this DateTime dt) =>
            dt.FirstDayOfWeek().AddDays(6);

        public static DateTime MidNight(this DateTime dateTime)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                0,
                0,
                0,
                0,
                dateTime.Kind);
        }

        public static DateTime DailyStakeEndDate(this DateTime dateTime)
        {

            return ThreePerDay(dateTime);

            //if (dateTime.Hour >= 18 && dateTime.Second >= 0)
            //{

            //    dateTime = dateTime.AddDays(1);
            //}

            //return new DateTime(
            //    dateTime.Year,
            //    dateTime.Month,
            //    dateTime.Day,
            //    18,
            //    0,
            //    0,
            //    0,
            //    dateTime.Kind);
        }

        public static DateTime ThreePerDay(this DateTime dateTime)
        {
            if (dateTime.Hour >= 12 && dateTime.Hour < 15 && dateTime.Second >= 0)
            {
                return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                15,
                0,
                0,
                0,
                dateTime.Kind);
            }
            else if (dateTime.Hour >= 15 && dateTime.Hour <= 18 && dateTime.Second >= 0)
            {

                return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                18,
                0,
                0,
                0,
                dateTime.Kind);
            }
            else if (dateTime.Hour >= 18 && dateTime.Second >= 0)
            {
                dateTime = dateTime.AddDays(1);
            }

            return new DateTime(
            dateTime.Year,
            dateTime.Month,
            dateTime.Day,
            12,
            0,
            0,
            0,
            dateTime.Kind);

        }


        public static DateTime NextFiveMinutes(this DateTime dateTime)
        {
            var minute = dateTime.Minute;
            if (minute % 5 == 0)
                minute += 5;

            while (minute % 5 != 0)
            {
                minute++;
            }
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                minute,
                0,
                0,
                dateTime.Kind);
        }


        public static string ToString(this decimal theDecimal)
        {
            return theDecimal.ToString("0.##");
        }


    }
}
