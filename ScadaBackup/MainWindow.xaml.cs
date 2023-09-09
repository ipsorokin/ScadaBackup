using ScadaBackup.ViewModels;
using System.Windows;

namespace ScadaBackup
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F5)
            {
                ((MainViewModel)DataContext).ReloadBackupFiles();
            }
        }
    }
}
