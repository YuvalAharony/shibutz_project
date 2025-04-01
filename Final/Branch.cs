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

    // בנאי עם פרמטרים
    public Branch(int id, string name, List<Shift> shifts)
    {
        this.ID = id;
        this.Name = name;
        this.Shifts = shifts ?? new List<Shift>();
    }

    // שיטה ליצירת העתק

}