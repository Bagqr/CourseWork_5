using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BusParkManagementSystem.Models;
using BusParkManagementSystem.Repositories;

namespace BusParkManagementSystem.ViewModels
{
    public class BusEditViewModel : BaseViewModel
    {
        private readonly IBusRepository _busRepository;
        private readonly ILookupRepository _lookupRepository;
        private Bus _bus;
        private bool _isEditMode;
        private ObservableCollection<string> _errors;
        private string _originalGovPlate;
        private int _originalInventoryNumber;
        private bool _isInitialized = false; // Флаг инициализации

        // Изменяем свойство Bus для автоматической подписки на изменения
        public Bus Bus
        {
            get => _bus;
            set
            {
                if (_bus != null)
                {
                    _bus.PropertyChanged -= Bus_PropertyChanged;
                }

                SetField(ref _bus, value);

                if (_bus != null)
                {
                    _bus.PropertyChanged += Bus_PropertyChanged;

                    // Сохраняем оригинальные значения ПРИ ИНИЦИАЛИЗАЦИИ
                    if (IsEditMode && _bus.Id > 0)
                    {
                        _originalGovPlate = _bus.GovPlate;
                        _originalInventoryNumber = _bus.InventoryNumber;

                        // Устанавливаем флаг инициализации
                        _isInitialized = true;
                    }
                    // Не запускаем валидацию сразу - ждем завершения инициализации
                }
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetField(ref _isEditMode, value);
        }

        public string WindowTitle => IsEditMode ? "Редактирование автобуса" : "Добавление автобуса";

        public ObservableCollection<Model> Models { get; } = new ObservableCollection<Model>();
        public ObservableCollection<BusState> BusStates { get; } = new ObservableCollection<BusState>();
        public ObservableCollection<Color> Colors { get; } = new ObservableCollection<Color>();
        public ObservableCollection<Employee> Drivers { get; } = new ObservableCollection<Employee>();

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

        public BusEditViewModel(IBusRepository busRepository, ILookupRepository lookupRepository)
        {
            _busRepository = busRepository;
            _lookupRepository = lookupRepository;
            Errors = new ObservableCollection<string>();

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave);
            CancelCommand = new RelayCommand(_ => Cancel());
        }
        public async Task InitializeAsync()
        {
            await LoadLookupDataAsync();
        }

        // Обработчик изменений свойств Bus для динамической валидации
        private async void Bus_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Если данные еще не инициализированы, не валидируем
            if (!_isInitialized && IsEditMode)
                return;

            // Запускать валидацию только для важных полей
            if (e.PropertyName == nameof(Bus.InventoryNumber) ||
                e.PropertyName == nameof(Bus.GovPlate) ||
                e.PropertyName == nameof(Bus.ModelId) ||
                e.PropertyName == nameof(Bus.StateId) ||
                e.PropertyName == nameof(Bus.ColorId) ||
                e.PropertyName == nameof(Bus.EngineNumber) ||
                e.PropertyName == nameof(Bus.ChasisNumber) ||
                e.PropertyName == nameof(Bus.BodyNumber) ||
                e.PropertyName == nameof(Bus.ManufacturerDate) ||
                e.PropertyName == nameof(Bus.Mileage) ||
                e.PropertyName == nameof(Bus.LastOverhaulDate))
            {
                await ValidateAndUpdateAsync();
            }
        }

        // Новый метод для валидации с обновлением UI
        private async Task ValidateAndUpdateAsync()
        {
            await ValidateAsync();
        }

