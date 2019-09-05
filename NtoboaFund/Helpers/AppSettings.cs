using NtoboaFund.Data.DTO_s;

namespace NtoboaFund.Helpers
{
    public class AppSettings
    {
        public SettingsDTO Settings { get; set; }

        public RaveApiSettingsDTO FlatterWaveSettings { get; set; }


        public SendGridSettingsDTO SendGridSettings { get; set; }
    }
}
