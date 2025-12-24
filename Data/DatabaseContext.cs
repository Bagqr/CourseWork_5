using Dapper;
using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace BusParkManagementSystem.Data
{
    public class DatabaseContext : IDisposable
    {
        private readonly string _databasePath;
        private bool _disposed = false;

        public DatabaseContext()
        {
            // Используем папку AppData для постоянного хранения
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "BusParkManagementSystem");

            // Создаем папку приложения, если ее нет
            Directory.CreateDirectory(appFolder);

            _databasePath = Path.Combine(appFolder, "buspark.db");

            // Выводим путь для отладки (можно убрать позже)
            Debug.WriteLine($"База данных находится по пути: {_databasePath}");

            // Инициализируем БД при первом запуске
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            // Если БД не существует, создаем ее
            if (!File.Exists(_databasePath))
            {
                CreateDatabaseFromScratch();
            }
            else
            {
                // Если БД существует, проверяем и обновляем структуру при необходимости
                UpdateDatabaseStructure();
            }
        }

        private void CreateDatabaseFromScratch()
        {
            string directory = Path.GetDirectoryName(_databasePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var connection = new SQLiteConnection($"Data Source={_databasePath};Version=3;"))
            {
                connection.Open();

                // Создаем таблицы из вашего полного SQL-скрипта
                // Я использую упрощенную версию, основанную на вашем полном скрипте
                string createScript = GetFullDatabaseScript();

                // Разделяем скрипт на отдельные команды
                var commands = createScript.Split(
                    new[] { ";\r\n", ";\n" },
                    StringSplitOptions.RemoveEmptyEntries);

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var commandText in commands)
                        {
                            if (!string.IsNullOrWhiteSpace(commandText))
                            {
                                using (var command = new SQLiteCommand(commandText, connection, transaction))
                                {
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                        transaction.Commit();

                        Debug.WriteLine("База данных успешно создана в AppData");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Debug.WriteLine($"Ошибка при создании БД: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        private string GetFullDatabaseScript()
        {
            // Возвращаем полный скрипт создания БД
            // Это упрощенная версия из вашего SQL-файла
            return @"
BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS ""автобус"" (
	""id""	INTEGER NOT NULL UNIQUE,
	""inventory_number""	INTEGER NOT NULL,
	""Model_id""	INTEGER NOT NULL,
	""State_id""	INTEGER NOT NULL,
	""Gov_plate""	TEXT NOT NULL,
	""Engine_number""	NUMERIC NOT NULL,
	""Chasis_number""	TEXT NOT NULL,
	""Body_number""	TEXT NOT NULL,
	""Manufacturer_date""	TEXT NOT NULL,
	""Mileage""	INTEGER NOT NULL CHECK(""mileage"" >= 0),
	""Last_overhaul_date""	INTEGER NOT NULL,
	""Color_id""	INTEGER NOT NULL,
	""текущий_водитель_id""	INTEGER,
	PRIMARY KEY(""id"" AUTOINCREMENT),
	FOREIGN KEY(""Color_id"") REFERENCES ""цвет""(""id""),
	FOREIGN KEY(""Model_id"") REFERENCES ""модель""(""id""),
	FOREIGN KEY(""State_id"") REFERENCES ""состояние_автобуса""(""id""),
	FOREIGN KEY(""текущий_водитель_id"") REFERENCES ""сотрудник""(""id"")
);
CREATE TABLE IF NOT EXISTS ""вид_кадрового_мероприятия"" (
	""id""	INTEGER NOT NULL,
	""personnel_event_name""	TEXT NOT NULL UNIQUE,
	PRIMARY KEY(""id"" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS ""время_вождения"" (
	""id""	INTEGER,
	""водитель_id""	INTEGER NOT NULL,
	""рейс_id""	INTEGER NOT NULL,
	""время_начала""	DATETIME NOT NULL,
	""время_окончания""	DATETIME NOT NULL,
	PRIMARY KEY(""id"" AUTOINCREMENT),
	FOREIGN KEY(""водитель_id"") REFERENCES ""сотрудник""(""id""),
	FOREIGN KEY(""рейс_id"") REFERENCES ""рейс""(""id"")
);
CREATE TABLE IF NOT EXISTS ""выручка_рейса"" (
	""id""	INTEGER,
	""рейс_id""	INTEGER NOT NULL,
	""фактическая_выручка""	DECIMAL(10, 2) NOT NULL,
	""продано_билетов""	INTEGER NOT NULL,
	""дата_записи""	DATETIME DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY(""id"" AUTOINCREMENT),
	FOREIGN KEY(""рейс_id"") REFERENCES ""рейс""(""id"")
);
CREATE TABLE IF NOT EXISTS ""график_движения"" (
	""id""	INTEGER NOT NULL,
	""route_id""	INTEGER NOT NULL,
	""first_departure_time""	TEXT NOT NULL,
	""Last_departure_time""	TEXT NOT NULL,
	""intervals""	INTEGER NOT NULL CHECK(""intervals"" > 0),
	PRIMARY KEY(""id"" AUTOINCREMENT),
	FOREIGN KEY(""route_id"") REFERENCES ""маршрут""(""id"")
);
CREATE TABLE IF NOT EXISTS ""должность"" (
	""id""	INTEGER NOT NULL,
	""position_name""	TEXT NOT NULL UNIQUE,
	""базовый_оклад""	DECIMAL(10, 2),
	""процент_премии""	DECIMAL(5, 2) DEFAULT 10.0,
	PRIMARY KEY(""id"" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS ""запас_билетов"" (
	""id""	INTEGER,
	""кондуктор_id""	INTEGER NOT NULL,
	""серия_билетов""	TEXT NOT NULL,
	""начальный_номер""	INTEGER NOT NULL,
	""конечный_номер""	INTEGER NOT NULL,
	""дата_выдачи""	DATE NOT NULL,
	""дата_возврата""	DATE,
	""использовано""	INTEGER DEFAULT 0,
	PRIMARY KEY(""id"" AUTOINCREMENT),
	FOREIGN KEY(""кондуктор_id"") REFERENCES ""сотрудник""(""id"")
);
CREATE TABLE IF NOT EXISTS ""интервалы_движения"" (
	""id""	INTEGER,
	""график_id""	INTEGER NOT NULL,
	""время_начала""	TIME NOT NULL,
	""время_окончания""	TIME NOT NULL,
	""интервал_минуты""	INTEGER NOT NULL,
	""тип_дня""	TEXT NOT NULL CHECK(""тип_дня"" IN ('будни', 'выходные', 'праздничные')),
	PRIMARY KEY(""id"" AUTOINCREMENT),
	FOREIGN KEY(""график_id"") REFERENCES ""график_движения""(""id"")
);
CREATE TABLE IF NOT EXISTS ""история_кадровых_событий"" (
	""id""	INTEGER,
	""сотрудник_id""	INTEGER NOT NULL,
	""должность_id""	INTEGER NOT NULL,
	""тип_мероприятия_id""	INTEGER NOT NULL,
	""дата_события""	DATE NOT NULL,
	""номер_документа""	TEXT,
	""тип_документа""	TEXT,
	""причина""	TEXT,
	""подразделение""	TEXT,
	""базовый_оклад""	DECIMAL(10, 2),
	""примечания""	TEXT,
	PRIMARY KEY(""id"" AUTOINCREMENT),
	FOREIGN KEY(""должность_id"") REFERENCES ""должность""(""id""),
	FOREIGN KEY(""сотрудник_id"") REFERENCES ""сотрудник""(""id""),
	FOREIGN KEY(""тип_мероприятия_id"") REFERENCES ""вид_кадрового_мероприятия""(""id"")
);
CREATE TABLE IF NOT EXISTS ""история_состояний_автобусов"" (
	""id""	INTEGER,
	""автобус_id""	INTEGER NOT NULL,
	""состояние_id""	INTEGER NOT NULL,
	""дата_изменения""	DATETIME DEFAULT CURRENT_TIMESTAMP,
	""причина""	TEXT,
	""изменено_кем""	INTEGER,
	PRIMARY KEY(""id"" AUTOINCREMENT),
	FOREIGN KEY(""автобус_id"") REFERENCES ""автобус""(""id""),
	FOREIGN KEY(""изменено_кем"") REFERENCES ""сотрудник""(""id""),
	FOREIGN KEY(""состояние_id"") REFERENCES ""состояние_автобуса""(""id"")
);
CREATE TABLE IF NOT EXISTS ""маршрут"" (
	""id""	INTEGER NOT NULL,
	""route_number""	INTEGER NOT NULL,
	""turnover_time""	TEXT NOT NULL,
	PRIMARY KEY(""id"" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS ""модель"" (
	""id""	INTEGER NOT NULL,
	""model_name""	TEXT NOT NULL UNIQUE,
	PRIMARY KEY(""id"" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS ""остановка"" (
	""id""	INTEGER NOT NULL,
	""name""	TEXT NOT NULL UNIQUE,
	PRIMARY KEY(""id"" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS ""рабочая_смена"" (
	""id""	INTEGER,
	""сотрудник_id""	INTEGER NOT NULL,
	""тип_смены_id""	INTEGER NOT NULL,
	""дата_работы""	DATE NOT NULL,
	""плановые_часы""	DECIMAL(4, 2) NOT NULL,
	""фактические_часы""	DECIMAL(4, 2),
	""маршрут_id""	INTEGER,
	PRIMARY KEY(""id"" AUTOINCREMENT),
	UNIQUE(""сотрудник_id"",""дата_работы""),
	FOREIGN KEY(""маршрут_id"") REFERENCES ""маршрут""(""id""),
	FOREIGN KEY(""сотрудник_id"") REFERENCES ""сотрудник""(""id""),
	FOREIGN KEY(""тип_смены_id"") REFERENCES ""тип_смены""(""id"")
);
CREATE TABLE IF NOT EXISTS ""рейс"" (
	""id""	INTEGER,
	""маршрут_id""	INTEGER NOT NULL,
	""автобус_id""	INTEGER NOT NULL,
	""водитель_id""	INTEGER NOT NULL,
	""кондуктор_id""	INTEGER NOT NULL,
	""дата_рейса""	DATE NOT NULL,
	""тип_смены_id""	INTEGER NOT NULL,
	""плановая_выручка""	DECIMAL(10, 2) NOT NULL DEFAULT 0,
	""статус""	TEXT DEFAULT 'запланирован' CHECK(""статус"" IN ('запланирован', 'в_пути', 'завершен', 'отменен')),
	""причина_снятия""	TEXT,
	PRIMARY KEY(""id"" AUTOINCREMENT),
	FOREIGN KEY(""автобус_id"") REFERENCES ""автобус""(""id""),
	FOREIGN KEY(""водитель_id"") REFERENCES ""сотрудник""(""id""),
	FOREIGN KEY(""кондуктор_id"") REFERENCES ""сотрудник""(""id""),
	FOREIGN KEY(""маршрут_id"") REFERENCES ""маршрут""(""id""),
	FOREIGN KEY(""тип_смены_id"") REFERENCES ""тип_смены""(""id"")
);
CREATE TABLE IF NOT EXISTS ""состояние_автобуса"" (
	""id""	INTEGER NOT NULL,
	""state_name""	TEXT NOT NULL UNIQUE,
	PRIMARY KEY(""id"" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS ""сотрудник"" (
	""id""	INTEGER NOT NULL UNIQUE,
	""Full_name""	TEXT NOT NULL,
	""Gender""	TEXT NOT NULL CHECK(""gender"" IN ('М', 'Ж')),
	""Birth_date""	TEXT NOT NULL,
	""Street_id""	INTEGER NOT NULL,
	""Position_id""	INTEGER NOT NULL,
	""Salary""	INTEGER NOT NULL CHECK(""salary"" >= 0),
	""House""	INTEGER NOT NULL CHECK(""house"" > 0),
	""активен""	BOOLEAN DEFAULT 1,
	""дата_увольнения""	DATE,
	PRIMARY KEY(""id"" AUTOINCREMENT),
	FOREIGN KEY(""Position_id"") REFERENCES ""должность""(""id""),
	FOREIGN KEY(""Street_id"") REFERENCES ""улица""(""id"")
);
CREATE TABLE IF NOT EXISTS ""техобслуживание"" (
	""id""	INTEGER,
	""автобус_id""	INTEGER NOT NULL,
	""дата_обслуживания""	DATE NOT NULL,
	""тип_обслуживания""	TEXT NOT NULL CHECK(""тип_обслуживания"" IN ('ремонт', 'осмотр', 'капремонт', 'обслуживание')),
	""описание""	TEXT,
	""стоимость""	DECIMAL(10, 2),
	""инженер_id""	INTEGER NOT NULL,
	""следующее_обслуживание""	DATE,
	PRIMARY KEY(""id"" AUTOINCREMENT),
	FOREIGN KEY(""автобус_id"") REFERENCES ""автобус""(""id""),
	FOREIGN KEY(""инженер_id"") REFERENCES ""сотрудник""(""id"")
);
CREATE TABLE IF NOT EXISTS ""тип_смены"" (
	""id""	INTEGER NOT NULL,
	""shift_name""	TEXT NOT NULL UNIQUE,
	PRIMARY KEY(""id"" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS ""трудовая_книжка"" (
	""id""	INTEGER NOT NULL,
	""worker_id""	INTEGER NOT NULL,
	""position_id""	INTEGER NOT NULL,
	""seniority""	INTEGER NOT NULL,
	""personnel_event_type_id""	INTEGER NOT NULL,
	""hiring_document_number""	INTEGER NOT NULL UNIQUE,
	""firing_reason""	TEXT NOT NULL,
	PRIMARY KEY(""id"" AUTOINCREMENT),
	FOREIGN KEY(""personnel_event_type_id"") REFERENCES ""вид_кадрового_мероприятия""(""id""),
	FOREIGN KEY(""position_id"") REFERENCES ""должность""(""id""),
	FOREIGN KEY(""worker_id"") REFERENCES ""сотрудник""(""id"")
);
CREATE TABLE IF NOT EXISTS ""улица"" (
	""id""	INTEGER NOT NULL,
	""street_name""	TEXT NOT NULL UNIQUE,
	PRIMARY KEY(""id"" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS ""участок_маршрута"" (
	""id""	INTEGER NOT NULL,
	""route_id""	INTEGER NOT NULL,
	""stop_id""	INTEGER NOT NULL,
	""order""	INTEGER NOT NULL CHECK(""order"" > 0),
	""distance_from_start""	INTEGER NOT NULL,
	PRIMARY KEY(""id"" AUTOINCREMENT),
	FOREIGN KEY(""route_id"") REFERENCES ""маршрут""(""id""),
	FOREIGN KEY(""stop_id"") REFERENCES ""остановка""(""id"")
);
CREATE TABLE IF NOT EXISTS ""цвет"" (
	""id""	INTEGER NOT NULL,
	""color_name""	TEXT NOT NULL UNIQUE,
	PRIMARY KEY(""id"" AUTOINCREMENT)
);
INSERT INTO ""автобус"" VALUES (1,1001,1,1,'А123БВ','ENG001','CHS001','BODY001','2020-01-15',45000,20230510,2,NULL);
INSERT INTO ""автобус"" VALUES (2,1002,2,1,'Б234ВГ','ENG002','CHS002','BODY002','2019-03-20',120000,20221115,1,NULL);
INSERT INTO ""автобус"" VALUES (3,1003,3,1,'В345ГД','ENG003','CHS003','BODY003','2021-07-10',30000,20210710,3,NULL);
INSERT INTO ""автобус"" VALUES (4,1004,4,1,'Г456ДЕ','ENG004','CHS004','BODY004','2018-05-25',180000,20230120,4,NULL);
INSERT INTO ""автобус"" VALUES (5,1005,5,1,'Д567ЕЖ','ENG005','CHS005','BODY005','2022-02-14',15000,20220214,5,NULL);
INSERT INTO ""автобус"" VALUES (6,1006,6,1,'Е678ЖЗ','ENG006','CHS006','BODY006','2020-11-30',75000,20230805,6,NULL);
INSERT INTO ""автобус"" VALUES (7,1007,7,1,'Ж789ЗИ','ENG007','CHS007','BODY007','2019-08-12',95000,20230318,7,NULL);
INSERT INTO ""автобус"" VALUES (8,1008,8,3,'З890ИК','ENG008','CHS008','BODY008','2021-04-05',40000,20210405,8,NULL);
INSERT INTO ""автобус"" VALUES (9,1009,9,1,'И901КЛ','ENG009','CHS009','BODY009','2022-09-22',25000,20220922,9,NULL);
INSERT INTO ""автобус"" VALUES (10,1010,10,1,'К012ЛМ','ENG010','CHS010','BODY010','2020-06-18',60000,20231012,10,NULL);
INSERT INTO ""автобус"" VALUES (11,1011,2,3,'П111ПП','ENG011','CHS011','BODY011','2018-01-01',120000,'2023-01-01',1,NULL);
INSERT INTO ""автобус"" VALUES (12,1013,2,1,'Н113НН','ENG013','CHS013','BODY013','2018-01-01',120000,'2023-01-01',1,NULL);
INSERT INTO ""автобус"" VALUES (13,1014,3,1,'Н114НН','ENG014','CHS014','BODY014','2017-06-01',110000,'2023-08-01',5,NULL);
INSERT INTO ""автобус"" VALUES (14,1014,3,1,'Н114НН','ENG014','CHS014','BODY014','2017-06-01',110000,'2023-08-01',5,NULL);
INSERT INTO ""автобус"" VALUES (15,1014,3,1,'Н114НН','ENG014','CHS014','BODY014','2017-06-01',110000,'2023-08-01',5,NULL);
INSERT INTO ""автобус"" VALUES (16,1014,3,1,'Н114НН','ENG014','CHS014','BODY014','2017-06-01',110000,'2023-08-01',5,NULL);
INSERT INTO ""автобус"" VALUES (17,1014,3,1,'Н114НН','ENG014','CHS014','BODY014','2017-06-01',110000,'2023-08-01',5,NULL);
INSERT INTO ""автобус"" VALUES (18,1014,3,1,'Н114НН','ENG014','CHS014','BODY014','2017-06-01',110000,'2023-08-01',5,NULL);
INSERT INTO ""вид_кадрового_мероприятия"" VALUES (1,'Прием на работу');
INSERT INTO ""вид_кадрового_мероприятия"" VALUES (2,'Увольнение');
INSERT INTO ""вид_кадрового_мероприятия"" VALUES (3,'Перевод');
INSERT INTO ""вид_кадрового_мероприятия"" VALUES (4,'Повышение');
INSERT INTO ""вид_кадрового_мероприятия"" VALUES (5,'Понижение');
INSERT INTO ""вид_кадрового_мероприятия"" VALUES (6,'Отпуск');
INSERT INTO ""вид_кадрового_мероприятия"" VALUES (7,'Больничный');
INSERT INTO ""вид_кадрового_мероприятия"" VALUES (8,'Командировка');
INSERT INTO ""вид_кадрового_мероприятия"" VALUES (9,'Премия');
INSERT INTO ""вид_кадрового_мероприятия"" VALUES (10,'Выговор');
INSERT INTO ""график_движения"" VALUES (1,1,'06:00','22:00',15);
INSERT INTO ""график_движения"" VALUES (2,2,'05:30','23:00',10);
INSERT INTO ""график_движения"" VALUES (3,3,'06:15','21:30',20);
INSERT INTO ""график_движения"" VALUES (4,4,'05:45','22:30',12);
INSERT INTO ""график_движения"" VALUES (5,5,'06:30','21:00',25);
INSERT INTO ""график_движения"" VALUES (6,6,'05:00','23:30',8);
INSERT INTO ""график_движения"" VALUES (7,7,'06:20','22:15',18);
INSERT INTO ""график_движения"" VALUES (8,8,'07:00','20:45',30);
INSERT INTO ""график_движения"" VALUES (9,9,'05:50','22:45',15);
INSERT INTO ""график_движения"" VALUES (10,10,'06:10','21:45',22);
INSERT INTO ""должность"" VALUES (1,'Водитель',NULL,10);
INSERT INTO ""должность"" VALUES (2,'Кондуктор',NULL,10);
INSERT INTO ""должность"" VALUES (3,'Директор',NULL,10);
INSERT INTO ""должность"" VALUES (4,'Диспетчер',NULL,10);
INSERT INTO ""должность"" VALUES (5,'Бухгалтер',NULL,10);
INSERT INTO ""должность"" VALUES (6,'Инженер гаража',NULL,10);
INSERT INTO ""должность"" VALUES (7,'Механик',NULL,10);
INSERT INTO ""должность"" VALUES (8,'Менеджер по кадрам',NULL,10);
INSERT INTO ""должность"" VALUES (9,'Начальник отдела',NULL,10);
INSERT INTO ""должность"" VALUES (10,'Техник',NULL,10);
INSERT INTO ""интервалы_движения"" VALUES (1,1,'06:00','10:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (2,2,'06:00','10:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (3,3,'06:00','10:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (4,4,'06:00','10:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (5,5,'06:00','10:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (6,6,'06:00','10:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (7,7,'06:00','10:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (8,8,'06:00','10:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (9,9,'06:00','10:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (10,10,'06:00','10:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (11,1,'10:00','16:00',20,'будни');
INSERT INTO ""интервалы_движения"" VALUES (12,2,'10:00','16:00',20,'будни');
INSERT INTO ""интервалы_движения"" VALUES (13,3,'10:00','16:00',20,'будни');
INSERT INTO ""интервалы_движения"" VALUES (14,4,'10:00','16:00',20,'будни');
INSERT INTO ""интервалы_движения"" VALUES (15,5,'10:00','16:00',20,'будни');
INSERT INTO ""интервалы_движения"" VALUES (16,6,'10:00','16:00',20,'будни');
INSERT INTO ""интервалы_движения"" VALUES (17,7,'10:00','16:00',20,'будни');
INSERT INTO ""интервалы_движения"" VALUES (18,8,'10:00','16:00',20,'будни');
INSERT INTO ""интервалы_движения"" VALUES (19,9,'10:00','16:00',20,'будни');
INSERT INTO ""интервалы_движения"" VALUES (20,10,'10:00','16:00',20,'будни');
INSERT INTO ""интервалы_движения"" VALUES (21,1,'16:00','22:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (22,2,'16:00','22:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (23,3,'16:00','22:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (24,4,'16:00','22:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (25,5,'16:00','22:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (26,6,'16:00','22:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (27,7,'16:00','22:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (28,8,'16:00','22:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (29,9,'16:00','22:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (30,10,'16:00','22:00',15,'будни');
INSERT INTO ""интервалы_движения"" VALUES (31,1,'08:00','22:00',30,'выходные');
INSERT INTO ""интервалы_движения"" VALUES (32,2,'08:00','22:00',30,'выходные');
INSERT INTO ""интервалы_движения"" VALUES (33,3,'08:00','22:00',30,'выходные');
INSERT INTO ""интервалы_движения"" VALUES (34,4,'08:00','22:00',30,'выходные');
INSERT INTO ""интервалы_движения"" VALUES (35,5,'08:00','22:00',30,'выходные');
INSERT INTO ""интервалы_движения"" VALUES (36,6,'08:00','22:00',30,'выходные');
INSERT INTO ""интервалы_движения"" VALUES (37,7,'08:00','22:00',30,'выходные');
INSERT INTO ""интервалы_движения"" VALUES (38,8,'08:00','22:00',30,'выходные');
INSERT INTO ""интервалы_движения"" VALUES (39,9,'08:00','22:00',30,'выходные');
INSERT INTO ""интервалы_движения"" VALUES (40,10,'08:00','22:00',30,'выходные');
INSERT INTO ""маршрут"" VALUES (1,1,'90');
INSERT INTO ""маршрут"" VALUES (2,2,'120');
INSERT INTO ""маршрут"" VALUES (3,3,'75');
INSERT INTO ""маршрут"" VALUES (4,4,'110');
INSERT INTO ""маршрут"" VALUES (5,5,'85');
INSERT INTO ""маршрут"" VALUES (6,6,'95');
INSERT INTO ""маршрут"" VALUES (7,7,'105');
INSERT INTO ""маршрут"" VALUES (8,8,'80');
INSERT INTO ""маршрут"" VALUES (9,9,'115');
INSERT INTO ""маршрут"" VALUES (10,10,'100');
INSERT INTO ""модель"" VALUES (1,'ПАЗ-3205');
INSERT INTO ""модель"" VALUES (2,'ЛИАЗ-5256');
INSERT INTO ""модель"" VALUES (3,'ЛИАЗ-5292');
INSERT INTO ""модель"" VALUES (4,'МАЗ-103');
INSERT INTO ""модель"" VALUES (5,'НефАЗ-5299');
INSERT INTO ""модель"" VALUES (6,'Volgabus-5270');
INSERT INTO ""модель"" VALUES (7,'ПАЗ-3234');
INSERT INTO ""модель"" VALUES (8,'КАВЗ-4235');
INSERT INTO ""модель"" VALUES (9,'Богдан-А601');
INSERT INTO ""модель"" VALUES (10,'МАЗ-206');
INSERT INTO ""остановка"" VALUES (1,'Центральный вокзал');
INSERT INTO ""остановка"" VALUES (2,'Площадь Ленина');
INSERT INTO ""остановка"" VALUES (3,'Университет');
INSERT INTO ""остановка"" VALUES (4,'Стадион');
INSERT INTO ""остановка"" VALUES (5,'Больница');
INSERT INTO ""остановка"" VALUES (6,'Торговый центр');
INSERT INTO ""остановка"" VALUES (7,'Школа №1');
INSERT INTO ""остановка"" VALUES (8,'Парк культуры');
INSERT INTO ""остановка"" VALUES (9,'Заводская');
INSERT INTO ""остановка"" VALUES (10,'Микрорайон Восточный');
INSERT INTO ""рейс"" VALUES (1,1,1,1,3,'2024-01-15',1,15000,'запланирован',NULL);
INSERT INTO ""рейс"" VALUES (2,2,2,2,5,'2024-01-15',2,18000,'запланирован',NULL);
INSERT INTO ""рейс"" VALUES (3,3,3,4,3,'2024-01-15',3,12000,'запланирован',NULL);
INSERT INTO ""рейс"" VALUES (4,4,4,6,7,'2024-01-16',1,16000,'запланирован',NULL);
INSERT INTO ""рейс"" VALUES (5,5,5,2,5,'2024-01-16',2,14000,'запланирован',NULL);
INSERT INTO ""рейс"" VALUES (6,6,6,1,3,'2024-01-16',3,17000,'запланирован',NULL);
INSERT INTO ""рейс"" VALUES (7,7,7,4,7,'2024-01-17',1,19000,'запланирован',NULL);
INSERT INTO ""рейс"" VALUES (8,8,9,6,5,'2024-01-17',2,13000,'запланирован',NULL);
INSERT INTO ""рейс"" VALUES (9,9,10,2,3,'2024-01-18',3,15000,'запланирован',NULL);
INSERT INTO ""рейс"" VALUES (10,10,1,1,7,'2024-01-18',1,20000,'запланирован',NULL);
INSERT INTO ""состояние_автобуса"" VALUES (1,'ИСПРАВЕН');
INSERT INTO ""состояние_автобуса"" VALUES (2,'В РЕМОНТЕ');
INSERT INTO ""состояние_автобуса"" VALUES (3,'НЕИСПРАВЕН');
INSERT INTO ""состояние_автобуса"" VALUES (4,'СПИСАН');
INSERT INTO ""сотрудник"" VALUES (1,'Иванов Иван Иванович','М','1980-05-15',1,1,45000,10,1,NULL);
INSERT INTO ""сотрудник"" VALUES (2,'Петров Петр Петрович','М','1985-08-20',2,1,42000,25,1,NULL);
INSERT INTO ""сотрудник"" VALUES (3,'Сидорова Мария Сергеевна','Ж','1990-03-10',3,2,35000,15,1,NULL);
INSERT INTO ""сотрудник"" VALUES (4,'Кузнецов Алексей Викторович','М','1978-12-05',4,1,48000,8,1,NULL);
INSERT INTO ""сотрудник"" VALUES (5,'Смирнова Ольга Дмитриевна','Ж','1988-07-18',5,2,34000,12,1,NULL);
INSERT INTO ""сотрудник"" VALUES (6,'Васильев Дмитрий Александрович','М','1983-11-25',6,1,43000,30,1,NULL);
INSERT INTO ""сотрудник"" VALUES (7,'Николаева Екатерина Павловна','Ж','1992-02-14',7,2,33000,5,1,NULL);
INSERT INTO ""сотрудник"" VALUES (8,'Алексеев Сергей Николаевич','М','1975-09-30',8,3,80000,18,1,NULL);
INSERT INTO ""сотрудник"" VALUES (9,'Федорова Анна Владимировна','Ж','1987-06-22',9,4,55000,22,1,NULL);
INSERT INTO ""сотрудник"" VALUES (10,'Дмитриев Андрей Игоревич','М','1981-04-08',10,6,60000,7,1,NULL);
INSERT INTO ""тип_смены"" VALUES (1,'Утренняя');
INSERT INTO ""тип_смены"" VALUES (2,'Дневная');
INSERT INTO ""тип_смены"" VALUES (3,'Вечерная');
INSERT INTO ""трудовая_книжка"" VALUES (1,1,1,4,1,12345,'не применимо');
INSERT INTO ""трудовая_книжка"" VALUES (2,2,1,5,1,12346,'не применимо');
INSERT INTO ""трудовая_книжка"" VALUES (3,3,2,3,1,12347,'не применимо');
INSERT INTO ""трудовая_книжка"" VALUES (4,4,1,6,1,12348,'не применимо');
INSERT INTO ""трудовая_книжка"" VALUES (5,5,2,2,1,12349,'не применимо');
INSERT INTO ""трудовая_книжка"" VALUES (6,6,1,5,1,12350,'не применимо');
INSERT INTO ""трудовая_книжка"" VALUES (7,7,2,1,1,12351,'не применимо');
INSERT INTO ""трудовая_книжка"" VALUES (8,8,3,9,1,12352,'не применимо');
INSERT INTO ""трудовая_книжка"" VALUES (9,9,4,7,1,12353,'не применимо');
INSERT INTO ""трудовая_книжка"" VALUES (10,10,6,8,1,12354,'не применимо');
INSERT INTO ""улица"" VALUES (1,'Ленина');
INSERT INTO ""улица"" VALUES (2,'Кирова');
INSERT INTO ""улица"" VALUES (3,'Гоголя');
INSERT INTO ""улица"" VALUES (4,'Пушкина');
INSERT INTO ""улица"" VALUES (5,'Советская');
INSERT INTO ""улица"" VALUES (6,'Мира');
INSERT INTO ""улица"" VALUES (7,'Гагарина');
INSERT INTO ""улица"" VALUES (8,'Комсомольская');
INSERT INTO ""улица"" VALUES (9,'Садовоя');
INSERT INTO ""улица"" VALUES (10,'Центральная');
INSERT INTO ""участок_маршрута"" VALUES (1,1,1,1,0);
INSERT INTO ""участок_маршрута"" VALUES (2,1,2,2,1500);
INSERT INTO ""участок_маршрута"" VALUES (3,1,3,3,3200);
INSERT INTO ""участок_маршрута"" VALUES (4,1,4,4,4800);
INSERT INTO ""участок_маршрута"" VALUES (5,1,5,5,6500);
INSERT INTO ""участок_маршрута"" VALUES (6,2,2,6,0);
INSERT INTO ""участок_маршрута"" VALUES (7,2,6,7,2200);
INSERT INTO ""участок_маршрута"" VALUES (8,2,7,8,4100);
INSERT INTO ""участок_маршрута"" VALUES (9,2,8,9,5800);
INSERT INTO ""участок_маршрута"" VALUES (10,2,9,10,7200);
INSERT INTO ""участок_маршрута"" VALUES (11,3,3,11,0);
INSERT INTO ""участок_маршрута"" VALUES (12,3,8,12,1800);
INSERT INTO ""участок_маршрута"" VALUES (13,3,10,13,3500);
INSERT INTO ""участок_маршрута"" VALUES (14,4,1,14,0);
INSERT INTO ""участок_маршрута"" VALUES (15,4,4,15,2700);
INSERT INTO ""участок_маршрута"" VALUES (16,4,6,16,4400);
INSERT INTO ""участок_маршрута"" VALUES (17,5,5,17,0);
INSERT INTO ""участок_маршрута"" VALUES (18,5,7,18,1900);
INSERT INTO ""участок_маршрута"" VALUES (19,5,9,19,3700);
INSERT INTO ""цвет"" VALUES (1,'Белый');
INSERT INTO ""цвет"" VALUES (2,'Желтый');
INSERT INTO ""цвет"" VALUES (3,'Синий');
INSERT INTO ""цвет"" VALUES (4,'Красный');
INSERT INTO ""цвет"" VALUES (5,'Зеленый');
INSERT INTO ""цвет"" VALUES (6,'Оранжевый');
INSERT INTO ""цвет"" VALUES (7,'Серый');
INSERT INTO ""цвет"" VALUES (8,'Черный');
INSERT INTO ""цвет"" VALUES (9,'Фиолетовый');
INSERT INTO ""цвет"" VALUES (10,'Голубой');
CREATE VIEW monthly_route_revenue AS
SELECT 
    р.маршрут_id,
    м.route_number,
    strftime('%Y-%m', р.дата_рейса) as месяц,
    COUNT(*) as количество_рейсов,
    SUM(р.плановая_выручка) as плановая_выручка,
    COALESCE(SUM(в.фактическая_выручка), 0) as фактическая_выручка
FROM рейс р
JOIN маршрут м ON р.маршрут_id = м.id
LEFT JOIN выручка_рейса в ON р.id = в.рейс_id
GROUP BY р.маршрут_id, strftime('%Y-%m', р.дата_рейса);
CREATE VIEW расчет_зарплаты AS
SELECT 
    e.id as сотрудник_id,
    e.full_name,
    d.position_name,
    d.базовый_оклад,
    d.процент_премии
FROM сотрудник e
JOIN должность d ON e.position_id = d.id
WHERE e.активен = 1;
CREATE INDEX IF NOT EXISTS ""idx_время_вождения_водитель"" ON ""время_вождения"" (
	""водитель_id""
);
CREATE INDEX IF NOT EXISTS ""idx_выручка_дата"" ON ""выручка_рейса"" (
	""дата_записи""
);
CREATE INDEX IF NOT EXISTS ""idx_история_дата"" ON ""история_кадровых_событий"" (
	""дата_события""
);
CREATE INDEX IF NOT EXISTS ""idx_история_сотрудник"" ON ""история_кадровых_событий"" (
	""сотрудник_id""
);
CREATE INDEX IF NOT EXISTS ""idx_рейс_дата"" ON ""рейс"" (
	""дата_рейса""
);
CREATE INDEX IF NOT EXISTS ""idx_рейс_статус"" ON ""рейс"" (
	""статус""
);
CREATE INDEX IF NOT EXISTS ""idx_смена_дата"" ON ""рабочая_смена"" (
	""дата_работы""
);
CREATE INDEX IF NOT EXISTS ""idx_сотрудник_активен"" ON ""сотрудник"" (
	""активен""
);
CREATE INDEX IF NOT EXISTS ""idx_техобслуживание_дата"" ON ""техобслуживание"" (
	""дата_обслуживания""
);
CREATE TRIGGER tr_автобус_состояние_изменение
AFTER UPDATE ON автобус
FOR EACH ROW
WHEN OLD.state_id IS NOT NEW.state_id
BEGIN
    INSERT INTO история_состояний_автобусов (автобус_id, состояние_id, дата_изменения)
    VALUES (NEW.id, NEW.state_id, CURRENT_TIMESTAMP);
END;
CREATE TRIGGER tr_сотрудник_прием
AFTER INSERT ON сотрудник
FOR EACH ROW
BEGIN
    INSERT INTO история_кадровых_событий (сотрудник_id, должность_id, тип_мероприятия_id, дата_события, базовый_оклад)
    VALUES (
        NEW.id, 
        NEW.position_id,
        COALESCE((SELECT id FROM вид_кадрового_мероприятия LIMIT 1), 1),
        CURRENT_DATE,
        COALESCE((SELECT базовый_оклад FROM должность WHERE id = NEW.position_id), 0)
    );
END;
COMMIT; ";
        }

        private void UpdateDatabaseStructure()
        {
            // Здесь можно добавить логику обновления структуры БД
            // Например, добавлять новые столбцы при обновлении приложения
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={_databasePath};Version=3;"))
                {
                    connection.Open();

                    // Проверяем существование таблиц и добавляем недостающие
                    CheckAndCreateTables(connection);

                    // Проверяем существование столбцов и добавляем недостающие
                    CheckAndUpdateColumns(connection);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при обновлении структуры БД: {ex.Message}");
            }
        }

        private void CheckAndCreateTables(SQLiteConnection connection)
        {
            // Проверяем существование основных таблиц
            var requiredTables = new[] { "улица", "должность", "сотрудник", "вид_кадрового_мероприятия", "история_кадровых_событий" };

            foreach (var tableName in requiredTables)
            {
                var checkTableSql = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
                using (var command = new SQLiteCommand(checkTableSql, connection))
                {
                    var result = command.ExecuteScalar();
                    if (result == null)
                    {
                        Debug.WriteLine($"Таблица {tableName} не найдена, требуется обновление структуры");
                        // Здесь можно вызвать метод для создания конкретной таблицы
                    }
                }
            }
        }

        private void CheckAndUpdateColumns(SQLiteConnection connection)
        {
            // Пример: проверяем наличие столбца experience_years в таблице сотрудник
            var checkColumnSql = @"
                SELECT COUNT(*) FROM pragma_table_info('сотрудник') 
                WHERE name='experience_years'";

            using (var command = new SQLiteCommand(checkColumnSql, connection))
            {
                var result = command.ExecuteScalar();
                if (result != null && Convert.ToInt32(result) == 0)
                {
                    // Столбец отсутствует, добавляем
                    var addColumnSql = @"ALTER TABLE сотрудник ADD COLUMN experience_years INTEGER DEFAULT 0";
                    using (var alterCommand = new SQLiteCommand(addColumnSql, connection))
                    {
                        alterCommand.ExecuteNonQuery();
                        Debug.WriteLine("Добавлен столбец experience_years в таблицу сотрудник");
                    }
                }
            }
        }

        public SQLiteConnection GetConnection()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DatabaseContext));

            var connection = new SQLiteConnection($"Data Source={_databasePath};Version=3;");
            connection.Open();
            return connection;
        }

        // Метод для получения пути к БД (для отладки)
        public string GetDatabasePath()
        {
            return _databasePath;
        }

        // Метод для получения информации о БД
        public string GetDatabaseInfo()
        {
            try
            {
                if (!File.Exists(_databasePath))
                    return "База данных не существует";

                var fileInfo = new FileInfo(_databasePath);
                return $"Путь: {_databasePath}\n" +
                       $"Размер: {fileInfo.Length} байт\n" +
                       $"Создана: {fileInfo.CreationTime}\n" +
                       $"Изменена: {fileInfo.LastWriteTime}";
            }
            catch (Exception ex)
            {
                return $"Ошибка получения информации: {ex.Message}";
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
        public string GetConnectionString()
        {
            return $"Data Source={_databasePath};Version=3;";
        }
        public bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    // Выполняем простой запрос для проверки
                    using (var command = new SQLiteCommand("SELECT 1", connection))
                    {
                        var result = command.ExecuteScalar();
                        return result != null;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}