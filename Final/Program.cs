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

            pop=initializeFirstPopulation();
            //for (int i = 0; i < ChromosomesEachGene; i++)
            //{
            //    pop= Crossover(pop);
            //    pop = Mutation(pop);
            //    pop.Chromoshomes = pop.Chromoshomes
            //    .OrderByDescending(c => c.Fitness)
            //    .Take(ChromosomesEachGene)
            //    .ToList();
            //}

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
            double crossoverRate = 0.8; // 80% chance of crossover
            int initialPopulationSize = pop.Chromoshomes.Count;
            int numberOfOffspringToCreate = ChromosomesEachGene; // כמה צאצאים ליצור

            // מצא את הכרומוזומים עם הכושר הגבוה ביותר
            List<Chromosome> sortedChromosomes = pop.Chromoshomes
                .OrderByDescending(c => c.Fitness)
                .ToList();

            // יצור צאצאים ולהוסיף אותם לאוכלוסייה הקיימת
            int offspringCreated = 0;

            while (offspringCreated < numberOfOffspringToCreate)
            {
                // בחר שני הורים באמצעות טורניר
                Chromosome parent1 = TournamentSelection(sortedChromosomes);
                Chromosome parent2 = TournamentSelection(sortedChromosomes);

                // החלט אם לבצע הכלאה
                if (random.NextDouble() < crossoverRate)
                {
                    // בצע הכלאה ליצירת שני צאצאים
                    Tuple<Chromosome, Chromosome> children = PerformCrossover(parent1, parent2);

                    // חשב את ערך הכושר לכרומוזומים החדשים
                    children.Item1.Fitness = calaulateChoromosomeFitness(children.Item1);
                    children.Item2.Fitness = calaulateChoromosomeFitness(children.Item2);

                    // הוסף את הצאצאים לאוכלוסייה הגלובלית
                    pop.Chromoshomes.Add(children.Item1);
                    offspringCreated++;

                    // הוסף את הצאצא השני רק אם עדיין צריך יותר כרומוזומים
                    if (offspringCreated < numberOfOffspringToCreate)
                    {
                        pop.Chromoshomes.Add(children.Item2);
                        offspringCreated++;
                    }
                }
            }

            return pop; // החזר את האוכלוסייה המעודכנת שכוללת הורים וצאצאים
        }

        // פונקציית עזר לבחירת טורניר
        private static Chromosome TournamentSelection(List<Chromosome> chromosomes)
        {
            Random random = new Random();
            int tournamentSize = 3; // בחר מתוך 3 כרומוזומים אקראיים

            List<Chromosome> tournament = new List<Chromosome>();

            // בחר כרומוזומים אקראיים לטורניר
            for (int i = 0; i < tournamentSize && i < chromosomes.Count; i++)
            {
                int index = random.Next(chromosomes.Count);
                tournament.Add(chromosomes[index]);
            }

            // החזר את הכרומוזום עם הכושר הגבוה ביותר
            return tournament.OrderByDescending(c => c.Fitness).First();
        }

        // פונקציית עזר לביצוע הכלאה
        private static Tuple<Chromosome, Chromosome> PerformCrossover(Chromosome parent1, Chromosome parent2)
        {
            Random random = new Random();

            // צור שני כרומוזומים חדשים עבור הצאצאים
            Chromosome child1 = new Chromosome() { Shifts = new Dictionary<string, List<Shift>>() };
            Chromosome child2 = new Chromosome() { Shifts = new Dictionary<string, List<Shift>>() };

            // קבל את כל שמות הסניפים משני ההורים
            HashSet<string> allBranchNames = new HashSet<string>();
            foreach (string branchName in parent1.Shifts.Keys) allBranchNames.Add(branchName);
            foreach (string branchName in parent2.Shifts.Keys) allBranchNames.Add(branchName);

            // עבור כל סניף, בחר משמרות מהורה 1 או הורה 2
            foreach (string branchName in allBranchNames)
            {
                // החלט באופן אקראי מאיזה הורה לקחת את הסניף הזה עבור כל צאצא
                bool takeFromParent1ForChild1 = random.Next(2) == 0;

                // עבור צאצא 1
                if (takeFromParent1ForChild1 && parent1.Shifts.ContainsKey(branchName))
                {
                    // קח מהורה 1
                    child1.Shifts[branchName] = DeepCopyShifts(parent1.Shifts[branchName]);
                }
                else if (parent2.Shifts.ContainsKey(branchName))
                {
                    // קח מהורה 2
                    child1.Shifts[branchName] = DeepCopyShifts(parent2.Shifts[branchName]);
                }

                // עבור צאצא 2 (קח מההורה ההפוך)
                if (!takeFromParent1ForChild1 && parent1.Shifts.ContainsKey(branchName))
                {
                    // קח מהורה 1
                    child2.Shifts[branchName] = DeepCopyShifts(parent1.Shifts[branchName]);
                }
                else if (parent2.Shifts.ContainsKey(branchName))
                {
                    // קח מהורה 2
                    child2.Shifts[branchName] = DeepCopyShifts(parent2.Shifts[branchName]);
                }
            }

            return new Tuple<Chromosome, Chromosome>(child1, child2);
        }

        // פונקציית עזר ליצירת עותק עמוק של רשימת משמרות
        private static List<Shift> DeepCopyShifts(List<Shift> originalShifts)
        {
            List<Shift> copies = new List<Shift>();

            foreach (Shift originalShift in originalShifts)
            {
                Shift shiftCopy = new Shift
                {
                    Id = originalShift.Id,
                    day = originalShift.day,
                    TimeSlot = originalShift.TimeSlot,
                    RequiredRoles = new Dictionary<string, int>(originalShift.RequiredRoles),
                    AssignedEmployees = new Dictionary<string, List<Employee>>(),
                    IsBusy = originalShift.IsBusy,
                    branch = originalShift.branch,
                    EventType = originalShift.EventType
                };

                // העתק את מבנה ה-AssignedEmployees
                foreach (var roleEntry in originalShift.AssignedEmployees)
                {
                    string role = roleEntry.Key;
                    List<Employee> employees = roleEntry.Value;

                    shiftCopy.AssignedEmployees[role] = new List<Employee>(employees);
                }

                copies.Add(shiftCopy);
            }

            return copies;
        }


        public static Population Mutation(Population pop)
        {
            Random random = new Random();
            double mutationRate = 0.2; // 20% chance of mutation

            // Create a copy of the population
            Population mutatedPopulation = new Population(new List<Chromosome>(), ChromosomesEachGene);

            foreach (Chromosome chromosome in pop.Chromoshomes)
            {
                // Decide if this chromosome should undergo mutation
                if (random.NextDouble() < mutationRate)
                {
                    // Create a deep copy of the chromosome
                    Chromosome mutatedChromosome = new Chromosome();
                    mutatedChromosome.Shifts = new Dictionary<string, List<Shift>>();

                    // Copy and potentially mutate each branch
                    foreach (var entry in chromosome.Shifts)
                    {
                        string branchName = entry.Key;
                        List<Shift> shifts = new List<Shift>();

                        // Copy all shifts
                        foreach (Shift originalShift in entry.Value)
                        {
                            Shift shiftCopy = new Shift
                            {
                                Id = originalShift.Id,
                                day = originalShift.day,
                                TimeSlot = originalShift.TimeSlot,
                                RequiredRoles = new Dictionary<string, int>(originalShift.RequiredRoles),
                                AssignedEmployees = new Dictionary<string, List<Employee>>(),
                                IsBusy = originalShift.IsBusy,
                                branch = originalShift.branch,
                                EventType = originalShift.EventType
                            };

                            // העתק את מבנה ה-AssignedEmployees
                            foreach (var roleEntry in originalShift.AssignedEmployees)
                            {
                                string role = roleEntry.Key;
                                List<Employee> employees = roleEntry.Value;

                                shiftCopy.AssignedEmployees[role] = new List<Employee>(employees);
                            }

                            shifts.Add(shiftCopy);
                        }

                        // Perform mutation: randomly select a shift
                        if (shifts.Count > 0)
                        {
                            int shiftIndex = random.Next(shifts.Count);
                            Shift selectedShift = shifts[shiftIndex];

                            // Choose mutation type
                            int mutationType = random.Next(3);

                            switch (mutationType)
                            {
                                case 0: // Add a random employee
                                    AddRandomEmployee(selectedShift);
                                    break;

                                case 1: // Remove a random employee
                                    RemoveRandomEmployee(selectedShift);
                                    break;

                                case 2: // Swap two employees
                                    SwapEmployees(shifts);
                                    break;
                            }
                        }

                        mutatedChromosome.Shifts.Add(branchName, shifts);
                    }

                    // Calculate fitness for the mutated chromosome
                    mutatedChromosome.Fitness = calaulateChoromosomeFitness(mutatedChromosome);
                    mutatedPopulation.Chromoshomes.Add(mutatedChromosome);
                }
                else
                {
                    // No mutation, just add the original chromosome
                    mutatedPopulation.Chromoshomes.Add(chromosome);
                }
            }

            return mutatedPopulation;
        }

        // פונקציות עזר למוטציות

        // הוספת עובד אקראי למשמרת
        private static void AddRandomEmployee(Shift shift)
        {
            Random random = new Random();

            // קבל את כל העובדים שכבר משובצים למשמרת
            HashSet<int> assignedEmployeeIds = new HashSet<int>();
            foreach (var employeeList in shift.AssignedEmployees.Values)
            {
                foreach (var emp in employeeList)
                {
                    assignedEmployeeIds.Add(emp.ID);
                }
            }

            // מצא עובדים שאינם משובצים למשמרת זו
            List<Employee> availableEmployees = Employees
                .Where(emp => !assignedEmployeeIds.Contains(emp.ID))
                .ToList();

            if (availableEmployees.Count > 0)
            {
                // בחר עובד אקראי
                int index = random.Next(availableEmployees.Count);
                Employee selectedEmployee = availableEmployees[index];

                // בחר תפקיד אקראי מהתפקידים הנדרשים
                List<string> availableRoles = shift.RequiredRoles.Keys.ToList();

                if (availableRoles.Count > 0)
                {
                    string selectedRole = availableRoles[random.Next(availableRoles.Count)];

                    // ודא שהרשימה עבור תפקיד זה קיימת
                    if (!shift.AssignedEmployees.ContainsKey(selectedRole))
                    {
                        shift.AssignedEmployees[selectedRole] = new List<Employee>();
                    }

                    // הוסף את העובד לתפקיד
                    shift.AssignedEmployees[selectedRole].Add(selectedEmployee);
                }
            }
        }

        // הסרת עובד אקראי ממשמרת
        private static void RemoveRandomEmployee(Shift shift)
        {
            Random random = new Random();

            // צור רשימה של כל התפקידים שיש בהם עובדים
            List<string> rolesWithEmployees = shift.AssignedEmployees
                .Where(kv => kv.Value != null && kv.Value.Count > 0)
                .Select(kv => kv.Key)
                .ToList();

            if (rolesWithEmployees.Count > 0)
            {
                // בחר תפקיד אקראי
                string selectedRole = rolesWithEmployees[random.Next(rolesWithEmployees.Count)];
                List<Employee> employees = shift.AssignedEmployees[selectedRole];

                if (employees.Count > 0)
                {
                    // בחר עובד אקראי מהתפקיד
                    int employeeIndex = random.Next(employees.Count);

                    // הסר את העובד
                    employees.RemoveAt(employeeIndex);

                    // אם הרשימה ריקה, הסר את התפקיד
                    if (employees.Count == 0)
                    {
                        shift.AssignedEmployees.Remove(selectedRole);
                    }
                }
            }
        }

        // החלפת עובדים בין שתי משמרות
        private static void SwapEmployees(List<Shift> shifts)
        {
            if (shifts.Count < 2)
                return;

            Random random = new Random();

            // בחר שתי משמרות שונות
            int shift1Index = random.Next(shifts.Count);
            int shift2Index;
            do
            {
                shift2Index = random.Next(shifts.Count);
            } while (shift1Index == shift2Index);

            Shift shift1 = shifts[shift1Index];
            Shift shift2 = shifts[shift2Index];

            // בדוק ששתי המשמרות יש להן עובדים משובצים
            bool shift1HasEmployees = shift1.AssignedEmployees.Any(kv => kv.Value != null && kv.Value.Count > 0);
            bool shift2HasEmployees = shift2.AssignedEmployees.Any(kv => kv.Value != null && kv.Value.Count > 0);

            if (!shift1HasEmployees || !shift2HasEmployees)
                return;

            // בחר תפקיד אקראי מכל משמרת
            List<string> roles1WithEmployees = shift1.AssignedEmployees
                .Where(kv => kv.Value != null && kv.Value.Count > 0)
                .Select(kv => kv.Key)
                .ToList();

            List<string> roles2WithEmployees = shift2.AssignedEmployees
                .Where(kv => kv.Value != null && kv.Value.Count > 0)
                .Select(kv => kv.Key)
                .ToList();

            string role1 = roles1WithEmployees[random.Next(roles1WithEmployees.Count)];
            string role2 = roles2WithEmployees[random.Next(roles2WithEmployees.Count)];

            // בחר עובד אקראי מכל תפקיד
            List<Employee> employees1 = shift1.AssignedEmployees[role1];
            List<Employee> employees2 = shift2.AssignedEmployees[role2];

            if (employees1.Count == 0 || employees2.Count == 0)
                return;

            int emp1Index = random.Next(employees1.Count);
            int emp2Index = random.Next(employees2.Count);

            Employee employee1 = employees1[emp1Index];
            Employee employee2 = employees2[emp2Index];

            // החלף את העובדים
            employees1.RemoveAt(emp1Index);
            employees2.RemoveAt(emp2Index);

            employees1.Add(employee2);
            employees2.Add(employee1);

            // אם אחת הרשימות התרוקנה, הסר את התפקיד מהמילון
            if (employees1.Count == 0)
            {
                shift1.AssignedEmployees.Remove(role1);
            }

            if (employees2.Count == 0)
            {
                shift2.AssignedEmployees.Remove(role2);
            }
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