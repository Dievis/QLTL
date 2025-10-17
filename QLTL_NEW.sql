
USE QLTL_NEW;
GO

CREATE DATABASE QLTL_NEW
GO

/* =============== PHÒNG BAN =============== */
CREATE TABLE Departments (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);

INSERT INTO Departments (DepartmentName, Description, IsDeleted)
VALUES (N'Không xác định', N'Phòng ban mặc định khi chưa gán hoặc bị xóa', 0);


/* =============== NGƯỜI DÙNG =============== */
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(200),
	Avatar NVARCHAR(500) NULL,
    Email NVARCHAR(150) UNIQUE,
    Phone NVARCHAR(20),
    DepartmentId INT NULL,
    IsSuperAdmin BIT NOT NULL DEFAULT 0,
    EmployeeCode NVARCHAR(50) UNIQUE,
    DateJoined DATE DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    ResetPasswordToken NVARCHAR(200) NULL,
    ResetPasswordExpiry DATETIME NULL,
    CONSTRAINT FK_Users_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId)
);

-- Tài khoản mặc định SA (super admin) - Password: 123 (bcrypt)
INSERT INTO Users
    (Username, PasswordHash, FullName, Avatar, Email, Phone, DepartmentId, IsSuperAdmin, EmployeeCode, DateJoined, IsActive, IsDeleted, CreatedAt, UpdatedAt, ResetPasswordToken, ResetPasswordExpiry)
VALUES
    (
      N'sa',
      N'$12$fh2r1KEYs2CXJeGEYHLH4e6Y83TJdkeLJvEaUY9BZxmTNsuP8BXuG',
      N'Super Admin',
      NULL,
      N'sa@localhost',
      NULL,
      NULL,
      1,                -- IsSuperAdmin = 1
      N'SA',
      GETDATE(),
      1,
      0,
      SYSDATETIME(),
      SYSDATETIME(),
      NULL,
      NULL
    );


/* =============== VAI TRÒ =============== */
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL,
    IsDefault BIT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);

INSERT INTO Roles (RoleName, Description, IsDefault) 
VALUES (N'User', N'Người dùng mặc định', 1);


/* =============== QUYỀN =============== */
CREATE TABLE Permissions (
    PermissionId INT IDENTITY(1,1) PRIMARY KEY,
    PermissionName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL,
    IsDefault BIT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);

INSERT INTO Permissions (PermissionName, Description, IsDefault) 
VALUES (N'View', N'Xem tài liệu mặc định', 1);


/* =============== ROLE - PERMISSION (N-N) =============== */
CREATE TABLE RolePermissions (
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
    CONSTRAINT FK_RolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId)
);


/* =============== USER - ROLE (N-N) =============== */
CREATE TABLE UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    DeletedAt DATETIME2 NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);


/* =============== LOẠI DANH MỤC =============== */
CREATE TABLE CategoryTypes (
    CategoryTypeId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryTypeName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL,
    IsDeleted BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 DEFAULT SYSDATETIME()
);


/* =============== DANH MỤC =============== */
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryTypeId INT NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL,
    IsDeleted BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Categories_CategoryTypes FOREIGN KEY (CategoryTypeId)
        REFERENCES CategoryTypes(CategoryTypeId)
);


/* =============== LOẠI TÀI LIỆU =============== */
CREATE TABLE DocumentTypes (
    DocumentTypeId INT IDENTITY(1,1) PRIMARY KEY,
    DocumentTypeName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL,
    IsDeleted BIT DEFAULT 0
	CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
);



/* =============== TÀI LIỆU =============== */
CREATE TABLE Documents (
    DocumentId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Content NVARCHAR(MAX) NULL,
    CategoryId INT NULL,
    DocumentTypeId INT NULL,
    UploaderID INT NOT NULL,
	FileName NVARCHAR(255) NULL,
    FilePath NVARCHAR(255) NULL,
    FileType NVARCHAR(50) NULL,
    FileSize BIGINT NULL,
    ApprovalStatus NVARCHAR(20) NOT NULL DEFAULT N'Pending',
    ApprovalDate DATETIME2 NULL,
    IsDeleted BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Documents_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    CONSTRAINT FK_Documents_Type FOREIGN KEY (DocumentTypeId) REFERENCES DocumentTypes(DocumentTypeId),
    CONSTRAINT FK_Documents_Uploader FOREIGN KEY (UploaderID) REFERENCES Users(UserID)
);

ALTER TABLE Documents
ADD FileName NVARCHAR(255) NULL,
    FileType NVARCHAR(50) NULL,
    FileSize BIGINT NULL;



