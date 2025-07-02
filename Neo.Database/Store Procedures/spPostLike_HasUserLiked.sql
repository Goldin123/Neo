/*
    Author: Goldin Baloyi
    Name: spPostLike_HasUserLiked
    Description: Checks if a user has already liked a post.
*/
CREATE PROCEDURE spPostLike_HasUserLiked
    @PostId INT,
    @UserId INT
AS
BEGIN
    SELECT COUNT(*) FROM PostLikes WHERE PostId = @PostId AND UserId = @UserId;
END
