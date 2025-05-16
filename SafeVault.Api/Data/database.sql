-- SQL for creating Users table
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(100) NOT NULL UNIQUE,
    Password VARCHAR(200) NOT NULL,
    Role VARCHAR(50) NOT NULL,
    Email VARCHAR(100)
);
