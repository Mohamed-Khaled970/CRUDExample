using AutoFixture;
using CRUDExample.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDTests
{
    public class PersonsControllerTest
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;

        private readonly Mock<ICountriesService> _countriesServiceMock;
        private readonly Mock<IPersonsService> _personsServiceMock;

        private readonly Fixture _fixture;

        public PersonsControllerTest()
        {
            _fixture = new Fixture();

            _countriesServiceMock = new Mock<ICountriesService>();
            _personsServiceMock = new Mock<IPersonsService>();

            _countriesService = _countriesServiceMock.Object;
            _personsService = _personsServiceMock.Object;
        }

        #region Index Unit Testing

        [Fact]
        public async Task Index_ShouldReturnIndexViewWithPersonsList()
        {
            List<PersonResponse> persons_response_list = _fixture.Create<List<PersonResponse>>();
            PersonController personsController = new PersonController(_personsService, _countriesService);

            //Mocking Search Method
            _personsServiceMock.Setup(x => x.Search(It.IsAny<string>() , It.IsAny<string>()))
                .ReturnsAsync(persons_response_list);

            _personsServiceMock.Setup(x => x.GetSortedPersons(It.IsAny<List<PersonResponse>>() , It.IsAny<string>(), It.IsAny<OrderOptions>()))
                .Returns(persons_response_list);
            
            IActionResult result = await personsController.Index(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<OrderOptions>());

            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            viewResult.ViewData.Model.Should().BeAssignableTo<IEnumerable<PersonResponse>>();
            viewResult.ViewData.Model.Should().Be(persons_response_list);

        }


        #endregion

        #region Add View Unit Testing
        [Fact]
        public async void Add_IfModelErrors_ToReturnAddView()
        {
            AddPersonRequest addPersonRequest = _fixture.Create<AddPersonRequest>();
            PersonResponse person_response = _fixture.Create<PersonResponse>();
            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();
            _countriesServiceMock
                     .Setup(temp => temp.GetAllCountriesAsync())
                     .ReturnsAsync(countries);

            _personsServiceMock
                     .Setup(temp => temp.AddPerson(It.IsAny<AddPersonRequest>()))
                     .ReturnsAsync(person_response);

            PersonController personsController = new PersonController(_personsService, _countriesService);

            personsController.ModelState.AddModelError("PersonName", "Person Name can't be blank");

            IActionResult result = await personsController.Add(addPersonRequest);

            ViewResult viewResult = Assert.IsType<ViewResult>(result);

            viewResult.ViewData.Model.Should().BeAssignableTo<AddPersonRequest>();

            viewResult.ViewName.Should().BeNull(); // why null not the view name => Add

            viewResult.ViewData.Model.Should().Be(addPersonRequest);
        }

        [Fact]
        public async void Add_IfNoModelErrors_ToReturnRedirectToIndex()
        {
            AddPersonRequest addPersonRequest = _fixture.Create<AddPersonRequest>();
            PersonResponse person_response = _fixture.Create<PersonResponse>();
            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();

            _countriesServiceMock
                     .Setup(temp => temp.GetAllCountriesAsync())
                     .ReturnsAsync(countries);

            _personsServiceMock
                     .Setup(temp => temp.AddPerson(It.IsAny<AddPersonRequest>()))
                     .ReturnsAsync(person_response);

            PersonController personsController = new PersonController(_personsService, _countriesService);

            IActionResult result = await personsController.Add(addPersonRequest);

            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);

            redirectResult.ActionName.Should().Be("Index");

        }

        #endregion
    }
}
