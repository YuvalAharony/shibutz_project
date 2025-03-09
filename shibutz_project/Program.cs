using EmployeeSchedulingApp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;

namespace shibutz_project
{
    public class Program
    {
        #region Data
        public static List<Employee> Employees = new List<Employee>();
        public static List<Branch> Branches = new List<Branch>();
        public static DB myDB = new DB();
        public const int ChromosomesEachGene = 10;

        public static Population pop = new Population(new List<Chromosome>(), ChromosomesEachGene);
        #endregion

        public static void createSceduele()
        {
            pop.Chromoshomes.Clear();
            #region inline
        
            {
                // עבור על כל הסניפים
                foreach (Branch branch in Branches)
                {
                    // עבור על כל המשמרות בכל סניף
                    foreach (Shift shift in branch.Shifts)
                    {
                        // אפס את רשימת העובדים המשובצים
                        shift.AssignedEmployees.Clear();
                    }
                }
            }
            #endregion
          
            initializeFirstPopulation();
        }

        public static Population initializeFirstPopulation()
        {
            Random random = new Random();

            #region InLine
            Chromosome initializeChoromosome()
            {
                List<Employee> employeesSortedByRate = sort_employees_by_rate(Employees);
                List<Employee> employeesSortedByavailabilty = sort_employees_by_availabilty(Employees);
                Dictionary<int, List<Employee>> employeesMappedByRequestedShifts = mappingEmployeesByRequestedShifts();
                employeesMappedByRequestedShifts.OrderBy(pair => pair.Key);
                Dictionary<string, List<Employee>> employeesMappedByRequestedRoles = mappingEmployeesByRole();

                #region InLine
                List<Employee> sort_employees_by_availabilty(List<Employee> employees)
                {
                    return employees.OrderBy(e => e.requestedShifts?.Count ?? 0).ToList();//מבטיח שי לא יקרוס אם לא הוגשו משמרות
                }

                List<Employee> sort_employees_by_rate(List<Employee> employees)
                {
                    return employees.OrderByDescending(e => e.Rate).ToList();
                }

                Dictionary<int, List<Employee>> mappingEmployeesByRequestedShifts()
                {
                    return Employees
                        .SelectMany(emp => emp.requestedShifts, (emp, shiftId) => new { shiftId, emp })
                        .GroupBy(entry => entry.shiftId)
                        .ToDictionary(group => group.Key, group => group.Select(entry => entry.emp).ToList());
                }

                Dictionary<string, List<Employee>> mappingEmployeesByRole()
                {
                    return Employees
                        .SelectMany(emp => emp.Roles, (emp, role) => new { role, emp }) // יצירת צמדים של תפקיד + עובד
                        .GroupBy(entry => entry.role) // קיבוץ לפי תפקיד
                        .ToDictionary(group => group.Key, group => group.Select(entry => entry.emp).ToList()); // המרה למילון
                }

                List<int> UpdateOverlappingShifts(Employee employee, Shift assignedShift)
                {
                    if (employee == null || assignedShift == null)
                        return null;

                    // קבל את ה"שארית" של מזהה המשמרת (החלק המשותף לכל הסניפים)
                    int shiftSuffix = assignedShift.Id % 100;  // לדוגמא: 101 -> 01, 201 -> 01
                    List<int> idsToRemove = new List<int>();

                    foreach (int id in employee.requestedShifts)
                    {
                        if (id % 100 == shiftSuffix)
                        {
                            idsToRemove.Add(id);
                        }
                    }
                    foreach (int id in idsToRemove)
                    {
                        employee.requestedShifts.Remove(id);
                    }
                    return idsToRemove;
                }

                List<Shift> fill_brach_shifts(Branch br)
                {
                    List<Shift> shiftsCopy = new List<Shift>();

                    foreach (Shift originalShift in br.Shifts)
                    {
                        Shift shiftCopy = new Shift
                        {
                            Id = originalShift.Id,
                            day = originalShift.day,
                            TimeSlot = originalShift.TimeSlot,
                            RequiredRoles = new Dictionary<string, int>(originalShift.RequiredRoles),
                            AssignedEmployees = new HashSet<int>()
                        };
                        shiftsCopy.Add(shiftCopy);
                    }
                    List<Shift> shuffledShifts = shiftsCopy.OrderBy(x => random.Next()).ToList();


                    foreach (Shift sh in shuffledShifts)
                    {
                        List<Employee> currenList = new List<Employee>();

                        foreach (var entry in sh.RequiredRoles)
                        {
                            List<Employee> employeesAvaliableForShift;
                            employeesMappedByRequestedShifts.TryGetValue(sh.Id, out employeesAvaliableForShift);

                            for (int i = 0; i < entry.Value; i++)
                            {
                                employeesMappedByRequestedRoles.TryGetValue(entry.Key, out List<Employee> employeesAvaliableForRole);

                                int currentListIdentifier = random.Next(1, 3);//1-employeesSortedByRate //2-employeesSortedByavailabilty

                                switch (currentListIdentifier)
                                {
                                    case (1):
                                        currenList = employeesSortedByRate;
                                        break;
                                    case (2):
                                        currenList = employeesSortedByavailabilty;
                                        break;
                                }

                                if (employeesAvaliableForShift != null)
                                {
                                    Employee selectedEmployee = currenList.FirstOrDefault(emp => employeesAvaliableForShift.Contains(emp));

                                    if (selectedEmployee != null)
                                    {
                                        sh.AssignedEmployees.Add(selectedEmployee.ID);
                                        List<int> idToRemove = UpdateOverlappingShifts(selectedEmployee, sh);
                                        foreach (int id in idToRemove)
                                        {
                                            if (employeesMappedByRequestedShifts.TryGetValue(id, out List<Employee> shiftToRemove))
                                            {
                                                shiftToRemove.Remove(selectedEmployee);

                                                // אם הרשימה ריקה אחרי ההסרה, הסר אותה מהמיפוי כולו
                                                if (shiftToRemove.Count == 0)
                                                {
                                                    employeesMappedByRequestedShifts.Remove(id);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return shiftsCopy;
                }
                #endregion

                Chromosome ch = new Chromosome();
                List<Branch> branchesCopy=new List<Branch>();
                foreach(Branch originalBranch in Branches)
                {
                    
                    List<Shift> shiftsCopy = new List<Shift>();
                    foreach (Shift shift in originalBranch.Shifts) {
                        shiftsCopy.Add(shift);
                    }
                    Branch branchCopy = new Branch
                    {
                        ID = originalBranch.ID,
                        Name = originalBranch.Name,
                        Shifts = shiftsCopy
                    };
                    branchesCopy.Add(branchCopy);


                }
                
                List<Branch> shuffledBranches = branchesCopy.OrderBy(x => random.Next()).ToList();

                foreach (Branch br in shuffledBranches)
                {
                    List<Shift> filledShifts = fill_brach_shifts(br);
                    br.Shifts = filledShifts; // עדכון הסניף עם המשמרות החדשות
                    ch.Shifts.Add(br.Name, filledShifts);
                }

                calaulateChoromosomeFitness(ch);
                return ch;
            }
            #endregion

            for (int i = 0; i < ChromosomesEachGene; i++)
            {
                foreach (Employee emp in Employees)
                {
                    emp.requestedShifts.Clear();
                    foreach (int id in emp.backUprequestedShifts)
                    {
                        emp.requestedShifts.Add(id);
                    }
                }
                Chromosome c=new Chromosome();
                c = initializeChoromosome();
                pop.Chromoshomes.Add(c);
             


            }
            return pop;
        }

        public static void calaulateChoromosomeFitness(Chromosome ch)
        {
            double currentChromosomeFitness = 0;
            #region inLine

            double calculatesBranchFitness(Branch br)
            {
                double currentBranchFitness = 0;
                #region inLine
                double calculatesShiftFitness(Shift shift)
                {
                    double fitness = 0;

                    // 1. ניקוד על שיבוץ נכון של תפקידים (חישוב ישיר עם LINQ)
                    fitness += shift.RequiredRoles.Sum(role =>
                        (shift.AssignedEmployees.Count(empId => Employees.Any(e => e.ID == empId && e.Roles.Contains(role.Key))) >= role.Value ? 10 : -10)
                    );

                    // 2. ניקוד על התאמה למשמרות ולסניפים (ללא תנאים חיצוניים)
                    fitness += shift.AssignedEmployees.Sum(empId =>
                    {
                        var emp = Employees.FirstOrDefault(e => e.ID == empId);
                        return emp == null ? 0 :
                            (emp.requestedShifts.Contains(shift.Id) ? 3 : -5);
                    });

                    // 3. ניקוד על נוכחות עובד מנוסה (Mentor) - שימוש ב- `Any()` במקום `if`
                    fitness += shift.AssignedEmployees.Any(empId => Employees.Any(e => e.ID == empId && e.isMentor)) ? 5 : -5;

                    // 4. חישוב עלות שכר והפחתת ניקוד (חישוב ישיר עם LINQ)
                    fitness -= shift.AssignedEmployees.Sum(empId =>
                        Employees.FirstOrDefault(e => e.ID == empId)?.HourlySalary * Employees.FirstOrDefault(e => e.ID == empId)?.AssignedHours ?? 0) / 100.0;

                    return fitness;
                }
                #endregion
                foreach (Shift sh in br.Shifts)
                {
                    currentBranchFitness += calculatesShiftFitness(sh);
                }
                return currentBranchFitness;
            }
            #endregion
            foreach (String branchName in ch.Shifts.Keys)
            {
                Branch br = Branches.FirstOrDefault(b => b.Name == branchName);
                if (br != null)
                {
                    currentChromosomeFitness += calculatesBranchFitness(br);
                }
                currentChromosomeFitness += calculatesBranchFitness(br);
            }
            ch.Fitness = currentChromosomeFitness;
        }

        public static Chromosome GetBestChromosome()
        {
            return pop.Chromoshomes.OrderByDescending(ch => ch.Fitness).FirstOrDefault();
        }

        static void Main()
        {
            Branches = DB.addBranches();
            Employees = DB.addEmployees();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new HomePage());
        }
    }
}