using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pilldue.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissedDoseEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "missed_dose_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MedicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_missed_dose_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_missed_dose_events_medications_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "medications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_missed_dose_events_MedicationId",
                table: "missed_dose_events",
                column: "MedicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "missed_dose_events");
        }
    }
}
