-- Create a new table in the database
USE master ;  
GO  
DROP DATABASE IF EXISTS TestDB
CREATE DATABASE TestDB
GO 
EXEC sp_configure 'CONTAINED DATABASE AUTHENTICATION', 1
GO
RECONFIGURE
GO

-- Create a new table
USE TestDB;
CREATE TABLE places (
    [Id] INT NOT NULL IDENTITY PRIMARY KEY,
    [LatD] DECIMAL(38, 0) NOT NULL, 
    [LatM] DECIMAL(38, 0) NOT NULL, 
    [LatS] DECIMAL(38, 0) NOT NULL, 
    [NS] VARCHAR(max) NOT NULL, 
    [LonD] DECIMAL(38, 0) NOT NULL, 
    [LonM] DECIMAL(38, 0) NOT NULL, 
    [LonS] DECIMAL(38, 0) NOT NULL, 
    [EW] VARCHAR(max) NOT NULL, 
    [City] VARCHAR(max) NOT NULL, 
    [State] VARCHAR(max) NOT NULL
);
GO

-- Insert test data
BULK INSERT places
FROM '/data.csv'
WITH (
    DATAFILETYPE = 'char',
    FIRSTROW = 1,
    FIELDTERMINATOR = ',' ,
    ROWTERMINATOR = '0x0a'
)

-- Create a new user with required privileges for that table
USE TestDB;
ALTER DATABASE TestDB SET CONTAINMENT = PARTIAL
GO 
CREATE LOGIN TestDev WITH PASSWORD = 'Xj7hgrw@hn!49' -- Change this!
CREATE USER TestDev FOR LOGIN TestDev
GO
GRANT INSERT, SELECT TO TestDev
GO