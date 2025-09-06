using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CharityDonationsApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLockoutCountToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LockoutCount",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockoutCount",
                table: "AspNetUsers");
        }
    }
}
