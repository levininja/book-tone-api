using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BookToneApi.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResourceMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BookId = table.Column<int>(type: "integer", nullable: true),
                    CpuUsagePercent = table.Column<double>(type: "double precision", nullable: false),
                    MemoryUsageBytes = table.Column<long>(type: "bigint", nullable: false),
                    AvailableMemoryBytes = table.Column<long>(type: "bigint", nullable: false),
                    MemoryUsagePercent = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceMetrics", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourceMetrics");
        }
    }
}
