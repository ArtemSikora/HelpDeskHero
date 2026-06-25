using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDeskHero.Api.Migrations
{
    /// <inheritdoc />
    public partial class UseGuidConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "RowVersion",
                table: "Tickets",
                type: "TEXT",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldRowVersion: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Tickets",
                type: "BLOB",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldRowVersion: true);
        }
    }
}
