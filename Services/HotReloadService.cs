using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace ShelfMaster.Services
{
    public class HotReloadService : IDisposable
    {
        private FileSystemWatcher? _xamlWatcher;
        private FileSystemWatcher? _styleWatcher;
        private FileSystemWatcher? _rootWatcher;
        private readonly string _projectPath;
        private readonly DispatcherTimer _reloadTimer;
        private bool _isDisposed = false;

        public HotReloadService()
        {
            // Get the project directory (parent of bin folder)
            var currentDir = Directory.GetCurrentDirectory();
            _projectPath = currentDir.Contains("\\bin\\")
                ? Directory.GetParent(currentDir)?.Parent?.FullName ?? currentDir
                : currentDir;

            // Use a timer to debounce rapid file changes
            _reloadTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _reloadTimer.Tick += OnReloadTimerTick;

            InitializeWatchers();
        }

        private void InitializeWatchers()
        {
            try
            {
                // Watch for XAML files in Views directory
                var viewsPath = Path.Combine(_projectPath, "Views");
                if (Directory.Exists(viewsPath))
                {
                    _xamlWatcher = new FileSystemWatcher(viewsPath, "*.xaml")
                    {
                        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                        EnableRaisingEvents = true
                    };
                    _xamlWatcher.Changed += OnXamlFileChanged;
                    _xamlWatcher.Created += OnXamlFileChanged;
                }

                // Watch for XAML files in Styles directory
                var stylesPath = Path.Combine(_projectPath, "Styles");
                if (Directory.Exists(stylesPath))
                {
                    _styleWatcher = new FileSystemWatcher(stylesPath, "*.xaml")
                    {
                        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                        EnableRaisingEvents = true
                    };
                    _styleWatcher.Changed += OnXamlFileChanged;
                    _styleWatcher.Created += OnXamlFileChanged;
                }

                // Also watch the root directory for MainWindow.xaml and App.xaml
                _rootWatcher = new FileSystemWatcher(_projectPath, "*.xaml")
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                    EnableRaisingEvents = true
                };
                _rootWatcher.Changed += OnXamlFileChanged;
                _rootWatcher.Created += OnXamlFileChanged;
            }
            catch (Exception ex)
            {
                // Log error but don't crash the app
                System.Diagnostics.Debug.WriteLine($"Error initializing Hot Reload: {ex.Message}");
            }
        }

        private void OnXamlFileChanged(object sender, FileSystemEventArgs e)
        {
            // Debounce: restart the timer on each change
            _reloadTimer.Stop();
            _reloadTimer.Start();
        }

        private void OnReloadTimerTick(object? sender, EventArgs e)
        {
            _reloadTimer.Stop();

            // Reload resources on the UI thread
            Application.Current?.Dispatcher.Invoke(() =>
            {
                try
                {
                    // Reload all resource dictionaries
                    ReloadResources();

                    // Force a refresh of the current window
                    if (Application.Current.MainWindow != null)
                    {
                        var currentWindow = Application.Current.MainWindow;
                        currentWindow.InvalidateVisual();
                        currentWindow.UpdateLayout();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reloading UI: {ex.Message}");
                }
            });
        }

        private void ReloadResources()
        {
            try
            {
                // Reload the main resource dictionary (ColorStyles.xaml) using pack URI
                var existingDict = Application.Current.Resources.MergedDictionaries
                    .FirstOrDefault(rd => rd.Source?.OriginalString?.Contains("ColorStyles") == true);

                if (existingDict != null)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(existingDict);
                }

                // Reload using pack URI (works for resources in the project)
                var resourceDict = new ResourceDictionary
                {
                    Source = new Uri("/Styles/ColorStyles.xaml", UriKind.Relative)
                };

                Application.Current.Resources.MergedDictionaries.Add(resourceDict);

                // Refresh all open windows
                foreach (Window window in Application.Current.Windows)
                {
                    window.InvalidateVisual();
                    window.UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reloading resources: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _reloadTimer?.Stop();
            _xamlWatcher?.Dispose();
            _styleWatcher?.Dispose();
            _rootWatcher?.Dispose();
            _isDisposed = true;
        }
    }
}

