/*
    Author: Goldin Baloyi
    Name: spUser_GetByUsername
    Description: Fetches a user by their username.
*/
CREATE PROCEDURE spUser_GetByUsername
    @Username VARCHAR(50)
AS
BEGIN
    SELECT TOP 1 Id, Username, PasswordHash, Role FROM Users WHERE Username = @Username;
END
