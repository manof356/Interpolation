using System.ComponentModel;

public class ResultRow : INotifyPropertyChanged
{
    private string _argument;
    private string _result;

    public string Argument
    {
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