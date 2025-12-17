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
using RepositoryContracts;
using Moq;
using FluentAssertions.Execution;
namespace CRUDTests
{
    public class PersonsServiceTest
    {
        private readonly IPersonsService _personsService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly Mock<IPersonsRepository> _PersonRepositoryMocked;
        private readonly IPersonsRepository _PersonRepository;

        private readonly IFixture _fixture;
        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {

            _PersonRepositoryMocked = new Mock<IPersonsRepository>();
            _PersonRepository = _PersonRepositoryMocked.Object;
            _personsService = new PersonsService(_PersonRepository);             
            _testOutputHelper = testOutputHelper;
            _fixture = new Fixture();
        }

        #region AddPerson Service Test Cases :

        [Fact]
        public async Task AddPerson_AddPersonRequestIsNull_ReturnNull()
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
        public async Task AddPerson_FullInformationOfPerson_ShouldBeSuccessed()
        {
            AddPersonRequest addPersonRequest = 
                _fixture.Create<AddPersonRequest>();

            Person person = addPersonRequest.ToPerson();
            PersonResponse personResponse_excpected = person.ToPersonResponse();

            _PersonRepositoryMocked.Setup
                (temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            PersonResponse person_response_from_add = await _personsService.AddPerson(addPersonRequest);

            personResponse_excpected.Id = person_response_from_add.Id;

            person_response_from_add.Id.Should().NotBe(Guid.Empty);
            person_response_from_add.Id.Should().Be(personResponse_excpected.Id);
        }

        #endregion

        #region GetPerson By Id Test Cases:

        [Fact]
        public async Task GetPersonById_PersonIdIsNull_ReturnNull()
        {
            Guid? id = null;

            PersonResponse? response = await _personsService.GetPersonById(id);

            response.Should().BeNull();
        }

        [Fact]
        public async Task GetPersonById_PersonIdIsValid_ShouldBeSuccess()
        {
           Person person = _fixture.Build<Person>()
                .With(t => t.Country , null as Country)
                .Create();
                ;

          PersonResponse personResponse_expected = person.ToPersonResponse();

            _PersonRepositoryMocked.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            PersonResponse? personResponse_fromGet = await _personsService.GetPersonById(person.Id);

            personResponse_fromGet.Should().Be(personResponse_expected);
        }

        #endregion

        #region GetAllPersons Test Cases:

        [Fact]
        public async Task GetAllPersons_ListIsEmpty_ReturnNull()
        {
            _PersonRepositoryMocked.Setup(temp => temp.GetAllPersons())
                                    .ReturnsAsync(new List<Person>());

            List<PersonResponse>? persons_from_get = await _personsService.GetAllPersons();
            persons_from_get.Should().BeNull();
        }

        [Fact]
        public async Task GetAllPersons_AddFewPersons_ShouldBeSuccess()
        {
            var persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With( t => t.Country , null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With( t => t.Country , null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With( t => t.Country , null as Country)
                .Create()
            };

            _PersonRepositoryMocked.Setup (temp => temp.GetAllPersons())
                .ReturnsAsync(persons);

            var personResponses_expected = persons.Select(x => x.ToPersonResponse());

            List<PersonResponse> personResponses_from_GetAll = (await _personsService.GetAllPersons())!.ToList();

            personResponses_from_GetAll.Should().BeEquivalentTo(personResponses_expected);
        }

        #endregion

        #region Search Person Test Cases:

        [Fact]
        public async Task Search_SearchTermIsEmpty()
        {
            var persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With( t => t.Country , null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With( t => t.Country , null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With( t => t.Country , null as Country)
                .Create()
            };

            _PersonRepositoryMocked.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(persons);

            var personResponses = persons.Select(x => x.ToPersonResponse());

            string search_Property = "Name";
            string search_Term = "";

            List<PersonResponse> ExpectedPersonResponses =
               personResponses
                        .Where(p => p.Name.Contains(search_Term, StringComparison.OrdinalIgnoreCase))
                        .ToList();

            List<PersonResponse> ActualPersonResponses = await _personsService.Search(search_Property, search_Term);

            ActualPersonResponses.Should().BeEquivalentTo(ExpectedPersonResponses);
        }

        #endregion

        #region Sort Person Test Cases :

        [Fact]
        public async Task GetSortedPersons_SortPropertyIsNullOrEmpty_ToBeSuccessfull()
        {
            var persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With( t => t.Country , null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With( t => t.Country , null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With( t => t.Country , null as Country)
                .Create()
            };

            _PersonRepositoryMocked.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(persons);

            var personResponses = persons.Select(x => x.ToPersonResponse()).ToList();

            List<PersonResponse> sortedPersons = _personsService.GetSortedPersons(personResponses, "", OrderOptions.ASC);

            for (int i = 0; i < sortedPersons.Count; i++)
            {
                sortedPersons[i].Should().BeEquivalentTo(personResponses[i]);
            }
        }

        #endregion


        #region Update Person Test Cases : 

        [Fact]
        public async Task UpdatePerson_PersonIdIsNotFound_ThrowsArgumentException()
        {          

            UpdatePersonRequest updatePersonRequest = _fixture.Build<UpdatePersonRequest>()
                 .With(temp => temp.PersonID, new Guid())
                 .Create();

            Func<Task> actual = async () =>
            {
                await _personsService.UpdatePerson(updatePersonRequest);
            };

            await actual.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task UpdatePerson_PersonIdIsValid_ToBeSuccessfull()
        {
            Person personAfterUpdate = _fixture.Build<Person>()
                                     .With(t => t.Country, null as Country)
                                     .Create();

            PersonResponse ExpectedPersonResponse = personAfterUpdate.ToPersonResponse();

            Person personBeforeUpdate = _fixture.Build<Person>()
                                     .With(t => t.Country, null as Country)
                                     .With(temp => temp.Id, personAfterUpdate.Id)
                                     .Create();



            UpdatePersonRequest updatePersonRequest = _fixture.Build<UpdatePersonRequest>()
                  .With(temp => temp.PersonID, personAfterUpdate.Id)
                  .With(temp => temp.DateOfBirth, personAfterUpdate.DateOfBirth)
                  .With(temp => temp.Name, personAfterUpdate.Name)
                  .With(temp => temp.PhoneNumber, personAfterUpdate.PhoneNumber)
                  .With(temp => temp.Address, personAfterUpdate.Address)
                  .With(temp => temp.CountryId, personAfterUpdate.CountryId)
                  .Create();

            _PersonRepositoryMocked.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(personBeforeUpdate);

            _PersonRepositoryMocked.Setup(temp => temp.UpdatePerson(It.IsAny<Person>()))
               .ReturnsAsync(personAfterUpdate);


            PersonResponse ActualPersonResponse = await _personsService.UpdatePerson(updatePersonRequest);

            ActualPersonResponse.Should().BeEquivalentTo(ExpectedPersonResponse);
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
            Person person = _fixture.Build<Person>()
                                     .With(t => t.Country, null as Country)
                                     .Create();

            _PersonRepositoryMocked.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(person);
            

            bool isDeleted = await _personsService.DeletePerson(person.Id);
            isDeleted.Should().BeTrue();
        }

        #endregion
    }
}
