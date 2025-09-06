using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CharityDonationsApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeUniqueIndexInAddresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Addresses_KycVerificationId_HouseNumber_Street_City_State_Country",
                table: "Addresses",
                columns: new[] { "KycVerificationId", "HouseNumber", "Street", "City", "State", "Country" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Addresses_KycVerificationId_HouseNumber_Street_City_State_Country",
                table: "Addresses");
        }
    }
}
