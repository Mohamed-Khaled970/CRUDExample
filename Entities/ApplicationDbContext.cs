using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace Entities
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Person> Persons { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Country>().ToTable("Countries");
            modelBuilder.Entity<Person>().ToTable("Persons");

            string countriesJson = File.ReadAllText("Countries.json");
            List<Country> countries = JsonSerializer.Deserialize<List<Country>>(countriesJson)!;

            foreach (Country country in countries)
                modelBuilder.Entity<Country>().HasData(country);

            string personsJson = File.ReadAllText("Persons.json");
            List<Person> persons = JsonSerializer.Deserialize<List<Person>>(personsJson)!;

            foreach (Person person in persons)
                modelBuilder.Entity<Person>().HasData(person);

            modelBuilder.Entity<Person>()
                             .HasOne(p => p.Country)
                             .WithMany(c => c.Persons)
                             .HasForeignKey(p => p.CountryId)
                             .OnDelete(DeleteBehavior.Restrict);
        }

        /// <summary>
        /// A Method That use Stored Procedure To Retrive All Persons From DataBase
        /// </summary>
        /// <returns>IEnumerable of Person</returns>
        public IEnumerable<Person> sp_GetAllPersons()
        {
            return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList() ;
        }

        public int sp_InsertPerson(Person person)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Id" , person.Id) ,
                new SqlParameter("@Name" , person.Name) ,
                new SqlParameter("@DateOfBirth" , person.DateOfBirth) ,
                new SqlParameter("@CountryId" , person.CountryId) ,
                new SqlParameter("@PhoneNumber" , person.PhoneNumber) ,
                new SqlParameter("@Address" , person.Address)
            };

            return Database.ExecuteSqlRaw("EXECUTE [dbo].[InsertPerson] @Id ,@Name ,@DateOfBirth ,@CountryId , @PhoneNumber ,@Address ",
                parameters);
        }
    }
}
