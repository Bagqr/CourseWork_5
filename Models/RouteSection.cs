namespace BusParkManagementSystem.Models
{
    public class RouteSection
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public int StopId { get; set; }
        public string StopName { get; set; }
        public int Order { get; set; }
        public int DistanceFromStart { get; set; }
    }
}