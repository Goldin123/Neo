/*
    Author: Goldin Baloyi
    Name: spComment_GetById
    Description: Fetches a comment by its Id.
*/
CREATE PROCEDURE spComment_GetById
    @Id INT
AS
BEGIN
    SELECT * FROM Comments WHERE Id = @Id;
END
