using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Final
{
    public class Shift
    {
        public int Id;


        public string branch { get; set; }

        public string TimeSlot { get; set; }

        public string day { get; set; }

        public Dictionary<string, int> RequiredRoles { get; set; }

        public bool IsBusy { get; set; }

        public HashSet<int> AssignedEmployees { get; set; }

        public string EventType { get; set; }

        public Shift() { }

        public Shift(int id, string branch, string timeSlot, string day, Dictionary<string, int> requiredRoles, bool isBusy, HashSet<int> assignedEmployees, string eventType)
        {
            this.Id = id;
            this.branch = branch;
            this.TimeSlot = timeSlot;
            this.RequiredRoles = requiredRoles;
            this.IsBusy = isBusy;
            this.day = day;
            this.AssignedEmployees = assignedEmployees;
            this.EventType = eventType;
        }

        public int GetTotalRequiredEmployees()
        {
            return RequiredRoles.Values.Sum();
        }

    }
}