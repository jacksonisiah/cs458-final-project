using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.MVC.Migrations
{
    /// <inheritdoc />
    public partial class EventLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "bid_id",
                table: "EventLogs",
                type: "integer",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "project_id",
                table: "EventLogs",
                type: "integer",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_bid_id",
                table: "EventLogs",
                column: "bid_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_project_id",
                table: "EventLogs",
                column: "project_id"
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_EventLogs_Bids_bid_id", table: "EventLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_EventLogs_Projects_project_id",
                table: "EventLogs"
            );

            migrationBuilder.DropIndex(name: "IX_EventLogs_bid_id", table: "EventLogs");

            migrationBuilder.DropIndex(name: "IX_EventLogs_project_id", table: "EventLogs");

            migrationBuilder.DropColumn(name: "bid_id", table: "EventLogs");

            migrationBuilder.DropColumn(name: "project_id", table: "EventLogs");
        }
    }
}
