namespace Common.Commons
{
    public static class Utils
    {
        public static DateTime UtcToLocalTimeZone(DateTime utc)
        {
            TimeZoneInfo tzi = TimeZoneInfo.Local;
            DateTime indo = TimeZoneInfo.ConvertTimeFromUtc(utc, tzi);

            return indo;
        }
    }
}
