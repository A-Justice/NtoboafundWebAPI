using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NtoboaFund.Data.Models
{
    public class UserBuilder
    {
        public int UserBuilderId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public string PhoneNumber { get;set;}
    }
}
