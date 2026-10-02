using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonationTracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddDonationsBySupporterProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EF Core has no model for stored procedures, so this migration
            // carries the SQL itself. The same text is in
            // sql/02_usp_GetDonationsBySupporter.sql.
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE dbo.usp_GetDonationsBySupporter
    @SupporterId int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, SupporterId, Amount, DonatedOn
    FROM dbo.Donations
    WHERE SupporterId = @SupporterId
    ORDER BY DonatedOn DESC;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.usp_GetDonationsBySupporter;");
        }
    }
}
