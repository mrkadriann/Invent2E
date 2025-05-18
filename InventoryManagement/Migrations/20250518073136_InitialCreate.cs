using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    customer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customer_picture = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    customer_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    customer_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    customer_status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    customer_remark = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer", x => x.customer_id);
                });

            migrationBuilder.CreateTable(
                name: "supplier",
                columns: table => new
                {
                    supplier_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    company_name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    profile_image = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    person_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    payment_method = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    courier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    portal_status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier", x => x.supplier_id);
                });

            migrationBuilder.CreateTable(
                name: "customer_information",
                columns: table => new
                {
                    customer_info_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    customer_email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    shipping_address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    billing_address = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_information", x => x.customer_info_id);
                    table.ForeignKey(
                        name: "FK_customer_information_customer_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplierContact",
                columns: table => new
                {
                    contact_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    supplier_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    supplier_company_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplierContact", x => x.contact_id);
                    table.ForeignKey(
                        name: "FK_supplierContact_supplier_supplier_company_id",
                        column: x => x.supplier_company_id,
                        principalTable: "supplier",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "descriptions",
                columns: table => new
                {
                    description_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_id = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    height = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    width = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    weight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    wholesale_price = table.Column<decimal>(type: "Decimal(18,2)", nullable: false),
                    retail_price = table.Column<decimal>(type: "Decimal(18,2)", nullable: true),
                    profit = table.Column<decimal>(type: "Decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_descriptions", x => x.description_id);
                });

            migrationBuilder.CreateTable(
                name: "image",
                columns: table => new
                {
                    image_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_id = table.Column<int>(type: "int", nullable: false),
                    image_data = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    image_order = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_image", x => x.image_id);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    item_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    supplier_id = table.Column<int>(type: "int", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    image_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.item_id);
                    table.ForeignKey(
                        name: "FK_product_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_product_image_image_id",
                        column: x => x.image_id,
                        principalTable: "image",
                        principalColumn: "image_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_product_supplier_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "supplier",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quantity",
                columns: table => new
                {
                    quantity_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quantity", x => x.quantity_id);
                    table.ForeignKey(
                        name: "FK_quantity_product_item_id",
                        column: x => x.item_id,
                        principalTable: "product",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_customer_information_customer_id",
                table: "customer_information",
                column: "customer_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_descriptions_item_id",
                table: "descriptions",
                column: "item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_image_item_id",
                table: "image",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_category_id",
                table: "product",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_image_id",
                table: "product",
                column: "image_id",
                unique: true,
                filter: "[image_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_product_supplier_id",
                table: "product",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_quantity_item_id",
                table: "quantity",
                column: "item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplier_company_name",
                table: "supplier",
                column: "company_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplierContact_supplier_company_id",
                table: "supplierContact",
                column: "supplier_company_id");

            migrationBuilder.AddForeignKey(
                name: "FK_descriptions_product_item_id",
                table: "descriptions",
                column: "item_id",
                principalTable: "product",
                principalColumn: "item_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_image_product_item_id",
                table: "image",
                column: "item_id",
                principalTable: "product",
                principalColumn: "item_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_image_product_item_id",
                table: "image");

            migrationBuilder.DropTable(
                name: "customer_information");

            migrationBuilder.DropTable(
                name: "descriptions");

            migrationBuilder.DropTable(
                name: "quantity");

            migrationBuilder.DropTable(
                name: "supplierContact");

            migrationBuilder.DropTable(
                name: "customer");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "image");

            migrationBuilder.DropTable(
                name: "supplier");
        }
    }
}
