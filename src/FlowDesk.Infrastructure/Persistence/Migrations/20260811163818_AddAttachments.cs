using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowDesk.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddAttachments : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Attachments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UploadedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                StoredFileName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                ContentType = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Attachments", x => x.Id);
                table.CheckConstraint("CK_Attachments_ContentType", "[ContentType] IN ('application/pdf', 'image/png', 'image/jpeg')");
                table.CheckConstraint("CK_Attachments_SizeInBytes", "[SizeInBytes] > 0 AND [SizeInBytes] <= 10485760");
                table.ForeignKey(
                    name: "FK_Attachments_Tickets_TicketId",
                    column: x => x.TicketId,
                    principalTable: "Tickets",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Attachments_Users_UploadedById",
                    column: x => x.UploadedById,
                    principalTable: "Users",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Attachments_StoredFileName",
            table: "Attachments",
            column: "StoredFileName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Attachments_TicketId_CreatedAtUtc_Id",
            table: "Attachments",
            columns: new[] { "TicketId", "CreatedAtUtc", "Id" });

        migrationBuilder.CreateIndex(
            name: "IX_Attachments_UploadedById",
            table: "Attachments",
            column: "UploadedById");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Attachments");
    }
}
