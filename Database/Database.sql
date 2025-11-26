CREATE DATABASE IF NOT EXISTS shelfmaster_db
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE shelfmaster_db;

CREATE TABLE Categories (
    CategoryId CHAR(36) NOT NULL,
    CategoryName VARCHAR(150) NOT NULL,
    Description TEXT NULL,
    PRIMARY KEY (CategoryId),
    UNIQUE KEY UX_Categories_CategoryName (CategoryName)
);

CREATE TABLE Books (
    BookId CHAR(36) NOT NULL,
    Title VARCHAR(200) NOT NULL,
    ISBN VARCHAR(50) NOT NULL,
    Author VARCHAR(150) NOT NULL,
    Publisher VARCHAR(150) NOT NULL,
    CategoryId CHAR(36) NOT NULL,
    CopyrightYear INT NULL,
    Summary TEXT NULL,
    PRIMARY KEY (BookId),
    CONSTRAINT FK_Books_Categories FOREIGN KEY (CategoryId)
        REFERENCES Categories(CategoryId) ON DELETE CASCADE
);

CREATE TABLE BookCopies (
    BookCopyId CHAR(36) NOT NULL,
    BookId CHAR(36) NOT NULL,
    CopyNumber INT NOT NULL,
    ShelfLocation VARCHAR(80) NOT NULL,
    Status INT NOT NULL,
    PRIMARY KEY (BookCopyId),
    CONSTRAINT FK_Copies_Books FOREIGN KEY (BookId)
        REFERENCES Books(BookId) ON DELETE CASCADE
);

CREATE TABLE Users (
    UserId CHAR(36) NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    MiddleName VARCHAR(100) NULL,
    LastName VARCHAR(100) NOT NULL,
    ContactNumber VARCHAR(50) NOT NULL,
    Address VARCHAR(255) NOT NULL,
    College VARCHAR(150) NOT NULL,
    Department VARCHAR(150) NOT NULL,
    Username VARCHAR(100) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    DateCreated DATETIME NOT NULL,
    DateUpdated DATETIME NOT NULL,
    Role INT NOT NULL,
    PRIMARY KEY (UserId),
    UNIQUE KEY UX_Users_Username (Username),
    UNIQUE KEY UX_Users_Email (Email)
);

CREATE TABLE UserAccounts (
    UserAccountId CHAR(36) NOT NULL,
    UserId CHAR(36) NOT NULL,
    Username VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    Password VARCHAR(255) NOT NULL,
    PRIMARY KEY (UserAccountId),
    UNIQUE KEY UX_UserAccounts_Username (Username),
    UNIQUE KEY UX_UserAccounts_Email (Email),
    CONSTRAINT FK_UserAccounts_Users FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE CASCADE
);

CREATE TABLE Clerks (
    ClerkId CHAR(36) NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    MiddleName VARCHAR(100) NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    ContactNumber VARCHAR(50) NOT NULL,
    PRIMARY KEY (ClerkId)
);

CREATE TABLE StaffAccounts (
    StaffAccountId CHAR(36) NOT NULL,
    ClerkId CHAR(36) NOT NULL,
    Username VARCHAR(100) NOT NULL,
    Password VARCHAR(255) NOT NULL,
    Role INT NOT NULL,
    PRIMARY KEY (StaffAccountId),
    UNIQUE KEY UX_StaffAccounts_Username (Username),
    CONSTRAINT FK_StaffAccounts_Clerks FOREIGN KEY (ClerkId)
        REFERENCES Clerks(ClerkId) ON DELETE CASCADE
);

CREATE TABLE Requests (
    RequestId CHAR(36) NOT NULL,
    UserId CHAR(36) NOT NULL,
    BookId CHAR(36) NOT NULL,
    BookCopyId CHAR(36) NOT NULL,
    RequestType VARCHAR(20) NOT NULL,
    DateRequested DATETIME NOT NULL,
    RequestStatus VARCHAR(20) NOT NULL,
    PRIMARY KEY (RequestId),
    CONSTRAINT FK_Requests_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Requests_Books FOREIGN KEY (BookId) REFERENCES Books(BookId),
    CONSTRAINT FK_Requests_Copies FOREIGN KEY (BookCopyId) REFERENCES BookCopies(BookCopyId)
);

CREATE TABLE Transactions (
    TransactionId CHAR(36) NOT NULL,
    UserId CHAR(36) NOT NULL,
    BookCopyId CHAR(36) NOT NULL,
    RequestId CHAR(36) NULL,
    ClerkId CHAR(36) NULL,
    Status INT NOT NULL,
    DateRequested DATETIME NOT NULL,
    DateIssued DATETIME NULL,
    DueDate DATETIME NULL,
    PRIMARY KEY (TransactionId),
    CONSTRAINT FK_Transactions_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Transactions_Copies FOREIGN KEY (BookCopyId) REFERENCES BookCopies(BookCopyId),
    CONSTRAINT FK_Transactions_Requests FOREIGN KEY (RequestId) REFERENCES Requests(RequestId),
    CONSTRAINT FK_Transactions_Clerks FOREIGN KEY (ClerkId) REFERENCES Clerks(ClerkId)
);

CREATE TABLE Returns (
    ReturnId CHAR(36) NOT NULL,
    TransactionId CHAR(36) NOT NULL,
    DateReturned DATETIME NOT NULL,
    FineAmount DECIMAL(10,2) NOT NULL,
    Remarks TEXT NULL,
    PRIMARY KEY (ReturnId),
    CONSTRAINT FK_Returns_Transactions FOREIGN KEY (TransactionId)
        REFERENCES Transactions(TransactionId) ON DELETE CASCADE
);