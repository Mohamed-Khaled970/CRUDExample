using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class InsertPerson_StoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sp_InsertPerson = @"

             CREATE PROCEDURE [dbo].[InsertPerson]
             ( @Id uniqueidentifier , @Name nvarchar(40) , @DateOfBirth datetime2(7) ,
               @CountryId uniqueidentifier , @PhoneNumber nvarchar(200) , @Address nvarchar(200) )

             AS BEGIN
                INSERT INTO [dbo].[Persons] (Id , Name , DateOfBirth , CountryId ,
                PhoneNumber , Address)
                VALUES (@Id ,@Name ,@DateOfBirth ,@CountryId , @PhoneNumber ,@Address)
             END
           ";

            migrationBuilder.Sql(sp_InsertPerson);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string DropInsertPerson = @"

              DROP PROCEDURE [dbo].[InsertPerson]
           ";

            migrationBuilder.Sql(DropInsertPerson);
        }
    }
}
