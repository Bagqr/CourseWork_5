using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BusParkManagementSystem.Models
{
    public class Bus : INotifyPropertyChanged
    {
        private int _id;
        private int _inventoryNumber;
        private string _govPlate;
        private int _modelId;
        private int _stateId;
        private int _colorId;
        private string _engineNumber;
        private string _chasisNumber;
        private string _bodyNumber;
        private DateTime _manufacturerDate;
        private int _mileage;
        private DateTime _lastOverhaulDate;
        private int? _driverId;
        private string _modelName;
        private string _stateName;
        private string _colorName;
        private string _driverName;
        public string DisplayName => $"{GovPlate} ({ModelName})";

        public int Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        public int InventoryNumber
        {
            get => _inventoryNumber;
            set => SetField(ref _inventoryNumber, value);
        }

        public string GovPlate
        {
            get => _govPlate;
            set => SetField(ref _govPlate, value);
        }

        public int ModelId
        {
            get => _modelId;
            set => SetField(ref _modelId, value);
        }

        public int StateId
        {
            get => _stateId;
            set => SetField(ref _stateId, value);
        }

        public int ColorId
        {
            get => _colorId;
            set => SetField(ref _colorId, value);
        }

        public string EngineNumber
        {
            get => _engineNumber;
            set => SetField(ref _engineNumber, value);
        }

        public string ChasisNumber
        {
            get => _chasisNumber;
            set => SetField(ref _chasisNumber, value);
        }

        public string BodyNumber
        {
            get => _bodyNumber;
            set => SetField(ref _bodyNumber, value);
        }

        public DateTime ManufacturerDate
        {
            get => _manufacturerDate;
            set => SetField(ref _manufacturerDate, value);
        }

        public int Mileage
        {
            get => _mileage;
            set => SetField(ref _mileage, value);
        }

        public DateTime LastOverhaulDate
        {
            get => _lastOverhaulDate;
            set => SetField(ref _lastOverhaulDate, value);
        }

        public int? DriverId
        {
            get => _driverId;
            set => SetField(ref _driverId, value);
        }

        // Дополнительные свойства для отображения
        public string ModelName
        {
            get => _modelName;
            set => SetField(ref _modelName, value);
        }

        public string StateName
        {
            get => _stateName;
            set => SetField(ref _stateName, value);
        }

        public string ColorName
        {
            get => _colorName;
            set => SetField(ref _colorName, value);
        }

        public string DriverName
        {
            get => _driverName;
            set => SetField(ref _driverName, value);
        }
        public int? CurrentDriverId
        {
            get => _driverId;
            set => SetField(ref _driverId, value);
        }
        // Вычисляемые свойства
        public string Age => $"{(DateTime.Now.Year - ManufacturerDate.Year)} лет";
        public string Status => StateName ?? "Неизвестно";
        public string FormattedMileage => $"{Mileage:N0} км";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}