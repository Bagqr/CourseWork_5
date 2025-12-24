using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using BusParkManagementSystem.Data;
using BusParkManagementSystem.Repositories;
using System;

namespace BusParkManagementSystem.Models
{
    // === ОСНОВНЫЕ МОДЕЛИ СПРАВОЧНИКОВ ===

    // 1. Модель автобуса
    public class Model
    {
        public int Id { get; set; }
        public string ModelName { get; set; }
        public override string ToString() => ModelName;
    }

    // 2. Состояние автобуса
    public class BusState
    {
        public int Id { get; set; }
        public string StateName { get; set; }
        public override string ToString() => StateName;
    }

    // 3. Цвет автобуса
    public class Color
    {
        public int Id { get; set; }
        public string ColorName { get; set; }
        public override string ToString() => ColorName;
    }

    // 4. Должность сотрудника
    public class Position
    {
        public int Id { get; set; }
        public string PositionName { get; set; }
        public decimal? BaseSalary { get; set; }
        public decimal? BonusPercent { get; set; }
        public override string ToString() => PositionName;
    }

    // 5. Улица для адреса
    public class Street
    {
        public int Id { get; set; }
        public string StreetName { get; set; }
        public override string ToString() => StreetName;
    }

    // 6. Вид кадрового мероприятия
    public class PersonnelEventType
    {
        public int Id { get; set; }
        public string EventName { get; set; }
        public override string ToString() => EventName;
    }

    // 7. Тип смены
    public class ShiftType
    {
        public int Id { get; set; }
        public string ShiftName { get; set; }
        public override string ToString() => ShiftName;
    }

