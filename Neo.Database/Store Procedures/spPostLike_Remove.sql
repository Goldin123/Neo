/*
    Author: Goldin Baloyi
    Name: spPostLike_Remove
    Description: Removes a like from a post by a user.
    Returns:
      1  = Success (like removed)
     -1 = You have not liked this post
*/
CREATE PROCEDURE spPostLike_Remove
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
