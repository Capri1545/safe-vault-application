-- SQL for creating Users table
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(100),
    Email VARCHAR(100)
);
