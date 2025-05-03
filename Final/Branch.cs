using Final;
using System.Collections.Generic;

// מחלקה המייצגת סניף במערכת
public class Branch
{
    // מזהה ייחודי של הסניף
    public int ID { get; set; }

    // שם הסניף
    public string Name { get; set; }

    // רשימת המשמרות בסניף
    public List<Shift> Shifts { get; set; }

    // בנאי של המחלקה - יוצר סניף חדש וריק
    public Branch()
    {
        Shifts = new List<Shift>();
    }
}