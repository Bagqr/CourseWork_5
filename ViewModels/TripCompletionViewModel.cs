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
    public class TripCompletionViewModel : INotifyPropertyChanged
    {
        private readonly ITripRepository _tripRepository;
        private decimal _actualRevenue;
        private int _ticketsSold;
        private Trip _trip;
        private ObservableCollection<string> _errors;

        public Trip Trip
        {
            get => _trip;
            set
            {
                _trip = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TripInfo));
                OnPropertyChanged(nameof(PlannedRevenueDisplay));

                // Устанавливаем плановую выручку как значение по умолчанию
                if (_trip != null)
                {
                    ActualRevenue = _trip.PlannedRevenue;
                    TicketsSold = 0; // Начальное значение для билетов
                }
            }
        }

        public decimal ActualRevenue
        {
            get => _actualRevenue;
            set
            {
                _actualRevenue = value;
                OnPropertyChanged();
                Validate();
            }
        }

        public int TicketsSold
        {
            get => _ticketsSold;
            set
            {
                _ticketsSold = value;
                OnPropertyChanged();
                Validate();
            }
        }

        public string TripInfo => Trip != null ?
            $"Рейс {Trip.Id} ({Trip.RouteNumber}), План: {Trip.PlannedRevenue:N2} руб." :
            string.Empty;

        public string PlannedRevenueDisplay => Trip != null ?
            $"{Trip.PlannedRevenue:N2} руб." : "0 руб.";

        public ObservableCollection<string> Errors
        {
            get => _errors;
            set
            {
                _errors = value;
                OnPropertyChanged();
            }
        }

        public bool HasErrors => Errors?.Count > 0;
        public bool CanComplete => !HasErrors && Trip != null;

        public ICommand CompleteCommand { get; }
        public ICommand CancelCommand { get; }

        public Action<bool?> CloseAction { get; set; }

        public TripCompletionViewModel(ITripRepository tripRepository)
        {
            _tripRepository = tripRepository;
            Errors = new ObservableCollection<string>();

            CompleteCommand = new RelayCommand(async _ => await CompleteAsync(), _ => CanComplete);
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void Validate()
        {
            var errors = new List<string>();

            if (ActualRevenue < 0)
                errors.Add("Выручка не может быть отрицательной");

            if (ActualRevenue > 1000000) // Ограничение максимальной выручки
                errors.Add("Выручка не может превышать 1 000 000 руб.");

            if (TicketsSold < 0)
                errors.Add("Количество билетов не может быть отрицательным");

            if (TicketsSold > 10000) // Реалистичное ограничение на билеты
                errors.Add("Количество билетов не может превышать 10 000");

            // Проверка на логическую корректность
            if (Trip != null && ActualRevenue > Trip.PlannedRevenue * 2)
                errors.Add("Фактическая выручка значительно превышает плановую. Проверьте данные.");

            if (Trip != null && TicketsSold > 0 && ActualRevenue == 0)
                errors.Add("Если проданы билеты, выручка должна быть больше 0");

            Errors.Clear();
            foreach (var error in errors) Errors.Add(error);

            OnPropertyChanged(nameof(HasErrors));
            OnPropertyChanged(nameof(CanComplete));
        }

        private async Task CompleteAsync()
        {
            if (!CanComplete)
                return;

            try
            {
                bool success = await _tripRepository.CompleteAsync(Trip.Id, ActualRevenue, TicketsSold);

                if (success)
                {
                    MessageBox.Show($"Рейс успешно завершен!\n" +
                                  $"Фактическая выручка: {ActualRevenue:N2} руб.\n" +
                                  $"Продано билетов: {TicketsSold}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseAction?.Invoke(true);
                }
                else
                {
                    MessageBox.Show("Не удалось завершить рейс", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка завершения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseAction?.Invoke(false);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}