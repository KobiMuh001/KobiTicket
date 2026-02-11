namespace KobiMuhendislikTicket.Application.Common
{
    public static class DateTimeHelper
    {
        // Istanbul timezone (UTC+3)
        private static readonly TimeZoneInfo IstanbulTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

        /// <summary>
        /// Gets the current date and time in Istanbul timezone (UTC+3)
        /// </summary>
        public static DateTime GetLocalNow()
        {
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, IstanbulTimeZone);
        }

        /// <summary>
        /// Converts a UTC DateTime to Istanbul timezone
        /// </summary>
        public static DateTime ConvertToLocal(DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTime(utcDateTime, IstanbulTimeZone);
        }
    }
}
