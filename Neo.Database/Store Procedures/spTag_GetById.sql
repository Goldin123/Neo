/*
    Author: Goldin Baloyi
    Name: spTag_GetById
    Description: Fetches a tag by its Id.
*/
CREATE PROCEDURE spTag_GetById
    @Id INT
AS
BEGIN
    SELECT * FROM Tags WHERE Id = @Id;
END
