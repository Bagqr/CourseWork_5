// Models\Route.cs - добавляем INotifyPropertyChanged
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BusParkManagementSystem.Models
{
    public class Route : INotifyPropertyChanged
    {
        private int _id;
        private int _routeNumber;
        private string _turnoverTime;
        private string _firstDepartureTime;
        private string _lastDepartureTime;
        private int _interval;
        public int StopsCount { get; set; }
        public decimal TotalLength { get; set; }
        public int Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        public int RouteNumber
        {
            get => _routeNumber;
            set => SetField(ref _routeNumber, value);
        }

        public string TurnoverTime
        {
            get => _turnoverTime;
            set => SetField(ref _turnoverTime, value);
        }

        public string FirstDepartureTime
        {
            get => _firstDepartureTime;
            set => SetField(ref _firstDepartureTime, value);
        }

        public string LastDepartureTime
        {
            get => _lastDepartureTime;
            set => SetField(ref _lastDepartureTime, value);
        }

        public int Interval
        {
            get => _interval;
            set => SetField(ref _interval, value);
        }

        // Вычисляемые свойства
        public string DisplayName => $"Маршрут №{RouteNumber}";
        public string RouteInfo => $"Маршрут №{RouteNumber}";
        public string Schedule => $"{FirstDepartureTime} - {LastDepartureTime}";
        public string IntervalInfo => $"Интервал: {Interval} мин";

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