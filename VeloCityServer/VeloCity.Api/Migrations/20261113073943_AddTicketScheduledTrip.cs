using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeloCity.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketScheduledTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ScheduledTripId",
                table: "Tickets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ServiceDate",
                table: "Tickets",
                type: "date",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ScheduledTripId", "ServiceDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ScheduledTripId", "ServiceDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ScheduledTripId", "ServiceDate" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ScheduledTripId",
                table: "Tickets",
                column: "ScheduledTripId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_ScheduledTrips_ScheduledTripId",
                table: "Tickets",
                column: "ScheduledTripId",
                principalTable: "ScheduledTrips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_ScheduledTrips_ScheduledTripId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ScheduledTripId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ScheduledTripId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ServiceDate",
                table: "Tickets");
        }
    }
}
