using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrapCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Traps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    TrapNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrapGroup = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignalStrength = table.Column<float>(type: "real", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BatteryPercentage = table.Column<int>(type: "int", nullable: false),
                    IndicatorStatus = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    LastEntryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalTransmissions = table.Column<int>(type: "int", nullable: false),
                    OperatingDays = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Traps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaptureEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaptureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActiveSensorCount = table.Column<int>(type: "int", nullable: false),
                    RodentWeightGrams = table.Column<double>(type: "float", nullable: false),
                    RodentLengthCm = table.Column<double>(type: "float", nullable: false),
                    RodentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignalStrength = table.Column<double>(type: "float", nullable: false),
                    NumberOfTransmissions = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaptureEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaptureEvents_Traps_TrapId",
                        column: x => x.TrapId,
                        principalTable: "Traps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaitMeasurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaptureEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeasurementTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BaitWeightGrams = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaitMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaitMeasurements_CaptureEvents_CaptureEventId",
                        column: x => x.CaptureEventId,
                        principalTable: "CaptureEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaitMeasurements_CaptureEventId",
                table: "BaitMeasurements",
                column: "CaptureEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CaptureEvents_TrapId",
                table: "CaptureEvents",
                column: "TrapId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaitMeasurements");

            migrationBuilder.DropTable(
                name: "CaptureEvents");

            migrationBuilder.DropTable(
                name: "Traps");
        }
    }
}
