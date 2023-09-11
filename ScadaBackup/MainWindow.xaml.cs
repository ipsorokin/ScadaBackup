using ScadaBackup.Models;
using ScadaBackup.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

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

        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var row = (DataGridRow)sender;
            var file = ((BackupFile)row.DataContext).FullName;
            Process.Start("explorer.exe", $"\"{file}\"");
        }
    }
}
