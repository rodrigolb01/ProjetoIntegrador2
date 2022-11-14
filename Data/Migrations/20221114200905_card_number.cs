using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expenses_Manager.Data.Migrations
{
    public partial class card_number : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "Card",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecurityCode",
                table: "Card",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Number",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "SecurityCode",
                table: "Card");
        }
    }
}
