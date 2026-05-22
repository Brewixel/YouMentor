using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Sessions",
                table: "Sessions");

            migrationBuilder.RenameTable(
                name: "Sessions",
                newName: "sessions");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "sessions",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "sessions",
                newName: "duration");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "sessions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "sessions",
                newName: "student_id");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "sessions",
                newName: "start_time");

            migrationBuilder.RenameColumn(
                name: "MentorId",
                table: "sessions",
                newName: "mentor_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sessions",
                table: "sessions",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_sessions",
                table: "sessions");

            migrationBuilder.RenameTable(
                name: "sessions",
                newName: "Sessions");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Sessions",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "duration",
                table: "Sessions",
                newName: "Duration");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Sessions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "student_id",
                table: "Sessions",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "start_time",
                table: "Sessions",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "mentor_id",
                table: "Sessions",
                newName: "MentorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sessions",
                table: "Sessions",
                column: "Id");
        }
    }
}
