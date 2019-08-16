using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NtoboaFund.Data.DTO_s
{
    public class RegistrationDTO
    {
        public string Id { get; set; }
        public IFormFile Images { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
        

        [Required]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }


        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }

    public class UserEditDTO
    {
        public string Id { get; set; }
        public IFormFile Images { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }


    }
}
