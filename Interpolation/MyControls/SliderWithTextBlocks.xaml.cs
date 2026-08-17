using System.Windows;
using System.Windows.Controls;

namespace Interpolation.MyControls
{
    /// <summary>
    /// Логика взаимодействия для SliderWithTextBlocks.xaml
    /// </summary>
    public partial class SliderWithTextBlocks : UserControl
    {
        public SliderWithTextBlocks()
        {
            InitializeComponent();
        }
        
        // Текст слева (например "Точность:")
        public string LeftText
        {
            get => (string)GetValue(LeftTextProperty);
            set => SetValue(LeftTextProperty, value);
        }
        // Связываем свой параметр пользовательского контрола с родным параметром текстблока чтобы можно
        // было задавать значение в XAML
        public static readonly DependencyProperty LeftTextProperty =
                               DependencyProperty.Register(nameof(LeftText), 
                               typeof(string), typeof(SliderWithTextBlocks),
                               new PropertyMetadata(""));
        
        // Начальное значение слайдера
        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }
        public static readonly DependencyProperty MinimumProperty =
                               DependencyProperty.Register(nameof(Minimum), 
                               typeof(double), typeof(SliderWithTextBlocks),
                               new PropertyMetadata(0.0));

        // Конечное значение слайдера
        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }
        public static readonly DependencyProperty MaximumProperty =
                               DependencyProperty.Register(nameof(Maximum), 
                               typeof(double), typeof(SliderWithTextBlocks),
                               new PropertyMetadata(100.0));

        // Шаг слайдера
        public double Step
        {
            get => (double)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }
        public static readonly DependencyProperty StepProperty =
                               DependencyProperty.Register(nameof(Step), 
                               typeof(double), typeof(SliderWithTextBlocks),
                               new PropertyMetadata(1.0));
    }
}
