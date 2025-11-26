using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShelfMaster.Models;
using ShelfMaster.Models.Enums;
using ShelfMaster.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ShelfMaster.ViewModels
{
    public partial class LibrarianDashboardViewModel : ObservableObject
    {
        private readonly LibraryService _libraryService;
        private readonly Clerk _currentClerk;

        public string ClerkName => _currentClerk.FullName;
        public string ClerkEmail => _currentClerk.Email;

        [ObservableProperty] private ObservableCollection<Transaction> pendingRequests = new();
        [ObservableProperty] private ObservableCollection<Transaction> activeLoans = new();
        [ObservableProperty] private Transaction? selectedRequest;
        [ObservableProperty] private Transaction? selectedLoan;
        [ObservableProperty] private int activeLoansCount;
        [ObservableProperty] private string selectedSection = "Overview";

        public LibrarianDashboardViewModel(Clerk currentClerk, LibraryService libraryService)
        {
            _currentClerk = currentClerk;
            _libraryService = libraryService;
            _ = LoadPendingRequests();
        }

        [RelayCommand]
        private async Task LoadPendingRequests()
        {
            var requests = await _libraryService.GetPendingRequestsAsync();
            PendingRequests = new ObservableCollection<Transaction>(requests);

            var loans = await _libraryService.GetActiveLoansAsync();
            ActiveLoans = new ObservableCollection<Transaction>(loans);
            ActiveLoansCount = loans.Count;
        }

        [RelayCommand]
        private async Task ApproveAndIssue()
        {
            if (SelectedRequest == null)
            {
                MessageBox.Show("Please select a request to approve.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await _libraryService.IssueBookAsync(SelectedRequest.TransactionId, _currentClerk.ClerkId);
                MessageBox.Show($"Book issued successfully to {SelectedRequest.User.FullName}!",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadPendingRequests();
                SelectedRequest = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task RejectRequest()
        {
            if (SelectedRequest == null) return;

            if (MessageBox.Show("Reject this request?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                SelectedRequest.Status = TransactionStatus.Rejected;
                await _libraryService.UpdateTransactionAsync(SelectedRequest);
                await LoadPendingRequests();
                MessageBox.Show("Request rejected.");
            }
        }

        [RelayCommand]
        private async Task ReturnLoan(Transaction? loan)
        {
            var targetLoan = loan ?? SelectedLoan;

            if (targetLoan == null)
            {
                MessageBox.Show("Select a loan to return.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                await _libraryService.ReturnBookAsync(targetLoan.TransactionId);
                MessageBox.Show($"Book copy #{targetLoan.BookCopy.CopyNumber} returned by {targetLoan.User.FullName}.",
                    "Return complete", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadPendingRequests();
                SelectedLoan = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Return error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
    }
}