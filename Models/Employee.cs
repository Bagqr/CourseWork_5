using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BusParkManagementSystem.Models
{
    public class Employee : INotifyPropertyChanged
    {
        private int _id;
        private string _fullName;
        private string _gender;
        private DateTime _birthDate;
        private int _streetId;
        private int _positionId;
        private decimal _salary;
        private int _house;
        private bool _isActive;
        private DateTime? _dismissalDate;
        private string _positionName;
        private string _streetName;
        private int _experienceYears;

        public int Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        public string FullName
        {
            get => _fullName;
            set => SetField(ref _fullName, value);
        }

        public string Gender
        {
            get => _gender;
            set => SetField(ref _gender, value);
        }

        public DateTime BirthDate
        {
            get => _birthDate;
            set => SetField(ref _birthDate, value);
        }

        public int StreetId
        {
            get => _streetId;
            set => SetField(ref _streetId, value);
        }

        public int PositionId
        {
            get => _positionId;
            set => SetField(ref _positionId, value);
        }

        public decimal Salary
        {
            get => _salary;
            set => SetField(ref _salary, value);
        }

        public int House
        {
            get => _house;
            set => SetField(ref _house, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetField(ref _isActive, value);
        }

        public DateTime? DismissalDate
        {
            get => _dismissalDate;
            set => SetField(ref _dismissalDate, value);
        }

        public string PositionName
        {
            get => _positionName;
            set => SetField(ref _positionName, value);
        }

        public string StreetName
        {
            get => _streetName;
            set => SetField(ref _streetName, value);
        }

        public int ExperienceYears
        {
            get => _experienceYears;
            set => SetField(ref _experienceYears, value);
        }

        // Вычисляемые свойства для View
        public string Status => IsActive ? "Активен" : "Уволен";
        public string Experience => $"{ExperienceYears} лет";
        public string FormattedSalary => Salary.ToString("N2") + " руб.";
        public string DisplayName => $"{FullName} ({PositionName})";

        public string ShortName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FullName))
                    return string.Empty;

                var parts = FullName.Split(' ');
                if (parts.Length >= 3)
                    return $"{parts[0]} {parts[1][0]}.{parts[2][0]}.";
                return FullName;
            }
        }

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