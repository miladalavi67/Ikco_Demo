using System;
using System.Globalization;

namespace IKCO.AfterSales.WinForms.Common
{
    /// <summary>
    /// Converts between Gregorian DateTime and Persian (Jalali) text.
    /// The database stores Gregorian dates; the UI shows Persian ones.
    /// </summary>
    public static class PersianDateHelper
    {
        private static readonly PersianCalendar Calendar = new PersianCalendar();

        private static readonly string[] MonthNames =
        {
            "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
            "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
        };

        public static int GetPersianYear(DateTime date)
        {
            return Calendar.GetYear(date);
        }

        public static string GetMonthName(int monthNo)
        {
            if (monthNo < 1 || monthNo > 12) return string.Empty;
            return MonthNames[monthNo - 1];
        }

        /// <summary>Formats as 1405/02/17</summary>
        public static string ToPersianDate(DateTime date)
        {
            return string.Format("{0:0000}/{1:00}/{2:00}",
                Calendar.GetYear(date), Calendar.GetMonth(date), Calendar.GetDayOfMonth(date));
        }

        public static string ToPersianDate(DateTime? date)
        {
            return date.HasValue ? ToPersianDate(date.Value) : string.Empty;
        }

        /// <summary>Formats as 1405/02/17 - 14:30</summary>
        public static string ToPersianDateTime(DateTime date)
        {
            return ToPersianDate(date) + " - " + date.ToString("HH:mm");
        }

        /// <summary>Parses 1405/02/17 back to a Gregorian DateTime.</summary>
        public static bool TryParsePersianDate(string text, out DateTime result)
        {
            result = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(text)) return false;

            var parts = text.Trim().Replace('-', '/').Split('/');
            if (parts.Length != 3) return false;

            int y, m, d;
            if (!int.TryParse(parts[0], out y)) return false;
            if (!int.TryParse(parts[1], out m)) return false;
            if (!int.TryParse(parts[2], out d)) return false;

            if (y < 1300 || y > 1500 || m < 1 || m > 12 || d < 1 || d > 31) return false;

            try
            {
                result = Calendar.ToDateTime(y, m, d, 0, 0, 0, 0);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        /// <summary>Formats an amount as "1,650,000 ریال".</summary>
        public static string ToCurrency(decimal amount)
        {
            return amount.ToString("#,##0") + " ریال";
        }
    }
}
