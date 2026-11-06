using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeloCity.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStopOsmIdentifiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OsmId",
                table: "Stops",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OsmType",
                table: "Stops",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stops_OsmType_OsmId",
                table: "Stops",
                columns: new[] { "OsmType", "OsmId" },
                unique: true,
                filter: "\"OsmId\" IS NOT NULL AND \"OsmType\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stops_OsmType_OsmId",
                table: "Stops");

            migrationBuilder.DropColumn(
                name: "OsmId",
                table: "Stops");

            migrationBuilder.DropColumn(
                name: "OsmType",
                table: "Stops");
        }
    }
}
