using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopServiceDA.Migrations
{
    public partial class addedmapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Material_MaterialId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_MaterialId",
                table: "Product");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Product_MaterialId",
                table: "Product",
                column: "MaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Material_MaterialId",
                table: "Product",
                column: "MaterialId",
                principalTable: "Material",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
