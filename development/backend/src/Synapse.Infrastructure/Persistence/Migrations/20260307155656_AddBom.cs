using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Synapse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "m_bom",
                columns: table => new
                {
                    ParentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_m_bom", x => new { x.ParentItemId, x.ChildItemId });
                    table.ForeignKey(
                        name: "FK_m_bom_m_items_ChildItemId",
                        column: x => x.ChildItemId,
                        principalTable: "m_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_m_bom_m_items_ParentItemId",
                        column: x => x.ParentItemId,
                        principalTable: "m_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_m_bom_ChildItemId",
                table: "m_bom",
                column: "ChildItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "m_bom");
        }
    }
}
