using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace NtoboaFund.Data.DTO_s
{
    public class RegistrationDTO
    {
        public string Id { get; set; }
        public IFormFile Images { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string LastName { get; set; }


        [Required]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }


        [Required]
        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }
    }

    public class UserEditDTO
    {
        public string Id { get; set; }
        public IFormFile Images { get; set; }

        [Required]
        public string FirstName { get; set; }


        public string LastName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }


        public string Email { get; set; }

        public string Country { get; set; }

        public string MobileMoneyNumber { get; set; }

        public string Network { get; set; }

        public string Currency { get; set; }

        public string BankName { get; set; }

        public string AccountNumber { get; set; }

        public string SwiftCode { get; set; }

        public string PreferredReceptionMethod { get; set; }



    }
}
