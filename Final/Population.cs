﻿using System;
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

        //בנאי המחלקה
        public Population(List<Chromosome> chromoshomes)
        {
            Chromoshomes = chromoshomes;    
        }
    }
}