using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeSessionStatusValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
	        migrationBuilder.Sql(
		        """
		        DO $$
		        BEGIN
		            IF EXISTS (
		                SELECT 1
		                FROM sessions
		                WHERE status = 2
		            ) THEN
		                RAISE EXCEPTION
		                    'Cannot normalize statuses: legacy Completed sessions exist.';
		            END IF;

		            UPDATE sessions
		            SET status = 2
		            WHERE status = 3;
		        END
		        $$;
		        """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
	        migrationBuilder.Sql(
				"""
				UPDATE sessions
					SET status = 3
					WHERE status = 2;
				""");
        }
    }
}
