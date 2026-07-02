using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportMap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTimeSlotToDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_TimeSlotId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DayOfWeek",
                table: "TimeSlots");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "TimeSlots",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TimeSlotId",
                table: "Bookings",
                column: "TimeSlotId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_TimeSlotId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "TimeSlots");

            migrationBuilder.AddColumn<int>(
                name: "DayOfWeek",
                table: "TimeSlots",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TimeSlotId",
                table: "Bookings",
                column: "TimeSlotId");
        }
    }
}
