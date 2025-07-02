/*
    Author: Goldin Baloyi
    Name: spPost_GetLikeCount
    Description: Returns the like count for a post.
*/
CREATE PROCEDURE spPost_GetLikeCount
    @PostId INT
AS
BEGIN
    SELECT COUNT(*) FROM PostLikes WHERE PostId = @PostId;
END