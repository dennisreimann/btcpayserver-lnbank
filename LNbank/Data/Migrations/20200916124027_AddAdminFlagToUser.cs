using Microsoft.EntityFrameworkCore.Migrations;

namespace LNbank.Data.Migrations
{
    public partial class AddAdminFlagToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BTCPayIsAdmin",
                table: "Users",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BTCPayIsAdmin",
                table: "Users");
        }
    }
}
