using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RPA.Web.Migrations
{
    public partial class update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "WorkerConfigurations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Group = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    JSONOptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CanRemoveOrDisable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerConfigurations", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "WorkerInfors",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DownloadPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CanRemoveOrDisable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerInfors", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkerConfigurations_IsActive",
                table: "WorkerConfigurations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WorkerInfors_IsActive",
                table: "WorkerInfors",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WorkerInfors_Name",
                table: "WorkerInfors",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkerConfigurations");

            migrationBuilder.DropTable(
                name: "WorkerInfors");

            migrationBuilder.CreateIndex(
                name: "IX_FormRecognizerLogs_SupplierID_ModelID_FileName_Type",
                table: "FormRecognizerLogs",
                columns: new[] { "SupplierID", "ModelID", "FileName", "Type" },
                unique: true);
        }
    }
}
