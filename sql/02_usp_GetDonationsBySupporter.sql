-- Returns one supporter's donations, newest first.
USE DonationTracker;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetDonationsBySupporter
    @SupporterId int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, SupporterId, Amount, DonatedOn
    FROM dbo.Donations
    WHERE SupporterId = @SupporterId
    ORDER BY DonatedOn DESC;
END
GO
