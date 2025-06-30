/*
    Author: Goldin Baloyi
    Name: spUser_GetById
    Description: Fetches a user by their Id.
*/
CREATE PROCEDURE spUser_GetById
    @Id INT
AS
BEGIN
    SELECT TOP 1 Id, Username, PasswordHash, Role FROM Users WHERE Id = @Id;
END
