using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusParkManagementSystem.Models;
using BusParkManagementSystem.Repositories;

namespace BusParkManagementSystem.ViewModels
{
    public class ColorViewModel : BaseLookupViewModel<Color>
    {
        public ColorViewModel(ILookupRepository lookupRepository)
            : base(lookupRepository, "Справочник цветов автобусов", "Цвета")
        {
        }

        protected override async Task<IEnumerable<Color>> LoadDataFromRepositoryAsync()
        {
            return await _lookupRepository.GetColorsAsync();
        }

        protected override async Task<int> AddItemToRepositoryAsync(Color item)
        {
            if (!await IsColorNameUniqueAsync(item.ColorName))
            {
                ShowError($"Цвет с названием '{item.ColorName}' уже существует");
                return -1;
            }

            return await _lookupRepository.AddColorAsync(item);
        }

        protected override async Task<bool> UpdateItemInRepositoryAsync(Color item)
        {
            if (!await IsColorNameUniqueAsync(item.ColorName, item.Id))
            {
                ShowError($"Цвет с названием '{item.ColorName}' уже существует");
                return false;
            }

            return await _lookupRepository.UpdateColorAsync(item);
        }

        protected override async Task<bool> DeleteItemFromRepositoryAsync(int id)
        {
            return await _lookupRepository.DeleteColorAsync(id);
        }

        protected override bool ItemMatchesSearch(Color item, string searchTerm)
        {
            return item.ColorName?.ToLower().Contains(searchTerm) ?? false;
        }

        public async Task<bool> IsColorNameUniqueAsync(string colorName, int? excludeId = null)
        {
            try
            {
                var colors = await _lookupRepository.GetColorsAsync();
                return !colors.Any(c =>
                    c.ColorName.Equals(colorName, StringComparison.OrdinalIgnoreCase) &&
                    (!excludeId.HasValue || c.Id != excludeId.Value));
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка проверки уникальности: {ex.Message}");
                return false;
            }
        }
    }
}