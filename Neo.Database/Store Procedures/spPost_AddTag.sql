/*
    Author: Goldin Baloyi
    Name: spPost_AddTag
    Description: Adds a tag to a post (creates tag if not exists).
*/
CREATE PROCEDURE spPost_AddTag
    @PostId INT,
    @TagName VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TagId INT

    -- Create tag if it doesn't exist
    IF NOT EXISTS (SELECT 1 FROM Tags WHERE Name = @TagName)
    BEGIN
        INSERT INTO Tags (Name) VALUES (@TagName)
        SET @TagId = SCOPE_IDENTITY()
    END
    ELSE
        SELECT @TagId = Id FROM Tags WHERE Name = @TagName

    -- Insert PostTag relation if not exists
    IF NOT EXISTS (SELECT 1 FROM PostTags WHERE PostId = @PostId AND TagId = @TagId)
        INSERT INTO PostTags (PostId, TagId) VALUES (@PostId, @TagId)
END
