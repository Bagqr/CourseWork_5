using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusParkManagementSystem.Models;

namespace BusParkManagementSystem.Repositories
{
    public interface IEmployeeRepository
    {
        // CRUD операции
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee> GetByIdAsync(int id);
        Task<int> AddAsync(Employee employee);
        Task<bool> UpdateAsync(Employee employee);
        Task<bool> DeleteAsync(int id);

        // Дополнительные операции
        Task<IEnumerable<Employee>> GetByPositionAsync(string position);
        Task<IEnumerable<Employee>> GetActiveAsync();
        Task<bool> DismissAsync(int id, DateTime dismissalDate, string reason);
        Task<IEnumerable<Employee>> SearchAsync(string searchTerm);
        Task<decimal> CalculateSalaryAsync(int employeeId, int month, int year);
    }
}