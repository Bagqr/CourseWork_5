using System.Collections.Generic;
using System.Threading.Tasks;
using BusParkManagementSystem.Models;

namespace BusParkManagementSystem.Repositories
{
    public interface IBusRepository
    {
        Task<IEnumerable<Bus>> GetAllAsync();
        Task<Bus> GetByIdAsync(int id);
        Task<IEnumerable<Bus>> GetByStateAsync(string state);
        Task<int> AddAsync(Bus bus);
        Task<bool> UpdateAsync(Bus bus);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Bus>> SearchAsync(string searchTerm);
        Task<IEnumerable<Bus>> GetBusesByRouteAsync(int routeId);
        Task<IEnumerable<Bus>> GetAvailableBusesAsync();

        Task<bool> IsGovPlateUniqueAsync(string govPlate, int? excludeId = null);
        Task<bool> IsInventoryNumberUniqueAsync(int inventoryNumber, int? excludeId = null);
    }
}