using ScadaBackup.Commands;
using ScadaBackup.Controllers;
using ScadaBackup.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace ScadaBackup.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private IBackupController _controller;
        private RelayCommand _saveCommand;
        private RelayCommand _restoreCommand;
        private RelayCommand _removeBackupCommand;
        private BackupFile _selectedBackupFile;
        private bool _isSelectedBackupFile;
        private ObservableCollection<BackupFile> _backupFiles;

        public MainViewModel() 
        {
            _controller = new BackupController();
            LoadBackupFiles();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsCopyBaseSettings { get; } = true;

        /// <summary>
        /// 
        /// </summary>
        public bool IsCopyEventsDatabase { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsCopyVoicesDatabase { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool IsCopyScripts { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSelectedBackupFile { get => _isSelectedBackupFile; private set { _isSelectedBackupFile = value; OnPropertyChanged(); } }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<BackupFile> BackupFiles { get => _backupFiles; private set { _backupFiles = value; OnPropertyChanged(); } }

        /// <summary>
        /// 
        /// </summary>
        public BackupFile SelectedBackupFile { get => _selectedBackupFile; set { _selectedBackupFile = value; IsSelectedBackupFile = value != null; OnPropertyChanged(); }  }

        /// <summary>
        /// 
        /// </summary>
        public RelayCommand SaveCommand => _saveCommand ?? (_saveCommand = new RelayCommand(obj =>
        {
            try
            {
                BackupFile newFile = _controller.CreateBackup(IsCopyEventsDatabase, IsCopyVoicesDatabase, IsCopyScripts);
                BackupFiles.Add(newFile);
                MessageBox.Show("Резервная копия успешно создана.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }));

        /// <summary>
        /// 
        /// </summary>
        public RelayCommand RestoreCommand => _restoreCommand ?? (_restoreCommand = new RelayCommand(obj =>
        {
            try
            {
                _controller.RestoreBackup(SelectedBackupFile, IsCopyEventsDatabase, IsCopyVoicesDatabase, IsCopyScripts);
                MessageBox.Show("Восстановление из резервной копии завершено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }));

        /// <summary>
        /// 
        /// </summary>
        public RelayCommand RemoveBackupCommand => _removeBackupCommand ?? (_removeBackupCommand = new RelayCommand(obj =>
        {
            if (SelectedBackupFile == null)
            {
                MessageBox.Show("Файл не выбран.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"ВЫ ДЕЙСТВИТЕЛЬНО ХОТИТЕ УДАЛИТЬ РЕЗЕРВНУЮ КОПИЮ?\n{SelectedBackupFile.Name}", "ВНИМАНИЕ",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

            if (result == MessageBoxResult.No)
                return;

            try
            {
                _controller.RemoveBackup(SelectedBackupFile);
                string backupFileName = SelectedBackupFile.Name;
                BackupFiles.Remove(SelectedBackupFile);
                MessageBox.Show($"Резервная копия {backupFileName} удалена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                BackupFiles.Remove(SelectedBackupFile);
                return;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }));

        public void ReloadBackupFiles()
        {
            BackupFiles.Clear();
            LoadBackupFiles();
        }

        private void LoadBackupFiles()
        {
            BackupFiles = new ObservableCollection<BackupFile>(_controller.GetBackupFiles());
        }
    }
}
