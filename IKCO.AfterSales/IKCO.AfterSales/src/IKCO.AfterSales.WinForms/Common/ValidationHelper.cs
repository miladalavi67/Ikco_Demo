using System;
using System.Text.RegularExpressions;

namespace IKCO.AfterSales.WinForms.Common
{
    /// <summary>
    /// Input validation rules shared by the edit forms.
    /// </summary>
    public static class ValidationHelper
    {
        private static readonly Regex MobileRegex = new Regex(@"^09\d{9}$");
        private static readonly Regex EmailRegex  = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        private static readonly Regex DigitsOnly  = new Regex(@"^\d+$");

        public static bool IsRequired(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static bool IsValidMobile(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && MobileRegex.IsMatch(value.Trim());
        }

        public static bool IsValidEmail(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return true; // optional field
            return EmailRegex.IsMatch(value.Trim());
        }

        /// <summary>
        /// Iranian national code: 10 digits with a check digit.
        /// </summary>
        public static bool IsValidNationalCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            value = value.Trim();
            if (value.Length != 10 || !DigitsOnly.IsMatch(value)) return false;

            // reject sequences such as 1111111111
            bool allSame = true;
            for (int i = 1; i < 10; i++)
            {
                if (value[i] != value[0]) { allSame = false; break; }
            }
            if (allSame) return false;

            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += (value[i] - '0') * (10 - i);
            }

            int remainder = sum % 11;
            int check = value[9] - '0';

            return remainder < 2 ? check == remainder : check == 11 - remainder;
        }

        public static bool IsValidProductionYear(int year)
        {
            int currentPersianYear = PersianDateHelper.GetPersianYear(DateTime.Now);
            return year >= 1350 && year <= currentPersianYear + 1;
        }
    }
}
