using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;

namespace CRUDExample.Controllers
{
    public class PersonController : Controller
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        public PersonController(IPersonsService personsService , ICountriesService countriesService)
        {
            _personsService = personsService;
           _countriesService = countriesService;
        }
        [Route("/")]
        public async Task<IActionResult> Index(string? searchProperty, string? searchTerm, string? sortBy, OrderOptions orderOptions = OrderOptions.ASC)
        {
            // Search
            var persons = await _personsService.Search(searchProperty ?? "", searchTerm ?? "");

            // Sort
            if (!string.IsNullOrEmpty(sortBy))
            {
                persons = _personsService.GetSortedPersons(persons, sortBy, orderOptions);
            }

            ViewBag.CurrentSearchProperty = searchProperty;
            ViewBag.CurrentSearchTerm = searchTerm;
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentOrder = orderOptions;

            return View(persons);
        }

        [Route("/Person/Add")]
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var countries = await _countriesService.GetAllCountriesAsync();
            ViewBag.Countries = new SelectList(countries, "CountryId", "CountryName");
            return View("Add");
        }

        [Route("/Person/Add")]
        [HttpPost]
        public async Task<IActionResult> Add(AddPersonRequest request)
        {
            if (!ModelState.IsValid)
            {
                var countries = await _countriesService.GetAllCountriesAsync();
                ViewBag.Countries = new SelectList(countries, "CountryId", "CountryName");
                return View(request);
            }

            try
            {
                await _personsService.AddPerson(request);
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var countries = await _countriesService.GetAllCountriesAsync();
                ViewBag.Countries = new SelectList(countries, "CountryId", "CountryName");
                return View("Add", request);
            }
        }

        [Route("/Person/Edit")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var person = await _personsService.GetPersonById(id);

            if (person == null)
                return RedirectToAction("Index");

            var updateRequest = new UpdatePersonRequest()
            {
                PersonID = person.Id,
                Name = person.Name,
                DateOfBirth = person.DateOfBirth,
                CountryId = person.CountryId,
                PhoneNumber = person.PhoneNumber,
                Address = person.Address
            };

            var countries = await _countriesService.GetAllCountriesAsync();
            ViewBag.Countries = new SelectList(countries, "CountryId", "CountryName", person.CountryId);

            return View(updateRequest);
        }

        [Route("/Person/Edit")]
        [HttpPost]
        public async Task<IActionResult> Edit(UpdatePersonRequest request)
        {
            if (!ModelState.IsValid)
            {
                var countries = await _countriesService.GetAllCountriesAsync();
                ViewBag.Countries = new SelectList(countries, "CountryId", "CountryName", request.CountryId);
                return View(request);
            }

            await _personsService.UpdatePerson(request);
            return RedirectToAction("Index");
        }

        [Route("/Person/Delete")]
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var person = await _personsService.GetPersonById(id);

            if (person == null)
                return RedirectToAction("Index");

            return View(person);
        }

        [Route("/Person/Delete")]
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            bool deleted = await _personsService.DeletePerson(id);

            if (deleted)
            {
                TempData["SuccessMessage"] = "Person deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Something went wrong. Could not delete person.";
            }

            return RedirectToAction("Index");
        }

        [Route("/Person/PersonsPdf")]
        public async  Task<IActionResult> DownloadPdf()
        {
            var persons = await _personsService.GetAllPersons(); // تجيب كل الأشخاص
            return new ViewAsPdf("PersonsPdf", persons)
            {
                FileName = "PersonsList.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }


        [Route("/Person/PersonsCSV")]

        public async Task<IActionResult> DownloadCSV()
        {
            MemoryStream stream = await _personsService.GetPersonsCSV();


            return File(stream, "application/octet-stream");
        }

        [Route("/Person/PersonsExcel")]

        public async Task<IActionResult> DownloadExcel()
        {
            MemoryStream stream = await _personsService.GetPersonsExcel();


            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" , "persons.xlsx");
        }

    }
}
