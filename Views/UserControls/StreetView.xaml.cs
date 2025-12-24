using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BusParkManagementSystem.ViewModels.Permissions;
using BusParkManagementSystem.Models;

namespace BusParkManagementSystem
{
    public partial class StreetView : UserControl
    {
        private StreetViewModel _viewModel;

        public StreetView()
        {
            InitializeComponent();
            _viewModel = new StreetViewModel();
            this.DataContext = _viewModel;
            Loaded += async (s, e) => await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                await _viewModel.LoadDataAsync();
                DataGrid.ItemsSource = _viewModel.Items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.CanWrite)
            {
                MessageBox.Show("У вас нет прав на добавление записей", 
                    "Нет прав доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Логика добавления
            MessageBox.Show("Функция добавления пока не реализована", 
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.CanEdit)
            {
                MessageBox.Show("У вас нет прав на редактирование записей", 
                    "Нет прав доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var selectedItem = DataGrid.SelectedItem as LookupModel;
            if (selectedItem == null)
            {
                MessageBox.Show("Выберите запись для редактирования", 
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            // Логика редактирования
            MessageBox.Show($"Редактирование записи: {selectedItem.Name}", 
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.CanDelete)
            {
                MessageBox.Show("У вас нет прав на удаление записей", 
                    "Нет прав доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var selectedItem = DataGrid.SelectedItem as LookupModel;
            if (selectedItem == null)
            {
                MessageBox.Show("Выберите запись для удаления", 
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var result = MessageBox.Show($"Удалить запись: {selectedItem.Name}?", 
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                // Логика удаления
                MessageBox.Show("Функция удаления пока не реализована", 
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadData();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Поиск...")
                SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                SearchBox.Text = "Поиск...";
        }
    }

    public class StreetViewModel : BasePermissionViewModel
    {
        public List<LookupModel> Items { get; set; } = new List<LookupModel>();

        public StreetViewModel()
        {
            _ = InitializePermissions();
        }

        private async Task InitializePermissions()
        {
            await CheckPermissionsAsync("streets");
            SetPermissionButtonsVisibility();
        }

        public async Task LoadDataAsync()
        {
            // Проверяем права на чтение
            if (!CanRead)
            {
                Items.Clear();
                return;
            }

            try
            {
                // Загружаем данные из базы
                Items = await LoadStreetsFromDatabase();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки улиц: {ex.Message}");
                Items = new List<LookupModel>();
            }
        }

        private async Task<List<LookupModel>> LoadStreetsFromDatabase()
        {
            // В реальном приложении здесь будет вызов репозитория
            // Пока возвращаем тестовые данные
            return new List<LookupModel>
            {
                new LookupModel { Id = 1, Name = "Ленина" },
                new LookupModel { Id = 2, Name = "Гагарина" },
                new LookupModel { Id = 3, Name = "Советская" },
                new LookupModel { Id = 4, Name = "Кирова" },
                new LookupModel { Id = 5, Name = "Пушкина" }
            };
        }
    }
}