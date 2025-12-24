using System;
using System.Globalization;
using System.Windows;
using BusParkManagementSystem.Models;

namespace BusParkManagementSystem.Views.Dialogs
{
    public partial class PositionEditDialog : Window
    {
        public Position Position { get; set; }

        public PositionEditDialog(Position position = null)
        {
            InitializeComponent();

            if (position != null)
            {
                // Создаем копию объекта для редактирования
                Position = new Position
                {
                    Id = position.Id,
                    PositionName = position.PositionName,
                    BaseSalary = position.BaseSalary,
                    BonusPercent = position.BonusPercent
                };
            }
            else
            {
                Position = new Position();
            }

            DataContext = Position;
            Loaded += (s, e) => PositionNameTextBox.Focus();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Position.PositionName))
            {
                MessageBox.Show("Название должности не может быть пустым", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Валидация базового оклада (может быть null)
            if (Position.BaseSalary.HasValue && Position.BaseSalary.Value < 0)
            {
                MessageBox.Show("Базовый оклад не может быть отрицательным", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Валидация процента премии (может быть null)
            if (Position.BonusPercent.HasValue &&
                (Position.BonusPercent.Value < 0 || Position.BonusPercent.Value > 100))
            {
                MessageBox.Show("Процент премии должен быть от 0 до 100", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}