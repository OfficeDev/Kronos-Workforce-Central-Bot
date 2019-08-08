using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Teams.App.KronosWfc.Models
{
    public class SwapShiftObj
    {
        public string QueryDateSpan { get; set; }
        public string RequestorPersonNumber { get; set; }
        public string RequestedToPersonNumber { get; set; }
        public string RequestorName { get; set; }
        public string RequestedToName { get; set; }
        public DateTime Emp1FromDateTime { get; set; }
        public DateTime Emp1ToDateTime { get; set; }
        public DateTime Emp2FromDateTime { get; set; }
        public DateTime Emp2ToDateTime { get; set; }
        public string SelectedShiftToSwap { get; set; }
        public string SelectedLocation { get; set; }
        public string SelectedJob { get; set; }
        public string SelectedEmployee { get; set; }
        public string SelectedAvailableShift { get; set; }

        public string RequestId { get; set; }

        public List<string> AllEmps { get; set; }
    }
}
