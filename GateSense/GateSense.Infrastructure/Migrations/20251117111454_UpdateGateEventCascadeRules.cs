using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateSense.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGateEventCascadeRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GateEvents_AccessKeys_AccessKeyId",
                table: "GateEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_GateEvents_AspNetUsers_InitiatorUserId",
                table: "GateEvents");

            migrationBuilder.AddForeignKey(
                name: "FK_GateEvents_AccessKeys_AccessKeyId",
                table: "GateEvents",
                column: "AccessKeyId",
                principalTable: "AccessKeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GateEvents_AspNetUsers_InitiatorUserId",
                table: "GateEvents",
                column: "InitiatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GateEvents_AccessKeys_AccessKeyId",
                table: "GateEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_GateEvents_AspNetUsers_InitiatorUserId",
                table: "GateEvents");

            migrationBuilder.AddForeignKey(
                name: "FK_GateEvents_AccessKeys_AccessKeyId",
                table: "GateEvents",
                column: "AccessKeyId",
                principalTable: "AccessKeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GateEvents_AspNetUsers_InitiatorUserId",
                table: "GateEvents",
                column: "InitiatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
