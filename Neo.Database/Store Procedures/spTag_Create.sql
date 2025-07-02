/*
    Author : Goldin Baloyi
    Name: spTag_Create
    Description: Creates a tag if not exists and returns the Id.
*/
CREATE PROCEDURE spTag_Create
    @Name NVARCHAR(50),
    @NewId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN
            IF NOT EXISTS (SELECT 1 FROM Tags WHERE Name = @Name)
                INSERT INTO Tags (Name) VALUES (@Name);

            SELECT @NewId = Id FROM Tags WHERE Name = @Name;
        COMMIT TRAN
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END
