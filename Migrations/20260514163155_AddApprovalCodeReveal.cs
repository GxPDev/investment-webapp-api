using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestmentWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalCodeReveal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ApprovalCodeRevealed",
                table: "WithdrawalRequests",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalCodeRevealed",
                table: "WithdrawalRequests");
        }
    }
}
