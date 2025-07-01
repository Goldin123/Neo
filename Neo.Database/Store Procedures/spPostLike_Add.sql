/*
    Author: Goldin Baloyi
    Name: spPostLike_Add
    Description: Adds a like to a post by a user.
    Usage:
        DECLARE @Id INT;
        EXEC spPostLike_Add 1, 2, @Id OUTPUT;
*/
Create PROCEDURE [dbo].[spPostLike_Add]
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


