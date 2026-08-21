using System.Text.RegularExpressions;

namespace Interpolation.Validators
{
    // Валидатор чисел. Не позволяет вводить в TextBox ничего, кроме чисел, знаков + и -, а также десятичного разделителя (точки или запятой).
    public static class NumericValidator
    {
        private static readonly Regex NumericRegex = new Regex(@"^[+-]?\d*[.,]?\d*$");

        public static bool IsValid(string text)
        {
            return NumericRegex.IsMatch(text);
        }
    }
}