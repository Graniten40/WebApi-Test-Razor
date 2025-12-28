using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbContext.Migrations.mysqlDbContext
{
    /// <inheritdoc />
    public partial class miInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "supusr");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Artists",
                schema: "supusr",
                columns: table => new
                {
                    ArtistId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FirstName = table.Column<string>(type: "varchar(200)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(200)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BirthDay = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Seeded = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.ArtistId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MusicGroups",
                schema: "supusr",
                columns: table => new
                {
                    MusicGroupId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    strGenre = table.Column<string>(type: "varchar(200)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstablishedYear = table.Column<int>(type: "int", nullable: false),
                    Genre = table.Column<int>(type: "int", nullable: false),
                    Seeded = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicGroups", x => x.MusicGroupId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserName = table.Column<string>(type: "varchar(200)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(200)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserRole = table.Column<string>(type: "varchar(200)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(200)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Albums",
                schema: "supusr",
                columns: table => new
                {
                    AlbumId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MusicGroupDbMMusicGroupId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ReleaseYear = table.Column<int>(type: "int", nullable: false),
                    CopiesSold = table.Column<long>(type: "bigint", nullable: false),
                    Seeded = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.AlbumId);
                    table.ForeignKey(
                        name: "FK_Albums_MusicGroups_MusicGroupDbMMusicGroupId",
                        column: x => x.MusicGroupDbMMusicGroupId,
                        principalSchema: "supusr",
                        principalTable: "MusicGroups",
                        principalColumn: "MusicGroupId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArtistDbMMusicGroupDbM",
                schema: "supusr",
                columns: table => new
                {
                    ArtistsDbMArtistId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MusicGroupsDbMMusicGroupId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistDbMMusicGroupDbM", x => new { x.ArtistsDbMArtistId, x.MusicGroupsDbMMusicGroupId });
                    table.ForeignKey(
                        name: "FK_ArtistDbMMusicGroupDbM_Artists_ArtistsDbMArtistId",
                        column: x => x.ArtistsDbMArtistId,
                        principalSchema: "supusr",
                        principalTable: "Artists",
                        principalColumn: "ArtistId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistDbMMusicGroupDbM_MusicGroups_MusicGroupsDbMMusicGroupId",
                        column: x => x.MusicGroupsDbMMusicGroupId,
                        principalSchema: "supusr",
                        principalTable: "MusicGroups",
                        principalColumn: "MusicGroupId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_MusicGroupDbMMusicGroupId",
                schema: "supusr",
                table: "Albums",
                column: "MusicGroupDbMMusicGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistDbMMusicGroupDbM_MusicGroupsDbMMusicGroupId",
                schema: "supusr",
                table: "ArtistDbMMusicGroupDbM",
                column: "MusicGroupsDbMMusicGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Albums",
                schema: "supusr");

            migrationBuilder.DropTable(
                name: "ArtistDbMMusicGroupDbM",
                schema: "supusr");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Artists",
                schema: "supusr");

            migrationBuilder.DropTable(
                name: "MusicGroups",
                schema: "supusr");
        }
    }
}
