using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortalCORE.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeResumePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResumePath",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResumePath",
                table: "Employees");
        }
    }
}
