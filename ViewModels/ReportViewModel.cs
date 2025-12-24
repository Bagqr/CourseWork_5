using System;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using BusParkManagementSystem.Data;

namespace BusParkManagementSystem.ViewModels
{
    public class ReportViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseContext _dbContext;
        private DataTable _reportData;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _selectedReportType;
        private bool _isLoading;

        public DataTable ReportData
        {
            get => _reportData;
            set
            {
                _reportData = value;
                OnPropertyChanged();
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
            }
        }

        public string SelectedReportType
        {
            get => _selectedReportType;
            set
            {
                _selectedReportType = value;
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

        public string[] ReportTypes { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ReportViewModel()
        {
            try
            {
                _dbContext = new DatabaseContext();

                ReportData = new DataTable();
                StartDate = DateTime.Today.AddMonths(-1);
                EndDate = DateTime.Today;

                ReportTypes = new[]
                {
                    "Выручка по дням",
                    "Выручка по маршрутам",
                    "Рейсы по водителям",
                    "Статистика автобусов",
                    "Зарплатная ведомость"
                };

                SelectedReportType = ReportTypes[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации ReportViewModel: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task GenerateReportAsync()
        {
            try
            {
                IsLoading = true;

                string sqlQuery = GetReportSqlQuery(SelectedReportType);

                using (var connection = _dbContext.GetConnection())
                {
                    using (var command = new SQLiteCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@StartDate", StartDate.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@EndDate", EndDate.ToString("yyyy-MM-dd"));

                        using (var adapter = new SQLiteDataAdapter(command))
                        {
                            var dataTable = new DataTable();
                            await Task.Run(() => adapter.Fill(dataTable));

                            ReportData = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации отчета: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string GetReportSqlQuery(string reportType)
        {
            switch (reportType)
            {
                case "Выручка по дням":
                    return @"
                SELECT 
                    DATE(р.дата_рейса) as 'Дата',
                    COUNT(*) as 'КоличествоРейсов',
                    SUM(р.плановая_выручка) as 'ПлановаяВыручка',
                    SUM(COALESCE(вр.фактическая_выручка, 0)) as 'ФактическаяВыручка',
                    SUM(COALESCE(вр.фактическая_выручка, 0)) - SUM(р.плановая_выручка) as 'Отклонение'
                FROM рейс р
                LEFT JOIN выручка_рейса вр ON р.id = вр.рейс_id
                WHERE DATE(р.дата_рейса) BETWEEN @StartDate AND @EndDate
                GROUP BY DATE(р.дата_рейса)
                ORDER BY DATE(р.дата_рейса) DESC";

                case "Выручка по маршрутам":
                    return @"
                SELECT 
                    m.route_number as 'НомерМаршрута',
                    COUNT(DISTINCT р.id) as 'КоличествоРейсов',
                    SUM(р.плановая_выручка) as 'ПлановаяВыручка',
                    SUM(COALESCE(вр.фактическая_выручка, 0)) as 'ФактическаяВыручка',
                    AVG(COALESCE(вр.фактическая_выручка, 0)) as 'СредняяВыручкаЗаРейс'
                FROM маршрут m
                JOIN рейс р ON m.id = р.маршрут_id
                LEFT JOIN выручка_рейса вр ON р.id = вр.рейс_id
                WHERE DATE(р.дата_рейса) BETWEEN @StartDate AND @EndDate
                GROUP BY m.id
                ORDER BY 'ФактическаяВыручка' DESC";

                case "Рейсы по водителям":
                    return @"
                SELECT 
                    в.full_name as 'Водитель',
                    d.position_name as 'Должность',
                    COUNT(DISTINCT р.id) as 'КоличествоРейсов',
                    SUM(р.плановая_выручка) as 'ПлановаяВыручка',
                    SUM(COALESCE(вр.фактическая_выручка, 0)) as 'ФактическаяВыручка'
                FROM сотрудник в
                JOIN должность d ON в.position_id = d.id
                JOIN рейс р ON в.id = р.водитель_id
                LEFT JOIN выручка_рейса вр ON р.id = вр.рейс_id
                WHERE DATE(р.дата_рейса) BETWEEN @StartDate AND @EndDate
                    AND в.активен = 1
                GROUP BY в.id
                ORDER BY 'КоличествоРейсов' DESC";

                case "Статистика автобусов":
                    return @"
                SELECT 
                    а.gov_plate as 'ГосНомер',
                    m.model_name as 'Модель',
                    s.state_name as 'Состояние',
                    а.mileage as 'Пробег',
                    COUNT(DISTINCT р.id) as 'КоличествоРейсов',
                    SUM(COALESCE(вр.фактическая_выручка, 0)) as 'Выручка'
                FROM автобус а
                JOIN модель m ON а.model_id = m.id
                JOIN состояние_автобуса s ON а.state_id = s.id
                LEFT JOIN рейс р ON а.id = р.автобус_id
                LEFT JOIN выручка_рейса вр ON р.id = вр.рейс_id
                WHERE DATE(р.дата_рейса) BETWEEN @StartDate AND @EndDate
                    OR р.id IS NULL
                GROUP BY а.id
                ORDER BY а.gov_plate";

                case "Зарплатная ведомость":
                    return @"
                SELECT 
                    в.full_name as 'Сотрудник',
                    d.position_name as 'Должность',
                    в.salary as 'Оклад',
                    COUNT(DISTINCT р.id) as 'КоличествоРейсов',
                    (в.salary * d.процент_премии / 100) as 'Премия',
                    в.salary + (в.salary * d.процент_премии / 100) as 'ИтогоКВыплате'
                FROM сотрудник в
                JOIN должность d ON в.position_id = d.id
                LEFT JOIN рейс р ON в.id = р.водитель_id OR в.id = р.кондуктор_id
                WHERE в.активен = 1
                    AND (DATE(р.дата_рейса) BETWEEN @StartDate AND @EndDate 
                        OR р.id IS NULL)
                GROUP BY в.id
                ORDER BY 'ИтогоКВыплате' DESC";

                default:
                    return "SELECT 'Выберите тип отчета' as 'Результат'";
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}