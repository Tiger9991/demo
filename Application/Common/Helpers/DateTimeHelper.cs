using System.Globalization;
using System;

namespace Application.Common.Helpers
{
    public static class DateTimeHelper
    {
        public static readonly TimeZoneInfo EgyptZone;
        public static readonly CultureInfo ArabicCulture;

        static DateTimeHelper()
        {
            // Set up Egypt Time Zone (works on Windows, Linux, and Mac)
            try
            {
                EgyptZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            }
            catch
            {
                EgyptZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
            }

            // Set up Arabic culture
            ArabicCulture = new CultureInfo("ar-EG");
            ArabicCulture.NumberFormat.DigitSubstitution = DigitShapes.NativeNational;
        }

        public static string ToEgyptianArabic12Hour(this DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "-";

            // Convert from UTC to Egypt time
            DateTime egyptTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime.Value, EgyptZone);
            return egyptTime.ToString("yyyy-MM-dd hh:mm tt", ArabicCulture);
        }
    }
}
