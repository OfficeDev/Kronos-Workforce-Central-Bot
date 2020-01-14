using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Teams.App.KronosWfc.Models
{
    public class Message
    {
        public string message { get; set; }
        public LuisResultModel luisResult { get; set; }

        public string jID { get; set;  }
    }
}