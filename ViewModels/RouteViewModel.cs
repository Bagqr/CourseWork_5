using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public class RouteViewModel : INotifyPropertyChanged
    {
        private readonly IRouteRepository _routeRepository;
        private ObservableCollection<Route> _routes;
        private Route _selectedRoute;
        private ObservableCollection<RouteSection> _routeStops; // Изменено с RouteStop на RouteSection
        private string _searchTerm;
        private bool _isLoading;

        public ObservableCollection<Route> Routes
        {
            get => _routes;
            set
            {
                _routes = value;
                OnPropertyChanged();
            }
        }

        public Route SelectedRoute
        {
            get => _selectedRoute;
            set
            {
                _selectedRoute = value;
                OnPropertyChanged();
                if (value != null)
                {
                    _ = LoadRouteStopsAsync(value.Id);
                }
            }
        }

        public ObservableCollection<RouteSection> RouteStops // Изменено
        {
            get => _routeStops;
            set
            {
                _routeStops = value;
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
                _ = SearchRoutesAsync();
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

        public ICommand AddRouteCommand { get; }
        public ICommand EditRouteCommand { get; }
        public ICommand DeleteRouteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ViewDetailsCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public RouteViewModel()
        {
            try
            {
                var dbContext = new DatabaseContext();
                _routeRepository = new RouteRepository(dbContext);

                Routes = new ObservableCollection<Route>();
                RouteStops = new ObservableCollection<RouteSection>(); // Изменено

                // Инициализация команд
                AddRouteCommand = new RelayCommand(AddRoute);
                EditRouteCommand = new RelayCommand(EditRoute, CanEditOrDelete);
                DeleteRouteCommand = new RelayCommand(DeleteRoute, CanEditOrDelete);
                RefreshCommand = new RelayCommand(Refresh);
                ViewDetailsCommand = new RelayCommand(ViewDetails, CanEditOrDelete);

                // Загрузка данных
                _ = LoadRoutesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации RouteViewModel: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadRoutesAsync()
        {
            try
            {
                IsLoading = true;
                var routes = await _routeRepository.GetAllAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Routes.Clear();
                    foreach (var route in routes)
                    {
                        Routes.Add(route);
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка при загрузке маршрутов: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadRouteStopsAsync(int routeId)
        {
            try
            {
                var stops = await _routeRepository.GetRouteStopsAsync(routeId);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    RouteStops.Clear();
                    foreach (var stop in stops)
                    {
                        RouteStops.Add(stop); 
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка при загрузке остановок: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private async Task SearchRoutesAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                await LoadRoutesAsync();
                return;
            }

            try
            {
                IsLoading = true;
                var routes = await _routeRepository.SearchAsync(SearchTerm);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Routes.Clear();
                    foreach (var route in routes)
                    {
                        Routes.Add(route);
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

        private async void AddRoute(object parameter)
        {
            try
            {
                var newRoute = new Route
                {
                    RouteNumber = 0,
                    TurnoverTime = "90",
                    FirstDepartureTime = "06:00",
                    LastDepartureTime = "22:00",
                    Interval = 15
                };

                var dialog = new RouteEditDialog();
                var viewModel = new RouteEditViewModel(
                    _routeRepository,
                    new LookupRepository(new DatabaseContext()))
                {
                    Route = newRoute,
                    IsEditMode = false,
                    CloseAction = result =>
                    {
                        dialog.DialogResult = result;
                        dialog.Close();
                    }
                };

                dialog.DataContext = viewModel;
                dialog.Owner = Application.Current.MainWindow;

                if (dialog.ShowDialog() == true)
                {
                    // Обновляем список
                    await LoadRoutesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении маршрута: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditRoute(object parameter)
        {
            if (SelectedRoute == null) return;

            try
            {
                // Загружаем полные данные маршрута
                var route = await _routeRepository.GetByIdAsync(SelectedRoute.Id);
                if (route == null)
                {
                    MessageBox.Show("Маршрут не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var dialog = new RouteEditDialog();
                var viewModel = new RouteEditViewModel(
                    _routeRepository,
                    new LookupRepository(new DatabaseContext()))
                {
                    Route = route,
                    IsEditMode = true,
                    CloseAction = result =>
                    {
                        dialog.DialogResult = result;
                        dialog.Close();
                    }
                };

                // ЗАГРУЖАЕМ ОСТАНОВКИ МАРШРУТА ИЗ БД
                await viewModel.LoadRouteStopsAsync(route.Id);

                dialog.DataContext = viewModel;
                dialog.Owner = Application.Current.MainWindow;

                if (dialog.ShowDialog() == true)
                {
                    // Обновляем список
                    await LoadRoutesAsync();

                    // Обновляем выбранный элемент
                    var updatedRoute = await _routeRepository.GetByIdAsync(route.Id);
                    if (updatedRoute != null)
                    {
                        SelectedRoute = updatedRoute;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании маршрута: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteRoute(object parameter)
        {
            if (SelectedRoute == null) return;

            try
            {
                // Проверяем, есть ли зависимости (рейсы)
                using (var connection = new DatabaseContext().GetConnection())
                {
                    var checkQuery = "SELECT COUNT(*) FROM рейс WHERE маршрут_id = @RouteId";
                    var tripCount = await connection.ExecuteScalarAsync<int>(checkQuery,
                        new { RouteId = SelectedRoute.Id });

                    if (tripCount > 0)
                    {
                        MessageBox.Show($"Невозможно удалить маршрут! Он используется в {tripCount} рейсах.\n\n" +
                                       "Сначала удалите связанные рейсы.",
                            "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить маршрут?\n\n" +
                    $"№ маршрута: {SelectedRoute.RouteNumber}\n\n" +
                    "Это действие нельзя отменить!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    IsLoading = true;
                    bool success = await _routeRepository.DeleteAsync(SelectedRoute.Id);

                    if (success)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            Routes.Remove(SelectedRoute);
                            SelectedRoute = null;
                            RouteStops.Clear();
                        });

                        MessageBox.Show("Маршрут успешно удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить маршрут", "Ошибка",
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
            await LoadRoutesAsync();
            SearchTerm = string.Empty;
            RouteStops.Clear();

            MessageBox.Show("Данные обновлены", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ViewDetails(object parameter)
        {
            if (SelectedRoute != null)
            {
                try
                {
                    int busesCount = await _routeRepository.GetBusesCountOnRouteAsync(SelectedRoute.Id);
                    decimal routeLength = await _routeRepository.GetRouteLengthAsync(SelectedRoute.Id);

                    MessageBox.Show($"Детали маршрута №{SelectedRoute.RouteNumber}:\n\n" +
                                   $"Количество автобусов: {busesCount}\n" +
                                   $"Протяженность: {routeLength} м\n" +
                                   $"Время оборота: {SelectedRoute.TurnoverTime} мин\n" +
                                   $"Расписание: {SelectedRoute.Schedule}\n" +
                                   $"Интервал: {SelectedRoute.Interval} мин",
                        "Детали маршрута", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при получении деталей: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanEditOrDelete(object parameter)
        {
            return SelectedRoute != null && !IsLoading;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}