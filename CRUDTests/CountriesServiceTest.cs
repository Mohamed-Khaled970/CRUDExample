using Entities;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDTests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countriesService;

        public CountriesServiceTest()
        {
            var countriesInitialData = new List<Country>() { }; 

            DbContextMock<ApplicationDbContext> dbContextMock = 
                new DbContextMock<ApplicationDbContext>
                (new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            var dbContext = dbContextMock.Object;

            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);

            _countriesService = new CountriesService(dbContext);
        }

        #region AddCountry Test Cases:

        [Fact]
        public async Task AddCountry_CountryAddRequestIsNull()
        {
            CountryAddRequest? request = null;

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _countriesService.AddCountryAsync(request);
            });
        }

        [Fact]
        public async Task AddCountry_CountryNameIsNull()
        {
            CountryAddRequest request = new CountryAddRequest() { CountryName = null };

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _countriesService.AddCountryAsync(request);
            });
        }

        [Fact]
        public async Task AddCountry_DuplicateCountryName()
        {
            CountryAddRequest request1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest request2 = new CountryAddRequest() { CountryName = "USA" };

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _countriesService.AddCountryAsync(request1);
                await _countriesService.AddCountryAsync(request2);
            });
        }

        [Fact]
        public async Task AddCountry_ProperCountryDetails()
        {
            CountryAddRequest request = new CountryAddRequest() { CountryName = "Tunisia" };

            CountryResponse response = await _countriesService.AddCountryAsync(request);

            List<CountryResponse> countryResponses = await _countriesService.GetAllCountriesAsync();

            Assert.True(response.CountryId != Guid.Empty);
            Assert.Contains(response, countryResponses);
        }

        #endregion

        #region GetAllCountries Test Cases:

        [Fact]
        public async Task GetAllCountries_EmptyList()
        {
            List<CountryResponse> countries = await _countriesService.GetAllCountriesAsync();
            Assert.Empty(countries);
        }

        [Fact]
        public async Task GetAllCountries_AddFewCountries()
        {
            List<CountryAddRequest> countryAddRequests = new List<CountryAddRequest>
            {
                new CountryAddRequest() { CountryName = "USA"},
                new CountryAddRequest() { CountryName = "UK"}
            };

            List<CountryResponse> countries_from_Add = new List<CountryResponse>();

            foreach (var country in countryAddRequests)
            {
                countries_from_Add.Add(await _countriesService.AddCountryAsync(country));
            }

            List<CountryResponse> countries_from_GetAll = await _countriesService.GetAllCountriesAsync();

            foreach (var country in countries_from_Add)
            {
                Assert.Contains(country, countries_from_GetAll);
            }
        }

        #endregion

        #region GetCountryById Test Cases:

        [Fact]
        public async Task GetCountryById_IdIsNull()
        {
            CountryResponse? countryResponse = await _countriesService.GetCountryByIdAsync(null);
            Assert.Null(countryResponse);
        }

        [Fact]
        public async Task GetCountryById_SupplyValidCountryId()
        {
            CountryAddRequest request = new CountryAddRequest() { CountryName = "Japan" };
            CountryResponse countryFromAdd = await _countriesService.AddCountryAsync(request);

            Guid countryId = countryFromAdd.CountryId;

            CountryResponse? countryFromGet = await _countriesService.GetCountryByIdAsync(countryId);

            Assert.Equal(countryFromAdd, countryFromGet);
        }

        #endregion
    }
}
