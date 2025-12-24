using AutoFixture;
using Entities;
using Fizzler.Systems.HtmlAgilityPack;
using FluentAssertions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ServiceContracts.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CRUDTests
{
    public class PersonsControllerIntegrationTest : IClassFixture<CustomWebApplicationFactory>
    {

        private HttpClient _client;
        private readonly IFixture _fixture;
        private CustomWebApplicationFactory _factory;

        public PersonsControllerIntegrationTest(CustomWebApplicationFactory factory)
        {
           _client = factory.CreateClient(new WebApplicationFactoryClientOptions
           {
               AllowAutoRedirect = false  // This is the key!
           });

            _fixture = new Fixture();
            _factory = factory;

        }
        #region Index
        [Fact]
        public async void Index_ShouldReturnView()
        {

            //Act
              HttpResponseMessage response = await _client.GetAsync("/");

            //Assert
            response.IsSuccessStatusCode.Should().BeTrue();   

            var responseBody = await response.Content.ReadAsStringAsync();

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(responseBody);

            var document = html.DocumentNode;

            document.QuerySelectorAll("persons-table").Should().NotBeNull();
        }

        #endregion

        #region
        [Fact]
        public async void AddPerson_ShouldRedirectToIndexView()
        {
            var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
            Guid egyptId;

            using (var scope = scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var egypt = new Country { CountryId = Guid.NewGuid(), CountryName = "Egypt" };
                context.Countries.Add(egypt);
                context.SaveChanges();

                egyptId = egypt.CountryId; // عشان تستخدمه بعد كده
            }
            var formData = new Dictionary<string, string>
                                {
                                    { "Name", "Mohamed Khaled" },
                                    { "DateOfBirth", "1995-01-01" },
                                    { "CountryId", egyptId.ToString() },
                                    { "PhoneNumber", "01099429789" },
                                    { "Address", "Cairo, Egypt" }
                                };

            var content = new FormUrlEncodedContent(formData);

            var response = await _client.PostAsync("/Person/Add", content);
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);

            var redirectResponse = await _client.GetAsync(response.Headers.Location!);
            redirectResponse.EnsureSuccessStatusCode();
            var html = await redirectResponse.Content.ReadAsStringAsync();
            html.Should().Contain("<h2>All Persons</h2>");
        }





        #endregion
    }
}
