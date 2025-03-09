using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shibutz_project
{
    public class Employee
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<String> Roles { get; set; }
        public HashSet<int> requestedShifts { get; set; }
        public double Rate { get; set; }
        public int HourlySalary { get; set; }
        public int AssignedHours { get; set; }
        public bool isMentor { get; set; }



        public Employee(int iD, string name, List<string> roles, HashSet<int> requestedShifts, double rate, int hourlySalary, int assignedHours, bool isMentor)
        {
            ID = iD;
            Name = name;
            Roles = roles;
            this.requestedShifts = requestedShifts;
            Rate = rate;
            HourlySalary = hourlySalary;
            AssignedHours = assignedHours;
            this.isMentor = isMentor;
        }

        
    }
}
