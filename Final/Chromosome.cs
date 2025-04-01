using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final
{

    public class Chromosome
    {
        public Dictionary<String, List<Shift>> Shifts { get; set; } = new Dictionary<String, List<Shift>>();
        public double Fitness { get; set; }

        public Chromosome()
        {

        }



    }
}