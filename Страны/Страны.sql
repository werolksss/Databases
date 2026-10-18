USE master;
GO

IF DB_ID(N'Страны') IS NULL
    EXEC(N'CREATE DATABASE [Страны]');
GO

USE [Страны];
GO

IF OBJECT_ID(N'dbo.Countries', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Countries
    (
        Id INT IDENTITY PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Capital NVARCHAR(100) NOT NULL,
        Population BIGINT NOT NULL,
        Area DECIMAL(15,2) NOT NULL,
        Continent NVARCHAR(50) NOT NULL
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Countries)
BEGIN
    INSERT INTO dbo.Countries
        (Name, Capital, Population, Area, Continent)
    VALUES
    (N'Australia', N'Canberra', 26000000, 7692024, N'Австралия'),
    (N'Austria', N'Vienna', 9000000, 83879, N'Европа'),
    (N'France', N'Paris', 68000000, 551695, N'Европа'),
    (N'Germany', N'Berlin', 84000000, 357588, N'Европа'),
    (N'Italy', N'Rome', 59000000, 301340, N'Европа'),
    (N'China', N'Beijing', 1410000000, 9596961, N'Азия'),
    (N'India', N'New Delhi', 1420000000, 3287263, N'Азия'),
    (N'Japan', N'Tokyo', 125000000, 377975, N'Азия'),
    (N'Kazakhstan', N'Astana', 20000000, 2724900, N'Азия'),
    (N'Egypt', N'Cairo', 110000000, 1002450, N'Африка'),
    (N'Algeria', N'Algiers', 45000000, 2381741, N'Африка'),
    (N'Tunisia', N'Tunis', 12000000, 163610, N'Африка'),
    (N'Seychelles', N'Victoria', 100000, 455, N'Африка'),
    (N'Canada', N'Ottawa', 40000000, 9984670, N'Северная Америка'),
    (N'Brazil', N'Brasilia', 215000000, 8515767, N'Южная Америка');
END;
GO

SELECT * FROM dbo.Countries;