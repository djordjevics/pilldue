using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pilldue.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "medications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    PackageSizePills = table.Column<int>(type: "INTEGER", nullable: false),
                    PrescribedPackageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DailyDosagePills = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentStockPills = table.Column<int>(type: "INTEGER", nullable: false),
                    RefillDayOfMonthOverride = table.Column<int>(type: "INTEGER", nullable: true),
                    PrescriptionStartDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PrescriptionDurationMonths = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "refill_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MedicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PackageCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refill_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refill_events_medications_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "medications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "skip_dose_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MedicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PillsReturned = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skip_dose_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_skip_dose_events_medications_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "medications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_refill_events_MedicationId",
                table: "refill_events",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_skip_dose_events_MedicationId",
                table: "skip_dose_events",
                column: "MedicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refill_events");

            migrationBuilder.DropTable(
                name: "skip_dose_events");

            migrationBuilder.DropTable(
                name: "medications");
        }
    }
}
