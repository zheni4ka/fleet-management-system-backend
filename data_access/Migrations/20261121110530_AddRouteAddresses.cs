using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace data_access.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteAddresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DestinationAddressLatitude",
                table: "Routes",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationAddressLine",
                table: "Routes",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DestinationAddressLongitude",
                table: "Routes",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "StartAddressLatitude",
                table: "Routes",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartAddressLine",
                table: "Routes",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "StartAddressLongitude",
                table: "Routes",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationAddressLine",
                table: "DriverTrips",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartAddressLine",
                table: "DriverTrips",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationAddressLatitude",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "DestinationAddressLine",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "DestinationAddressLongitude",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "StartAddressLatitude",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "StartAddressLine",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "StartAddressLongitude",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "DestinationAddressLine",
                table: "DriverTrips");

            migrationBuilder.DropColumn(
                name: "StartAddressLine",
                table: "DriverTrips");
        }
    }
}
