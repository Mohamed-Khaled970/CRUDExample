using Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTO
{
    public class AddPersonRequest
    {
        [Required(ErrorMessage ="Name Should not be Null")]
        [MinLength(6 , ErrorMessage ="The Length of Name Shouldn't be less than 6")]
        public string? Name { get; set; } = string.Empty;

        [Required(ErrorMessage ="Date Of Birth Can't be Empty")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Country Can't be Empty")]
        public Guid? CountryId { get; set; }

        [Required(ErrorMessage = "Phone Number Should not be null")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;


        public Person ToPerson()
        {
            return new Person()
            {
                Name = this.Name!,
                DateOfBirth = (DateTime)this.DateOfBirth!,
                CountryId = (Guid)this.CountryId!,
                PhoneNumber = this.PhoneNumber,
                Address = this.Address
            };
        }
    }
}
