using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BookToneApi.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchJobTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BatchId",
                table: "BatchProcessingLogs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BatchJobId",
                table: "BatchProcessingLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BatchJobDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BookId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchJobDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BatchJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TotalBooks = table.Column<int>(type: "integer", nullable: false),
                    ProcessedBooks = table.Column<int>(type: "integer", nullable: false),
                    FailedBooks = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchJobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatchProcessingLogs_BatchJobId",
                table: "BatchProcessingLogs",
                column: "BatchJobId");

            migrationBuilder.AddForeignKey(
                name: "FK_BatchProcessingLogs_BatchJobs_BatchJobId",
                table: "BatchProcessingLogs",
                column: "BatchJobId",
                principalTable: "BatchJobs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatchProcessingLogs_BatchJobs_BatchJobId",
                table: "BatchProcessingLogs");

            migrationBuilder.DropTable(
                name: "BatchJobDetails");

            migrationBuilder.DropTable(
                name: "BatchJobs");

            migrationBuilder.DropIndex(
                name: "IX_BatchProcessingLogs_BatchJobId",
                table: "BatchProcessingLogs");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "BatchProcessingLogs");

            migrationBuilder.DropColumn(
                name: "BatchJobId",
                table: "BatchProcessingLogs");
        }
    }
}
