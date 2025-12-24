using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly ICountriesRepository _countriesRepository;

        public CountriesService(ICountriesRepository countriesRepository)
        {
            _countriesRepository = countriesRepository;
        }

        public async Task<CountryResponse> AddCountryAsync(CountryAddRequest? request)
        {
            if (request is null)
                throw new ArgumentNullException("Country Can't Be Null");

            if (request.CountryName is null)
                throw new ArgumentException("Country Name Can't Be Null");

            // Check duplicate
            var exists = await _countriesRepository.GetCountryByCountryName(request.CountryName);

            if (exists is not null)
                throw new ArgumentException("Duplicate Country Names");

            Country country = request.ToCountry();
            country.CountryId = Guid.NewGuid();

            await _countriesRepository.AddCountry(country);    

            return country.ToCountryResponse();
        }

        public async Task<List<CountryResponse>?> GetAllCountriesAsync()
        {
            var countries = await _countriesRepository.GetAllCountries();

            if (countries.Count() <= 0)
                return new List<CountryResponse>();


            return  countries.Select(c => c.ToCountryResponse()).ToList();
        }

        public async Task<CountryResponse?> GetCountryByIdAsync(Guid? id)
        {
            if (id is null)
                return null;

            Country? country = await _countriesRepository.GetCountryByCountryID(id.Value);

            return country?.ToCountryResponse();
        }

        public async Task<int> UploadCountriesFromExcelFile(IFormFile formFile)
        {
            MemoryStream memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            int countriesInserted = 0;

            ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization");


            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                var workSheet = excelPackage.Workbook.Worksheets.FirstOrDefault();

                if (workSheet == null)
                    throw new Exception("Excel file does not contain any sheets.");

                if (workSheet.Dimension == null)
                    throw new Exception("Excel sheet is empty.");

                int rowCount = workSheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    string? cellValue = Convert.ToString(workSheet.Cells[row, 1].Value);

                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        string? countryName = cellValue;

                        if (await _countriesRepository.GetCountryByCountryName(countryName) is null)
                        {
                            Country country = new Country() { CountryName = countryName };
                            await _countriesRepository.AddCountry(country);   

                             countriesInserted++;
                        }
                    }
                }
            }

            return countriesInserted;
        }
    }
}
