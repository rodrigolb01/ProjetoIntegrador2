using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expenses_Manager.Data.Migrations
{
    public partial class receipt_data : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Receipt",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Receipt");
        }
    }
}
