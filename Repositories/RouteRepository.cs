using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BusParkManagementSystem.Data;
using BusParkManagementSystem.Models;
using Dapper;

namespace BusParkManagementSystem.Repositories
{
    public class RouteRepository : IRouteRepository
    {
        private readonly DatabaseContext _dbContext;
        public IDbConnection GetConnection()
        {
            return _dbContext.GetConnection();
        }
        // Метод проверки уникальности номера маршрута
        public async Task<bool> IsRouteNumberUniqueAsync(int routeNumber, int? excludeId = null)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "SELECT COUNT(*) FROM маршрут WHERE route_number = @RouteNumber";
                var parameters = new DynamicParameters();
                parameters.Add("@RouteNumber", routeNumber);

                if (excludeId.HasValue)
                {
                    query += " AND id != @ExcludeId";
                    parameters.Add("@ExcludeId", excludeId.Value);
                }

                var count = await connection.ExecuteScalarAsync<int>(query, parameters);
                return count == 0;
            }
        }

        // Сохранение графика движения
        public async Task<int> SaveScheduleAsync(int routeId, string firstDepartureTime,
                                               string lastDepartureTime, int interval)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    INSERT INTO график_движения (route_id, first_departure_time, last_departure_time, intervals)
                    VALUES (@RouteId, @FirstDepartureTime, @LastDepartureTime, @Interval);
                    SELECT last_insert_rowid();";

                return await connection.ExecuteScalarAsync<int>(query, new
                {
                    RouteId = routeId,
                    FirstDepartureTime = firstDepartureTime,
                    LastDepartureTime = lastDepartureTime,
                    Interval = interval
                });
            }
        }
        // Обновление графика движения
        public async Task<bool> UpdateScheduleAsync(int routeId, string firstDepartureTime,
                                                  string lastDepartureTime, int interval)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    UPDATE график_движения SET
                        first_departure_time = @FirstDepartureTime,
                        last_departure_time = @LastDepartureTime,
                        intervals = @Interval
                    WHERE route_id = @RouteId";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    RouteId = routeId,
                    FirstDepartureTime = firstDepartureTime,
                    LastDepartureTime = lastDepartureTime,
                    Interval = interval
                });

                return affectedRows > 0;
            }
        }
        // Получение интервалов движения
        public async Task<IEnumerable<MovementInterval>> GetMovementIntervalsAsync(int scheduleId)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        id as Id,
                        график_id as ScheduleId,
                        время_начала as StartTime,
                        время_окончания as EndTime,
                        интервал_минуты as IntervalMinutes,
                        тип_дня as DayType
                    FROM интервалы_движения
                    WHERE график_id = @ScheduleId
                    ORDER BY время_начала";

                return await connection.QueryAsync<MovementInterval>(query, new { ScheduleId = scheduleId });
            }
        }
        // Добавление интервала движения
        public async Task<int> AddMovementIntervalAsync(MovementInterval interval)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    INSERT INTO интервалы_движения (график_id, время_начала, время_окончания, интервал_минуты, тип_дня)
                    VALUES (@ScheduleId, @StartTime, @EndTime, @IntervalMinutes, @DayType);
                    SELECT last_insert_rowid();";

                return await connection.ExecuteScalarAsync<int>(query, interval);
            }
        }
        // Обновление интервала движения
        public async Task<bool> UpdateMovementIntervalAsync(MovementInterval interval)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    UPDATE интервалы_движения SET
                        время_начала = @StartTime,
                        время_окончания = @EndTime,
                        интервал_минуты = @IntervalMinutes,
                        тип_дня = @DayType
                    WHERE id = @Id";

                var affectedRows = await connection.ExecuteAsync(query, interval);
                return affectedRows > 0;
            }
        }
        // Удаление интервала движения
        public async Task<bool> DeleteMovementIntervalAsync(int intervalId)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "DELETE FROM интервалы_движения WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { Id = intervalId });
                return affectedRows > 0;
            }
        }
    public RouteRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Route>> GetAllAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        m.id as Id,
                        m.route_number as RouteNumber,
                        m.turnover_time as TurnoverTime,
                        g.first_departure_time as FirstDepartureTime,
                        g.last_departure_time as LastDepartureTime,
                        g.intervals as Interval,
                        (SELECT COUNT(*) FROM участок_маршрута WHERE route_id = m.id) as StopsCount,
                        (SELECT MAX(distance_from_start) FROM участок_маршрута WHERE route_id = m.id) as TotalLength
                    FROM маршрут m
                    LEFT JOIN график_движения g ON m.id = g.route_id
                    ORDER BY m.route_number";

                return await connection.QueryAsync<Route>(query);
            }
        }

        public async Task<Route> GetByIdAsync(int id)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        m.id as Id,
                        m.route_number as RouteNumber,
                        m.turnover_time as TurnoverTime,
                        g.first_departure_time as FirstDepartureTime,
                        g.last_departure_time as LastDepartureTime,
                        g.intervals as Interval
                    FROM маршрут m
                    LEFT JOIN график_движения g ON m.id = g.route_id
                    WHERE m.id = @Id";

                return await connection.QueryFirstOrDefaultAsync<Route>(query, new { Id = id });
            }
        }

        public async Task<IEnumerable<Route>> GetByRouteNumberAsync(int routeNumber)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        m.id as Id,
                        m.route_number as RouteNumber,
                        m.turnover_time as TurnoverTime,
                        g.first_departure_time as FirstDepartureTime,
                        g.last_departure_time as LastDepartureTime,
                        g.intervals as Interval
                    FROM маршрут m
                    LEFT JOIN график_движения g ON m.id = g.route_id
                    WHERE m.route_number = @RouteNumber
                    ORDER BY m.id";

                return await connection.QueryAsync<Route>(query, new { RouteNumber = routeNumber });
            }
        }

        public async Task<IEnumerable<RouteSection>> GetRouteStopsAsync(int routeId) 
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
            SELECT 
                um.id as Id,
                um.route_id as RouteId,
                um.stop_id as StopId,
                um.[order] as [Order],
                um.distance_from_start as DistanceFromStart,
                o.name as StopName
            FROM участок_маршрута um
            JOIN остановка o ON um.stop_id = o.id
            WHERE um.route_id = @RouteId
            ORDER BY um.[order]";

                return await connection.QueryAsync<RouteSection>(query, new { RouteId = routeId }); 
            }
        }

        public async Task<int> AddAsync(Route route)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    INSERT INTO маршрут (route_number, turnover_time) 
                    VALUES (@RouteNumber, @TurnoverTime);
                    SELECT last_insert_rowid();";

                return await connection.ExecuteScalarAsync<int>(query, route);
            }
        }

        public async Task<bool> UpdateAsync(Route route)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    UPDATE маршрут SET
                        route_number = @RouteNumber,
                        turnover_time = @TurnoverTime
                    WHERE id = @Id";

                var affectedRows = await connection.ExecuteAsync(query, route);
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "DELETE FROM маршрут WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }

        public async Task<IEnumerable<Route>> SearchAsync(string searchTerm)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        m.id as Id,
                        m.route_number as RouteNumber,
                        m.turnover_time as TurnoverTime,
                        g.first_departure_time as FirstDepartureTime,
                        g.last_departure_time as LastDepartureTime,
                        g.intervals as Interval
                    FROM маршрут m
                    LEFT JOIN график_движения g ON m.id = g.route_id
                    WHERE CAST(m.route_number as TEXT) LIKE @SearchTerm
                    ORDER BY m.route_number";

                return await connection.QueryAsync<Route>(query,
                    new { SearchTerm = $"%{searchTerm}%" });
            }
        }

        public async Task<int> GetBusesCountOnRouteAsync(int routeId)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT COUNT(DISTINCT автобус_id) 
                    FROM рейс 
                    WHERE маршрут_id = @RouteId";

                return await connection.ExecuteScalarAsync<int>(query, new { RouteId = routeId });
            }
        }

        public async Task<decimal> GetRouteLengthAsync(int routeId)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT MAX(distance_from_start) 
                    FROM участок_маршрута 
                    WHERE route_id = @RouteId";

                return await connection.ExecuteScalarAsync<decimal>(query, new { RouteId = routeId });
            }
        }
    }
}