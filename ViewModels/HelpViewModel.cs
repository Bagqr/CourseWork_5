using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BusParkManagementSystem.ViewModels.Utilities;

namespace BusParkManagementSystem.ViewModels
{
    public class HelpViewModel : BaseViewModel
    {
        private string _selectedContent;
        private bool _isContentSelected = true;

        public string SelectedContent
        {
            get => _selectedContent;
            set => SetField(ref _selectedContent, value);
        }

        public bool IsContentSelected
        {
            get => _isContentSelected;
            set
            {
                SetField(ref _isContentSelected, value);
                if (value) SelectedContent = GetContentText();
            }
        }

        public bool IsAboutSelected
        {
            get => !_isContentSelected;
            set
            {
                SetField(ref _isContentSelected, !value);
                if (value) SelectedContent = GetAboutText();
            }
        }

        public ICommand NavigateToContentCommand { get; }
        public ICommand NavigateToAboutCommand { get; }
        public ICommand OpenDocumentationCommand { get; }
        public ICommand CloseCommand { get; }

        public HelpViewModel()
        {
            SelectedContent = GetContentText();

            NavigateToContentCommand = new RelayCommand(_ => IsContentSelected = true);
            NavigateToAboutCommand = new RelayCommand(_ => IsAboutSelected = true);
            OpenDocumentationCommand = new RelayCommand(_ => OpenDocumentation());
            CloseCommand = new RelayCommand(_ => CloseWindow());
        }

        private string GetContentText()
        {
            return @"АВТОБУСНЫЙ ПАРК - Система управления

СОДЕРЖАНИЕ:

1. АВТОБУСЫ
   • Просмотр списка автобусов
   • Добавление/редактирование автобусов
   • Изменение состояния автобусов
   • История техобслуживания

2. СОТРУДНИКИ
   • Учет персонала
   • Кадровые мероприятия
   • История работы сотрудников
   • Управление должностями

3. МАРШРУТЫ
   • Управление маршрутами
   • График движения
   • Остановки маршрутов
   • Протяженность маршрутов

4. РЕЙСЫ
   • Планирование рейсов
   • Назначение водителей и кондукторов
   • Контроль выполнения рейсов
   • Учет выручки

5. ЗАПРОСЫ
   • Статистические запросы
   • Оперативная информация
   • Аналитика работы парка

6. ОТЧЕТЫ
   • Финансовая отчетность
   • Отчеты по технике
   • Кадровые отчеты

КРАТКИЕ ОБОЗНАЧЕНИЯ:
✓ - операция выполнена
⚠ - требуется внимание
✗ - операция отменена
";
        }

        private string GetAboutText()
        {
            return @"АВТОБУСНЫЙ ПАРК
Информационная система управления муниципальным автопредприятием

ВЕРСИЯ: 1.0.0
ДАТА СОЗДАНИЯ: 2024 г.

ОПИСАНИЕ:
Система предназначена для автоматизации деятельности автобусного парка,
обслуживающего внутригородские транспортные маршруты.

ОСНОВНЫЕ ФУНКЦИОНАЛЬНЫЕ ВОЗМОЖНОСТИ:
• Учет подвижного состава
• Управление маршрутами и графиками движения
• Кадровый учет и планирование смен
• Учет выручки и билетов
• Техническое обслуживание автобусов
• Формирование отчетности

РАЗРАБОТЧИКИ:
Проект разработан в рамках курсовой работы по дисциплине
'Разработка информационных систем'

ТЕХНОЛОГИИ:
• Платформа: .NET Framework 4.7.2
• Язык: C#
• GUI: WPF (MVVM)
• База данных: SQLite
• ORM: Dapper

КОНТАКТНАЯ ИНФОРМАЦИЯ:
Для обратной связи и сообщения об ошибках:
support@buspark.local

ЛИЦЕНЗИЯ:
© 2024 Автобусный парк. Все права защищены.
Данное программное обеспечение является учебным проектом.
";
        }

        private void OpenDocumentation()
        {
            try
            {
                // Можно добавить ссылку на документацию или открыть PDF
                MessageBox.Show("Документация будет открыта в браузере",
                    "Документация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Пример открытия ссылки:
                // Process.Start("https://ваш-сайт-с-документацией.com");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть документацию: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CloseWindow()
        {
            Application.Current.Windows?
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)?
                .Close();
        }
    }
}