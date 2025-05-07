using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.MVC.Migrations
{
    /// <inheritdoc />
    public partial class Accountability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "submitter_id",
                table: "Projects",
                type: "text",
                nullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "bidder_id",
                table: "Bids",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Projects_submitter_id",
                table: "Projects",
                column: "submitter_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Bids_bidder_id",
                table: "Bids",
                column: "bidder_id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_AspNetUsers_bidder_id",
                table: "Bids",
                column: "bidder_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_AspNetUsers_submitter_id",
                table: "Projects",
                column: "submitter_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Bids_AspNetUsers_bidder_id", table: "Bids");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_AspNetUsers_submitter_id",
                table: "Projects"
            );

            migrationBuilder.DropIndex(name: "IX_Projects_submitter_id", table: "Projects");

            migrationBuilder.DropIndex(name: "IX_Bids_bidder_id", table: "Bids");

            migrationBuilder.DropColumn(name: "submitter_id", table: "Projects");

            migrationBuilder.AlterColumn<int>(
                name: "bidder_id",
                table: "Bids",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true
            );
        }
    }
}
