using System;

namespace OsrsOpen.Chase.Reference
{
    public sealed class TimeRange
    {
        public DateTime StartTime
        {
            get;
        }

        public DateTime EndTime
        {
            get;
        }

        public bool InInterval(DateTime value)
        {
            value = TimeRangeUtils.ToNormal(value);
            return value >= StartTime && value < EndTime;
        }

        public bool InInterval(DateTime value, bool startInclusive, bool endInclusive)
        {
            value = TimeRangeUtils.ToNormal(value);
            if (value > StartTime && value < EndTime)
                return true;

            if (startInclusive && value.Equals(StartTime))
                return true;
            if (endInclusive && value.Equals(EndTime))
                return true;

            return false;
        }

        public TimeRange(DateTime startTime, DateTime endTime)
        {
            startTime = TimeRangeUtils.ToNormal(startTime);
            endTime = TimeRangeUtils.ToNormal(endTime);
            if (startTime > endTime)
                throw new ArgumentException();

            this.StartTime = startTime;
            this.EndTime = endTime;
        }
    }

    public static class TimeRangeUtils
    {
        public static DateTime ToNormal(DateTime item)
        {
            if (item.Kind == DateTimeKind.Utc)
                return item;
            if (item.Kind == DateTimeKind.Local)
                return new DateTime(item.ToUniversalTime().Ticks, DateTimeKind.Utc);

            return new DateTime(item.Ticks, DateTimeKind.Utc);
        }

        public static TimeRange Create(DateTime a, DateTime b)
        {
            a = ToNormal(a);
            b = ToNormal(b);
            if (a < b)
                return new TimeRange(a, b);
            return new TimeRange(b, a);
        }

        public static TimeRange Create(DateTime start, TimeSpan duration)
        {
            return Create(start, start.Add(duration));
        }

        public static TimeRange Create(DateTime start, double deltaSeconds)
        {
            return Create(start, start.AddSeconds(deltaSeconds));
        }
    }
}
