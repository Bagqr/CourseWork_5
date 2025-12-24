using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BusParkManagementSystem.Models
{
    public class Trip : INotifyPropertyChanged
    {
        private int _id;
        private int _routeId;
        private int _busId;
        private int _driverId;
        private int _conductorId;
        private DateTime _tripDate;
        private int _shiftTypeId;
        private decimal _plannedRevenue;
        private string _status;
        private string _cancellationReason;
        private string _routeNumber;
        private string _busGovPlate;
        private string _busModel;
        private string _driverName;
        private string _conductorName;
        private string _shiftName;
        private decimal _actualRevenue;
        private int _ticketsSold;

        public int Id { get => _id; set => SetField(ref _id, value); }
        public int RouteId { get => _routeId; set => SetField(ref _routeId, value); }
        public int BusId { get => _busId; set => SetField(ref _busId, value); }
        public int DriverId { get => _driverId; set => SetField(ref _driverId, value); }
        public int ConductorId { get => _conductorId; set => SetField(ref _conductorId, value); }
        public DateTime TripDate { get => _tripDate; set => SetField(ref _tripDate, value); }
        public int ShiftTypeId { get => _shiftTypeId; set => SetField(ref _shiftTypeId, value); }
        public decimal PlannedRevenue { get => _plannedRevenue; set => SetField(ref _plannedRevenue, value); }
        public string Status { get => _status; set => SetField(ref _status, value); }
        public string CancellationReason { get => _cancellationReason; set => SetField(ref _cancellationReason, value); }
        public string RouteNumber { get => _routeNumber; set => SetField(ref _routeNumber, value); }
        public string BusGovPlate { get => _busGovPlate; set => SetField(ref _busGovPlate, value); }
        public string BusModel { get => _busModel; set => SetField(ref _busModel, value); }
        public string DriverName { get => _driverName; set => SetField(ref _driverName, value); }
        public string ConductorName { get => _conductorName; set => SetField(ref _conductorName, value); }
        public string ShiftName { get => _shiftName; set => SetField(ref _shiftName, value); }
        public decimal ActualRevenue { get => _actualRevenue; set => SetField(ref _actualRevenue, value); }
        public int TicketsSold { get => _ticketsSold; set => SetField(ref _ticketsSold, value); }

        // Вычисляемые свойства
        public string TripInfo => $"Рейс {Id} ({RouteNumber})";
        public string DateFormatted => TripDate.ToString("dd.MM.yyyy");
        public string RevenueInfo => $"{ActualRevenue:N2} / {PlannedRevenue:N2} руб.";
        public bool CanEdit => Status == "запланирован";
        public bool CanCancel => Status == "запланирован" || Status == "в_пути";

        public string StatusColor
        {
            get
            {
                switch (Status)
                {
                    case "запланирован": return "Blue";
                    case "в_пути": return "Green";
                    case "завершен": return "Gray";
                    case "отменен": return "Red";
                    default: return "Black";
                }
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