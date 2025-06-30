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
CREATE PROCEDURE spPost_Create
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
