using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pilldue.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDoseIntervalDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DoseIntervalDays",
                table: "medications",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoseIntervalDays",
                table: "medications");
        }
    }
}
