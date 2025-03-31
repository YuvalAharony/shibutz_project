using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shibutz_project
{
<<<<<<< HEAD

    public class Chromosome
    {
        public Dictionary<String, List<Shift>> Shifts { get; set; } = new Dictionary<String, List<Shift>>();
        public double Fitness { get; set; }

        public Chromosome()
        {

        }



=======
    class Chromosome
    {
        public Dictionary<Branch, List<Shift>> Shifts { get; set; }
        public double Fitness { get; set; }
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
    }
}
