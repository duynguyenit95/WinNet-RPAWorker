using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RPA.Web.Migrations
{
    public partial class @base : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FormRecognizerLogs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    SupplierID = table.Column<int>(type: "int", nullable: false),
                    ModelID = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CanRemoveOrDisable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormRecognizerLogs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "RegexInfors",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Pattern = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DateFormat = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Options = table.Column<int>(type: "int", nullable: false),
                    SplitContent = table.Column<bool>(type: "bit", nullable: false),
                    ValueIndex = table.Column<int>(type: "int", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CanRemoveOrDisable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegexInfors", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "RequestLogs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Controller = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    QueryURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueryString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    User = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RequestTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLogs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    InvoiceFormRecognizerModel = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: true),
                    PIOCFormRecognizerModel = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: true),
                    SAPID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PIC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CanRemoveOrDisable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormRecognizerLogs_IsActive",
                table: "FormRecognizerLogs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FormRecognizerLogs_SupplierID_ModelID_FileName_Type",
                table: "FormRecognizerLogs",
                columns: new[] { "SupplierID", "ModelID", "FileName", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegexInfors_IsActive",
                table: "RegexInfors",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_IsActive",
                table: "Suppliers",
                column: "IsActive");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormRecognizerLogs");

            migrationBuilder.DropTable(
                name: "RegexInfors");

            migrationBuilder.DropTable(
                name: "RequestLogs");

            migrationBuilder.DropTable(
                name: "Suppliers");
        }
    }
}
