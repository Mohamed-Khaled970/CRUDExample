using ServiceContracts.DTO;
using ServiceContracts;
using Entities;

using Services.Helpers;
using ServiceContracts.Enums;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using OfficeOpenXml;
using RepositoryContracts;

namespace Services
{
    public class PersonsService : IPersonsService
    {
        private readonly IPersonsRepository _personsRepository;
        public PersonsService(IPersonsRepository personsRepository)
        {
            _personsRepository  = personsRepository;    
        }
        public async Task<PersonResponse> AddPerson(AddPersonRequest? request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            ValidatorHelper.ValidateModel(request);

            Guid id = Guid.NewGuid();

            Person person = request.ToPerson();
            person.Id = id;
            await _personsRepository.AddPerson(person);

            PersonResponse response = person.ToPersonResponse();
            return response;

        }



        public async Task<List<PersonResponse>?> GetAllPersons()
        {

            var allPersons = await _personsRepository.GetAllPersons();

            if (allPersons.Count() == 0)
                return null;

            return allPersons.Select(person => person.ToPersonResponse()).ToList();
        }

        public async Task<PersonResponse?> GetPersonById(Guid? id)
        {
            if (id is null)
                return null;

            Person? person
                = await _personsRepository.GetPersonByPersonID(id.Value);

            if (person == null)
                return null;

            return person.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> Search(string searchProperty, string searchTerm)
        {
            var allPersons = await _personsRepository.GetAllPersons();


            if (string.IsNullOrEmpty(searchProperty) || string.IsNullOrEmpty(searchTerm))
            {
                

                return allPersons.Select(res =>
                {
                    var response = res.ToPersonResponse();
                    return response;
                }).ToList();
            }

            switch (searchProperty)
            {
                case "Name":
                    return allPersons.Where(x => x.Name.Contains(searchTerm,
                        StringComparison.OrdinalIgnoreCase))
                        .Select(res =>
                        {
                            var response = res.ToPersonResponse();
                            return response;
                        }).ToList();

                case "DateOfBirth":
                    return allPersons.Where(x => x.DateOfBirth.ToString().
                    Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                       .Select(res =>
                       {
                           var response = res.ToPersonResponse();
                           return response;
                       }).ToList();
                case "Id":
                    return allPersons.Where(x => x.Id == Guid.Parse(searchTerm))
                       .Select(res =>
                       {
                           var response = res.ToPersonResponse();
                           return response;
                       }).ToList();

                case "Address":
                    return allPersons.Where(x => x.Address.Contains(searchTerm,
                        StringComparison.OrdinalIgnoreCase))
                        .Select(res =>
                        {
                            var response = res.ToPersonResponse();
                            return response;
                        }).ToList();
                case "PhoneNumber":
                    return allPersons.Where(x => x.PhoneNumber.Contains(searchTerm,
                        StringComparison.OrdinalIgnoreCase))
                        .Select(res =>
                        {
                            var response = res.ToPersonResponse();
                            return response;
                        }).ToList();

                default:
                    return allPersons.Select(res =>
                    {
                        var response = res.ToPersonResponse();
                        return response;
                    }).ToList();

            }
        }

        public List<PersonResponse> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return allPersons;
            }


            if(sortBy == "Name" && orderOptions == OrderOptions.ASC)
            {
                return allPersons.OrderBy(x => x.Name).ToList();    
            }
            else if (sortBy == "Name" && orderOptions == OrderOptions.DESC)
            {
                return allPersons.OrderByDescending(x => x.Name).ToList();
            }
            if (sortBy == "Id" && orderOptions == OrderOptions.ASC)
            {
                return allPersons.OrderBy(x => x.Id).ToList();
            }
            if (sortBy == "Id" && orderOptions == OrderOptions.DESC)
            {
                return allPersons.OrderByDescending(x => x.Id).ToList();
            }
            if (sortBy == "DateOfBirth" && orderOptions == OrderOptions.ASC)
            {
                return allPersons.OrderBy(x => x.DateOfBirth).ToList();
            }
            if (sortBy == "DateOfBirth" && orderOptions == OrderOptions.DESC)
            {
                return allPersons.OrderByDescending(x => x.DateOfBirth).ToList();
            }
            if (sortBy == "Address" && orderOptions == OrderOptions.ASC)
            {
                return allPersons.OrderBy(x => x.Address).ToList();
            }
            if (sortBy == "Address" && orderOptions == OrderOptions.DESC)
            {
                return allPersons.OrderByDescending(x => x.Address).ToList();
            }
            if (sortBy == "PhoneNumber" && orderOptions == OrderOptions.ASC)
            {
                return allPersons.OrderBy(x => x.PhoneNumber).ToList();
            }
            if (sortBy == "PhoneNumber" && orderOptions == OrderOptions.DESC)
            {
                return allPersons.OrderByDescending(x => x.Address).ToList();
            }
            if (sortBy == "Country" && orderOptions == OrderOptions.ASC)
            {
                return allPersons.OrderBy(x => x.Country).ToList();
            }
            if (sortBy == "Country" && orderOptions == OrderOptions.DESC)
            {
                return allPersons.OrderByDescending(x => x.Country).ToList();
            }
            else
            {
                return allPersons;
            }
        }

