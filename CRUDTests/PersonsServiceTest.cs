using Entities;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Bson;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using AutoFixture;
using FluentAssertions;
namespace CRUDTests
{
    public class PersonsServiceTest
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;
        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {

            var countriesInitialData = new List<Country>() { };
            var personsInitialData = new List<Person>() { };


            DbContextMock<ApplicationDbContext> dbContextMock =
                new DbContextMock<ApplicationDbContext>
                (new DbContextOptionsBuilder<ApplicationDbContext>().Options);


            var dbContext = dbContextMock.Object;
            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);
            dbContextMock.CreateDbSetMock(x => x.Persons, personsInitialData);

            _countriesService = new CountriesService(dbContext);

            _personsService = new PersonsService(_countriesService , dbContext);     
            

            _testOutputHelper = testOutputHelper;
            _fixture = new Fixture();
        }

        #region AddPerson Service Test Cases :

        [Fact]
        public async Task AddPerson_AddPersonRequestIsNull()
        {
            AddPersonRequest? addPersonRequest = null;

            Func<Task> actual = async () =>
            {
                await _personsService.AddPerson(addPersonRequest!);
            };

            await actual.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task AddPerson_PersonNameIsNull()
        {
            AddPersonRequest addPersonRequest = _fixture.Build<AddPersonRequest>()
                .With(temp => temp.Name , null as string)
                .Create();

            Func<Task> actual = async () =>
            {
                await _personsService.AddPerson(addPersonRequest!);
            };

            await actual.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task AddPerson_AddPersonRequestIsValid()
        {
            AddPersonRequest addPersonRequest = 
                _fixture.Create<AddPersonRequest>();

            PersonResponse response = await _personsService.AddPerson(addPersonRequest);

            List<PersonResponse> responseFromGetAllPersons = (await _personsService.GetAllPersons())!.ToList();

            PersonResponse personResponseInGetAllPersonsList =
                responseFromGetAllPersons.FirstOrDefault(x => x.Id == response.Id)!;

            response.Id.Should().NotBe(Guid.Empty);
            personResponseInGetAllPersonsList.Should().Be(response);
        }

        #endregion

        #region GetPerson By Id Test Cases:

        [Fact]
        public async Task GetPersonById_PersonIdIsNull()
        {
            Guid? id = null;

            PersonResponse? response = await _personsService.GetPersonById(id);

            response.Should().BeNull();
        }

        [Fact]
        public async Task GetPersonById_PersonIdIsValid()
        {
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse = await _countriesService.AddCountryAsync(countryAddRequest);

            AddPersonRequest personRequest = _fixture.Build<AddPersonRequest>()
                .With(temp => temp.CountryId , countryResponse.CountryId)
                .Create();

            PersonResponse personResponse_fromAdd = await _personsService.AddPerson(personRequest);
            personResponse_fromAdd.Country = countryResponse.CountryName;

            PersonResponse? personResponse_fromGet = await _personsService.GetPersonById(personResponse_fromAdd.Id);
            personResponse_fromGet!.Country = countryResponse.CountryName;

            personResponse_fromAdd.Should().Be(personResponse_fromGet);
        }

        #endregion

        #region GetAllPersons Test Cases:

        [Fact]
        public async Task GetAllPersons_ListIsEmpty()
        {
            var persons = await _personsService.GetAllPersons();
            persons.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllPersons_AddFewPersons()
        {
            CountryAddRequest countryAddRequest1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest countryAddRequest2 = _fixture.Create<CountryAddRequest>();


            CountryResponse countryResponse1 = await _countriesService.AddCountryAsync(countryAddRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountryAsync(countryAddRequest2);

            AddPersonRequest personRequest1 = _fixture.Build<AddPersonRequest>()
                .With(temp => temp.CountryId, countryResponse1.CountryId)
                .Create();
            AddPersonRequest personRequest2 = _fixture.Build<AddPersonRequest>()
              .With(temp => temp.CountryId, countryResponse2.CountryId)
              .Create();
            AddPersonRequest personRequest3 = _fixture.Build<AddPersonRequest>()
              .With(temp => temp.CountryId, countryResponse2.CountryId)
              .Create();


            List<PersonResponse> personResponses_from_Add = new List<PersonResponse>()
            {
                await _personsService.AddPerson(personRequest1),
                await _personsService.AddPerson(personRequest2),
                await _personsService.AddPerson(personRequest3)
            };

            List<PersonResponse> personResponses_from_GetAll = (await _personsService.GetAllPersons())!.ToList();

            foreach (var personResponse_in_Add in personResponses_from_Add)
            {
                personResponses_from_GetAll.Should().ContainEquivalentOf(personResponse_in_Add);
            }
        }

        #endregion

        #region Search Person Test Cases:

        [Fact]
        public async Task Search_SearchTermIsEmpty()
        {
            CountryAddRequest countryAddRequest1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest countryAddRequest2 = _fixture.Create<CountryAddRequest>();


            CountryResponse countryResponse1 = await _countriesService.AddCountryAsync(countryAddRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountryAsync(countryAddRequest2);

            AddPersonRequest personRequest1 = _fixture.Build<AddPersonRequest>()
                .With(temp => temp.CountryId, countryResponse1.CountryId)
                .Create();
            AddPersonRequest personRequest2 = _fixture.Build<AddPersonRequest>()
              .With(temp => temp.CountryId, countryResponse2.CountryId)
              .Create();
            AddPersonRequest personRequest3 = _fixture.Build<AddPersonRequest>()
              .With(temp => temp.CountryId, countryResponse2.CountryId)
              .Create();

            List<PersonResponse> addedPersons = new List<PersonResponse>()
            {
                await _personsService.AddPerson(personRequest1),
                await _personsService.AddPerson(personRequest2),
                await _personsService.AddPerson(personRequest3)
            };

            string search_Property = "Name";
            string search_Term = "";

            List<PersonResponse> ExpectedPersonResponses =
                (await _personsService.GetAllPersons())!
                .Where(p => p.Name.Contains(search_Term, StringComparison.OrdinalIgnoreCase))
                .ToList();

            List<PersonResponse> ActualPersonResponses = await _personsService.Search(search_Property, search_Term);

            foreach(var  personResponse in ActualPersonResponses)
            {
                ExpectedPersonResponses.Should().ContainEquivalentOf(personResponse);
            }
        }

        #endregion

        #region Sort Person Test Cases :

        [Fact]
        public async Task GetSortedPersons_SortPropertyIsNullOrEmpty()
        {
            CountryAddRequest countryAddRequest1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest countryAddRequest2 = _fixture.Create<CountryAddRequest>();


            CountryResponse countryResponse1 = await _countriesService.AddCountryAsync(countryAddRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountryAsync(countryAddRequest2);

            AddPersonRequest personRequest1 = _fixture.Build<AddPersonRequest>()
              .With(temp => temp.CountryId, countryResponse1.CountryId)
              .Create();
            AddPersonRequest personRequest2 = _fixture.Build<AddPersonRequest>()
              .With(temp => temp.CountryId, countryResponse2.CountryId)
              .Create();
            AddPersonRequest personRequest3 = _fixture.Build<AddPersonRequest>()
              .With(temp => temp.CountryId, countryResponse2.CountryId)
              .Create();

            List<PersonResponse> allPersons = new List<PersonResponse>()
            {
                await _personsService.AddPerson(personRequest1),
                await _personsService.AddPerson(personRequest2),
                await _personsService.AddPerson(personRequest3)
            };

            List<PersonResponse> sortedPersons = _personsService.GetSortedPersons(allPersons, "", OrderOptions.ASC);

            for (int i = 0; i < allPersons.Count; i++)
            {
                allPersons[i].Should().BeEquivalentTo(sortedPersons[i]);
            }
        }

        #endregion


        #region Update Person Test Cases : 

        [Fact]
        public async Task UpdatePerson_PersonIdIsNotFound()
        {
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
            CountryResponse countryResponse = await _countriesService.AddCountryAsync(countryAddRequest);


            AddPersonRequest personRequest = _fixture.Build<AddPersonRequest>()
                 .With(temp => temp.CountryId, countryResponse.CountryId)
                 .Create();

            PersonResponse personResponse = await _personsService.AddPerson(personRequest);

            UpdatePersonRequest updatePersonRequest = _fixture.Build<UpdatePersonRequest>()
                 .With(temp => temp.CountryId, countryResponse.CountryId)
                 .With(temp => temp.PersonID , new Guid())
                 .Create();

            Func<Task> actual = async () =>
            {
                await _personsService.UpdatePerson(updatePersonRequest);
            };
            
            await actual.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task UpdatePerson_PersonIdIsValid()
        {
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
            CountryResponse countryResponse = await _countriesService.AddCountryAsync(countryAddRequest);


            AddPersonRequest personRequest = _fixture.Build<AddPersonRequest>()
                 .With(temp => temp.CountryId, countryResponse.CountryId)
                 .Create();

            PersonResponse personResponse = await _personsService.AddPerson(personRequest);

            UpdatePersonRequest updatePersonRequest = _fixture.Build<UpdatePersonRequest>()
                  .With(temp => temp.CountryId, countryResponse.CountryId)
                  .With(temp => temp.PersonID, personResponse.Id)
                  .Create();

            PersonResponse updatedPerson = await _personsService.UpdatePerson(updatePersonRequest);

            PersonResponse? personFromGet = await _personsService.GetPersonById(personResponse.Id);

            personFromGet.Should().BeEquivalentTo(updatedPerson);
        }

        #endregion

        #region Delete Person Test Cases:

        [Fact]
        public async Task DeletePerson_PersonIdIsNotFound()
        {
            bool isDeleted = await _personsService.DeletePerson(Guid.NewGuid());
            isDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task DeletePerson_PersonIdIsFound()
        {
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
            CountryResponse countryResponse = await _countriesService.AddCountryAsync(countryAddRequest);


            AddPersonRequest personRequest = _fixture.Build<AddPersonRequest>()
                 .With(temp => temp.CountryId, countryResponse.CountryId)
                 .Create();

            PersonResponse personResponse = await _personsService.AddPerson(personRequest);

            bool isDeleted = await _personsService.DeletePerson(personResponse.Id);
            isDeleted.Should().BeTrue();


            // Optional: تأكد إنه تم الحذف بالفعل
            PersonResponse? deletedPerson = await _personsService.GetPersonById(personResponse.Id);
            deletedPerson.Should().BeNull();
        }

        #endregion
    }
}
