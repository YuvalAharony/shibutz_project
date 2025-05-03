using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final
{
    // מחלקה המייצגת פתרון אפשרי (כרומוזום) באלגוריתם הגנטי
    public class Chromosome
    {
        // מילון המכיל את המשמרות לפי סניפים
        public Dictionary<String, List<Shift>> Shifts { get; set; } = new Dictionary<String, List<Shift>>();

        // ציון הכושר  של הכרומוזום
        public double Fitness { get; set; }

        // בנאי - יוצר כרומוזום ריק
        public Chromosome()
        {
            this.Shifts = new Dictionary<string, List<Shift>>();
            Fitness = 0;
        }
    }
}