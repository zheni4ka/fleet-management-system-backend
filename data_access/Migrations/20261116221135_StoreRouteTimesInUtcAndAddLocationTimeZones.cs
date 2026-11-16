using business_logic.Constants;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace data_access.Migrations
{
    /// <inheritdoc />
    public partial class StoreRouteTimesInUtcAndAddLocationTimeZones : Migration
    {
        private const string KyivSqlServerTimeZoneName = "FLE Standard Time";
        private const string UtcSqlServerTimeZoneName = "UTC";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"IF EXISTS (SELECT 1 FROM [Locations] WHERE [Country] NOT IN (N'{LocationConstraints.UkraineCountryName}', N'{LocationConstraints.PolandCountryName}')) THROW 50001, N'Locations outside Ukraine and Poland need an explicit TimeZoneId before this migration can run.', 1;");

            migrationBuilder.RenameColumn(
                name: "DepartureTime",
                table: "Routes",
                newName: "DepartureTimeUtc");

            migrationBuilder.RenameColumn(
                name: "ArrivalTime",
                table: "Routes",
                newName: "ArrivalTimeUtc");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_DriverId_Status_DepartureTime",
                table: "Routes",
                newName: "IX_Routes_DriverId_Status_DepartureTimeUtc");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_DepartureTime",
                table: "Routes",
                newName: "IX_Routes_DepartureTimeUtc");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_AutoId_Status_DepartureTime",
                table: "Routes",
                newName: "IX_Routes_AutoId_Status_DepartureTimeUtc");

            migrationBuilder.Sql($"UPDATE [Routes] SET [DepartureTimeUtc] = CONVERT(datetime2, ([DepartureTimeUtc] AT TIME ZONE N'{KyivSqlServerTimeZoneName}') AT TIME ZONE N'{UtcSqlServerTimeZoneName}'), [ArrivalTimeUtc] = CONVERT(datetime2, ([ArrivalTimeUtc] AT TIME ZONE N'{KyivSqlServerTimeZoneName}') AT TIME ZONE N'{UtcSqlServerTimeZoneName}');");

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "Locations",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql($"UPDATE [Locations] SET [TimeZoneId] = CASE [Country] WHEN N'{LocationConstraints.UkraineCountryName}' THEN N'{LocationConstraints.UkraineTimeZoneId}' WHEN N'{LocationConstraints.PolandCountryName}' THEN N'{LocationConstraints.PolandTimeZoneId}' END;");

            migrationBuilder.AlterColumn<string>(
                name: "TimeZoneId",
                table: "Locations",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldDefaultValue: "");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 1,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 2,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 3,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 4,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 5,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 6,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 7,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 8,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 9,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 10,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 11,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 12,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 13,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 14,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 15,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 16,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 17,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 18,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 19,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 20,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 21,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 22,
                column: "TimeZoneId",
                value: "Europe/Kyiv");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 24,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 25,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 26,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 27,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 28,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 29,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 30,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 31,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 32,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 33,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 34,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 35,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 36,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 37,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 38,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 39,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 40,
                column: "TimeZoneId",
                value: "Europe/Warsaw");

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 41,
                column: "TimeZoneId",
                value: "Europe/Warsaw");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"UPDATE [Routes] SET [DepartureTimeUtc] = CONVERT(datetime2, ([DepartureTimeUtc] AT TIME ZONE N'{UtcSqlServerTimeZoneName}') AT TIME ZONE N'{KyivSqlServerTimeZoneName}'), [ArrivalTimeUtc] = CONVERT(datetime2, ([ArrivalTimeUtc] AT TIME ZONE N'{UtcSqlServerTimeZoneName}') AT TIME ZONE N'{KyivSqlServerTimeZoneName}');");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "Locations");

            migrationBuilder.RenameColumn(
                name: "DepartureTimeUtc",
                table: "Routes",
                newName: "DepartureTime");

            migrationBuilder.RenameColumn(
                name: "ArrivalTimeUtc",
                table: "Routes",
                newName: "ArrivalTime");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_DriverId_Status_DepartureTimeUtc",
                table: "Routes",
                newName: "IX_Routes_DriverId_Status_DepartureTime");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_DepartureTimeUtc",
                table: "Routes",
                newName: "IX_Routes_DepartureTime");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_AutoId_Status_DepartureTimeUtc",
                table: "Routes",
                newName: "IX_Routes_AutoId_Status_DepartureTime");
        }
    }
}
