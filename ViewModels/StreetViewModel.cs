using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusParkManagementSystem.Models;
using BusParkManagementSystem.Repositories;

namespace BusParkManagementSystem.ViewModels
{
    public class StreetViewModel : BaseLookupViewModel<Street>
    {
        public StreetViewModel(ILookupRepository lookupRepository)
            : base(lookupRepository, "Справочник улиц", "Улицы")
        {
        }

        protected override async Task<IEnumerable<Street>> LoadDataFromRepositoryAsync()
        {
            return await _lookupRepository.GetStreetsAsync();
        }

        protected override async Task<int> AddItemToRepositoryAsync(Street item)
        {
            return await _lookupRepository.AddStreetAsync(item);
        }

        protected override async Task<bool> UpdateItemInRepositoryAsync(Street item)
        {
            return await _lookupRepository.UpdateStreetAsync(item);
        }

        protected override async Task<bool> DeleteItemFromRepositoryAsync(int id)
        {
            return await _lookupRepository.DeleteStreetAsync(id);
        }

        protected override bool ItemMatchesSearch(Street item, string searchTerm)
        {
            return item.StreetName?.ToLower().Contains(searchTerm) ?? false;
        }
    }
}