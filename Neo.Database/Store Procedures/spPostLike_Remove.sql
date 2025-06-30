/*
    Author: Goldin Baloyi
    Name: spPostLike_Remove
    Description: Removes a like from a post by a user.
*/
CREATE PROCEDURE spPostLike_Remove
    @PostId INT,
    @UserId INT
AS
BEGIN
    DELETE FROM PostLikes WHERE PostId = @PostId AND UserId = @UserId;
END
