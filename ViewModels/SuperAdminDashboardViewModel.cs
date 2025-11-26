using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShelfMaster.Models;
using ShelfMaster.Models.Enums;
using ShelfMaster.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ShelfMaster.ViewModels
{
    public partial class SuperAdminDashboardViewModel : ObservableObject
    {
        private readonly LibraryService _libraryService;
        private readonly Clerk _currentAdmin;

        [ObservableProperty] private DashboardStats? stats;
        [ObservableProperty] private ObservableCollection<Category> categories = new();
        [ObservableProperty] private ObservableCollection<Clerk> staffMembers = new();

        [ObservableProperty] private string newCategoryName = string.Empty;
        [ObservableProperty] private string? newCategoryDescription;

        [ObservableProperty] private string staffFirstName = string.Empty;
        [ObservableProperty] private string staffLastName = string.Empty;
        [ObservableProperty] private string? staffMiddleName;
        [ObservableProperty] private string staffEmail = string.Empty;
        [ObservableProperty] private string staffContactNumber = string.Empty;
        [ObservableProperty] private string staffUsername = string.Empty;
        [ObservableProperty] private string staffPassword = string.Empty;
        [ObservableProperty] private UserRole selectedStaffRole = UserRole.Clerk;

        // Student registration properties
        [ObservableProperty] private string studentFirstName = string.Empty;
        [ObservableProperty] private string? studentMiddleName;
        [ObservableProperty] private string studentLastName = string.Empty;
        [ObservableProperty] private string studentEmail = string.Empty;
        [ObservableProperty] private string studentContactNumber = string.Empty;
        [ObservableProperty] private string studentAddress = string.Empty;
        [ObservableProperty] private string studentCollege = string.Empty;
        [ObservableProperty] private string studentDepartment = string.Empty;
        [ObservableProperty] private string studentUsername = string.Empty;
        [ObservableProperty] private string studentPassword = string.Empty;
        [ObservableProperty] private ObservableCollection<User> students = new();

        // Navigation
        [ObservableProperty] private string selectedView = "Dashboard";

        public IEnumerable<UserRole> StaffRoles => new[] { UserRole.Clerk, UserRole.Admin };
        public string AdminName => _currentAdmin.FullName;

        public SuperAdminDashboardViewModel(Clerk admin, LibraryService libraryService)
        {
            _currentAdmin = admin;
            _libraryService = libraryService;
            _ = LoadDashboardAsync();
        }

        [RelayCommand]
        private async Task LoadDashboardAsync()
        {
            Stats = await _libraryService.GetDashboardStatsAsync();
            Categories = new ObservableCollection<Category>(await _libraryService.GetCategoriesAsync());
            StaffMembers = new ObservableCollection<Clerk>(await _libraryService.GetClerksAsync());
            Students = new ObservableCollection<User>(await _libraryService.GetStudentsAsync());
        }

        [RelayCommand]
        private async Task CreateCategoryAsync()
        {
            try
            {
                await _libraryService.AddCategoryAsync(NewCategoryName, NewCategoryDescription);
                NewCategoryName = string.Empty;
                NewCategoryDescription = string.Empty;
                await LoadDashboardAsync();
                MessageBox.Show("Category added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to add category", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        [RelayCommand]
        private async Task RegisterStaffAsync()
        {
            try
            {
                await _libraryService.CreateStaffAccountAsync(
                    StaffFirstName,
                    StaffMiddleName,
                    StaffLastName,
                    StaffEmail,
                    StaffContactNumber,
                    StaffUsername,
                    StaffPassword,
                    SelectedStaffRole);

                StaffFirstName = StaffLastName = StaffMiddleName = StaffEmail = StaffContactNumber = StaffUsername = StaffPassword = string.Empty;
                SelectedStaffRole = UserRole.Clerk;

                await LoadDashboardAsync();
                MessageBox.Show("Staff account created!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to create staff", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        [RelayCommand]
        private async Task RegisterStudentAsync()
        {
            try
            {
                await _libraryService.CreateStudentAccountAsync(
                    StudentFirstName,
                    StudentMiddleName,
                    StudentLastName,
                    StudentEmail,
                    StudentContactNumber,
                    StudentAddress,
                    StudentCollege,
                    StudentDepartment,
                    StudentUsername,
                    StudentPassword);

                // Clear form
                StudentFirstName = StudentLastName = StudentMiddleName = StudentEmail =
                    StudentContactNumber = StudentAddress = StudentCollege = StudentDepartment =
                    StudentUsername = StudentPassword = string.Empty;

                await LoadDashboardAsync();
                MessageBox.Show("Student account created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to create student account", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        [RelayCommand]
        private void NavigateToView(string viewName)
        {
            SelectedView = viewName;
        }
    }
}

