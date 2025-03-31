<<<<<<< HEAD
﻿using shibutz_project;
using System.Collections.Generic;

public class Branch
{
    public int ID { get; set; }
    public string Name { get; set; }
    public List<Shift> Shifts { get; set; }

    // בנאי ברירת מחדל
    public Branch()
    {
        Shifts = new List<Shift>();
    }

    // בנאי עם פרמטרים
    public Branch(int id, string name, List<Shift> shifts)
    {
        this.ID = id;
        this.Name = name;
        this.Shifts = shifts ?? new List<Shift>();
    }

    // שיטה ליצירת העתק
    
}
=======
﻿using System;
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
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
