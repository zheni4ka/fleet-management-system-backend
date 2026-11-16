using System;
using business_logic.Constants;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace data_access.Migrations
{
    /// <inheritdoc />
    public partial class AddDepotsAndArchiving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepotId",
                table: "Routes",
                type: "int",
                nullable: false,
                defaultValue: DepotConstraints.DefaultDepotId);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAtUtc",
                table: "Locations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Locations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAtUtc",
                table: "Drivers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepotId",
                table: "Drivers",
                type: "int",
                nullable: false,
                defaultValue: DepotConstraints.DefaultDepotId);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Drivers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAtUtc",
                table: "Autos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepotId",
                table: "Autos",
                type: "int",
                nullable: false,
                defaultValue: DepotConstraints.DefaultDepotId);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Autos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Depots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserDepots",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DepotId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDepots", x => new { x.UserId, x.DepotId });
                    table.ForeignKey(
                        name: "FK_UserDepots_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDepots_Depots_DepotId",
                        column: x => x.DepotId,
                        principalTable: "Depots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Depots",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Основне депо" });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ArchivedAtUtc", "IsArchived" },
                values: new object[] { null, false });

            migrationBuilder.CreateIndex(
                name: "IX_Routes_DepotId",
                table: "Routes",
                column: "DepotId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_IsArchived",
                table: "Locations",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_DepotId",
                table: "Drivers",
                column: "DepotId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_IsArchived",
                table: "Drivers",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_Autos_DepotId",
                table: "Autos",
                column: "DepotId");

            migrationBuilder.CreateIndex(
                name: "IX_Autos_IsArchived",
                table: "Autos",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_Depots_Name",
                table: "Depots",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDepots_DepotId",
                table: "UserDepots",
                column: "DepotId");

            migrationBuilder.AddForeignKey(
                name: "FK_Autos_Depots_DepotId",
                table: "Autos",
                column: "DepotId",
                principalTable: "Depots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Depots_DepotId",
                table: "Drivers",
                column: "DepotId",
                principalTable: "Depots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Depots_DepotId",
                table: "Routes",
                column: "DepotId",
                principalTable: "Depots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql($"INSERT INTO [UserDepots] ([UserId], [DepotId]) SELECT [userRole].[UserId], {DepotConstraints.DefaultDepotId} FROM [AspNetUserRoles] AS [userRole] INNER JOIN [AspNetRoles] AS [role] ON [role].[Id] = [userRole].[RoleId] WHERE [role].[NormalizedName] = N'{UserRoles.Dispatcher.ToUpperInvariant()}';");

            foreach (var depotScopedTableName in new[] { "Routes", "Drivers", "Autos" })
            {
                migrationBuilder.AlterColumn<int>(
                    name: "DepotId",
                    table: depotScopedTableName,
                    type: "int",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "int",
                    oldDefaultValue: DepotConstraints.DefaultDepotId);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Autos_Depots_DepotId",
                table: "Autos");

            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Depots_DepotId",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Depots_DepotId",
                table: "Routes");

            migrationBuilder.DropTable(
                name: "UserDepots");

            migrationBuilder.DropTable(
                name: "Depots");

            migrationBuilder.DropIndex(
                name: "IX_Routes_DepotId",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Locations_IsArchived",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_DepotId",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_IsArchived",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Autos_DepotId",
                table: "Autos");

            migrationBuilder.DropIndex(
                name: "IX_Autos_IsArchived",
                table: "Autos");

            migrationBuilder.DropColumn(
                name: "DepotId",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "ArchivedAtUtc",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "ArchivedAtUtc",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "DepotId",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "ArchivedAtUtc",
                table: "Autos");

            migrationBuilder.DropColumn(
                name: "DepotId",
                table: "Autos");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Autos");
        }
    }
}
