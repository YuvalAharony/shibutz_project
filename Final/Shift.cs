using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Final
{
    public class Shift
    {
        public int Id { get; set; }


        public string branch { get; set; }

        public string TimeSlot { get; set; }

        public string day { get; set; }

        public Dictionary<string, int> RequiredRoles { get; set; }

        //public bool IsBusy { get; set; }

        public Dictionary<String,List<Employee>> AssignedEmployees { get; set; }

        public string EventType { get; set; }

        public Shift() { }

        public Shift(int id, string branch, string timeSlot, string day, Dictionary<string, int> requiredRoles, bool isBusy, Dictionary<String,List<Employee>
           > assignedEmployees, string eventType)
        {
            this.Id = id;
            this.branch = branch;
            this.TimeSlot = timeSlot;
            this.RequiredRoles = requiredRoles;
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