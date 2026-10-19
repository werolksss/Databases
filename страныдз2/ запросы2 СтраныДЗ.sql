USE [СтраныДЗ];
GO

ALTER TABLE dbo.Countries ALTER COLUMN Population BIGINT NULL;
ALTER TABLE dbo.Cities ALTER COLUMN Population BIGINT NULL;
GO

CREATE OR ALTER VIEW dbo.CountryReport AS
SELECT c.Id, c.Name, c.Population, c.Area,
       p.Name AS Continent,
       city.Name AS Capital,
       city.Population AS CapitalPopulation
FROM dbo.Countries c
JOIN dbo.Continents p ON p.Id = c.ContinentId
LEFT JOIN dbo.Cities city
    ON city.CountryId = c.Id AND city.IsCapital = 1;
GO

CREATE OR ALTER PROCEDURE dbo.Top3Countries AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP(3) Name, Population
    FROM dbo.Countries
    WHERE Population IS NOT NULL
    ORDER BY Population DESC, Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.Top3Capitals AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP(3) Name, Population
    FROM dbo.Cities
    WHERE IsCapital = 1 AND Population IS NOT NULL
    ORDER BY Population DESC, Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.MostPopulatedCountry AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP(1) Name, Population
    FROM dbo.Countries
    WHERE Population IS NOT NULL
    ORDER BY Population DESC, Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.MostPopulatedCity AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP(1) Name, Population
    FROM dbo.Cities
    WHERE Population IS NOT NULL
    ORDER BY Population DESC, Id;
END;
GO