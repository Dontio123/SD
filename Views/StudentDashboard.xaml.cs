using ShelfMaster.Models;
using ShelfMaster.Services;
using ShelfMaster.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ShelfMaster.Views
{
    public partial class StudentDashboard : Window
    {
        private bool isInventoryFullScreen = false;
        private Thickness originalMargin;
        private int originalRow;
        private int originalRowSpan;

        public StudentDashboard(User currentUser, LibraryService service)
        {
            InitializeComponent();
            DataContext = new StudentDashboardViewModel(currentUser, service);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

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

        private bool _isInventoryFullscreen = false;

        private void RootBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
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
