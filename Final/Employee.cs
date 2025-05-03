using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final
{
    // מחלקה המייצגת עובד במערכת
    public class Employee
    {
        // מזהה ייחודי של העובד
        public int ID { get; set; }
        // שם העובד
        public string Name { get; set; }
        // אוסף התפקידים של העובד
        public HashSet<String> roles { get; set; }
        // אוסף המשמרות המבוקשות על ידי העובד
        public HashSet<int> requestedShifts { get; set; }
        // גיבוי למשמרות המבוקשות
        public HashSet<int> backUprequestedShifts { get; set; }
        // דירוג העובד
        public int Rate { get; set; }
        // שכר שעתי של העובד
        public int HourlySalary { get; set; }
        // האם העובד מנוסה/מנטור
        public bool isMentor { get; set; }
        // רשימת הסניפים בהם העובד עובד
        public List<string> Branches { get; set; }

        // בנאי המחלקה - יוצר אובייקט עובד חדש
        public Employee(int ID, string name, HashSet<string> roles, HashSet<int> requestedShifts, int rate, int hourlySalary, bool isMentor, List<string> branches)
        {
            this.ID = ID;
            Name = name;
            this.roles = roles;
            this.requestedShifts = requestedShifts;
            this.backUprequestedShifts = new HashSet<int>();
            if (requestedShifts != null)
            {
                foreach (int id in this.requestedShifts)
                {
                    this.backUprequestedShifts.Add(id);
                }
            }
            Rate = rate;
            HourlySalary = hourlySalary;
            this.isMentor = isMentor;
            Branches = branches;
        }
    }
}