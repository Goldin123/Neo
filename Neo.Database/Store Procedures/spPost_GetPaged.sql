/*
    Name: spPost_GetPaged
    Description: Returns paged, filtered, and sorted list of posts with optional author/date/tag.
*/
CREATE PROCEDURE spPost_GetPaged
    @Page INT,
    @PageSize INT,
    @AuthorId INT = NULL,
    @Start DATETIME = NULL,
    @End DATETIME = NULL,
    @Tag VARCHAR(50) = NULL,
    @SortBy VARCHAR(50) = NULL,
    @Descending BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT p.*
    FROM Posts p
    LEFT JOIN PostTags pt ON p.Id = pt.PostId
    LEFT JOIN Tags t ON pt.TagId = t.Id
    WHERE (@AuthorId IS NULL OR p.UserId = @AuthorId)
      AND (@Start IS NULL OR p.CreatedAt >= @Start)
      AND (@End IS NULL OR p.CreatedAt <= @End)
      AND (@Tag IS NULL OR t.Name = @Tag)
    GROUP BY p.Id, p.UserId, p.Title, p.Content, p.CreatedAt, p.IsFlagged, p.FlagReason
    ORDER BY
        CASE WHEN @SortBy = 'CreatedAt' AND @Descending = 0 THEN p.CreatedAt END ASC,
        CASE WHEN @SortBy = 'CreatedAt' AND @Descending = 1 THEN p.CreatedAt END DESC,
        CASE WHEN @SortBy = 'Title' AND @Descending = 0 THEN p.Title END ASC,
        CASE WHEN @SortBy = 'Title' AND @Descending = 1 THEN p.Title END DESC,
        p.Id
    OFFSET (@Page - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
