using System;

namespace BusParkManagementSystem.Models
{
    public class Maintenance
    {
        public int Id { get; set; }
        public int BusId { get; set; }
        public string BusGovPlate { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public string MaintenanceType { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public int EngineerId { get; set; }
        public string EngineerName { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
    }
}