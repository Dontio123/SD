using ShelfMaster.Services;
using System.Windows;

namespace ShelfMaster
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; set; } = null!;
        private HotReloadService? _hotReloadService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize Hot Reload service for real-time UI updates
            _hotReloadService = new HotReloadService();

            new MainWindow().Show();  // ← This is the ONLY one now
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Clean up the hot reload service
            _hotReloadService?.Dispose();
            base.OnExit(e);
        }
    }
}