using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final
{
    internal class ShiftDisplayInfo
    {
       
            public int ShiftID { get; set; }
            public string BranchName { get; set; }
            public string DayOfWeek { get; set; }
            public string TimeSlot { get; set; }
            public string ShiftType { get; set; }

            public override string ToString()
            {
                return $"{BranchName} - {DayOfWeek} {TimeSlot} ({ShiftType})";
            }
        
    }
}
