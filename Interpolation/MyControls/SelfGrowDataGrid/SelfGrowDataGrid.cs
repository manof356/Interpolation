using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Interpolation.MyControls.SelfGrowDataGrid
{
    // Дочерний класс на основе DataGrid в котором прописана логика увеличения кол-ва строк
    public class SelfGrowingDataGrid : ValidatingDataGrid
    {
        // создаём переменную _rows, которая будет хранить строки в нашей таблице
        private ObservableCollection<SelfGrowingDataGridRow> _rows;
        private bool _isTextEditMode = false;
        private bool _mouseInitiatedFocus = false;
        // Событие: сообщает наружу (MainWindow), что значение в конкретной ячейке изменилось.
        // row — строка, columnIndex — индекс столбца в этой строке.
        public event Action<SelfGrowingDataGridRow, int>? CellValueChanged;
        // метод-конструктор, который будет вызываться при создании экземпляра SelfGrowingDataGrid
        public SelfGrowingDataGrid()
        {
            // Устанавливаем свойства DataGrid, чтобы отключить
            AutoGenerateColumns = false; // автоматическое создание столбцов и установить выбор ячеек
            SelectionUnit = DataGridSelectionUnit.Cell; // выбор по ячейке
            SelectionMode = DataGridSelectionMode.Single; // выбор одной ячейки
            CanUserAddRows = false; // запрет на добавление строк пользователем
            CanUserSortColumns = false; // отключаем сортировку по клику на заголовок — она ломает порядок строк
            CanUserResizeRows = false; // запрет на изменение высоты строк вручную
        }
        // Переопределяем (override) метод OnInitialized, который вызывается при инициализации компонента
        // это тоже своего рода метод инициализатор, но он вызывается после конструктора и после того,
        // как все свойства компонента были установлены
        protected override void OnInitialized(EventArgs e)
        {
            // base - аналог super(). из python. то есть вызов оригинального метода из класса DataGrid
            base.OnInitialized(e);
            // получаем кол-во столбцов таблицы
            int columnsCount = Columns.Count;
            // создаем коллекцию строк
            _rows = new ObservableCollection<SelfGrowingDataGridRow>();
            // добавляем в коллекцию строку с кол-вом ячеек равным кол-ву столбцов
            AddNewRow(columnsCount);
            // присвоение значениям таблицы коллекции строк
            ItemsSource = _rows;
        }
        //todo. дописать комменты, разобраться что тут происходит. изучить
        // При фокусе на ячейку — сразу включаем редактирование
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            if (e.OriginalSource is DataGridCell cell && !cell.IsEditing && !cell.IsReadOnly)
            {
                // Если фокус пришёл не от клика мыши (Tab, переход стрелками) —
                // всегда возвращаемся в режим выделения ячейки
                if (!_mouseInitiatedFocus)
                    _isTextEditMode = false;
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

        // метод добавления новой строки в таблицу
        private void AddNewRow(int columnsCount)
        {
            // создаём пустую строку
            var row = new SelfGrowingDataGridRow(columnsCount);
            // проставляем каждой ячейке признак "обязательна ли она к заполнению"
            // берём значение с соответствующего столбца по индексу
            for (int i = 0; i < columnsCount; i++)
            {
                var column = Columns[i];
                row.Values[i].IsRequired = GridColumnExtensions.GetIsFillingRequired(column);
            }
            // Подписываемся на изменение каждой ячейки в этой строке
            foreach (var cell in row.Values)
                cell.PropertyChanged += Cell_PropertyChanged;
            // добавляем в коллекцию строк новую строку
            _rows.Add(row);
        }

        private void Cell_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // приводим var к написанному нами классу
            var changedCell = sender as SelfGrowingDataGridCell;
            // проверяем содержит ли строка таблицы измененную ячейку
            var row = _rows.FirstOrDefault(r => r.Values.Contains(changedCell));
            if (row == null) return; // защита. если такой строки нет, то ничего не вернуть

            // сообщаем наружу только когда изменилось именно отображаемое значение
            if (e.PropertyName == nameof(SelfGrowingDataGridCell.Value))
            {
                int columnIndex = row.Values.IndexOf(changedCell);
                CellValueChanged?.Invoke(row, columnIndex);
            }

            // рост/удаление строк реагирует только на обязательные столбцы —
            // запись вычисленного результата не должна это трогать
            if (!changedCell.IsRequired) return;

            // проверка что найденная строка последняя
            bool isLastRow = _rows.IndexOf(row) == _rows.Count - 1;
            // если строка и последняя и заполненная
            if (row.IsFilled() && isLastRow)
                // добавляем новую строку
                AddNewRow(Columns.Count);
            // если строка пустая и не последнедняя
            else if (row.IsEmpty() && !isLastRow)
            {
                foreach (var cell in row.Values)
                    // отписываемся для каждой ячейки от события и
                    cell.PropertyChanged -= Cell_PropertyChanged;
                // удаляем всю строку
                _rows.Remove(row);
            }
        }
    }
}