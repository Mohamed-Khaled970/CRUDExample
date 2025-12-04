using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly ApplicationDbContext _context;

        public CountriesService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CountryResponse> AddCountryAsync(CountryAddRequest? request)
        {
            if (request is null)
                throw new ArgumentNullException("Country Can't Be Null");

            if (request.CountryName is null)
                throw new ArgumentException("Country Name Can't Be Null");

            // Check duplicate
            bool exists = await _context.Countries
                .AnyAsync(c => c.CountryName == request.CountryName);

            if (exists)
                throw new ArgumentException("Duplicate Country Names");

            Country country = request.ToCountry();
            country.CountryId = Guid.NewGuid();

            await _context.Countries.AddAsync(country);
            await _context.SaveChangesAsync();

            return country.ToCountryResponse();
        }

        public async Task<List<CountryResponse>> GetAllCountriesAsync()
        {
            return await _context.Countries
                .Select(c => c.ToCountryResponse())
                .ToListAsync();
        }

        public async Task<CountryResponse?> GetCountryByIdAsync(Guid? id)
        {
            if (id is null)
                return null;

            Country? country = await _context.Countries
                .FirstOrDefaultAsync(c => c.CountryId == id);

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

                        if (_context.Countries.Where(temp => temp.CountryName == countryName).Count() == 0)
                        {
                            Country country = new Country() { CountryName = countryName };
                            _context.Countries.Add(country);
                            await _context.SaveChangesAsync();

                            countriesInserted++;
                        }
                    }
                }
            }

            return countriesInserted;
        }
    }
}
