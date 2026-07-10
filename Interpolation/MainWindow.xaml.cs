using Interpolation.InterpMath;
using Interpolation.MyControls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography.Xml;
using System.Windows;

namespace Interpolation
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<ResultRow> _resultRows;
        public MainWindow()
        {
            InitializeComponent();

            // создаём 5 пустых строк для таблицы результатов
            _resultRows = new ObservableCollection<ResultRow>();
            for (int i = 0; i < 5; i++)
            {
                _resultRows.Add(new ResultRow());
            }
            linearResultDataGrid.ItemsSource = _resultRows;


            /*double testPoint = 0.5;
            DataPoint test1 = new DataPoint();
            test1.X =0.5; test1.Y =800;
            DataPoint test2 = new DataPoint();
            test2.X = 0.7; test2.Y = 650;
            DataPoint test3 = new DataPoint();
            test3.X = 1.0; test3.Y = 550;
            List<DataPoint> mainTable = new List<DataPoint> { test1, test2, test3 };
            List<DataPoint> result = LinearInterpolation.GetClosedPairs(mainTable, testPoint);
            foreach (DataPoint r in result)
            {
                Debug.WriteLine($"----------X = {r.X} Y = {r.Y}--------------");
            }
            (double x1,double y1,double x2,double y2) = LinearInterpolation.UnwrapTable(result);
            Debug.WriteLine($"x1 = {x1}, y1 = {y1}, x2 = {x2}, y2 = {y2}");
            double superResult = LinearInterpolation.Linterp(x1,y1,x2,y2, testPoint);
            Debug.WriteLine(superResult);*/

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


        public List<double> GetResultArguments()
        {
            var arguments = new List<double>();
            foreach (var row in _resultRows)
            {
                if (!TryParseCellAsDouble(row.Argument, out double x)) continue;
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

        // вытащить данные в командную строку по кнопке
        // todo. удалить потом
        private void GetDataButton_Click(object sender, RoutedEventArgs e)
        {
            List<DataPoint> points = GetInputData();
            double testPoint = 0.6;
            (double x1, double y1, double x2, double y2) = LinearInterpolation.UnwrapTable(points);
            double superResult = LinearInterpolation.Linterp(x1, y1, x2, y2, testPoint);
            Debug.WriteLine(superResult);
        }
    }
}