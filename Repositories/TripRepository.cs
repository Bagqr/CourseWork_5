using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusParkManagementSystem.Data;
using BusParkManagementSystem.Models;
using Dapper;

namespace BusParkManagementSystem.Repositories
{
    public class TripRepository : ITripRepository
    {
        private readonly DatabaseContext _dbContext;

        public TripRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Trip>> GetAllAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
            SELECT 
                р.id as Id,
                р.маршрут_id as RouteId,
                р.автобус_id as BusId,
                р.водитель_id as DriverId,
                р.кондуктор_id as ConductorId,
                р.дата_рейса as TripDate,
                р.тип_смены_id as ShiftTypeId,
                р.плановая_выручка as PlannedRevenue,
                р.статус as Status,
                р.причина_снятия as CancellationReason,
                м.route_number as RouteNumber,
                а.gov_plate as BusGovPlate,
                м2.model_name as BusModel,
                в.full_name as DriverName,
                к.full_name as ConductorName,
                тс.shift_name as ShiftName,
                COALESCE(вр.фактическая_выручка, 0) as ActualRevenue,  -- Изменено здесь
                COALESCE(вр.продано_билетов, 0) as TicketsSold          -- Изменено здесь
            FROM рейс р
            LEFT JOIN маршрут м ON р.маршрут_id = м.id
            LEFT JOIN автобус а ON р.автобус_id = а.id
            LEFT JOIN модель м2 ON а.model_id = м2.id
            LEFT JOIN сотрудник в ON р.водитель_id = в.id
            LEFT JOIN сотрудник к ON р.кондуктор_id = к.id
            LEFT JOIN тип_смены тс ON р.тип_смены_id = тс.id
            LEFT JOIN выручка_рейса вр ON р.id = вр.рейс_id  -- Изменено здесь
            ORDER BY р.дата_рейса DESC, р.id";

