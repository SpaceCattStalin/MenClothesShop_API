using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ModifyOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SizeId",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Items",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "Items",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Items_SizeId",
                table: "Items",
                column: "SizeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Sizes_SizeId",
                table: "Items",
                column: "SizeId",
                principalTable: "Sizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Sizes_SizeId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_SizeId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "SizeId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "Items");
        }
    }
}
