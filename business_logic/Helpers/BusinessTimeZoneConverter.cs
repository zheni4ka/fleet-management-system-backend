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

        public static DateTime GetBusinessNowDateTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BusinessTimeZoneInfo);
        }
    }
}
