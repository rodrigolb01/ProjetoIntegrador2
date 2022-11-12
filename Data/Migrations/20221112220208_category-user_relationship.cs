using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expenses_Manager.Data.Migrations
{
    public partial class categoryuser_relationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Category",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Category");
        }
    }
}
