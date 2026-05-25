using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestmentWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddWithdrawalProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FinalApprovedAt",
                table: "WithdrawalRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessingAt",
                table: "WithdrawalRequests",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalApprovedAt",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "ProcessingAt",
                table: "WithdrawalRequests");
        }
    }
}
