using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SenaPro.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCamposAdicionaisSorteio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Acumulado",
                table: "sorteios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataProximoConcurso",
                table: "sorteios",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "DezenasOrdemSorteio",
                table: "sorteios",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalSorteio",
                table: "sorteios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MunicipioUFSorteio",
                table: "sorteios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorAcumuladoProximoConcurso",
                table: "sorteios",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorArrecadado",
                table: "sorteios",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorEstimadoProximoConcurso",
                table: "sorteios",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Acumulado",
                table: "sorteios");

            migrationBuilder.DropColumn(
                name: "DataProximoConcurso",
                table: "sorteios");

            migrationBuilder.DropColumn(
                name: "DezenasOrdemSorteio",
                table: "sorteios");

            migrationBuilder.DropColumn(
                name: "LocalSorteio",
                table: "sorteios");

            migrationBuilder.DropColumn(
                name: "MunicipioUFSorteio",
                table: "sorteios");

            migrationBuilder.DropColumn(
                name: "ValorAcumuladoProximoConcurso",
                table: "sorteios");

            migrationBuilder.DropColumn(
                name: "ValorArrecadado",
                table: "sorteios");

            migrationBuilder.DropColumn(
                name: "ValorEstimadoProximoConcurso",
                table: "sorteios");
        }
    }
}
