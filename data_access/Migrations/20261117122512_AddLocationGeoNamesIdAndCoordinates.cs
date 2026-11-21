using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace data_access.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationGeoNamesIdAndCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GeoNamesId",
                table: "Locations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Locations",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Locations",
                type: "float",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "GeoNamesId", "Latitude", "Longitude" },
                values: new object[] { null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_GeoNamesId",
                table: "Locations",
                column: "GeoNamesId",
                unique: true,
                filter: "[GeoNamesId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Locations_GeoNamesId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "GeoNamesId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Locations");
        }
    }
}
