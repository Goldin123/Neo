/*
    Author: Goldin Baloyi
    Name: spUser_Create
    Description: Creates a new user in the Users table.
    Parameters:
        @Username NVARCHAR(50): The user's username.
        @PasswordHash NVARCHAR(200): The hashed password.
        @Role NVARCHAR(20): The user's role.
        @NewId INT OUTPUT: The new user's ID.
    Usage:
        DECLARE @Id INT;
        EXEC spUser_Create 'jdoe', 'HASH', 'User', @Id OUTPUT;
*/
CREATE PROCEDURE spUser_Create
    @Username VARCHAR(50),
    @PasswordHash VARCHAR(200),
    @Role VARCHAR(20),
    @NewId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN
            INSERT INTO Users (Username, PasswordHash, Role)
            VALUES (@Username, @PasswordHash, @Role);

            SET @NewId = SCOPE_IDENTITY();
        COMMIT TRAN
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END
