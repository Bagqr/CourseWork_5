using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BusParkManagementSystem.Models;
using BusParkManagementSystem.Repositories;

namespace BusParkManagementSystem.ViewModels
{
    /// <summary>
    /// Базовый ViewModel для всех справочников
    /// </summary>
    /// <typeparam name="T">Тип модели справочника</typeparam>
    public abstract class BaseLookupViewModel<T> : BaseViewModel where T : class, new()
    {
        protected readonly ILookupRepository _lookupRepository;

        private ObservableCollection<T> _items;
        private T _selectedItem;
        private string _searchTerm;
        private bool _isLoading;
        private string _lookupTitle;
        private string _lookupName;

        public ObservableCollection<T> Items
        {
            get => _items;
            set => SetField(ref _items, value);
        }

        public T SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetField(ref _selectedItem, value);
                OnPropertyChanged(nameof(IsItemSelected));
            }
        }

        public bool IsItemSelected => SelectedItem != null;

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (SetField(ref _searchTerm, value))
                {
                    OnSearchTermChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetField(ref _isLoading, value);
        }

        public string LookupTitle
        {
            get => _lookupTitle;
            protected set => SetField(ref _lookupTitle, value);
        }

        public string LookupName
        {
            get => _lookupName;
            protected set => SetField(ref _lookupName, value);
        }

        // Команды
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearSearchCommand { get; }

        protected BaseLookupViewModel(ILookupRepository lookupRepository, string lookupTitle, string lookupName)
        {
            _lookupRepository = lookupRepository ?? throw new ArgumentNullException(nameof(lookupRepository));
            LookupTitle = lookupTitle;
            LookupName = lookupName;

            Items = new ObservableCollection<T>();

            // Инициализация команд с правильными сигнатурами делегатов
            AddCommand = new RelayCommand(async (param) => await ExecuteAddCommand());
            EditCommand = new RelayCommand(async (param) => await ExecuteEditCommand(), (param) => CanExecuteEditCommand());
            DeleteCommand = new RelayCommand(async (param) => await ExecuteDeleteCommand(), (param) => CanExecuteDeleteCommand());
            RefreshCommand = new RelayCommand(async (param) => await ExecuteRefreshCommand());
            ClearSearchCommand = new RelayCommand((param) => ExecuteClearSearchCommand());

            // Загружаем данные
            LoadDataAsync().ConfigureAwait(false);
        }

        // Методы для команд
        private async Task ExecuteAddCommand()
        {
            await AddItemAsync();
        }

        private async Task ExecuteEditCommand()
        {
            await EditItemAsync();
        }

        private bool CanExecuteEditCommand()
        {
            return IsItemSelected;
        }

        private async Task ExecuteDeleteCommand()
        {
            await DeleteItemAsync();
        }

        private bool CanExecuteDeleteCommand()
        {
            return IsItemSelected;
        }

        private async Task ExecuteRefreshCommand()
        {
            await LoadDataAsync();
        }

        private void ExecuteClearSearchCommand()
        {
            ClearSearch();
        }

        // Абстрактные методы
        protected abstract Task<IEnumerable<T>> LoadDataFromRepositoryAsync();
        protected abstract Task<int> AddItemToRepositoryAsync(T item);
        protected abstract Task<bool> UpdateItemInRepositoryAsync(T item);
        protected abstract Task<bool> DeleteItemFromRepositoryAsync(int id);
        protected abstract bool ItemMatchesSearch(T item, string searchTerm);

        // Основные методы
        public virtual async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                var data = await LoadDataFromRepositoryAsync();
                Items = new ObservableCollection<T>(data);
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected virtual async Task AddItemAsync()
        {
            try
            {
                var dialogResult = ShowEditDialog("Добавление записи");
                if (dialogResult != null)
                {
                    var id = await AddItemToRepositoryAsync(dialogResult);
                    if (id > 0)
                    {
                        await LoadDataAsync();
                        ShowSuccess("Запись успешно добавлена");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка добавления: {ex.Message}");
            }
        }

        protected virtual async Task EditItemAsync()
        {
            if (SelectedItem == null)
            {
                ShowWarning("Выберите запись для редактирования");
                return;
            }

            try
            {
                // СОЗДАЕМ КОПИЮ для редактирования
                var itemCopy = CreateCopy(SelectedItem);
                var dialogResult = ShowEditDialog("Редактирование записи", itemCopy);

                if (dialogResult != null)
                {
                    // Получаем ID для отладки
                    var idProperty = dialogResult.GetType().GetProperty("Id");
                    int id = -1;
                    if (idProperty != null)
                    {
                        id = (int)idProperty.GetValue(dialogResult);
                    }

                    // Получаем имя для отладки
                    var nameProperty = dialogResult.GetType().GetProperties()
                        .FirstOrDefault(p => p.PropertyType == typeof(string) && p.CanRead);
                    string itemName = nameProperty?.GetValue(dialogResult)?.ToString() ?? "запись";

                    var success = await UpdateItemInRepositoryAsync(dialogResult);
                    if (success)
                    {
                        await LoadDataAsync();
                        ShowSuccess($"Запись '{itemName}' успешно обновлена (ID: {id})");
                    }
                    else
                    {
                        ShowError($"Не удалось обновить запись '{itemName}' (ID: {id})");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка редактирования: {ex.Message}");
            }
        }

        // Вспомогательный метод для создания копии
        private T CreateCopy(T original)
        {
            if (original == null) return null;

            var copy = new T();
            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                if (prop.CanWrite && prop.CanRead)
                {
                    var value = prop.GetValue(original);
                    prop.SetValue(copy, value);
                }
            }
            return copy;
        }

        protected virtual async Task DeleteItemAsync()
        {
            if (SelectedItem == null)
            {
                ShowWarning("Выберите запись для удаления");
                return;
            }

            var result = MessageBox.Show(
                "Вы уверены, что хотите удалить выбранную запись?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var idProperty = SelectedItem.GetType().GetProperty("Id");
                    if (idProperty == null)
                    {
                        ShowError("Не удалось определить ID записи");
                        return;
                    }

                    var id = (int)idProperty.GetValue(SelectedItem);
                    var success = await DeleteItemFromRepositoryAsync(id);

                    if (success)
                    {
                        await LoadDataAsync();
                        ShowSuccess("Запись успешно удалена");
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        protected virtual void OnSearchTermChanged()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                LoadDataAsync().ConfigureAwait(false);
            }
            else
            {
                Task.Run(async () =>
                {
                    var allData = await LoadDataFromRepositoryAsync();
                    var filtered = allData.Where(item =>
                        ItemMatchesSearch(item, SearchTerm.ToLower())
                    ).ToList();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Items = new ObservableCollection<T>(filtered);
                    });
                });
            }
        }

        protected virtual void ClearSearch()
        {
            SearchTerm = string.Empty;
            LoadDataAsync().ConfigureAwait(false);
        }

        // Диалоговое окно - возвращает новый/отредактированный объект или null если отмена
        protected virtual T ShowEditDialog(string title, T existingItem = null)
        {
            // Создаем окно диалога
            var dialog = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new System.Windows.Controls.StackPanel
            {
                Margin = new Thickness(20)
            };

            var textBlock = new System.Windows.Controls.TextBlock
            {
                Text = $"Введите {LookupName.ToLower()}:"
            };

            var textBox = new System.Windows.Controls.TextBox
            {
                Margin = new Thickness(0, 5, 0, 15),
                Text = existingItem != null ? GetItemName(existingItem) : ""
            };

            var buttonPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right
            };

            var okButton = new System.Windows.Controls.Button
            {
                Content = "OK",
                Width = 80,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };

            var cancelButton = new System.Windows.Controls.Button
            {
                Content = "Отмена",
                Width = 80,
                IsCancel = true
            };

            T resultItem = null;

            okButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    MessageBox.Show("Значение не может быть пустым", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // СОЗДАЕМ НОВЫЙ ОБЪЕКТ ДЛЯ РЕЗУЛЬТАТА
                resultItem = new T();

                // Копируем ID из существующего элемента, если он есть
                if (existingItem != null)
                {
                    var idProperty = existingItem.GetType().GetProperty("Id");
                    if (idProperty != null)
                    {
                        idProperty.SetValue(resultItem, idProperty.GetValue(existingItem));
                    }
                }

                // Находим первое строковое свойство и устанавливаем ему значение
                var stringProperties = typeof(T).GetProperties()
                    .Where(p => p.PropertyType == typeof(string) && p.CanWrite)
                    .ToList();

                if (stringProperties.Count > 0)
                {
                    stringProperties[0].SetValue(resultItem, textBox.Text.Trim());
                    dialog.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Не удалось найти свойство для редактирования", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            cancelButton.Click += (s, e) =>
            {
                dialog.DialogResult = false;
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(buttonPanel);

            dialog.Content = stackPanel;

            return dialog.ShowDialog() == true ? resultItem : null;
        }

        // Вспомогательный метод для получения имени элемента
        private string GetItemName(T item)
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string) && p.CanRead)
                .ToList();

            if (properties.Count > 0)
            {
                return properties[0].GetValue(item) as string ?? "";
            }

            return "";
        }

        // Вспомогательные методы для отображения сообщений
        protected void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected void ShowWarning(string message)
        {
            MessageBox.Show(message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        protected void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}