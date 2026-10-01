-- Covering index for usp_GetDonationsBySupporter.
-- Key columns match the WHERE and ORDER BY; Amount is included so the query
-- never has to go back to the clustered index. Id is the clustering key, so
-- it is stored in every nonclustered index automatically.
--
-- This script is how the index was first created and measured (see
-- PLAN-NOTES.md). The index now belongs to the EF Core migration
-- AddDonationSupporterDateIndex, so on an up-to-date database it already
-- exists and this script does nothing.
USE DonationTracker;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Donations_SupporterId_DonatedOn'
      AND object_id = OBJECT_ID('dbo.Donations'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Donations_SupporterId_DonatedOn
        ON dbo.Donations (SupporterId, DonatedOn DESC)
        INCLUDE (Amount);
END
GO
