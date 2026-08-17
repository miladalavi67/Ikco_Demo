using System;

namespace IKCO.AfterSales.WinForms.Models
{
    public class ServiceRequest
    {
        public int RequestId { get; set; }
        public string RequestNo { get; set; }

        public int VehicleId { get; set; }
        public string PlateNumber { get; set; }
        public string Model { get; set; }

        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        public int? TechnicianId { get; set; }
        public string TechnicianName { get; set; }

        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public bool IsFinal { get; set; }

        public DateTime RequestDate { get; set; }
        public string Description { get; set; }

        public decimal LaborHours { get; set; }
        public decimal LaborCost { get; set; }
        public decimal PartsCost { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalCost { get; set; }

        public DateTime? CompletedDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