        public async Task<PersonResponse> UpdatePerson(UpdatePersonRequest? request)
        {
            if(request is null)
                throw new ArgumentNullException(nameof(request));
 
            ValidatorHelper.ValidateModel(request);

            var person = await _personsRepository.GetPersonByPersonID(request.PersonID);

            if (person is null)
                throw new ArgumentException("Person is not Found ");

            person.Address = request.Address!;
            person.PhoneNumber = request.PhoneNumber;
            person.DateOfBirth = request.DateOfBirth;
            person.CountryId = request.CountryId;
            person.Name = request.Name!;
            person.Id = request.PersonID; ;

            await _personsRepository.UpdatePerson(person);

            return person.ToPersonResponse();


        }

        public async Task<bool> DeletePerson(Guid? PersonID)
        {
            if (PersonID is null)
                throw new ArgumentNullException(nameof(PersonID));

            var person = await _personsRepository.GetPersonByPersonID(PersonID.Value);

            if (person is null)
                return false;

            await _personsRepository.DeletePersonByPersonID(PersonID.Value);
            return true;
        }

        public async Task<MemoryStream> GetPersonsCSV()
        {
            MemoryStream stream = new MemoryStream();

            StreamWriter writer = new StreamWriter(stream);

            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);

            CsvWriter csvWriter = new CsvWriter(writer, csvConfiguration);

            csvWriter.WriteField(nameof(PersonResponse.Name));
            csvWriter.WriteField(nameof(PersonResponse.Country));

            csvWriter.NextRecord();

            var persons = await GetAllPersons();

            foreach(var person in persons!)
            {
                csvWriter.WriteField(person.Name);
                csvWriter.WriteField(person.Country);
                csvWriter.NextRecord();

                csvWriter.Flush();

            }

            stream.Position = 0;

            return stream;

        }

        public async Task<MemoryStream> GetPersonsExcel()
        {
            MemoryStream memoryStream = new MemoryStream();


            ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization");

            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream)) {

                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("PersonsSheet");

                worksheet.Cells["A1"].Value = "Person Name";
                worksheet.Cells["B1"].Value = "Phone Number";
                worksheet.Cells["C1"].Value = "Date of Birth";
                worksheet.Cells["D1"].Value = "Country";
                worksheet.Cells["E1"].Value = "Address";

                int row = 2;

                var persons = await GetAllPersons();

                foreach( var person in persons!)
                {
                    worksheet.Cells[row, 1].Value = person.Name;
                    worksheet.Cells[row, 2].Value = person.PhoneNumber;
                    worksheet.Cells[row, 3].Value = person.DateOfBirth.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 4].Value = person.Country;
                    worksheet.Cells[row, 5].Value = person.Address;


                    row++;
                }

                worksheet.Cells[$"A1:E{row}"].AutoFitColumns();

                await excelPackage.SaveAsync();


            }

            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}
