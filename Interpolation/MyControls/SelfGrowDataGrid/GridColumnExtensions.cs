using System.Windows;
// дополнительный класс который добавляет к GridColumn новое attached property,
// которое будет указывать, нужно ли заполнять колонку
namespace Interpolation.MyControls.SelfGrowDataGrid
{
    public static class GridColumnExtensions
    {
        public static readonly DependencyProperty IsFillingRequiredProperty =
            DependencyProperty.RegisterAttached(
                "IsFillingRequired",   // имя свойства (строкой)
                typeof(bool),          // тип свойства
                typeof(GridColumnExtensions), // класс-владелец
                new PropertyMetadata(true)); // значение по умолчанию

        // геттер — вызывается когда где-то читают local:GridColumnExtensions.IsFillingRequired
        public static bool GetIsFillingRequired(DependencyObject obj)
            => (bool)obj.GetValue(IsFillingRequiredProperty);

        // сеттер — вызывается когда в XAML это свойство присваивают
        public static void SetIsFillingRequired(DependencyObject obj, bool value)
            => obj.SetValue(IsFillingRequiredProperty, value);
    }
}