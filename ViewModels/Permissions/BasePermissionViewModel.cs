using System;
using System.Threading.Tasks;
using System.Windows;

namespace BusParkManagementSystem.ViewModels.Permissions
{
    public abstract class BasePermissionViewModel : BaseViewModel
    {
        private bool _canRead = false;
        private bool _canWrite = false;
        private bool _canEdit = false;
        private bool _canDelete = false;

        public bool CanRead
        {
            get => _canRead;
            protected set => SetField(ref _canRead, value);
        }

        public bool CanWrite
        {
            get => _canWrite;
            protected set => SetField(ref _canWrite, value);
        }

        public bool CanEdit
        {
            get => _canEdit;
            protected set => SetField(ref _canEdit, value);
        }

        public bool CanDelete
        {
            get => _canDelete;
            protected set => SetField(ref _canDelete, value);
        }

        protected async Task<bool> CheckPermissionsAsync(string moduleCode)
        {
            try
            {
                if (!CurrentUser.IsAuthenticated)
                {
                    return false;
                }

                // Администратор имеет все права
                if (CurrentUser.User?.Role == "Администратор")
                {
                    CanRead = true;
                    CanWrite = true;
                    CanEdit = true;
                    CanDelete = true;
                    return true;
                }

                // Проверяем права для текущего модуля
                CanRead = await CurrentUser.HasPermissionAsync(moduleCode, "read");
                CanWrite = await CurrentUser.HasPermissionAsync(moduleCode, "write");
                CanEdit = await CurrentUser.HasPermissionAsync(moduleCode, "edit");
                CanDelete = await CurrentUser.HasPermissionAsync(moduleCode, "delete");

                return CanRead || CanWrite || CanEdit || CanDelete;
            }
            catch (Exception ex)
            {
                // В случае ошибки логируем и возвращаем false
                Console.WriteLine($"Ошибка проверки прав доступа: {ex.Message}");
                return false;
            }
        }

        protected void SetPermissionButtonsVisibility()
        {
            // Метод для настройки видимости кнопок в зависимости от прав
            // Будет переопределяться в наследниках
        }
    }
}