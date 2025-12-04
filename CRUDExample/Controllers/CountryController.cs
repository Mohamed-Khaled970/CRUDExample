using Entities;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace CRUDExample.Controllers
{
    [Route("[controller]")]
    public class CountryController : Controller
    {
        private readonly ICountriesService _countriesService;

        public CountryController(ICountriesService countriesService)
        {
            _countriesService = countriesService;
        }

        [Route("UploadFromExcel")]
        public IActionResult UploadExcel()
        {
            return View("UploadExcel");
        }

        [HttpPost]
        [Route("UploadFromExcel")]

        public async Task<IActionResult> UploadExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                ViewBag.ErrorMessage = "Please select an xlsx file";
                return View("UploadExcel");
            }

            if (!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorMessage = "Unsupported file. 'xlsx' file is expected";
                return View("UploadExcel");
            }

            int countriesCountInserted = await _countriesService.UploadCountriesFromExcelFile(excelFile);

            ViewBag.Message = $"{countriesCountInserted} Countries Uploaded";
            return View("UploadExcel");
        }
    }
}
