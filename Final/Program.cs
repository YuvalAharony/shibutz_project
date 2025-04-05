using EmployeeSchedulingApp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;

namespace Final
{
    public class Program
    {
        #region Data
        public static List<Employee> Employees = new List<Employee>();
        public static List<Branch> Branches = new List<Branch>();
        public static DB myDB = new DB();
        public const int ChromosomesEachGene = 100;

        public static Population pop = new Population(new List<Chromosome>(), ChromosomesEachGene);
        #endregion

        public static void createSceduele()
        {
            pop.Chromoshomes.Clear();
        

            pop = initializeFirstPopulation(pop);
     

        }

        public static Population initializeFirstPopulation(Population pop)
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

                    List<Shift> shuffledShifts = br.Shifts.OrderBy(x => random.Next()).ToList();


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
                               
                                        if (!sh.AssignedEmployees.ContainsKey(entry.Key))
                                        {
                                            sh.AssignedEmployees[entry.Key] = new List<Employee>();
                                        }
                                        sh.AssignedEmployees[entry.Key].Add(selectedEmployee); 
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
                    return br.Shifts;
                }
                #endregion

                Chromosome ch = new Chromosome();
                List<Branch> branchesCopy = new List<Branch>();
                foreach (Branch originalBranch in Branches)
                {

                    List<Shift> shiftsCopy = new List<Shift>();
                    foreach (Shift originalShift in originalBranch.Shifts)
                    {
                        Shift shiftCopy = new Shift
                        {
                            Id = originalShift.Id,
                            day = originalShift.day,
                            TimeSlot = originalShift.TimeSlot,
                            RequiredRoles = new Dictionary<string, int>(originalShift.RequiredRoles),
                            AssignedEmployees = new Dictionary<string, List<Employee>>()
                        };
                        shiftsCopy.Add(shiftCopy); 
                    }
                    
                    Branch branchCopy = new Branch
                    {
                        ID = originalBranch.ID,
                        Name = originalBranch.Name,
                        Shifts= shiftsCopy
                    };
                    branchesCopy.Add(branchCopy);


                }

                List<Branch> shuffledBranches = branchesCopy.OrderBy(x => random.Next()).ToList();

