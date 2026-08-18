using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnalogHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnershipToUserContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Photos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Gear",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "FilmRolls",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Albums",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Existing rows predate per-user ownership (seed data, or anything created before this
            // migration) — assign them to the first-ever registered user (the bootstrap admin) rather
            // than leaving the all-zero sentinel default, which would fail the FK constraints added
            // below. A no-op on a fresh database: these tables have no rows yet to backfill.
            migrationBuilder.Sql("""
                UPDATE "Photos" SET "UserId" = (SELECT "Id" FROM "Users" ORDER BY "CreatedAtUtc" ASC LIMIT 1)
                WHERE "UserId" = '00000000-0000-0000-0000-000000000000';
                UPDATE "Gear" SET "UserId" = (SELECT "Id" FROM "Users" ORDER BY "CreatedAtUtc" ASC LIMIT 1)
                WHERE "UserId" = '00000000-0000-0000-0000-000000000000';
                UPDATE "FilmRolls" SET "UserId" = (SELECT "Id" FROM "Users" ORDER BY "CreatedAtUtc" ASC LIMIT 1)
                WHERE "UserId" = '00000000-0000-0000-0000-000000000000';
                UPDATE "Albums" SET "UserId" = (SELECT "Id" FROM "Users" ORDER BY "CreatedAtUtc" ASC LIMIT 1)
                WHERE "UserId" = '00000000-0000-0000-0000-000000000000';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Photos_UserId",
                table: "Photos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Gear_UserId",
                table: "Gear",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FilmRolls_UserId",
                table: "FilmRolls",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_UserId",
                table: "Albums",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_Users_UserId",
                table: "Albums",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FilmRolls_Users_UserId",
                table: "FilmRolls",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Gear_Users_UserId",
                table: "Gear",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Users_UserId",
                table: "Photos",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_Users_UserId",
                table: "Albums");

            migrationBuilder.DropForeignKey(
                name: "FK_FilmRolls_Users_UserId",
                table: "FilmRolls");

            migrationBuilder.DropForeignKey(
                name: "FK_Gear_Users_UserId",
                table: "Gear");

            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Users_UserId",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_UserId",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Gear_UserId",
                table: "Gear");

            migrationBuilder.DropIndex(
                name: "IX_FilmRolls_UserId",
                table: "FilmRolls");

            migrationBuilder.DropIndex(
                name: "IX_Albums_UserId",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Gear");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FilmRolls");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Albums");
        }
    }
}
