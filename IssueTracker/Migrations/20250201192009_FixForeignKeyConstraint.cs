using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IssueTracker.Migrations
{
    /// <inheritdoc />
    public partial class FixForeignKeyConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Applications_ApplicationId",
                table: "Issues");

            migrationBuilder.AddColumn<string>(
                name: "DeveloperId",
                table: "Issues",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TesterId",
                table: "Issues",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Issues_DeveloperId",
                table: "Issues",
                column: "DeveloperId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_TesterId",
                table: "Issues",
                column: "TesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Applications_ApplicationId",
                table: "Issues",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_AspNetUsers_DeveloperId",
                table: "Issues",
                column: "DeveloperId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_AspNetUsers_TesterId",
                table: "Issues",
                column: "TesterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Applications_ApplicationId",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Issues_AspNetUsers_DeveloperId",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Issues_AspNetUsers_TesterId",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_DeveloperId",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_TesterId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "DeveloperId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "TesterId",
                table: "Issues");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Applications_ApplicationId",
                table: "Issues",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
