using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace data_access.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverContactsAndTrips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Drivers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Drivers",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DriverTrips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: true),
                    AutoId = table.Column<int>(type: "int", nullable: false),
                    AutoMark = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    AutoModel = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    AutoNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    StartLocationCity = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartLocationCountry = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartLocationTimeZoneId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DestinationLocationCity = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DestinationLocationCountry = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DestinationLocationTimeZoneId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PlannedDepartureTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlannedArrivalTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DrivingMinutes = table.Column<int>(type: "int", nullable: false),
                    FinalStatus = table.Column<int>(type: "int", nullable: false),
                    DepotId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverTrips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverTrips_Depots_DepotId",
                        column: x => x.DepotId,
                        principalTable: "Depots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DriverTrips_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DriverTrips_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverTrips_DepotId",
                table: "DriverTrips",
                column: "DepotId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverTrips_DriverId_StartedAtUtc",
                table: "DriverTrips",
                columns: new[] { "DriverId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_DriverTrips_RouteId",
                table: "DriverTrips",
                column: "RouteId",
                unique: true,
                filter: "[RouteId] IS NOT NULL");

            migrationBuilder.Sql("""
                INSERT INTO [DriverTrips] (
                    [DriverId], [RouteId], [AutoId], [AutoMark], [AutoModel], [AutoNumber],
                    [StartLocationCity], [StartLocationCountry], [StartLocationTimeZoneId],
                    [DestinationLocationCity], [DestinationLocationCountry], [DestinationLocationTimeZoneId],
                    [PlannedDepartureTimeUtc], [PlannedArrivalTimeUtc], [StartedAtUtc], [EndedAtUtc],
                    [DrivingMinutes], [FinalStatus], [DepotId])
                SELECT
                    [r].[DriverId], [r].[Id], [r].[AutoId], [a].[Mark], [a].[Model], [a].[Number],
                    [startLocation].[City], [startLocation].[Country], [startLocation].[TimeZoneId],
                    [destinationLocation].[City], [destinationLocation].[Country], [destinationLocation].[TimeZoneId],
                    [r].[DepartureTimeUtc], [r].[ArrivalTimeUtc],
                    [trip].[StartedAtUtc], [trip].[EndedAtUtc],
                    CASE WHEN [trip].[EndedAtUtc] > [trip].[StartedAtUtc]
                        THEN DATEDIFF(minute, [trip].[StartedAtUtc], [trip].[EndedAtUtc]) ELSE 0 END,
                    [r].[Status], [r].[DepotId]
                FROM [Routes] AS [r]
                INNER JOIN [Autos] AS [a] ON [a].[Id] = [r].[AutoId]
                INNER JOIN [Locations] AS [startLocation] ON [startLocation].[Id] = [r].[StartLocationId]
                INNER JOIN [Locations] AS [destinationLocation] ON [destinationLocation].[Id] = [r].[DestinationLocationId]
                CROSS APPLY (SELECT
                    COALESCE([r].[ActualDepartureTimeUtc], [r].[DepartureTimeUtc]) AS [StartedAtUtc],
                    COALESCE([r].[ActualArrivalTimeUtc], [r].[ArrivalTimeUtc]) AS [EndedAtUtc]) AS [trip]
                WHERE [r].[Status] = 2;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverTrips");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Drivers");
        }
    }
}
