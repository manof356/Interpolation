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

        // формула по умолчанию — без чисел, показывается, пока строка не выбрана
        private const string DefaultFormulaLatex =
            @"f(x)=f(x_1)+(x-x_1)\frac{f(x_2)-f(x_1)}{x_2-x_1}";
        private string _formulaLatex = DefaultFormulaLatex;
        private readonly PlotModel _linearPlotModel;
        private GridLength lastFormulaHeight = new GridLength(100);
        private GridLength lastGraphHeight = new GridLength(250);
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; // теперь биндинги видят свойства MainWindow
            // подписка на изменения ячеек обеих таблиц
            linearResultDataGrid.CellValueChanged += LinearResultDataGrid_CellValueChanged;
            linearInpDataGrid.CellValueChanged += LinearInpDataGrid_CellValueChanged;
            linearResultDataGrid.CurrentCellChanged += (s, e) => UpdateSelectedRowDisplay();

            // переменные для графика
            _linearPlotModel = new PlotModel();
            
            _linearPlotModel.Axes.Add(new LinearAxis 
            { 
                Position = AxisPosition.Bottom, 
                Title = "Аргумент",
                MinimumPadding = 0.05, // отступ 5% с каждой стороны
                MaximumPadding = 0.05,
                IsPanEnabled = false,
                IsZoomEnabled = false
            });

            _linearPlotModel.Axes.Add(new LinearAxis 
            { 
                Position = AxisPosition.Left, 
                Title = "Значение/результат",
                MinimumPadding = 0.05,
                MaximumPadding = 0.05,
                IsPanEnabled = false,
                IsZoomEnabled = false
            });
            linearPlotView.Model = _linearPlotModel;
            var controller = new PlotController();
            controller.UnbindAll(); // снимаем все стандартные привязки мыши/колеса
            linearPlotView.Controller = controller;
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

        public string FormulaLatex
        {
            get => _formulaLatex;
            set
            {
                _formulaLatex = value;
                OnPropertyChanged(nameof(FormulaLatex));
            }
        }

        // решает какую формулу показать — дефолтную или с числами выбранной строки
        private void UpdateFormula()
        {
            // нет выбранной строки или в ней ещё нет результата — дефолт
            if (linearResultDataGrid.CurrentCell.Item is not SelfGrowingDataGridRow row
                || row.Values[1].RawValue is not double y3)
            {
                FormulaLatex = DefaultFormulaLatex;
                return;
            }

            if (!TryParseCellAsDouble(row.Values[0].Value, out double x3))
            {
                FormulaLatex = DefaultFormulaLatex;
                return;
            }

            List<InterpPoint> points = GetInputData();
            if (points.Count < 2) // мало исходных данных — дефолт
            {
                FormulaLatex = DefaultFormulaLatex;
                return;
            }

            // берём ту же пару точек, что использовалась для расчёта
            List<InterpPoint> pair = LinearInterpolation.GetClosedPairs(points, x3);
            (double x1, double y1, double x2, double y2) = LinearInterpolation.UnwrapTable(pair);

            FormulaLatex = BuildFormulaLatex(x1, y1, x2, y2, x3, y3);
        }

        // собирает LaTeX-строку с реальными числами, округлёнными по текущей точности
        private string BuildFormulaLatex(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            string sX1 = FormatResult(x1, Precision);
            string sY1 = FormatResult(y1, Precision);
            string sX2 = FormatResult(x2, Precision);
            string sY2 = FormatResult(y2, Precision);
            string sX3 = FormatResult(x3, Precision);
            string sY3 = FormatResult(y3, Precision);

            return $@"f({sX3})={sY1}+({sX3}-{sX1})\frac{{{sY2}-{sY1}}}{{{sX2}-{sX1}}}={sY3}";
        }
        // метод получения исходных данных из таблицы
        private List<OxyPlot.DataPoint> GetSourcePoints()
        {
            return GetInputData()
                .Select(p => new OxyPlot.DataPoint(p.X, p.Y))
                .OrderBy(p => p.X)
                .ToList();
        }
        // обновляем (перерисовываем) график на исходные данные
        private void UpdatePlot()
        {
            var points = GetSourcePoints();

            _linearPlotModel.Series.Clear(); // убираем старую серию перед перерисовкой

            if (points.Count == 0)
            {
                _linearPlotModel.InvalidatePlot(false); // пустой график, нечего рисовать
                return;
            }

            var series = new LineSeries()
            {
                Color = OxyColors.Black,                // цвет линии
                StrokeThickness = 1,                    // толщина линии (по умолчанию 2)
                MarkerType = MarkerType.Circle,         // тип точки на линии
                MarkerSize = 4,                         // размер точки в пикселях
                MarkerFill = OxyColor.FromRgb(2,37,164) // цвет заливки точки
            };

            if (points.Count == 1)
                series.Points.Add(points[0]); // одна точка — линию рисовать не из чего
            else
                series.Points.AddRange(points); // 2+ точек — рисуем линию

            _linearPlotModel.Series.Add(series);
            _linearPlotModel.InvalidatePlot(true); // true — данные изменились, перерисовать заново
        }
        // обновляем (перерисовываем) график на результат
        private void UpdateResultPointOnPlot()
        {
            // убираем старую точку результата и старый крестик — рисуем заново каждый раз
            var oldResultSeries = _linearPlotModel.Series.FirstOrDefault(s => s.Tag as string == "resultPoint");
            if (oldResultSeries != null)
                _linearPlotModel.Series.Remove(oldResultSeries);

            RemoveAnnotationsByTag("resultPoint");

            // нет выбранной строки или в ней ещё нет результата — точку не показываем
            if (linearResultDataGrid.CurrentCell.Item is not SelfGrowingDataGridRow row
                || row.Values[1].RawValue is not double y)
            {
                _linearPlotModel.InvalidatePlot(true);
                return;
            }

            if (!TryParseCellAsDouble(row.Values[0].Value, out double x))
            {
                _linearPlotModel.InvalidatePlot(true);
                return;
            }

            var resultSeries = new ScatterSeries
            {
                Tag = "resultPoint",
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerFill = OxyColor.FromRgb(177, 0, 0)
            };
            resultSeries.Points.Add(new ScatterPoint(x, y));
            _linearPlotModel.Series.Add(resultSeries);
            linearPlotView.InvalidatePlot(true); // просим PlotView перерисовать
            linearPlotView.UpdateLayout();       // форсируем WPF пересчитать layout синхронно
            AddResultCrosshair(x, y); // пунктирные линии + подписи на осях

            _linearPlotModel.InvalidatePlot(true);
        }

        // убирает все аннотации с заданной меткой (используется перед перерисовкой)
        private void RemoveAnnotationsByTag(string tag)
        {
            var old = _linearPlotModel.Annotations.Where(a => a.Tag as string == tag).ToList();
            foreach (var a in old)
                _linearPlotModel.Annotations.Remove(a);
        }

        // рисует пунктирные линии от точки результата к осям и подписывает значения
        private void AddResultCrosshair(double x, double y)
        {
            var xAxis = _linearPlotModel.Axes.First(a => a.Position == AxisPosition.Bottom);
            var yAxis = _linearPlotModel.Axes.First(a => a.Position == AxisPosition.Left);

            // ------------------ТЕСТ--------------------------
            /*var pointLabel = new TextAnnotation
            {
                Text = $"{FormatResult(x, Precision)}, {FormatResult(y, Precision)}",
                TextPosition = new OxyPlot.DataPoint(x, y),
                Offset = new ScreenVector(8, -8), // небольшой сдвиг в пикселях, чтобы не закрывать саму точку
                TextVerticalAlignment = OxyPlot.VerticalAlignment.Bottom,
                TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left,
                Stroke = OxyColors.Transparent,
                Background = OxyColors.White,
                Tag = "resultPoint"
            };*/
            // ------------------ТЕСТ--------------------------

            // вертикальная пунктирная линия: от оси X вверх до точки
            var vLine = new LineAnnotation
            {
                Type = LineAnnotationType.Vertical,
                X = x,
                MinimumY = yAxis.ActualMinimum,
                MaximumY = y,
                Color = OxyColors.Gray,
                LineStyle = LineStyle.Dash,
                StrokeThickness = 1,
                Tag = "resultPoint"
            };

            // горизонтальная пунктирная линия: от оси Y вправо до точки
            var hLine = new LineAnnotation
            {
                Type = LineAnnotationType.Horizontal,
                Y = y,
                MinimumX = xAxis.ActualMinimum,
                MaximumX = x,
                Color = OxyColors.Gray,
                LineStyle = LineStyle.Dash,
                StrokeThickness = 1,
                Tag = "resultPoint"
            };

            // подпись значения X под осью
            /*var xLabel = new TextAnnotation
            {
                Text = FormatResult(x, Precision),
                // чуть выше нижней границы графика, а не ровно на ней — остаёмся внутри области
                TextPosition = new OxyPlot.DataPoint(x, yAxis.ActualMinimum + (yAxis.ActualMaximum - yAxis.ActualMinimum) * 0.02),
                TextVerticalAlignment = OxyPlot.VerticalAlignment.Bottom,
                TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Center,
                Stroke = OxyColors.Transparent,
                Background = OxyColors.White, // перекрывает линии/точки под собой
                Tag = "resultPoint"
            };

            // подпись значения Y слева от оси
            var yLabel = new TextAnnotation
            {
                Text = FormatResult(y, Precision),
                // чуть правее левой границы графика
                TextPosition = new OxyPlot.DataPoint(xAxis.ActualMinimum + (xAxis.ActualMaximum - xAxis.ActualMinimum) * 0.02, y),
                TextVerticalAlignment = OxyPlot.VerticalAlignment.Middle,
                TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left,
                Stroke = OxyColors.Transparent,
                Background = OxyColors.White,
                Tag = "resultPoint"
            };*/

            _linearPlotModel.Annotations.Add(vLine);
            _linearPlotModel.Annotations.Add(hLine);
            //_linearPlotModel.Annotations.Add(pointLabel);
            /*_linearPlotModel.Annotations.Add(xLabel);
            _linearPlotModel.Annotations.Add(yLabel);*/
        }

        // вызывается везде, где меняется выбранная строка или её результат —
        // обновляет и формулу, и точку на графике вместе
        private void UpdateSelectedRowDisplay()
        {
            UpdateFormula();
            UpdateResultPointOnPlot();
        }

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

        private void ToggleFormula_Click(object sender, RoutedEventArgs e)
        {
            ToggleRow(formulaRow, ToggleFormulaMenuItem, ref lastFormulaHeight, "Показать формулу", "Скрыть формулу");
        }

        private void ToggleGraph_Click(object sender, RoutedEventArgs e)
        {
            ToggleRow(graphRow, ToggleGraphMenuItem, ref lastGraphHeight, "Показать график", "Скрыть график");
        }
    }
}