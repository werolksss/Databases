USE master;
GO

IF DB_ID(N'СтраныДЗ') IS NULL
    EXEC(N'CREATE DATABASE [СтраныДЗ]');
GO

USE [СтраныДЗ];
GO

IF OBJECT_ID(N'dbo.Continents', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Continents
    (
        Id INT IDENTITY PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL UNIQUE
    );

    CREATE TABLE dbo.Countries
    (
        Id INT IDENTITY PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL UNIQUE,
        Population BIGINT NOT NULL,
        Area DECIMAL(15,2) NOT NULL,
        ContinentId INT NOT NULL
            REFERENCES dbo.Continents(Id)
    );

    CREATE TABLE dbo.Cities
    (
        Id INT IDENTITY PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Population BIGINT NOT NULL,
        IsCapital BIT NOT NULL,
        IsMajor BIT NOT NULL,
        CountryId INT NOT NULL
            REFERENCES dbo.Countries(Id),
        UNIQUE (CountryId, Name)
    );

    CREATE UNIQUE INDEX IX_OneCapital
        ON dbo.Cities(CountryId)
        WHERE IsCapital = 1;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Countries)
BEGIN
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    INSERT INTO dbo.Continents(Name)
    SELECT DISTINCT Continent
    FROM [Страны].dbo.Countries;

    INSERT INTO dbo.Countries
        (Name, Population, Area, ContinentId)
    SELECT c.Name, c.Population, c.Area, p.Id
    FROM [Страны].dbo.Countries c
    JOIN dbo.Continents p ON p.Name = c.Continent;

    INSERT INTO dbo.Countries
        (Name, Population, Area, ContinentId)
    SELECT N'Uganda', 45000000, 241550, Id
    FROM dbo.Continents
    WHERE Name = N'Африка';

    INSERT INTO dbo.Cities
        (Name, Population, IsCapital, IsMajor, CountryId)
    SELECT v.Capital, v.Population, 1, v.IsMajor, c.Id
    FROM (VALUES
        (N'Australia',  N'Canberra',  460000,   0),
        (N'Austria',    N'Vienna',    2000000,  1),
        (N'France',     N'Paris',     2100000,  1),
        (N'Germany',    N'Berlin',    3700000,  1),
        (N'Italy',      N'Rome',      2800000,  1),
        (N'China',      N'Beijing',   21000000, 1),
        (N'India',      N'New Delhi', 250000,   0),
        (N'Japan',      N'Tokyo',     14000000, 1),
        (N'Kazakhstan', N'Astana',    1400000,  1),
        (N'Egypt',      N'Cairo',     10000000, 1),
        (N'Algeria',    N'Algiers',   3000000,  1),
        (N'Tunisia',    N'Tunis',     600000,   1),
        (N'Seychelles', N'Victoria',  26000,    0),
        (N'Canada',     N'Ottawa',    1000000,  1),
        (N'Brazil',     N'Brasilia',  3000000,  1),
        (N'Uganda',     N'Kampala',   1700000,  1)
    ) v(Country, Capital, Population, IsMajor)
    JOIN dbo.Countries c ON c.Name = v.Country;

    INSERT INTO dbo.Cities
        (Name, Population, IsCapital, IsMajor, CountryId)
    SELECT v.City, v.Population, 0, 1, c.Id
    FROM (VALUES
        (N'Australia',  N'Sydney',        5300000),
        (N'Australia',  N'Melbourne',     5000000),
        (N'Austria',    N'Graz',          300000),
        (N'Austria',    N'Linz',          210000),
        (N'France',     N'Marseille',     870000),
        (N'France',     N'Lyon',          520000),
        (N'Germany',    N'Hamburg',       1900000),
        (N'Germany',    N'Munich',        1500000),
        (N'Italy',      N'Milan',         1400000),
        (N'Italy',      N'Naples',        900000),
        (N'China',      N'Shanghai',      25000000),
        (N'China',      N'Guangzhou',     18000000),
        (N'India',      N'Mumbai',        12000000),
        (N'India',      N'Bengaluru',     8400000),
        (N'Japan',      N'Yokohama',      3800000),
        (N'Japan',      N'Osaka',         2700000),
        (N'Kazakhstan', N'Almaty',        2200000),
        (N'Kazakhstan', N'Shymkent',      1200000),
        (N'Egypt',      N'Alexandria',    5500000),
        (N'Egypt',      N'Giza',          4400000),
        (N'Algeria',    N'Oran',          800000),
        (N'Algeria',    N'Constantine',   450000),
        (N'Tunisia',    N'Sfax',          330000),
        (N'Tunisia',    N'Sousse',        270000),
        (N'Canada',     N'Toronto',       2800000),
        (N'Canada',     N'Montreal',      1800000),
        (N'Brazil',     N'Sao Paulo',     12000000),
        (N'Brazil',     N'Rio de Janeiro',6000000),
        (N'Uganda',     N'Gulu',          150000),
        (N'Uganda',     N'Mbarara',       100000)
    ) v(Country, City, Population)
    JOIN dbo.Countries c ON c.Name = v.Country;

    COMMIT TRANSACTION;
END;
GO

SELECT * FROM dbo.Continents;
SELECT * FROM dbo.Countries;
SELECT * FROM dbo.Cities;