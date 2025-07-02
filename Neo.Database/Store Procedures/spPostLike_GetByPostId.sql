/*
    Author: Goldin Baloyi
    Name: spPostLike_GetByPostId
    Description: Returns a list of user likes.
    Returns: A list of likes per user 
*/
CREATE PROCEDURE spPostLike_GetByPostId
    @PostId INT
AS
BEGIN
    SELECT
        pl.Id,
        pl.PostId,
        pl.UserId,
        u.Username,         -- Join to get user name
        pl.CreatedAt
    FROM PostLikes pl
    INNER JOIN Users u ON pl.UserId = u.Id
    WHERE pl.PostId = @PostId
    ORDER BY pl.CreatedAt ASC
END