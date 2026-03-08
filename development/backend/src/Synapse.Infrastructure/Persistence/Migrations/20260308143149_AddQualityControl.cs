using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Synapse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_quality_inspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InspectionType = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    LotNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    InspectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InspectorUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    InspectionQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PassQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    FailQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Result = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_quality_inspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_t_quality_inspections_m_items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "m_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_quality_inspections_t_work_orders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "t_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "t_defects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefectNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    QualityInspectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    EstimatedCause = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CorrectiveAction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Disposition = table.Column<int>(type: "integer", nullable: false),
                    DispositionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_defects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_t_defects_m_items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "m_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_defects_m_processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "m_processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_t_defects_t_quality_inspections_QualityInspectionId",
                        column: x => x.QualityInspectionId,
                        principalTable: "t_quality_inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_t_defects_t_work_orders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "t_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_defects_DefectNumber",
                table: "t_defects",
                column: "DefectNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_t_defects_ItemId",
                table: "t_defects",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_t_defects_ItemId_OccurredAt",
                table: "t_defects",
                columns: new[] { "ItemId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_t_defects_ProcessId",
                table: "t_defects",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_t_defects_QualityInspectionId",
                table: "t_defects",
                column: "QualityInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_t_defects_WorkOrderId",
                table: "t_defects",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_t_quality_inspections_InspectionNumber",
                table: "t_quality_inspections",
                column: "InspectionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_t_quality_inspections_InspectionType",
                table: "t_quality_inspections",
                column: "InspectionType");

            migrationBuilder.CreateIndex(
                name: "IX_t_quality_inspections_ItemId_InspectedAt",
                table: "t_quality_inspections",
                columns: new[] { "ItemId", "InspectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_t_quality_inspections_WorkOrderId",
                table: "t_quality_inspections",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_defects");

            migrationBuilder.DropTable(
                name: "t_quality_inspections");
        }
    }
}
