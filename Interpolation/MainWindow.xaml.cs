using Interpolation.InterpMath;
using Interpolation.MyControls.SelfGrowDataGrid;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;

namespace Interpolation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            // создаём 5 пустых строк для таблицы результатов
            /*_resultRows = new ObservableCollection<ResultRow>();
            for (int i = 0; i < 5; i++)
            {
                _resultRows.Add(new ResultRow());
            }
            linearResultDataGrid.ItemsSource = _resultRows;*/
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
            int i = 0;
            foreach (var item in linearResultDataGrid.Items)
            {
                if (item is not SelfGrowingDataGridRow row) continue;
                if (i >= results.Count) break;
                row.Values[1].Value = results[i].ToString(CultureInfo.InvariantCulture);
                i++;
            }
        }

        // вытащить данные в таблицу результатов
        private void GetDataButton_Click(object sender, RoutedEventArgs e)
        {
            List<DataPoint> points = GetInputData();
            List<double> testPoints = GetResultArguments();
            List<double> result = LinearInterpolation.LinterpList(points, testPoints);
            SetResultValues(result);
        }
    }
}