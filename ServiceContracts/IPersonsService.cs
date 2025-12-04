using ServiceContracts.DTO;
using ServiceContracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public interface IPersonsService
    {
        public  Task<PersonResponse> AddPerson(AddPersonRequest? request);
        public Task<List<PersonResponse>?> GetAllPersons();

        public Task<PersonResponse?> GetPersonById(Guid? id); 
        
        public Task<List<PersonResponse>> Search( string searchProperty ,string searchTerm);

        public List<PersonResponse> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, OrderOptions orderOptions);
        public Task<PersonResponse> UpdatePerson(UpdatePersonRequest? request);
        public Task<bool> DeletePerson(Guid? PersonID);

        public Task<MemoryStream> GetPersonsCSV();

        public Task<MemoryStream> GetPersonsExcel();



    }
}
