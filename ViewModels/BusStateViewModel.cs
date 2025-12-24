using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusParkManagementSystem.Models;
using BusParkManagementSystem.Repositories;

namespace BusParkManagementSystem.ViewModels
{
    public class BusStateViewModel : BaseLookupViewModel<BusState>
    {
        public BusStateViewModel(ILookupRepository lookupRepository)
            : base(lookupRepository, "Справочник состояний автобусов", "Состояния")
        {
        }

        protected override async Task<IEnumerable<BusState>> LoadDataFromRepositoryAsync()
        {
            return await _lookupRepository.GetBusStatesAsync();
        }

        protected override async Task<int> AddItemToRepositoryAsync(BusState item)
        {
            return await _lookupRepository.AddBusStateAsync(item);
        }

        protected override async Task<bool> UpdateItemInRepositoryAsync(BusState item)
        {
            return await _lookupRepository.UpdateBusStateAsync(item);
        }

        protected override async Task<bool> DeleteItemFromRepositoryAsync(int id)
        {
            return await _lookupRepository.DeleteBusStateAsync(id);
        }

        protected override bool ItemMatchesSearch(BusState item, string searchTerm)
        {
            return item.StateName?.ToLower().Contains(searchTerm) ?? false;
        }
    }
}