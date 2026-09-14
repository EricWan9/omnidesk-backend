using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniDesk.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerAndConversationRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Conversations_TenantId_Status",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "Conversations");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Conversations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CustomerId",
                table: "Conversations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_TenantId_CustomerId",
                table: "Conversations",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId",
                table: "Customers",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Customers_CustomerId",
                table: "Conversations",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Customers_CustomerId",
                table: "Conversations");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_CustomerId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_TenantId_CustomerId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Conversations");

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "Conversations",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "Conversations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_TenantId_Status",
                table: "Conversations",
                columns: new[] { "TenantId", "Status" });
        }
    }
}
