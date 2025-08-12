using System;

namespace RPA.Core
{
    public class ScheduleOptions
    {
        public const string NoRepeat = "None";
        public const string Monthly = "Monthly";
        public const string Daily = "Daily";
        public const string Hourly = "Hourly";
        public const string Weekly = "Weekly";
        public const string EveryXMinutes = "EveryXMinutes";
        public int Year { get; set; } = 1;
        public int Month { get; set; } = 1;
        /// <summary>
        /// Specific day of the month. Use -1 for last day of Month
        /// </summary>
        public int Day { get; set; } = 1;

        public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Monday;

        /// <summary>
        /// Specific hour of the day. From 0 - 23
        /// </summary>
        public int Hour { get; set; } = 0;
        /// <summary>
        /// Specific minute of the hour. From 0 - 59
        /// </summary>
        public int Minute { get; set; } = 0;
        /// <summary>
        /// RepeatType: Monthly - Weekly- Daily - Hourly - None
        /// </summary>
        public string RepeatType { get; set; } = NoRepeat;

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public int MinuteInterval { get; set; } = 15;

        public string JSONParam { get; set; }

        public bool IsValid => Validate();
        public DateTime NextRunTime => GetNextRunTime();

        DateTime GetNextRunTime()
        {
            DateTime next = new DateTime();
            switch (RepeatType)
            {
                case EveryXMinutes:
                    {
                        if(DateTime.Now < StartDateTime)
                        {
                            next = StartDateTime.AddMinutes(MinuteInterval);
                        }
                        else if(DateTime.Now > StartDateTime && DateTime.Now < EndDateTime)
                        {
                            next = DateTime.Now.AddMinutes(MinuteInterval);
                        }
                        break;
                    }
                // Run once at specific time
                case NoRepeat:
                    {
                        next = new DateTime(Year, Month, Day, Hour, Minute, 0);
                        break;
                    }
                case Hourly:
                    {
                        // Not pass run time
                        if (DateTime.Now.Minute < Minute)
                        {
                            next = DateTime.Today + new TimeSpan(DateTime.Now.Hour, Minute, 0);
                        }
                        // Pass run time. Add 1 hour 
                        else
                        {
                            next = (DateTime.Today
                                + new TimeSpan(DateTime.Now.Hour, Minute, 0)
                                ).AddHours(1);
                        }
                        break;
                    }
                case Daily:
                    {
                        // Not pass run time
                        if (DateTime.Now.Hour <= Hour && DateTime.Now.Minute < Minute)
                        {
                            next = DateTime.Today + new TimeSpan(Hour, Minute, 0);
                        }
                        // Pass run time. Add 1 day
                        else
                        {
                            next = (DateTime.Today + new TimeSpan(Hour, Minute, 0)).AddDays(1);
                        }
                        break;
                    }
                case Weekly:
                    {
                        next = FindNextDayOfWeek(DateTime.Today, this.DayOfWeek);
                        // Haven't pass runtime . Simple add timespan
                        if (DateTime.Now.TimeOfDay < new TimeSpan(Hour, Minute, 0))
                        {
                            next += new TimeSpan(Hour, Minute, 0);
                        }
                        // Have passed runtime. recalculate from next day
                        else
                        {
                            next = FindNextDayOfWeek(DateTime.Today.AddDays(1), this.DayOfWeek) + new TimeSpan(Hour, Minute, 0);
                        }
                        break;
                    }
                case Monthly:
                    {
                        var runDate = Day == -1
                            ? DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)
                            : Day;
                        // Not pass run time
                        if (DateTime.Now.Day <= runDate
                            && DateTime.Now.Hour <= Hour
                            && DateTime.Now.Minute < Minute)
                        {
                            next = DateTime.Today + new TimeSpan(Hour, Minute, 0);
                        }
                        // Pass run time. Add 1 Month
                        else
                        {
                            next = (new DateTime(DateTime.Now.Year, DateTime.Now.Month, runDate)
                                   + new TimeSpan(Hour, Minute, 0)).AddMonths(1);
                        }
                        break;
                    }
                default: break;
            }
            return next;
        }
        DateTime FindNextDayOfWeek(DateTime date, DayOfWeek nextDayofWeek)
        {
            while (date.DayOfWeek != nextDayofWeek)
            {
                date = date.AddDays(1);
            }
            return date;
        }
        bool Validate()
        {
            return Year >= 0
                && 1 <= Month && Month <= 12
                && ((1 <= Day && Day <= 28) || Day == -1)
                && 0 <= Hour && Hour <= 23
                && 0 <= Minute && Minute <= 59;
        }
    }
}
