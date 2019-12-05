using NtoboaFund.Data.DTO_s;

namespace NtoboaFund.Helpers
{
    public class AppSettings
    {
        public SettingsDTO Settings { get; set; }

        public RaveApiSettingsDTO FlatterWaveSettings { get; set; }

        public SlydePayApiSettingsDTO SlydePaySettings { get; set; }


        public SendGridSettingsDTO SendGridSettings { get; set; }

        public MNotifySettingsDTO MNotifySettings { get; set; }

        public ReddeSettingsDTO ReddeSettings { get; set; }
    }
}
