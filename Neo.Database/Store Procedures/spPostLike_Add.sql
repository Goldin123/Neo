/*
    Author: Goldin Baloyi
    Name: spPostLike_Add
    Description: Adds a like to a post by a user.
    Usage:
        DECLARE @Id INT;
        EXEC spPostLike_Add 1, 2, @Id OUTPUT;
*/
CREATE PROCEDURE spPostLike_Add
    @PostId INT,
    @UserId INT,
    @NewId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN
            -- Prevent liking own post or duplicate like
            IF EXISTS (
                SELECT 1 FROM Posts WHERE Id = @PostId AND UserId = @UserId
            ) OR EXISTS (
                SELECT 1 FROM PostLikes WHERE PostId = @PostId AND UserId = @UserId
            )
            BEGIN
                SET @NewId = -1;
                ROLLBACK TRAN;
                RETURN;
            END

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
