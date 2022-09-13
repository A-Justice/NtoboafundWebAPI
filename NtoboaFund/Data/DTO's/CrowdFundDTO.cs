using System;
namespace NtoboaFund.Data.DTOs
{
    public class CrowdFundForReturnDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string PhoneNumber { get; set; }

        public string DateCreated { get; set; }

        public string EndDate { get; set; }

        public string MainImageUrl { get; set; }

        public string SecondImageUrl { get; set; }

        public string ThirdImageUrl { get; set; }

        public string videoUrl { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal TotalAmountRecieved { get; set; }

        public int TypeId { get; set; }

        public int PeopleContributed { get; set; }

        public string UserId
        {
            get; set;
        }

        //Additional
        public string Username { get; set; }

        public string CategoryName { get; set; }

        public string CrowdfundTypeName { get; set; }


    }
}