        private async Task LoadLookupDataAsync()
        {
            try
            {
                var models = await _lookupRepository.GetModelsAsync();
                var states = await _lookupRepository.GetBusStatesAsync();
                var colors = await _lookupRepository.GetColorsAsync();
                var drivers = await _lookupRepository.GetActiveDriversAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Models.Clear();
                    foreach (var model in models) Models.Add(model);

                    BusStates.Clear();
                    foreach (var state in states) BusStates.Add(state);

                    Colors.Clear();
                    foreach (var color in colors) Colors.Add(color);

                    Drivers.Clear();
                    foreach (var employee in drivers) Drivers.Add(employee);

                    // Добавляем пустой элемент для водителя - используем Employee вместо Driver
                    Drivers.Insert(0, new Employee
                    {
                        Id = 0,
                        FullName = "(Не назначен)",
                        PositionName = "Водитель",
                        IsActive = true,
                        Gender = "М",
                        BirthDate = DateTime.Now, 
                        StreetId = 0,
                        PositionId = 0,
                        Salary = 0,
                        House = 0,
                        DismissalDate = null,
                        StreetName = "",
                        ExperienceYears = 0
                    });
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

            // 1. Проверка обязательных полей
            if (Bus.InventoryNumber <= 0)
                errors.Add("Инвентарный номер должен быть положительным числом");

            if (string.IsNullOrWhiteSpace(Bus.GovPlate))
                errors.Add("Государственный номер обязателен");
            else if (Bus.GovPlate.Length < 6)
                errors.Add("Государственный номер должен содержать не менее 6 символов");

            if (Bus.ModelId <= 0)
                errors.Add("Модель обязательна");

            if (Bus.StateId <= 0)
                errors.Add("Состояние обязательно");

            if (Bus.ColorId <= 0)
                errors.Add("Цвет обязателен");

            if (string.IsNullOrWhiteSpace(Bus.EngineNumber))
                errors.Add("Номер двигателя обязателен");

            if (string.IsNullOrWhiteSpace(Bus.ChasisNumber))
                errors.Add("Номер шасси обязателен");

            if (string.IsNullOrWhiteSpace(Bus.BodyNumber))
                errors.Add("Номер кузова обязателен");

            if (Bus.ManufacturerDate == default)
                errors.Add("Дата выпуска обязательна");
            else if (Bus.ManufacturerDate > DateTime.Now)
                errors.Add("Дата выпуска не может быть в будущем");

            if (Bus.Mileage < 0)
                errors.Add("Пробег не может быть отрицательным");

            if (Bus.LastOverhaulDate == default)
                errors.Add("Дата капремонта обязательна");
            else if (Bus.LastOverhaulDate > DateTime.Now)
                errors.Add("Дата капремонта не может быть в будущем");

            // 2. Проверка уникальности (только если нет других ошибок)
            if (!errors.Any())
            {
                // Для нового автобуса
                if (!IsEditMode)
                {
                    // Проверяем госномер
                    if (!string.IsNullOrWhiteSpace(Bus.GovPlate))
                    {
                        bool isGovPlateUnique = await _busRepository.IsGovPlateUniqueAsync(
                            Bus.GovPlate, null);

                        if (!isGovPlateUnique)
                            errors.Add($"Государственный номер '{Bus.GovPlate}' уже существует");
                    }

                    // Проверяем инвентарный номер
                    bool isInventoryNumberUnique = await _busRepository.IsInventoryNumberUniqueAsync(
                        Bus.InventoryNumber, null);

                    if (!isInventoryNumberUnique)
                        errors.Add($"Инвентарный номер '{Bus.InventoryNumber}' уже существует");
                }
                // Для редактирования
                else if (IsEditMode && Bus.Id > 0)
                {
                    // Проверяем, изменился ли госномер
                    if (Bus.GovPlate != _originalGovPlate)
                    {
                        bool isGovPlateUnique = await _busRepository.IsGovPlateUniqueAsync(
                            Bus.GovPlate, Bus.Id);

                        if (!isGovPlateUnique)
                            errors.Add($"Государственный номер '{Bus.GovPlate}' уже существует");
                    }

                    // Проверяем, изменился ли инвентарный номер
                    if (Bus.InventoryNumber != _originalInventoryNumber)
                    {
                        bool isInventoryNumberUnique = await _busRepository.IsInventoryNumberUniqueAsync(
                            Bus.InventoryNumber, Bus.Id);

                        if (!isInventoryNumberUnique)
                            errors.Add($"Инвентарный номер '{Bus.InventoryNumber}' уже существует");
                    }
                }
            }

            // Обновляем UI
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
                    bool success = await _busRepository.UpdateAsync(Bus);
                    if (success)
                    {
                        MessageBox.Show("Автобус успешно обновлен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        CloseAction?.Invoke(true);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить автобус", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    int newId = await _busRepository.AddAsync(Bus);
                    if (newId > 0)
                    {
                        Bus.Id = newId;
                        MessageBox.Show("Автобус успешно добавлен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        CloseAction?.Invoke(true);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить автобус", "Ошибка",
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

        // Убираем старый метод SubscribeToPropertyChanges, так как теперь подписка происходит автоматически
        // через свойство Bus
    }
}