                return await connection.QueryAsync<Trip>(query);
            }
        }

        public async Task<Trip> GetByIdAsync(int id)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
            SELECT 
                р.id as Id,
                р.маршрут_id as RouteId,
                р.автобус_id as BusId,
                р.водитель_id as DriverId,
                р.кондуктор_id as ConductorId,
                р.дата_рейса as TripDate,
                р.тип_смены_id as ShiftTypeId,
                р.плановая_выручка as PlannedRevenue,
                р.статус as Status,
                р.причина_снятия as CancellationReason,
                м.route_number as RouteNumber,
                а.gov_plate as BusGovPlate,
                м2.model_name as BusModel,
                в.full_name as DriverName,
                к.full_name as ConductorName,
                тс.shift_name as ShiftName,
                COALESCE(вр.фактическая_выручка, 0) as ActualRevenue,
                COALESCE(вр.продано_билетов, 0) as TicketsSold
            FROM рейс р
            LEFT JOIN маршрут м ON р.маршрут_id = м.id
            LEFT JOIN автобус а ON р.автобус_id = а.id
            LEFT JOIN модель м2 ON а.model_id = м2.id
            LEFT JOIN сотрудник в ON р.водитель_id = в.id
            LEFT JOIN сотрудник к ON р.кондуктор_id = к.id
            LEFT JOIN тип_смены тс ON р.тип_смены_id = тс.id
            LEFT JOIN выручка_рейса вр ON р.id = вр.рейс_id
            WHERE р.id = @Id";

                return await connection.QueryFirstOrDefaultAsync<Trip>(query, new { Id = id });
            }
        }

        public async Task<IEnumerable<Trip>> GetByDateAsync(DateTime date)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        р.id as Id,
                        р.маршрут_id as RouteId,
                        р.автобус_id as BusId,
                        р.водитель_id as DriverId,
                        р.кондуктор_id as ConductorId,
                        р.дата_рейса as TripDate,
                        р.тип_смены_id as ShiftTypeId,
                        р.плановая_выручка as PlannedRevenue,
                        р.статус as Status,
                        р.причина_снятия as CancellationReason,
                        м.route_number as RouteNumber,
                        а.gov_plate as BusGovPlate,
                        в.full_name as DriverName,
                        к.full_name as ConductorName,
                        тс.shift_name as ShiftName
                    FROM рейс р
                    LEFT JOIN маршрут м ON р.маршрут_id = м.id
                    LEFT JOIN автобус а ON р.автобус_id = а.id
                    LEFT JOIN сотрудник в ON р.водитель_id = в.id
                    LEFT JOIN сотрудник к ON р.кондуктор_id = к.id
                    LEFT JOIN тип_смены тс ON р.тип_смены_id = тс.id
                    WHERE DATE(р.дата_рейса) = DATE(@Date)
                    ORDER BY р.id";

                return await connection.QueryAsync<Trip>(query, new { Date = date.ToString("yyyy-MM-dd") });
            }
        }

        public async Task<IEnumerable<Trip>> GetByRouteAsync(int routeId)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        р.id as Id,
                        р.маршрут_id as RouteId,
                        р.автобус_id as BusId,
                        р.водитель_id as DriverId,
                        р.кондуктор_id as ConductorId,
                        р.дата_рейса as TripDate,
                        р.тип_смены_id as ShiftTypeId,
                        р.плановая_выручка as PlannedRevenue,
                        р.статус as Status,
                        р.причина_снятия as CancellationReason,
                        м.route_number as RouteNumber,
                        а.gov_plate as BusGovPlate,
                        в.full_name as DriverName,
                        к.full_name as ConductorName,
                        тс.shift_name as ShiftName
                    FROM рейс р
                    LEFT JOIN маршрут м ON р.маршрут_id = м.id
                    LEFT JOIN автобус а ON р.автобус_id = а.id
                    LEFT JOIN сотрудник в ON р.водитель_id = в.id
                    LEFT JOIN сотрудник к ON р.кондуктор_id = к.id
                    LEFT JOIN тип_смены тс ON р.тип_смены_id = тс.id
                    WHERE р.маршрут_id = @RouteId
                    ORDER BY р.дата_рейса DESC";

                return await connection.QueryAsync<Trip>(query, new { RouteId = routeId });
            }
        }

        public async Task<IEnumerable<Trip>> GetByStatusAsync(string status)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        р.id as Id,
                        р.маршрут_id as RouteId,
                        р.автобус_id as BusId,
                        р.водитель_id as DriverId,
                        р.кондуктор_id as ConductorId,
                        р.дата_рейса as TripDate,
                        р.тип_смены_id as ShiftTypeId,
                        р.плановая_выручка as PlannedRevenue,
                        р.статус as Status,
                        р.причина_снятия as CancellationReason,
                        м.route_number as RouteNumber,
                        а.gov_plate as BusGovPlate,
                        в.full_name as DriverName,
                        к.full_name as ConductorName,
                        тс.shift_name as ShiftName
                    FROM рейс р
                    LEFT JOIN маршрут м ON р.маршрут_id = м.id
                    LEFT JOIN автобус а ON р.автобус_id = а.id
                    LEFT JOIN сотрудник в ON р.водитель_id = в.id
                    LEFT JOIN сотрудник к ON р.кондуктор_id = к.id
                    LEFT JOIN тип_смены тс ON р.тип_смены_id = тс.id
                    WHERE р.статус = @Status
                    ORDER BY р.дата_рейса DESC";

                return await connection.QueryAsync<Trip>(query, new { Status = status });
            }
        }

        public async Task<IEnumerable<Trip>> GetByDriverAsync(int driverId)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        р.id as Id,
                        р.маршрут_id as RouteId,
                        р.автобус_id as BusId,
                        р.водитель_id as DriverId,
                        р.кондуктор_id as ConductorId,
                        р.дата_рейса as TripDate,
                        р.тип_смены_id as ShiftTypeId,
                        р.плановая_выручка as PlannedRevenue,
                        р.статус as Status,
                        р.причина_снятия as CancellationReason,
                        м.route_number as RouteNumber,
                        а.gov_plate as BusGovPlate,
                        в.full_name as DriverName,
                        к.full_name as ConductorName,
                        тс.shift_name as ShiftName
                    FROM рейс р
                    LEFT JOIN маршрут м ON р.маршрут_id = м.id
                    LEFT JOIN автобус а ON р.автобус_id = а.id
                    LEFT JOIN сотрудник в ON р.водитель_id = в.id
                    LEFT JOIN сотрудник к ON р.кондуктор_id = к.id
                    LEFT JOIN тип_смены тс ON р.тип_смены_id = тс.id
                    WHERE р.водитель_id = @DriverId
                    ORDER BY р.дата_рейса DESC";

                return await connection.QueryAsync<Trip>(query, new { DriverId = driverId });
            }
        }

        public async Task<int> AddAsync(Trip trip)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    INSERT INTO рейс (
                        маршрут_id, автобус_id, водитель_id, кондуктор_id,
                        дата_рейса, тип_смены_id, плановая_выручка, статус
                    ) VALUES (
                        @RouteId, @BusId, @DriverId, @ConductorId,
                        @TripDate, @ShiftTypeId, @PlannedRevenue, @Status
                    );
                    SELECT last_insert_rowid();";

                return await connection.ExecuteScalarAsync<int>(query, trip);
            }
        }

        public async Task<bool> UpdateAsync(Trip trip)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    UPDATE рейс SET
                        маршрут_id = @RouteId,
                        автобус_id = @BusId,
                        водитель_id = @DriverId,
                        кондуктор_id = @ConductorId,
                        дата_рейса = @TripDate,
                        тип_смены_id = @ShiftTypeId,
                        плановая_выручка = @PlannedRevenue,
                        статус = @Status,
                        причина_снятия = @CancellationReason
                    WHERE id = @Id";

                var affectedRows = await connection.ExecuteAsync(query, trip);
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "DELETE FROM рейс WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }

        public async Task<bool> CancelAsync(int id, string reason)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    UPDATE рейс SET
                        статус = 'отменен',
                        причина_снятия = @Reason
                    WHERE id = @Id AND статус IN ('запланирован', 'в_пути')";

                var affectedRows = await connection.ExecuteAsync(query,
                    new { Id = id, Reason = reason });
                return affectedRows > 0;
            }
        }

        public async Task<bool> CompleteAsync(int id, decimal actualRevenue, int ticketsSold)
        {
            using (var connection = _dbContext.GetConnection())
            {
                // Обновляем статус рейса
                var updateTripQuery = @"
                    UPDATE рейс SET
                        статус = 'завершен'
                    WHERE id = @Id";

                await connection.ExecuteAsync(updateTripQuery, new { Id = id });

                // Добавляем запись о выручке
                var insertRevenueQuery = @"
                    INSERT INTO выручка_рейса (рейс_id, фактическая_выручка, продано_билетов)
                    VALUES (@Id, @ActualRevenue, @TicketsSold)";

                var affectedRows = await connection.ExecuteAsync(insertRevenueQuery,
                    new { Id = id, ActualRevenue = actualRevenue, TicketsSold = ticketsSold });

                return affectedRows > 0;
            }
        }

        public async Task<IEnumerable<Trip>> SearchAsync(string searchTerm)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        р.id as Id,
                        р.маршрут_id as RouteId,
                        р.автобус_id as BusId,
                        р.водитель_id as DriverId,
                        р.кондуктор_id as ConductorId,
                        р.дата_рейса as TripDate,
                        р.тип_смены_id as ShiftTypeId,
                        р.плановая_выручка as PlannedRevenue,
                        р.статус as Status,
                        р.причина_снятия as CancellationReason,
                        м.route_number as RouteNumber,
                        а.gov_plate as BusGovPlate,
                        в.full_name as DriverName,
                        к.full_name as ConductorName,
                        тс.shift_name as ShiftName
                    FROM рейс р
                    LEFT JOIN маршрут м ON р.маршрут_id = м.id
                    LEFT JOIN автобус а ON р.автобус_id = а.id
                    LEFT JOIN сотрудник в ON р.водитель_id = в.id
                    LEFT JOIN сотрудник к ON р.кондуктор_id = к.id
                    LEFT JOIN тип_смены тс ON р.тип_смены_id = тс.id
                    WHERE м.route_number LIKE @SearchTerm OR 
                          а.gov_plate LIKE @SearchTerm OR
                          в.full_name LIKE @SearchTerm OR
                          к.full_name LIKE @SearchTerm
                    ORDER BY р.дата_рейса DESC";

                return await connection.QueryAsync<Trip>(query,
                    new { SearchTerm = $"%{searchTerm}%" });
            }
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT COALESCE(SUM(фактическая_выручка), 0)
                    FROM выручка_рейса в
                    JOIN рейс р ON в.рейс_id = р.id
                    WHERE DATE(р.дата_рейса) BETWEEN DATE(@StartDate) AND DATE(@EndDate)";

                return await connection.ExecuteScalarAsync<decimal>(query,
                    new
                    {
                        StartDate = startDate.ToString("yyyy-MM-dd"),
                        EndDate = endDate.ToString("yyyy-MM-dd")
                    });
            }
        }
    }
}