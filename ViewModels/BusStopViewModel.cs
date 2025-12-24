using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusParkManagementSystem.Models;
using BusParkManagementSystem.Repositories;

namespace BusParkManagementSystem.ViewModels
{
    public class BusStopViewModel : BaseLookupViewModel<BusStop>
    {
        public BusStopViewModel(ILookupRepository lookupRepository)
            : base(lookupRepository, "Справочник остановок", "Остановки")
        {
        }

        protected override async Task<IEnumerable<BusStop>> LoadDataFromRepositoryAsync()
        {
            return await _lookupRepository.GetStopsAsync();
        }

        protected override async Task<int> AddItemToRepositoryAsync(BusStop item)
        {
            return await _lookupRepository.AddBusStopAsync(item);
        }

        protected override async Task<bool> UpdateItemInRepositoryAsync(BusStop item)
        {
            return await _lookupRepository.UpdateBusStopAsync(item);
        }

        protected override async Task<bool> DeleteItemFromRepositoryAsync(int id)
        {
            return await _lookupRepository.DeleteBusStopAsync(id);
        }

        protected override bool ItemMatchesSearch(BusStop item, string searchTerm)
        {
            return item.Name?.ToLower().Contains(searchTerm) ?? false;
        }
    }
}