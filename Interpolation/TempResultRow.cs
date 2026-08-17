using System.ComponentModel;

/// <summary>
/// Типовой класс для создания связи между UI и кодом. чтобы была двусторонняя связь. 
/// получение и отправка данных туда сюда. класс строки таблицы.
/// </summary>
public class ResultRow : INotifyPropertyChanged
{
    private string _argument; // в таблице всего 2 столбца поэтому на каждую ячейку даём переменную.
    private string _result;

    public string Argument
    {
        // обычный геттер и сеттер
        get => _argument;
        set { _argument = value; OnPropertyChanged(nameof(Argument)); }
    }

    public string Result
    {
        get => _result;
        set { _result = value; OnPropertyChanged(nameof(Result)); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}