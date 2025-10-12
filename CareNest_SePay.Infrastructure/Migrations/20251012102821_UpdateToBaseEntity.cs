using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CareNest_SePay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TransactionContent",
                table: "SepayTransactions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Gateway",
                table: "SepayTransactions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "SepayTransactions",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "SepayTransactions",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SepayTransactions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "SepayTransactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SepayTransactions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "SepayTransactions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SepayTransactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "SepayTransactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "SepayTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SepayTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "SepayTransactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SepayTransactions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SepayTransactions_AccountNumber",
                table: "SepayTransactions",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SepayTransactions_CreatedAt",
                table: "SepayTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SepayTransactions_Status",
                table: "SepayTransactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SepayTransactions_TransactionId",
                table: "SepayTransactions",
                column: "TransactionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SepayTransactions_AccountNumber",
                table: "SepayTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SepayTransactions_CreatedAt",
                table: "SepayTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SepayTransactions_Status",
                table: "SepayTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SepayTransactions_TransactionId",
                table: "SepayTransactions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SepayTransactions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SepayTransactions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SepayTransactions");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "SepayTransactions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SepayTransactions");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "SepayTransactions");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "SepayTransactions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SepayTransactions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SepayTransactions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SepayTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionContent",
                table: "SepayTransactions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Gateway",
                table: "SepayTransactions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SepayTransactions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SepayTransactions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
