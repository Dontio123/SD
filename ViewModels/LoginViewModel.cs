using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ShelfMaster.Models.Enums;
using ShelfMaster.Services;
using ShelfMaster.Views;
using System.Windows;

namespace ShelfMaster.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthenticationService _authService;
        private readonly LibraryService _libraryService;

        [ObservableProperty] private string username = "";
        [ObservableProperty] private string errorMessage = "";
        [ObservableProperty] private bool isLoading = false;

        public LoginViewModel()
        {
            _authService = App.ServiceProvider.GetRequiredService<AuthenticationService>();
            _libraryService = App.ServiceProvider.GetRequiredService<LibraryService>();
        }

        [RelayCommand]
        private void Login(object passwordObj)
        {
            ErrorMessage = ""; // Clear previous errors

            if (string.IsNullOrWhiteSpace(Username) || passwordObj is not string password || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Please enter both email and password";
                return;
            }

            try
            {
                IsLoading = true;
                var result = _authService.Login(Username, password);

                if (result == null)
                {
                    ErrorMessage = "Invalid email or password!";
                    IsLoading = false;
                    return;
                }

                ErrorMessage = ""; // Clear error on success
                Application.Current.MainWindow?.Hide();

                if (result.Student != null)
                {
                    new StudentDashboard(result.Student, _libraryService).Show();
                }
                else if (result.Staff != null)
                {
                    if (result.Role == UserRole.Admin)
                        new SuperAdminDashboard(result.Staff, _libraryService).Show();
                    else
                        new LibrarianDashboard(result.Staff, _libraryService).Show();
                }

                IsLoading = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                IsLoading = false;
            }
        }
    }
}