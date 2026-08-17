using System.Collections.ObjectModel;
using System.Linq;

namespace Interpolation.MyControls.SelfGrowDataGrid
{
    // модель данных для одной строки в DataGrid
    // это как маленький список (типа List), который хранит
    // в себе объекты ячеек SelfGrowingDataGridCell одной строки
    public class SelfGrowingDataGridRow
    {
        // Коллекция ячеек одной строки
        public ObservableCollection<SelfGrowingDataGridCell> Values { get; set; } = new ObservableCollection<SelfGrowingDataGridCell>();
        // Конструктор (как __init__ в python) класса. Создает элемент-строку
        // с заданным количеством столбцов
        public SelfGrowingDataGridRow(int columnsCount)
        {
            for (int i = 0; i < columnsCount; i++)
            {
                // Создаем объект-ячейку и добавляем ее в коллекцию
                Values.Add(new SelfGrowingDataGridCell());
            }
        }
        // Проверка: все ячейки заполнены
        public bool IsFilled()
        {
            return Values.Where(cell => cell.IsRequired)
                 .All(cell => !string.IsNullOrWhiteSpace(cell.Value));
        }
        // Проверка: все ячейки пустые
        public bool IsEmpty()
        {
            return Values.Where(cell => cell.IsRequired)
                 .All(cell => string.IsNullOrWhiteSpace(cell.Value));
        }
    }
}