/* =============== YÊU THÍCH TÀI LIỆU =============== */
Drop table FavoriteDocuments
CREATE TABLE FavoriteDocuments (
	FavoriteID INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    DocumentId INT NOT NULL,
    CONSTRAINT PK_FavoriteDocuments PRIMARY KEY (UserId, DocumentId),
    CONSTRAINT FK_FavDoc_User FOREIGN KEY (UserId) REFERENCES Users(UserID),
    CONSTRAINT FK_FavDoc_Document FOREIGN KEY (DocumentId) REFERENCES Documents(DocumentID)
);



/* =============== LIÊN KẾT TÀI LIỆU - PHÒNG BAN =============== */
CREATE TABLE DocumentDepartments (
    DocumentId INT NOT NULL,
    DepartmentId INT NOT NULL,
    IsDeleted BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    DeletedAt DATETIME2 NULL,
    CONSTRAINT PK_DocumentDepartments PRIMARY KEY (DocumentId, DepartmentId),
    CONSTRAINT FK_DocDept_Document FOREIGN KEY (DocumentId) REFERENCES Documents(DocumentID),
    CONSTRAINT FK_DocDept_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentID)
);


/* =============== PHÊ DUYỆT TÀI LIỆU =============== */
CREATE TABLE DocumentApproval (
    ApprovalID INT IDENTITY(1,1) PRIMARY KEY,
    DocumentID INT NOT NULL,
    UploaderID INT NOT NULL,
    ApproverID INT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT N'Pending',
    Reason NVARCHAR(255) NULL,
    DateUploaded DATETIME2 DEFAULT SYSDATETIME(),
    DateReviewed DATETIME2 NULL,
    CONSTRAINT FK_DocumentApproval_Document FOREIGN KEY (DocumentID) REFERENCES Documents(DocumentID),
    CONSTRAINT FK_DocumentApproval_Uploader FOREIGN KEY (UploaderID) REFERENCES Users(UserID),
    CONSTRAINT FK_DocumentApproval_Approver FOREIGN KEY (ApproverID) REFERENCES Users(UserID)
);


/* =============== LOG THAY ĐỔI TÀI LIỆU =============== */
CREATE TABLE DocumentChangeLog (
    ChangeID INT IDENTITY(1,1) PRIMARY KEY,
    DocumentID INT NOT NULL,
    ChangedBy INT NOT NULL,
    ChangeType NVARCHAR(50) NOT NULL,
    ChangeDescription NVARCHAR(500) NULL,
    ChangeDate DATETIME2 DEFAULT SYSDATETIME(),
    CONSTRAINT FK_DocumentChangeLog_Document FOREIGN KEY (DocumentID) REFERENCES Documents(DocumentID),
    CONSTRAINT FK_DocumentChangeLog_User FOREIGN KEY (ChangedBy) REFERENCES Users(UserID)
);


-- =============================================
-- 1. Thêm Loại danh mục (CategoryType)
-- =============================================
INSERT INTO CategoryTypes (CategoryTypeName)
VALUES 
(N'Quy định & Chính sách'),
(N'Hướng dẫn chuyên môn'),
(N'Hồ sơ quản lý'),
(N'Biểu mẫu hành chính'),
(N'Báo cáo & Thống kê');

-- =============================================
-- 2. Thêm Danh mục (Category)
-- =============================================
INSERT INTO Categories (CategoryName, CategoryTypeId)
VALUES
(N'Quy chế hoạt động', 1),
(N'Chính sách an toàn người bệnh', 1),

(N'Phác đồ điều trị', 2),
(N'Hướng dẫn sử dụng thuốc', 2),
(N'Quy trình chăm sóc', 2),

(N'Hồ sơ nhân sự', 3),
(N'Hồ sơ trang thiết bị y tế', 3),

(N'Mẫu đơn nghỉ phép', 4),
(N'Mẫu đăng ký khám bệnh', 4),

(N'Báo cáo tài chính', 5),
(N'Thống kê bệnh nhân', 5);

-- =============================================
-- 3. Thêm Loại tài liệu (DocumentType)
-- =============================================
INSERT INTO DocumentTypes (DocumentTypeName)
VALUES
(N'Quyết định'),
(N'Báo cáo'),
(N'Biểu mẫu'),
(N'Quy trình'),
(N'Hướng dẫn'),
(N'Hồ sơ');

