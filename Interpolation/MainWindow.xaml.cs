using Interpolation.InterpMath;
using Interpolation.MyControls.SelfGrowDataGrid;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace Interpolation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; // теперь биндинги видят свойства MainWindow
            Precision = 3; // точность по умолчанию — 3 знака после запятой
            // подписка на изменения ячеек обеих таблиц
            linearResultDataGrid.CellValueChanged += LinearResultDataGrid_CellValueChanged;
            linearInpDataGrid.CellValueChanged += LinearInpDataGrid_CellValueChanged;
        }

        // функция получения данных из таблицы исходных данных
        public List<DataPoint> GetInputData()
        {
            var points = new List<DataPoint>();

            // идём по всем строкам таблицы исходных данных
            foreach (var item in linearInpDataGrid.Items)
            {
                if (item is not SelfGrowingDataGridRow row) continue;

                // пропускаем пустую строку (последняя строка всегда пустая для роста таблицы)
                if (row.IsEmpty()) continue;

                // пробуем распарсить оба значения
                if (!TryParseCellAsDouble(row.Values[0].Value, out double x)) continue;
                if (!TryParseCellAsDouble(row.Values[1].Value, out double y)) continue;

                points.Add(new DataPoint { X = x, Y = y });
            }
            return points;
        }


        // достаёт столбец "Аргумент" из таблицы результатов
        public List<double> GetResultArguments()
        {
            var arguments = new List<double>();
            foreach (var item in linearResultDataGrid.Items)
            {
                if (item is not SelfGrowingDataGridRow row) continue;
                if (!TryParseCellAsDouble(row.Values[0].Value, out double x)) continue;
                arguments.Add(x);
            }
            return arguments;
        }

        // функция преобразования значений таблицы в double
        private bool TryParseCellAsDouble(string input, out double value)
        {
            input = input?.Replace(',', '.');
            return double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }


        // записывает список результатов во второй столбец таблицы результатов
        public void SetResultValues(List<double> results)
        {
            // снимок текущих строк — чтобы цикл не сломался, если коллекция
            // изменится во время расчёта (рост/удаление строк)
            var rows = linearResultDataGrid.Items.Cast<SelfGrowingDataGridRow>().ToList();

            int i = 0;
            foreach (var item in linearResultDataGrid.Items)
            {
                if (item is not SelfGrowingDataGridRow row) continue;
                if (i >= results.Count) break;
                row.Values[1].RawValue = results[i]; // точное число — источник правды
                row.Values[1].Value = FormatResult(results[i], Precision); // строка для отображения
                i++;
            }
        }

        // Округляет число по правилам математики (0.5 всегда вверх) до заданного
        // кол-ва знаков после запятой и обрезает хвостовые нули
        public static string FormatResult(double value, int precision)
        {
            // на всякий случай ограничиваем диапазон 0..15
            precision = Math.Clamp(precision, 0, 15);

            double rounded = Math.Round(value, precision, MidpointRounding.AwayFromZero);

            // "0.###############" — 0 обязательная цифра до запятой,
            // # после — необязательные, лишние нули обрезаются сами
            string format = "0." + new string('#', precision);

            return rounded.ToString(format, CultureInfo.InvariantCulture);
        }

        // вытащить данные в таблицу результатов
        private void RecalculateResults()
        {
            List<DataPoint> points = GetInputData();
            List<double> testPoints = GetResultArguments();

            // для интерполяции нужно минимум 2 исходные точки
            if (points.Count < 2)
            {
                ClearResultValues();
                return;
            }

            List<double> result = LinearInterpolation.LinterpList(points, testPoints);
            SetResultValues(result);
        }

        // очищает столбец "Результат" — без исходных данных считать нечего
        private void ClearResultValues()
        {
            foreach (var item in linearResultDataGrid.Items)
            {
                if (item is not SelfGrowingDataGridRow row) continue;
                row.Values[1].RawValue = null;
                row.Values[1].Value = string.Empty;
            }
        }


        // Метод, который "объявляет" — вот это свойство изменилось
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _precision;
        public int Precision
        {
            get => _precision;
            set
            {
                _precision = value;
                OnPropertyChanged(nameof(Precision));
                RecalculateResultStrings();
            }
        }

        private void RecalculateResultStrings()
        {
            foreach (var item in linearResultDataGrid.Items)
            {
                if (item is not SelfGrowingDataGridRow row) continue;
                if (row.Values[1].RawValue is not double raw) continue; // пропускаем пустые ячейки

                row.Values[1].Value = FormatResult(raw, Precision);
            }
        }


        // пересчёт по изменению столбца "Аргумент" в таблице результатов — всегда
        private void LinearResultDataGrid_CellValueChanged(SelfGrowingDataGridRow row, int columnIndex)
        {
            if (columnIndex != 0) return; // реагируем только на столбец "Аргумент"
            RecalculateResults();
        }

        // пересчёт по изменению исходных данных — только если результаты уже начали заполняться
        private void LinearInpDataGrid_CellValueChanged(SelfGrowingDataGridRow row, int columnIndex)
        {
            if (!AnyResultArgumentFilled()) return;
            RecalculateResults();
        }
        // проверка: есть ли хотя бы одна заполненная ячейка "Аргумент" в таблице результатов
        private bool AnyResultArgumentFilled()
        {
            foreach (var item in linearResultDataGrid.Items)
            {
                if (item is not SelfGrowingDataGridRow row) continue;
                if (!string.IsNullOrWhiteSpace(row.Values[0].Value)) return true;
            }
            return false;
        }
    }
}