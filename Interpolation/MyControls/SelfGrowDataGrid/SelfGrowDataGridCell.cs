using System.ComponentModel;

namespace Interpolation.MyControls
{
    public class SelfGrowingDataGridCell : INotifyPropertyChanged
    {
        // создаём класс для двусторонней связи что UI знал что ячейки в таблице
        // опустели или запомнились
        private string _value;

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}