using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;  
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BusParkManagementSystem.Data;
using BusParkManagementSystem.Models;
using BusParkManagementSystem.Repositories;
using BusParkManagementSystem.Views.Dialogs;
using Dapper;

namespace BusParkManagementSystem.ViewModels
{
    public class BusViewModel : INotifyPropertyChanged
    {
        private readonly IBusRepository _busRepository;
        private ObservableCollection<Bus> _buses;
        private Bus _selectedBus;
        private string _searchTerm;
        private string _selectedStateFilter;
        private string _selectedModelFilter;
        private bool _isLoading;

        public ObservableCollection<Bus> Buses
        {
            get => _buses;
            set
            {
                _buses = value;
                OnPropertyChanged();
            }
        }

        public Bus SelectedBus
        {
            get => _selectedBus;
            set
            {
                _selectedBus = value;
                OnPropertyChanged();
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
                _ = SearchBusesAsync();
            }
        }

        public string SelectedStateFilter
        {
            get => _selectedStateFilter;
            set
            {
                _selectedStateFilter = value;
                OnPropertyChanged();
                _ = FilterBusesAsync();
            }
        }

        public string SelectedModelFilter
        {
            get => _selectedModelFilter;
            set
            {
                _selectedModelFilter = value;
                OnPropertyChanged();
                _ = FilterBusesAsync();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> StateFilters { get; }
        public ObservableCollection<string> ModelFilters { get; }

        public ICommand AddBusCommand { get; }
        public ICommand EditBusCommand { get; }
        public ICommand DeleteBusCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public BusViewModel()
        {
            try
            {
                // Инициализация репозитория
                var dbContext = new DatabaseContext();
                _busRepository = new BusRepository(dbContext);

                Buses = new ObservableCollection<Bus>();

                // Инициализация фильтров
                StateFilters = new ObservableCollection<string>
                {
                    "Все",
                    "ИСПРАВЕН",
                    "В РЕМОНТЕ",
                    "НЕИСПРАВЕН",
                    "СПИСАН"
                };

                ModelFilters = new ObservableCollection<string>
                {
                    "Все модели",
                    "ПАЗ-3205",
                    "ЛИАЗ-5256",
                    "ЛИАЗ-5292",
                    "МАЗ-103",
                    "НефАЗ-5299",
                    "Volgabus-5270",
                    "ПАЗ-3234",
                    "КАВЗ-4235",
                    "Богдан-А601",
                    "МАЗ-206"
                };

                SelectedStateFilter = "Все";
                SelectedModelFilter = "Все модели";

                // Инициализация команд
                AddBusCommand = new RelayCommand(AddBus);
                EditBusCommand = new RelayCommand(EditBus, CanEditOrDelete);
                DeleteBusCommand = new RelayCommand(DeleteBus, CanEditOrDelete);
                RefreshCommand = new RelayCommand(Refresh);
                ClearFiltersCommand = new RelayCommand(ClearFilters);

                // Загрузка данных
                _ = LoadBusesAsync();
            }
            catch (Exception ex)
            {
                // Полная информация об ошибке
                string errorDetails = $"Ошибка при инициализации BusViewModel:\n\n" +
                                    $"Сообщение: {ex.Message}\n\n" +
                                    $"Тип: {ex.GetType().FullName}\n\n" +
                                    $"StackTrace: {ex.StackTrace}";

                MessageBox.Show(errorDetails, "Ошибка инициализации",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // Для отладки - запись в файл
                System.IO.File.WriteAllText("error_log.txt",
                    $"{DateTime.Now}: {ex.ToString()}\n\nInner Exception: {ex.InnerException?.ToString()}");
            }
        }

        private async Task LoadBusesAsync()
        {
            try
            {
                IsLoading = true;
                var buses = await _busRepository.GetAllAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // Сохраняем текущий выбранный элемент
                    var selectedId = SelectedBus?.Id;

                    // Очищаем и добавляем заново
                    Buses.Clear();
                    foreach (var bus in buses)
                    {
                        Buses.Add(bus);
                    }

                    // Восстанавливаем выбранный элемент, если он еще существует
                    if (selectedId.HasValue)
                    {
                        SelectedBus = Buses.FirstOrDefault(b => b.Id == selectedId.Value);
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка при загрузке автобусов: {ex.Message}\n\nДетали: {ex.InnerException?.Message}",
                        "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchBusesAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                await FilterBusesAsync();
                return;
            }

            try
            {
                IsLoading = true;
                var buses = await _busRepository.SearchAsync(SearchTerm);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Buses.Clear();
                    foreach (var bus in buses)
                    {
                        Buses.Add(bus);
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка при поиске: {ex.Message}",
                        "Ошибка поиска", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task FilterBusesAsync()
        {
            try
            {
                IsLoading = true;
                var allBuses = await _busRepository.GetAllAsync();

                var filteredBuses = allBuses.AsEnumerable();

                // Фильтр по состоянию
                if (SelectedStateFilter != "Все")
                {
                    filteredBuses = filteredBuses.Where(b => b.StateName == SelectedStateFilter);
                }

                // Фильтр по модели
                if (SelectedModelFilter != "Все модели")
                {
                    filteredBuses = filteredBuses.Where(b => b.ModelName == SelectedModelFilter);
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Buses.Clear();
                    foreach (var bus in filteredBuses)
                    {
                        Buses.Add(bus);
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка при фильтрации: {ex.Message}",
                        "Ошибка фильтрации", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void AddBus(object parameter)
        {
            try
            {
                var newBus = new Bus
                {
                    ManufacturerDate = DateTime.Now.AddYears(-1),
                    LastOverhaulDate = DateTime.Now,
                    Mileage = 0,
                    StateId = 1 // ИСПРАВЕН по умолчанию
                };

                var dialog = new BusEditDialog();
                var viewModel = new BusEditViewModel(_busRepository, new LookupRepository(new DatabaseContext()))
                {
                    Bus = newBus,
                    IsEditMode = false,
                    CloseAction = async result =>
                    {
                        if (result == true)
                        {
                            await LoadBusesAsync();
                            if (Buses.Count > 0) SelectedBus = Buses.Last();
                        }
                        dialog.DialogResult = result;
                        dialog.Close();
                    }
                };

                dialog.DataContext = viewModel;
                dialog.Owner = Application.Current.MainWindow;

                // Инициализируем перед показом
                await viewModel.InitializeAsync();

                if (dialog.ShowDialog() == true)
                {
                    await LoadBusesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditBus(object parameter)
        {
            if (SelectedBus == null) return;

            try
            {
                // Загружаем полные данные автобуса
                var bus = await _busRepository.GetByIdAsync(SelectedBus.Id);
                if (bus == null)
                {
                    MessageBox.Show("Автобус не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var dialog = new BusEditDialog();
                var viewModel = new BusEditViewModel(_busRepository, new LookupRepository(new DatabaseContext()))
                {
                    Bus = bus,
                    IsEditMode = true,
                    CloseAction = async result =>
                    {
                        if (result == true)
                        {
                            await LoadBusesAsync();
                            var updatedBus = Buses.FirstOrDefault(b => b.Id == bus.Id);
                            if (updatedBus != null) SelectedBus = updatedBus;
                        }
                        dialog.DialogResult = result;
                        dialog.Close();
                    }
                };

                dialog.DataContext = viewModel;
                dialog.Owner = Application.Current.MainWindow;

                // ИНИЦИАЛИЗИРУЕМ ViewModel ПЕРЕД показом диалога
                await viewModel.InitializeAsync();

                // Только после инициализации показываем диалог
                if (dialog.ShowDialog() == true)
                {
                    //await LoadBusesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteBus(object parameter)
        {
            if (SelectedBus == null) return;

            try
            {
                // Проверяем, есть ли зависимости (рейсы)
                using (var connection = new DatabaseContext().GetConnection())
                {
                    var checkQuery = "SELECT COUNT(*) FROM рейс WHERE автобус_id = @BusId";
                    var tripCount = await connection.ExecuteScalarAsync<int>(checkQuery,
                        new { BusId = SelectedBus.Id });

                    if (tripCount > 0)
                    {
                        MessageBox.Show($"Невозможно удалить автобус! Он используется в {tripCount} рейсах.\n\n" +
                                       "Сначала удалите связанные рейсы или измените состояние автобуса на 'СПИСАН'.",
                            "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить автобус?\n\n" +
                    $"Инв. №: {SelectedBus.InventoryNumber}\n" +
                    $"Гос. номер: {SelectedBus.GovPlate}\n\n" +
                    "Это действие нельзя отменить!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    IsLoading = true;
                    bool success = await _busRepository.DeleteAsync(SelectedBus.Id);

                    if (success)
                    {
                        // Сохраняем индекс для возможного выбора следующего элемента
                        var currentIndex = Buses.IndexOf(SelectedBus);

                        // Удаляем из коллекции
                        Buses.Remove(SelectedBus);

                        // Выбираем следующий или предыдущий элемент
                        if (Buses.Count > 0)
                        {
                            if (currentIndex >= Buses.Count)
                            {
                                SelectedBus = Buses.Last();
                            }
                            else if (currentIndex >= 0)
                            {
                                SelectedBus = Buses[currentIndex];
                            }
                            else
                            {
                                SelectedBus = Buses.FirstOrDefault();
                            }
                        }
                        else
                        {
                            SelectedBus = null;
                        }

                        MessageBox.Show("Автобус успешно удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить автобус", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void Refresh(object parameter)
        {
            await LoadBusesAsync();
            SearchTerm = string.Empty;
            SelectedStateFilter = "Все";
            SelectedModelFilter = "Все модели";

            MessageBox.Show("Данные обновлены", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearFilters(object parameter)
        {
            SearchTerm = string.Empty;
            SelectedStateFilter = "Все";
            SelectedModelFilter = "Все модели";
        }

        private bool CanEditOrDelete(object parameter)
        {
            return SelectedBus != null && !IsLoading;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Класс RelayCommand для реализации ICommand
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}