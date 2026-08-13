using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateBaitMeasurements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trap_update_BaitMeasurements");

            migrationBuilder.AlterColumn<Guid>(
                name: "CaptureEventId",
                table: "BaitMeasurements",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "TrapId",
                table: "BaitMeasurements",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_BaitMeasurements_TrapId",
                table: "BaitMeasurements",
                column: "TrapId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaitMeasurements_Traps_TrapId",
                table: "BaitMeasurements",
                column: "TrapId",
                principalTable: "Traps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaitMeasurements_Traps_TrapId",
                table: "BaitMeasurements");

            migrationBuilder.DropIndex(
                name: "IX_BaitMeasurements_TrapId",
                table: "BaitMeasurements");

            migrationBuilder.DropColumn(
                name: "TrapId",
                table: "BaitMeasurements");

            migrationBuilder.AlterColumn<Guid>(
                name: "CaptureEventId",
                table: "BaitMeasurements",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "trap_update_BaitMeasurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    trapsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaitWeightGrams = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MeasurementTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trap_update_BaitMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trap_update_BaitMeasurements_Traps_trapsId",
                        column: x => x.trapsId,
                        principalTable: "Traps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trap_update_BaitMeasurements_trapsId",
                table: "trap_update_BaitMeasurements",
                column: "trapsId");
        }
    }
}
