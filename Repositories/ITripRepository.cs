using BusParkManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusParkManagementSystem.Repositories
{
    public interface ITripRepository
    {
        Task<IEnumerable<Trip>> GetAllAsync();
        Task<Trip> GetByIdAsync(int id);
        Task<IEnumerable<Trip>> GetByDateAsync(DateTime date);
        Task<IEnumerable<Trip>> GetByRouteAsync(int routeId);
        Task<IEnumerable<Trip>> GetByStatusAsync(string status);
        Task<IEnumerable<Trip>> GetByDriverAsync(int driverId);
        Task<int> AddAsync(Trip trip);
        Task<bool> UpdateAsync(Trip trip);
        Task<bool> DeleteAsync(int id);
        Task<bool> CancelAsync(int id, string reason);
        Task<bool> CompleteAsync(int id, decimal actualRevenue, int ticketsSold);
        Task<IEnumerable<Trip>> SearchAsync(string searchTerm);
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
    }
}