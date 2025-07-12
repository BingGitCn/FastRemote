using System.Windows;
using System.Windows.Input;

namespace FastRemote.Views
{
    /// <summary>
    /// Interaction logic for CreateConnectionDialog.xaml
    /// </summary>
    public partial class CreateConnectionDialog : Window
    {
        public CreateConnectionDialog()
        {
            InitializeComponent();
            this.Loaded += CreateConnectionDialog_Loaded;
        }

        private void CreateConnectionDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.CreateConnectionViewModel viewModel)
            {
                viewModel.PasswordBox = PasswordBox;
                viewModel.RequestClose = (dialogResult) =>
                {
                    DialogResult = dialogResult;
                };
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}