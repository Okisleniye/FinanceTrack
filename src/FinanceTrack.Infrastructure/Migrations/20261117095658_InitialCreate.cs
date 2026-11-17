using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinanceTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, defaultValue: "#0d6efd"),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "bi-tag"),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Budgets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Budgets_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Color", "CreatedAt", "Icon", "Name", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "#fd7e14", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bi-cup-hot", "Food", 2, null },
                    { 2, "#0dcaf0", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bi-car-front", "Transport", 2, null },
                    { 3, "#6f42c1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bi-bag", "Shopping", 2, null },
                    { 4, "#0d6efd", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bi-house-door", "Housing", 2, null },
                    { 5, "#d63384", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bi-controller", "Entertainment", 2, null },
                    { 6, "#dc3545", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bi-heart-pulse", "Health", 2, null },
                    { 7, "#20c997", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bi-mortarboard", "Education", 2, null },
                    { 8, "#6c757d", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bi-three-dots", "Other", 2, null },
                    { 9, "#198754", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bi-cash-stack", "Salary", 1, null },
                    { 10, "#20c997", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bi-laptop", "Freelance", 1, null },
                    { 11, "#0d6efd", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bi-briefcase", "Business", 1, null },
                    { 12, "#ffc107", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bi-graph-up-arrow", "Investments", 1, null },
                    { 13, "#17a2b8", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bi-wallet2", "Other Income", 1, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_CategoryId_Month_Year",
                table: "Budgets",
                columns: new[] { "CategoryId", "Month", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_Year_Month",
                table: "Budgets",
                columns: new[] { "Year", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name_Type",
                table: "Categories",
                columns: new[] { "Name", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoryId",
                table: "Transactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoryId_Date",
                table: "Transactions",
                columns: new[] { "CategoryId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Date",
                table: "Transactions",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Type",
                table: "Transactions",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Budgets");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
