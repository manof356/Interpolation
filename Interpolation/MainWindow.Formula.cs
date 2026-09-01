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
    public partial class MainWindow
    {
        // формула по умолчанию — без чисел, показывается, пока строка не выбрана
        private const string DefaultFormulaLatex =
            @"f(x)=f(x_1)+(x-x_1)\frac{f(x_2)-f(x_1)}{x_2-x_1}";
        private string _formulaLatex = DefaultFormulaLatex;

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
    }
}

