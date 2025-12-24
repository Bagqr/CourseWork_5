using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusParkManagementSystem.Models;
using BusParkManagementSystem.Repositories;

namespace BusParkManagementSystem.ViewModels
{
    public class ShiftTypeViewModel : BaseLookupViewModel<ShiftType>
    {
        public ShiftTypeViewModel(ILookupRepository lookupRepository)
            : base(lookupRepository, "Справочник типов смен", "Типы смен")
        {
        }

        protected override async Task<IEnumerable<ShiftType>> LoadDataFromRepositoryAsync()
        {
            return await _lookupRepository.GetShiftTypesAsync();
        }

        protected override async Task<int> AddItemToRepositoryAsync(ShiftType item)
        {
            return await _lookupRepository.AddShiftTypeAsync(item);
        }

        protected override async Task<bool> UpdateItemInRepositoryAsync(ShiftType item)
        {
            return await _lookupRepository.UpdateShiftTypeAsync(item);
        }

        protected override async Task<bool> DeleteItemFromRepositoryAsync(int id)
        {
            return await _lookupRepository.DeleteShiftTypeAsync(id);
        }

        protected override bool ItemMatchesSearch(ShiftType item, string searchTerm)
        {
            return item.ShiftName?.ToLower().Contains(searchTerm) ?? false;
        }
    }
}