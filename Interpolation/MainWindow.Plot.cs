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
        private PlotModel _linearPlotModel;
        private void InitializePlot()
        {
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
                MarkerFill = OxyColor.FromRgb(2, 37, 164) // цвет заливки точки
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

            // ------------------ПОДПИСЬ ТОЧКИ У ТОЧКИ--------------------------
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
            // ------------------ПОДПИСЬ ТОЧКИ У ТОЧКИ--------------------------

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
    }
}
