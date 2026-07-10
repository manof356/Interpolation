using System.Collections.ObjectModel;
using System.Linq;

namespace Interpolation.MyControls
{
    public class SelfGrowingDataGridRow
    {
        // Коллекция ячеек одной строки
        public ObservableCollection<SelfGrowingDataGridCell> Values { get; set; } = new ObservableCollection<SelfGrowingDataGridCell>();
        // Создаёт строку с нужным количеством пустых ячеек
        public SelfGrowingDataGridRow(int columnsCount)
        {
            for (int i = 0; i < columnsCount; i++)
            {
                Values.Add(new SelfGrowingDataGridCell());
            }
        }
        // Проверка: все ячейки заполнены
        public bool IsFilled()
        {
            return Values.All(cell => !string.IsNullOrWhiteSpace(cell.Value));
        }
        // Проверка: все ячейки пустые
        public bool IsEmpty()
        {
            return Values.All(cell => string.IsNullOrWhiteSpace(cell.Value));
        }
    }
}