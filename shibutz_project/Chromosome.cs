using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shibutz_project
{
    class Chromosome
    {
        public Dictionary<Branch, List<Shift>> Shifts { get; set; }
        public double Fitness { get; set; }
    }
}
