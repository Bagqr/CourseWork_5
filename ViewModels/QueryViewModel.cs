using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BusParkManagementSystem.Data;

namespace BusParkManagementSystem.ViewModels
{
    public class QueryViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseContext _dbContext;
        private DataTable _queryResults;
        private string _selectedQuery;
        private string _queryTitle;
        private bool _isLoading;

        public DataTable QueryResults
        {
            get => _queryResults;
            set
            {
                _queryResults = value;
                OnPropertyChanged();
            }
        }

        public string SelectedQuery
        {
            get => _selectedQuery;
            set
            {
                _selectedQuery = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(value))
                {
                    _ = ExecuteQueryAsync(value);
                }
            }
        }

        public string QueryTitle
        {
            get => _queryTitle;
            set
            {
                _queryTitle = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public List<string> AvailableQueries { get; }

        public ICommand RefreshCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public QueryViewModel()
        {
            try
            {
                _dbContext = new DatabaseContext();

                QueryResults = new DataTable();
                AvailableQueries = new List<string>
                {
                    "1. Водители на маршруте",
                    "2. Автобусы на маршруте",
                    "3. Маршруты из пункта",
                    "4. Выручка по маршрутам",
                    "5. Состояние автобусов",
                    "6. Сотрудники по должности",
                    "7. Рейсы за дату",
                    "8. Пробег автобусов",
                    "9. Количество рейсов",
                    "10. Свободные автобусы",
                    "11. Занятые водители",
                    "12. Отчет по выручке"
                };

                RefreshCommand = new RelayCommand(Refresh);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации QueryViewModel: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteQueryAsync(string queryName)
        {
            try
            {
                IsLoading = true;

                string sqlQuery = GetSqlQuery(queryName);
                QueryTitle = GetQueryTitle(queryName);

                using (var connection = _dbContext.GetConnection())
                {
                    using (var command = new SQLiteCommand(sqlQuery, connection))
                    {
                        using (var adapter = new SQLiteDataAdapter(command))
                        {
                            var dataTable = new DataTable();
                            await Task.Run(() => adapter.Fill(dataTable));

                            QueryResults = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка выполнения запроса: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string GetSqlQuery(string queryName)
        {
            switch (queryName)
            {
                case "1. Водители на маршруте":
                    return @"
                    SELECT DISTINCT
                        в.full_name as 'ФИО водителя',
                        в.gender as 'Пол',
                        d.position_name as 'Должность',
                        COUNT(DISTINCT р.id) as 'Количество рейсов'
                    FROM сотрудник в
                    JOIN должность d ON в.position_id = d.id
                    JOIN рейс р ON в.id = р.водитель_id
                    WHERE d.position_name = 'Водитель' AND в.активен = 1
                    GROUP BY в.id
                    ORDER BY 'Количество рейсов' DESC";

                case "2. Автобусы на маршруте":
                    return @"
                    SELECT
                        а.gov_plate as 'Гос. номер',
                        m.model_name as 'Модель',
                        s.state_name as 'Состояние',
                        а.mileage as 'Пробег',
                        COUNT(DISTINCT р.id) as 'Количество рейсов'
                    FROM автобус а
                    LEFT JOIN модель m ON а.model_id = m.id
                    LEFT JOIN состояние_автобуса s ON а.state_id = s.id
                    LEFT JOIN рейс р ON а.id = р.автобус_id
                    GROUP BY а.id
                    ORDER BY а.gov_plate";

                case "3. Маршруты из пункта":
                    return @"
                    SELECT
                        m.route_number as 'Номер маршрута',
                        o.name as 'Начальная остановка',
                        COUNT(DISTINCT um.stop_id) as 'Кол-во остановок',
                        MAX(um.distance_from_start) as 'Протяженность (м)'
                    FROM маршрут m
                    JOIN участок_маршрута um ON m.id = um.route_id
                    JOIN остановка o ON um.stop_id = o.id
                    WHERE um.""order"" = 1
                    GROUP BY m.id
                    ORDER BY m.route_number";

                case "4. Выручка по маршрутам":
                    return @"
                    SELECT
                        m.route_number as 'Номер маршрута',
                        COUNT(DISTINCT р.id) as 'Кол-во рейсов',
                        SUM(COALESCE(вр.фактическая_выручка, р.плановая_выручка)) as 'Общая выручка',
                        AVG(COALESCE(вр.фактическая_выручка, р.плановая_выручка)) as 'Средняя выручка'
                    FROM маршрут m
                    LEFT JOIN рейс р ON m.id = р.маршрут_id
                    LEFT JOIN выручка_рейса вр ON р.id = вр.рейс_id
                    GROUP BY m.id
                    ORDER BY 'Общая выручка' DESC";

                case "5. Состояние автобусов":
                    return @"
                    SELECT
                        s.state_name as 'Состояние',
                        COUNT(*) as 'Количество автобусов',
                        AVG(а.mileage) as 'Средний пробег'
                    FROM автобус а
                    JOIN состояние_автобуса s ON а.state_id = s.id
                    GROUP BY s.state_name
                    ORDER BY 'Количество автобусов' DESC";

                case "6. Сотрудники по должности":
                    return @"
                    SELECT
                        d.position_name as 'Должность',
                        COUNT(*) as 'Количество сотрудников',
                        AVG(e.salary) as 'Средняя зарплата'
                    FROM сотрудник e
                    JOIN должность d ON e.position_id = d.id
                    WHERE e.активен = 1
                    GROUP BY d.position_name
                    ORDER BY 'Количество сотрудников' DESC";

                case "7. Рейсы за дату":
                    return @"
                    SELECT
                        р.дата_рейса as 'Дата',
                        COUNT(*) as 'Количество рейсов',
                        SUM(р.плановая_выручка) as 'Плановая выручка',
                        SUM(COALESCE(вр.фактическая_выручка, 0)) as 'Фактическая выручка'
                    FROM рейс р
                    LEFT JOIN выручка_рейса вр ON р.id = вр.рейс_id
                    GROUP BY р.дата_рейса
                    ORDER BY р.дата_рейса DESC
                    LIMIT 30";

                case "8. Пробег автобусов":
                    return @"
                    SELECT
                        а.gov_plate as 'Гос. номер',
                        m.model_name as 'Модель',
                        а.mileage as 'Текущий пробег',
                        а.last_overhaul_date as 'Дата капремонта',
                        а.manufacturer_date as 'Дата выпуска'
                    FROM автобус а
                    JOIN модель m ON а.model_id = m.id
                    ORDER BY а.mileage DESC";

                case "9. Количество рейсов":
                    return @"
                    SELECT
                        strftime('%Y-%m', р.дата_рейса) as 'Месяц',
                        COUNT(*) as 'Количество рейсов',
                        SUM(р.плановая_выручка) as 'Плановая выручка'
                    FROM рейс р
                    GROUP BY strftime('%Y-%m', р.дата_рейса)
                    ORDER BY 'Месяц' DESC";

                case "10. Свободные автобусы":
                    return @"
                    SELECT
                        а.gov_plate as 'Гос. номер',
                        m.model_name as 'Модель',
                        s.state_name as 'Состояние',
                        а.mileage as 'Пробег'
                    FROM автобус а
                    JOIN модель m ON а.model_id = m.id
                    JOIN состояние_автобуса s ON а.state_id = s.id
                    WHERE s.state_name = 'ИСПРАВЕН'
                        AND а.id NOT IN (
                            SELECT автобус_id 
                            FROM рейс 
                            WHERE статус IN ('запланирован', 'в_пути')
                        )
                    ORDER BY а.gov_plate";

                case "11. Занятые водители":
                    return @"
                    SELECT
                        в.full_name as 'ФИО водителя',
                        COUNT(DISTINCT р.id) as 'Активных рейсов',
                        GROUP_CONCAT(DISTINCT m.route_number) as 'Маршруты'
                    FROM сотрудник в
                    JOIN рейс р ON в.id = р.водитель_id
                    JOIN маршрут m ON р.маршрут_id = m.id
                    WHERE р.статус IN ('запланирован', 'в_пути')
                    GROUP BY в.id
                    ORDER BY 'Активных рейсов' DESC";

                case "12. Отчет по выручке":
                    return @"
                    SELECT
                        m.route_number as 'Номер маршрута',
                        COUNT(DISTINCT р.id) as 'Кол-во рейсов',
                        SUM(р.плановая_выручка) as 'Плановая выручка',
                        SUM(COALESCE(вр.фактическая_выручка, 0)) as 'Фактическая выручка',
                        SUM(COALESCE(вр.фактическая_выручка, 0)) - SUM(р.плановая_выручка) as 'Отклонение'
                    FROM маршрут m
                    JOIN рейс р ON m.id = р.маршрут_id
                    LEFT JOIN выручка_рейса вр ON р.id = вр.рейс_id
                    WHERE р.статус = 'завершен'
                    GROUP BY m.id
                    ORDER BY 'Отклонение' DESC";

                default:
                    return "SELECT 'Выберите запрос' as Результат";
            }
        }

        private string GetQueryTitle(string queryName)
        {
            switch (queryName)
            {
                case "1. Водители на маршруте":
                    return "Список водителей и количество их рейсов";
                case "2. Автобусы на маршруте":
                    return "Автобусы и их использование";
                case "3. Маршруты из пункта":
                    return "Маршруты и их характеристики";
                case "4. Выручка по маршрутам":
                    return "Выручка по маршрутам";
                case "5. Состояние автобусов":
                    return "Статистика по состоянию автобусов";
                case "6. Сотрудники по должности":
                    return "Распределение сотрудников по должностям";
                case "7. Рейсы за дату":
                    return "Рейсы по дням (последние 30 дней)";
                case "8. Пробег автобусов":
                    return "Пробег автобусов (по убыванию)";
                case "9. Количество рейсов":
                    return "Количество рейсов по месяцам";
                case "10. Свободные автобусы":
                    return "Свободные исправные автобусы";
                case "11. Занятые водители":
                    return "Водители с активными рейсами";
                case "12. Отчет по выручке":
                    return "Отчет по выручке по маршрутам";
                default:
                    return "Результаты запроса";
            }
        }

        private void Refresh(object parameter)
        {
            if (!string.IsNullOrEmpty(SelectedQuery))
            {
                _ = ExecuteQueryAsync(SelectedQuery);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}