-- Adds 1,000 supporters and 100,000 donations so execution plans are meaningful.
-- Every seeded supporter gets exactly 100 donations spread over the last two years.
-- Running this script again adds another full set of rows.
USE DonationTracker;
GO

SET NOCOUNT ON;

-- 1,000 supporters: Seed Supporter 1 .. Seed Supporter 1000.
WITH Numbers AS
(
    SELECT TOP (1000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N
    FROM sys.all_objects AS a
    CROSS JOIN sys.all_objects AS b
)
INSERT INTO dbo.Supporters (FullName, Email, CreatedOn)
SELECT
    CONCAT('Seed Supporter ', N),
    CONCAT('seed', N, '@example.com'),
    DATEADD(DAY, -(N % 365), SYSUTCDATETIME())
FROM Numbers;
GO

-- 100,000 donations. Donation N goes to seeded supporter number (N % 1000) + 1.
WITH Numbers AS
(
    SELECT TOP (100000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N
    FROM sys.all_objects AS a
    CROSS JOIN sys.all_objects AS b
),
SeedSupporters AS
(
    SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) AS RowNumber
    FROM dbo.Supporters
    WHERE Email LIKE 'seed%@example.com'
)
INSERT INTO dbo.Donations (SupporterId, Amount, DonatedOn)
SELECT
    s.Id,
    CAST(5 + (n.N % 200) * 2.5 AS decimal(10, 2)),              -- 5.00 to 502.50
    DATEADD(DAY, -(n.N % 730) - 1, CAST(GETDATE() AS date))     -- 1 to 730 days ago
FROM Numbers AS n
JOIN SeedSupporters AS s
    ON s.RowNumber = (n.N % 1000) + 1;
GO

SELECT
    (SELECT COUNT(*) FROM dbo.Supporters) AS SupporterCount,
    (SELECT COUNT(*) FROM dbo.Donations) AS DonationCount;
GO
