using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BusParkManagementSystem.Data;
using BusParkManagementSystem.Models;
using BusParkManagementSystem.Repositories;
using Dapper;

namespace BusParkManagementSystem.ViewModels
{
    public class RouteEditViewModel : BaseViewModel
    {
        private readonly IRouteRepository _routeRepository;
        private readonly ILookupRepository _lookupRepository;
        private readonly DatabaseContext _dbContext;
        private Route _route;
        private bool _isEditMode;
        private ObservableCollection<string> _errors;
        private ObservableCollection<RouteSection> _routeStops;
        private RouteSection _selectedRouteStop;
        private ObservableCollection<BusStop> _availableStops;
        private int _selectedStopId;
        private string _stopDistance;
        private BusStop _selectedBusStop;   
        public Route Route
        {
            get => _route;
            set
            {
                if (_route != null)
                {
                    _route.PropertyChanged -= Route_PropertyChanged;
                }

                SetField(ref _route, value);

                if (_route != null)
                {
                    _route.PropertyChanged += Route_PropertyChanged;
                }

                _ = ValidateAndUpdateAsync();
            }
        }
        public BusStop SelectedBusStop
        {
            get => _selectedBusStop;
            set
            {
                SetField(ref _selectedBusStop, value);
                if (value != null)
                {
                    SelectedStopId = value.Id; // Обновляем ID
                }
            }
        }
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetField(ref _isEditMode, value);
        }

        public string WindowTitle => IsEditMode ? "Редактирование маршрута" : "Добавление маршрута";

        public ObservableCollection<RouteSection> RouteStops
        {
            get => _routeStops;
            set => SetField(ref _routeStops, value);
        }

