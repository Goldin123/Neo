/*
 Author: Goldin Baloyi
    Name: spComment_GetByPostId
    Description: Fetches all comments for a given post.
*/
CREATE PROCEDURE spComment_GetByPostId
    @PostId INT
AS
BEGIN
    SELECT * FROM Comments WHERE PostId = @PostId ORDER BY CreatedAt ASC;
END
