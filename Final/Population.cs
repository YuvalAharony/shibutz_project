using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final
{
    // מחלקה המייצגת אוכלוסייה של פתרונות אפשריים באלגוריתם גנטי
    public class Population
    {
        // רשימת הכרומוזומים (פתרונות) באוכלוסייה
        public List<Chromosome> Chromoshomes { get; set; }
        // גודל האוכלוסייה
        public int Size { get; set; }

        public Population(List<Chromosome> chromoshomes, int size)
        {
            Chromoshomes = chromoshomes;
            Size = size;
        }
    }
}