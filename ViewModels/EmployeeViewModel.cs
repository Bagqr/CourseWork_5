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

namespace BusParkManagementSystem.ViewModels
{
    public class EmployeeViewModel : INotifyPropertyChanged
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILookupRepository _lookupRepository;  
        private ObservableCollection<Employee> _employees;
        private Employee _selectedEmployee;
        private string _searchTerm;
        private string _selectedPositionFilter;
        private string _selectedStatusFilter;
        private bool _isLoading;

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set
            {
                _employees = value;
                OnPropertyChanged();
            }
        }

        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
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
                _ = SearchEmployeesAsync();
            }
        }

        public string SelectedPositionFilter
        {
            get => _selectedPositionFilter;
            set
            {
                _selectedPositionFilter = value;
                OnPropertyChanged();
                _ = FilterEmployeesAsync();
            }
        }

        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                _selectedStatusFilter = value;
                OnPropertyChanged();
                _ = FilterEmployeesAsync();
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

        public ObservableCollection<string> PositionFilters { get; }
        public ObservableCollection<string> StatusFilters { get; }

        public ICommand AddEmployeeCommand { get; }
        public ICommand EditEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }
        public ICommand DismissEmployeeCommand { get; }
        public ICommand CalculateSalaryCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand CheckDatabaseCommand { get; }


        public event PropertyChangedEventHandler PropertyChanged;

        public EmployeeViewModel()
        {
            try
            {
                var dbContext = new DatabaseContext();
                _employeeRepository = new EmployeeRepository(dbContext);
                _lookupRepository = new LookupRepository(dbContext);
                Employees = new ObservableCollection<Employee>();

                // Инициализация фильтров
                PositionFilters = new ObservableCollection<string>
                {
                    "Все должности",
                    "Водитель",
                    "Кондуктор",
                    "Директор",
                    "Диспетчер",
                    "Бухгалтер",
                    "Инженер гаража"
                };

                StatusFilters = new ObservableCollection<string>
                {
                    "Все",
                    "Активен",
                    "Уволен"
                };

                SelectedPositionFilter = "Все должности";
                SelectedStatusFilter = "Все";

                // Инициализация команд
                AddEmployeeCommand = new RelayCommand(AddEmployee);
                EditEmployeeCommand = new RelayCommand(EditEmployee, CanEditOrDelete);
                DeleteEmployeeCommand = new RelayCommand(DeleteEmployee, CanEditOrDelete);
                DismissEmployeeCommand = new RelayCommand(DismissEmployee, CanEditOrDelete);
                CalculateSalaryCommand = new RelayCommand(CalculateSalary, CanEditOrDelete);
                RefreshCommand = new RelayCommand(Refresh);
                ClearFiltersCommand = new RelayCommand(ClearFilters);
                CheckDatabaseCommand = new RelayCommand(CheckDatabase);

                // Загрузка данных
                _ = LoadEmployeesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации EmployeeViewModel: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadEmployeesAsync(bool forceReload = false)
        {
            try
            {
                IsLoading = true;

                var employees = await _employeeRepository.GetAllAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // Если это не принудительная перезагрузка, обновляем по одному
                    if (!forceReload && Employees.Count > 0)
                    {
                        // Обновляем существующие и добавляем новые
                        foreach (var employee in employees)
                        {
                            var existing = Employees.FirstOrDefault(e => e.Id == employee.Id);
                            if (existing != null)
                            {
                                // Обновляем свойства
                                existing.FullName = employee.FullName;
                                existing.Gender = employee.Gender;
                                existing.BirthDate = employee.BirthDate;
                                existing.StreetId = employee.StreetId;
                                existing.PositionId = employee.PositionId;
                                existing.Salary = employee.Salary;
                                existing.House = employee.House;
                                existing.IsActive = employee.IsActive;
                                existing.DismissalDate = employee.DismissalDate;
                                existing.PositionName = employee.PositionName;
                                existing.StreetName = employee.StreetName;
                                existing.ExperienceYears = employee.ExperienceYears;
                            }
                            else
                            {
                                Employees.Add(employee);
                            }
                        }

                        // Удаляем удаленных
                        var idsToRemove = Employees
                            .Where(e => !employees.Any(emp => emp.Id == e.Id))
                            .Select(e => e.Id)
                            .ToList();

                        foreach (var id in idsToRemove)
                        {
                            var toRemove = Employees.FirstOrDefault(e => e.Id == id);
                            if (toRemove != null)
                                Employees.Remove(toRemove);
                        }
                    }
                    else
                    {
                        // Полная перезагрузка
                        Employees.Clear();
                        foreach (var employee in employees)
                        {
                            Employees.Add(employee);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка при загрузке сотрудников: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchEmployeesAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                await FilterEmployeesAsync();
                return;
            }

            try
            {
                IsLoading = true;
                var employees = await _employeeRepository.SearchAsync(SearchTerm);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Employees.Clear();
                    foreach (var employee in employees)
                    {
                        Employees.Add(employee);
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

        private async Task FilterEmployeesAsync()
        {
            try
            {
                IsLoading = true;
                var allEmployees = await _employeeRepository.GetAllAsync();

                var filteredEmployees = allEmployees.AsEnumerable();

                // Фильтр по должности
                if (SelectedPositionFilter != "Все должности")
                {
                    filteredEmployees = filteredEmployees.Where(e => e.PositionName == SelectedPositionFilter);
                }

                // Фильтр по статусу
                if (SelectedStatusFilter != "Все")
                {
                    bool isActive = SelectedStatusFilter == "Активен";
                    filteredEmployees = filteredEmployees.Where(e => e.IsActive == isActive);
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Employees.Clear();
                    foreach (var employee in filteredEmployees)
                    {
                        Employees.Add(employee);
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

        private void AddEmployee(object parameter)
        {
            try
            {
                var newEmployee = new Employee
                {
                    BirthDate = DateTime.Now.AddYears(-25),
                    Salary = 30000,
                    House = 1,
                    IsActive = true,
                    ExperienceYears = 0
                };

                var dialog = new EmployeeEditDialog();
                var viewModel = new EmployeeEditViewModel(_employeeRepository, _lookupRepository)
                {
                    Employee = newEmployee,
                    IsEditMode = false,
                    CloseAction = async result =>
                    {
                        dialog.DialogResult = result;
                        dialog.Close();

                        // Обновить список после закрытия диалога
                        if (result == true)
                        {
                            await LoadEmployeesAsync();
                        }
                    }
                };

                dialog.DataContext = viewModel;
                dialog.Owner = Application.Current.MainWindow;

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении сотрудника: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditEmployee(object parameter)
        {
            if (SelectedEmployee == null) return;

            try
            {
                // Создать копию сотрудника для редактирования
                var employeeCopy = new Employee
                {
                    Id = SelectedEmployee.Id,
                    FullName = SelectedEmployee.FullName,
                    Gender = SelectedEmployee.Gender,
                    BirthDate = SelectedEmployee.BirthDate,
                    StreetId = SelectedEmployee.StreetId,
                    PositionId = SelectedEmployee.PositionId,
                    Salary = SelectedEmployee.Salary,
                    House = SelectedEmployee.House,
                    IsActive = SelectedEmployee.IsActive,
                    DismissalDate = SelectedEmployee.DismissalDate,
                    PositionName = SelectedEmployee.PositionName,
                    StreetName = SelectedEmployee.StreetName,
                    ExperienceYears = SelectedEmployee.ExperienceYears
                };

                var dialog = new EmployeeEditDialog();
                var viewModel = new EmployeeEditViewModel(_employeeRepository, _lookupRepository)
                {
                    Employee = employeeCopy,
                    IsEditMode = true,
                    CloseAction = async result =>
                    {
                        dialog.DialogResult = result;
                        dialog.Close();

                        // Обновить список после закрытия диалога
                        if (result == true)
                        {
                           // await LoadEmployeesAsync();
                            await RefreshSelectedEmployeeAsync();
                        }
                    }
                };

                dialog.DataContext = viewModel;
                dialog.Owner = Application.Current.MainWindow;

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании сотрудника: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }   
        }
        private async Task RefreshSelectedEmployeeAsync()
        {
            if (SelectedEmployee != null)
            {
                var updated = await _employeeRepository.GetByIdAsync(SelectedEmployee.Id);
                if (updated != null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var index = Employees.IndexOf(SelectedEmployee);
                        if (index >= 0)
                        {
                            Employees[index] = updated;
                            SelectedEmployee = updated;
                        }
                    });
                }
            }
        }
        private async void DeleteEmployee(object parameter)
        {
            if (SelectedEmployee != null)
            {
                // Проверяем, не активен ли сотрудник
                if (SelectedEmployee.IsActive)
                {
                    MessageBox.Show("Нельзя удалить активного сотрудника. Сначала увольте его.",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем, есть ли связанные записи (например, в истории рейсов)
                var result = MessageBox.Show(
                    $"Удалить сотрудника {SelectedEmployee.ShortName} из базы данных?\n\n" +
                    "Внимание: Это действие нельзя отменить!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        IsLoading = true;
                        bool success = await _employeeRepository.DeleteAsync(SelectedEmployee.Id);

                        if (success)
                        {
                            // Удаляем из ObservableCollection
                            await Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                Employees.Remove(SelectedEmployee);
                                SelectedEmployee = null;
                            });

                            MessageBox.Show("Сотрудник успешно удален", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить сотрудника. Возможно, есть связанные записи.",
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

        private async void DismissEmployee(object parameter)
        {
            if (SelectedEmployee != null && SelectedEmployee.IsActive)
            {
                // Диалог для ввода причины увольнения
                var dialog = new DismissalDialog();
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        IsLoading = true;
                        bool success = await _employeeRepository.DismissAsync(
                            SelectedEmployee.Id,
                            DateTime.Now,
                            dialog.Reason);

                        if (success)
                        {
                            // Обновляем данные через репозиторий
                            await RefreshSelectedEmployeeAsync();

                            MessageBox.Show("Сотрудник успешно уволен", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при увольнении: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
            }
        }

        private async void CalculateSalary(object parameter)
        {
            if (SelectedEmployee != null)
            {
                try
                {
                    IsLoading = true;
                    decimal salary = await _employeeRepository.CalculateSalaryAsync(
                        SelectedEmployee.Id, DateTime.Now.Month, DateTime.Now.Year);

                    MessageBox.Show($"Расчетная зарплата для {SelectedEmployee.ShortName}:\n\n" +
                                   $"Базовая: {SelectedEmployee.Salary:N2} руб.\n" +
                                   $"С премией: {salary:N2} руб.",
                        "Расчет зарплаты", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при расчете зарплаты: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async void Refresh(object parameter)
        {
            await LoadEmployeesAsync();
            SearchTerm = string.Empty;
            SelectedPositionFilter = "Все должности";
            SelectedStatusFilter = "Все";

            MessageBox.Show("Данные обновлены", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearFilters(object parameter)
        {
            SearchTerm = string.Empty;
            SelectedPositionFilter = "Все должности";
            SelectedStatusFilter = "Все";
        }

        private bool CanEditOrDelete(object parameter)
        {
            return SelectedEmployee != null && !IsLoading;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void CheckDatabase(object parameter)
        {
            try
            {
                var dbContext = new DatabaseContext();
                var info = dbContext.GetDatabaseInfo();

                MessageBox.Show(info, "Информация о базе данных",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}