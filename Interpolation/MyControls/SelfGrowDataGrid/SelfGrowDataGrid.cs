using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Linq;

namespace Interpolation.MyControls
{
    public class SelfGrowingDataGrid : DataGrid
    {
        private ObservableCollection<SelfGrowingDataGridRow> _rows;

        public SelfGrowingDataGrid()
        {
            AutoGenerateColumns = false;
            SelectionUnit = DataGridSelectionUnit.Cell;
        }

        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);

            int columnsCount = Columns.Count;

            _rows = new ObservableCollection<SelfGrowingDataGridRow>();
            AddNewRow(columnsCount);

            ItemsSource = _rows;
        }

        private void AddNewRow(int columnsCount)
        {
            var row = new SelfGrowingDataGridRow(columnsCount);

            // Подписываемся на изменение каждой ячейки в этой строке
            foreach (var cell in row.Values)
            {
                cell.PropertyChanged += Cell_PropertyChanged;
            }
            _rows.Add(row);
        }

        private void Cell_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Находим строку, которой принадлежит изменившаяся ячейка
            var changedCell = sender as SelfGrowingDataGridCell;
            var row = _rows.FirstOrDefault(r => r.Values.Contains(changedCell));

            if (row == null) return;

            bool isLastRow = _rows.IndexOf(row) == _rows.Count - 1;

            if (row.IsFilled() && isLastRow)
            {
                // Последняя строка заполнена полностью → добавляем новую пустую
                AddNewRow(Columns.Count);
            }
            else if (row.IsEmpty() && !isLastRow)
            {
                // Строка (не последняя) стала пустой → удаляем её
                foreach (var cell in row.Values)
                {
                    cell.PropertyChanged -= Cell_PropertyChanged; // отписываемся
                }
                _rows.Remove(row);
            }
        }
    }
}