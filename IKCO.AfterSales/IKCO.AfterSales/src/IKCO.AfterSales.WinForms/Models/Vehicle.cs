using System;

namespace IKCO.AfterSales.WinForms.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string PlateNumber { get; set; }
        public string Model { get; set; }
        public int ProductionYear { get; set; }
        public string VIN { get; set; }
        public int Mileage { get; set; }
        public DateTime CreatedAt { get; set; }

        public string DisplayTitle
        {
            get { return PlateNumber + " - " + Model; }
        }

        public override string ToString()
        {
            return DisplayTitle;
        }
    }
}
