using AutoFixture;
using Azure.Core;
using Entities;
using EntityFrameworkCoreMock;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CRUDTests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countriesService;
        private readonly ICountriesRepository _countriesRepository;
        private readonly Mock<ICountriesRepository> _countryRepositoryMocked;
        private readonly IFixture _fixture;



        public CountriesServiceTest()
        {
            _countryRepositoryMocked = new Mock<ICountriesRepository>();
            _countriesRepository = _countryRepositoryMocked.Object;
            _countriesService = new CountriesService(_countriesRepository);
            _fixture = new Fixture();

        }

        #region AddCountry Test Cases:

        [Fact]
        public async Task AddCountry_CountryAddRequestIsNull()
        {
            CountryAddRequest? request = null;

            Func<Task> actual = async () =>
            {
                await _countriesService.AddCountryAsync(request);
            };

           await actual.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task AddCountry_CountryNameIsNull()
        {
            CountryAddRequest request = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName , null as string)
                .Create();

            Func<Task> actual = async () =>
            {
                await _countriesService.AddCountryAsync(request);
            };

            await actual.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task AddCountry_DuplicateCountryName()
        {
            CountryAddRequest request1 = _fixture.Create<CountryAddRequest>();

            CountryAddRequest request2 = _fixture.Build<CountryAddRequest>()
                     .With(temp => temp.CountryName, request1.CountryName)
                     .Create();

            var existingCountry = request1.ToCountry();

             _countryRepositoryMocked.Setup(temp => temp.GetCountryByCountryName(It.IsAny<string>()))
                .ReturnsAsync(existingCountry);

            Func<Task> actual = async () =>
            {
                await _countriesService.AddCountryAsync(request2);
            };

            await actual.Should().ThrowAsync<ArgumentException>();

        }

        [Fact]
        public async Task AddCountry_ProperCountryDetails()
        {
            CountryAddRequest request = _fixture.Create<CountryAddRequest>();

            Country country = _fixture.Build<Country>()
                .With(temp => temp.CountryName , request.CountryName)
                .Create();

            CountryResponse expectedCountryResponse = country.ToCountryResponse();

            _countryRepositoryMocked.Setup(temp => temp.AddCountry(It.IsAny<Country>()))
                    .ReturnsAsync(country);

            CountryResponse actualCountryResponse = await _countriesService.AddCountryAsync(request);

            expectedCountryResponse.CountryId = actualCountryResponse.CountryId;

            actualCountryResponse.CountryId.Should().NotBe(Guid.Empty);
            actualCountryResponse.Should().BeEquivalentTo(expectedCountryResponse);
 
        }

        #endregion

        #region GetAllCountries Test Cases:

        [Fact]
        public async Task GetAllCountries_EmptyList()
        {
            _countryRepositoryMocked.Setup(temp => temp.GetAllCountries())
         .ReturnsAsync(new List<Country>());

            var AllCountries = await _countriesService.GetAllCountriesAsync();

            AllCountries.Should().BeNull();

        }

        [Fact]
        public async Task GetAllCountries_AddFewCountries()
        {
            List<Country> countries = new List<Country>() 
            {
            _fixture.Create<Country>(),
            _fixture.Create<Country>(),
            _fixture.Create<Country>(),
            };

            _countryRepositoryMocked.Setup(temp => temp.GetAllCountries())
                    .ReturnsAsync(countries);


            List<CountryResponse> expectedCountriesResponse = countries
                .Select(c => c.ToCountryResponse()).ToList();

            List<CountryResponse>? actualCountriesResponse = await _countriesService.GetAllCountriesAsync();

            actualCountriesResponse.Should().BeEquivalentTo(expectedCountriesResponse); 
        }

        #endregion

        #region GetCountryById Test Cases:

        [Fact]
        public async Task GetCountryById_IdIsNull()
        {
            CountryResponse? countryResponse = await _countriesService.GetCountryByIdAsync(null);
            countryResponse.Should().BeNull();  
        }

        [Fact]
        public async Task GetCountryById_SupplyValidCountryId()
        {
            CountryAddRequest request = _fixture.Create<CountryAddRequest>();

            Country country = _fixture.Build<Country>()
                .With(temp => temp.CountryName , request.CountryName)
                .Create(); 

            CountryResponse expectedCountryResoponse = country.ToCountryResponse();

            _countryRepositoryMocked.Setup(temp => temp.GetCountryByCountryID(It.IsAny<Guid>()))
                    .ReturnsAsync(country);

            CountryResponse? actualCountryResoponse = await _countriesService.GetCountryByIdAsync(Guid.NewGuid());

            actualCountryResoponse!.CountryId.Should().NotBe(Guid.Empty);
            actualCountryResoponse!.Should().BeEquivalentTo(expectedCountryResoponse);
        }

        #endregion
    }
}
