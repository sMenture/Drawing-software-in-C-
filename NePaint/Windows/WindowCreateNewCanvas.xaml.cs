using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace NePaint.Windows
{
    public partial class WindowCreateNewCanvas : Window
    {
        public string ProjectTitle { get; private set; }
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        private const int minLimit = 100;
        private const int maxLimit = 2000;

        public WindowCreateNewCanvas()
        {
            InitializeComponent();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            int width, height;
            if (string.IsNullOrWhiteSpace(ProjectTitleTextBox.Text))
            {
                MessageBox.Show("Название проекта не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (int.TryParse(ScreenWidthTextBox.Text, out width) && int.TryParse(ScreenHeightTextBox.Text, out height))
            {
                if (width >= minLimit && width <= maxLimit && height >= minLimit && height <= maxLimit)
                {
                    ProjectTitle = ProjectTitleTextBox.Text;
                    ScreenWidth = width;
                    ScreenHeight = height;
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show($"Размеры холста должны быть в диапазоне от {minLimit} до {maxLimit}.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите корректные размеры холста.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region LimitInputScreenText
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        #endregion
    }
}
