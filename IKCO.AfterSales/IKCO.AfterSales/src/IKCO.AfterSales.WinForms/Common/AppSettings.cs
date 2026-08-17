using System.Configuration;

namespace IKCO.AfterSales.WinForms.Common
{
    /// <summary>
    /// Reads application settings from App.config.
    /// </summary>
    public static class AppSettings
    {
        public static string ConnectionString
        {
            get
            {
                var cs = ConfigurationManager.ConnectionStrings["AfterSalesDb"];
                return cs == null ? string.Empty : cs.ConnectionString;
            }
        }

        public static int DefaultPageSize
        {
            get
            {
                int size;
                if (int.TryParse(ConfigurationManager.AppSettings["DefaultPageSize"], out size) && size > 0)
                    return size;
                return 20;
            }
        }

        /// <summary>
        /// Business rule: loyalty discount percentage applied to labour cost
        /// once a customer reaches LoyaltyThreshold completed requests.
        /// </summary>
        public static decimal LoyaltyDiscountPercent
        {
            get
            {
                decimal p;
                if (decimal.TryParse(ConfigurationManager.AppSettings["LoyaltyDiscountPercent"], out p))
                    return p;
                return 5m;
            }
        }

        public static int LoyaltyThreshold
        {
            get
            {
                int t;
                if (int.TryParse(ConfigurationManager.AppSettings["LoyaltyThreshold"], out t))
                    return t;
                return 3;
            }
        }
    }
}
