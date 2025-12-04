using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class GetAllPersons_StoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sp_GetAllPersons = @"

             CREATE PROCEDURE [dbo].[GetAllPersons]
             AS BEGIN
                SELECT * FROM [dbo].[Persons]
             END
           ";

            migrationBuilder.Sql(sp_GetAllPersons);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string DropGetAllPersons = @"

              DROP PROCEDURE [dbo].[GetAllPersons]
           ";

            migrationBuilder.Sql(DropGetAllPersons);
        }
    }
}
