using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expenses_Manager.Data.Migrations
{
    public partial class FK_relationships : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_UserData_AspNetUser_UserId",
                table: "UserData",
                column: "UserId",
                principalTable: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipt_AspNetUser_UserId",
                table: "Receipt",
                column: "UserId",
                principalTable: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_Card_AspNetUser",
                table: "Card",
                column: "UserId",
                principalTable: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_Expense_AspNetUser_UserId",
                table: "Expense",
                column: "UserId",
                principalTable: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_Expense_Receipt_ReceiptId",
                table: "Expense",
                column: "ReceiptId",
                principalTable: "Receipt");

            migrationBuilder.AddForeignKey(
                name: "FK_Expense_Category_CategoryId",
                table: "Expense",
                column: "CategoryId",
                principalTable: "Category");

            migrationBuilder.AddForeignKey(
                name: "FK_Expense_Card_PaymentMethodId",
                table: "Expense",
                column: "PaymentMethodId",
                principalTable: "Card");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserData_AspNetUser_UserId",
                table: "UserData");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipt_AspNetUser_UserId",
                table: "Receipt");

            migrationBuilder.DropForeignKey(
                name: "FK_Card_AspNetUser",
                table: "Card");

            migrationBuilder.DropForeignKey(
                name: "FK_Expense_AspNetUser_UserId",
                table: "Expense");

            migrationBuilder.DropForeignKey(
                name: "FK_Expense_Receipt_ReceiptI",
                table: "Expense");

            migrationBuilder.DropForeignKey(
               name: "FK_Expense_Category_CategoryId",
               table: "Expense");

            migrationBuilder.DropForeignKey(
               name: "FK_Expense_Card_PaymentMethodId",
               table: "Expense");
        }
    }
}
