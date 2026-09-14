using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniDesk.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddWidgetConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WidgetConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 100, nullable: false),
                    WidgetKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WidgetConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WidgetConfigurations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WidgetConfigurations_TenantId",
                table: "WidgetConfigurations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_WidgetConfigurations_WidgetKey",
                table: "WidgetConfigurations",
                column: "WidgetKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WidgetConfigurations");
        }
    }
}
