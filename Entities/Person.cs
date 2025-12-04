using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Person
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(40)]
        public string Name { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public Guid CountryId { get; set; }

        [StringLength(200)]
        public string PhoneNumber { get; set; } = string.Empty;


        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        public virtual Country Country { get; set; }

    }
}
