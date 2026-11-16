using business_logic.Constants;

namespace business_logic.Helpers
{
    public static class BusinessTimeZoneConverter
    {
        private static readonly TimeZoneInfo BusinessTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(BusinessTimeZone.TimeZoneId);

        public static DateTime ConvertBusinessDateStartToUtc(DateOnly businessDate)
        {
            var businessDateStartDateTime = businessDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(businessDateStartDateTime, BusinessTimeZoneInfo);
        }

        public static DateTime ConvertUtcToBusinessDateTime(DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException($"Expected a UTC date and time, but '{utcDateTime:O}' has kind '{utcDateTime.Kind}'.", nameof(utcDateTime));
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, BusinessTimeZoneInfo);
        }

        public static DateTime GetBusinessNowDateTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BusinessTimeZoneInfo);
        }

        public static DateOnly GetBusinessToday()
        {
            return DateOnly.FromDateTime(GetBusinessNowDateTime());
        }

        public static string FormatUtcAsBusinessDateTime(DateTime utcDateTime)
        {
            return ConvertUtcToBusinessDateTime(utcDateTime).ToString(DisplayFormats.DateTime) + DisplayFormats.BusinessTimeZoneSuffix;
        }
    }
}
