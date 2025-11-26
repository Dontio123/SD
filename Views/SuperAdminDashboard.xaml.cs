using ShelfMaster.Models;
using ShelfMaster.Services;
using ShelfMaster.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ShelfMaster.Views
{
    public partial class SuperAdminDashboard : Window
    {
        public SuperAdminDashboard(Clerk admin, LibraryService service)
        {
            InitializeComponent();
            var vm = new SuperAdminDashboardViewModel(admin, service);
            vm.PropertyChanged += ViewModelOnPropertyChanged;
            DataContext = vm;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private bool _isFullscreen = false;
        private System.Windows.Rect _restoreBounds;

        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isFullscreen)
            {
                // Restore window
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.CanResize;
                if (_restoreBounds != System.Windows.Rect.Empty)
                {
                    Left = _restoreBounds.Left;
                    Top = _restoreBounds.Top;
                    Width = _restoreBounds.Width;
                    Height = _restoreBounds.Height;
                }
                _isFullscreen = false;
            }
            else
            {
                // Save current bounds
                _restoreBounds = new System.Windows.Rect(Left, Top, Width, Height);

                // Make fullscreen
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Maximized;
                _isFullscreen = true;
            }
        }

        private void RootBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SuperAdminDashboardViewModel vm && sender is PasswordBox box)
            {
                vm.StaffPassword = box.Password;
            }
        }

        private void StudentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SuperAdminDashboardViewModel vm && sender is PasswordBox box)
            {
                vm.StudentPassword = box.Password;
            }
        }

        private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (DataContext is SuperAdminDashboardViewModel vm)
            {
                if (e.PropertyName == nameof(SuperAdminDashboardViewModel.StaffPassword)
                    && string.IsNullOrEmpty(vm.StaffPassword))
                {
                    StaffPasswordBox.Password = string.Empty;
                }
                else if (e.PropertyName == nameof(SuperAdminDashboardViewModel.StudentPassword)
                    && string.IsNullOrEmpty(vm.StudentPassword))
                {
                    StudentPasswordBox.Password = string.Empty;
                }
            }
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.Show();
                if (mainWindow.WindowState == WindowState.Minimized)
                {
                    mainWindow.WindowState = WindowState.Normal;
                }
                mainWindow.Activate();
            }
            else
            {
                var newMainWindow = new MainWindow();
                Application.Current.MainWindow = newMainWindow;
                newMainWindow.Show();
            }

            Close();
        }
    }
}

