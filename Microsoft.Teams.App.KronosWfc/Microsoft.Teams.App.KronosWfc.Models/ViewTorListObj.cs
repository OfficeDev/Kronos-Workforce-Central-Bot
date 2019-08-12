using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Teams.App.KronosWfc.Models
{
    public class ViewTorListObj
    {
        public Hashtable Filters { get; set; }
        public Hashtable Employees { get; set; }
        public Hashtable EmployeesRoles { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPageCount { get; set; }
        public string ConversationId { get; set; }
        public string ActivityId { get; set; }

    }
}
