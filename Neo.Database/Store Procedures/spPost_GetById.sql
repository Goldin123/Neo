/*
    Author: Goldin Baloyi
    Name: spPost_GetById
    Description: Fetches a post by its Id.
*/
CREATE PROCEDURE spPost_GetById
    @Id INT
AS
BEGIN
    SELECT * FROM Posts WHERE Id = @Id;
END
