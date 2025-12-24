using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BusParkManagementSystem.Data;
using BusParkManagementSystem.Models;
using BusParkManagementSystem.Repositories;
using BusParkManagementSystem.Views.Dialogs;

namespace BusParkManagementSystem.ViewModels
{
    public class TripViewModel : INotifyPropertyChanged
    {
        private readonly ITripRepository _tripRepository;
        private ObservableCollection<Trip> _trips;
        private ObservableCollection<Trip> _allTrips; // Хранит все рейсы
        private Trip _selectedTrip;
        private string _searchTerm;
        private string _selectedStatusFilter;
        private DateTime? _selectedDateFilter;
        private bool _isLoading;

        public ObservableCollection<Trip> Trips
        {
            get => _trips;
            set
            {
                _trips = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Trip> AllTrips
        {
            get => _allTrips;
            set
            {
                _allTrips = value;
                OnPropertyChanged();
            }
        }

        public Trip SelectedTrip
        {
            get => _selectedTrip;
            set
            {
                _selectedTrip = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanEditOrDelete));
                OnPropertyChanged(nameof(CanCancelTrip));
                OnPropertyChanged(nameof(CanCompleteTrip));
                OnPropertyChanged(nameof(CanStartTrip));
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
            }
        }

        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                _selectedStatusFilter = value;
                OnPropertyChanged();
            }
        }

        public DateTime? SelectedDateFilter
        {
            get => _selectedDateFilter;
            set
            {
                _selectedDateFilter = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanEditOrDelete));
                OnPropertyChanged(nameof(CanCancelTrip));
                OnPropertyChanged(nameof(CanCompleteTrip));
                OnPropertyChanged(nameof(CanStartTrip));
            }
        }

        public ObservableCollection<string> StatusFilters { get; }
        public ObservableCollection<string> ShiftTypes { get; }

        public ICommand AddTripCommand { get; }
        public ICommand EditTripCommand { get; }
        public ICommand DeleteTripCommand { get; }
        public ICommand CancelTripCommand { get; }
        public ICommand CompleteTripCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand StartTripCommand { get; }
        public ICommand ApplyFiltersCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public TripViewModel()
        {
            try
            {
                var dbContext = new DatabaseContext();
                _tripRepository = new TripRepository(dbContext);

                Trips = new ObservableCollection<Trip>();
                AllTrips = new ObservableCollection<Trip>();

                // Инициализация фильтров
                StatusFilters = new ObservableCollection<string>
                {
                    "Все статусы",
                    "запланирован",
                    "в_пути",
                    "завершен",
                    "отменен"
                };

                ShiftTypes = new ObservableCollection<string>
                {
                    "Утренняя",
                    "Дневная",
                    "Вечерная"
                };

                SelectedStatusFilter = "Все статусы";
                SelectedDateFilter = null;

                // Инициализация команд
                AddTripCommand = new RelayCommand(AddTrip);
                EditTripCommand = new RelayCommand(EditTrip, CanEditOrDelete);
                DeleteTripCommand = new RelayCommand(DeleteTrip, CanEditOrDelete);
                CancelTripCommand = new RelayCommand(CancelTrip, CanCancelTrip);
                CompleteTripCommand = new RelayCommand(CompleteTrip, CanCompleteTrip);
                RefreshCommand = new RelayCommand(Refresh);
                ClearFiltersCommand = new RelayCommand(ClearFilters);
                StartTripCommand = new RelayCommand(StartTrip, CanStartTrip);
                ApplyFiltersCommand = new RelayCommand(ApplyFilters);

                // Загрузка данных
                _ = LoadTripsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации TripViewModel: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadTripsAsync()
        {
            try
            {
                IsLoading = true;
                var trips = await _tripRepository.GetAllAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    AllTrips.Clear();
                    Trips.Clear();

                    foreach (var trip in trips)
                    {
                        AllTrips.Add(trip);
                        Trips.Add(trip);
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка при загрузке рейсов: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchTripsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                await ApplyFiltersAsync();
                return;
            }

            try
            {
                IsLoading = true;
                var searchTermLower = SearchTerm.ToLower();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var filteredTrips = AllTrips.Where(t =>
                        (!string.IsNullOrEmpty(t.RouteNumber) && t.RouteNumber.ToLower().Contains(searchTermLower)) ||
                        (!string.IsNullOrEmpty(t.BusGovPlate) && t.BusGovPlate.ToLower().Contains(searchTermLower)) ||
                        (!string.IsNullOrEmpty(t.DriverName) && t.DriverName.ToLower().Contains(searchTermLower)) ||
                        (!string.IsNullOrEmpty(t.ConductorName) && t.ConductorName.ToLower().Contains(searchTermLower)));

                    // Применяем дополнительные фильтры
                    filteredTrips = ApplyAdditionalFilters(filteredTrips);

                    Trips.Clear();
                    foreach (var trip in filteredTrips)
                    {
                        Trips.Add(trip);
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка при поиске: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ApplyFiltersAsync()
        {
            try
            {
                IsLoading = true;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var filteredTrips = ApplyAdditionalFilters(AllTrips);

                    Trips.Clear();
                    foreach (var trip in filteredTrips)
                    {
                        Trips.Add(trip);
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка при фильтрации: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private System.Collections.Generic.IEnumerable<Trip> ApplyAdditionalFilters(System.Collections.Generic.IEnumerable<Trip> trips)
        {
            var filteredTrips = trips;

            // Фильтр по статусу
            if (SelectedStatusFilter != "Все статусы")
            {
                filteredTrips = filteredTrips.Where(t => t.Status == SelectedStatusFilter);
            }

            // Фильтр по дате
            if (SelectedDateFilter.HasValue)
            {
                filteredTrips = filteredTrips.Where(t =>
                    t.TripDate.Date == SelectedDateFilter.Value.Date);
            }

            // Фильтр по поисковому запросу
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var searchTermLower = SearchTerm.ToLower();
                filteredTrips = filteredTrips.Where(t =>
                    (!string.IsNullOrEmpty(t.RouteNumber) && t.RouteNumber.ToLower().Contains(searchTermLower)) ||
                    (!string.IsNullOrEmpty(t.BusGovPlate) && t.BusGovPlate.ToLower().Contains(searchTermLower)) ||
                    (!string.IsNullOrEmpty(t.DriverName) && t.DriverName.ToLower().Contains(searchTermLower)) ||
                    (!string.IsNullOrEmpty(t.ConductorName) && t.ConductorName.ToLower().Contains(searchTermLower)));
            }

            return filteredTrips.OrderByDescending(t => t.TripDate).ThenByDescending(t => t.Id);
        }

        private void ApplyFilters(object parameter)
        {
            _ = ApplyFiltersAsync();
        }

        private void AddTrip(object parameter)
        {
            try
            {
                Debug.WriteLine("AddTrip: Начало");

                var newTrip = new Trip
                {
                    TripDate = DateTime.Today,
                    PlannedRevenue = 10000,
                    Status = "запланирован"
                };

                Debug.WriteLine($"AddTrip: Создан новый Trip. Дата: {newTrip.TripDate}");

                var dialog = new TripEditDialog();

                var dbContext = new DatabaseContext();
                var tripRepo = new TripRepository(dbContext);
                var lookupRepo = new LookupRepository(dbContext);

                Debug.WriteLine($"AddTrip: Репозитории созданы. TripRepo: {tripRepo != null}, LookupRepo: {lookupRepo != null}");

                var viewModel = new TripEditViewModel(tripRepo, lookupRepo)
                {
                    Trip = newTrip,
                    IsEditMode = false,
                    CloseAction = async result =>
                    {
                        try
                        {
                            dialog.DialogResult = result;
                            dialog.Close();

                            if (result == true)
                            {
                                await LoadTripsAsync();
                                await ApplyFiltersAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"AddTrip CloseAction ошибка: {ex.Message}");
                        }
                    }
                };

                Debug.WriteLine("AddTrip: ViewModel создана");

                dialog.DataContext = viewModel;
                dialog.Owner = Application.Current.MainWindow;

                Debug.WriteLine("AddTrip: Показ диалога");
                dialog.ShowDialog();

                Debug.WriteLine("AddTrip: Завершено");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AddTrip ошибка: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Ошибка при добавлении рейса: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditTrip(object parameter)
        {
            if (SelectedTrip != null && SelectedTrip.CanEdit)
            {
                try
                {
                    Debug.WriteLine($"EditTrip: Начало редактирования рейса {SelectedTrip.Id}");

                    var dialog = new TripEditDialog();

                    var dbContext = new DatabaseContext();
                    var tripRepo = new TripRepository(dbContext);
                    var lookupRepo = new LookupRepository(dbContext);

                    var viewModel = new TripEditViewModel(tripRepo, lookupRepo, SelectedTrip)
                    {
                        CloseAction = async result =>
                        {
                            try
                            {
                                dialog.DialogResult = result;
                                dialog.Close();

                                if (result == true)
                                {
                                    await LoadTripsAsync();
                                    await ApplyFiltersAsync();

                                    if (SelectedTrip != null)
                                    {
                                        var updatedTrip = Trips.FirstOrDefault(t => t.Id == SelectedTrip.Id);
                                        SelectedTrip = updatedTrip;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"EditTrip CloseAction ошибка: {ex.Message}");
                            }
                        }
                    };

                    dialog.DataContext = viewModel;
                    dialog.Owner = Application.Current.MainWindow;

                    Debug.WriteLine("EditTrip: Показ диалога редактирования");
                    dialog.ShowDialog();

                    Debug.WriteLine("EditTrip: Завершено");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"EditTrip ошибка: {ex.Message}\n{ex.StackTrace}");
                    MessageBox.Show($"Ошибка при редактировании рейса: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (SelectedTrip != null)
            {
                MessageBox.Show("Редактирование невозможно: рейс уже выполняется или завершен",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void DeleteTrip(object parameter)
        {
            if (SelectedTrip != null)
            {
                if (SelectedTrip.Status == "в_пути")
                {
                    MessageBox.Show("Нельзя удалить рейс, который находится в пути. Сначала отмените его.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Удалить рейс ID: {SelectedTrip.Id}?\n" +
                    $"Дата: {SelectedTrip.DateFormatted}\n" +
                    $"Маршрут: {SelectedTrip.RouteNumber}\n" +
                    $"Автобус: {SelectedTrip.BusGovPlate}\n\n" +
                    "Это действие невозможно отменить!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        IsLoading = true;
                        bool success = await _tripRepository.DeleteAsync(SelectedTrip.Id);

                        if (success)
                        {
                            var tripToRemove = AllTrips.FirstOrDefault(t => t.Id == SelectedTrip.Id);
                            if (tripToRemove != null)
                            {
                                AllTrips.Remove(tripToRemove);
                            }

                            tripToRemove = Trips.FirstOrDefault(t => t.Id == SelectedTrip.Id);
                            if (tripToRemove != null)
                            {
                                Trips.Remove(tripToRemove);
                            }

                            SelectedTrip = null;

                            MessageBox.Show("Рейс успешно удален", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить рейс. Возможно, есть связанные записи.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
            }
        }

        private async void CancelTrip(object parameter)
        {
            if (SelectedTrip != null && SelectedTrip.CanCancel)
            {
                var reason = "По техническим причинам";

                var result = MessageBox.Show($"Отменить рейс {SelectedTrip.TripInfo}?",
                    "Подтверждение отмены", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        IsLoading = true;
                        bool success = await _tripRepository.CancelAsync(SelectedTrip.Id, reason);

                        if (success)
                        {
                            var tripToUpdate = AllTrips.FirstOrDefault(t => t.Id == SelectedTrip.Id);
                            if (tripToUpdate != null)
                            {
                                tripToUpdate.Status = "отменен";
                                tripToUpdate.CancellationReason = reason;
                            }

                            tripToUpdate = Trips.FirstOrDefault(t => t.Id == SelectedTrip.Id);
                            if (tripToUpdate != null)
                            {
                                tripToUpdate.Status = "отменен";
                                tripToUpdate.CancellationReason = reason;
                            }

                            OnPropertyChanged(nameof(Trips));
                            OnPropertyChanged(nameof(AllTrips));

                            MessageBox.Show("Рейс успешно отменен", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при отмене рейса: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
            }
            else if (SelectedTrip != null)
            {
                MessageBox.Show("Отмена невозможна: рейс уже завершен или отменен",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void CompleteTrip(object parameter)
        {
            if (SelectedTrip != null && SelectedTrip.Status == "в_пути")
            {
                try
                {
                    Debug.WriteLine($"CompleteTrip: Завершение рейса {SelectedTrip.Id}");

                    var dialog = new TripCompletionDialog();

                    var dbContext = new DatabaseContext();
                    var tripRepo = new TripRepository(dbContext);

                    var viewModel = new TripCompletionViewModel(tripRepo)
                    {
                        Trip = SelectedTrip,
                        CloseAction = async result =>
                        {
                            try
                            {
                                dialog.DialogResult = result;
                                dialog.Close();

                                if (result == true)
                                {
                                    await LoadTripsAsync();
                                    await ApplyFiltersAsync();
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"CompleteTrip CloseAction ошибка: {ex.Message}");
                            }
                        }
                    };

                    dialog.DataContext = viewModel;
                    dialog.Owner = Application.Current.MainWindow;

                    Debug.WriteLine("CompleteTrip: Показ диалога завершения");
                    dialog.ShowDialog();

                    Debug.WriteLine("CompleteTrip: Завершено");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"CompleteTrip ошибка: {ex.Message}\n{ex.StackTrace}");
                    MessageBox.Show($"Ошибка при завершении рейса: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (SelectedTrip != null)
            {
                MessageBox.Show("Завершение возможно только для рейсов в пути",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void Refresh(object parameter)
        {
            await LoadTripsAsync();

            MessageBox.Show("Данные обновлены", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearFilters(object parameter)
        {
            SelectedStatusFilter = "Все статусы";
            SelectedDateFilter = null;
            SearchTerm = string.Empty;

            _ = ApplyFiltersAsync();
        }

        private bool CanEditOrDelete(object parameter)
        {
            return SelectedTrip != null && !IsLoading;
        }

        private bool CanCancelTrip(object parameter)
        {
            return SelectedTrip != null && SelectedTrip.CanCancel && !IsLoading;
        }

        private bool CanCompleteTrip(object parameter)
        {
            return SelectedTrip != null && SelectedTrip.Status == "в_пути" && !IsLoading;
        }

        private bool CanStartTrip(object parameter)
        {
            return SelectedTrip != null && SelectedTrip.Status == "запланирован" && !IsLoading;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void StartTrip(object parameter)
        {
            if (SelectedTrip != null && SelectedTrip.Status == "запланирован")
            {
                var result = MessageBox.Show($"Начать рейс {SelectedTrip.TripInfo}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        IsLoading = true;
                        SelectedTrip.Status = "в_пути";
                        bool success = await _tripRepository.UpdateAsync(SelectedTrip);

                        if (success)
                        {
                            var tripToUpdate = AllTrips.FirstOrDefault(t => t.Id == SelectedTrip.Id);
                            if (tripToUpdate != null)
                            {
                                tripToUpdate.Status = "в_пути";
                            }

                            tripToUpdate = Trips.FirstOrDefault(t => t.Id == SelectedTrip.Id);
                            if (tripToUpdate != null)
                            {
                                tripToUpdate.Status = "в_пути";
                            }

                            OnPropertyChanged(nameof(Trips));
                            OnPropertyChanged(nameof(AllTrips));

                            MessageBox.Show("Рейс начат", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
            }
        }
    }
}