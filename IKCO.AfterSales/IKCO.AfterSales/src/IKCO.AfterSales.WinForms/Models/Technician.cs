namespace IKCO.AfterSales.WinForms.Models
{
    public class Technician
    {
        public int TechnicianId { get; set; }
        public string FullName { get; set; }
        public string PersonnelCode { get; set; }
        public string Specialty { get; set; }
        public decimal HourlyRate { get; set; }
        public bool IsActive { get; set; }

        public override string ToString()
        {
            return FullName;
        }
    }
}
