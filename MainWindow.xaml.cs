using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShelfMaster.Database;
using ShelfMaster.Services;
using ShelfMaster.ViewModels;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ShelfMaster
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeServices(); // Builds the ServiceProvider FIRST
            DataContext = App.ServiceProvider.GetRequiredService<LoginViewModel>();
        }

        private void InitializeServices()
        {
            var services = new ServiceCollection();

            // READ CONNECTION STRING FROM appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string connectionString = configuration.GetConnectionString("LibraryDB")!;
            string? serverVersionValue = configuration.GetSection("Database")?["ServerVersion"];

            var serverVersion = string.IsNullOrWhiteSpace(serverVersionValue)
                ? new MySqlServerVersion(new Version(8, 0, 36))
                : new MySqlServerVersion(Version.Parse(serverVersionValue));

            services.AddDbContext<LibraryDbContext>(options =>
                options.UseMySql(connectionString, serverVersion));

            // Register a DbContextFactory so services that depend on
            // IDbContextFactory<LibraryDbContext> can be resolved.
            services.AddDbContextFactory<LibraryDbContext>(options =>
                 options.UseMySql(connectionString, serverVersion));

            services.AddScoped<AuthenticationService>();
            services.AddScoped<LibraryService>();
            services.AddTransient<LoginViewModel>();

            App.ServiceProvider = services.BuildServiceProvider();

            using var scope = App.ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            context.Database.EnsureCreated();
            DatabaseSeeder.Seed(context);
        }

        // ——— UI EVENT HANDLERS ———

        private void TxtEmail_MouseDown(object sender, MouseButtonEventArgs e) => txtEmail.Focus();

        private void TxtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            textEmail.Visibility = string.IsNullOrEmpty(txtEmail.Text) ? Visibility.Visible : Visibility.Collapsed;
            if (DataContext is LoginViewModel vm)
            {
                vm.Username = txtEmail.Text;
            }
        }

        private void TextPassword_MouseDown(object sender, MouseButtonEventArgs e) => txtPassword.Focus();

        private void TxtPassword_TextChanged(object sender, RoutedEventArgs e)
            => textPassword.Visibility = string.IsNullOrEmpty(txtPassword.Password) ? Visibility.Visible : Visibility.Collapsed;

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                // Update username from textbox if needed
                if (string.IsNullOrWhiteSpace(vm.Username))
                {
                    vm.Username = txtEmail.Text?.Trim() ?? "";
                }
                vm.LoginCommand.Execute(txtPassword.Password);
            }
        }

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

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtPassword.Focus();
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(sender, e);
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}