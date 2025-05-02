using Final;
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

   

    

}