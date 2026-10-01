-- Covering index for usp_GetDonationsBySupporter.
-- Key columns match the WHERE and ORDER BY; Amount is included so the query
-- never has to go back to the clustered index. Id is the clustering key, so
-- it is stored in every nonclustered index automatically.
USE DonationTracker;
GO

CREATE NONCLUSTERED INDEX IX_Donations_SupporterId_DonatedOn
    ON dbo.Donations (SupporterId, DonatedOn DESC)
    INCLUDE (Amount);
GO
