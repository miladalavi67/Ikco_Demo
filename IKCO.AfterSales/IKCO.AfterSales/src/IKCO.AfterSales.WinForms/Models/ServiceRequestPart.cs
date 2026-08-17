namespace IKCO.AfterSales.WinForms.Models
{
    public class ServiceRequestPart
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public int PartId { get; set; }
        public string PartCode { get; set; }
        public string PartName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
