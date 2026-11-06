using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeloCity.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMpkExternalIdentifiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExternalRunId",
                table: "ScheduledTrips",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExternalVariantId",
                table: "Routes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTrips_ExternalRunId",
                table: "ScheduledTrips",
                column: "ExternalRunId",
                unique: true,
                filter: "\"ExternalRunId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_ExternalVariantId",
                table: "Routes",
                column: "ExternalVariantId",
                unique: true,
                filter: "\"ExternalVariantId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScheduledTrips_ExternalRunId",
                table: "ScheduledTrips");

            migrationBuilder.DropIndex(
                name: "IX_Routes_ExternalVariantId",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "ExternalRunId",
                table: "ScheduledTrips");

            migrationBuilder.DropColumn(
                name: "ExternalVariantId",
                table: "Routes");
        }
    }
}
