using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shibutz_project
{
    public class Branch
    {
        public int ID { get; set; }
        public string Name { set; get; }
        public List<Shift> Shifts { get; set; }

        public List<Employee> Employees { get; set; }
        public Branch(int iD, string name, List<Shift> shifts)
        {
            ID = iD;
            Name = name;
            Shifts = shifts;
        }
    }
}
