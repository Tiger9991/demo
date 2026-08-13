using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTrapGroupsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrapGroups_GroupNumber",
                table: "TrapGroups");

            migrationBuilder.RenameColumn(
                name: "GroupNumber",
                table: "TrapGroups",
                newName: "TrapNumber");

            migrationBuilder.AddColumn<string>(
                name: "TrapGroup",
                table: "TrapGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TrapGroups_TrapGroup_TrapNumber",
                table: "TrapGroups",
                columns: new[] { "TrapGroup", "TrapNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrapGroups_TrapGroup_TrapNumber",
                table: "TrapGroups");

            migrationBuilder.DropColumn(
                name: "TrapGroup",
                table: "TrapGroups");

            migrationBuilder.RenameColumn(
                name: "TrapNumber",
                table: "TrapGroups",
                newName: "GroupNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TrapGroups_GroupNumber",
                table: "TrapGroups",
                column: "GroupNumber",
                unique: true);
        }
    }
}
