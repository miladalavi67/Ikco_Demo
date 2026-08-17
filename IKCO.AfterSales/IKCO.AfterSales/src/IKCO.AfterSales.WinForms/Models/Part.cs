namespace IKCO.AfterSales.WinForms.Models
{
    public class Part
    {
        public int PartId { get; set; }
        public string PartCode { get; set; }
        public string PartName { get; set; }
        public decimal UnitPrice { get; set; }
        public int StockQty { get; set; }
        public int MinStockQty { get; set; }
        public bool IsActive { get; set; }

        public bool IsLowStock
        {
            get { return StockQty <= MinStockQty; }
        }

        public override string ToString()
        {
            return PartCode + " - " + PartName;
        }
    }
}
