using Interpolation.InterpMath;
using System.Diagnostics;
using System.Security.Cryptography.Xml;
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

            double testPoint = 0.5;

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
            Debug.WriteLine(superResult);

        }
    }
}