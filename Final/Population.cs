using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final
{
    public class Population
    {
        public List<Chromosome> Chromoshomes { get; set; }
        public int Size { get; set; }

        public Population(List<Chromosome> chromoshomes, int size)
        {
            Chromoshomes = chromoshomes;
            Size = size;
        }

        public Population(int size)
        {
            Size = size;
            this.Chromoshomes=new List<Chromosome>();
        }
    }

}