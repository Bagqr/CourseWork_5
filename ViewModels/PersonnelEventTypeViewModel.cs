using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusParkManagementSystem.Models;
using BusParkManagementSystem.Repositories;

namespace BusParkManagementSystem.ViewModels
{
    public class PersonnelEventTypeViewModel : BaseLookupViewModel<PersonnelEventType>
    {
        public PersonnelEventTypeViewModel(ILookupRepository lookupRepository)
            : base(lookupRepository, "Справочник кадровых мероприятий", "Кадровые мероприятия")
        {
        }

        protected override async Task<IEnumerable<PersonnelEventType>> LoadDataFromRepositoryAsync()
        {
            return await _lookupRepository.GetPersonnelEventTypesAsync();
        }

        protected override async Task<int> AddItemToRepositoryAsync(PersonnelEventType item)
        {
            return await _lookupRepository.AddPersonnelEventTypeAsync(item);
        }

        protected override async Task<bool> UpdateItemInRepositoryAsync(PersonnelEventType item)
        {
            return await _lookupRepository.UpdatePersonnelEventTypeAsync(item);
        }

        protected override async Task<bool> DeleteItemFromRepositoryAsync(int id)
        {
            return await _lookupRepository.DeletePersonnelEventTypeAsync(id);
        }

        protected override bool ItemMatchesSearch(PersonnelEventType item, string searchTerm)
        {
            return item.EventName?.ToLower().Contains(searchTerm) ?? false;
        }
    }
}