/*
    Author: Goldin Baloyi
    Name: spTag_GetByName
    Description: Fetches a tag by its name.
*/
CREATE PROCEDURE spTag_GetByName
    @Name NVARCHAR(50)
AS
BEGIN
    SELECT * FROM Tags WHERE Name = @Name;
END
