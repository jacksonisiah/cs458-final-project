using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.MVC.Migrations
{
    /// <inheritdoc />
    public partial class EventLogFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_EventLogs_Bids_bid_id", table: "EventLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_EventLogs_Projects_project_id",
                table: "EventLogs"
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "submitted_time",
                table: "Bids",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_EventLogs_Bids_bid_id",
                table: "EventLogs",
                column: "bid_id",
                principalTable: "Bids",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_EventLogs_Projects_project_id",
                table: "EventLogs",
                column: "project_id",
                principalTable: "Projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_EventLogs_Bids_bid_id", table: "EventLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_EventLogs_Projects_project_id",
                table: "EventLogs"
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "submitted_time",
                table: "Bids",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_EventLogs_Bids_bid_id",
                table: "EventLogs",
                column: "bid_id",
                principalTable: "Bids",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_EventLogs_Projects_project_id",
                table: "EventLogs",
                column: "project_id",
                principalTable: "Projects",
                principalColumn: "id"
            );
        }
    }
}
