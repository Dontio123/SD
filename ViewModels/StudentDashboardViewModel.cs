// ViewModels/StudentDashboardViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShelfMaster.Models;
using ShelfMaster.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ShelfMaster.ViewModels
{
    public partial class StudentDashboardViewModel : ObservableObject
    {
        private readonly LibraryService _libraryService;
        private readonly User _currentUser;

        public string StudentName => _currentUser.FullName;
        public string StudentEmail => _currentUser.Email;

        public ObservableCollection<string> FilterChips { get; } = new()
        {
            "All categories",
            "Programming",
            "Business",
            "Science",
            "History",
            "New arrivals"
        };

        [ObservableProperty] private string searchText = "";
        [ObservableProperty] private ObservableCollection<BookCopy> availableBooks = new();
        [ObservableProperty] private ObservableCollection<BookCopy> filteredBooks = new();
        [ObservableProperty] private ObservableCollection<Transaction> borrowedBooks = new();
        [ObservableProperty] private string selectedSection = "Dashboard";

        public StudentDashboardViewModel(User user, LibraryService libraryService)
        {
            _currentUser = user;
            _libraryService = libraryService;
            LoadBooksCommand.Execute(null);
            LoadBorrowedBooksCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadBooks()
        {
            var books = await _libraryService.GetAvailableBooksAsync();
            AvailableBooks = new ObservableCollection<BookCopy>(books);
            FilteredBooks = new ObservableCollection<BookCopy>(books);
        }

        [RelayCommand]
        private void Navigate(string? destination)
        {
            if (string.IsNullOrWhiteSpace(destination))
            {
                return;
            }

            SelectedSection = destination;
        }

        [RelayCommand]
        private async Task LoadBorrowedBooks()
        {
            var loans = await _libraryService.GetUserActiveLoansAsync(_currentUser.UserId);
            BorrowedBooks = new ObservableCollection<Transaction>(loans);
        }

        // Real-time search
        partial void OnSearchTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                FilteredBooks = new ObservableCollection<BookCopy>(AvailableBooks);
            }
            else
            {
                var lower = value.ToLower();
                var filtered = AvailableBooks.Where(b =>
                    b.Book.Title.ToLower().Contains(lower) ||
                    b.Book.Author.ToLower().Contains(lower) ||
                    b.Book.ISBN.Contains(value) ||
                    b.Book.Category.CategoryName.ToLower().Contains(lower)
                );

                FilteredBooks = new ObservableCollection<BookCopy>(filtered);
            }
        }

        [RelayCommand]
        private async Task RequestBook(Guid bookCopyId)
        {
            try
            {
                await _libraryService.RequestBookAsync(_currentUser.UserId, bookCopyId);
                MessageBox.Show("Book requested successfully! Waiting for approval.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadBooks();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ReturnBorrowedBook(Guid transactionId)
        {
            try
            {
                await _libraryService.ReturnBookAsync(transactionId, _currentUser.UserId);
                MessageBox.Show("Book returned successfully. Thank you!", "Return complete",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadBorrowedBooks();
                await LoadBooks();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Return error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}