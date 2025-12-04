using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTO
{
    public class CountryResponse
    {
        public Guid CountryId { get; set; }
        public string? CountryName { get; set; }

        public override bool Equals(object? obj)
        {
           if(obj is null)
                return false;
           
           if(obj.GetType() != typeof(CountryResponse))
                return false;

            CountryResponse country_from_obj = (CountryResponse)obj;

            return CountryId == country_from_obj.CountryId &&
                 CountryName == country_from_obj.CountryName;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }

    public static class CountryExtension
    {
        public static CountryResponse ToCountryResponse
            (this Country country)
        {
            return new CountryResponse
            {
                CountryId = country.CountryId,
                CountryName = country.CountryName
            };
        }
    }
}
