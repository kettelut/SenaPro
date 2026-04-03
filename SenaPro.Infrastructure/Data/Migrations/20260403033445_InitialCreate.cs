using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SenaPro.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sorteios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Concurso = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Dezena1 = table.Column<byte>(type: "smallint", nullable: false),
                    Dezena2 = table.Column<byte>(type: "smallint", nullable: false),
                    Dezena3 = table.Column<byte>(type: "smallint", nullable: false),
                    Dezena4 = table.Column<byte>(type: "smallint", nullable: false),
                    Dezena5 = table.Column<byte>(type: "smallint", nullable: false),
                    Dezena6 = table.Column<byte>(type: "smallint", nullable: false),
                    PremioSena = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    GanhadoresSena = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PremioQuina = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    GanhadoresQuina = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PremioQuadra = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    GanhadoresQuadra = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Conferido = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sorteios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sorteios_Concurso",
                table: "sorteios",
                column: "Concurso",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sorteios");
        }
    }
}
