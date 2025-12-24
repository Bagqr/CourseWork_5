using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BusParkManagementSystem.Models;
using BusParkManagementSystem.Repositories;

namespace BusParkManagementSystem.ViewModels
{
    public class TripEditViewModel : BaseViewModel
    {
        private readonly ITripRepository _tripRepository;
        private readonly ILookupRepository _lookupRepository;
        private Trip _trip;
        private bool _isEditMode;
        private ObservableCollection<string> _errors;
        private bool _isLoading;

        public Trip Trip
        {
            get => _trip;
            set
            {
                try
                {
                    if (_trip != null)
                    {
                        _trip.PropertyChanged -= Trip_PropertyChanged;
                    }

                    SetField(ref _trip, value);

                    if (_trip != null)
                    {
                        _trip.PropertyChanged += Trip_PropertyChanged;
                        _ = ValidateAndUpdateAsync();  // Запускаем валидацию только если Trip не null
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка установки Trip: {ex.Message}");
                }
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetField(ref _isEditMode, value);
        }
        public bool IsLoading
        {
            get => _isLoading;
            set => SetField(ref _isLoading, value);
        }
        public string WindowTitle => IsEditMode ? "Редактирование рейса" : "Добавление рейса";

        // Справочники с правильными типами
        public ObservableCollection<Route> Routes { get; } = new ObservableCollection<Route>();
        public ObservableCollection<Bus> Buses { get; } = new ObservableCollection<Bus>();
        public ObservableCollection<Employee> Drivers { get; } = new ObservableCollection<Employee>();
        public ObservableCollection<Employee> Conductors { get; } = new ObservableCollection<Employee>();
        public ObservableCollection<ShiftType> ShiftTypes { get; } = new ObservableCollection<ShiftType>();

        public ObservableCollection<string> Statuses { get; } = new ObservableCollection<string>
        {
            "запланирован",
            "в_пути",
            "завершен",
            "отменен"
        };

        public ObservableCollection<string> Errors
        {
            get => _errors;
            set => SetField(ref _errors, value);
        }

        public bool HasErrors => Errors?.Count > 0;
        public bool CanSave => !HasErrors;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public Action<bool?> CloseAction { get; set; }

        public TripEditViewModel(ITripRepository tripRepository, ILookupRepository lookupRepository, Trip trip = null)
        {
            try
            {
                Debug.WriteLine("TripEditViewModel: Начало конструктора");

                if (tripRepository == null)
                    throw new ArgumentNullException(nameof(tripRepository));
                if (lookupRepository == null)
                    throw new ArgumentNullException(nameof(lookupRepository));

                _tripRepository = tripRepository;
                _lookupRepository = lookupRepository;
                Errors = new ObservableCollection<string>();

                Debug.WriteLine("TripEditViewModel: Репозитории установлены");

                SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave);
                CancelCommand = new RelayCommand(_ => Cancel());

                Debug.WriteLine("TripEditViewModel: Команды созданы");

                // Если передан trip, значит это редактирование
                if (trip != null)
                {
                    Trip = trip;
                    IsEditMode = true;
                    Debug.WriteLine($"TripEditViewModel: Режим редактирования, ID: {trip.Id}");
                }
                else
                {
                    // Создаем новый рейс
                    Trip = new Trip
                    {
                        TripDate = DateTime.Today,
                        PlannedRevenue = 10000,
                        Status = "запланирован"
                    };
                    IsEditMode = false;
                    Debug.WriteLine("TripEditViewModel: Режим добавления");
                }

                // Загружаем справочники
                _ = LoadLookupDataAsync();

                Debug.WriteLine("TripEditViewModel: Конструктор завершен");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TripEditViewModel: Ошибка в конструкторе: {ex.Message}");
                MessageBox.Show($"Ошибка создания ViewModel: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private async void Trip_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (Trip == null) return;

                if (e.PropertyName == nameof(Trip.RouteId) ||
                    e.PropertyName == nameof(Trip.BusId) ||
                    e.PropertyName == nameof(Trip.DriverId) ||
                    e.PropertyName == nameof(Trip.ConductorId) ||
                    e.PropertyName == nameof(Trip.TripDate) ||
                    e.PropertyName == nameof(Trip.ShiftTypeId) ||
                    e.PropertyName == nameof(Trip.PlannedRevenue) ||
                    e.PropertyName == nameof(Trip.Status))
                {
                    await ValidateAndUpdateAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Trip_PropertyChanged ошибка: {ex.Message}");
            }
        }

        private async Task ValidateAndUpdateAsync()
        {
            await ValidateAsync();
        }

        private async Task LoadLookupDataAsync()
        {
            try
            {
                IsLoading = true;

                // Получаем данные используя полноценные модели
                var routes = await _lookupRepository.GetRoutesAsync();
                var buses = await _lookupRepository.GetAvailableBusesAsync();
                var drivers = await _lookupRepository.GetAvailableDriversAsync();
                var conductors = await _lookupRepository.GetAvailableConductorsAsync();
                var shiftTypes = await _lookupRepository.GetShiftTypesAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // Заполняем коллекции
                    Routes.Clear();
                    foreach (var route in routes) Routes.Add(route);

                    Buses.Clear();
                    foreach (var bus in buses) Buses.Add(bus);

                    Drivers.Clear();
                    foreach (var driver in drivers) Drivers.Add(driver);

                    Conductors.Clear();
                    foreach (var conductor in conductors) Conductors.Add(conductor);

                    ShiftTypes.Clear();
                    foreach (var shiftType in shiftTypes) ShiftTypes.Add(shiftType);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочников: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<bool> ValidateAsync()
        {
            var errors = new List<string>();

            if (Trip == null)
            {
                errors.Add("Рейс не инициализирован");
                goto UpdateErrors;
            }

            if (Trip.RouteId <= 0)
                errors.Add("Маршрут обязателен");

            if (Trip.BusId <= 0)
                errors.Add("Автобус обязателен");

            if (Trip.DriverId <= 0)
                errors.Add("Водитель обязателен");

            if (Trip.ConductorId <= 0)
                errors.Add("Кондуктор обязателен");

            if (Trip.TripDate == default)
                errors.Add("Дата рейса обязательна");
            else if (!IsEditMode && Trip.TripDate < DateTime.Today)
                errors.Add("Нельзя создавать рейсы на прошедшую дату");

            if (Trip.ShiftTypeId <= 0)
                errors.Add("Тип смены обязателен");

            // Улучшенная валидация плановой выручки
            if (Trip.PlannedRevenue < 0)
                errors.Add("Плановая выручка не может быть отрицательной");
            else if (Trip.PlannedRevenue == 0)
                errors.Add("Плановая выручка должна быть больше 0");
            else if (Trip.PlannedRevenue > 1000000) // Ограничение на максимальную выручку
                errors.Add("Плановая выручка не может превышать 1 000 000 руб.");

            if (string.IsNullOrWhiteSpace(Trip.Status))
                errors.Add("Статус обязателен");

            // Проверка на доступность автобуса в выбранную дату
            if (!IsEditMode && Trip.BusId > 0 && Trip.TripDate != default)
            {
                var existingTrips = await _tripRepository.GetByDateAsync(Trip.TripDate);
                if (existingTrips.Any(t => t.BusId == Trip.BusId && t.Status != "отменен"))
                    errors.Add("Автобус уже занят в эту дату");
            }

            // Проверка на доступность водителя в выбранную дату
            if (!IsEditMode && Trip.DriverId > 0 && Trip.TripDate != default)
            {
                var existingTrips = await _tripRepository.GetByDateAsync(Trip.TripDate);
                if (existingTrips.Any(t => t.DriverId == Trip.DriverId && t.Status != "отменен"))
                    errors.Add("Водитель уже занят в эту дату");
            }

        UpdateErrors:
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Errors.Clear();
                foreach (var error in errors) Errors.Add(error);
                OnPropertyChanged(nameof(HasErrors));
                OnPropertyChanged(nameof(CanSave));
            });

            return !errors.Any();
        }

        private async Task SaveAsync()
        {
            if (!await ValidateAsync())
                return;

            try
            {
                if (IsEditMode)
                {
                    bool success = await _tripRepository.UpdateAsync(Trip);
                    if (success)
                    {
                        MessageBox.Show("Рейс успешно обновлен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        CloseAction?.Invoke(true);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить рейс", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    int newId = await _tripRepository.AddAsync(Trip);
                    if (newId > 0)
                    {
                        Trip.Id = newId;
                        MessageBox.Show("Рейс успешно добавлен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        CloseAction?.Invoke(true);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить рейс", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseAction?.Invoke(false);
        }
    }
}