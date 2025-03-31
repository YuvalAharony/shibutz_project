using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using shibutz_project;


namespace shibutz_project
{
    public class Shift
    {
        public int Id;


<<<<<<< HEAD
        public string branch {  get; set; }

        public string TimeSlot { get; set; }

        public string day {  get; set; }

        public Dictionary<string, int> RequiredRoles { get; set; }

        public bool IsBusy { get; set; }

        public HashSet<int> AssignedEmployees { get; set; }

        public string EventType { get; set; }

        public Shift() { }

        public Shift(int id, string branch, string timeSlot, string day, Dictionary<string, int> requiredRoles, bool isBusy, HashSet<int> assignedEmployees, string eventType)
=======
        public string branch;

        public string TimeSlot;

        public Dictionary<string, int> RequiredRoles;

        public bool IsBusy;

        public HashSet<int> AssignedEmployees;

        public string EventType;

        public Shift(int id, string barnch, string timeSlot, Dictionary<string, int> requiredRoles, bool isBusy, HashSet<int> assignedEmployees, string eventType)
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
        {
            this.Id = id;
            this.branch = branch;
            this.TimeSlot = timeSlot;
            this.RequiredRoles = requiredRoles;
            this.IsBusy = isBusy;
<<<<<<< HEAD
            this.day = day;
=======
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
            this.AssignedEmployees = assignedEmployees;
            this.EventType = eventType;
        }

        public int GetTotalRequiredEmployees()
        {
            return RequiredRoles.Values.Sum();
        }

    }
}
