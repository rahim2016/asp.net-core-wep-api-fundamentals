using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CityInfoAPI.Migrations
{
    /// <inheritdoc />
    public partial class CityInfoDbAddedOmmitedDesction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PointsOfInterest",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "A very tall building.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PointsOfInterest",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: null);
        }
    }
}
