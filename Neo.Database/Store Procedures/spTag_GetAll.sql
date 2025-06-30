/*
    Author: Goldin Baloyi
    Name: spTag_GetAll
    Description: Fetches all tags.
*/
CREATE PROCEDURE spTag_GetAll
AS
BEGIN
    SELECT * FROM Tags ORDER BY Name;
END
