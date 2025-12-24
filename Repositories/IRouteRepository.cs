using BusParkManagementSystem.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BusParkManagementSystem.Repositories
{
    public interface IRouteRepository
    {
        Task<IEnumerable<Route>> GetAllAsync();
        Task<Route> GetByIdAsync(int id);
        Task<IEnumerable<Route>> GetByRouteNumberAsync(int routeNumber);
        Task<IEnumerable<RouteSection>> GetRouteStopsAsync(int routeId); 
        Task<int> AddAsync(Route route);
        Task<bool> UpdateAsync(Route route);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Route>> SearchAsync(string searchTerm);
        Task<int> GetBusesCountOnRouteAsync(int routeId);
        Task<decimal> GetRouteLengthAsync(int routeId);
        IDbConnection GetConnection();


        // Методы для работы с графиком движения
        Task<int> SaveScheduleAsync(int routeId, string firstDepartureTime,
                                   string lastDepartureTime, int interval);
        Task<bool> UpdateScheduleAsync(int routeId, string firstDepartureTime,
                                      string lastDepartureTime, int interval);

        // Метод проверки уникальности номера маршрута
        Task<bool> IsRouteNumberUniqueAsync(int routeNumber, int? excludeId = null);

        // Методы для работы с интервалами движения
        Task<IEnumerable<MovementInterval>> GetMovementIntervalsAsync(int scheduleId);
        Task<int> AddMovementIntervalAsync(MovementInterval interval);
        Task<bool> UpdateMovementIntervalAsync(MovementInterval interval);
        Task<bool> DeleteMovementIntervalAsync(int intervalId);
    }

    // Новая модель для интервалов движения
    public class MovementInterval
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int IntervalMinutes { get; set; }
        public string DayType { get; set; } // 'будни', 'выходные', 'праздничные'
    }
}