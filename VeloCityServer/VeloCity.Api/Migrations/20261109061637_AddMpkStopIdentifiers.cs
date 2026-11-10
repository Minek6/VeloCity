using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeloCity.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMpkStopIdentifiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExternalMpkStopId",
                table: "Stops",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalMpkStopNumber",
                table: "Stops",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stops_ExternalMpkStopId",
                table: "Stops",
                column: "ExternalMpkStopId",
                unique: true,
                filter: "\"ExternalMpkStopId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Stops_ExternalMpkStopNumber",
                table: "Stops",
                column: "ExternalMpkStopNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stops_ExternalMpkStopId",
                table: "Stops");

            migrationBuilder.DropIndex(
                name: "IX_Stops_ExternalMpkStopNumber",
                table: "Stops");

            migrationBuilder.DropColumn(
                name: "ExternalMpkStopId",
                table: "Stops");

            migrationBuilder.DropColumn(
                name: "ExternalMpkStopNumber",
                table: "Stops");
        }
    }
}
