using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kuvik.ONDC.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMySqlTransactionState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    MessageId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<string>(type: "longtext", nullable: true),
                    ItemId = table.Column<string>(type: "longtext", nullable: true),
                    ApplicationReference = table.Column<string>(type: "longtext", nullable: true),
                    PayloadJson = table.Column<string>(type: "LONGTEXT", nullable: false),
                    ErrorCode = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_MessageId",
                table: "Transactions",
                column: "MessageId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
