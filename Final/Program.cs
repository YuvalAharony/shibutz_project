using EmployeeSchedulingApp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;

namespace Final
{
    public class Program
    {
        //פתיחת החיבור לבסיס הנתונים
        private static readonly string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True";

        //הכרזה על משתנים גלובליים לתוכנית
        #region Data
        public static  Random random = new Random();
        public static List<Employee> Employees=new List<Employee>();
        public static DataBaseHelper DataBaseHelper=new DataBaseHelper();
        public static List<Branch> Branches=new List<Branch>();
        public const int ChromosomesEachGene = 200;
        public const int Genes = 200;
        public const int hoursPerWeek = 42;
        public const int hoursPerDay = 9;
        public const int hoursPerShift = 9;



        public static int count1 = 0;
        public static int count2 = 0; 
       

        public static Population pop;
        #endregion

       

        //אלגוריתם ראשי למציאת סידור עבודה אופטימלי ע"י אלגוריתם גנטי
        public static void createSceduele(string username)
        {
            //יצירת אוכלוסייה חדשה בכל פעם שמפעילים את האלגוריתם
            pop = new Population(new List<Chromosome>(), ChromosomesEachGene);
            //טעינת כל הנתונים של המשתמש המחובר
            DataBaseHelper.LoadDataForUser(username, Branches, Employees);
            //יצירת אוכלוסייה ראשונית- שלב 1 באלגוריתם הגנטי 
            pop = initializeFirstPopulation(pop);
            //לולאה הרצה לפי מספר הדורות הנקבע ויוצרת דור חדש של צאצאים
            for (int i = 0; i < Genes; i++)
            {
                //שיפור הכרומוזומים ע"י הכלאה בין זוגות כרומוזומים
                crossover(pop);
                //שיפור הכרומוזומים ע"י מוטציות בין זוגות כרומוזומים
                Mutation(pop);
                //מיון האוכלוסייה החדשה ושמירת מספר מסוים(קבוע שהוחלט)
                //של הכרומוזומים הטובים ביותר שנוצרו בכל הדורות עד כה
                pop.Chromoshomes = pop.Chromoshomes.OrderByDescending(x => x.Fitness).Take(ChromosomesEachGene).ToList();
            }
            //הדפסת הודעה למשתמש בסיום האלגוריתם שסידור העבודה נוצר בהצלחה

            MessageBox.Show("נוצר בהצלחה", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        //פונקציה היוצרת אוכלוסייה ראשונית- שלב ראשון באלגוריתם הגנטי
        public static Population initializeFirstPopulation(Population pop)
        {
            //ניצור כרומוזום בעבור כמות הכרומוזומים הרמויה בכל דור
            for (int i = 0; i < ChromosomesEachGene; i++)
            {
                //שחזור המשמרות של כל עובד בשביל יצירת הכרומוזום הבא
                RestoreEmployeesRequestedShifts();
                //אתחול כרומוזום חדש
                Chromosome c;
                //יצירת הכרומוזום
                c = initializeChoromosome();
                //הוספת הכרומוזום לאוכלוסייה
                pop.Chromoshomes.Add(c);
                //קביעת ציון הכושר של הכרומוזום
                c.Fitness = CalculateChromosomeFitness(c);
            }
            return pop;
        }

        #region InitializeFirstPopulation Helper Functions
        //פונקציה שמשחזרת את המשמרות המבוקשות של כל עובד
        public static void RestoreEmployeesRequestedShifts()
        {
            
            foreach (Employee emp in Employees)//מעבר על רשימת העובדים
            {
                emp.requestedShifts.Clear();//ניקוי המשמרות המבוקשות(כדיי שלא יהיו כפילויות)
                foreach (int id in emp.backUprequestedShifts)//מעבר על גיבוי המשמרות המבוקשות
                {
                    emp.requestedShifts.Add(id);//הוספת המשמרות מהגיבוי למשמרות המבוקשות
                }
            }
        }

        //פונקציה שמטרתה ליצור כרווזום חדש
        private static Chromosome initializeChoromosome()
        {
          
            //אתחול כרומוזום חדש
            Chromosome ch = new Chromosome();

            //יצירת העתק של הסניפים והמשמרות
            #region CopyOfBranchesAndShifts
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
                    Shifts = shiftsCopy
                };
                branchesCopy.Add(branchCopy);
            }
            #endregion

            //ערבוב הסניפים במטרה ליצור גיוון בין כרומוזום לכרומוזום
            List<Branch> shuffledBranches = branchesCopy.OrderBy(x => random.Next()).ToList();

            //מעבר על הסניפים
            foreach (Branch br in shuffledBranches)
            {
                //מילוי המשמרות של הסניף 
                br.Shifts = fill_brach_shifts(br);
                //הוספת הסניף והמשמרות שלו למילון המשמרות של הכרומוזום
                ch.Shifts.Add(br.Name, br.Shifts);
            }

            return ch;
        }

        //פונקציה הממינת את העובדים לפי הזמינות שלהם- הכי פחות זמינים בהתחלה
        public static List<Employee> sort_employees_by_availabilty(List<Employee> employees)
        {
            return employees.OrderBy(e => e.requestedShifts?.Count ?? 0).ToList();
        }

        //פונקצייה הממינת את העובדים לפי הציון שלהם
        public static List<Employee> sort_employees_by_rate(List<Employee> employees)
        {
            return employees.OrderByDescending(e => e.Rate).ToList();
        }

        //פונקציה הממפה את העובדים לפי משמרות מבוקשות
        public static Dictionary<int, List<Employee>> mappingEmployeesByRequestedShifts()
        {
            return Employees
                .SelectMany(emp => emp.requestedShifts, (emp, shiftId) => new { shiftId, emp })
                .GroupBy(entry => entry.shiftId)
                .ToDictionary(group => group.Key, group => group.Select(entry => entry.emp).ToList());
        }
        
        //פונקציה הממפה את העובדים לפי התפקיד שלהם
        public static Dictionary<string, List<Employee>> mappingEmployeesByRole()
        {
            return Employees
                .SelectMany(emp => emp.roles, (emp, role) => new { role, emp })
                .GroupBy(entry => entry.role)
                .ToDictionary(group => group.Key, group => group.Select(entry => entry.emp).ToList());
        }

        //פונקציה המחזירה רשימת משמרות חופפות בהתאם לעובד ולמשמרת ששובץ בה
        public static List<int> UpdateOverlappingShifts(Employee employee, Shift assignedShift)
        {
            //בדיקה שיש עובד ומשמרת
            if (employee == null || assignedShift == null)
                return null;

            List<int> idsToRemove = new List<int>();

            //מעבר על המשמרות המבוקשות של העובד שמטפלים בו
            foreach (int shiftId in employee.requestedShifts)
            {
                //קבלת המשמרת לפי המזהה שלה
                Shift shift = FindShiftById(shiftId);
                //בדיקה אם המשמרת חופפת למשמרת שהעובד שובץ בה
                if (shift != null &&
                    shift.day == assignedShift.day &&
                    shift.TimeSlot == assignedShift.TimeSlot)
                {
                    //הוספת המשמרת לרשימת המשמרות שיש להסיר במידה והיא חופפת
                    idsToRemove.Add(shiftId);
                }
            }
            return idsToRemove;
        }

