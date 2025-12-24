using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BusParkManagementSystem.Models;  // ← ВАЖНО: используем Models
using BusParkManagementSystem.Repositories;

namespace BusParkManagementSystem.ViewModels
{
    public class EmployeeEditViewModel : BaseViewModel
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILookupRepository _lookupRepository;  // Из Models
        private Employee _employee;
        private bool _isEditMode;
        private ObservableCollection<string> _errors;

        public Employee Employee
        {
            get => _employee;
            set
            {
                if (_employee != null)
                {
                    _employee.PropertyChanged -= Employee_PropertyChanged;
                }

                SetField(ref _employee, value);

                if (_employee != null)
                {
                    _employee.PropertyChanged += Employee_PropertyChanged;
                }

                // Запустить валидацию сразу
                _ = ValidateAndUpdateAsync();
            }
        }
        private async void Employee_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Запускать валидацию только для свойств, влияющих на валидацию
            if (e.PropertyName == nameof(Employee.FullName) ||
                e.PropertyName == nameof(Employee.Gender) ||
                e.PropertyName == nameof(Employee.BirthDate) ||
                e.PropertyName == nameof(Employee.PositionId) ||
                e.PropertyName == nameof(Employee.StreetId) ||
                e.PropertyName == nameof(Employee.Salary) ||
                e.PropertyName == nameof(Employee.House))
            {
                await ValidateAndUpdateAsync();
            }
        }
        private async Task ValidateAndUpdateAsync()
        {
            await ValidateAsync();
        }
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetField(ref _isEditMode, value);
        }

        public string WindowTitle => IsEditMode ? "Редактирование сотрудника" : "Добавление сотрудника";

        public ObservableCollection<Position> Positions { get; } = new ObservableCollection<Position>();
        public ObservableCollection<Street> Streets { get; } = new ObservableCollection<Street>();
        public ObservableCollection<PersonnelEventType> PersonnelEventTypes { get; } = new ObservableCollection<PersonnelEventType>();

        public ObservableCollection<string> Genders { get; } = new ObservableCollection<string> { "М", "Ж" };

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

        public EmployeeEditViewModel(IEmployeeRepository employeeRepository, ILookupRepository lookupRepository)
        {
            _employeeRepository = employeeRepository;
            _lookupRepository = lookupRepository;
            Errors = new ObservableCollection<string>();

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave);
            CancelCommand = new RelayCommand(_ => Cancel());

            _ = LoadLookupDataAsync();
        }

        private async Task LoadLookupDataAsync()
        {
            try
            {
                var positions = await _lookupRepository.GetPositionsAsync();
                var streets = await _lookupRepository.GetStreetsAsync();
                var personnelEventTypes = await _lookupRepository.GetPersonnelEventTypesAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Positions.Clear();
                    foreach (var position in positions) Positions.Add(position);

                    Streets.Clear();
                    foreach (var street in streets) Streets.Add(street);

                    PersonnelEventTypes.Clear();
                    foreach (var eventType in personnelEventTypes) PersonnelEventTypes.Add(eventType);
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

            if (string.IsNullOrWhiteSpace(Employee.FullName))
                errors.Add("ФИО обязательно");
            else if (Employee.FullName.Length < 5)
                errors.Add("ФИО должно содержать не менее 5 символов");

            if (string.IsNullOrWhiteSpace(Employee.Gender))
                errors.Add("Пол обязателен");

            if (Employee.BirthDate == default)
                errors.Add("Дата рождения обязательна");
            else if (Employee.BirthDate > DateTime.Now.AddYears(-18))
                errors.Add("Сотрудник должен быть старше 18 лет");

            if (Employee.PositionId <= 0)
                errors.Add("Должность обязательна");

            if (Employee.StreetId <= 0)
                errors.Add("Улица обязательна");

            if (Employee.Salary < 0)
                errors.Add("Оклад не может быть отрицательным");

            if (Employee.House <= 0)
                errors.Add("Номер дома должен быть положительным числом");

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
            if (!await ValidateAsync())
                return;

            try
            {
                if (IsEditMode)
                {
                    bool success = await _employeeRepository.UpdateAsync(Employee);
                    if (success)
                    {
                        MessageBox.Show("Сотрудник успешно обновлен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        CloseAction?.Invoke(true);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить сотрудника", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    int newId = await _employeeRepository.AddAsync(Employee);
                    if (newId > 0)
                    {
                        Employee.Id = newId;
                        MessageBox.Show("Сотрудник успешно добавлен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        CloseAction?.Invoke(true);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить сотрудника", "Ошибка",
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