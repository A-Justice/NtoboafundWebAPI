namespace NtoboaFund.Helpers
{
    public class RaveApiSettingsDTO
    {

        string currentMode = "Test";
       // string currentMode = "Live";



        public string LiveApiKey { get; set; }

        public string LiveApiSecret { get; set; }


        public string TestApiKey { get; set; }

        public string TestApiSecret { get; set; }

        public string EncryptionKey { get; set; }


        /// <summary>
        /// Returns The Api Key base on current Mode
        /// </summary>
        /// <param name="mode">Live or Test</param>
        /// <returns></returns>
        public string GetApiSecret()
        {
            if (currentMode == "Live")
                return LiveApiSecret;
            else if (currentMode == "Test")
                return TestApiSecret;
            else return null;
        }


        /// <summary>
        /// Returns The Api Key base on current Mode
        /// </summary>
        /// <param name="mode">Live or Test</param>
        /// <returns></returns>
        public string GetPublicApiKey()
        {
            if (currentMode == "Live")
                return LiveApiKey;
            else if (currentMode == "Test")
                return TestApiKey;
            else return null;
        }


    }



}