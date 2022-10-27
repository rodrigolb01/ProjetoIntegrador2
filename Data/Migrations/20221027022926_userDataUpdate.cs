using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expenses_Manager.Data.Migrations
{
    public partial class userDataUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Category",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "MailAddress",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "UserData");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Category",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "MailAddress",
                table: "UserData");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "UserData");
        }
    }
}
