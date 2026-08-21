using System.ComponentModel;

namespace Interpolation.MyControls.SelfGrowDataGrid
{
    // создаём класс с подключением интерфейса INotifyPropertyChanged,
    // чтобы UI знал что ячейки в таблице изменились
    public class SelfGrowingDataGridCell : INotifyPropertyChanged
    {
        // создаём класс для двусторонней связи чтобы UI знал что ячейки в таблице
        // опустели или заполнились
        private string _value; // приватное значение ячейки
        // свойство того что данная ячейка обязательна для заполнения, по умолчанию true
        public bool IsRequired { get; set; } = true;
        // Точное число, без округления. Null — если ячейка не результат (например, аргумент)
        public double? RawValue { get; set; }
        public string Value
        {
            // обычный геттер и сеттер для получения и записи значения ячейки, с уведомлением об изменении свойства
            get => _value;
            set
            {
                _value = value;
                // вызываем метод OnPropertyChanged с именем свойства, чтобы уведомить UI об изменении значения
                OnPropertyChanged(nameof(Value));
            }
        }
        // событие для уведомления об изменении свойства
        public event PropertyChangedEventHandler PropertyChanged;
        // метод для вызова события PropertyChanged
        protected void OnPropertyChanged(string propertyName)
        {
            // вызываем событие PropertyChanged, если оно не равно null
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}