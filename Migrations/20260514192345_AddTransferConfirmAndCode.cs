using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestmentWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferConfirmAndCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RecipientConfirmed",
                table: "TransferRequests",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecipientConfirmedAt",
                table: "TransferRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferCode",
                table: "TransferRequests",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipientConfirmed",
                table: "TransferRequests");

            migrationBuilder.DropColumn(
                name: "RecipientConfirmedAt",
                table: "TransferRequests");

            migrationBuilder.DropColumn(
                name: "TransferCode",
                table: "TransferRequests");
        }
    }
}
