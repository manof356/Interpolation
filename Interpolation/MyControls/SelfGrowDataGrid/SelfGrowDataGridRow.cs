using Interpolation.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Interpolation.MyControls.SelfGrowDataGrid
{
    // модель данных для одной строки в DataGrid
    // это как маленький список (типа List), который хранит
    // в себе объекты ячеек SelfGrowingDataGridCell одной строки
    public class SelfGrowingDataGridRow : INotifyPropertyChanged
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

        // Статус строки для иконки в row header (ошибка/предупреждение/инфо)
        private RowStatus _status = RowStatus.None;
        public RowStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        // Текст тултипа для иконки статуса
        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}