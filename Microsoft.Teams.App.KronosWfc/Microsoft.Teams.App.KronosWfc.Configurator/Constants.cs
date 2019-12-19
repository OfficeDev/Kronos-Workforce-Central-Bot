namespace Microsoft.Teams.App.KronosWfc.Configurator
{
    public class Constants
    {
        public const string SoapEnvOpen = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:hs=""http://localhost/wfc/XMLAPISchema"" ><soapenv:Body><hs:KronosWFC><Kronos_WFC version = ""1.0"">";

        public const string SoapEnvClose = @"</Kronos_WFC></hs:KronosWFC></soapenv:Body></soapenv:Envelope>";

        public const string SoapAction = "http://localhost/wfc/XMLAPISchema";

        public const string System = "System";

        public const string LogonAction = "Logon";
        public const string Response = "Response";
        public const string Success = "Success";

        public const string Failure = "Failure";
    }
}
