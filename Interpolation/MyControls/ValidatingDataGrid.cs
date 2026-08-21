using Interpolation.MyControls.SelfGrowDataGrid;
using Interpolation.Validators;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Interpolation.MyControls
{
    public class ValidatingDataGrid : DataGrid
    {
        // Срабатывает каждый раз, когда ячейка (любая, в том числе новая) переходит в режим редактирования
        protected override void OnPreparingCellForEdit(DataGridPreparingCellForEditEventArgs e)
        {
            base.OnPreparingCellForEdit(e);

            if (e.EditingElement is TextBox textBox)
            {
                // На случай повторного входа в редактирование той же ячейки —
                // сначала отписываемся, чтобы не подписаться дважды
                textBox.PreviewTextInput -= TextBox_PreviewTextInput;
                textBox.PreviewTextInput += TextBox_PreviewTextInput;

                // Подписываемся на вставку из буфера обмена (Ctrl+V и правый клик "Вставить")
                DataObject.RemovePastingHandler(textBox, TextBox_Pasting);
                DataObject.AddPastingHandler(textBox, TextBox_Pasting);
            }
        }

        // Проверяем каждый вводимый символ ДО того, как он попадёт в текст
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;

            // Сначала убираем выделенный текст (если есть), потом вставляем новый ввод
            string textWithoutSelection = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength);
            string proposedText = textWithoutSelection.Insert(textBox.SelectionStart, e.Text);

            if (!NumericValidator.IsValid(proposedText))
            {
                e.Handled = true;
            }
        }

        // Срабатывает при вставке текста из буфера обмена
        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            var textBox = sender as TextBox;

            // Достаём из буфера именно текст (вставка может нести и картинку, и файл, и т.д.)
            if (!e.DataObject.GetDataPresent(DataFormats.Text))
            {
                e.CancelCommand(); // отменяем вставку — там не текст
                return;
            }

            string pastedText = e.DataObject.GetData(DataFormats.Text) as string;

            // Собираем, каким будет текст ПОСЛЕ вставки
            string proposedText = textBox.Text.Insert(textBox.CaretIndex, pastedText);

            if (!NumericValidator.IsValid(proposedText))
            {
                e.CancelCommand(); // отменяем вставку — результат не пройдёт валидацию
            }
        }
    }
}