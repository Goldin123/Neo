/*
    Author: Goldin Baloyi
    Name: spTag_GetTagsByPostId
    Description: Fetches all tags assigned to a specific post.
*/
CREATE PROCEDURE spTag_GetTagsByPostId
    @PostId INT
AS
BEGIN
    SELECT t.* FROM Tags t
    INNER JOIN PostTags pt ON t.Id = pt.TagId
    WHERE pt.PostId = @PostId;
END
