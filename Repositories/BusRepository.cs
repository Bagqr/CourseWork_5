using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusParkManagementSystem.Data;
using BusParkManagementSystem.Models;
using Dapper;

namespace BusParkManagementSystem.Repositories
{
    public class BusRepository : IBusRepository
    {
        private readonly DatabaseContext _dbContext;

        public BusRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Bus>> GetAllAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
            SELECT 
                а.id as Id,
                а.inventory_number as InventoryNumber,
                а.gov_plate as GovPlate,
                а.model_id as ModelId,
                а.state_id as StateId,
                а.color_id as ColorId,
                а.engine_number as EngineNumber,
                а.chasis_number as ChasisNumber,
                а.body_number as BodyNumber,
                а.manufacturer_date as ManufacturerDate,
                а.mileage as Mileage,
                -- Преобразуем last_overhaul_date в строку
                CASE 
                    WHEN typeof(а.last_overhaul_date) = 'integer' 
                    THEN CAST(а.last_overhaul_date AS TEXT)
                    ELSE а.last_overhaul_date 
                END as LastOverhaulDate,
                а.текущий_водитель_id as CurrentDriverId,
                м.model_name as ModelName,
                с.state_name as StateName,
                ц.color_name as ColorName
            FROM автобус а
            LEFT JOIN модель м ON а.model_id = м.id
            LEFT JOIN состояние_автобуса с ON а.state_id = с.id
            LEFT JOIN цвет ц ON а.color_id = ц.id
            ORDER BY а.gov_plate";

