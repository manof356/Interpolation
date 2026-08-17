using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Interpolation.MyControls.SelfGrowDataGrid
{
    public class SelfGrowingDataGrid : DataGrid
    {
        private ObservableCollection<SelfGrowingDataGridRow> _rows;
        // Заменяем _isDoubleClick на постоянный флаг режима
        private bool _isTextEditMode = false;
        private bool _mouseInitiatedFocus = false;

        public SelfGrowingDataGrid()
        {
            AutoGenerateColumns = false;
            SelectionUnit = DataGridSelectionUnit.Cell;
            SelectionMode = DataGridSelectionMode.Single;
            CanUserAddRows = false;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            int columnsCount = Columns.Count;
            _rows = new ObservableCollection<SelfGrowingDataGridRow>();
            AddNewRow(columnsCount);
            ItemsSource = _rows;
        }

        // При фокусе на ячейку — сразу включаем редактирование
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (e.OriginalSource is DataGridCell cell && !cell.IsEditing && !cell.IsReadOnly)
            {
                // Если фокус пришёл не от клика мыши (Tab, переход стрелками) —
                // всегда возвращаемся в режим выделения ячейки
                if (!_mouseInitiatedFocus)
                {
                    _isTextEditMode = false;
                }
                _mouseInitiatedFocus = false;

                BeginEdit();
            }
        }

        // Отслеживаем двойной клик
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // Помечаем, что фокус сейчас придёт от клика мыши (а не от Tab/стрелок)
            _mouseInitiatedFocus = true;
            _isTextEditMode = (e.ClickCount == 2);

            base.OnPreviewMouseLeftButtonDown(e);
        }

        // Настройка курсора при входе в режим редактирования
        protected override void OnPreparingCellForEdit(DataGridPreparingCellForEditEventArgs e)
        {
            base.OnPreparingCellForEdit(e);

            if (e.EditingElement is TextBox textBox)
            {
                if (!_isTextEditMode)
                {
                    // Чтобы при вводе "10" единица не выделялась и не затиралась нулем,
                    // ставим каретку в конец текста и сбрасываем выделение:
                    textBox.Select(textBox.Text.Length, 0);
                }
            }
        }

        // Обработка клавиш (Delete, Enter, Стрелки)
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            var currentCellInfo = CurrentCell;
            if (!currentCellInfo.IsValid) return;

            // Находим TextBox текущей ячейки
            TextBox textBox = null;
            if (currentCellInfo.Column != null && currentCellInfo.Item != null)
            {
                var cellContent = currentCellInfo.Column.GetCellContent(currentCellInfo.Item);
                if (cellContent is TextBox tb)
                    textBox = tb;
                else if (cellContent != null)
                    textBox = FindVisualChild<TextBox>(cellContent);
            }

            bool isCellTextEmpty = textBox != null && string.IsNullOrEmpty(textBox.Text);
            // Проверяем, находимся ли мы в режиме редактирования текста внутри ячейки
            bool isEditingInsideText = _isTextEditMode && !isCellTextEmpty;

            // --- Обработка Delete ---
            if (e.Key == Key.Delete)
            {
                if (isEditingInsideText)
                {
                    // Режим текстовой каретки: позволяем TextBox самому удалить один символ справа от каретки
                    return;
                }

                // Обычный режим выделенной ячейки (Excel): очищаем всю ячейку
                if (textBox != null)
                {
                    textBox.Text = string.Empty;
                }
                else if (currentCellInfo.Item is SelfGrowingDataGridRow row)
                {
                    int colIndex = Columns.IndexOf(currentCellInfo.Column);
                    if (colIndex >= 0 && colIndex < row.Values.Count)
                    {
                        row.Values[colIndex].Value = string.Empty;
                    }
                }
                CommitEdit(DataGridEditingUnit.Cell, true);
                e.Handled = true;
                return;
            }

            // --- Обработка Enter (переход на строку ниже) ---
            if (e.Key == Key.Enter)
            {
                CommitEdit(DataGridEditingUnit.Row, true);

                int currentRow = Items.IndexOf(CurrentItem);
                if (currentRow < Items.Count - 1)
                {
                    CurrentCell = new DataGridCellInfo(Items[currentRow + 1], Columns[currentCellInfo.Column.DisplayIndex]);
                }

                e.Handled = true;
                return;
            }

            // --- Обработка стрелок навигации ---
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
            {
                if (isEditingInsideText)
                {
                    if (textBox != null)
                    {
                        if (e.Key == Key.Up)
                        {
                            textBox.CaretIndex = 0; // В начало текста
                            e.Handled = true;
                        }
                        else if (e.Key == Key.Down)
                        {
                            textBox.CaretIndex = textBox.Text.Length; // В конец текста
                            e.Handled = true;
                        }
                        // Для Key.Left и Key.Right НИЧЕГО не делаем и НЕ ставим e.Handled = true!
                        // TextBox нативно переместит каретку влево/вправо от символа к символу.
                    }
                }
                else
                {
                    // Обычный режим Excel: переносим выделение на соседнюю ячейку
                    CommitEdit(DataGridEditingUnit.Cell, true);

                    int rowIndex = Items.IndexOf(CurrentItem);
                    int colIndex = Columns.IndexOf(currentCellInfo.Column);

                    switch (e.Key)
                    {
                        case Key.Left:
                            if (colIndex > 0) colIndex--;
                            break;
                        case Key.Right:
                            if (colIndex < Columns.Count - 1) colIndex++;
                            break;
                        case Key.Up:
                            if (rowIndex > 0) rowIndex--;
                            break;
                        case Key.Down:
                            if (rowIndex < Items.Count - 1) rowIndex++;
                            break;
                    }

                    CurrentCell = new DataGridCellInfo(Items[rowIndex], Columns[colIndex]);
                    e.Handled = true;
                }
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void AddNewRow(int columnsCount)
        {
            var row = new SelfGrowingDataGridRow(columnsCount);

            for (int i = 0; i < columnsCount; i++)
            {
                var column = Columns[i];
                row.Values[i].IsRequired = GridColumnExtensions.GetIsFillingRequired(column);
            }

            foreach (var cell in row.Values)
                cell.PropertyChanged += Cell_PropertyChanged;

            _rows.Add(row);
        }

        private void Cell_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var changedCell = sender as SelfGrowingDataGridCell;
            var row = _rows.FirstOrDefault(r => r.Values.Contains(changedCell));
            if (row == null) return;

            bool isLastRow = _rows.IndexOf(row) == _rows.Count - 1;

            if (row.IsFilled() && isLastRow)
            {
                AddNewRow(Columns.Count);
            }
            else if (row.IsEmpty() && !isLastRow)
            {
                foreach (var cell in row.Values)
                    cell.PropertyChanged -= Cell_PropertyChanged;

                _rows.Remove(row);
            }
        }
    }
}