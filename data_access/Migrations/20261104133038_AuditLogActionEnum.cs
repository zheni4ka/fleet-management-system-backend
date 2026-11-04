using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace data_access.Migrations
{
    /// <inheritdoc />
    public partial class AuditLogActionEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EntityId",
                table: "AuditLogs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE AuditLogs
SET EntityId = NULLIF(LTRIM(RTRIM(SUBSTRING([Action], LEN('Created Route') + 1, 450))), ''),
    [Action] = 'RouteCreated'
WHERE [Action] LIKE 'Created Route%';

UPDATE AuditLogs SET [Action] = 'RouteUpdated' WHERE [Action] LIKE '%Updated Route%';
UPDATE AuditLogs SET [Action] = 'RouteDeleted' WHERE [Action] LIKE '%Deleted Route%';
UPDATE AuditLogs SET [Action] = 'AutoCreated'  WHERE [Action] LIKE 'Created Auto%';
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM AuditLogs WHERE [Action] NOT IN (
    'RouteCreated','RouteUpdated','RouteDeleted',
    'AutoCreated','AutoUpdated','AutoStatusChanged','AutoDeleted',
    'DriverCreated','DriverUpdated','DriverDeleted',
    'LocationCreated','LocationDeleted',
    'MaintenanceCreated','MaintenanceUpdated','MaintenanceDeleted',
    'UserRegistered','UserDeleted'))
    THROW 50000, 'AuditLogs contains Action values that cannot be mapped to AuditAction. Map them manually and re-run the migration.', 1;
");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
