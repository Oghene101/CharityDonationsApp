using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CharityDonationsApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexToBvnHashAndNinHashInKycVerifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_KycVerifications_BvnHash",
                table: "KycVerifications",
                column: "BvnHash",
                unique: true,
                filter: "[BvnHash] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_KycVerifications_NinHash",
                table: "KycVerifications",
                column: "NinHash",
                unique: true,
                filter: "[NinHash] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KycVerifications_BvnHash",
                table: "KycVerifications");

            migrationBuilder.DropIndex(
                name: "IX_KycVerifications_NinHash",
                table: "KycVerifications");
        }
    }
}
