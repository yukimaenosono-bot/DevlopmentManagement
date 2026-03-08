using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Synapse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoutingAndWorkOrderOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "m_routings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_m_routings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_m_routings_m_items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "m_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "t_work_order_operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ActualStartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualEndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    DefectQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    WorkerUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_work_order_operations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_t_work_order_operations_m_equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "m_equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_t_work_order_operations_m_processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "m_processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_work_order_operations_t_work_orders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "t_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "m_routing_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoutingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    StandardTime = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_m_routing_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_m_routing_steps_m_equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "m_equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_m_routing_steps_m_processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "m_processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_m_routing_steps_m_routings_RoutingId",
                        column: x => x.RoutingId,
                        principalTable: "m_routings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_m_routing_steps_EquipmentId",
                table: "m_routing_steps",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_m_routing_steps_ProcessId",
                table: "m_routing_steps",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_m_routing_steps_RoutingId_Sequence",
                table: "m_routing_steps",
                columns: new[] { "RoutingId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_m_routings_ItemId_IsDefault",
                table: "m_routings",
                columns: new[] { "ItemId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_t_work_order_operations_EquipmentId",
                table: "t_work_order_operations",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_t_work_order_operations_ProcessId",
                table: "t_work_order_operations",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_t_work_order_operations_WorkOrderId",
                table: "t_work_order_operations",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_t_work_order_operations_WorkOrderId_Sequence",
                table: "t_work_order_operations",
                columns: new[] { "WorkOrderId", "Sequence" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "m_routing_steps");

            migrationBuilder.DropTable(
                name: "t_work_order_operations");

            migrationBuilder.DropTable(
                name: "m_routings");
        }
    }
}
