using System;

namespace IKCO.AfterSales.WinForms.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string NationalCode { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int VehicleCount { get; set; }

        public override string ToString()
        {
            return FullName;
        }
    }
}
