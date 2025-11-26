namespace ShelfMaster.Models.Enums
{
    public enum BookCopyStatus { Available, Borrowed, Lost, Damaged }
    public enum TransactionStatus { Pending, Approved, Issued, Returned, Overdue, Rejected }
    public enum UserRole { Student, Clerk, Admin }
}