using NtoboaFund.Data.DTO_s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NtoboaFund.Helpers
{
    public class AppSettings
    {
        public SettingsDTO Settings { get; set; }

        public HubtelApiSettingsDTO HubtelApiSettings { get; set; }

    }
}
