using Microsoft.AspNetCore.Http;
using ServiceContracts.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public interface ICountriesService
    {
        Task<CountryResponse> AddCountryAsync(CountryAddRequest? request);

        Task<List<CountryResponse>> GetAllCountriesAsync();

        Task<CountryResponse?> GetCountryByIdAsync(Guid? id);

        Task<int> UploadCountriesFromExcelFile(IFormFile formFile);

    }
}