        public RouteSection SelectedRouteStop
        {
            get => _selectedRouteStop;
            set
            {
                SetField(ref _selectedRouteStop, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ObservableCollection<BusStop> AvailableStops
        {
            get => _availableStops;
            set => SetField(ref _availableStops, value);
        }

        public int SelectedStopId
        {
            get => _selectedStopId;
            set => SetField(ref _selectedStopId, value);
        }

        public string StopDistance
        {
            get => _stopDistance;
            set => SetField(ref _stopDistance, value);
        }

        public ObservableCollection<string> Errors
        {
            get => _errors;
            set => SetField(ref _errors, value);
        }

        public bool HasErrors => Errors?.Count > 0;
        public bool CanSave => !HasErrors && ValidateRouteStops();

        // Основные команды
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Команды для остановок
        public ICommand AddStopCommand { get; }
        public ICommand RemoveStopCommand { get; }
        public ICommand MoveStopUpCommand { get; }
        public ICommand MoveStopDownCommand { get; }

        // Команды для загрузки данных
        public ICommand LoadStopsCommand { get; }

        public Action<bool?> CloseAction { get; set; }

        public RouteEditViewModel(IRouteRepository routeRepository, ILookupRepository lookupRepository)
        {
            _routeRepository = routeRepository;
            _lookupRepository = lookupRepository;
            _dbContext = new DatabaseContext();

            Errors = new ObservableCollection<string>();
            RouteStops = new ObservableCollection<RouteSection>();
            AvailableStops = new ObservableCollection<BusStop>();

            // Инициализация команд
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave);
            CancelCommand = new RelayCommand(_ => Cancel());

            // Команды для остановок
            AddStopCommand = new RelayCommand(_ => AddStop());
            RemoveStopCommand = new RelayCommand(_ => RemoveStop(), _ => SelectedRouteStop != null);
            MoveStopUpCommand = new RelayCommand(_ => MoveStopUp(), _ => CanMoveStopUp());
            MoveStopDownCommand = new RelayCommand(_ => MoveStopDown(), _ => CanMoveStopDown());
            LoadStopsCommand = new RelayCommand(async _ => await LoadAvailableStopsAsync());

            // Загружаем доступные остановки при создании ViewModel
            _ = LoadAvailableStopsAsync();
        }

        private async void Route_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Route.RouteNumber) ||
                e.PropertyName == nameof(Route.TurnoverTime) ||
                e.PropertyName == nameof(Route.FirstDepartureTime) ||
                e.PropertyName == nameof(Route.LastDepartureTime) ||
                e.PropertyName == nameof(Route.Interval))
            {
                await ValidateAndUpdateAsync();
            }
        }

        private async Task ValidateAndUpdateAsync()
        {
            await ValidateAsync();
        }

        private async Task LoadAvailableStopsAsync()
        {
            try
            {
                var stops = await _lookupRepository.GetStopsAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    AvailableStops.Clear();

                    // Добавляем пустой элемент для выбора
                    AvailableStops.Add(new BusStop { Id = 0, Name = "-- Выберите остановку --" });

                    foreach (var stop in stops)
                    {
                        AvailableStops.Add(stop);
                    }

                    // Устанавливаем первый элемент по умолчанию
                    if (AvailableStops.Count > 0)
                    {
                        SelectedStopId = 0;
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки остановок из базы данных: {ex.Message}\n\n" +
                                  $"Детали: {ex.InnerException?.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        public async Task LoadRouteStopsAsync(int routeId)
        {
            try
            {
                // Используем прямой SQL-запрос для загрузки остановок маршрута
                using (var connection = _dbContext.GetConnection())
                {
                    var query = @"
                        SELECT 
                            um.id as Id,
                            um.route_id as RouteId,
                            um.stop_id as StopId,
                            um.[order] as [Order],
                            um.distance_from_start as DistanceFromStart,
                            o.name as StopName
                        FROM участок_маршрута um
                        LEFT JOIN остановка o ON um.stop_id = o.id
                        WHERE um.route_id = @RouteId
                        ORDER BY um.[order]";

                    var stops = await connection.QueryAsync<RouteSection>(query, new { RouteId = routeId });

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        RouteStops.Clear();
                        foreach (var stop in stops)
                        {
                            RouteStops.Add(stop);
                        }

                        // Выводим информацию в консоль для отладки
                        Console.WriteLine($"Загружено остановок маршрута: {RouteStops.Count}");
                    });
                }
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки остановок маршрута: {ex.Message}\n\n" +
                                  $"Детали: {ex.InnerException?.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void AddStop()
        {
            MessageBox.Show($"Выбрана остановка с ID: {SelectedStopId}\n" +
                 $"Количество остановок в списке: {AvailableStops.Count}",
                 "Отладка", MessageBoxButton.OK, MessageBoxImage.Information);
            if (SelectedStopId <= 0)
            {
                MessageBox.Show("Выберите остановку из списка", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(StopDistance))
            {
                MessageBox.Show("Укажите расстояние от начала маршрута", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(StopDistance, out int distance) || distance < 0)
            {
                MessageBox.Show("Расстояние должно быть положительным числом", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка, что остановка еще не добавлена
            if (RouteStops.Any(s => s.StopId == SelectedStopId))
            {
                MessageBox.Show("Эта остановка уже добавлена в маршрут", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedStop = AvailableStops.FirstOrDefault(s => s.Id == SelectedStopId);
            if (selectedStop == null) return;

            var newStop = new RouteSection
            {
                Id = 0, // Новый ID будет сгенерирован при сохранении
                RouteId = Route?.Id ?? 0,
                StopId = SelectedStopId,
                StopName = selectedStop.Name,
                Order = RouteStops.Count + 1,
                DistanceFromStart = distance
            };

            RouteStops.Add(newStop);
            SelectedRouteStop = newStop;

            // Сброс формы и обновление UI
            SelectedStopId = 0;
            StopDistance = string.Empty;
            OnPropertyChanged(nameof(CanSave));

            // Уведомление команд о необходимости перепроверить CanExecute
            CommandManager.InvalidateRequerySuggested();

            MessageBox.Show($"Остановка '{selectedStop.Name}' добавлена в маршрут",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RemoveStop()
        {
            if (SelectedRouteStop == null) return;

            var result = MessageBox.Show(
                $"Удалить остановку '{SelectedRouteStop.StopName}' из маршрута?\n\n" +
                $"Порядок: {SelectedRouteStop.Order}\n" +
                $"Расстояние: {SelectedRouteStop.DistanceFromStart} м",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int removedOrder = SelectedRouteStop.Order;
                RouteStops.Remove(SelectedRouteStop);
                SelectedRouteStop = null;

                // Пересчет порядка оставшихся остановок
                int order = 1;
                foreach (var stop in RouteStops.OrderBy(s => s.Order))
                {
                    stop.Order = order++;
                }

                OnPropertyChanged(nameof(CanSave));
                CommandManager.InvalidateRequerySuggested();

                MessageBox.Show("Остановка удалена из маршрута",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanMoveStopUp()
        {
            if (SelectedRouteStop == null) return false;
            var index = RouteStops.IndexOf(SelectedRouteStop);
            return index > 0 && RouteStops.Count > 1;
        }

        private void MoveStopUp()
        {
            if (!CanMoveStopUp()) return;

            var currentIndex = RouteStops.IndexOf(SelectedRouteStop);
            var newIndex = currentIndex - 1;

            // Меняем порядок
            var tempOrder = RouteStops[currentIndex].Order;
            RouteStops[currentIndex].Order = RouteStops[newIndex].Order;
            RouteStops[newIndex].Order = tempOrder;

            // Меняем позиции в коллекции
            RouteStops.Move(currentIndex, newIndex);

            // Обновляем выбранный элемент
            SelectedRouteStop = RouteStops[newIndex];

            OnPropertyChanged(nameof(CanSave));
            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanMoveStopDown()
        {
            if (SelectedRouteStop == null) return false;
            var index = RouteStops.IndexOf(SelectedRouteStop);
            return index >= 0 && index < RouteStops.Count - 1;
        }

        private void MoveStopDown()
        {
            if (!CanMoveStopDown()) return;

            var currentIndex = RouteStops.IndexOf(SelectedRouteStop);
            var newIndex = currentIndex + 1;

            // Меняем порядок
            var tempOrder = RouteStops[currentIndex].Order;
            RouteStops[currentIndex].Order = RouteStops[newIndex].Order;
            RouteStops[newIndex].Order = tempOrder;

            // Меняем позиции в коллекции
            RouteStops.Move(currentIndex, newIndex);

            // Обновляем выбранный элемент
            SelectedRouteStop = RouteStops[newIndex];

            OnPropertyChanged(nameof(CanSave));
            CommandManager.InvalidateRequerySuggested();
        }

        private bool ValidateRouteStops()
        {

            // Проверка уникальности остановок
            var stopIds = RouteStops.Select(s => s.StopId).Distinct();
            if (stopIds.Count() != RouteStops.Count)
            {
                return false;
            }

            // Проверка расстояний (должны возрастать)
            for (int i = 1; i < RouteStops.Count; i++)
            {
                if (RouteStops[i].DistanceFromStart <= RouteStops[i - 1].DistanceFromStart)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> ValidateAsync()
        {
            var errors = new List<string>();

            if (Route.RouteNumber <= 0)
                errors.Add("Номер маршрута должен быть положительным числом");

            if (string.IsNullOrWhiteSpace(Route.TurnoverTime))
                errors.Add("Время оборота обязательно");
            else if (!int.TryParse(Route.TurnoverTime, out int turnover) || turnover <= 0)
                errors.Add("Время оборота должно быть положительным числом");

            if (string.IsNullOrWhiteSpace(Route.FirstDepartureTime))
                errors.Add("Время первого отправления обязательно");
            else if (!TimeSpan.TryParse(Route.FirstDepartureTime, out TimeSpan firstDeparture))
                errors.Add("Время первого отправления должно быть в формате ЧЧ:ММ");

            if (string.IsNullOrWhiteSpace(Route.LastDepartureTime))
                errors.Add("Время последнего отправления обязательно");
            else if (!TimeSpan.TryParse(Route.LastDepartureTime, out TimeSpan lastDeparture))
                errors.Add("Время последнего отправления должно быть в формате ЧЧ:ММ");

            if (Route.Interval <= 0)
                errors.Add("Интервал должен быть положительным числом");

            // Проверка времени: первое отправление должно быть раньше последнего
            if (TimeSpan.TryParse(Route.FirstDepartureTime, out TimeSpan first) &&
                TimeSpan.TryParse(Route.LastDepartureTime, out TimeSpan last) &&
                first >= last)
            {
                errors.Add("Время первого отправления должно быть раньше времени последнего отправления");
            }

            // Проверка уникальности номера маршрута (если номер изменился)
            if (IsEditMode || Route.Id == 0)
            {
                bool isRouteNumberUnique = await _routeRepository.IsRouteNumberUniqueAsync(
                    Route.RouteNumber, IsEditMode ? Route.Id : (int?)null);
                if (!isRouteNumberUnique)
                    errors.Add($"Маршрут с номером {Route.RouteNumber} уже существует");
            }

           
            // Обновляем Errors в UI потоке
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
            if (!await ValidateAsync() || !ValidateRouteStops())
                return;

            try
            {
                int routeId;

                if (IsEditMode)
                {
                    bool success = await _routeRepository.UpdateAsync(Route);
                    if (!success)
                    {
                        MessageBox.Show("Не удалось обновить маршрут", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    routeId = Route.Id;
                }
                else
                {
                    routeId = await _routeRepository.AddAsync(Route);
                    if (routeId <= 0)
                    {
                        MessageBox.Show("Не удалось добавить маршрут", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    Route.Id = routeId;
                }

                // Сохранение остановок маршрута
                await SaveRouteStopsAsync(routeId);

                MessageBox.Show(IsEditMode ? "Маршрут успешно обновлен" : "Маршрут успешно добавлен",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}\n\nДетали: {ex.InnerException?.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveRouteStopsAsync(int routeId)
        {
            using (var connection = _dbContext.GetConnection())
            {
                // Начинаем транзакцию
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Удаляем старые остановки маршрута
                        var deleteQuery = "DELETE FROM участок_маршрута WHERE route_id = @RouteId";
                        await connection.ExecuteAsync(deleteQuery, new { RouteId = routeId }, transaction);

                        // Добавляем новые остановки
                        foreach (var stop in RouteStops)
                        {
                            var insertQuery = @"
                                INSERT INTO участок_маршрута (route_id, stop_id, [order], distance_from_start)
                                VALUES (@RouteId, @StopId, @Order, @DistanceFromStart)";

                            await connection.ExecuteAsync(insertQuery, new
                            {
                                RouteId = routeId,
                                StopId = stop.StopId,
                                Order = stop.Order,
                                DistanceFromStart = stop.DistanceFromStart
                            }, transaction);
                        }

                        // Сохраняем график движения
                        await SaveScheduleAsync(routeId, connection, transaction);

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private async Task SaveScheduleAsync(int routeId, IDbConnection connection, IDbTransaction transaction)
        {
            // Проверяем, есть ли уже график для маршрута
            var checkQuery = "SELECT COUNT(*) FROM график_движения WHERE route_id = @RouteId";
            var exists = await connection.ExecuteScalarAsync<int>(checkQuery,
                new { RouteId = routeId }, transaction) > 0;

            if (exists)
            {
                // Обновляем существующий график
                var updateQuery = @"
                    UPDATE график_движения SET
                        first_departure_time = @FirstDepartureTime,
                        last_departure_time = @LastDepartureTime,
                        intervals = @Interval
                    WHERE route_id = @RouteId";

                await connection.ExecuteAsync(updateQuery, new
                {
                    RouteId = routeId,
                    FirstDepartureTime = Route.FirstDepartureTime,
                    LastDepartureTime = Route.LastDepartureTime,
                    Interval = Route.Interval
                }, transaction);
            }
            else
            {
                // Создаем новый график
                var insertQuery = @"
                    INSERT INTO график_движения (route_id, first_departure_time, last_departure_time, intervals)
                    VALUES (@RouteId, @FirstDepartureTime, @LastDepartureTime, @Interval)";

                await connection.ExecuteAsync(insertQuery, new
                {
                    RouteId = routeId,
                    FirstDepartureTime = Route.FirstDepartureTime,
                    LastDepartureTime = Route.LastDepartureTime,
                    Interval = Route.Interval
                }, transaction);
            }
        }

        private void Cancel()
        {
            CloseAction?.Invoke(false);
        }
    }
}