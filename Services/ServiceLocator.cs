using BusParkManagementSystem.Data;
using BusParkManagementSystem.Models;
using BusParkManagementSystem.Repositories;
using BusParkManagementSystem.ViewModels;
using System;
using System.Collections.Generic;

namespace BusParkManagementSystem.Services
{
    /// <summary>
    /// Упрощенный ServiceLocator без Dependency Injection
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static bool _isInitialized = false;

        static ServiceLocator()
        {
            Initialize();
        }

        private static void Initialize()
        {
            if (_isInitialized) return;

            // Создаем экземпляры основных сервисов
            var dbContext = new DatabaseContext();

            // Регистрируем контекст базы данных
            Register<DatabaseContext>(dbContext);

            // Регистрируем репозитории
            var lookupRepository = new LookupRepository(dbContext);
            Register<ILookupRepository>(lookupRepository);

            // Регистрируем ViewModel справочников
            Register<ModelViewModel>(new ModelViewModel(lookupRepository));
            Register<ColorViewModel>(new ColorViewModel(lookupRepository));
            Register<BusStateViewModel>(new BusStateViewModel(lookupRepository));
            Register<PositionViewModel>(new PositionViewModel(lookupRepository));
            Register<ShiftTypeViewModel>(new ShiftTypeViewModel(lookupRepository));
            Register<StreetViewModel>(new StreetViewModel(lookupRepository));
            Register<BusStopViewModel>(new BusStopViewModel(lookupRepository));
            Register<PersonnelEventTypeViewModel>(new PersonnelEventTypeViewModel(lookupRepository));

            _isInitialized = true;
        }

        /// <summary>
        /// Регистрация сервиса
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            _services[typeof(T)] = service;
        }

        /// <summary>
        /// Получение сервиса
        /// </summary>
        public static T GetService<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"Сервис типа {typeof(T).Name} не зарегистрирован");
        }

        /// <summary>
        /// Получение сервиса по типу
        /// </summary>
        public static object GetService(Type serviceType)
        {
            if (_services.TryGetValue(serviceType, out var service))
            {
                return service;
            }

            throw new InvalidOperationException($"Сервис типа {serviceType.Name} не зарегистрирован");
        }

        /// <summary>
        /// Проверка регистрации сервиса
        /// </summary>
        public static bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }
    }
}