using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BusParkManagementSystem.Models;
using BusParkManagementSystem.Repositories;
using BusParkManagementSystem.Views.Dialogs;

namespace BusParkManagementSystem.ViewModels
{
    public class PositionViewModel : BaseLookupViewModel<Position>
    {
        public PositionViewModel(ILookupRepository lookupRepository)
            : base(lookupRepository, "Справочник должностей", "Должности")
        {
        }

        protected override async Task<IEnumerable<Position>> LoadDataFromRepositoryAsync()
        {
            return await _lookupRepository.GetPositionsAsync();
        }

        protected override async Task<int> AddItemToRepositoryAsync(Position item)
        {
            // Проверка уникальности перед добавлением
            if (!await IsPositionNameUniqueAsync(item.PositionName))
            {
                ShowError($"Должность с названием '{item.PositionName}' уже существует");
                return -1;
            }

            return await _lookupRepository.AddPositionAsync(item);
        }

        protected override async Task<bool> UpdateItemInRepositoryAsync(Position item)
        {
            // Проверка уникальности перед обновлением
            if (!await IsPositionNameUniqueAsync(item.PositionName, item.Id))
            {
                ShowError($"Должность с названием '{item.PositionName}' уже существует");
                return false;
            }

            return await _lookupRepository.UpdatePositionAsync(item);
        }

        protected override async Task<bool> DeleteItemFromRepositoryAsync(int id)
        {
            return await _lookupRepository.DeletePositionAsync(id);
        }

        protected override bool ItemMatchesSearch(Position item, string searchTerm)
        {
            return item.PositionName?.ToLower().Contains(searchTerm) ?? false;
        }

        // ПЕРЕОПРЕДЕЛЯЕМ диалог для должностей
        protected override Position ShowEditDialog(string title, Position existingItem = null)
        {
            var dialog = new PositionEditDialog(existingItem);

            if (dialog.ShowDialog() == true)
            {
                // Диалог уже создал новый объект с правильным ID
                return dialog.Position;
            }

            return null;
        }

        private async Task<bool> IsPositionNameUniqueAsync(string positionName, int? excludeId = null)
        {
            try
            {
                var positions = await _lookupRepository.GetPositionsAsync();
                return !positions.Any(p =>
                    p.PositionName.Equals(positionName, StringComparison.OrdinalIgnoreCase) &&
                    (!excludeId.HasValue || p.Id != excludeId.Value));
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка проверки уникальности: {ex.Message}");
                return false;
            }
        }
    }
}