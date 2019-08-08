namespace Microsoft.Teams.App.KronosWfc.Models
{
    using System;

    [Serializable]
    public class LoginResponse
    {
        public string JsessionID { get; set; }
        public string PersonNumber { get; set; }
        public string Name { get; set; }
    }
}
