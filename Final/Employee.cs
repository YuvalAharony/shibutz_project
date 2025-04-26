using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final
{
    public class Employee
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public HashSet<String> roles { get; set; }
        public HashSet<int> requestedShifts { get; set; }
        public HashSet<int> backUprequestedShifts { get; set; }
        public int Rate { get; set; }
        public int HourlySalary { get; set; }
        public bool isMentor { get; set; }
        public List<string> Branches { get; set; }

        public int Property
        {
            get => default;
            set
            {
            }
        }

        public Employee(int ID, string name, HashSet<string> roles, HashSet<int> requestedShifts, int rate, int hourlySalary, bool isMentor, List<string> branches)
        {
            this.ID = ID;
            Name = name;
            this.roles = roles;
            this.requestedShifts = requestedShifts;
            this.backUprequestedShifts = new HashSet<int>();
            if (requestedShifts!=null)
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