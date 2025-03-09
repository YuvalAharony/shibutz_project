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


        public string branch;

        public string TimeSlot;

        public Dictionary<string, int> RequiredRoles;

        public bool IsBusy;

        public HashSet<int> AssignedEmployees;

        public string EventType;

        public Shift(int id, string barnch, string timeSlot, Dictionary<string, int> requiredRoles, bool isBusy, HashSet<int> assignedEmployees, string eventType)
        {
            this.Id = id;
            this.branch = branch;
            this.TimeSlot = timeSlot;
            this.RequiredRoles = requiredRoles;
            this.IsBusy = isBusy;
            this.AssignedEmployees = assignedEmployees;
            this.EventType = eventType;
        }

        public int GetTotalRequiredEmployees()
        {
            return RequiredRoles.Values.Sum();
        }

    }
}
