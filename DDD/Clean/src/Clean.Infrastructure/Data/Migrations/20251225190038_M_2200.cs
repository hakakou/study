using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clean.Infrastructure.Data.Migrations;

  /// <inheritdoc />
  public partial class M_2200 : Migration
  {
      /// <inheritdoc />
      protected override void Up(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.CreateTable(
              name: "IssueLabel",
              columns: table => new
              {
                  Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                  IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                  Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_IssueLabel", x => x.Id);
              });

          migrationBuilder.CreateTable(
              name: "Issues",
              columns: table => new
              {
                  Id = table.Column<int>(type: "int", nullable: false),
                  GitRepositoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                  Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                  Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                  CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                  AssignedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_Issues", x => x.Id);
              });
      }

      /// <inheritdoc />
      protected override void Down(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.DropTable(
              name: "IssueLabel");

          migrationBuilder.DropTable(
              name: "Issues");
      }
  }
