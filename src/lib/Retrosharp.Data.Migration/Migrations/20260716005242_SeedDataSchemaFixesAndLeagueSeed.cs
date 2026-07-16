using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Retrosharp.Data.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataSchemaFixesAndLeagueSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PitchingModel_Franchise_FranchiseId",
                table: "PitchingModel");

            migrationBuilder.DropForeignKey(
                name: "FK_PitchingModel_Person_PersonId",
                table: "PitchingModel");

            migrationBuilder.DropIndex(
                name: "IX_Franchise_FranchiseCode",
                table: "Franchise");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PitchingModel",
                table: "PitchingModel");

            migrationBuilder.RenameTable(
                name: "PitchingModel",
                newName: "Pitching");

            migrationBuilder.RenameIndex(
                name: "IX_PitchingModel_PersonId_FranchiseId_SeasonYear",
                table: "Pitching",
                newName: "IX_Pitching_PersonId_FranchiseId_SeasonYear");

            migrationBuilder.RenameIndex(
                name: "IX_PitchingModel_FranchiseId",
                table: "Pitching",
                newName: "IX_Pitching_FranchiseId");

            migrationBuilder.AlterColumn<string>(
                name: "DivisionCode",
                table: "Franchise",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<string>(
                name: "AlternateNickname",
                table: "Franchise",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "StateProvinceCountry",
                table: "Ballpark",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "ParkName",
                table: "Ballpark",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FirstGame",
                table: "Ballpark",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pitching",
                table: "Pitching",
                column: "Id");

            migrationBuilder.InsertData(
                table: "League",
                columns: new[] { "Id", "LeagueCode", "LeagueName" },
                values: new object[,]
                {
                    { 1, "AA", "American Association" },
                    { 2, "AL", "American League" },
                    { 3, "FL", "Federal League" },
                    { 4, "NA", "National Association" },
                    { 5, "NL", "National League" },
                    { 6, "PL", "Players League" },
                    { 7, "UA", "Union Association" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Franchise_FranchiseCode",
                table: "Franchise",
                column: "FranchiseCode");

            migrationBuilder.CreateIndex(
                name: "IX_Franchise_FranchiseIdentifier_FranchiseStart",
                table: "Franchise",
                columns: new[] { "FranchiseIdentifier", "FranchiseStart" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pitching_Franchise_FranchiseId",
                table: "Pitching",
                column: "FranchiseId",
                principalTable: "Franchise",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pitching_Person_PersonId",
                table: "Pitching",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pitching_Franchise_FranchiseId",
                table: "Pitching");

            migrationBuilder.DropForeignKey(
                name: "FK_Pitching_Person_PersonId",
                table: "Pitching");

            migrationBuilder.DropIndex(
                name: "IX_Franchise_FranchiseCode",
                table: "Franchise");

            migrationBuilder.DropIndex(
                name: "IX_Franchise_FranchiseIdentifier_FranchiseStart",
                table: "Franchise");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pitching",
                table: "Pitching");

            migrationBuilder.DeleteData(
                table: "League",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "League",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "League",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "League",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "League",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "League",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "League",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.RenameTable(
                name: "Pitching",
                newName: "PitchingModel");

            migrationBuilder.RenameIndex(
                name: "IX_Pitching_PersonId_FranchiseId_SeasonYear",
                table: "PitchingModel",
                newName: "IX_PitchingModel_PersonId_FranchiseId_SeasonYear");

            migrationBuilder.RenameIndex(
                name: "IX_Pitching_FranchiseId",
                table: "PitchingModel",
                newName: "IX_PitchingModel_FranchiseId");

            migrationBuilder.AlterColumn<string>(
                name: "DivisionCode",
                table: "Franchise",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AlternateNickname",
                table: "Franchise",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StateProvinceCountry",
                table: "Ballpark",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ParkName",
                table: "Ballpark",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FirstGame",
                table: "Ballpark",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PitchingModel",
                table: "PitchingModel",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Franchise_FranchiseCode",
                table: "Franchise",
                column: "FranchiseCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PitchingModel_Franchise_FranchiseId",
                table: "PitchingModel",
                column: "FranchiseId",
                principalTable: "Franchise",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PitchingModel_Person_PersonId",
                table: "PitchingModel",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