                foreach (Branch br in shuffledBranches)
                {
                    br.Shifts = fill_brach_shifts(br);
                    ch.Shifts.Add(br.Name, br.Shifts);
                }

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
                Chromosome c = new Chromosome();
                c = initializeChoromosome();
                pop.Chromoshomes.Add(c);
                c.Fitness=calaulateChoromosomeFitness(c);



            }
            return pop;
        }



        public static double calaulateChoromosomeFitness(Chromosome ch)
        {
            double currentChromosomeFitness = 0;
            #region inLine
            double calculatesShiftFitness(Shift shift)
            {
                double fitness = 0;

                // ודא שהמילון קיים
                if (shift.AssignedEmployees == null)
                {
                    shift.AssignedEmployees = new Dictionary<string, List<Employee>>();
                }

                // 1. ניקוד על שיבוץ נכון של תפקידים
                foreach (var roleEntry in shift.RequiredRoles)
                {
                    string role = roleEntry.Key;
                    int requiredCount = roleEntry.Value;

                    // בדוק אם התפקיד קיים במילון ויש מספיק עובדים
                    if (shift.AssignedEmployees.ContainsKey(role) &&
                        shift.AssignedEmployees[role] != null &&
                        shift.AssignedEmployees[role].Count >= requiredCount)
                    {
                        fitness += 10;
                    }
                    else
                    {
                        fitness -= 10;
                    }
                }

                // 2. ניקוד על התאמה למשמרות ולסניפים
                foreach (var roleEntry in shift.AssignedEmployees)
                {
                    string role = roleEntry.Key;
                    List<Employee> employees = roleEntry.Value;

                    if (employees != null)
                    {
                        foreach (var emp in employees)
                        {
                            if (emp != null && emp.backUprequestedShifts != null &&
                                emp.backUprequestedShifts.Contains(shift.Id))
                            {
                                fitness += 3;
                            }
                            else
                            {
                                fitness -= 5;
                            }
                        }
                    }
                }

                // 3. ניקוד על נוכחות עובד מנוסה (Mentor)
                bool hasMentor = false;
                foreach (var employeeList in shift.AssignedEmployees.Values)
                {
                    if (employeeList != null && employeeList.Any(emp => emp != null && emp.isMentor))
                    {
                        hasMentor = true;
                        break;
                    }
                }
                fitness += hasMentor ? 5 : -5;

                // 4. חישוב עלות שכר והפחתת ניקוד
                double salaryExpense = 0;
                foreach (var employeeList in shift.AssignedEmployees.Values)
                {
                    if (employeeList != null)
                    {
                        foreach (var emp in employeeList)
                        {
                            if (emp != null)
                            {
                                salaryExpense += (emp.HourlySalary * emp.AssignedHours) / 100.0;
                            }
                        }
                    }
                }
                fitness -= salaryExpense;

                return fitness;
            }
            #endregion

            // חישוב הפיטנס לכל המשמרות בכל הסניפים
            if (ch.Shifts != null)
            {
                foreach (var branchEntry in ch.Shifts)
                {
                    string branchName = branchEntry.Key;
                    List<Shift> branchShifts = branchEntry.Value;

                    if (branchShifts != null)
                    {
                        foreach (var shift in branchShifts)
                        {
                            if (shift != null)
                            {
                                // ודא שהמילון קיים לפני החישוב
                                if (shift.AssignedEmployees == null)
                                {
                                    shift.AssignedEmployees = new Dictionary<string, List<Employee>>();
                                }

                                // חשב את הפיטנס רק למשמרות שיש להן עובדים משובצים
                                if (shift.AssignedEmployees.Count > 0)
                                {
                                    currentChromosomeFitness += calculatesShiftFitness(shift);
                                }
                            }
                        }
                    }
                }
            }

            // בונוס/קנס עבור אילוצים ברמת הכרומוזום
            int unassignedShifts = 0;

            if (ch.Shifts != null)
            {
                foreach (var branchEntry in ch.Shifts)
                {
                    List<Shift> branchShifts = branchEntry.Value;

                    if (branchShifts != null)
                    {
                        foreach (var shift in branchShifts)
                        {
                            if (shift != null && shift.RequiredRoles != null)
                            {
                                bool allRolesCovered = true;

                                foreach (var roleEntry in shift.RequiredRoles)
                                {
                                    string role = roleEntry.Key;
                                    int requiredCount = roleEntry.Value;

                                    if (shift.AssignedEmployees == null ||
                                        !shift.AssignedEmployees.ContainsKey(role) ||
                                        shift.AssignedEmployees[role] == null ||
                                        shift.AssignedEmployees[role].Count < requiredCount)
                                    {
                                        allRolesCovered = false;
                                        break;
                                    }
                                }

                                if (!allRolesCovered)
                                {
                                    unassignedShifts++;
                                }
                            }
                        }
                    }
                }
            }

            currentChromosomeFitness -= unassignedShifts * 20; // קנס עבור כל משמרת לא מכוסה

            // אפשרות להוסיף מדדי פיטנס נוספים ברמת הכרומוזום
            // למשל, בדיקה שעובד לא עובד יותר מדי משמרות ברצף

            return currentChromosomeFitness;
        }
        
        public static Chromosome GetBestChromosome()
        {
            return pop.Chromoshomes.OrderByDescending(ch => ch.Fitness).FirstOrDefault();
        }

        public static Population Crossover(Population pop)
        {
            
            Random random = new Random();
            #region inLine
            Chromosome createSonFromTwoParents(Chromosome ch1,Chromosome ch2)
            {
               

                // Create a new chromosome for the offspring
                Chromosome child = new Chromosome();
                child.Shifts = new Dictionary<string, List<Shift>>();

                // Get the union of branch names from both parents
                HashSet<string> allBranchNames = new HashSet<string>();
                foreach (string branchName in ch1.Shifts.Keys) allBranchNames.Add(branchName);
                foreach (string branchName in ch2.Shifts.Keys) allBranchNames.Add(branchName);

                // For each branch, randomly choose shifts from either parent
                foreach (string branchName in allBranchNames)
                {
                    child.Shifts[branchName] = new List<Shift>();

                    // Get the shifts for this branch from both parents
                    List<Shift> shiftsParent1 = ch1.Shifts.ContainsKey(branchName) ? ch1.Shifts[branchName] : new List<Shift>();
                    List<Shift> shiftsParent2 = ch2.Shifts.ContainsKey(branchName) ? ch2.Shifts[branchName] : new List<Shift>();

                    // Create a set of all shift IDs
                    HashSet<int> allShiftIds = new HashSet<int>();
                    foreach (Shift shift in shiftsParent1) allShiftIds.Add(shift.Id);
                    foreach (Shift shift in shiftsParent2) allShiftIds.Add(shift.Id);

                    // For each shift ID, choose parent to take it from
                    foreach (int shiftId in allShiftIds)
                    {
                        // Find the shift in both parents
                        Shift shiftParent1 = shiftsParent1.FirstOrDefault(s => s.Id == shiftId);
                        Shift shiftParent2 = shiftsParent2.FirstOrDefault(s => s.Id == shiftId);

                        // Choose which parent to take from
                        bool takeFromParent1 = random.Next(2) == 0;

                        Shift sourceShift = null;
                        if (takeFromParent1 && shiftParent1 != null)
                        {
                            sourceShift = shiftParent1;
                        }
                        else if (shiftParent2 != null)
                        {
                            sourceShift = shiftParent2;
                        }
                        else if (shiftParent1 != null)
                        {
                            sourceShift = shiftParent1;
                        }

                        if (sourceShift != null)
                        {
                            // Create a deep copy of the shift
                            Shift childShift = new Shift();
                            childShift.Id = sourceShift.Id;
                            childShift.day = sourceShift.day;
                            childShift.TimeSlot = sourceShift.TimeSlot;
                            childShift.branch = sourceShift.branch;
                            childShift.IsBusy = sourceShift.IsBusy;
                            childShift.EventType = sourceShift.EventType;

                            // Copy the required roles
                            childShift.RequiredRoles = new Dictionary<string, int>();
                            foreach (var roleEntry in sourceShift.RequiredRoles)
                            {
                                childShift.RequiredRoles[roleEntry.Key] = roleEntry.Value;
                            }

                            // Copy the assigned employees
                            childShift.AssignedEmployees = new Dictionary<string, List<Employee>>();
                            foreach (var roleEntry in sourceShift.AssignedEmployees)
                            {
                                string role = roleEntry.Key;
                                List<Employee> employees = roleEntry.Value;

                                // Create a new list for the role
                                childShift.AssignedEmployees[role] = new List<Employee>();

                                // Copy all employees (references only, not deep copies)
                                foreach (Employee emp in employees)
                                {
                                    childShift.AssignedEmployees[role].Add(emp);
                                }
                            }

                            child.Shifts[branchName].Add(childShift);
                        }
                    }
                }

                // Calculate fitness for the child
                child.Fitness = calaulateChoromosomeFitness(child);

                return child;
            }
            #endregion
           
            pop.Chromoshomes.OrderBy(x => random.Next()).ToList();
            int chromosomesPreviousGene = pop.Chromoshomes.Count;
            for (int i = 0; i < chromosomesPreviousGene; i+=2) {
                pop.Chromoshomes.Add(createSonFromTwoParents(pop.Chromoshomes[i], pop.Chromoshomes[i+1]));
            }
            return pop;
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