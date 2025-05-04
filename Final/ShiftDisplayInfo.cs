using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Final
{
    public class ShiftDisplayInfo
    {
        // מאפייני המחלקה לתצוגת מידע על משמרת

        // מזהה המשמרת
        public int ShiftID { get; set; }

        // שם הסניף שבו מתקיימת המשמרת
        public string BranchName { get; set; }

        // היום בשבוע בו מתקיימת המשמרת
        public string DayOfWeek { get; set; }

        // משבצת הזמן של המשמרת (בוקר/ערב וכו')
        public string TimeSlot { get; set; }

        // סוג המשמרת (רגילה/מיוחדת/חג וכו')
        public string ShiftType { get; set; }

        // פונקציה ליצירת מחרוזת המייצגת את המשמרת
        // פרמטרים: אין
        // ערך מוחזר: מחרוזת המתארת את המשמרת בפורמט מוגדר
        public override string ToString()
        {
            // הרכבת הייצוג המחרוזתי של המשמרת
            return $"{BranchName} - {DayOfWeek} {TimeSlot} ({ShiftType})";
        }
    }
}