    // 8. Остановка маршрута
    public class BusStop
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public override string ToString() => Name;
    }

    // 9. Интервал движения (для графика)
    public class MovementInterval
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int IntervalMinutes { get; set; }
        public string DayType { get; set; } // 'будни', 'выходные', 'праздничные'
    }

    // === ИНТЕРФЕЙС И РЕПОЗИТОРИЙ ДЛЯ СПРАВОЧНИКОВ ===

    public interface ILookupRepository
    {
        // Методы получения данных (существующие)
        Task<IEnumerable<Model>> GetModelsAsync();
        Task<IEnumerable<BusState>> GetBusStatesAsync();
        Task<IEnumerable<Color>> GetColorsAsync();
        Task<IEnumerable<Employee>> GetActiveDriversAsync();
        Task<IEnumerable<Position>> GetPositionsAsync();
        Task<IEnumerable<Street>> GetStreetsAsync();
        Task<IEnumerable<PersonnelEventType>> GetPersonnelEventTypesAsync();
        Task<IEnumerable<BusStop>> GetStopsAsync();
        Task<IEnumerable<ShiftType>> GetShiftTypesAsync();
        Task<IEnumerable<MovementInterval>> GetDayTypesAsync();

        // Методы для рейсов
        Task<IEnumerable<Route>> GetRoutesAsync();
        Task<IEnumerable<Bus>> GetAvailableBusesAsync();
        Task<IEnumerable<Employee>> GetAvailableDriversAsync();
        Task<IEnumerable<Employee>> GetAvailableConductorsAsync();
        Task<IEnumerable<Employee>> GetDriversByShiftAsync(string shiftType);

        // НОВЫЕ CRUD-методы для справочников
        Task<int> AddModelAsync(Model model);
        Task<bool> UpdateModelAsync(Model model);
        Task<bool> DeleteModelAsync(int id);

        Task<int> AddColorAsync(Color color);
        Task<bool> UpdateColorAsync(Color color);
        Task<bool> DeleteColorAsync(int id);

        Task<int> AddBusStateAsync(BusState state);
        Task<bool> UpdateBusStateAsync(BusState state);
        Task<bool> DeleteBusStateAsync(int id);

        Task<int> AddPositionAsync(Position position);
        Task<bool> UpdatePositionAsync(Position position);
        Task<bool> DeletePositionAsync(int id);

        Task<int> AddStreetAsync(Street street);
        Task<bool> UpdateStreetAsync(Street street);
        Task<bool> DeleteStreetAsync(int id);

        Task<int> AddBusStopAsync(BusStop stop);
        Task<bool> UpdateBusStopAsync(BusStop stop);
        Task<bool> DeleteBusStopAsync(int id);

        Task<int> AddShiftTypeAsync(ShiftType shiftType);
        Task<bool> UpdateShiftTypeAsync(ShiftType shiftType);
        Task<bool> DeleteShiftTypeAsync(int id);

        Task<int> AddPersonnelEventTypeAsync(PersonnelEventType eventType);
        Task<bool> UpdatePersonnelEventTypeAsync(PersonnelEventType eventType);
        Task<bool> DeletePersonnelEventTypeAsync(int id);

    }

    // === РЕПОЗИТОРИЙ СПРАВОЧНИКОВ (расширенный) ===
    public class LookupRepository : ILookupRepository
    {
        private readonly DatabaseContext _dbContext;

        public LookupRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        // === СУЩЕСТВУЮЩИЕ МЕТОДЫ ПОЛУЧЕНИЯ ДАННЫХ ===

        public async Task<IEnumerable<Model>> GetModelsAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                return await connection.QueryAsync<Model>(
                    "SELECT id, model_name as ModelName FROM модель ORDER BY model_name");
            }
        }

        public async Task<IEnumerable<BusState>> GetBusStatesAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                return await connection.QueryAsync<BusState>(
                    "SELECT id, state_name as StateName FROM состояние_автобуса ORDER BY state_name");
            }
        }

        public async Task<IEnumerable<Color>> GetColorsAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                return await connection.QueryAsync<Color>(
                    "SELECT id, color_name as ColorName FROM цвет ORDER BY color_name");
            }
        }

        public async Task<IEnumerable<Employee>> GetActiveDriversAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT DISTINCT
                        с.id as Id,
                        с.full_name as FullName,
                        с.gender as Gender,
                        с.birth_date as BirthDate,
                        с.street_id as StreetId,
                        с.position_id as PositionId,
                        с.salary as Salary,
                        с.house as House,
                        с.активен as IsActive,
                        с.дата_увольнения as DismissalDate
                    FROM сотрудник с
                    LEFT JOIN должность д ON с.position_id = д.id
                    WHERE с.активен = 1 
                      AND д.position_name = 'Водитель'
                    ORDER BY с.full_name";

                return await connection.QueryAsync<Employee>(query);
            }
        }

        public async Task<IEnumerable<Position>> GetPositionsAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        id as Id,
                        position_name as PositionName,
                        базовый_оклад as BaseSalary,
                        процент_премии as BonusPercent
                    FROM должность 
                    ORDER BY position_name";

                return await connection.QueryAsync<Position>(query);
            }
        }

        public async Task<IEnumerable<Street>> GetStreetsAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        id as Id,
                        street_name as StreetName
                    FROM улица 
                    ORDER BY street_name";

                return await connection.QueryAsync<Street>(query);
            }
        }

        public async Task<IEnumerable<PersonnelEventType>> GetPersonnelEventTypesAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        id as Id,
                        personnel_event_name as EventName
                    FROM вид_кадрового_мероприятия 
                    ORDER BY personnel_event_name";

                return await connection.QueryAsync<PersonnelEventType>(query);
            }
        }

        public async Task<IEnumerable<BusStop>> GetStopsAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        id as Id,
                        name as Name
                    FROM остановка 
                    ORDER BY name";

                return await connection.QueryAsync<BusStop>(query);
            }
        }

        public async Task<IEnumerable<ShiftType>> GetShiftTypesAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        id as Id,
                        shift_name as ShiftName
                    FROM тип_смены 
                    ORDER BY shift_name";

                return await connection.QueryAsync<ShiftType>(query);
            }
        }

        public async Task<IEnumerable<MovementInterval>> GetDayTypesAsync()
        {
            return await Task.FromResult(new List<MovementInterval>
            {
                new MovementInterval { DayType = "будни" },
                new MovementInterval { DayType = "выходные" },
                new MovementInterval { DayType = "праздничные" }
            });
        }

        // Методы для рейсов
        public async Task<IEnumerable<Route>> GetRoutesAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        id as Id,
                        route_number as RouteNumber
                    FROM маршрут 
                    ORDER BY route_number";

                return await connection.QueryAsync<Route>(query);
            }
        }

        public async Task<IEnumerable<Bus>> GetAvailableBusesAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT DISTINCT
                        а.id as Id,
                        а.inventory_number as InventoryNumber,
                        а.gov_plate as GovPlate,
                        а.model_id as ModelId,
                        а.state_id as StateId,
                        а.color_id as ColorId
                    FROM автобус а
                    LEFT JOIN состояние_автобуса с ON а.state_id = с.id
                    WHERE с.state_name = 'ИСПРАВЕН'
                    ORDER BY а.gov_plate";

                return await connection.QueryAsync<Bus>(query);
            }
        }

        public async Task<IEnumerable<Employee>> GetAvailableDriversAsync()
        {
            return await GetActiveDriversAsync();
        }

        public async Task<IEnumerable<Employee>> GetAvailableConductorsAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT DISTINCT
                        с.id as Id,
                        с.full_name as FullName,
                        с.gender as Gender,
                        с.birth_date as BirthDate,
                        с.street_id as StreetId,
                        с.position_id as PositionId,
                        с.salary as Salary,
                        с.house as House,
                        с.активен as IsActive,
                        с.дата_увольнения as DismissalDate
                    FROM сотрудник с
                    LEFT JOIN должность д ON с.position_id = д.id
                    WHERE с.активен = 1 
                      AND д.position_name = 'Кондуктор'
                    ORDER BY с.full_name";

                return await connection.QueryAsync<Employee>(query);
            }
        }

        public async Task<IEnumerable<Employee>> GetDriversByShiftAsync(string shiftType)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT DISTINCT
                        с.id as Id,
                        с.full_name as FullName,
                        с.gender as Gender,
                        с.birth_date as BirthDate,
                        с.street_id as StreetId,
                        с.position_id as PositionId,
                        с.salary as Salary,
                        с.house as House,
                        с.активен as IsActive,
                        с.дата_увольнения as DismissalDate
                    FROM сотрудник с
                    JOIN должность д ON с.position_id = д.id
                    JOIN рейс р ON с.id = р.водитель_id
                    JOIN тип_смены т ON р.тип_смены_id = т.id
                    WHERE с.активен = 1 
                      AND д.position_name = 'Водитель'
                      AND т.shift_name = @ShiftType
                    ORDER BY с.full_name";

                return await connection.QueryAsync<Employee>(query, new { ShiftType = shiftType });
            }
        }

        // === НОВЫЕ CRUD-МЕТОДЫ ДЛЯ СПРАВОЧНИКОВ ===

        // Методы для моделей автобусов
        public async Task<int> AddModelAsync(Model model)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "INSERT INTO модель (model_name) VALUES (@ModelName); SELECT last_insert_rowid();";
                return await connection.ExecuteScalarAsync<int>(query, new { model.ModelName });
            }
        }

        public async Task<bool> UpdateModelAsync(Model model)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "UPDATE модель SET model_name = @ModelName WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    model.Id,
                    model.ModelName
                });

                // Для отладки
                Console.WriteLine($"UpdateModelAsync: ID={model.Id}, Name={model.ModelName}, AffectedRows={affectedRows}");

                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteModelAsync(int id)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "DELETE FROM модель WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }

        // Методы для цветов
        public async Task<int> AddColorAsync(Color color)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "INSERT INTO цвет (color_name) VALUES (@ColorName); SELECT last_insert_rowid();";
                return await connection.ExecuteScalarAsync<int>(query, new { color.ColorName });
            }
        }

        public async Task<bool> UpdateColorAsync(Color color)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "UPDATE цвет SET color_name = @ColorName WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { color.Id, color.ColorName });
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteColorAsync(int id)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "DELETE FROM цвет WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }

        // Методы для состояний автобусов
        public async Task<int> AddBusStateAsync(BusState state)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "INSERT INTO состояние_автобуса (state_name) VALUES (@StateName); SELECT last_insert_rowid();";
                return await connection.ExecuteScalarAsync<int>(query, new { state.StateName });
            }
        }

        public async Task<bool> UpdateBusStateAsync(BusState state)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "UPDATE состояние_автобуса SET state_name = @StateName WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { state.Id, state.StateName });
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteBusStateAsync(int id)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "DELETE FROM состояние_автобуса WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }

        // Методы для должностей
        public async Task<int> AddPositionAsync(Position position)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"INSERT INTO должность (position_name, базовый_оклад, процент_премии) 
                             VALUES (@PositionName, @BaseSalary, @BonusPercent); 
                             SELECT last_insert_rowid();";
                return await connection.ExecuteScalarAsync<int>(query, position);
            }
        }

        public async Task<bool> UpdatePositionAsync(Position position)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"UPDATE должность SET 
                     position_name = @PositionName,
                     базовый_оклад = @BaseSalary,
                     процент_премии = @BonusPercent
                     WHERE id = @Id";

                var affectedRows = await connection.ExecuteAsync(query, new
                {
                    position.Id,
                    position.PositionName,
                    position.BaseSalary,
                    position.BonusPercent
                });

                return affectedRows > 0;
            }
        }

        public async Task<bool> DeletePositionAsync(int id)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "DELETE FROM должность WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }

        // Методы для улиц
        public async Task<int> AddStreetAsync(Street street)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "INSERT INTO улица (street_name) VALUES (@StreetName); SELECT last_insert_rowid();";
                return await connection.ExecuteScalarAsync<int>(query, new { street.StreetName });
            }
        }

        public async Task<bool> UpdateStreetAsync(Street street)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "UPDATE улица SET street_name = @StreetName WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { street.Id, street.StreetName });
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteStreetAsync(int id)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "DELETE FROM улица WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }

        // Методы для остановок
        public async Task<int> AddBusStopAsync(BusStop stop)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "INSERT INTO остановка (name) VALUES (@Name); SELECT last_insert_rowid();";
                return await connection.ExecuteScalarAsync<int>(query, new { stop.Name });
            }
        }

        public async Task<bool> UpdateBusStopAsync(BusStop stop)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "UPDATE остановка SET name = @Name WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { stop.Id, stop.Name });
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteBusStopAsync(int id)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "DELETE FROM остановка WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }

        // Методы для типов смен
        public async Task<int> AddShiftTypeAsync(ShiftType shiftType)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "INSERT INTO тип_смены (shift_name) VALUES (@ShiftName); SELECT last_insert_rowid();";
                return await connection.ExecuteScalarAsync<int>(query, new { shiftType.ShiftName });
            }
        }

        public async Task<bool> UpdateShiftTypeAsync(ShiftType shiftType)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "UPDATE тип_смены SET shift_name = @ShiftName WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { shiftType.Id, shiftType.ShiftName });
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteShiftTypeAsync(int id)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "DELETE FROM тип_смены WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }

        // Методы для видов кадровых мероприятий
        public async Task<int> AddPersonnelEventTypeAsync(PersonnelEventType eventType)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "INSERT INTO вид_кадрового_мероприятия (personnel_event_name) VALUES (@EventName); SELECT last_insert_rowid();";
                return await connection.ExecuteScalarAsync<int>(query, new { eventType.EventName });
            }
        }

        public async Task<bool> UpdatePersonnelEventTypeAsync(PersonnelEventType eventType)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "UPDATE вид_кадрового_мероприятия SET personnel_event_name = @EventName WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { eventType.Id, eventType.EventName });
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeletePersonnelEventTypeAsync(int id)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "DELETE FROM вид_кадрового_мероприятия WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }
    }
}