-- =============================================
-- 4. Thêm Tài liệu (Document) - Sample 10
-- =============================================
INSERT INTO Documents (Title, CategoryId, DocumentTypeId, UploaderID, CreatedAt)
VALUES
(N'Quy chế hoạt động bệnh viện năm 2025', 1, 1, 1, SYSDATETIME()),
(N'Chính sách an toàn người bệnh mới', 2, 1, 1, SYSDATETIME()),
(N'Phác đồ điều trị Covid-19', 3, 5, 1, SYSDATETIME()),
(N'Hướng dẫn sử dụng thuốc kháng sinh', 4, 5, 1, SYSDATETIME()),
(N'Quy trình chăm sóc bệnh nhân hậu phẫu', 5, 4, 1, SYSDATETIME()),
(N'Hồ sơ nhân sự phòng cấp cứu', 6, 6, 1, SYSDATETIME()),
(N'Danh sách kiểm kê thiết bị y tế 2025', 7, 6, 1, SYSDATETIME()),
(N'Mẫu đơn xin nghỉ phép 2025', 8, 3, 1, SYSDATETIME()),
(N'Mẫu đăng ký khám bệnh ngoại trú', 9, 3, 1, SYSDATETIME()),
(N'Báo cáo tài chính quý 2 năm 2025', 10, 2, 1, SYSDATETIME());


/* ========================
   1. TRIGGER CHO USERS
   ======================== */
DROP TRIGGER IF EXISTS TRG_SoftDelete_Users;
GO
CREATE TRIGGER TRG_SoftDelete_Users
ON Users
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Xóa mềm UserRoles
    UPDATE ur
    SET ur.IsDeleted = 1, ur.DeletedAt = SYSDATETIME()
    FROM UserRoles ur
    INNER JOIN Inserted i ON ur.UserId = i.UserId
    WHERE i.IsDeleted = 1;

    -- FavoriteDocuments đã drop các cột liên quan, nên bỏ phần cập nhật này
END;
GO


/* ========================
   2. TRIGGER CHO ROLES
   ======================== */
DROP TRIGGER IF EXISTS TRG_SoftDelete_Roles;
GO
CREATE TRIGGER TRG_SoftDelete_Roles
ON Roles
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Xóa mềm UserRoles
    UPDATE ur
    SET ur.IsDeleted = 1, ur.DeletedAt = SYSDATETIME()
    FROM UserRoles ur
    INNER JOIN Inserted i ON ur.RoleId = i.RoleId
    WHERE i.IsDeleted = 1;

    -- Xóa mềm RolePermissions
    UPDATE rp
    SET rp.IsDeleted = 1, rp.UpdatedAt = SYSDATETIME()
    FROM RolePermissions rp
    INNER JOIN Inserted i ON rp.RoleId = i.RoleId
    WHERE i.IsDeleted = 1;
END;
GO


/* ========================
   3. TRIGGER CHO PERMISSIONS
   ======================== */
DROP TRIGGER IF EXISTS TRG_SoftDelete_Permissions;
GO
CREATE TRIGGER TRG_SoftDelete_Permissions
ON Permissions
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Xóa mềm RolePermissions
    UPDATE rp
    SET rp.IsDeleted = 1, rp.UpdatedAt = SYSDATETIME()
    FROM RolePermissions rp
    INNER JOIN Inserted i ON rp.PermissionId = i.PermissionId
    WHERE i.IsDeleted = 1;
END;
GO


/* ========================
   4. TRIGGER CHO DOCUMENTS
   ======================== */
DROP TRIGGER IF EXISTS TRG_SoftDelete_Documents;
GO

CREATE TRIGGER TRG_SoftDelete_Documents
ON Documents
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- DocumentDepartments vẫn giữ cột IsDeleted → vẫn update bình thường
    UPDATE dd
    SET dd.IsDeleted = 1, dd.DeletedAt = SYSDATETIME()
    FROM DocumentDepartments dd
    INNER JOIN Inserted i ON dd.DocumentId = i.DocumentId
    WHERE i.IsDeleted = 1;

    -- FavoriteDocuments đã drop các cột → bỏ phần cập nhật này
END;
GO



/* ========================
   5. TRIGGER CHO DEPARTMENTS
   ======================== */
DROP TRIGGER IF EXISTS TRG_SoftDelete_Departments;
GO
CREATE TRIGGER TRG_SoftDelete_Departments
ON Departments
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DefaultDeptId INT;
    SELECT TOP 1 @DefaultDeptId = DepartmentId FROM Departments WHERE DepartmentName = N'Không xác định';

    -- Khi phòng ban bị xóa mềm → nhân viên gán về phòng mặc định
    UPDATE u
    SET u.DepartmentId = @DefaultDeptId
    FROM Users u
    INNER JOIN Inserted i ON u.DepartmentId = i.DepartmentId
    WHERE i.IsDeleted = 1;
END;
GO
