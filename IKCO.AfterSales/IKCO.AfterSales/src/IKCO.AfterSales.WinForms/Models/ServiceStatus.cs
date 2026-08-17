namespace IKCO.AfterSales.WinForms.Models
{
    public class ServiceStatus
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public bool IsFinal { get; set; }

        public override string ToString()
        {
            return StatusName;
        }
    }
}
