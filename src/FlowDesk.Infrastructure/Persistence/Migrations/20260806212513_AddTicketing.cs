using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FlowDesk.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddTicketing : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "CompanyId",
            table: "Users",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "Categories",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                NormalizedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Categories", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Tickets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RequesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                Priority = table.Column<byte>(type: "tinyint", nullable: false),
                Status = table.Column<byte>(type: "tinyint", nullable: false),
                StatusChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                ResolvedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tickets", x => x.Id);
                table.CheckConstraint("CK_Tickets_LogicalDeletion", "([IsDeleted] = 0 AND [DeletedAtUtc] IS NULL AND [DeletedByUserId] IS NULL) OR ([IsDeleted] = 1 AND [DeletedAtUtc] IS NOT NULL AND [DeletedByUserId] IS NOT NULL)");
                table.CheckConstraint("CK_Tickets_Priority", "[Priority] IN (1, 2, 3, 4)");
                table.CheckConstraint("CK_Tickets_Status", "[Status] IN (1, 2, 3, 4)");
                table.CheckConstraint("CK_Tickets_StatusDates", "([Status] IN (1, 2) AND [ResolvedAtUtc] IS NULL AND [ClosedAtUtc] IS NULL) OR ([Status] = 3 AND [ResolvedAtUtc] IS NOT NULL AND [ClosedAtUtc] IS NULL) OR ([Status] = 4 AND [ResolvedAtUtc] IS NOT NULL AND [ClosedAtUtc] IS NOT NULL)");
                table.ForeignKey(
                    name: "FK_Tickets_Categories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Categories",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Tickets_Companies_CompanyId",
                    column: x => x.CompanyId,
                    principalTable: "Companies",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Tickets_Users_DeletedByUserId",
                    column: x => x.DeletedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Tickets_Users_RequesterId",
                    column: x => x.RequesterId,
                    principalTable: "Users",
                    principalColumn: "Id");
            });

        migrationBuilder.InsertData(
            table: "Categories",
            columns: new[] { "Id", "CreatedAtUtc", "Description", "IsActive", "Name", "NormalizedName", "UpdatedAtUtc" },
            values: new object[,]
            {
                { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "General service requests.", true, "General", "GENERAL", new DateTime(2026, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Authentication and permission problems.", true, "Access", "ACCESS", new DateTime(2026, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Physical equipment and device problems.", true, "Hardware", "HARDWARE", new DateTime(2026, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Application and software problems.", true, "Software", "SOFTWARE", new DateTime(2026, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2026, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Connectivity and network problems.", true, "Network", "NETWORK", new DateTime(2026, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) }
            });

        migrationBuilder.CreateIndex(
            name: "IX_Users_CompanyId",
            table: "Users",
            column: "CompanyId");

        migrationBuilder.CreateIndex(
            name: "UX_Categories_NormalizedName",
            table: "Categories",
            column: "NormalizedName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Tickets_CategoryId",
            table: "Tickets",
            column: "CategoryId",
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_Tickets_CompanyId_Status_CreatedAtUtc",
            table: "Tickets",
            columns: new[] { "CompanyId", "Status", "CreatedAtUtc" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_Tickets_DeletedByUserId",
            table: "Tickets",
            column: "DeletedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Tickets_RequesterId_CreatedAtUtc",
            table: "Tickets",
            columns: new[] { "RequesterId", "CreatedAtUtc" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_Tickets_Status_CreatedAtUtc",
            table: "Tickets",
            columns: new[] { "Status", "CreatedAtUtc" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.AddForeignKey(
            name: "FK_Users_Companies_CompanyId",
            table: "Users",
            column: "CompanyId",
            principalTable: "Companies",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Users_Companies_CompanyId",
            table: "Users");

        migrationBuilder.DropTable(
            name: "Tickets");

        migrationBuilder.DropTable(
            name: "Categories");

        migrationBuilder.DropIndex(
            name: "IX_Users_CompanyId",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "CompanyId",
            table: "Users");
    }
}
