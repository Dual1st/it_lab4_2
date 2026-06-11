using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using FlightApp.Commands;
using FlightLibrary.Models;

namespace FlightApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> LogMessages { get; } = [];
        public ObservableCollection<string> ClassNames { get; } = [];
        public ObservableCollection<MethodInfoViewModel> Methods { get; } = [];
        public ObservableCollection<ParameterViewModel> Parameters { get; } = [];

        private string _libraryPath = string.Empty;
        private string _selectedClassName = string.Empty;
        private MethodInfoViewModel? _selectedMethod;
        private Type[] _vehicleTypes = [];

        public MainViewModel()
        {
            LoadLibraryCommand = new Command(LoadLibrary, CanLoadLibrary);
            SelectClassCommand = new Command(SelectClass, CanSelectClass);
            SelectMethodCommand = new Command(SelectMethod, CanSelectMethod);
            ExecuteMethodCommand = new Command(ExecuteMethod, CanExecuteMethod);

            AddLog("Приложение готово. Введите путь к FlightLibrary.dll и нажмите Загрузить.");
        }

        public string LibraryPath
        {
            get => _libraryPath;
            set
            {
                _libraryPath = value;
                OnPropertyChanged();
                Command.RaiseCanExecuteChanged();
            }
        }

        public string SelectedClassName
        {
            get => _selectedClassName;
            set
            {
                _selectedClassName = value;
                OnPropertyChanged();
                Command.RaiseCanExecuteChanged();
            }
        }

        public MethodInfoViewModel? SelectedMethod
        {
            get => _selectedMethod;
            set
            {
                _selectedMethod = value;
                OnPropertyChanged();
                LoadParameters();
                Command.RaiseCanExecuteChanged();
            }
        }

        public ICommand LoadLibraryCommand { get; }
        public ICommand SelectClassCommand { get; }
        public ICommand SelectMethodCommand { get; }
        public ICommand ExecuteMethodCommand { get; }

        private bool CanLoadLibrary(object? parameter) => 
           !string.IsNullOrWhiteSpace(LibraryPath) && File.Exists(LibraryPath);

        private void LoadLibrary(object? parameter)
        {
            try
            {
                AddLog($"Загрузка библиотеки: {LibraryPath}");
                var assembly = Assembly.LoadFrom(LibraryPath);

                _vehicleTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract &&
                           (typeof(IFlyingVehicle).IsAssignableFrom(t) ||
                            typeof(FlyingVehicle).IsAssignableFrom(t) && t != typeof(FlyingVehicle)))
                    .ToArray();

                ClassNames.Clear();
                foreach (var type in _vehicleTypes)
                {
                    ClassNames.Add(type.Name);
                }

                AddLog($"Библиотека загружена. Найдено классов: {_vehicleTypes.Length}");
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка загрузки: {ex.Message}");
            }
        }

        private bool CanSelectClass(object? parameter) => !string.IsNullOrWhiteSpace(SelectedClassName);

        private void SelectClass(object? parameter)
        {
            AddLog($"=== SelectClass вызван ===");
            AddLog($"SelectedClassName = '{SelectedClassName}'");
            AddLog($"Всего классов в массиве: {_vehicleTypes.Length}");

            try
            {
                var selectedType = _vehicleTypes.FirstOrDefault(t => t.Name == SelectedClassName);

                if (selectedType == null)
                {
                    AddLog($"❌ ОШИБКА: Класс '{SelectedClassName}' не найден в массиве!");
                    AddLog($"Доступные классы: {string.Join(", ", _vehicleTypes.Select(t => t.Name))}");
                    return;
                }

                AddLog($"✓ Выбран класс: {selectedType.Name}");
                AddLog($"  Полный тип: {selectedType.FullName}");
                AddLog($"  Базовый класс: {selectedType.BaseType?.Name ?? "нет"}");

                Methods.Clear();

                var methodInfos = selectedType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                AddLog($"  Найдено методов: {methodInfos.Length}");

                foreach (var method in methodInfos)
                {
                    AddLog($"    - {method.Name} (SpecialName: {method.IsSpecialName})");

                    if (!method.IsSpecialName)
                    {
                        Methods.Add(new MethodInfoViewModel(method));
                    }
                }

                AddLog($"✓ Добавлено в список: {Methods.Count} методов");
            }
            catch (Exception ex)
            {
                AddLog($"❌ Ошибка: {ex.Message}");
                AddLog($"  Детали: {ex.InnerException?.Message}");
            }
        }

        private bool CanSelectMethod(object? parameter) => SelectedMethod != null;

        private void SelectMethod(object? parameter)
        {
            AddLog($"=== SelectMethod вызван ===");
            LoadParameters();
        }

        private void LoadParameters()
        {
            Parameters.Clear();
            if (SelectedMethod == null)
            {
                AddLog("Нет выбранного метода");
                return;
            }

            var parameters = SelectedMethod.MethodInfo.GetParameters();
            AddLog($"Метод {SelectedMethod.MethodInfo.Name} имеет {parameters.Length} параметров");

            foreach (var param in parameters)
            {
                Parameters.Add(new ParameterViewModel
                {
                    Name = param.Name ?? string.Empty,
                    ParameterType = param.ParameterType,
                    Value = GetDefaultValue(param.ParameterType)
                });
                AddLog($"  Параметр: {param.Name} ({param.ParameterType.Name})");
            }
        }

        private static object? GetDefaultValue(Type type)
        {
            if (type == typeof(string)) return string.Empty;
            if (type == typeof(int)) return 0;
            if (type == typeof(double)) return 0.0;
            if (type == typeof(bool)) return false;
            if (type.IsValueType) return Activator.CreateInstance(type);
            return null;
        }

        private bool CanExecuteMethod(object? parameter) => SelectedMethod != null;

        private void ExecuteMethod(object? parameter)
        {
            try
            {
                var selectedType = _vehicleTypes.FirstOrDefault(t => t.Name == SelectedClassName);
                if (selectedType == null)
                {
                    AddLog("❌ Класс не найден");
                    return;
                }

                AddLog($"=== Создание объекта {selectedType.Name} ===");
                var instance = Activator.CreateInstance(selectedType);

                var nameProp = selectedType.GetProperty("Name");
                if (nameProp?.CanWrite == true)
                {
                    nameProp.SetValue(instance, "Test_Vehicle");
                    AddLog($"Установлено свойство Name = Test_Vehicle");
                }

                var runwayProp = selectedType.GetProperty("RunwayLength");
                if (runwayProp?.CanWrite == true)
                {
                    runwayProp.SetValue(instance, 3000.0);
                    AddLog($"Установлено свойство RunwayLength = 3000");
                }

                var methodParams = SelectedMethod!.MethodInfo.GetParameters();
                var args = methodParams.Select(p =>
                {
                    var paramVM = Parameters.FirstOrDefault(v => v.Name == p.Name);
                    var value = paramVM?.Value != null ? Convert.ChangeType(paramVM.Value, p.ParameterType) : null;
                    AddLog($"  Аргумент {p.Name}: {value ?? "null"}");
                    return value;
                }).ToArray();

                AddLog($">>> Выполнение метода {SelectedMethod.MethodInfo.Name}...");
                var result = SelectedMethod.MethodInfo.Invoke(instance, args);

                AddLog($"✓ Результат: {result ?? "Void"}");

                var heightProp = selectedType.GetProperty("Height");
                if (heightProp != null)
                {
                    var height = heightProp.GetValue(instance);
                    AddLog($"Текущая высота: {height}м");
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ Ошибка выполнения: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        private void AddLog(string message)
        {
            string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            Application.Current.Dispatcher.Invoke(() => LogMessages.Add(logEntry));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class MethodInfoViewModel
    {
        public MethodInfoViewModel(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            var pars = methodInfo.GetParameters();
            string parStr = pars.Length > 0 ? string.Join(", ", pars.Select(p => $"{p.ParameterType.Name} {p.Name}")) : "";
            DisplayName = $"{methodInfo.Name}({parStr})";
        }

        public MethodInfo MethodInfo { get; }
        public string DisplayName { get; }
        public override string ToString() => DisplayName;
    }

    public class ParameterViewModel : INotifyPropertyChanged
    {
        private object? _value;
        public string Name { get; init; } = string.Empty;
        public Type ParameterType { get; init; } = typeof(object);

        public object? Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? p = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}