using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusParkManagementSystem.Models;
using BusParkManagementSystem.Repositories;

namespace BusParkManagementSystem.ViewModels
{
    public class ModelViewModel : BaseLookupViewModel<Model>
    {
        public ModelViewModel(ILookupRepository lookupRepository)
            : base(lookupRepository, "Справочник моделей автобусов", "Модели автобусов")
        {
        }

        protected override async Task<IEnumerable<Model>> LoadDataFromRepositoryAsync()
        {
            return await _lookupRepository.GetModelsAsync();
        }

        protected override async Task<int> AddItemToRepositoryAsync(Model item)
        {
            if (!await IsModelNameUniqueAsync(item.ModelName))
            {
                ShowError($"Модель с названием '{item.ModelName}' уже существует");
                return -1;
            }

            return await _lookupRepository.AddModelAsync(item);
        }

        protected override async Task<bool> UpdateItemInRepositoryAsync(Model item)
        {
            if (!await IsModelNameUniqueAsync(item.ModelName, item.Id))
            {
                ShowError($"Модель с названием '{item.ModelName}' уже существует");
                return false;
            }

            return await _lookupRepository.UpdateModelAsync(item);
        }

        protected override async Task<bool> DeleteItemFromRepositoryAsync(int id)
        {
            return await _lookupRepository.DeleteModelAsync(id);
        }

        protected override bool ItemMatchesSearch(Model item, string searchTerm)
        {
            return item.ModelName?.ToLower().Contains(searchTerm) ?? false;
        }

        public async Task<bool> IsModelNameUniqueAsync(string modelName, int? excludeId = null)
        {
            try
            {
                var models = await _lookupRepository.GetModelsAsync();
                return !models.Any(m =>
                    m.ModelName.Equals(modelName, StringComparison.OrdinalIgnoreCase) &&
                    (!excludeId.HasValue || m.Id != excludeId.Value));
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка проверки уникальности: {ex.Message}");
                return false;
            }
        }
    }
}