using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ModifyOrderItemkey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_Items_Orders_OrderId", "Items");
            migrationBuilder.DropForeignKey("FK_Items_ProductVariants_ProductVariantId", "Items");
            migrationBuilder.DropForeignKey("FK_Items_Sizes_SizeId", "Items");

            // Drop old PK
            migrationBuilder.DropPrimaryKey("PK_Items", "Items");

            // Add new PK
            migrationBuilder.AddPrimaryKey(
                "PK_Items",
                "Items",
                new[] { "OrderId", "ProductVariantId", "SizeId" });

            // Recreate FKs
            migrationBuilder.AddForeignKey(
                "FK_Items_Orders_OrderId",
                "Items",
                "OrderId",
                "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                "FK_Items_ProductVariants_ProductVariantId",
                "Items",
                "ProductVariantId",
                "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                "FK_Items_Sizes_SizeId",
                "Items",
                "SizeId",
                "Sizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_Items_Orders_OrderId", "Items");
            migrationBuilder.DropForeignKey("FK_Items_ProductVariants_ProductVariantId", "Items");
            migrationBuilder.DropForeignKey("FK_Items_Sizes_SizeId", "Items");

            // Drop old PK
            migrationBuilder.DropPrimaryKey("PK_Items", "Items");

            // Add new PK
            migrationBuilder.AddPrimaryKey(
                "PK_Items",
                "Items",
                new[] { "OrderId", "ProductVariantId", "SizeId" });

            // Recreate FKs
            migrationBuilder.AddForeignKey(
                "FK_Items_Orders_OrderId",
                "Items",
                "OrderId",
                "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                "FK_Items_ProductVariants_ProductVariantId",
                "Items",
                "ProductVariantId",
                "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                "FK_Items_Sizes_SizeId",
                "Items",
                "SizeId",
                "Sizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
