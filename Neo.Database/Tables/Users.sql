CREATE TABLE Users (
    Id INT IDENTITY PRIMARY KEY,
    Username VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(200) NOT NULL,
    Role VARCHAR(20) NOT NULL -- 'User' or 'Moderator'
);