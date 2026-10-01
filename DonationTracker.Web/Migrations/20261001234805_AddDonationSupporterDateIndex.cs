using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonationTracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddDonationSupporterDateIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Donations_SupporterId",
                table: "Donations");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_SupporterId_DonatedOn",
                table: "Donations",
                columns: new[] { "SupporterId", "DonatedOn" },
                descending: new[] { false, true })
                .Annotation("SqlServer:Include", new[] { "Amount" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Donations_SupporterId_DonatedOn",
                table: "Donations");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_SupporterId",
                table: "Donations",
                column: "SupporterId");
        }
    }
}
