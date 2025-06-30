/*
    Author: Goldin Baloyi
    Name: spPost_Flag
    Description: Flags a post as misleading or false, sets reason.
*/
CREATE PROCEDURE spPost_Flag
    @PostId INT,
    @ModeratorId INT,
    @Reason VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Posts
    SET IsFlagged = 1, FlagReason = @Reason
    WHERE Id = @PostId
END
