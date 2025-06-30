/*
    Author: Goldin Baloyi
    Name: spComment_Create
    Description: Adds a new comment to a post.
    Usage:
        DECLARE @Id INT;
        EXEC spComment_Create 1, 2, 'Nice post!', GETUTCDATE(), @Id OUTPUT;
*/
CREATE PROCEDURE spComment_Create
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
