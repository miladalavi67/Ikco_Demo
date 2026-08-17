using System.Collections.Generic;

namespace IKCO.AfterSales.WinForms.Models
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public PagedResult()
        {
            Items = new List<T>();
        }

        public int PageCount
        {
            get
            {
                if (PageSize <= 0) return 0;
                return (TotalCount + PageSize - 1) / PageSize;
            }
        }
    }
}
