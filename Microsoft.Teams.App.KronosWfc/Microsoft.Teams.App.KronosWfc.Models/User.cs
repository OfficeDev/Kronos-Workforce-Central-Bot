using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microsoft.Teams.App.KronosWfc.Models
{
    public class User
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string TenantId { get; set; }


        public string TeamsUserId { get; set; }

        public string PersonNumber { get; set; }

        public string ConversationId { get; set; }
    }
}