                return await connection.QueryAsync<Bus>(query);
            }
        }

        public async Task<Bus> GetByIdAsync(int id)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        а.id as Id,
                        а.inventory_number as InventoryNumber,
                        а.model_id as ModelId,
                        м.model_name as ModelName,
                        а.state_id as StateId,
                        с.state_name as StateName,
                        а.gov_plate as GovPlate,
                        а.engine_number as EngineNumber,
                        а.chasis_number as ChasisNumber,
                        а.body_number as BodyNumber,
                        а.manufacturer_date as ManufacturerDate,
                        а.mileage as Mileage,
                        -- Преобразование Last_overhaul_date в DateTime
                        CASE 
                            WHEN typeof(а.last_overhaul_date) = 'integer' 
                            THEN date(
                                substr(а.last_overhaul_date, 1, 4) || '-' || 
                                substr(а.last_overhaul_date, 5, 2) || '-' || 
                                substr(а.last_overhaul_date, 7, 2)
                            )
                            ELSE date(а.last_overhaul_date) 
                        END as LastOverhaulDate,
                        а.color_id as ColorId,
                        ц.color_name as ColorName,
                        а.[текущий_водитель_id] as CurrentDriverId,
                        в.full_name as DriverName
                    FROM автобус а
                    LEFT JOIN модель м ON а.model_id = м.id
                    LEFT JOIN состояние_автобуса с ON а.state_id = с.id
                    LEFT JOIN цвет ц ON а.color_id = ц.id
                    LEFT JOIN сотрудник в ON а.[текущий_водитель_id] = в.id
                    WHERE а.id = @Id";

                return await connection.QueryFirstOrDefaultAsync<Bus>(query, new { Id = id });
            }
        }

        public async Task<IEnumerable<Bus>> GetByStateAsync(string state)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        а.id as Id,
                        а.inventory_number as InventoryNumber,
                        а.model_id as ModelId,
                        м.model_name as ModelName,
                        а.state_id as StateId,
                        с.state_name as StateName,
                        а.gov_plate as GovPlate,
                        а.engine_number as EngineNumber,
                        а.chasis_number as ChasisNumber,
                        а.body_number as BodyNumber,
                        а.manufacturer_date as ManufacturerDate,
                        а.mileage as Mileage,
                        -- Преобразование Last_overhaul_date в DateTime
                        CASE 
                            WHEN typeof(а.last_overhaul_date) = 'integer' 
                            THEN date(
                                substr(а.last_overhaul_date, 1, 4) || '-' || 
                                substr(а.last_overhaul_date, 5, 2) || '-' || 
                                substr(а.last_overhaul_date, 7, 2)
                            )
                            ELSE date(а.last_overhaul_date) 
                        END as LastOverhaulDate,
                        а.color_id as ColorId,
                        ц.color_name as ColorName,
                        а.[текущий_водитель_id] as CurrentDriverId,
                        в.full_name as DriverName
                    FROM автобус а
                    LEFT JOIN модель м ON а.model_id = м.id
                    LEFT JOIN состояние_автобуса с ON а.state_id = с.id
                    LEFT JOIN цвет ц ON а.color_id = ц.id
                    LEFT JOIN сотрудник в ON а.[текущий_водитель_id] = в.id
                    WHERE с.state_name = @State
                    ORDER BY а.inventory_number";

                return await connection.QueryAsync<Bus>(query, new { State = state });
            }
        }

        public async Task<int> AddAsync(Bus bus)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    INSERT INTO автобус (
                        inventory_number, model_id, state_id, gov_plate, 
                        engine_number, chasis_number, body_number, 
                        manufacturer_date, mileage, last_overhaul_date, 
                        color_id, [текущий_водитель_id]
                    ) VALUES (
                        @InventoryNumber, @ModelId, @StateId, @GovPlate,
                        @EngineNumber, @ChasisNumber, @BodyNumber,
                        @ManufacturerDate, @Mileage, @LastOverhaulDate,
                        @ColorId, @CurrentDriverId
                    );
                    SELECT last_insert_rowid();";

                return await connection.ExecuteScalarAsync<int>(query, bus);
            }
        }

        public async Task<bool> UpdateAsync(Bus bus)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    UPDATE автобус SET
                        inventory_number = @InventoryNumber,
                        model_id = @ModelId,
                        state_id = @StateId,
                        gov_plate = @GovPlate,
                        engine_number = @EngineNumber,
                        chasis_number = @ChasisNumber,
                        body_number = @BodyNumber,
                        manufacturer_date = @ManufacturerDate,
                        mileage = @Mileage,
                        last_overhaul_date = @LastOverhaulDate,
                        color_id = @ColorId,
                        [текущий_водитель_id] = @CurrentDriverId
                    WHERE id = @Id";

                var affectedRows = await connection.ExecuteAsync(query, bus);
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "DELETE FROM автобус WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }

        public async Task<IEnumerable<Bus>> SearchAsync(string searchTerm)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        а.id as Id,
                        а.inventory_number as InventoryNumber,
                        а.model_id as ModelId,
                        м.model_name as ModelName,
                        а.state_id as StateId,
                        с.state_name as StateName,
                        а.gov_plate as GovPlate,
                        а.engine_number as EngineNumber,
                        а.chasis_number as ChasisNumber,
                        а.body_number as BodyNumber,
                        а.manufacturer_date as ManufacturerDate,
                        а.mileage as Mileage,
                        -- Преобразование Last_overhaul_date в DateTime
                        CASE 
                            WHEN typeof(а.last_overhaul_date) = 'integer' 
                            THEN date(
                                substr(а.last_overhaul_date, 1, 4) || '-' || 
                                substr(а.last_overhaul_date, 5, 2) || '-' || 
                                substr(а.last_overhaul_date, 7, 2)
                            )
                            ELSE date(а.last_overhaul_date) 
                        END as LastOverhaulDate,
                        а.color_id as ColorId,
                        ц.color_name as ColorName,
                        а.[текущий_водитель_id] as CurrentDriverId,
                        в.full_name as DriverName
                    FROM автобус а
                    LEFT JOIN модель м ON а.model_id = м.id
                    LEFT JOIN состояние_автобуса с ON а.state_id = с.id
                    LEFT JOIN цвет ц ON а.color_id = ц.id
                    LEFT JOIN сотрудник в ON а.[текущий_водитель_id] = в.id
                    WHERE а.gov_plate LIKE @SearchTerm 
                       OR а.inventory_number LIKE @SearchTerm 
                       OR а.engine_number LIKE @SearchTerm
                       OR а.chasis_number LIKE @SearchTerm
                       OR а.body_number LIKE @SearchTerm
                       OR м.model_name LIKE @SearchTerm
                    ORDER BY а.inventory_number";

                return await connection.QueryAsync<Bus>(query,
                    new { SearchTerm = $"%{searchTerm}%" });
            }
        }

        public async Task<IEnumerable<Bus>> GetBusesByRouteAsync(int routeId)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT DISTINCT
                        а.id as Id,
                        а.inventory_number as InventoryNumber,
                        а.model_id as ModelId,
                        м.model_name as ModelName,
                        а.state_id as StateId,
                        с.state_name as StateName,
                        а.gov_plate as GovPlate,
                        а.engine_number as EngineNumber,
                        а.chasis_number as ChasisNumber,
                        а.body_number as BodyNumber,
                        а.manufacturer_date as ManufacturerDate,
                        а.mileage as Mileage,
                        -- Преобразование Last_overhaul_date в DateTime
                        CASE 
                            WHEN typeof(а.last_overhaul_date) = 'integer' 
                            THEN date(
                                substr(а.last_overhaul_date, 1, 4) || '-' || 
                                substr(а.last_overhaul_date, 5, 2) || '-' || 
                                substr(а.last_overhaul_date, 7, 2)
                            )
                            ELSE date(а.last_overhaul_date) 
                        END as LastOverhaulDate,
                        а.color_id as ColorId,
                        ц.color_name as ColorName,
                        а.[текущий_водитель_id] as CurrentDriverId,
                        в.full_name as DriverName
                    FROM автобус а
                    LEFT JOIN модель м ON а.model_id = м.id
                    LEFT JOIN состояние_автобуса с ON а.state_id = с.id
                    LEFT JOIN цвет ц ON а.color_id = ц.id
                    LEFT JOIN сотрудник в ON а.[текущий_водитель_id] = в.id
                    JOIN рейс р ON а.id = р.автобус_id
                    WHERE р.маршрут_id = @RouteId
                    ORDER BY а.inventory_number";

                return await connection.QueryAsync<Bus>(query, new { RouteId = routeId });
            }
        }

        public async Task<IEnumerable<Bus>> GetAvailableBusesAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
            SELECT 
                а.id as Id,
                а.inventory_number as InventoryNumber,
                а.model_id as ModelId,
                а.state_id as StateId,
                а.gov_plate as GovPlate,
                а.engine_number as EngineNumber,
                а.chasis_number as ChasisNumber,
                а.body_number as BodyNumber,
                а.manufacturer_date as ManufacturerDate,
                а.mileage as Mileage,
                а.last_overhaul_date as LastOverhaulDate,
                а.color_id as ColorId,
                а.текущий_водитель_id as CurrentDriverId,
                м.model_name as ModelName,
                с.state_name as StateName,
                ц.color_name as ColorName
            FROM автобус а
            LEFT JOIN модель м ON а.model_id = м.id
            LEFT JOIN состояние_автобуса с ON а.state_id = с.id
            LEFT JOIN цвет ц ON а.color_id = ц.id
            WHERE с.state_name = 'ИСПРАВЕН'
            ORDER BY а.gov_plate";

                var buses = await connection.QueryAsync<Bus>(query);
                return buses;
            }
        }

        public async Task<IEnumerable<Employee>> GetAvailableDriversAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
            SELECT 
                с.id as Id,
                с.full_name as FullName,
                с.gender as Gender,
                с.birth_date as BirthDate,
                с.street_id as StreetId,
                с.position_id as PositionId,
                с.salary as Salary,
                с.house as House,
                с.активен as IsActive,
                с.дата_увольнения as DismissalDate,
                д.position_name as PositionName,
                у.street_name as StreetName
            FROM сотрудник с
            LEFT JOIN должность д ON с.position_id = д.id
            LEFT JOIN улица у ON с.street_id = у.id
            WHERE с.активен = 1 
              AND д.position_name = 'Водитель'
            ORDER BY с.full_name";

                return await connection.QueryAsync<Employee>(query);
            }
        }

        public async Task<IEnumerable<Employee>> GetAvailableConductorsAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
            SELECT 
                с.id as Id,
                с.full_name as FullName,
                с.gender as Gender,
                с.birth_date as BirthDate,
                с.street_id as StreetId,
                с.position_id as PositionId,
                с.salary as Salary,
                с.house as House,
                с.активен as IsActive,
                с.дата_увольнения as DismissalDate,
                д.position_name as PositionName,
                у.street_name as StreetName
            FROM сотрудник с
            LEFT JOIN должность д ON с.position_id = д.id
            LEFT JOIN улица у ON с.street_id = у.id
            WHERE с.активен = 1 
              AND д.position_name = 'Кондуктор'
            ORDER BY с.full_name";

                return await connection.QueryAsync<Employee>(query);
            }
        }

        public async Task<bool> IsGovPlateUniqueAsync(string govPlate, int? excludeId = null)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "SELECT COUNT(*) FROM автобус WHERE gov_plate = @GovPlate";
                var parameters = new DynamicParameters();
                parameters.Add("@GovPlate", govPlate);

                if (excludeId.HasValue && excludeId.Value > 0)
                {
                    query += " AND id != @ExcludeId";
                    parameters.Add("@ExcludeId", excludeId.Value);
                }

                var count = await connection.ExecuteScalarAsync<int>(query, parameters);
                return count == 0;
            }
        }

        public async Task<bool> IsInventoryNumberUniqueAsync(int inventoryNumber, int? excludeId = null)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "SELECT COUNT(*) FROM автобус WHERE inventory_number = @InventoryNumber";
                var parameters = new DynamicParameters();
                parameters.Add("@InventoryNumber", inventoryNumber);

                if (excludeId.HasValue && excludeId.Value > 0)
                {
                    query += " AND id != @ExcludeId";
                    parameters.Add("@ExcludeId", excludeId.Value);
                }

                var count = await connection.ExecuteScalarAsync<int>(query, parameters);
                return count == 0;
            }
        }
    }
}