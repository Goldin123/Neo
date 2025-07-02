# Neo Forum Backend

A robust, secure, and efficient backend API for a web-based text forum, designed to demonstrate best practices in modern C# backend development using Clean Architecture and CQRS.  
**Technology Stack:** ASP.NET Core, Dapper, MediatR, xUnit, FluentValidation, SQL Server.

## Table of Contents

- [Introduction](#introduction)
- [Technology Stack](#technology-stack)
- [Architecture Overview](#architecture-overview)
  - [Clean Architecture](#clean-architecture)
  - [CQRS with MediatR](#cqrs-with-mediatr)
  - [Input Validation (FluentValidation)](#input-validation-fluentvalidation)
- [Database Setup](#database-setup)
  - [1. Requirements](#1-requirements)
  - [2. Creating the Databases](#2-creating-the-databases)
  - [3. Create Tables](#3-create-tables)
  - [4. Create Stored Procedures](#4-create-stored-procedures)
- [How to Run the Project](#how-to-run-the-project)
- [Testing Strategy](#testing-strategy)
  - [Unit Tests](#unit-tests)
  - [Functional Tests](#functional-tests)
  - [Integration Tests](#integration-tests)
- [Key Business Rules](#key-business-rules)
- [API Documentation & Postman Collection](#api-documentation--postman-collection)

## Introduction

This project implements the backend API for a web text forum, supporting a small number of users. The system enforces strong business rules, such as no editing/deleting posts and restricted liking behavior, while supporting features such as post creation, comments, moderation, and filtering.  
The stack leverages modern .NET best practices:  
- **ASP.NET Core Web API** for HTTP services  
- **Dapper** for high-performance data access  
- **MediatR** for CQRS and application flow  
- **FluentValidation** for robust input validation  
- **xUnit** for comprehensive automated testing  
- **SQL Server** for relational storage, with extensive use of stored procedures for data operations.

## Technology Stack

- **.NET 8 (ASP.NET Core)**
- **Dapper** (micro-ORM for DB access)
- **MediatR** (CQRS, orchestration)
- **FluentValidation** (input validation)
- **SQL Server** (primary DB; supports stored procedures)
- **xUnit** (unit, functional, and integration testing)
- **JWT Auth** (local password-based, no external provider)
- **Postman** (API documentation & manual testing)

## Architecture Overview

### Clean Architecture

The solution is organized using Clean (Onion) Architecture principles:
- **Domain**: Core business logic and entities, independent of frameworks.
- **Application**: CQRS handlers, validation, DTOs, interfaces.
- **Infrastructure**: Data access (Dapper), repository implementations, external integrations.
- **API**: Controllers, dependency injection, application wiring.

This separation ensures high maintainability, testability, and independence of frameworks/tech.

### CQRS with MediatR

- **Command/Query Responsibility Segregation (CQRS):**  
  All write operations (commands) and read operations (queries) are handled by their respective request handlers, mediated by MediatR.
- **MediatR** orchestrates requests, enabling clear separation and single-responsibility of each handler.

### Input Validation (FluentValidation)

- All user input (DTOs) is validated using **FluentValidation** before it reaches business logic.
- Validation failures result in structured, user-friendly errors.

## Database Setup

### 1. Requirements

- **SQL Server** (local or Docker)
- Two databases:
  - `Neo` (main)
  - `Neotest` (test, used for automated integration tests)

### 2. Creating the Databases

You can use either SQL Server Management Studio (SSMS) or a SQL command-line tool:

```sql
CREATE DATABASE Neo;
CREATE DATABASE Neotest;

```
### 3. Create Tables

You can use either SQL Server Management Studio (SSMS) or a SQL command-line tool, Make sure you run these both on the databases created above:

```sql
CREATE TABLE [dbo].[Comments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PostId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[Content] [varchar](max) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[PostLikes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PostId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED ([Id] ASC),
 CONSTRAINT [UC_PostLike] UNIQUE NONCLUSTERED ([PostId] ASC, [UserId] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Posts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	  NOT NULL,
	[Content] [varchar](max) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[IsFlagged] [bit] NOT NULL,
	  NULL,
PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[PostTags](
	[PostId] [int] NOT NULL,
	[TagId] [int] NOT NULL,
PRIMARY KEY CLUSTERED ([PostId] ASC, [TagId] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Tags](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	  NOT NULL,
PRIMARY KEY CLUSTERED ([Id] ASC),
UNIQUE NONCLUSTERED ([Name] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	  NOT NULL,
	  NOT NULL,
	  NOT NULL,
PRIMARY KEY CLUSTERED ([Id] ASC),
UNIQUE NONCLUSTERED ([Username] ASC)
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Comments] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[PostLikes] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT ((0)) FOR [IsFlagged]
GO

ALTER TABLE [dbo].[Comments]  WITH CHECK ADD FOREIGN KEY([PostId]) REFERENCES [dbo].[Posts] ([Id])
GO
ALTER TABLE [dbo].[Comments]  WITH CHECK ADD FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[PostLikes]  WITH CHECK ADD FOREIGN KEY([PostId]) REFERENCES [dbo].[Posts] ([Id])
GO
ALTER TABLE [dbo].[PostLikes]  WITH CHECK ADD FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Posts]  WITH CHECK ADD FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[PostTags]  WITH CHECK ADD FOREIGN KEY([PostId]) REFERENCES [dbo].[Posts] ([Id])
GO
ALTER TABLE [dbo].[PostTags]  WITH CHECK ADD FOREIGN KEY([TagId]) REFERENCES [dbo].[Tags] ([Id])
GO


```

### 4. Create Stored Procedures

You can use either SQL Server Management Studio (SSMS) or a SQL command-line tool, Make sure you run these both on the databases created above:

```sql

/*
    Author: Goldin Baloyi
    Name: spComment_Create
    Description: Adds a new comment to a post.
    Usage:
        DECLARE @Id INT;
        EXEC spComment_Create 1, 2, 'Nice post!', GETUTCDATE(), @Id OUTPUT;
*/
CREATE PROCEDURE [dbo].[spComment_Create]
    @PostId INT,
    @UserId INT,
    @Content VARCHAR(MAX),
    @CreatedAt DATETIME,
    @NewId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN
            INSERT INTO Comments (PostId, UserId, Content, CreatedAt)
            VALUES (@PostId, @UserId, @Content, @CreatedAt);

            SET @NewId = SCOPE_IDENTITY();
        COMMIT TRAN
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[spComment_GetById]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spComment_GetById
    Description: Fetches a comment by its Id.
*/
CREATE PROCEDURE [dbo].[spComment_GetById]
    @Id INT
AS
BEGIN
    SELECT * FROM Comments WHERE Id = @Id;
END
GO
/****** Object:  StoredProcedure [dbo].[spComment_GetByPostId]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
 Author: Goldin Baloyi
    Name: spComment_GetByPostId
    Description: Fetches all comments for a given post.
*/
CREATE PROCEDURE [dbo].[spComment_GetByPostId]
    @PostId INT
AS
BEGIN
    SELECT * FROM Comments WHERE PostId = @PostId ORDER BY CreatedAt ASC;
END
GO
/****** Object:  StoredProcedure [dbo].[spPost_AddTag]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spPost_AddTag
    Description: Adds a tag to a post (creates tag if not exists).
*/
CREATE   PROCEDURE [dbo].[spPost_AddTag]
    @PostId INT,
    @TagName VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TagId INT

    -- Create tag if it doesn't exist
    IF NOT EXISTS (SELECT 1 FROM Tags WHERE Name = @TagName)
    BEGIN
        INSERT INTO Tags (Name) VALUES (@TagName)
        SET @TagId = SCOPE_IDENTITY()
    END
    ELSE
        SELECT @TagId = Id FROM Tags WHERE Name = @TagName

    -- Insert PostTag relation if not exists
    IF NOT EXISTS (SELECT 1 FROM PostTags WHERE PostId = @PostId AND TagId = @TagId)
        INSERT INTO PostTags (PostId, TagId) VALUES (@PostId, @TagId)
END
GO
/****** Object:  StoredProcedure [dbo].[spPost_Create]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spPost_Create
    Description: Creates a new post.
    Parameters:
        @UserId INT
        @Title NVARCHAR(200)
        @Content NVARCHAR(MAX)
        @CreatedAt DATETIME
        @NewId INT OUTPUT
*/
CREATE PROCEDURE [dbo].[spPost_Create]
    @UserId INT,
    @Title VARCHAR(200),
    @Content VARCHAR(MAX),
    @CreatedAt DATETIME,
    @NewId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN
            INSERT INTO Posts (UserId, Title, Content, CreatedAt)
            VALUES (@UserId, @Title, @Content, @CreatedAt);

            SET @NewId = SCOPE_IDENTITY();
        COMMIT TRAN
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[spPost_Flag]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spPost_Flag
    Description: Flags a post as misleading or false, sets reason.
*/
CREATE PROCEDURE [dbo].[spPost_Flag]
    @PostId INT,
    @ModeratorId INT,
    @Reason VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Posts
    SET IsFlagged = 1, FlagReason = @Reason
    WHERE Id = @PostId
END
GO
/****** Object:  StoredProcedure [dbo].[spPost_GetById]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spPost_GetById
    Description: Fetches a post by its Id.
*/
CREATE PROCEDURE [dbo].[spPost_GetById]
    @Id INT
AS
BEGIN
    SELECT * FROM Posts WHERE Id = @Id;
END
GO
/****** Object:  StoredProcedure [dbo].[spPost_GetLikeCount]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spPost_GetLikeCount
    Description: Returns the like count for a post.
*/
CREATE PROCEDURE [dbo].[spPost_GetLikeCount]
    @PostId INT
AS
BEGIN
    SELECT COUNT(*) FROM PostLikes WHERE PostId = @PostId;
END
GO
/****** Object:  StoredProcedure [dbo].[spPost_GetPaged]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Name: spPost_GetPaged
    Description: Returns paged, filtered, and sorted list of posts with optional author/date/tag.
*/
CREATE PROCEDURE [dbo].[spPost_GetPaged]
    @Page INT,
    @PageSize INT,
    @AuthorId INT = NULL,
    @Start DATETIME = NULL,
    @End DATETIME = NULL,
    @Tag NVARCHAR(50) = NULL,
    @SortBy NVARCHAR(50) = NULL,
    @Descending BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT p.Id, p.UserId, p.Title, p.Content, p.CreatedAt, p.IsFlagged, p.FlagReason, t.Name AS TagName
    FROM Posts p
    LEFT JOIN PostTags pt ON p.Id = pt.PostId
    LEFT JOIN Tags t ON pt.TagId = t.Id
    WHERE (@AuthorId IS NULL OR p.UserId = @AuthorId)
      AND (@Start IS NULL OR p.CreatedAt >= @Start)
      AND (@End IS NULL OR p.CreatedAt <= @End)
      AND (@Tag IS NULL OR t.Name = @Tag)
    ORDER BY
        CASE WHEN @SortBy = 'CreatedAt' AND @Descending = 0 THEN p.CreatedAt END ASC,
        CASE WHEN @SortBy = 'CreatedAt' AND @Descending = 1 THEN p.CreatedAt END DESC,
        CASE WHEN @SortBy = 'Title' AND @Descending = 0 THEN p.Title END ASC,
        CASE WHEN @SortBy = 'Title' AND @Descending = 1 THEN p.Title END DESC,
        p.Id
    OFFSET (@Page - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

END
GO
/****** Object:  StoredProcedure [dbo].[spPostLike_Add]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spPostLike_Add
    Description: Adds a like to a post by a user.
    Usage:
        DECLARE @Id INT;
        EXEC spPostLike_Add 1, 2, @Id OUTPUT;
*/
Create   PROCEDURE [dbo].[spPostLike_Add]
    @PostId INT,
    @UserId INT,
    @NewId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN

            -- Case 1: User cannot like their own post
            IF EXISTS (SELECT 1 FROM Posts WHERE Id = @PostId AND UserId = @UserId)
            BEGIN
                SET @NewId = -2;  -- -2: Cannot like own post
                ROLLBACK TRAN;
                RETURN;
            END

            -- Case 2: Prevent duplicate like
            IF EXISTS (SELECT 1 FROM PostLikes WHERE PostId = @PostId AND UserId = @UserId)
            BEGIN
                SET @NewId = -1;  -- -1: Already liked
                ROLLBACK TRAN;
                RETURN;
            END

            -- Insert like
            INSERT INTO PostLikes (PostId, UserId, CreatedAt)
            VALUES (@PostId, @UserId, GETUTCDATE());

            SET @NewId = SCOPE_IDENTITY();

        COMMIT TRAN
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END

GO
/****** Object:  StoredProcedure [dbo].[spPostLike_GetByPostId]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spPostLike_GetByPostId
    Description: Returns a list of user likes.
    Returns: A list of likes per user 
*/
CREATE PROCEDURE [dbo].[spPostLike_GetByPostId]
    @PostId INT
AS
BEGIN
    SELECT
        pl.Id,
        pl.PostId,
        pl.UserId,
        u.UserName,         -- Join to get user name
        pl.CreatedAt
    FROM PostLikes pl
    INNER JOIN Users u ON pl.UserId = u.Id
    WHERE pl.PostId = @PostId
    ORDER BY pl.CreatedAt ASC
END

GO
/****** Object:  StoredProcedure [dbo].[spPostLike_HasUserLiked]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spPostLike_HasUserLiked
    Description: Checks if a user has already liked a post.
*/
CREATE PROCEDURE [dbo].[spPostLike_HasUserLiked]
    @PostId INT,
    @UserId INT
AS
BEGIN
    SELECT COUNT(*) FROM PostLikes WHERE PostId = @PostId AND UserId = @UserId;
END
GO
/****** Object:  StoredProcedure [dbo].[spPostLike_Remove]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spPostLike_Remove
    Description: Removes a like from a post by a user.
    Returns:
      1  = Success (like removed)
     -1 = You have not liked this post
*/
CREATE   PROCEDURE [dbo].[spPostLike_Remove]
    @PostId INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM PostLikes WHERE PostId = @PostId AND UserId = @UserId)
    BEGIN
        SELECT -1 AS Result; -- You have not liked this post
        RETURN;
    END

    DELETE FROM PostLikes WHERE PostId = @PostId AND UserId = @UserId;
    SELECT 1 AS Result; -- Success
END
GO
/****** Object:  StoredProcedure [dbo].[spTag_Create]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author : Goldin Baloyi
    Name: spTag_Create
    Description: Creates a tag if not exists and returns the Id.
*/
CREATE PROCEDURE [dbo].[spTag_Create]
    @Name NVARCHAR(50),
    @NewId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN
            IF NOT EXISTS (SELECT 1 FROM Tags WHERE Name = @Name)
                INSERT INTO Tags (Name) VALUES (@Name);

            SELECT @NewId = Id FROM Tags WHERE Name = @Name;
        COMMIT TRAN
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[spTag_GetAll]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spTag_GetAll
    Description: Fetches all tags.
*/
CREATE PROCEDURE [dbo].[spTag_GetAll]
AS
BEGIN
    SELECT * FROM Tags ORDER BY Name;
END
GO
/****** Object:  StoredProcedure [dbo].[spTag_GetById]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spTag_GetById
    Description: Fetches a tag by its Id.
*/
CREATE PROCEDURE [dbo].[spTag_GetById]
    @Id INT
AS
BEGIN
    SELECT * FROM Tags WHERE Id = @Id;
END
GO
/****** Object:  StoredProcedure [dbo].[spTag_GetByName]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spTag_GetByName
    Description: Fetches a tag by its name.
*/
CREATE PROCEDURE [dbo].[spTag_GetByName]
    @Name NVARCHAR(50)
AS
BEGIN
    SELECT * FROM Tags WHERE Name = @Name;
END
GO
/****** Object:  StoredProcedure [dbo].[spTag_GetTagsByPostId]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spTag_GetTagsByPostId
    Description: Fetches all tags assigned to a specific post.
*/
CREATE PROCEDURE [dbo].[spTag_GetTagsByPostId]
    @PostId INT
AS
BEGIN
    SELECT t.* FROM Tags t
    INNER JOIN PostTags pt ON t.Id = pt.TagId
    WHERE pt.PostId = @PostId;
END
GO
/****** Object:  StoredProcedure [dbo].[spUser_Create]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spUser_Create
    Description: Creates a new user in the Users table.
    Parameters:
        @Username NVARCHAR(50): The user's username.
        @PasswordHash NVARCHAR(200): The hashed password.
        @Role NVARCHAR(20): The user's role.
        @NewId INT OUTPUT: The new user's ID.
    Usage:
        DECLARE @Id INT;
        EXEC spUser_Create 'jdoe', 'HASH', 'User', @Id OUTPUT;
*/
CREATE   PROCEDURE  [dbo].[spUser_Create]
    @Username VARCHAR(50),
    @PasswordHash VARCHAR(200),
    @Role VARCHAR(20),
    @NewId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN
            INSERT INTO Users (Username, PasswordHash, Role)
            VALUES (@Username, @PasswordHash, @Role);

            SET @NewId = SCOPE_IDENTITY();
        COMMIT TRAN
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[spUser_GetById]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spUser_GetById
    Description: Fetches a user by their Id.
*/
CREATE PROCEDURE [dbo].[spUser_GetById]
    @Id INT
AS
BEGIN
    SELECT TOP 1 Id, Username, PasswordHash, Role FROM Users WHERE Id = @Id;
END
GO
/****** Object:  StoredProcedure [dbo].[spUser_GetByUsername]    Script Date: 2025/07/01 21:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
    Author: Goldin Baloyi
    Name: spUser_GetByUsername
    Description: Fetches a user by their username.
*/
CREATE   PROCEDURE [dbo].[spUser_GetByUsername]
    @Username VARCHAR(50)
AS
BEGIN
    SELECT TOP 1 Id, Username, PasswordHash, Role FROM Users WHERE Username = @Username;
END
GO

```

## How to Run the Project

1. **Clone the repository:**
   ```bash
   git clone https://github.com/Goldin123/Neo.git
   cd Neo

2. **Configure connection strings:**  
   Open `appsettings.Development.json` (and/or `appsettings.json`) in the root or API project directory.  
   Replace the default connection strings with your actual SQL Server instance details for both `Neo` and `Neotest` databases. For example:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=Neo;User Id=your_user;Password=your_password;TrustServerCertificate=True;",
       "TestConnection": "Server=localhost;Database=Neotest;User Id=your_user;Password=your_password;TrustServerCertificate=True;"
     }
   }
3. **Restore dependencies & build:**
   Open a terminal in the project root and run:
   ```bash
   dotnet restore
   dotnet build
4. **Run the API:**
   ```bash
   dotnet run --project Neo.Api
## Testing Strategy

Testing is implemented with **xUnit** and covers:

### Unit Tests

- Isolated tests for business logic and validation (e.g., MediatR handlers, validators).
- Mocked repositories to avoid DB dependencies.

### Functional Tests

- Test the HTTP API surface with an in-memory or local DB, using the real MediatR handlers and pipeline.

### Integration Tests

- Spin up the full stack against the `Neotest` database.
- Validate end-to-end flows, including actual data persistence, auth, and business rules.

**Run all tests:**
```bash
dotnet test
```
## Key Business Rules

- **Users must be registered first** before posting any post via the Auth endpoints and login for a token.
- **Posts cannot be edited or deleted** once created (enforced at both API and database level).
- **Users can like a post only once**, and cannot like their own post.
- **Anonymous users** may view posts, but **must log in to create posts, comments, or like posts**.
- **User roles:**  
  - **Regular users:** Can post, comment, and like.  
  - **Moderators:** Have all user permissions and can additionally tag posts as "misleading or false information".
- **Posts and comments** can be retrieved with pagination, filters (date, author, tags), and sorted by date/like count.
- **All input** is validated for required fields, data type, and length before processing.

## API Documentation & Postman Collection

- A full **Postman collection** is included in the repository at `[solution root]/NeoForum.postman_collection.json` with preconfigured requests for:
  - User registration and login (JWT-based)
  - Creating posts and comments
  - Liking and unliking posts
  - Moderator flagging/tagging
  - Paging, filtering, and sorting posts and comments

Refer to the Postman documentation tab for request/response examples and usage instructions.

---

**For any issues, please open a GitHub Issue or PR.**

