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
<<<<<<< HEAD
        public HashSet<int> backUprequestedShifts { get; set; }
=======
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
        public double Rate { get; set; }
        public int HourlySalary { get; set; }
        public int AssignedHours { get; set; }
        public bool isMentor { get; set; }

<<<<<<< HEAD
        public List<string> Branches {  get; set; }
        public HashSet<int> requestedShiftsBackup { get; set; }

        
        public Employee(int iD, string name, List<string> roles, HashSet<int> requestedShifts, double rate, int hourlySalary, int assignedHours, bool isMentor, List<string> branches)
=======


        public Employee(int iD, string name, List<string> roles, HashSet<int> requestedShifts, double rate, int hourlySalary, int assignedHours, bool isMentor)
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
        {
            ID = iD;
            Name = name;
            Roles = roles;
            this.requestedShifts = requestedShifts;
<<<<<<< HEAD
            this.backUprequestedShifts =new HashSet<int>();
            foreach (int id in this.requestedShifts)
            {
                this.backUprequestedShifts.Add(id);
            }
=======
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
            Rate = rate;
            HourlySalary = hourlySalary;
            AssignedHours = assignedHours;
            this.isMentor = isMentor;
<<<<<<< HEAD
            Branches = branches;
        }
=======
        }

        
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
    }
}
