using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Final
{
    public class Shift
    {
        // מזהה המשמרת
        public int Id { get; set; }
        //סניף
        public string branch { get; set; }
        //זמן
        public string TimeSlot { get; set; }
        //יום
        public string day { get; set; }
        //תפקידים דרושים
        public Dictionary<string, int> RequiredRoles { get; set; }
        //עובדים במשמרת
        public Dictionary<String, List<Employee>> AssignedEmployees { get; set; }
        //סוכ משמרת
        public string EventType { get; set; }

        // בנאי ברירת מחדל
        public Shift() { }

        //בנאי
        public Shift(int id, string branch, string timeSlot, string day, Dictionary<string, int> requiredRoles, bool isBusy, Dictionary<String, List<Employee>
           > assignedEmployees, string eventType)
        {
            // אתחול מאפייני המשמרת
            this.Id = id;
            this.branch = branch;
            this.TimeSlot = timeSlot;
            this.RequiredRoles = requiredRoles;
            this.day = day;
            this.AssignedEmployees = assignedEmployees;
            this.EventType = eventType;
        }

        // פונקציה לחישוב סך כל העובדים הנדרשים למשמרת
        // פרמטרים: אין
        // ערך מוחזר: מספר העובדים הכולל הנדרש למשמרת
        public int GetTotalRequiredEmployees()
        {
            // סיכום כל הערכים במילון התפקידים הנדרשים
            return RequiredRoles.Values.Sum();
        }
    }
}