        //פונקציה המקבלת מזהה משמרת ומחזירה את המשמרת
        //(UpdateOverlappingShifts פונקציית עזר ל)
        public static Shift FindShiftById(int shiftId)
        {
            //מעבר על כל המשמרות בכל הסניפים והחזרת המשמרת אם המזהים חופפים
            foreach (Branch branch in Program.Branches)
            {
                foreach (Shift shift in branch.Shifts)
                {
                    if (shift.Id == shiftId)
                    {
                        return shift;
                    }
                }
            }
            return null;
        }
        //מילוי משמרות של סניף מסוים בעובדים
        public static List<Shift> fill_brach_shifts(Branch br)
        {   
            //מיפוי עובדים לפי משמרות מבוקשות
            Dictionary<int, List<Employee>> employeesMappedByRequestedShifts = mappingEmployeesByRequestedShifts();
            //מיפוי עובדים לפי תפקיד
            Dictionary<string, List<Employee>> employeesMappedByRequestedRoles = mappingEmployeesByRole();
            //מיון עובדים לפי ציון
            List<Employee> employeesSortedByRate = sort_employees_by_rate(Employees);
           
            //ערבוב משמרות הסניף על מנת ליצור גיוון בין הכרומזומים
            List<Shift> shuffledShifts = br.Shifts.OrderBy(x => random.Next()).ToList();
            //ערבוב המשמרות על מנת ליצור גיוון בין כרומוזום לכרומזום
            foreach (Shift sh in shuffledShifts)
            {
                //מיון העובדים לפי זמינות
                List<Employee> employeesSortedByavailabilty = sort_employees_by_availabilty(Employees);
                //אתחול הרשימה שאיתה נעבוד בשיבוץ הנוכחי
                List<Employee> currenList = new List<Employee>();
                //הבאה מהמילון את רשימת העובדים שיכולים לעבוד במשמרת הנוכחית
                List<Employee> employeesAvaliableForShift;
                employeesMappedByRequestedShifts.TryGetValue(sh.Id, out employeesAvaliableForShift);
                //מעבר על כל התפקידים המבוקשים במשמרת הנוכחית
                foreach (var entry in sh.RequiredRoles)
                {
                    //מעבר על התפקיד הנוכחי לפי הכמות הנדררשת ממנו
                    for (int i = 0; i < entry.Value; i++)
                    {
                        //הבאה מהמילון את העובדים המתאימים לתפקיד הנוכחי
                        employeesMappedByRequestedRoles.TryGetValue(entry.Key, out List<Employee> employeesAvaliableForRole);
                        //(הגרלת מספר בין 1 ל2 שתשמש לבחירת הרשימה שדרכה נבחר עובד( לפי ציון או לפי זמינות 
                        int currentListIdentifier = random.Next(1, 3);
                        //החירת הרשימה: 1-לפי ציון, 2-לפי זמינות
                        switch (currentListIdentifier)
                        {
                            case (1):
                                currenList = employeesSortedByRate;
                                break;
                            case (2):
                                currenList = employeesSortedByavailabilty;
                                break;
                        }
                        //בדיקה אם יש עובד לשיבוץ במשמרת
                        if (employeesAvaliableForShift != null)
                        {
                            //בחירת העובד הראשון מהרשימה שנבחרה שזמין למשמרת הנוכחית
                            Employee selectedEmployee = currenList.FirstOrDefault(emp =>
                            employeesAvaliableForShift.Contains(emp) && employeesAvaliableForRole.Contains(emp));
                            //בדיקה אם נמצא עובד
                            if (selectedEmployee != null)
                            {
                                //בדיקה אם כבר שובץ עובד לתפקיד הנוכחי
                                if (!sh.AssignedEmployees.ContainsKey(entry.Key))
                                {
                                    //הוספת התפקיד הנוכחי למילון העובדים ששמשובצים למשמרת הנוכחית
                                    sh.AssignedEmployees[entry.Key] = new List<Employee>();
                                }
                                //הוספת העובד למשמרת לתפקיד המתאים
                                sh.AssignedEmployees[entry.Key].Add(selectedEmployee);
                                //קבלת מזהי המשמרות החופפות שיש להסיר מהמשמרות המבוקשות של העובד הנוכחי
                                List<int> idToRemove = UpdateOverlappingShifts(selectedEmployee, sh);
                                //מעבר על המזהים הללו
                                foreach (int id in idToRemove)
                                {
                                    //(קבלת רשימת העובדים הזמינים למשמרת(אם יש
                                    if (employeesMappedByRequestedShifts.TryGetValue(id, out List<Employee> shiftToRemove))
                                    {
                                        //הורדת העובד מרשימה זו
                                        shiftToRemove.Remove(selectedEmployee);
                                        //מחיקת רשימת העובדים הזמינים למשמרת זו מהמילון אם לא נותרו עובדים זמינים
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

        #region Crrosover

        //פונקציה היוצרת כרומוזומים חדשים בעזרת הכלאה
        public static void crossover(Population pop)
        {
            //אתחול רשימת הכרומוזומים החדשים
            List<Chromosome> newOffspring = new List<Chromosome>();
            int desiredOffspringCount = ChromosomesEachGene * 3 / 4;

            // Get the best chromosome for elitism
            var sortedChromosomes = pop.Chromoshomes.OrderByDescending(ch => ch.Fitness).ToList();
            Chromosome bestChromosome = null;
            if (sortedChromosomes.Count > 0)
                bestChromosome = CopyChromosome(sortedChromosomes[0]);

            for (int i = 0; i < desiredOffspringCount; i++)
            {
                // Parent selection via tournament
                Chromosome parent1 = SelectParentByTournament(pop.Chromoshomes, random);
                Chromosome parent2 = SelectParentByTournament(pop.Chromoshomes, random);

                // Try to make sure parents are different
                int attempts = 0;
                while (parent1 == parent2 && attempts < 3 && pop.Chromoshomes.Count > 1)
                {
                    parent2 = SelectParentByTournament(pop.Chromoshomes, random);
                    attempts++;
                }

                // Choose crossover method based on randomness
                Chromosome offspring;                
                offspring = PerformCrossover(parent1, parent2, random);
                    
                // Calculate fitness for new offspring
                offspring.Fitness = CalculateChromosomeFitness(offspring);
                newOffspring.Add(offspring);
            }

            // Add new offspring to population
            pop.Chromoshomes.AddRange(newOffspring);

            // Select best chromosomes for next generation
            pop.Chromoshomes = pop.Chromoshomes
                .OrderByDescending(ch => ch.Fitness)
                .Take(ChromosomesEachGene - 1) // Leave room for elite
                .ToList();

            // Add the best chromosome back (elitism)
            if (bestChromosome != null)
                pop.Chromoshomes.Add(bestChromosome);
        }

        private static Chromosome SelectParentByTournament(List<Chromosome> chromosomes, Random random)
        {
            // Tournament selection with 3 random candidates
            Chromosome best = null;
            double bestFitness = double.MinValue;

            // Handle empty list
            if (chromosomes.Count == 0)
                return null;


            // Select 3 random candidates and pick the best
            for (int i = 0; i < 3; i++)
            {
                int idx = random.Next(chromosomes.Count);
                Chromosome candidate = chromosomes[idx];

                if (best == null || candidate.Fitness > bestFitness)
                {
                    best = candidate;
                    bestFitness = candidate.Fitness;
                }
            }

            return best;
        }

        //private static Chromosome PerformSimpleCrossover(Chromosome parent1, Chromosome parent2, Random random)
        //{
        //    Chromosome offspring = new Chromosome();
        //    offspring.Shifts = new Dictionary<string, List<Shift>>();

        //    // Track employee assignments to prevent conflicts
        //    Dictionary<Employee, HashSet<string>> employeeAssignments = new Dictionary<Employee, HashSet<string>>();

        //    // Get all branch names from both parents
        //    var allBranchNames = new HashSet<string>(parent1.Shifts.Keys.Concat(parent2.Shifts.Keys));

        //    foreach (string branchName in allBranchNames)
        //    {
        //        // If branch exists in both parents, create a map of all day+timeslot combinations
        //        List<Shift> offspringShifts = new List<Shift>();
        //        var shiftSlots = new Dictionary<string, List<Shift>>();

        //        // Collect shifts from both parents into shift slots
        //        foreach (var shift in parent1.Shifts[branchName].Concat(parent2.Shifts[branchName]))
        //        {
        //            string key = $"{shift.day}_{shift.TimeSlot}";

        //            if (!shiftSlots.ContainsKey(key))
        //                shiftSlots[key] = new List<Shift>();

        //            shiftSlots[key].Add(shift);
        //        }

        //        // For each slot, randomly select a shift from available options
        //        foreach (var entry in shiftSlots)
        //        {
        //            // Get random shift for this slot
        //            Shift selectedShift = DeepCopyShift(entry.Value[random.Next(entry.Value.Count)]);

        //            // Ensure no employee conflicts
        //            string shiftKey = $"{selectedShift.day}_{selectedShift.TimeSlot}";
        //            RemoveConflictingEmployees(selectedShift, shiftKey, employeeAssignments);

        //            offspringShifts.Add(selectedShift);
        //        }

        //        offspring.Shifts[branchName] = offspringShifts;
        //    }

        //    return offspring;
        //}

        //private static Chromosome PerformDayBasedCrossover(Chromosome parent1, Chromosome parent2, Random random)
        //{
        //    Chromosome offspring = new Chromosome();
        //    offspring.Shifts = new Dictionary<string, List<Shift>>();

        //    // Track employee assignments to prevent conflicts
        //    Dictionary<Employee, HashSet<string>> employeeAssignments = new Dictionary<Employee, HashSet<string>>();

        //    // Common branches in both parents
        //    var commonBranches = parent1.Shifts.Keys.Intersect(parent2.Shifts.Keys).ToList();

        //    // Process common branches with day-based crossover
        //    foreach (string branchName in commonBranches)
        //    {
        //        List<Shift> offspringShifts = new List<Shift>();

        //        // Group shifts by day
        //        var dayGroups = new Dictionary<string, List<Tuple<Shift, Shift>>>();

        //        // Map shifts from parent1
        //        Dictionary<string, Shift> shiftsMap1 = parent1.Shifts[branchName]
        //            .ToDictionary(s => $"{s.day}_{s.TimeSlot}", s => s);

        //        // Map shifts from parent2
        //        Dictionary<string, Shift> shiftsMap2 = parent2.Shifts[branchName]
        //            .ToDictionary(s => $"{s.day}_{s.TimeSlot}", s => s);

        //        // Get all unique days from both parents
        //        var allDays = new HashSet<string>();
        //        foreach (var shift in parent1.Shifts[branchName]) allDays.Add(shift.day);
        //        foreach (var shift in parent2.Shifts[branchName]) allDays.Add(shift.day);

        //        // For each day, randomly choose a parent
        //        foreach (string day in allDays)
        //        {
        //            bool useParent1 = random.Next(2) == 0;

        //            // Get all shifts for this day
        //            var shiftsForDay1 = parent1.Shifts[branchName].Where(s => s.day == day).ToList();
        //            var shiftsForDay2 = parent2.Shifts[branchName].Where(s => s.day == day).ToList();

        //            // Select shifts based on chosen parent
        //            List<Shift> selectedShifts;
        //            if (useParent1)
        //                selectedShifts = shiftsForDay1.Count > 0 ? shiftsForDay1 : shiftsForDay2;
        //            else
        //                selectedShifts = shiftsForDay2.Count > 0 ? shiftsForDay2 : shiftsForDay1;

        //            // Create deep copies and add to offspring
        //            foreach (Shift shift in selectedShifts)
        //            {
        //                Shift newShift = DeepCopyShift(shift);
        //                string shiftKey = $"{newShift.day}_{newShift.TimeSlot}";
        //                RemoveConflictingEmployees(newShift, shiftKey, employeeAssignments);
        //                offspringShifts.Add(newShift);
        //            }
        //        }

        //        offspring.Shifts[branchName] = offspringShifts;
        //    }

        //    // Add branches unique to each parent
        //    foreach (string branchName in parent1.Shifts.Keys.Except(commonBranches))
        //    {
        //        offspring.Shifts[branchName] = DeepCopyShifts(parent1.Shifts[branchName]);
        //        UpdateEmployeeAssignments(offspring.Shifts[branchName], employeeAssignments);
        //    }

        //    foreach (string branchName in parent2.Shifts.Keys.Except(commonBranches))
        //    {
        //        offspring.Shifts[branchName] = DeepCopyShifts(parent2.Shifts[branchName]);
        //        UpdateEmployeeAssignments(offspring.Shifts[branchName], employeeAssignments);
        //    }

        //    return offspring;
        //}

        private static Chromosome PerformCrossover(Chromosome parent1, Chromosome parent2, Random random)
        {
            Chromosome offspring = new Chromosome();
            offspring.Shifts = new Dictionary<string, List<Shift>>();

            // Track employee assignments to prevent conflicts
            Dictionary<Employee, HashSet<string>> employeeAssignments = new Dictionary<Employee, HashSet<string>>();

            // Get all branch names from both parents
            var allBranchNames = new HashSet<string>(parent1.Shifts.Keys);

            foreach (string branchName in allBranchNames)
            {
                // Create map of shifts by day+timeslot for each parent
                Dictionary<string, Shift> shiftsMap1 = parent1.Shifts[branchName]
                    .ToDictionary(s => $"{s.day}_{s.TimeSlot}", s => s);

                Dictionary<string, Shift> shiftsMap2 = parent2.Shifts[branchName]
                    .ToDictionary(s => $"{s.day}_{s.TimeSlot}", s => s);

                // Get all shift slots
                var allSlots = new HashSet<string>(shiftsMap1.Keys.Concat(shiftsMap2.Keys));

                List<Shift> offspringShifts = new List<Shift>();

                // For each unique slot, create a new shift with basic structure
                foreach (string slot in allSlots)
                {
                    string[] parts = slot.Split('_');
                    string day = parts[0];
                    string timeSlot = parts[1];

                    Shift shift1 = shiftsMap1[slot];
                    Shift shift2 =  shiftsMap2[slot];

                    // Both parents have this shift - create new shift with base properties from shift1
                    Shift offspringShift = new Shift
                    {
                        Id = shift1.Id,
                        branch = shift1.branch,
                        day = shift1.day,
                        TimeSlot = shift1.TimeSlot,
                        IsBusy = shift1.IsBusy,
                        EventType = shift1.EventType,
                        RequiredRoles = new Dictionary<string, int>(shift1.RequiredRoles),
                        AssignedEmployees = new Dictionary<string, List<Employee>>()
                    };

                    // For each role, randomly choose employees from either parent
                    var allRoles = new HashSet<string>(
                        shift1.AssignedEmployees.Keys.Concat(shift2.AssignedEmployees.Keys));

                    foreach (string role in allRoles)
                    {
                        offspringShift.AssignedEmployees[role] = new List<Employee>();

                        List<Employee> employees1 = shift1.AssignedEmployees.ContainsKey(role) ?
                            shift1.AssignedEmployees[role] : new List<Employee>();

                        List<Employee> employees2 = shift2.AssignedEmployees.ContainsKey(role) ?
                            shift2.AssignedEmployees[role] : new List<Employee>();

                        // Select randomly which parent to use for this role
                        List<Employee> selectedEmployees = random.Next(2) == 0 ?
                            new List<Employee>(employees1) : new List<Employee>(employees2);

                        // Add selected employees, avoiding conflicts
                        string shiftKey = $"{offspringShift.day}_{offspringShift.TimeSlot}";

                        foreach (Employee emp in selectedEmployees)
                        {
                            if (!IsEmployeeAlreadyAssigned(emp, shiftKey, employeeAssignments))
                            {
                                offspringShift.AssignedEmployees[role].Add(emp);

                                if (!employeeAssignments.ContainsKey(emp))
                                    employeeAssignments[emp] = new HashSet<string>();

                                employeeAssignments[emp].Add(shiftKey);
                            }
                        }
                    }

                    offspringShifts.Add(offspringShift);
                }

                offspring.Shifts[branchName] = offspringShifts;
            }

            return offspring;
        }

        //private static void UpdateEmployeeAssignments(List<Shift> shifts, Dictionary<Employee, HashSet<string>> employeeAssignments)
        //{
        //    foreach (Shift shift in shifts)
        //    {
        //        string shiftKey = $"{shift.day}_{shift.TimeSlot}";

        //        if (shift.AssignedEmployees == null)
        //            continue;

        //        foreach (var roleEntry in shift.AssignedEmployees)
        //        {
        //            foreach (Employee employee in roleEntry.Value)
        //            {
        //                if (!employeeAssignments.ContainsKey(employee))
        //                    employeeAssignments[employee] = new HashSet<string>();

        //                employeeAssignments[employee].Add(shiftKey);
        //            }
        //        }
        //    }
        //}

        private static bool IsEmployeeAlreadyAssigned(Employee employee, string shiftKey, Dictionary<Employee, HashSet<string>> employeeAssignments)
        {
            return employeeAssignments.ContainsKey(employee) &&
                   employeeAssignments[employee].Contains(shiftKey);
        }

        //private static void RemoveConflictingEmployees(Shift shift, string shiftKey, Dictionary<Employee, HashSet<string>> employeeAssignments)
        //{
        //    if (shift.AssignedEmployees == null)
        //        return;

        //    foreach (var roleEntry in shift.AssignedEmployees.ToList())
        //    {
        //        string role = roleEntry.Key;
        //        List<Employee> employees = roleEntry.Value;
        //        List<Employee> keptEmployees = new List<Employee>();

        //        foreach (Employee employee in employees)
        //        {
        //            if (!IsEmployeeAlreadyAssigned(employee, shiftKey, employeeAssignments))
        //            {
        //                keptEmployees.Add(employee);

        //                if (!employeeAssignments.ContainsKey(employee))
        //                    employeeAssignments[employee] = new HashSet<string>();

        //                employeeAssignments[employee].Add(shiftKey);
        //            }
        //        }

        //        shift.AssignedEmployees[role] = keptEmployees;
        //    }
        //}

        private static Shift CopyShift(Shift originalShift)
        {
            if (originalShift == null)
                return null;

            Shift copy = new Shift
            {
                Id = originalShift.Id,
                branch = originalShift.branch,
                day = originalShift.day,
                TimeSlot = originalShift.TimeSlot,
                IsBusy = originalShift.IsBusy,
                EventType = originalShift.EventType,
                RequiredRoles = new Dictionary<string, int>(),
                AssignedEmployees = new Dictionary<string, List<Employee>>()
            };

            // Copy required roles
            if (originalShift.RequiredRoles != null)
            {
                foreach (var entry in originalShift.RequiredRoles)
                {
                    copy.RequiredRoles[entry.Key] = entry.Value;
                }
            }

            // Copy assigned employees
            if (originalShift.AssignedEmployees != null)
            {
                foreach (var roleEntry in originalShift.AssignedEmployees)
                {
                    string role = roleEntry.Key;
                    copy.AssignedEmployees[role] = new List<Employee>();

                    if (roleEntry.Value != null)
                    {
                        foreach (Employee emp in roleEntry.Value)
                        {
                            if (emp != null)
                                copy.AssignedEmployees[role].Add(emp);
                        }
                    }
                }
            }

            return copy;
        }

        private static List<Shift> CopyShifts(List<Shift> shifts)
        {
            if (shifts == null)
                return new List<Shift>();

            List<Shift> copies = new List<Shift>();

            foreach (var shift in shifts)
            {
                if (shift != null)
                    copies.Add(CopyShift(shift));
            }

            return copies;
        }

        private static Chromosome CopyChromosome(Chromosome original)
        {
            if (original == null)
                return null;

            Chromosome copy = new Chromosome();
            copy.Fitness = original.Fitness;
            copy.Shifts = new Dictionary<string, List<Shift>>();

            if (original.Shifts != null)
            {
                foreach (var branchEntry in original.Shifts)
                {
                    string branchName = branchEntry.Key;
                    List<Shift> originalShifts = branchEntry.Value;

                    copy.Shifts[branchName] = CopyShifts(originalShifts);
                }
            }

            return copy;
        }
        #endregion

        #region Mutation

        public static void Mutation(Population pop)
        {
            Random random = new Random();
            List<Chromosome> newChromosomes = new List<Chromosome>();

            // מעקב אחרי עובדים שמשויכים למשמרות למניעת חפיפות
            Dictionary<Employee, HashSet<string>> globalEmployeeAssignments = new Dictionary<Employee, HashSet<string>>();

            foreach (Chromosome chromosome in pop.Chromoshomes)
            {
                // יצירת העתק עמוק של הכרומוזום לצורך מוטציה
                Chromosome mutatedChromosome = CopyChromosome(chromosome);
                bool wasMutated = false;
                double originalFitness = chromosome.Fitness;

                // ניקוי המעקב עבור כל כרומוזום חדש
                globalEmployeeAssignments.Clear();

                // בניית המעקב הקיים של שיוכי עובדים כדי לזהות חפיפות
                foreach (var branchEntry in mutatedChromosome.Shifts)
                {
                    foreach (Shift shift in branchEntry.Value)
                    {
                        foreach (var roleEntry in shift.AssignedEmployees)
                        {
                            foreach (Employee emp in roleEntry.Value)
                            {
                                string shiftKey = $"{shift.day}_{shift.TimeSlot}";
                                if (!globalEmployeeAssignments.ContainsKey(emp))
                                    globalEmployeeAssignments[emp] = new HashSet<string>();
                                globalEmployeeAssignments[emp].Add(shiftKey);
                            }
                        }
                    }
                }

                // ניסיונות מוטציה מרובים - הגדלנו את מספר הניסיונות
                for (int attempt = 0; attempt < 10; attempt++)
                {
                    // בחירת סניף אקראי לביצוע מוטציה
                    if (mutatedChromosome.Shifts.Count == 0)
                        continue;

                    var branchKeys = mutatedChromosome.Shifts.Keys.ToList();
                    string branchName = branchKeys[random.Next(branchKeys.Count)];
                    List<Shift> shifts = mutatedChromosome.Shifts[branchName];

                    if (shifts == null || shifts.Count == 0)
                        continue;

                    // בחירת משמרת אקראית למוטציה
                    int shiftIndex = random.Next(shifts.Count);
                    Shift shiftToMutate = shifts[shiftIndex];
                    string shiftKey = $"{shiftToMutate.day}_{shiftToMutate.TimeSlot}";

                    if (shiftToMutate.AssignedEmployees == null)
                        shiftToMutate.AssignedEmployees = new Dictionary<string, List<Employee>>();

                    // Strategy 1: מילוי משרות ריקות (Focus on empty slots first)
                    bool appliedStrategy1 = TryFillEmptyPositions(shiftToMutate, shiftKey, globalEmployeeAssignments);
                    if (appliedStrategy1)
                    {
                        wasMutated = true;
                        continue;
                    }

                    // Strategy 2: שיפור שיבוץ על ידי החלפת עובדים בעלי דירוג נמוך בעובדים בעלי דירוג גבוה
                    bool appliedStrategy2 = TryUpgradeEmployees(shiftToMutate, shiftKey, globalEmployeeAssignments);
                    if (appliedStrategy2)
                    {
                        wasMutated = true;
                        continue;
                    }

                    // Strategy 3: מנסה לשבץ עובדים שביקשו משמרת זו במקום עובדים שלא ביקשו
                    bool appliedStrategy3 = TryMatchPreferredShifts(shiftToMutate, shiftKey, globalEmployeeAssignments);
                    if (appliedStrategy3)
                    {
                        wasMutated = true;
                        continue;
                    }

                    // Strategy 4: הוספת עובד מנטור אם אין כזה במשמרת
                    bool appliedStrategy4 = TryAddMentor(shiftToMutate, shiftKey, globalEmployeeAssignments);
                    if (appliedStrategy4)
                    {
                        wasMutated = true;
                        continue;
                    }

                    // Strategy 5: ניסיון להפחית עלויות על ידי החלפת עובדים יקרים בעובדים זולים יותר
                    bool appliedStrategy5 = TryReduceCosts(shiftToMutate, shiftKey, globalEmployeeAssignments);
                    if (appliedStrategy5)
                    {
                        wasMutated = true;
                        continue;
                    }
                }

                if (wasMutated)
                {
                    // חישוב ציון הכושר החדש
                    mutatedChromosome.Fitness = Program.CalculateChromosomeFitness(mutatedChromosome);

                    // רק אם הכרומוזום שופר (או לא נפגע) נוסיף אותו לאוכלוסייה
                    if (mutatedChromosome.Fitness >= originalFitness)
                    {
                        newChromosomes.Add(mutatedChromosome);
                    }
                }
            }

            // הוספת הכרומוזומים החדשים לאוכלוסייה
            foreach (var newChromosome in newChromosomes)
            {
                pop.Chromoshomes.Add(newChromosome);
            }

            // שמירה על הכרומוזומים הטובים בלבד
            pop.Chromoshomes = pop.Chromoshomes
                .OrderByDescending(ch => ch.Fitness)
                .Take(Program.ChromosomesEachGene)
                .ToList();
        }

        // ניסיון למלא משרות ריקות
        private static bool TryFillEmptyPositions(Shift shiftToMutate, string shiftKey,
            Dictionary<Employee, HashSet<string>> globalEmployeeAssignments)
        {
            // בדיקה האם קיימות משרות שלא מולאו
            foreach (var roleReq in shiftToMutate.RequiredRoles)
            {
                string role = roleReq.Key;
                int required = roleReq.Value;

                if (!shiftToMutate.AssignedEmployees.ContainsKey(role))
                    shiftToMutate.AssignedEmployees[role] = new List<Employee>();

                if (shiftToMutate.AssignedEmployees[role].Count < required)
                {
                    // מציאת עובדים שכבר במשמרת
                    HashSet<Employee> employeesInShift = GetEmployeesInShift(shiftToMutate);

                    // מציאת עובדים זמינים לתפקיד, עם עדיפות למי שביקש את המשמרת
                    List<Employee> preferredEmployees = Program.Employees
                        .Where(e => e.roles.Contains(role) &&
                               e.requestedShifts.Contains(shiftToMutate.Id) &&
                               !employeesInShift.Contains(e) &&
                               (!globalEmployeeAssignments.ContainsKey(e) ||
                                !globalEmployeeAssignments[e].Contains(shiftKey)))
                        .OrderByDescending(e => e.Rate)
                        .ToList();

                    // אם יש עובדים מתאימים שביקשו את המשמרת
                    if (preferredEmployees.Count > 0)
                    {
                        // הוספת עובד מועדף למשרה הריקה
                        Employee selectedEmployee = preferredEmployees[0];
                        shiftToMutate.AssignedEmployees[role].Add(selectedEmployee);

                        // עדכון המעקב
                        if (!globalEmployeeAssignments.ContainsKey(selectedEmployee))
                            globalEmployeeAssignments[selectedEmployee] = new HashSet<string>();
                        globalEmployeeAssignments[selectedEmployee].Add(shiftKey);

                        return true;
                    }
                    else
                    {
                        // אם אין עובדים שביקשו את המשמרת, ננסה לשבץ כל עובד מתאים
                        List<Employee> availableEmployees = Program.Employees
                            .Where(e => e.roles.Contains(role) &&
                                  !employeesInShift.Contains(e) &&
                                  (!globalEmployeeAssignments.ContainsKey(e) ||
                                   !globalEmployeeAssignments[e].Contains(shiftKey)))
                            .OrderByDescending(e => e.Rate)
                            .ToList();

                        if (availableEmployees.Count > 0)
                        {
                            // הוספת עובד מתאים למשרה הריקה
                            Employee selectedEmployee = availableEmployees[0];
                            shiftToMutate.AssignedEmployees[role].Add(selectedEmployee);

                            // עדכון המעקב
                            if (!globalEmployeeAssignments.ContainsKey(selectedEmployee))
                                globalEmployeeAssignments[selectedEmployee] = new HashSet<string>();
                            globalEmployeeAssignments[selectedEmployee].Add(shiftKey);

                            return true;
                        }
                    }
                }
            }
            return false;
        }

        // ניסיון לשפר את הרכב העובדים על ידי החלפת עובדים בעלי דירוג נמוך בעובדים בעלי דירוג גבוה
        private static bool TryUpgradeEmployees(Shift shiftToMutate, string shiftKey,
            Dictionary<Employee, HashSet<string>> globalEmployeeAssignments)
        {
            if (shiftToMutate.AssignedEmployees.Count == 0)
                return false;

            // בחירת תפקיד אקראי שיש בו עובדים
            var rolesWithEmployees = shiftToMutate.AssignedEmployees
                .Where(kv => kv.Value != null && kv.Value.Count > 0)
                .Select(kv => kv.Key)
                .ToList();

            if (rolesWithEmployees.Count == 0)
                return false;

            Random random = new Random();
            string role = rolesWithEmployees[random.Next(rolesWithEmployees.Count)];
            List<Employee> employees = shiftToMutate.AssignedEmployees[role];

            if (employees.Count > 0)
            {
                // מיון העובדים לפי דירוג (מהנמוך לגבוה) כדי להחליף את הנמוך ביותר
                employees.Sort((a, b) => a.Rate.CompareTo(b.Rate));
                Employee currentEmployee = employees[0]; // העובד עם הדירוג הנמוך ביותר

                // מציאת עובדים שכבר במשמרת
                HashSet<Employee> employeesInShift = GetEmployeesInShift(shiftToMutate);

                // מציאת עובדים פוטנציאליים להחלפה עם דירוג גבוה יותר
                List<Employee> potentialReplacements = Program.Employees
                    .Where(e => e.roles.Contains(role) &&
                           e.Rate > currentEmployee.Rate &&
                           !employeesInShift.Contains(e) &&
                           (!globalEmployeeAssignments.ContainsKey(e) ||
                            !globalEmployeeAssignments[e].Contains(shiftKey)))
                    .OrderByDescending(e => e.Rate)
                    .ToList();

                if (potentialReplacements.Count > 0)
                {
                    // הסרת העובד הנוכחי מהמעקב
                    if (globalEmployeeAssignments.ContainsKey(currentEmployee))
                        globalEmployeeAssignments[currentEmployee].Remove(shiftKey);

                    // החלפה בעובד טוב יותר
                    Employee replacement = potentialReplacements[0];
                    int employeeIndex = employees.IndexOf(currentEmployee);
                    shiftToMutate.AssignedEmployees[role][employeeIndex] = replacement;

                    if (!globalEmployeeAssignments.ContainsKey(replacement))
                        globalEmployeeAssignments[replacement] = new HashSet<string>();
                    globalEmployeeAssignments[replacement].Add(shiftKey);

                    return true;
                }
            }
            return false;
        }

        // ניסיון להתאים עובדים למשמרות שהם ביקשו
        private static bool TryMatchPreferredShifts(Shift shiftToMutate, string shiftKey,
            Dictionary<Employee, HashSet<string>> globalEmployeeAssignments)
        {
            if (shiftToMutate.AssignedEmployees.Count == 0)
                return false;

            foreach (var roleEntry in shiftToMutate.AssignedEmployees)
            {
                string role = roleEntry.Key;
                List<Employee> employees = roleEntry.Value;

                // מיון העובדים לפי אלו שלא ביקשו את המשמרת
                var nonPreferredEmployees = employees
                    .Where(e => !e.requestedShifts.Contains(shiftToMutate.Id))
                    .ToList();

                if (nonPreferredEmployees.Count > 0)
                {
                    Employee currentEmployee = nonPreferredEmployees[0];

                    // מציאת עובדים שכבר במשמרת
                    HashSet<Employee> employeesInShift = GetEmployeesInShift(shiftToMutate);
                    employeesInShift.Remove(currentEmployee); // מסירים את העובד הנוכחי שנחליף

                    // מציאת עובדים שכן ביקשו את המשמרת
                    List<Employee> preferredEmployees = Program.Employees
                        .Where(e => e.roles.Contains(role) &&
                               e.requestedShifts.Contains(shiftToMutate.Id) &&
                               !employeesInShift.Contains(e) &&
                               (!globalEmployeeAssignments.ContainsKey(e) ||
                                !globalEmployeeAssignments[e].Contains(shiftKey)))
                        .OrderByDescending(e => e.Rate)
                        .ToList();

                    if (preferredEmployees.Count > 0)
                    {
                        // הסרת העובד הנוכחי מהמעקב
                        if (globalEmployeeAssignments.ContainsKey(currentEmployee))
                            globalEmployeeAssignments[currentEmployee].Remove(shiftKey);

                        // החלפה בעובד שביקש את המשמרת
                        Employee replacement = preferredEmployees[0];
                        int employeeIndex = employees.IndexOf(currentEmployee);
                        shiftToMutate.AssignedEmployees[role][employeeIndex] = replacement;

                        if (!globalEmployeeAssignments.ContainsKey(replacement))
                            globalEmployeeAssignments[replacement] = new HashSet<string>();
                        globalEmployeeAssignments[replacement].Add(shiftKey);

                        return true;
                    }
                }
            }
            return false;
        }

        // ניסיון להוסיף עובד מנטור אם אין כזה במשמרת
        private static bool TryAddMentor(Shift shiftToMutate, string shiftKey,
            Dictionary<Employee, HashSet<string>> globalEmployeeAssignments)
        {
            // בדיקה אם כבר יש מנטור במשמרת
            bool hasMentor = false;
            foreach (var employeeList in shiftToMutate.AssignedEmployees.Values)
            {
                if (employeeList.Any(e => e.isMentor))
                {
                    hasMentor = true;
                    break;
                }
            }

            // אם אין מנטור, ננסה להוסיף אחד
            if (!hasMentor)
            {
                // עובר על כל התפקידים כדי לנסות להחליף עובד רגיל במנטור
                foreach (var roleEntry in shiftToMutate.AssignedEmployees)
                {
                    string role = roleEntry.Key;
                    List<Employee> employees = roleEntry.Value;

                    if (employees.Count > 0)
                    {
                        // בוחר עובד אקראי שאינו מנטור
                        Random random = new Random();
                        var nonMentors = employees.Where(e => !e.isMentor).ToList();

                        if (nonMentors.Count > 0)
                        {
                            Employee currentEmployee = nonMentors[random.Next(nonMentors.Count)];

                            // מציאת עובדים שכבר במשמרת
                            HashSet<Employee> employeesInShift = GetEmployeesInShift(shiftToMutate);
                            employeesInShift.Remove(currentEmployee); // מסירים את העובד הנוכחי שנחליף

                            // חיפוש מנטור מתאים
                            List<Employee> availableMentors = Program.Employees
                                .Where(e => e.roles.Contains(role) &&
                                       e.isMentor &&
                                       !employeesInShift.Contains(e) &&
                                       (!globalEmployeeAssignments.ContainsKey(e) ||
                                        !globalEmployeeAssignments[e].Contains(shiftKey)))
                                .OrderByDescending(e => e.Rate)
                                .ToList();

                            if (availableMentors.Count > 0)
                            {
                                // הסרת העובד הנוכחי מהמעקב
                                if (globalEmployeeAssignments.ContainsKey(currentEmployee))
                                    globalEmployeeAssignments[currentEmployee].Remove(shiftKey);

                                // החלפה במנטור
                                Employee mentor = availableMentors[0];
                                int employeeIndex = employees.IndexOf(currentEmployee);
                                shiftToMutate.AssignedEmployees[role][employeeIndex] = mentor;

                                if (!globalEmployeeAssignments.ContainsKey(mentor))
                                    globalEmployeeAssignments[mentor] = new HashSet<string>();
                                globalEmployeeAssignments[mentor].Add(shiftKey);

                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        // ניסיון להפחית עלויות על ידי החלפת עובדים יקרים בעובדים זולים יותר
        private static bool TryReduceCosts(Shift shiftToMutate, string shiftKey,
            Dictionary<Employee, HashSet<string>> globalEmployeeAssignments)
        {
            if (shiftToMutate.AssignedEmployees.Count == 0)
                return false;

            // בחירת תפקיד אקראי שיש בו עובדים
            Random random = new Random();
            var rolesWithEmployees = shiftToMutate.AssignedEmployees
                .Where(kv => kv.Value != null && kv.Value.Count > 0)
                .Select(kv => kv.Key)
                .ToList();

            if (rolesWithEmployees.Count == 0)
                return false;

            string role = rolesWithEmployees[random.Next(rolesWithEmployees.Count)];
            List<Employee> employees = shiftToMutate.AssignedEmployees[role];

            if (employees.Count > 0)
            {
                // מיון העובדים לפי שכר (מהגבוה לנמוך)
                employees.Sort((a, b) => b.HourlySalary.CompareTo(a.HourlySalary));
                Employee expensiveEmployee = employees[0]; // העובד עם השכר הגבוה ביותר

                // רק אם העובד מרוויח מעל למינימום מסוים - שווה להחליף אותו
                if (expensiveEmployee.HourlySalary > 50)
                {
                    // מציאת עובדים שכבר במשמרת
                    HashSet<Employee> employeesInShift = GetEmployeesInShift(shiftToMutate);
                    employeesInShift.Remove(expensiveEmployee); // מסירים את העובד היקר שנחליף

                    // חיפוש עובדים זולים יותר
                    List<Employee> cheaperEmployees = Program.Employees
                        .Where(e => e.roles.Contains(role) &&
                               e.HourlySalary < expensiveEmployee.HourlySalary &&
                               e.Rate >= expensiveEmployee.Rate * 0.8 && // לא יותר מדי נמוך בדירוג
                               !employeesInShift.Contains(e) &&
                               (!globalEmployeeAssignments.ContainsKey(e) ||
                                !globalEmployeeAssignments[e].Contains(shiftKey)))
                        .OrderBy(e => e.HourlySalary)
                        .ToList();

                    if (cheaperEmployees.Count > 0)
                    {
                        // הסרת העובד היקר מהמעקב
                        if (globalEmployeeAssignments.ContainsKey(expensiveEmployee))
                            globalEmployeeAssignments[expensiveEmployee].Remove(shiftKey);

                        // החלפה בעובד זול יותר
                        Employee replacement = cheaperEmployees[0];
                        int employeeIndex = employees.IndexOf(expensiveEmployee);
                        shiftToMutate.AssignedEmployees[role][employeeIndex] = replacement;

                        if (!globalEmployeeAssignments.ContainsKey(replacement))
                            globalEmployeeAssignments[replacement] = new HashSet<string>();
                        globalEmployeeAssignments[replacement].Add(shiftKey);

                        return true;
                    }
                }
            }
            return false;
        }

        // Helper method to get all employees currently in a shift
        private static HashSet<Employee> GetEmployeesInShift(Shift shift)
        {
            HashSet<Employee> employeesInShift = new HashSet<Employee>();
            foreach (var roleEntry in shift.AssignedEmployees)
            {
                foreach (Employee emp in roleEntry.Value)
                {
                    employeesInShift.Add(emp);
                }
            }
            return employeesInShift;
        }

        // Helper function to find shifts that would overlap with an assigned shift
        private static List<int> FindOverlappingShifts(Employee employee, Shift assignedShift)
        {
            if (employee == null || assignedShift == null)
                return new List<int>();

            List<int> overlappingShiftIds = new List<int>();
            foreach (int shiftId in employee.requestedShifts)
            {
                // Find the shift in the branches
                Shift shift = null;
                foreach (Branch branch in Program.Branches)
                {
                    shift = branch.Shifts.FirstOrDefault(s => s.Id == shiftId);
                    if (shift != null)
                        break;
                }

                // Check if it's an overlapping shift (same day and time slot)
                if (shift != null && shift.day == assignedShift.day && shift.TimeSlot == assignedShift.TimeSlot)
                {
                    overlappingShiftIds.Add(shiftId);
                }
            }

            return overlappingShiftIds;
        }

        #endregion

        #region Fitness

        //פונקציה לחישוב ציון הכושר של כרומוזום
        public static double CalculateChromosomeFitness(Chromosome chromosome)
        {
            //השמת ערך התחלתי לציון לכרומוזום
            double totalFitness = 0;

            // מעקב אחר שעות העבודה השבועיות והיומיות של כל עובד
            Dictionary<Employee, double> weeklyHoursPerEmployee = new Dictionary<Employee, double>();
            Dictionary<Employee, Dictionary<string, double>> dailyHoursPerEmployee = new Dictionary<Employee, Dictionary<string, double>>();
            //החזרת ערך מינימלי במקרה שהמשמרות ריקות
            if (chromosome.Shifts == null)
                return Double.MinValue;

            //מעבר על כל הסניפים בכרומוזום
            foreach (var branchEntry in chromosome.Shifts)
            {
                //קבלת רשימת המשמרות של הסניף הנוכחי
                string branchName = branchEntry.Key;
                List<Shift> branchShifts = branchEntry.Value;

                if (branchShifts == null)
                    continue;

                // מעבר על כל המשמרות בסניף הנוכחי
                foreach (var shift in branchShifts)
                {
                    //חישוב ציון הכושר של המשמרת הנוכחית
                    double shiftFitness = CalculateShiftFitness(shift, weeklyHoursPerEmployee, dailyHoursPerEmployee);
                    //הוספת ציון הכושר של המשמרת הנוכחית לציון הכושר הכולל
                    totalFitness += shiftFitness;
                }
            }

            // בדיקת אילוץ- עובד לא יעבוד יותר שעות שבועיות מהמותר בחוק
            //מעבר על המילון ששומר את כמות השעות השבועיות של כל עובד
            foreach (var entry in weeklyHoursPerEmployee)
            {
                Employee emp = entry.Key;
                double hours = entry.Value;
                // קנס משמעותי על חריגה ממגבלת השעות השבועיות
                if (hours > hoursPerWeek)
                {
                    totalFitness -= (hours - hoursPerWeek) * 10; 
                }
            }

            // בדיקת אילוץ- עובד לא יעבוד יותר שעות יומיות מהמותר בחוק
            //מעבר על המילון ששומר את כמות השעות היומיות של כל עובד
            foreach (var empEntry in dailyHoursPerEmployee)
            {
                var dayHours = empEntry.Value;
                foreach (var dayEntry in dayHours)
                {
                    double hours = dayEntry.Value;
                    // קנס משמעותי יותר על חריגה ממגבלת השעות היומיות
                    if (hours > hoursPerDay)
                    {
                        totalFitness -= (hours - hoursPerDay) * 15; 
                    }
                }
            }
            //החזרת ציון הכושר הכולל
            return totalFitness;
        }
        //פונקציה לחישוב ציון הכושר של משמרת בכרומוזום
        private static double CalculateShiftFitness(Shift shift,
            Dictionary<Employee, double> weeklyHoursPerEmployee,
            Dictionary<Employee, Dictionary<string, double>> dailyHoursPerEmployee)
        {
            //השמת ערך התחלתי לציון המשמרת
            double shiftFitness = 0;

            //קבלת כמות העובדים הדרושים וכמות העובדים בפועל למשמרת
            int totalEmployees = GetTotalEmployeesInShift(shift);
            int requiredEmployees = shift.GetTotalRequiredEmployees();

            // בדיקת אילוץ - כמות מינימלית של עובדים
            // קנס חמור על חוסר בעובדים
            if (totalEmployees < requiredEmployees)
            {
                shiftFitness -= (requiredEmployees - totalEmployees) * 20; 
            }

            // בדיקת אילוץ חובה - תפקידים נדרשים
            foreach (var roleEntry in shift.RequiredRoles)
            {
                string role = roleEntry.Key;
                int requiredCount = roleEntry.Value;

                if (!shift.AssignedEmployees.ContainsKey(role) ||
                    shift.AssignedEmployees[role].Count < requiredCount)
                {
                    int missing = requiredCount - (shift.AssignedEmployees.ContainsKey(role) ?
                        shift.AssignedEmployees[role].Count : 0);

                    shiftFitness -= missing * 25; // קנס חמור אף יותר על היעדר תפקידים נדרשים
                }
                else
                {
                    shiftFitness += 10; // בונוס על מילוי דרישת תפקיד
                }
            }

            // בדיקת אילוץ - עובדים משובצים רק לסניפים ומשמרות שביקשו
            foreach (var roleEntry in shift.AssignedEmployees)
            {
                foreach (var employee in roleEntry.Value)
                {
                    // בדיקה אם העובד שייך לסניף
                    if (!employee.Branches.Contains(shift.branch))
                    {
                        shiftFitness -= 30; // קנס חמור על שיבוץ לסניף לא נכון
                    }

                    // בדיקה אם העובד ביקש את המשמרת
                    if (!employee.requestedShifts.Contains(shift.Id))
                    {
                        shiftFitness -= 15; // קנס על שיבוץ למשמרת לא מבוקשת
                
                    }
                    else
                    {
                        shiftFitness += 5; // בונוס על שיבוץ למשמרת מבוקשת
                    
                    }

                    // עדכון מעקב שעות עבודה לעובד
                    UpdateWorkingHours(employee, shift.day, hoursPerShift, weeklyHoursPerEmployee, dailyHoursPerEmployee);
                }
            }

            // בדיקת אילוץ -  עובד מנוסה בכל משמרת
            bool hasMentor = false;
            foreach (var employeeList in shift.AssignedEmployees.Values)
            {
                if (employeeList.Any(emp => emp.isMentor))
                {
                    hasMentor = true;
                    break;
                }
            }

            if (hasMentor)
            {
                shiftFitness += 100; // בונוס על נוכחות עובד מנוסה במשמרת
            }
            else
            {
                shiftFitness -= 100; // קנס  על היעדר עובד מנוסה במשמרת
            }

            // בדיקת אילוץ  - התאמת רמת העובדים למידת העומס
            if (shift.IsBusy)
            {
                double avgRate = CalculateAverageEmployeeRate(shift);
                if (avgRate >= 3.5)
                {
                    shiftFitness += 70; // בונוס על צוות חזק במשמרת עמוסה
                }
                else
                {
                    shiftFitness -= 70; // קנס קל על צוות חלש במשמרת עמוסה
                }
            }

            // בדיקת אילוץ  - תמהיל צוותי מאוזן
            double experiencedRatio = CalculateExperiencedRatio(shift);
            if (experiencedRatio >= 0.3 && experiencedRatio <= 0.7)
            {
                shiftFitness += 50; // בונוס על איזון טוב בין מנוסים לחדשים
            }

            // בדיקת אילוץ - מינימום עלות משמרת
            double shiftCost = CalculateShiftCost(shift, hoursPerShift);
            shiftFitness -= shiftCost / 200; // קנס קל לפי עלות המשמרת

            return shiftFitness;
        }

      
        //פונקציה המקבלת משמרת ומחזירה את כמו העובדים ששובצו אליה
        private static int GetTotalEmployeesInShift(Shift shift)
        {
            return shift.AssignedEmployees.Values.Sum(lst => lst.Count);
        }

        //פונקציה המעדכנת את המילון ששומר את השעות של כל עובד 
        private static void UpdateWorkingHours(Employee employee, string day, double hours,
            Dictionary<Employee, double> weeklyHoursPerEmployee,
            Dictionary<Employee, Dictionary<string, double>> dailyHoursPerEmployee)
        {
            // עדכון שעות שבועיות
            if (!weeklyHoursPerEmployee.ContainsKey(employee))
            {
                //הכנסת העובד למילון אם הוא עוד לא קיים שם
                weeklyHoursPerEmployee[employee] = 0;
            }
            //הוספת שעות המשמרת שלו
            weeklyHoursPerEmployee[employee] += hours;

            // עדכון שעות יומיות
            if (!dailyHoursPerEmployee.ContainsKey(employee))
            {
                dailyHoursPerEmployee[employee] = new Dictionary<string, double>();
            }

            if (!dailyHoursPerEmployee[employee].ContainsKey(day))
            {
                //הכנסת העובד למילון אם הוא עוד לא קיים שם
                dailyHoursPerEmployee[employee][day] = 0;
            }
            //הוספת שעות המשמרת שלו

            dailyHoursPerEmployee[employee][day] += hours;
        }

        //פונקציה המחשבת את הציון הממוצע של עובדים במשמרת
        private static double CalculateAverageEmployeeRate(Shift shift)
        {
            //קבלת מספר העובדים
            int totalEmployees = GetTotalEmployeesInShift(shift);
            if (totalEmployees == 0)
                return 0;
            //צבירת סכום הציונים שלהם במונה
            double sumRate = 0;
            foreach (var employeeList in shift.AssignedEmployees.Values)
            {
                foreach (var employee in employeeList)
                {
                    sumRate += employee.Rate;
                }
            }
            //חישוב הממוצע
            return sumRate / totalEmployees;
        }

        //פונקציה המחשבת את כמות העובדים המנוסים מסך כל העובדים
        private static double CalculateExperiencedRatio(Shift shift)
        {
            //קבלת מספר העובדים
            int totalEmployees = GetTotalEmployeesInShift(shift);
            if (totalEmployees == 0)
                return 0;

            int experiencedCount = 0;
            //מעבר על כל העובדים במשמרת 
            foreach (var employeeList in shift.AssignedEmployees.Values)
            {
                foreach (var employee in employeeList)
                {
                    //סכימת כמות העובדים המנוסים
                    if (employee.isMentor)
                    {
                        experiencedCount++;
                    }
                }
            }
            //חישוב אחוז העובדים המנוסים מסך כל העובדים
            return (double)experiencedCount / totalEmployees;
        }

        //פונקציה המחשבת את עלות המשמרת
        private static double CalculateShiftCost(Shift shift, double hours)
        {
            double totalCost = 0;
            //מעבר על כל העובדים במשמרת
            foreach (var employeeList in shift.AssignedEmployees.Values)
            {
                foreach (var employee in employeeList)
                {
                    //סכימת שכר העובדים
                    totalCost += employee.HourlySalary * hours;
                }
            }

            return totalCost;
        }

        //פונקציה המחזירה את הכרומוזום הטוב ביותר באוכלוסייה
        public static Chromosome GetBestChromosome()
        {
            //מיון הכרומוזומיים בסדר יורד והחזרת הכרומוזום הטוב ביותר
            return pop.Chromoshomes.OrderByDescending(ch => ch.Fitness).FirstOrDefault();
        }
        /// <summary>
        /// לשנות ליעיל יותר
        /// </summary>
        #endregion
        static void Main()
        {
          
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new HomePage());
        }
    }
}
