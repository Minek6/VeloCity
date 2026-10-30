using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VeloCity.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledTimetableArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ScheduledTripId",
                table: "Trips",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RouteId",
                table: "RouteStops",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LineId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routes_Lines_LineId",
                        column: x => x.LineId,
                        principalTable: "Lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCalendars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: false),
                    Monday = table.Column<bool>(type: "boolean", nullable: false),
                    Tuesday = table.Column<bool>(type: "boolean", nullable: false),
                    Wednesday = table.Column<bool>(type: "boolean", nullable: false),
                    Thursday = table.Column<bool>(type: "boolean", nullable: false),
                    Friday = table.Column<bool>(type: "boolean", nullable: false),
                    Saturday = table.Column<bool>(type: "boolean", nullable: false),
                    Sunday = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCalendars", x => x.Id);
                    table.CheckConstraint("CK_ServiceCalendar_ValidDates", "\"ValidFrom\" <= \"ValidTo\"");
                });

            migrationBuilder.CreateTable(
                name: "ScheduledTrips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RouteId = table.Column<int>(type: "integer", nullable: false),
                    ServiceCalendarId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTrips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledTrips_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledTrips_ServiceCalendars_ServiceCalendarId",
                        column: x => x.ServiceCalendarId,
                        principalTable: "ServiceCalendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledStopTimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScheduledTripId = table.Column<int>(type: "integer", nullable: false),
                    RouteStopId = table.Column<int>(type: "integer", nullable: false),
                    ArrivalTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    DepartureTime = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledStopTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledStopTimes_RouteStops_RouteStopId",
                        column: x => x.RouteStopId,
                        principalTable: "RouteStops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledStopTimes_ScheduledTrips_ScheduledTripId",
                        column: x => x.ScheduledTripId,
                        principalTable: "ScheduledTrips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Balance", "Email", "PasswordHash" },
                values: new object[] { 100.00m, "driver@driver.com", "$2a$12$3s7iX0hZX00hn6JFwKJ06elVcg0A5mw9LVx4QHvaZW.Q3MWEyrPA2" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Balance", "Email" },
                values: new object[] { 150.00m, "user@user.com" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Balance", "Email" },
                values: new object[] { 100.00m, "admin@admin.com" });

            migrationBuilder.CreateIndex(
                name: "IX_Trips_ScheduledTripId",
                table: "Trips",
                column: "ScheduledTripId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_RouteId_Sequence",
                table: "RouteStops",
                columns: new[] { "RouteId", "Sequence" },
                unique: true,
                filter: "\"RouteId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_LineId",
                table: "Routes",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledStopTimes_RouteStopId",
                table: "ScheduledStopTimes",
                column: "RouteStopId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledStopTimes_ScheduledTripId_RouteStopId",
                table: "ScheduledStopTimes",
                columns: new[] { "ScheduledTripId", "RouteStopId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTrips_RouteId",
                table: "ScheduledTrips",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTrips_ServiceCalendarId",
                table: "ScheduledTrips",
                column: "ServiceCalendarId");

            migrationBuilder.AddForeignKey(
                name: "FK_RouteStops_Routes_RouteId",
                table: "RouteStops",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_ScheduledTrips_ScheduledTripId",
                table: "Trips",
                column: "ScheduledTripId",
                principalTable: "ScheduledTrips",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RouteStops_Routes_RouteId",
                table: "RouteStops");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_ScheduledTrips_ScheduledTripId",
                table: "Trips");

            migrationBuilder.DropTable(
                name: "ScheduledStopTimes");

            migrationBuilder.DropTable(
                name: "ScheduledTrips");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "ServiceCalendars");

            migrationBuilder.DropIndex(
                name: "IX_Trips_ScheduledTripId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_RouteStops_RouteId_Sequence",
                table: "RouteStops");

            migrationBuilder.DropColumn(
                name: "ScheduledTripId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "RouteId",
                table: "RouteStops");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Balance", "Email", "PasswordHash" },
                values: new object[] { 0.00m, "driver@velocity.pl", "$2a$12$mVyskPgOLm8Ih5RumTF8xeXH1.B20XqSVu8SxcIOEV0F6EFmdNKMq" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Balance", "Email" },
                values: new object[] { 50.00m, "piboloz@student.wsb-nlu.edu.pl" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Balance", "Email" },
                values: new object[] { 0.00m, "dkflorek@student.wsb-nlu.edu.pl" });
        }
    }
}
