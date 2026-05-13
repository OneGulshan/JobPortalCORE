using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortalCORE.Migrations
{
    public partial class AddAuditLogIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ❌ Maine yahan se Rename aur Primary Key drop karne wala faaltu code hata diya hai

            // ✅ 1. Column ki size choti kar rahe hain taaki Index lag sake (max par index nahi lagta)
            migrationBuilder.AlterColumn<string>(
                name: "TableName",
                table: "AuditLogs",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // ✅ 2. TableName par Index
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TableName",
                table: "AuditLogs",
                column: "TableName");

            // ✅ 3. Timestamp par Index
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TableName",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "TableName",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}