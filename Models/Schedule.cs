namespace BusParkManagementSystem.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public string FirstDepartureTime { get; set; }
        public string LastDepartureTime { get; set; }
        public int Intervals { get; set; }
    }
}