using System;
namespace NtoboaFund.Data.DTO_s
{
    public class TellerSettingsDTO
    {
        public string TestEndpoint { get; set; }

        public string LiveEndpoint { get; set; }

        public string TestBaseEndpoint { get; set; }

        public string LiveBaseEndpoint { get; set; }

        public string TestUserName { get; set; }

        public string TestApiKey { get; set; }

        public string LiveUserName { get; set; }

        public string LiveApiKey { get; set; }

        public string MerchantId { get; set; }
    }
}
