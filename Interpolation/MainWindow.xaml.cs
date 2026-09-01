using Interpolation.InterpMath;
using Interpolation.MyControls.SelfGrowDataGrid;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Annotations;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Interpolation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private GridLength lastFormulaHeight = new GridLength(100);
        private GridLength lastGraphHeight = new GridLength(250);

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

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; // теперь биндинги видят свойства MainWindow
            // подписка на изменения ячеек обеих таблиц
            linearResultDataGrid.CellValueChanged += LinearResultDataGrid_CellValueChanged;
            linearInpDataGrid.CellValueChanged += LinearInpDataGrid_CellValueChanged;
            linearResultDataGrid.CurrentCellChanged += (s, e) => UpdateSelectedRowDisplay();

            InitializePlot();
            Precision = 3; // точность по умолчанию — 3 знака после запятой
        }

        // функция получения данных из таблицы исходных данных
        public List<InterpPoint> GetInputData()
        {
            var points = new List<InterpPoint>();
            // идём по всем строкам таблицы исходных данных
            foreach (var item in linearInpDataGrid.Items)
            {
                if (item is not SelfGrowingDataGridRow row) continue;
                // пропускаем пустую строку (последняя строка всегда пустая для роста таблицы)
                if (row.IsEmpty()) continue;
                // пробуем распарсить оба значения
                if (!TryParseCellAsDouble(row.Values[0].Value, out double x)) continue;
                if (!TryParseCellAsDouble(row.Values[1].Value, out double y)) continue;
                points.Add(new InterpPoint { X = x, Y = y });
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
            // сброс статуса "Скопировано" — данные могли измениться
            foreach (var item in linearResultDataGrid.Items)
            {
                if (item is SelfGrowingDataGridRow row)
                    row.Values[3].Value = string.Empty;
            }
            List<InterpPoint> points = GetInputData();
            List<double> testPoints = GetResultArguments();
            // для интерполяции нужно минимум 2 исходные точки
            if (points.Count < 2)
            {
                ClearResultValues();
                return;
            }
            List<double> result = LinearInterpolation.LinterpList(points, testPoints);
            SetResultValues(result);
            UpdateSelectedRowDisplay();
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
            UpdateSelectedRowDisplay();
        }

        // Метод, который "объявляет" — вот это свойство изменилось
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RecalculateResultStrings()
        {
            foreach (var item in linearResultDataGrid.Items)
            {
                if (item is not SelfGrowingDataGridRow row) continue;
                if (row.Values[1].RawValue is not double raw) continue; // пропускаем пустые ячейки

                row.Values[1].Value = FormatResult(raw, Precision);
                row.Values[3].Value = string.Empty; // формат результата поменялся — статус устарел
            }
            UpdateSelectedRowDisplay();
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
            UpdatePlot(); // график обновляется всегда, независимо от того, заполнены ли результаты
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
        //  скопировать содержимое таблицы результатов
        private void CopyAllButton_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            var copiedRows = new List<SelfGrowingDataGridRow>();
            foreach (var item in linearResultDataGrid.Items)
            {
                if (item is not SelfGrowingDataGridRow row) continue;
                string arg = row.Values[0].Value;
                string result = row.Values[1].Value;
                // пропускаем пустую (растущую) строку в конце
                if (string.IsNullOrWhiteSpace(arg) && string.IsNullOrWhiteSpace(result))
                    continue;
                // приводим разделитель дробной части к точке — для Excel
                arg = arg?.Replace(',', '.');
                result = result?.Replace(',', '.');
                // столбцы через Tab — Excel сам разобьёт по ячейкам
                sb.Append(arg).Append('\t').Append(result).Append(Environment.NewLine);
                copiedRows.Add(row); 
            }
            if (sb.Length > 0)
                Clipboard.SetText(sb.ToString());
                // проставляем статус только тем строкам, что реально скопировались
                foreach (var row in copiedRows)
                    row.Values[3].Value = "Скопировано";
        }

        // копирует результат одной строки в буфер обмена
        private void CopyRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Tag is not SelfGrowingDataGridRow row) return;
            string result = row.Values[1].Value?.Replace(',', '.');
            if (string.IsNullOrWhiteSpace(result)) return;
            Clipboard.SetText(result);
            // сбрасываем статус у всех строк — копирование одной строки отменяет предыдущий статус
            foreach (var item in linearResultDataGrid.Items)
            {
                if (item is SelfGrowingDataGridRow r)
                    r.Values[3].Value = string.Empty;
            }
            row.Values[3].Value = "Скопировано";
        }

        // вызывается везде, где меняется выбранная строка или её результат —
        // обновляет и формулу, и точку на графике вместе
        private void UpdateSelectedRowDisplay()
        {
            UpdateFormula();
            UpdateResultPointOnPlot();
        }
        // скрыть строку grid (общая функция)
        private void ToggleRow(RowDefinition row, MenuItem menuItem, ref GridLength lastHeight, string showText, string hideText)
        {
            bool isVisible = row.Height.Value > 0;
            if (isVisible)
            {
                lastHeight = row.Height;
                row.Height = new GridLength(0);
            }
            else
                row.Height = lastHeight;
            menuItem.Header = isVisible ? showText : hideText;
        }
        // показать/скрыть формулу
        private void ToggleFormula_Click(object sender, RoutedEventArgs e)
        {
            ToggleRow(formulaRow, ToggleFormulaMenuItem, ref lastFormulaHeight, "Показать формулу", "Скрыть формулу");
        }
        // показать скрыть график
        private void ToggleGraph_Click(object sender, RoutedEventArgs e)
        {
            ToggleRow(graphRow, ToggleGraphMenuItem, ref lastGraphHeight, "Показать график", "Скрыть график");
        }
    }
}