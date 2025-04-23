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
        public const int ChromosomesEachGene = 300;
        public const int Genes = 300;
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

        #region InitializeFirstPopulation 
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

            foreach (Chromosome chromosome in pop.Chromoshomes)
            {
                // יצירת העתק של הכרומוזום לביצוע מוטציה
                Chromosome mutatedChromosome = CopyChromosome(chromosome);
                double originalFitness = chromosome.Fitness;
                bool wasImproved = false;

                // מיפוי שיבוצים נוכחיים
                Dictionary<Employee, HashSet<string>> assignments = GetEmployeeAssignments(mutatedChromosome);

                // מספר ניסיונות לשיפור הכרומוזום
                for (int i = 0; i < 8; i++)
                {
                    // בחירת משמרת אקראית מהכרומוזום
                    Shift shift = GetRandomShift(mutatedChromosome, random);
                    if (shift == null) continue;

                    string shiftKey = $"{shift.day}_{shift.TimeSlot}";

                    // הפעלת אסטרטגיות מוטציה מהחשובה לפחות חשובה
                    if (!ShiftHasMentor(shift) && AddMentor(shift, shiftKey, assignments))
                        wasImproved = true;
                    else if (random.Next(2) == 0 && FillEmptyPositions(shift, shiftKey, assignments))
                        wasImproved = true;
                    else if (random.Next(2) == 0 && OptimizePreferences(shift, shiftKey, assignments))
                        wasImproved = true;
                    else if (random.Next(3) == 0 && UpgradeEmployees(shift, shiftKey, assignments))
                        wasImproved = true;
                    else if (random.Next(4) == 0 && ReduceCosts(shift, shiftKey, assignments))
                        wasImproved = true;
                }

                // אם הכרומוזום השתפר, חשב מחדש את ציון הכושר והוסף לרשימה
                if (wasImproved)
                {
                    mutatedChromosome.Fitness = CalculateChromosomeFitness(mutatedChromosome);
                    if (mutatedChromosome.Fitness > originalFitness)
                        newChromosomes.Add(mutatedChromosome);
                }
            }

            // הוסף את הכרומוזומים המשופרים לאוכלוסייה
            pop.Chromoshomes.AddRange(newChromosomes);

            // שמור על הכרומוזומים הטובים ביותר
            pop.Chromoshomes = pop.Chromoshomes
                .OrderByDescending(ch => ch.Fitness)
                .Take(ChromosomesEachGene)
                .ToList();
        }

        // מיפוי שיבוצי עובדים נוכחיים
        private static Dictionary<Employee, HashSet<string>> GetEmployeeAssignments(Chromosome chromosome)
        {
            var assignments = new Dictionary<Employee, HashSet<string>>();

            foreach (var branch in chromosome.Shifts)
            {
                foreach (Shift shift in branch.Value)
                {
                    string key = $"{shift.day}_{shift.TimeSlot}";

                    if (shift.AssignedEmployees != null)
                    {
                        foreach (var role in shift.AssignedEmployees)
                        {
                            foreach (Employee emp in role.Value)
                            {
                                if (!assignments.ContainsKey(emp))
                                    assignments[emp] = new HashSet<string>();

                                assignments[emp].Add(key);
                            }
                        }
                    }
                }
            }

            return assignments;
        }

        // בחירת משמרת אקראית מהכרומוזום
        private static Shift GetRandomShift(Chromosome chromosome, Random random)
        {
            if (chromosome.Shifts.Count == 0) return null;

            var branchKeys = chromosome.Shifts.Keys.ToList();
            string branch = branchKeys[random.Next(branchKeys.Count)];

            if (chromosome.Shifts[branch].Count == 0) return null;

            return chromosome.Shifts[branch][random.Next(chromosome.Shifts[branch].Count)];
        }

        // בדיקה אם יש מנטור במשמרת
        private static bool ShiftHasMentor(Shift shift)
        {
            if (shift.AssignedEmployees == null) return false;

            return shift.AssignedEmployees.Values
                .SelectMany(emps => emps)
                .Any(emp => emp.isMentor);
        }

        // הוספת מנטור למשמרת
        private static bool AddMentor(Shift shift, string shiftKey, Dictionary<Employee, HashSet<string>> assignments)
        {
            if (shift.AssignedEmployees == null || shift.AssignedEmployees.Count == 0) return false;

            // בחר תפקיד אקראי ועובד אקראי להחלפה
            var roles = shift.AssignedEmployees.Where(r => r.Value.Count > 0).ToList();
            if (roles.Count == 0) return false;

            var roleEntry = roles[new Random().Next(roles.Count)];
            string role = roleEntry.Key;
            int empIndex = new Random().Next(roleEntry.Value.Count);
            Employee currentEmp = roleEntry.Value[empIndex];

            // בדוק אילו עובדים כבר במשמרת
            var employeesInShift = shift.AssignedEmployees.Values
                .SelectMany(emps => emps)
                .ToHashSet();

            // מצא מנטור מתאים להחלפה
            var mentor = Employees
                .Where(e => e.roles.Contains(role) &&
                       e.isMentor &&
                       !employeesInShift.Contains(e) &&
                       (!assignments.ContainsKey(e) || !assignments[e].Contains(shiftKey)))
                .OrderByDescending(e => e.Rate)
                .FirstOrDefault();

            if (mentor == null) return false;

            // עדכן את המשמרת ואת מפת השיבוצים
            if (assignments.ContainsKey(currentEmp))
                assignments[currentEmp].Remove(shiftKey);

            roleEntry.Value[empIndex] = mentor;

            if (!assignments.ContainsKey(mentor))
                assignments[mentor] = new HashSet<string>();

            assignments[mentor].Add(shiftKey);

            return true;
        }

        // מילוי משרות ריקות
        private static bool FillEmptyPositions(Shift shift, string shiftKey, Dictionary<Employee, HashSet<string>> assignments)
        {
            if (shift.AssignedEmployees == null)
                shift.AssignedEmployees = new Dictionary<string, List<Employee>>();

            foreach (var roleReq in shift.RequiredRoles)
            {
                string role = roleReq.Key;
                int required = roleReq.Value;

                if (!shift.AssignedEmployees.ContainsKey(role))
                    shift.AssignedEmployees[role] = new List<Employee>();

                if (shift.AssignedEmployees[role].Count < required)
                {
                    // בדוק אילו עובדים כבר במשמרת
                    var employeesInShift = shift.AssignedEmployees.Values
                        .SelectMany(emps => emps)
                        .ToHashSet();

                    // מצא עובד מתאים
                    var emp = Employees
                        .Where(e => e.roles.Contains(role) &&
                               !employeesInShift.Contains(e) &&
                               (!assignments.ContainsKey(e) || !assignments[e].Contains(shiftKey)))
                        .OrderByDescending(e => e.requestedShifts.Contains(shift.Id) ? 1 : 0)
                        .ThenByDescending(e => e.Rate)
                        .FirstOrDefault();

                    if (emp != null)
                    {
                        shift.AssignedEmployees[role].Add(emp);

                        if (!assignments.ContainsKey(emp))
                            assignments[emp] = new HashSet<string>();

                        assignments[emp].Add(shiftKey);

                        return true;
                    }
                }
            }

            return false;
        }

        // התאמת העדפות עובדים
        private static bool OptimizePreferences(Shift shift, string shiftKey, Dictionary<Employee, HashSet<string>> assignments)
        {
            if (shift.AssignedEmployees == null || shift.AssignedEmployees.Count == 0) return false;

            foreach (var roleEntry in shift.AssignedEmployees)
            {
                for (int i = 0; i < roleEntry.Value.Count; i++)
                {
                    Employee current = roleEntry.Value[i];

                    // בדוק אם העובד הנוכחי לא ביקש את המשמרת
                    if (!current.requestedShifts.Contains(shift.Id))
                    {
                        // בדוק אילו עובדים כבר במשמרת
                        var employeesInShift = shift.AssignedEmployees.Values
                            .SelectMany(emps => emps)
                            .Where(e => e != current)
                            .ToHashSet();

                        // מצא עובד שכן ביקש את המשמרת
                        var preferred = Employees
                            .Where(e => e.roles.Contains(roleEntry.Key) &&
                                   e.requestedShifts.Contains(shift.Id) &&
                                   !employeesInShift.Contains(e) &&
                                   (!assignments.ContainsKey(e) || !assignments[e].Contains(shiftKey)))
                            .OrderByDescending(e => e.Rate)
                            .FirstOrDefault();

                        if (preferred != null)
                        {
                            if (assignments.ContainsKey(current))
                                assignments[current].Remove(shiftKey);

                            roleEntry.Value[i] = preferred;

                            if (!assignments.ContainsKey(preferred))
                                assignments[preferred] = new HashSet<string>();

                            assignments[preferred].Add(shiftKey);

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        // שיפור איכות העובדים
        private static bool UpgradeEmployees(Shift shift, string shiftKey, Dictionary<Employee, HashSet<string>> assignments)
        {
            if (shift.AssignedEmployees == null || shift.AssignedEmployees.Count == 0) return false;

            // מצא את העובד עם הדירוג הנמוך ביותר
            Employee lowest = null;
            string lowestRole = null;
            int lowestIndex = -1;
            int lowestRate = int.MaxValue;

            foreach (var roleEntry in shift.AssignedEmployees)
            {
                for (int i = 0; i < roleEntry.Value.Count; i++)
                {
                    if (roleEntry.Value[i].Rate < lowestRate)
                    {
                        lowest = roleEntry.Value[i];
                        lowestRole = roleEntry.Key;
                        lowestIndex = i;
                        lowestRate = roleEntry.Value[i].Rate;
                    }
                }
            }

            if (lowest == null) return false;

            // בדוק אילו עובדים כבר במשמרת
            var employeesInShift = shift.AssignedEmployees.Values
                .SelectMany(emps => emps)
                .Where(e => e != lowest)
                .ToHashSet();

            // מצא עובד טוב יותר
            var better = Employees
                .Where(e => e.roles.Contains(lowestRole) &&
                       e.Rate > lowest.Rate &&
                       !employeesInShift.Contains(e) &&
                       (!assignments.ContainsKey(e) || !assignments[e].Contains(shiftKey)))
                .OrderByDescending(e => e.Rate)
                .FirstOrDefault();

            if (better == null) return false;

            if (assignments.ContainsKey(lowest))
                assignments[lowest].Remove(shiftKey);

            shift.AssignedEmployees[lowestRole][lowestIndex] = better;

            if (!assignments.ContainsKey(better))
                assignments[better] = new HashSet<string>();

            assignments[better].Add(shiftKey);

            return true;
        }

        // הפחתת עלויות
        private static bool ReduceCosts(Shift shift, string shiftKey, Dictionary<Employee, HashSet<string>> assignments)
        {
            if (shift.AssignedEmployees == null || shift.AssignedEmployees.Count == 0) return false;

            // מצא את העובד היקר ביותר
            Employee mostExpensive = null;
            string expRole = null;
            int expIndex = -1;
            int highestSalary = 0;

            foreach (var roleEntry in shift.AssignedEmployees)
            {
                for (int i = 0; i < roleEntry.Value.Count; i++)
                {
                    if (roleEntry.Value[i].HourlySalary > highestSalary)
                    {
                        mostExpensive = roleEntry.Value[i];
                        expRole = roleEntry.Key;
                        expIndex = i;
                        highestSalary = roleEntry.Value[i].HourlySalary;
                    }
                }
            }

            // טיפול רק בעובדים יקרים במיוחד
            if (mostExpensive == null || highestSalary <= 50) return false;

            // בדוק אילו עובדים כבר במשמרת
            var employeesInShift = shift.AssignedEmployees.Values
                .SelectMany(emps => emps)
                .Where(e => e != mostExpensive)
                .ToHashSet();

            // מצא עובד זול יותר
            var cheaper = Employees
                .Where(e => e.roles.Contains(expRole) &&
                       e.HourlySalary < mostExpensive.HourlySalary * 0.8 &&
                       e.Rate >= mostExpensive.Rate * 0.8 &&
                       !employeesInShift.Contains(e) &&
                       (!assignments.ContainsKey(e) || !assignments[e].Contains(shiftKey)))
                .OrderByDescending(e => e.Rate)
                .ThenBy(e => e.HourlySalary)
                .FirstOrDefault();

            if (cheaper == null) return false;

            if (assignments.ContainsKey(mostExpensive))
                assignments[mostExpensive].Remove(shiftKey);

            shift.AssignedEmployees[expRole][expIndex] = cheaper;

            if (!assignments.ContainsKey(cheaper))
                assignments[cheaper] = new HashSet<string>();

            assignments[cheaper].Add(shiftKey);

            return true;
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
