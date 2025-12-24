using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using BusParkManagementSystem.Data;
using BusParkManagementSystem.Models;
using Dapper;

namespace BusParkManagementSystem.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly DatabaseContext _dbContext;

        public EmployeeRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
            SELECT 
                s.id as Id,
                s.full_name as FullName,
                s.gender as Gender,
                s.birth_date as BirthDate,
                s.street_id as StreetId,
                s.position_id as PositionId,
                s.salary as Salary,
                s.house as House,
                s.активен as IsActive,
                s.дата_увольнения as DismissalDate,
                d.position_name as PositionName,
                u.street_name as StreetName
                -- experience_years не запрашиваем, так как его нет в таблице
            FROM сотрудник s
            LEFT JOIN должность d ON s.position_id = d.id
            LEFT JOIN улица u ON s.street_id = u.id
            ORDER BY s.full_name";

                return await connection.QueryAsync<Employee>(query);
            }
        }

        public async Task<Employee> GetByIdAsync(int id)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        s.id as Id,
                        s.full_name as FullName,
                        s.gender as Gender,
                        s.birth_date as BirthDate,
                        s.street_id as StreetId,
                        s.position_id as PositionId,
                        s.salary as Salary,
                        s.house as House,
                        s.активен as IsActive,
                        s.дата_увольнения as DismissalDate,
                        d.position_name as PositionName,
                        u.street_name as StreetName
                    FROM сотрудник s
                    LEFT JOIN должность d ON s.position_id = d.id
                    LEFT JOIN улица u ON s.street_id = u.id
                    WHERE s.id = @Id";

                return await connection.QueryFirstOrDefaultAsync<Employee>(query, new { Id = id });
            }
        }

        public async Task<int> AddAsync(Employee employee)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
            INSERT INTO сотрудник (
                full_name, gender, birth_date, street_id, 
                position_id, salary, house, активен
            ) VALUES (
                @FullName, @Gender, @BirthDate, @StreetId,
                @PositionId, @Salary, @House, 1
            );
            SELECT last_insert_rowid();";

                var parameters = new
                {
                    employee.FullName,
                    employee.Gender,
                    BirthDate = employee.BirthDate.ToString("yyyy-MM-dd"),
                    employee.StreetId,
                    employee.PositionId,
                    employee.Salary,
                    employee.House
                    // Не добавляем experience_years, так как его нет в таблице
                };

                return await connection.ExecuteScalarAsync<int>(query, parameters);
            }
        }


        public async Task<bool> UpdateAsync(Employee employee)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    UPDATE сотрудник SET
                        full_name = @FullName,
                        gender = @Gender,
                        birth_date = @BirthDate,
                        street_id = @StreetId,
                        position_id = @PositionId,
                        salary = @Salary,
                        house = @House,
                        активен = @IsActive,
                        дата_увольнения = @DismissalDate
                    WHERE id = @Id";

                var parameters = new
                {
                    employee.Id,
                    employee.FullName,
                    employee.Gender,
                    BirthDate = employee.BirthDate.ToString("yyyy-MM-dd"),
                    employee.StreetId,
                    employee.PositionId,
                    employee.Salary,
                    employee.House,
                    employee.IsActive,
                    DismissalDate = employee.DismissalDate?.ToString("yyyy-MM-dd")
                };

                var affectedRows = await connection.ExecuteAsync(query, parameters);
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = "DELETE FROM сотрудник WHERE id = @Id";
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }

        public async Task<IEnumerable<Employee>> GetByPositionAsync(string position)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        s.id as Id,
                        s.full_name as FullName,
                        s.gender as Gender,
                        s.birth_date as BirthDate,
                        s.street_id as StreetId,
                        s.position_id as PositionId,
                        s.salary as Salary,
                        s.house as House,
                        s.активен as IsActive,
                        s.дата_увольнения as DismissalDate,
                        d.position_name as PositionName,
                        u.street_name as StreetName
                    FROM сотрудник s
                    LEFT JOIN должность d ON s.position_id = d.id
                    LEFT JOIN улица u ON s.street_id = u.id
                    WHERE d.position_name = @Position AND s.активен = 1
                    ORDER BY s.full_name";

                return await connection.QueryAsync<Employee>(query, new { Position = position });
            }
        }

        public async Task<IEnumerable<Employee>> GetActiveAsync()
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        s.id as Id,
                        s.full_name as FullName,
                        s.gender as Gender,
                        s.birth_date as BirthDate,
                        s.street_id as StreetId,
                        s.position_id as PositionId,
                        s.salary as Salary,
                        s.house as House,
                        s.активен as IsActive,
                        s.дата_увольнения as DismissalDate,
                        d.position_name as PositionName,
                        u.street_name as StreetName
                    FROM сотрудник s
                    LEFT JOIN должность d ON s.position_id = d.id
                    LEFT JOIN улица u ON s.street_id = u.id
                    WHERE s.активен = 1
                    ORDER BY s.full_name";

                return await connection.QueryAsync<Employee>(query);
            }
        }

        public async Task<bool> DismissAsync(int id, DateTime dismissalDate, string reason)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    UPDATE сотрудник SET
                        активен = 0,
                        дата_увольнения = @DismissalDate
                    WHERE id = @Id";

                var parameters = new
                {
                    Id = id,
                    DismissalDate = dismissalDate.ToString("yyyy-MM-dd")
                };

                var affectedRows = await connection.ExecuteAsync(query, parameters);
                return affectedRows > 0;
            }
        }

        public async Task<IEnumerable<Employee>> SearchAsync(string searchTerm)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        s.id as Id,
                        s.full_name as FullName,
                        s.gender as Gender,
                        s.birth_date as BirthDate,
                        s.street_id as StreetId,
                        s.position_id as PositionId,
                        s.salary as Salary,
                        s.house as House,
                        s.активен as IsActive,
                        s.дата_увольнения as DismissalDate,
                        d.position_name as PositionName,
                        u.street_name as StreetName
                    FROM сотрудник s
                    LEFT JOIN должность d ON s.position_id = d.id
                    LEFT JOIN улица u ON s.street_id = u.id
                    WHERE s.full_name LIKE @SearchTerm 
                       OR d.position_name LIKE @SearchTerm
                       OR u.street_name LIKE @SearchTerm
                    ORDER BY s.full_name";

                return await connection.QueryAsync<Employee>(query,
                    new { SearchTerm = $"%{searchTerm}%" });
            }
        }

        public async Task<decimal> CalculateSalaryAsync(int employeeId, int month, int year)
        {
            using (var connection = _dbContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        s.salary as baseSalary,
                        d.процент_премии as bonusPercent
                    FROM сотрудник s
                    LEFT JOIN должность d ON s.position_id = d.id
                    WHERE s.id = @EmployeeId";

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(query,
                    new { EmployeeId = employeeId });

                if (result == null) return 0;

                decimal baseSalary = result.baseSalary ?? 0;
                decimal bonusPercent = result.bonusPercent ?? 10.0m;

                return baseSalary * (1 + bonusPercent / 100);
            }
        }
    }
}