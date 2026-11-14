using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace data_access.Migrations
{
    /// <inheritdoc />
    public partial class ListSortingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Routes_DepartureTime",
                table: "Routes",
                column: "DepartureTime");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Time",
                table: "AuditLogs",
                column: "Time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Routes_DepartureTime",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Time",
                table: "AuditLogs");
        }
    }
}
