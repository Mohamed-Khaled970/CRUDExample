    using Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace ServiceContracts.DTO
    {
        public class PersonResponse
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;

            public DateTime DateOfBirth { get; set; }

            public Guid CountryId { get; set; }

            public string? Country {  get; set; } = string.Empty;

            public string PhoneNumber { get; set; } = string.Empty;

            public string Address { get; set; } = string.Empty;

            public override bool Equals(object? obj)
            {
                if(obj is null)
                    return false;

                if(obj.GetType() != typeof(PersonResponse))
                    return false;

                PersonResponse response = (PersonResponse)obj;

                return Id == response.Id && Name == response.Name &&
                    DateOfBirth == response.DateOfBirth &&
                    CountryId == response.CountryId &&
                    Country == response.Country &&
                    PhoneNumber == response.PhoneNumber &&
                    Address == response.Address;
            }

            public override string ToString()
            {
                return $"Id : {Id}\n Name: {Name}\n PhoneNumber: {PhoneNumber}\n CountryId: {CountryId}\n Country: {Country}\n Address: {Address}\n\n\n\n";
            }
        }

        public static class PersonExtensions
        {
            public static PersonResponse ToPersonResponse(this Person person)
            {
            if (person == null)
                throw new ArgumentNullException(nameof(person));

            return new PersonResponse()
            {
                Id = person.Id,
                Name = person.Name ?? string.Empty,
                DateOfBirth = person.DateOfBirth,
                CountryId = person.Country?.CountryId ?? Guid.Empty, // لو Country null يبقى Guid.Empty
                Country = person.Country?.CountryName ?? string.Empty, // لو Country null يبقى empty
                PhoneNumber = person.PhoneNumber ?? string.Empty,
                Address = person.Address ?? string.Empty,
            };
        }
        }
    }
