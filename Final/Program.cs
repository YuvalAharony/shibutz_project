using EmployeeSchedulingApp;
using Final;
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
    // מחלקה ראשית המפעילה את האלגוריתם הגנטי ליצירת סידור עבודה אופטימלי
    public class Program
    {
        // משתנים גלובליים 
        #region Data
        public static Random random = new Random();
        public static List<Employee> Employees = new List<Employee>();
        public static DataBaseHelper DataBaseHelper = new DataBaseHelper();
        public static List<Branch> Branches = new List<Branch>();
        public static Population pop=new Population(new List<Chromosome>());


        // קבועים להגדרת האלגוריתם הגנטי
        public const int ChromosomesEachGene = 400;
        public const int Generations = 200;
        public const int hoursPerWeek = 42;
        public const int hoursPerDay = 9;
        public const int hoursPerShift = 9;
        const int maxGenerationsWithoutImprovement = 50;


        // קבועים למשקלים של פונקציית הכושר
        private const double MissingEmployeePenalty = 350.0;
        private const double MentorBonus = 200.0;
        private const double MentorPenalty = 200.0;
        private const double SpecialEventHighRatingBonus = 140.0;
        private const double SpecialEventLowRatingPenalty = 140.0;
        private const double RegularEventHighRatingBonus = 70.0;
        private const double RegularEventLowRatingPenalty = 70.0;
        private const double BalancedTeamBonus = 50.0;
        private const double CostDivisor = 200.0;
        private const double WeeklyHoursOveragePenalty = 10.0;
        private const double DailyHoursOveragePenalty = 15.0;
        private const double GoodRatingThreshold = 3.5;
        private const double MinBalancedExperienceRatio = 0.3;
        private const double MaxBalancedExperienceRatio = 0.7;

        // מונים לסטטיסטיקות
        public static int succefulMutation = 0; // מונה מוטציות מוצלחות
        public static int UnsuccefulMutation = 0; // מונה מוטציות לא מוצלחות
        public static int generationCount = 0; // מונה דורות בוצעו באלגוריתם
        #endregion

        // אלגוריתם ראשי למציאת סידור עבודה אופטימלי ע"י אלגוריתם גנטי
        // פרמטרים
        // username - שם המשתמש שעבורו יש ליצור את הסידור
        // ערך מוחזר: אין
        public static void createSceduele(string username)
        {
            InitializeAlgorithm(username);
            RunGeneticAlgorithm();
            ShowResults();
        }

        // פונקציה לאתחול האלגוריתם הגנטי
        // פרמטרים
        // username - שם המשתמש שעבורו יש לאתחל את האלגוריתם
        // ערך מוחזר: אין
        private static void InitializeAlgorithm(string username)
        {
            // איפוס מונים
            succefulMutation = 0;
            UnsuccefulMutation = 0;
            generationCount = 0;

            //יצירת אוכלוסייה חדשה בכל פעם שמפעילים את האלגוריתם
            pop = new Population(new List<Chromosome>());
            //טעינת כל הנתונים של המשתמש המחובר
            DataBaseHelper.LoadDataForUser(username, Branches, Employees);

            //יצירת אוכלוסייה ראשונית- שלב 1 באלגוריתם הגנטי 
            pop = initializeFirstPopulation(pop);
        }

        // פונקציה המריצה את האלגוריתם הגנטי
        // פרמטרים: אין
        // ערך מוחזר: אין
        private static void RunGeneticAlgorithm()
        {
            double previousBestFitness = double.MinValue;
            int noImprovementCount = 0;
            int i;
            //לולאה הרצה לפי מספר הדורות הנקבע ויוצרת דור חדש של צאצאים
            for (i = 0; i < Generations; i++)
            {
                // שמירת הכרומוזומים הכי טובים לדור הבא
                pop.Chromoshomes = pop.Chromoshomes.OrderByDescending(x => x.Fitness).Take(ChromosomesEachGene).ToList();

                // בדיקה אם יש שיפור בין דורות
                double currentBestFitness = pop.Chromoshomes.First().Fitness;
                if (currentBestFitness - previousBestFitness < 0.001)
                {
                    noImprovementCount++;
                    if (noImprovementCount >= maxGenerationsWithoutImprovement)
                    {
                        generationCount = i;
                        // עצירה מוקדמת אם אין שיפור במשך מספר דורות
                        return;
                    }
                }
                else
                {
                    noImprovementCount = 0;
                    previousBestFitness = currentBestFitness;
                }

                //שיפור הכרומוזומים ע"י הכלאה בין זוגות כרומוזומים
                crossover(pop);

                //שיפור הכרומוזומים ע"י מוטציות 
                Mutation(pop);
            }
            generationCount = i;
            return;
        }

        // פונקציה להצגת תוצאות האלגוריתם
        // פרמטרים: אין
        // ערך מוחזר: אין
        private static void ShowResults()
        {
            // הצגת הצלחה למשתמש
            MessageBox.Show("נוצר בהצלחה", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Console.WriteLine($"מוטציות מוצלחות: {succefulMutation}");
            Console.WriteLine($"מוטציות לא מוצלחות: {UnsuccefulMutation}");
            Console.WriteLine($"מספר דורות: {generationCount}");
        }

        #region InitializeFirstPopulation 
        // פונקציה היוצרת אוכלוסייה ראשונית- שלב ראשון באלגוריתם הגנטי
        // פרמטרים
        // pop - אוכלוסייה ריקה להכנסת הכרומוזומים אליה
        // ערך מוחזר: אוכלוסייה מלאה בכרומוזומים
        public static Population initializeFirstPopulation(Population pop)
        {
            //מיון עובדים לפי ציון-ניתן למיין רק פעם אחת כי הציון לא משתנה
            List<Employee> employeesSortedByRate = sort_employees_by_rate(Employees);
            //ניצור כרומוזום בעבור כמות הכרומוזומים הרצויה בכל דור
            for (int i = 0; i < ChromosomesEachGene; i++)
            {
                //שחזור המשמרות של כל עובד בשביל יצירת הכרומוזום הבא
                RestoreEmployeesRequestedShifts();

                //אתחול הכרומוזום
                Chromosome c = initializeChoromosome(employeesSortedByRate);

                //קביעת ציון הכושר של הכרומוזום
                c.Fitness = CalculateChromosomeFitness(c);

                //הוספת הכרומוזום לאוכלוסייה
                pop.Chromoshomes.Add(c);
            }
            return pop;
        }


        // פונקציה שמשחזרת את המשמרות המבוקשות של כל עובד
        // פרמטרים: אין
        // ערך מוחזר: אין
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

        // פונקציה שמטרתה ליצור כרומוזום חדש
        // פרמטרים
        // employeesSortedByRate - רשימת עובדים ממויינת לפי ציון
        // ערך מוחזר: כרומוזום חדש
        private static Chromosome initializeChoromosome(List<Employee> employeesSortedByRate)
        {
            //מיפוי עובדים לפי משמרות מבוקשות
            Dictionary<int, List<Employee>> employeesMappedByRequestedShifts = mappingEmployeesByRequestedShifts();
            //מיפוי עובדים לפי תפקיד
            Dictionary<string, List<Employee>> employeesMappedByRequestedRoles = mappingEmployeesByRole();
            //אתחול כרומוזום חדש
            Chromosome ch = new Chromosome();
            //יצירת העתק של הסניפים והמשמרות
            List<Branch> branchesCopy = CopyBranchesAndShifts();

            //ערבוב הסניפים במטרה ליצור גיוון בין כרומוזום לכרומוזום
            List<Branch> shuffledBranches = branchesCopy.OrderBy(x => random.Next()).ToList();

            //מעבר על הסניפים
            foreach (Branch br in shuffledBranches)
            {
                //מילוי המשמרות של הסניף 
                br.Shifts = fill_brach_shifts(br, employeesMappedByRequestedShifts, employeesMappedByRequestedRoles, employeesSortedByRate);
                //הוספת הסניף והמשמרות שלו למילון המשמרות של הכרומוזום
                ch.Shifts.Add(br.Name, br.Shifts);
            }

            return ch;
        }

        // פונקציה ליצירת העתקים של הסניפים והמשמרות
        // פרמטרים: אין
        // ערך מוחזר: רשימת הסניפים המועתקים
        private static List<Branch> CopyBranchesAndShifts()
        {
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
                        EventType = originalShift.EventType,
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

            return branchesCopy;
        }

        // פונקציה הממינת את העובדים לפי הזמינות שלהם- הכי פחות זמינים בהתחלה
        // פרמטרים
        // employees - רשימת העובדים למיון
        // ערך מוחזר: רשימת העובדים ממויינת לפי זמינות
        public static List<Employee> sort_employees_by_availabilty(List<Employee> employees)
        {
            return employees.OrderBy(e => e.requestedShifts?.Count ?? 0).ToList();
        }

        // פונקצייה הממינת את העובדים לפי הציון שלהם
        // פרמטרים
        // employees - רשימת העובדים למיון
        // ערך מוחזר: רשימת העובדים ממויינת לפי ציון
        public static List<Employee> sort_employees_by_rate(List<Employee> employees)
        {
            return employees.OrderByDescending(e => e.Rate).ToList();
        }

        // פונקציה הממפה את העובדים לפי משמרות מבוקשות
        // פרמטרים: אין
        // ערך מוחזר: מילון של משמרות והעובדים שביקשו אותן
        public static Dictionary<int, List<Employee>> mappingEmployeesByRequestedShifts()
        {
            return Employees
                .SelectMany(emp => emp.requestedShifts, (emp, shiftId) => new { shiftId, emp })
                .GroupBy(entry => entry.shiftId)
                .ToDictionary(group => group.Key, group => group.Select(entry => entry.emp).ToList());
        }

        // פונקציה הממפה את העובדים לפי התפקיד שלהם
        // פרמטרים: אין
        // ערך מוחזר: מילון של תפקידים והעובדים שמתאימים להם
        public static Dictionary<string, List<Employee>> mappingEmployeesByRole()
        {
            return Employees
                .SelectMany(emp => emp.roles, (emp, role) => new { role, emp })
                .GroupBy(entry => entry.role)
                .ToDictionary(group => group.Key, group => group.Select(entry => entry.emp).ToList());
        }

        // פונקציה המחזירה רשימת משמרות חופפות בהתאם לעובד ולמשמרת ששובץ בה
        // פרמטרים
        // employee - העובד שיש לבדוק
        // assignedShift - המשמרת שהעובד שובץ בה
        // ערך מוחזר: רשימת מזהי המשמרות החופפות
        public static List<int> GetOverlappingShifts(Employee employee, Shift assignedShift)
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

        // פונקציה המקבלת מזהה משמרת ומחזירה את המשמרת
        // פרמטרים
        // shiftId - מזהה המשמרת לחיפוש
        // ערך מוחזר: אובייקט המשמרת אם נמצא, אחרת null
        public static Shift FindShiftById(int shiftId)
        {
            //מעבר על כל המשמרות בכל הסניפים והחזרת המשמרת אם המזהים חופפים
            foreach (Branch branch in Branches)
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

        // מילוי משמרות של סניף מסוים בעובדים
        // פרמטרים
        // br - הסניף שיש למלא את משמרותיו
        // employeesMappedByRequestedShifts - מילון של עובדים לפי משמרות מבוקשות
        // employeesMappedByRequestedRoles - מילון של עובדים לפי תפקידים
        // employeesSortedByRate - רשימת עובדים ממוינת לפי ציון
        // ערך מוחזר: רשימת המשמרות של הסניף לאחר המילוי
        public static List<Shift> fill_brach_shifts(Branch br, Dictionary<int, List<Employee>> employeesMappedByRequestedShifts,
            Dictionary<string, List<Employee>> employeesMappedByRequestedRoles,
            List<Employee> employeesSortedByRate)
        {


            //ערבוב משמרות הסניף על מנת ליצור גיוון בין הכרומזומים
            List<Shift> shuffledShifts = br.Shifts.OrderBy(x => random.Next()).ToList();

            //מעבר על המשמרות
            foreach (Shift sh in shuffledShifts)
            {
                FillShiftWithEmployees(sh, employeesMappedByRequestedShifts, employeesMappedByRequestedRoles, employeesSortedByRate);
            }

            return br.Shifts;
        }

        // פונקציה למילוי משמרת בעובדים לפי תפקידים נדרשים
        // פרמטרים
        // sh - המשמרת שיש למלא
        // employeesMappedByRequestedShifts - מילון של עובדים לפי משמרות מבוקשות
        // employeesMappedByRequestedRoles - מילון של עובדים לפי תפקידים
        // employeesSortedByRate - רשימת עובדים ממוינת לפי ציון
        // ערך מוחזר: אין
        private static void FillShiftWithEmployees(
            Shift sh,
            Dictionary<int, List<Employee>> employeesMappedByRequestedShifts,
            Dictionary<string, List<Employee>> employeesMappedByRequestedRoles,
            List<Employee> employeesSortedByRate)
        {
            //מיון העובדים לפי זמינות
            List<Employee> employeesSortedByavailabilty = sort_employees_by_availabilty(Employees);

            //הבאה מהמילון את רשימת העובדים שיכולים לעבוד במשמרת הנוכחית
            employeesMappedByRequestedShifts.TryGetValue(sh.Id, out List<Employee> employeesAvaliableForShift);

            //מעבר על כל התפקידים המבוקשים במשמרת הנוכחית
            foreach (var entry in sh.RequiredRoles)
            {
                //מעבר על התפקיד הנוכחי לפי הכמות הנדררשת ממנו
                for (int i = 0; i < entry.Value; i++)
                {
                    // הבאה מהמילון את העובדים המתאימים לתפקיד הנוכחי
                    employeesMappedByRequestedRoles.TryGetValue(entry.Key, out List<Employee> employeesAvaliableForRole);

                    // בחירת אסטרטגיית השיבוץ - לפי ציון או לפי זמינות
                    List<Employee> currenList = ChooseAssignmentStrategy(employeesSortedByRate, employeesSortedByavailabilty);

                    // שיבוץ עובד מתאים למשמרת
                    AssignEmployeeToShift(sh, entry.Key, currenList, employeesAvaliableForShift, employeesAvaliableForRole, employeesMappedByRequestedShifts);
                }
            }
        }

        // בחירת אסטרטגיית שיבוץ - לפי ציון או לפי זמינות
        // פרמטרים
        // employeesSortedByRate - רשימת עובדים ממוינת לפי ציון
        // employeesSortedByavailabilty - רשימת עובדים ממוינת לפי זמינות
        // ערך מוחזר: רשימת העובדים שנבחרה
        private static List<Employee> ChooseAssignmentStrategy(
            List<Employee> employeesSortedByRate,
            List<Employee> employeesSortedByavailabilty)
        {
            // הגרלת מספר בין 1 ל-2 שתשמש לבחירת הרשימה
            int currentListIdentifier = random.Next(1, 3);

            // החזרת הרשימה הנבחרת לפי המספר שהוגרל
            if (currentListIdentifier == 1)
                return employeesSortedByRate;

            return employeesSortedByavailabilty;


        }

        // שיבוץ עובד למשמרת
        // פרמטרים
        // sh - המשמרת לשיבוץ
        // role - התפקיד לשיבוץ
        // employeesList - רשימת העובדים המועמדים לשיבוץ
        // employeesAvaliableForShift - רשימת העובדים הזמינים למשמרת
        // employeesAvaliableForRole - רשימת העובדים המתאימים לתפקיד
        // employeesMappedByRequestedShifts - מילון של עובדים לפי משמרות מבוקשות
        // ערך מוחזר: אין
        private static void AssignEmployeeToShift(
            Shift sh,
            string role,
            List<Employee> employeesList,
            List<Employee> employeesAvaliableForShift,
            List<Employee> employeesAvaliableForRole,
            Dictionary<int, List<Employee>> employeesMappedByRequestedShifts)
        {
            // בדיקה אם יש עובד לשיבוץ במשמרת
            if (employeesAvaliableForShift != null)
            {
                // בחירת העובד הראשון מהרשימה שנבחרה שזמין למשמרת הנוכחית ולתפקיד הנדרש
                Employee selectedEmployee = employeesList.FirstOrDefault(emp =>
                    employeesAvaliableForShift.Contains(emp) &&
                    employeesAvaliableForRole.Contains(emp));

                // בדיקה אם נמצא עובד
                if (selectedEmployee != null)
                {
                    // בדיקה אם כבר שובץ עובד לתפקיד הנוכחי
                    if (!sh.AssignedEmployees.ContainsKey(role))
                    {
                        // הוספת התפקיד הנוכחי למילון העובדים ששמשובצים למשמרת הנוכחית
                        sh.AssignedEmployees[role] = new List<Employee>();
                    }

                    // הוספת העובד למשמרת לתפקיד המתאים
                    sh.AssignedEmployees[role].Add(selectedEmployee);

                    // טיפול במשמרות חופפות
                    HandleOverlappingShifts(selectedEmployee, sh, employeesMappedByRequestedShifts);
                }
            }
        }

        // פונקציה המטפלת במשמרות חופפות כשעובד משובץ למשמרת
        // פרמטרים
        // employee - העובד ששובץ למשמרת
        // shift - המשמרת שהעובד שובץ בה
        // employeesMappedByRequestedShifts - מילון של עובדים לפי משמרות מבוקשות
        // ערך מוחזר: אין
        private static void HandleOverlappingShifts(
            Employee employee,
            Shift shift,
            Dictionary<int, List<Employee>> employeesMappedByRequestedShifts)
        {
            // קבלת מזהי המשמרות החופפות שיש להסיר מהמשמרות המבוקשות של העובד הנוכחי
            List<int> idToRemove = GetOverlappingShifts(employee, shift);

            // מעבר על המזהים הללו
            foreach (int id in idToRemove)
            {
                // קבלת רשימת העובדים הזמינים למשמרת (אם יש)
                if (employeesMappedByRequestedShifts.TryGetValue(id, out List<Employee> shiftToRemove))
                {
                    // הורדת העובד מרשימה זו
                    shiftToRemove.Remove(employee);

                    // מחיקת רשימת העובדים הזמינים למשמרת זו מהמילון אם לא נותרו עובדים זמינים
                    if (shiftToRemove.Count == 0)
                    {
                        employeesMappedByRequestedShifts.Remove(id);
                    }
                }
            }
        }
        #endregion

        #region Crrosover
        // פונקציה היוצרת כרומוזומים חדשים בעזרת הכלאה
        // פרמטרים
        // pop - האוכלוסייה שעליה יש לבצע הכלאה
        // ערך מוחזר: אין
        public static void crossover(Population pop)
        {
            //אתחול רשימת הכרומוזומים החדשים
            List<Chromosome> newOffspring = new List<Chromosome>();
            int desiredOffspringCount = ChromosomesEachGene * 3 / 4;

            // אליטיזם - שמירה על הכרומוזום הטוב ביותר
            Chromosome bestChromosome = SaveEliteChromosome(pop.Chromoshomes);

            // יצירת צאצאים חדשים
            CreateNewOffspring(pop.Chromoshomes, newOffspring, desiredOffspringCount);

            // הוספת הצאצאים החדשים לאוכלוסייה
            pop.Chromoshomes.AddRange(newOffspring);

            // בחירת הכרומוזומים הטובים ביותר לדור הבא
            SelectBestChromosomesForNextGeneration(pop, bestChromosome);
        }

        // פונקציה השומרת על הכרומוזום הטוב ביותר-אליטיזם
        // פרמטרים
        // chromosomes - רשימת הכרומוזומים
        // ערך מוחזר: העתק של הכרומוזום הטוב ביותר
        private static Chromosome SaveEliteChromosome(List<Chromosome> chromosomes)
        {
            Chromosome bestChromosome = null;
            if (chromosomes.Count > 0)
                bestChromosome = CopyChromosome(chromosomes[0]);

            return bestChromosome;
        }

        // פונקצי היוצרת צאצאים חדשים באמצעות הכלאה
        // פרמטרים
        // parentChromosomes - רשימת הכרומוזומים ההורים
        // newOffspring - רשימת הצאצאים החדשים
        // desiredOffspringCount - מספר הצאצאים הרצוי
        // ערך מוחזר: אין
        private static void CreateNewOffspring(
            List<Chromosome> parentChromosomes,
            List<Chromosome> newOffspring,
            int desiredOffspringCount)
        {
            for (int i = 0; i < desiredOffspringCount; i++)
            {
                // בחירת הורים באמצעות טורניר
                Chromosome parent1 = SelectParentByTournament(parentChromosomes);
                Chromosome parent2 = SelectParentByTournament(parentChromosomes);

                // ניסיון להבטיח שההורים שונים
                EnsureDifferentParents(parent1, parent2, parentChromosomes);

                // ביצוע הכלאה
                Chromosome offspring = PerformCrossover(parent1, parent2);

                // חישוב ציון הכושר לצאצא החדש
                offspring.Fitness = CalculateChromosomeFitness(offspring);

                // הוספת הצאצא לרשימת הצאצאים החדשים
                newOffspring.Add(offspring);
            }
        }

        // פונקציה המנסה להבטיח שההורים שונים
        // פרמטרים
        // parent1 - הורה ראשון
        // parent2 - הורה שני
        // chromosomes - רשימת הכרומוזומים האפשריים
        // ערך מוחזר: אין
        private static void EnsureDifferentParents(
            Chromosome parent1,
            Chromosome parent2,
            List<Chromosome> chromosomes)
        {
            const int maxAttempts = 3;
            int attempts = 0;

            while (parent1 == parent2 && attempts < maxAttempts && chromosomes.Count > 1)
            {
                parent2 = SelectParentByTournament(chromosomes);
                attempts++;
            }
        }

        // פונקציה הבוחרת את הכרומוזומים הטובים ביותר לדור הבא
        // פרמטרים
        // pop - האוכלוסייה לעדכון
        // bestChromosome - הכרומוזום הטוב ביותר
        // ערך מוחזר: אין
        private static void SelectBestChromosomesForNextGeneration(Population pop, Chromosome bestChromosome)
        {
            // בחירת הכרומוזומים הטובים ביותר
            pop.Chromoshomes = pop.Chromoshomes
                .OrderByDescending(ch => ch.Fitness)
                .Take(ChromosomesEachGene - 1) // השארת מקום לטוב ביותר שנשמר
                .ToList();

            // הוספת הכרומוזום הטוב ביותר בחזרה-אליטיזם
            if (bestChromosome != null)
                pop.Chromoshomes.Add(bestChromosome);
        }

        // פונקציה הבוחרת כרומוזום מהאוכלוסייה באמצעות שיטת הטורניר
        // פרמטרים
        // chromosomes - רשימת הכרומוזומים
        // ערך מוחזר: הכרומוזום הנבחר
        private static Chromosome SelectParentByTournament(List<Chromosome> chromosomes)
        {
            // טורניר עם 3 מועמדים אקראיים
            Chromosome best = null;
            double bestFitness = double.MinValue;

            // טיפול ברשימה ריקה
            if (chromosomes.Count == 0)
                return null;

            // הגרלת 3 מועמדים אקראיים ובחירת הטוב ביותר
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

        // פונקציה המבצעת את ההכלאה בין שני ההורים בפועל
        // פרמטרים
        // parent1 - הורה ראשון
        // parent2 - הורה שני
        // ערך מוחזר: הכרומוזום הצאצא
        private static Chromosome PerformCrossover(Chromosome parent1, Chromosome parent2)
        {
            Chromosome offspring = new Chromosome();
            offspring.Shifts = new Dictionary<string, List<Shift>>();

            // מעקב אחר שיבוץ עובדים למניעת קונפליקטים
            Dictionary<Employee, HashSet<string>> employeeAssignments = new Dictionary<Employee, HashSet<string>>();

            // קבלת כל שמות הסניפים 
            var allBranchNames = new HashSet<string>(parent1.Shifts.Keys);

            // מעבר על כל הסניפים
            foreach (string branchName in allBranchNames)
            {
                CreateOffspringShiftsForBranch(parent1, parent2, offspring, employeeAssignments, branchName);
            }

            return offspring;
        }

        // פונקציה היוצרת משמרות חדשות לצאצא משני ההורים
        // פרמטרים
        // parent1 - הורה ראשון
        // parent2 - הורה שני
        // offspring - הצאצא
        // employeeAssignments - מילון מעקב אחר שיבוץ עובדים
        // branchName - שם הסניף
        // ערך מוחזר: אין
        private static void CreateOffspringShiftsForBranch(
            Chromosome parent1,
            Chromosome parent2,
            Chromosome offspring,
            Dictionary<Employee, HashSet<string>> employeeAssignments,
            string branchName)
        {
            // יצירת מיפוי המשמרות לפי זמן בשבוע
            Dictionary<string, Shift> shiftsMap1 = CreateShiftsMap(parent1, branchName);
            Dictionary<string, Shift> shiftsMap2 = CreateShiftsMap(parent2, branchName);
           
           
            // קבלת כל זמני המשמרות
            HashSet<string> allSlots = new HashSet<string>(shiftsMap1.Keys.Concat(shiftsMap2.Keys));

            //יצירת המשמרות 
            List<Shift> offspringShifts = CreateOffspringShifts(
                allSlots, shiftsMap1, shiftsMap2, employeeAssignments);

            offspring.Shifts[branchName] = offspringShifts;
        }

        // פונקציה היוצרת את מפת המשמרות
        // פרמטרים
        // parent - הורה
        // branchName - שם הסניף
        // ערך מוחזר: מילון של מזהי משמרות ואובייקטי משמרות
        private static Dictionary<string, Shift> CreateShiftsMap(Chromosome parent, string branchName)
        {
            try
            {
                return parent.Shifts[branchName]
                .ToDictionary(s => $"{s.day}_{s.TimeSlot}", s => s);
            }
            catch
            {
                MessageBox.Show("לא ניתן שיהיו שתי משמרות זו", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            
        }

        // פונקציה היוצרת את משמרות הצאצא בפועל
        // פרמטרים
        // slots - מזהי זמני המשמרות
        // shiftsMap1 - מפת משמרות של הורה ראשון
        // shiftsMap2 - מפת משמרות של הורה שני
        // employeeAssignments - מילון מעקב אחר שיבוץ עובדים
        // ערך מוחזר: רשימת המשמרות החדשות
        private static List<Shift> CreateOffspringShifts(
            HashSet<string> slots,
            Dictionary<string, Shift> shiftsMap1,
            Dictionary<string, Shift> shiftsMap2,
            Dictionary<Employee, HashSet<string>> employeeAssignments)
        {
            //אתחול רשימת המשמרות החדשה
            List<Shift> offspringShifts = new List<Shift>();

            //מעבר על כל המשמרות
            foreach (string slot in slots)
            {
                // יצירת משמרת חדשה
                Shift offspringShift = CreateOffspringShift(slot, shiftsMap1, shiftsMap2);

                // שיבוץ עובדים למשמרת החדשה
                AssignEmployeesToOffspringShift(
                    slot, shiftsMap1, shiftsMap2, offspringShift, employeeAssignments);

                //הוספת המשמרת החדשה לרשימת המשמרות
                offspringShifts.Add(offspringShift);
            }

            return offspringShifts;
        }

        // פונקציה היוצרת משמרת חדשה לצאצא
        // פרמטרים
        // slot - מזהה זמן המשמרת
        // shiftsMap1 - מפת משמרות של הורה ראשון
        // shiftsMap2 - מפת משמרות של הורה שני
        // ערך מוחזר: אובייקט משמרת חדש
        private static Shift CreateOffspringShift(
            string slot,
            Dictionary<string, Shift> shiftsMap1,
            Dictionary<string, Shift> shiftsMap2)
        {
            //קבלת פרטים על המשמרת
            string[] parts = slot.Split('_');
            string day = parts[0];
            string timeSlot = parts[1];

            // בחירת משמרת בסיס מאחד ההורים
            Shift baseShift = SelectBaseShift(slot, shiftsMap1, shiftsMap2);

            // יצירת משמרת חדשה עם תכונות בסיס
            return new Shift
            {
                Id = baseShift.Id,
                branch = baseShift.branch,
                day = baseShift.day,
                TimeSlot = baseShift.TimeSlot,
                EventType = baseShift.EventType,
                RequiredRoles = new Dictionary<string, int>(baseShift.RequiredRoles),
                AssignedEmployees = new Dictionary<string, List<Employee>>()
            };
        }

        // פונקציה הבוחרת משמרת בסיס מאחד ההורים
        // פרמטרים
        // slot - מזהה זמן המשמרת
        // shiftsMap1 - מפת משמרות של הורה ראשון
        // shiftsMap2 - מפת משמרות של הורה שני
        // ערך מוחזר: אובייקט המשמרת הנבחר
        private static Shift SelectBaseShift(
            string slot,
            Dictionary<string, Shift> shiftsMap1,
            Dictionary<string, Shift> shiftsMap2)
        {
            return random.Next(2) == 0 ? shiftsMap1[slot] : shiftsMap2[slot];
        }

        // פונקציה המשבצת עובדים למשמרת הצאצא
        // פרמטרים
        // slot - מזהה זמן המשמרת
        // shiftsMap1 - מפת משמרות של הורה ראשון
        // shiftsMap2 - מפת משמרות של הורה שני
        // offspringShift - המשמרת החדשה
        // employeeAssignments - מילון מעקב אחר שיבוץ עובדים
        // ערך מוחזר: אין
        private static void AssignEmployeesToOffspringShift(
            string slot,
            Dictionary<string, Shift> shiftsMap1,
            Dictionary<string, Shift> shiftsMap2,
            Shift offspringShift,
            Dictionary<Employee, HashSet<string>> employeeAssignments)
        {
            // איסוף כל התפקידים למשמרת
            var allRoles = shiftsMap1[slot].AssignedEmployees.Keys;

            foreach (string role in allRoles)
            {
                offspringShift.AssignedEmployees[role] = new List<Employee>();

                // קבלת רשימות העובדים המתאימים לתפקיד משני ההורים
                List<Employee> employees1 = GetEmployeesForRole(slot, shiftsMap1, role);
                List<Employee> employees2 = GetEmployeesForRole(slot, shiftsMap2, role);

                // בחירת עובדים לפי אסטרטגיה
                List<Employee> selectedEmployees = SelectEmployeesByStrategy(
                    employees1, employees2, offspringShift, role);

                // הוספת העובדים שנבחרו, תוך הימנעות מקונפליקטים
                AddSelectedEmployeesToShift(offspringShift, role, selectedEmployees, employeeAssignments);
            }
        }

        // פונקציה המקבלת רשימת עובדים לתפקיד מסוים
        // פרמטרים
        // slot - מזהה זמן המשמרת
        // shiftsMap - מפת משמרות
        // role - התפקיד המבוקש
        // ערך מוחזר: רשימת העובדים המתאימים לתפקיד
        private static List<Employee> GetEmployeesForRole(
            string slot,
            Dictionary<string, Shift> shiftsMap,
            string role)
        {
            List<Employee> employees = new List<Employee>();

            if (shiftsMap.ContainsKey(slot) &&
                shiftsMap[slot].AssignedEmployees != null &&
                shiftsMap[slot].AssignedEmployees.ContainsKey(role))
            {
                employees = shiftsMap[slot].AssignedEmployees[role];
            }

            return employees;
        }

        // בחירת עובדים לפי אסטרטגיות שונות
        // פרמטרים
        // employees1 - רשימת עובדים מהורה ראשון
        // employees2 - רשימת עובדים מהורה שני
        // offspringShift - המשמרת החדשה
        // role - התפקיד המבוקש
        // ערך מוחזר: רשימת העובדים הנבחרים
        private static List<Employee> SelectEmployeesByStrategy(
            List<Employee> employees1,
            List<Employee> employees2,
            Shift offspringShift,
            string role)
        {
            // הגדרת האסטרטגיות כמערך של פונקציות
            var strategies = new Func<List<Employee>>[]
            {
                // אסטרטגיה ראשונה- לקחת את הורה 1 אם קיים, אחרת את הורה 2
                () => employees1.Count > 0
                      ? new List<Employee>(employees1)
                      : new List<Employee>(employees2),
                  
                // אסטרטגיה שנייה- לקחת את הורה 2 אם קיים, אחרת את הורה 3
                () => employees2.Count > 0
                      ? new List<Employee>(employees2)
                      : new List<Employee>(employees1),
                  
                //אסטרטגיה שלוש- שילוב ההורים
                () => MixEmployeesFromBothParents(employees1, employees2),
            };

            //בחירת אסטרגייה באופן רנדומלי
            int strategyIndex = random.Next(strategies.Length);
            return strategies[strategyIndex]();
        }

        // פונקציה המערבבת עובדים משני ההורים
        // פרמטרים
        // employees1 - רשימת עובדים מהורה ראשון
        // employees2 - רשימת עובדים מהורה שני
        // ערך מוחזר: רשימת עובדים משולבת
        private static List<Employee> MixEmployeesFromBothParents(List<Employee> employees1, List<Employee> employees2)
        {
            List<Employee> selectedEmployees = new List<Employee>();

            // קח חצי מכל הורה כשאפשר
            int count1 = Math.Min(employees1.Count, employees1.Count / 2 + 1);
            int count2 = Math.Min(employees2.Count, employees2.Count / 2 + 1);

            //הוספת עובדים מהורה 1
            for (int i = 0; i < count1 && i < employees1.Count; i++)
            {
                selectedEmployees.Add(employees1[i]);
            }
            //2 הוספת עובדים מהורה 
            for (int i = 0; i < count2 && i < employees2.Count; i++)
            {
                selectedEmployees.Add(employees2[i]);
            }

            return selectedEmployees;
        }

        // פונקציה המוסיפה עובדים שנבחרו למשמרת תוך הימנעות מחפיפות
        // פרמטרים
        // offspringShift - המשמרת החדשה
        // role - התפקיד המבוקש
        // selectedEmployees - רשימת העובדים שנבחרו
        // employeeAssignments - מילון מעקב אחר שיבוץ עובדים
        // ערך מוחזר: אין
        private static void AddSelectedEmployeesToShift(
            Shift offspringShift,
            string role,
            List<Employee> selectedEmployees,
            Dictionary<Employee, HashSet<string>> employeeAssignments)
        {
            //קבלת יום ומן המשמרת
            string shiftKey = $"{offspringShift.day}_{offspringShift.TimeSlot}";

            //מעבר על העובדים שנבחרו
            foreach (Employee emp in selectedEmployees)
            {
                //בדיקה אם עובד משובץ במשמרת חופפת
                if (!IsEmployeeAlreadyAssigned(emp, shiftKey, employeeAssignments))
                {
                    //הוספת העובד אם לא משובץ במשמרת חופפת
                    offspringShift.AssignedEmployees[role].Add(emp);

                    if (!employeeAssignments.ContainsKey(emp))
                        employeeAssignments[emp] = new HashSet<string>();
                    //הוספת המשמרת לרשימת המשמרות של העובד
                    employeeAssignments[emp].Add(shiftKey);
                }
            }
        }

        // פונקציה הבודקת אם עובד משובץ במשמרת חופפת
        // פרמטרים
        // employee - העובד לבדיקה
        // shiftKey - מזהה המשמרת
        // employeeAssignments - מילון מעקב אחר שיבוץ עובדים
        // ערך מוחזר: האם העובד משובץ למשמרת חופפת
        private static bool IsEmployeeAlreadyAssigned(Employee employee, string shiftKey, Dictionary<Employee, HashSet<string>> employeeAssignments)
        {
            return employeeAssignments.ContainsKey(employee) &&
                   employeeAssignments[employee].Contains(shiftKey);
        }

        // פונקציה היוצרת העתק של משמרת
        // פרמטרים
        // originalShift - המשמרת המקורית
        // ערך מוחזר: העתק של המשמרת
        private static Shift CopyShift(Shift originalShift)
        {
            if (originalShift == null)
                return null;
            //העתקת המשמרת
            Shift copy = new Shift
            {
                Id = originalShift.Id,
                branch = originalShift.branch,
                day = originalShift.day,
                TimeSlot = originalShift.TimeSlot,
                EventType = originalShift.EventType,
                RequiredRoles = new Dictionary<string, int>(),
                AssignedEmployees = new Dictionary<string, List<Employee>>()
            };

            // העתקת תפקידים נדרשים
            if (originalShift.RequiredRoles != null)
            {
                foreach (var entry in originalShift.RequiredRoles)
                {
                    copy.RequiredRoles[entry.Key] = entry.Value;
                }
            }

            // העתקת עובדים שמשובצים
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

        // פונקציה היוצרת העתק של רשימת משמרות
        // פרמטרים
        // shifts - רשימת המשמרות המקורית
        // ערך מוחזר: העתק של רשימת המשמרות
        private static List<Shift> CopyShifts(List<Shift> shifts)
        {
            //החזרת רשימה ריקה אם אין משמרות
            if (shifts == null)
                return new List<Shift>();

            List<Shift> copies = new List<Shift>();
            //יצירת העתק לכל משמרת
            foreach (var shift in shifts)
            {
                if (shift != null)
                    copies.Add(CopyShift(shift));
            }

            return copies;
        }

        // פונקציה היוצרת העתק של כרומוזום
        // פרמטרים
        // original - הכרומוזום המקורי
        // ערך מוחזר: העתק של הכרומוזום
        private static Chromosome CopyChromosome(Chromosome original)
        {
            //החזרת ערך ריק אם הכרומוזום ריק
            if (original == null)
                return null;
            //אתחול כרומוזם ההעתקה
            Chromosome copy = new Chromosome();
            copy.Fitness = original.Fitness;
            copy.Shifts = new Dictionary<string, List<Shift>>();
            //יצירת העתק למשמרות כל סניף
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

        #region mutation
        // פונקציה היוצרת כרומוזומים חדשים באמצעות מוצטיה
        // פרמטרים
        // pop - האוכלוסייה שעליה יש לבצע מוטציה
        // ערך מוחזר: אין
        public static void Mutation(Population pop)
        {
            List<Chromosome> newChromosomes = new List<Chromosome>();

            // מעקב אחר התקדמות להתאמת אסטרטגיית המוטציה
            bool isLateStage = IsLateStageOfAlgorithm();

            // הגדלת ניסיונות המוטציה בשלבים מאוחרים
            int mutationAttemptsPerChromosome = isLateStage ? 15 : 8;

            foreach (Chromosome chromosome in pop.Chromoshomes)
            {
                // ביצוע מוטציה לכרומוזום
                Chromosome mutatedChromosome = MutateChromosome(
                    chromosome,
                    mutationAttemptsPerChromosome,
                    isLateStage,
                    out bool wasImproved);

                //  עדכון סטטיסטיקות והוספה לרשימה
                UpdateMutationStatistics(
                    mutatedChromosome,
                    chromosome.Fitness,
                    wasImproved,
                    newChromosomes);
            }

            // הוספת הכרומוזומים המשופרים לאוכלוסייה
            pop.Chromoshomes.AddRange(newChromosomes);
        }

        // פונקציה הבודקת אם האלגוריתם נמצא בשלב מאוחר
        // פרמטרים: אין
        // ערך מוחזר: האם האלגוריתם נמצא בשלב מאוחר
        private static bool IsLateStageOfAlgorithm()
        {
            // חישוב יחס התקדמות לפי מוטציות מוצלחות/לא מוצלחות
            double progressRatio = (double)succefulMutation / (succefulMutation + UnsuccefulMutation + 1);

            // נחשב לשלב מאוחר אם 70% מהמוטציות הן לא מוצלחות
            return progressRatio > 0.7;
        }

        // פונקציה המבצעת מוטציה לכרומוזום
        // פרמטרים
        // chromosome - הכרומוזום לביצוע מוטציה
        // mutationAttemptsPerChromosome - מספר ניסיונות המוטציה לכרומוזום
        // isLateStage - האם האלגוריתם נמצא בשלב מאוחר
        // wasImproved - פרמטר יציאה המציין אם הכרומוזום השתפר
        // ערך מוחזר: הכרומוזום לאחר מוטציה
        private static Chromosome MutateChromosome(
            Chromosome chromosome,
            int mutationAttemptsPerChromosome,
            bool isLateStage,
            out bool wasImproved)
        {
            // יצירת העתק של הכרומוזום למוטציה
            Chromosome mutatedChromosome = CopyChromosome(chromosome);
            wasImproved = false;

            // ניסיונות מרובים לשפר את הכרומוזום באמצעות אסטרטגיות שונות
            for (int i = 0; i < mutationAttemptsPerChromosome; i++)
            {
                // קבלת משמרת אקראית מהכרומוזום
                Shift shift = GetRandomShift(mutatedChromosome);

                // בחירת אסטרטגיית מוטציה בהתאם לשלב ועדכון אם הכרומוזום השתפר
                wasImproved |= ChooseMutationStrategy(shift, isLateStage, chromosome);
            }

            return mutatedChromosome;
        }

        // בדיקה אם עובד משובץ למשמרת חופפת בכרומוזום נתון
        // פרמטרים
        // chromosome - הכרומוזום לבדיקה
        // employee - העובד לבדיקה
        // currentShift - המשמרת הנוכחית
        // ערך מוחזר: האם העובד משובץ למשמרת חופפת
        private static bool IsEmployeeAssignedToOverlappingShift(Chromosome chromosome, Employee employee, Shift currentShift)
        {
            // מעבר על כל הסניפים בכרומוזום
            foreach (var branchEntry in chromosome.Shifts)
            {
                // מעבר על כל המשמרות בסניף
                foreach (var shift in branchEntry.Value)
                {
                    // אם זו אותה משמרת שאנחנו בודקים כרגע, דלג
                    if (shift == currentShift)
                        continue;

                    // אם המשמרת חופפת (אותו יום ואותה שעה)
                    if (shift.day == currentShift.day && shift.TimeSlot == currentShift.TimeSlot)
                    {
                        // בדוק אם העובד משובץ למשמרת זו
                        foreach (var roleEmployees in shift.AssignedEmployees.Values)
                        {
                            if (roleEmployees.Contains(employee))
                                return true; // העובד משובץ למשמרת חופפת
                        }
                    }
                }
            }

            return false; // העובד לא משובץ למשמרת חופפת
        }

        // פונקציה הבוחרת אסטרטגיית מוטציה בהתאם לשלב
        // פרמטרים
        // shift - המשמרת לביצוע מוטציה
        // isLateStage - האם האלגוריתם נמצא בשלב מאוחר
        // ch - הכרומוזום המקורי
        // ערך מוחזר: האם בוצעה מוטציה מוצלחת
        private static bool ChooseMutationStrategy(Shift shift, bool isLateStage, Chromosome ch)
        {
            bool wasImproved = false;

            //בדיקה אם אנחנו בשלב מאוחר
            if (isLateStage)
            {
                // בשלבים מאוחרים, נבצע מוטציות רחבות יותר בסבירות של 30%
                if (random.Next(100) < 30)
                {
                    // ביצוע מוטציה רחבה - החלפת מספר עובדים
                    wasImproved = PerformLargeMutation(shift, ch);
                }
                else
                {
                    // בשלבים מאוחרים, נבצע מוצטיות רגילות בסירות של 70%
                    wasImproved = PerformStandardMutation(shift, ch);
                }
            }
            else
            {
                // בשלבים מוקדמים, השתמש במוטציות סטנדרטיות
                wasImproved = PerformStandardMutation(shift, ch);
            }

            return wasImproved;
        }

        // פונקציה לעדכון סטטיסטיקות המוטציה
        // פרמטרים
        // mutatedChromosome - הכרומוזום לאחר מוטציה
        // originalFitness - ציון הכושר המקורי
        // wasImproved - האם בוצעה מוטציה מוצלחת
        // newChromosomes - רשימת הכרומוזומים החדשים
        // ערך מוחזר: אין
        private static void UpdateMutationStatistics(
            Chromosome mutatedChromosome,
            double originalFitness,
            bool wasImproved,
            List<Chromosome> newChromosomes)
        {
            // אם הכרומוזום השתפר, חישוב מחדש של ציון הכושר והוספה לרשימה
            if (wasImproved)
            {
                //נעלה את מונה המוצטיות המוצלחות
                succefulMutation++;
                //נחשב מחדש את ציון כושר של הכרומוזום
                mutatedChromosome.Fitness = CalculateChromosomeFitness(mutatedChromosome);

                // נשמור רק מוטציות ששיפרו את ציון הכושר
                if (mutatedChromosome.Fitness > originalFitness)
                    newChromosomes.Add(mutatedChromosome);
            }
            //אם לא היה שיפור, נעלה את מונה המוצטיות הלא מוצלחות
            else
            {
                UnsuccefulMutation++;
            }
        }

        // פונקציה המבצעת מוצטיה רחבה- מחליפה מספר עובדים
        // פרמטרים
        // shift - המשמרת לביצוע מוטציה
        // ch - הכרומוזום המקורי
        // ערך מוחזר: האם בוצעה מוטציה מוצלחת
        private static bool PerformLargeMutation(Shift shift, Chromosome ch)
        {
            if (shift == null || shift.AssignedEmployees == null || shift.AssignedEmployees.Count == 0)
                return false;

            // נבחר תפקיד אקראי לביצוע המוטציה
            var roleEntries = shift.AssignedEmployees.Keys.ToList();
            if (roleEntries.Count == 0)
                return false;

            string selectedRole = roleEntries[random.Next(roleEntries.Count)];

            // ננסה לשלוף ממילון העובדים של המשמרת את רשימת העובדים ששובצו לתפקיד
            List<Employee> employees = null;
            if (!shift.AssignedEmployees.TryGetValue(selectedRole, out employees) || employees == null || employees.Count == 0)
                return false;

            // נגריל כמה אחוז מהעובדים נחליף-50%-100%
            int numToReplace = Math.Max(1, random.Next(employees.Count / 2, employees.Count + 1));

            HashSet<Employee> currentEmployees = new HashSet<Employee>(employees);

            // נמצא עובדים המועמדים להחליף
            var potentialReplacements = FindPotentialReplacements(selectedRole, shift.Id, currentEmployees, ch, shift);
            if (potentialReplacements.Count == 0)
                return false;
            //נחליף את העובדים
            return ReplaceEmployees(employees, potentialReplacements, numToReplace, shift.Id);
        }

        // פונקציית עזר המוצאת את העובדים הזמינים להחליף במשמרת ובתפקיד
        // פרמטרים
        // role - התפקיד המבוקש
        // shiftId - מזהה המשמרת
        // currentEmployees - העובדים הנוכחיים במשמרת
        // chromosome - הכרומוזום לבדיקה
        // currentShift - המשמרת הנוכחית
        // ערך מוחזר: רשימת העובדים הזמינים להחלפה
        private static List<Employee> FindPotentialReplacements(string role, int shiftId, HashSet<Employee> currentEmployees, Chromosome chromosome, Shift currentShift)
        {
            return Employees
                .Where(emp =>
                    emp.roles.Contains(role) &&//מתאים לתפקיד
                    emp.requestedShifts.Contains(shiftId) &&//ביקש את המשמרת
                    !currentEmployees.Contains(emp) &&//לא שובץ למשמרת
                    !IsEmployeeAssignedToOverlappingShift(chromosome, emp, currentShift))//לא משובץ למשמרת חופפת
                .ToList();
        }

        // פונקציה המחליפה עובדים במשמרת
        // פרמטרים
        // employees - רשימת העובדים הנוכחית
        // potentialReplacements - רשימת העובדים הפוטנציאלים להחלפה
        // numToReplace - מספר העובדים להחלפה
        // shiftId - מזהה המשמרת
        // ערך מוחזר: האם בוצעה החלפה מוצלחת
        private static bool ReplaceEmployees(
            List<Employee> employees,
            List<Employee> potentialReplacements,
            int numToReplace,
            int shiftId)
        {
            bool madeChanges = false;

            for (int i = 0; i < numToReplace && i < employees.Count && potentialReplacements.Count > 0; i++)
            {
                // נגריל עובד להחלפה
                int indexToReplace = random.Next(employees.Count);

                // נמצא עובד מחליף
                int replacementIndex = random.Next(potentialReplacements.Count);
                Employee replacement = potentialReplacements[replacementIndex];

                // נחליף את העובדים
                employees[indexToReplace] = replacement;

                // נוריד את העובד החדש ששובץ למשמרת מרשימת העובדים הפוטנציאלים כדיי למנוע כפילויות
                potentialReplacements.RemoveAt(replacementIndex);
                madeChanges = true;
            }

            return madeChanges;
        }

        // פונקציה המבצעת מוצטיה פשוטה
        // פרמטרים
        // shift - המשמרת לביצוע מוטציה
        // ch - הכרומוזום המקורי
        // ערך מוחזר: האם בוצעה מוטציה מוצלחת
        private static bool PerformStandardMutation(Shift shift, Chromosome ch)
        {
            // בחירה רנדומלית של האסטרטגיה שנבחר בה
            int strategy = random.Next(3);

            //הרצת האסטרטגיה שהוגרלה
            if (strategy == 0)
            {
                // הוספת עובד מנסה אם אין
                if (!ShiftHasMentor(shift))
                {
                    return TryAddMentor(shift, ch);
                }
            }
            else if (strategy == 1)
            {
                // מילוי מקומות ריקים
                return TryFillEmptyPositions(shift, ch);
            }
            else if (strategy == 2)
            {
                // שדרוג רמת העובדים
                return TryUpgradeEmployees(shift, ch);
            }

            return false;
        }

        // פונקציה הבודקת אם יש במשמרת עובד מנוסה
        // פרמטרים
        // shift - המשמרת לבדיקה
        // ערך מוחזר: האם יש במשמרת עובד מנוסה
        private static bool ShiftHasMentor(Shift shift)
        {
            if (shift.AssignedEmployees == null) return false;

            return shift.AssignedEmployees.Values
                .SelectMany(emp => emp)
                .Any(emp => emp.isMentor);
        }

        // פונקציה המנסה להוסיף עובד מנוסה למשמרת
        // פרמטרים
        // shift - המשמרת לעדכון
        // ch - הכרומוזום המקורי
        // ערך מוחזר: האם הצליח להוסיף עובד מנוסה
        private static bool TryAddMentor(Shift shift, Chromosome ch)
        {
            // ננסה להוסיף עובד מנוסה למשמרת
            var validRoleEntry = shift.AssignedEmployees
                .Where(entry => entry.Value.Count > 0)
                .FirstOrDefault(entry => TryAddMentorToRole(entry.Key, entry.Value, shift.Id, ch));

            // נחזיר אמת עם הצלחנו להוסיף עובד מנוסה
            return validRoleEntry.Key != null;
        }

        // פונקציה המנסה להוסיף עובד מנוסה לתפקיד מסוים
        // פרמטרים
        // role - התפקיד המבוקש
        // employees - רשימת העובדים הנוכחית
        // shiftId - מזהה המשמרת
        // ch - הכרומוזום המקורי
        // ערך מוחזר: האם הצליח להוסיף עובד מנוסה
        private static bool TryAddMentorToRole(string role, List<Employee> employees, int shiftId, Chromosome ch)
        {
            //נחפש עובד לא מנסה
            for (int i = 0; i < employees.Count; i++)
            {
                if (!employees[i].isMentor)
                {
                    // ננסה למצוא עובד מנוסה המתאים לתפקיד
                    Employee mentor = Employees
                        .Where(e => e.roles.Contains(role) &&//בדיקה אם מתאים לתפקיד
                               e.isMentor &&//בדיקה אם עובד מנוסה
                               e.requestedShifts.Contains(shiftId) &&//בדיקה אם ביקש את המשמרת
                               !IsEmployeeAssignedToOverlappingShift(ch, e, FindShiftById(shiftId)))//בדיקה אם לא משובץ במשמרת חופפת
                        .OrderByDescending(e => e.Rate)//נתינת עדיפות לעובדים בעלי ציון גבוה
                        .FirstOrDefault();

                    // אם מצאנו עובד מנוסה נחליף 
                    if (mentor != null)
                    {
                        employees[i] = mentor;
                        return true;
                    }
                }
            }

            //נחזיר שקר אם לא הצלחנו למצוא עובד מנוסה מתאים
            return false;
        }

        // פונקציה המנסה למלא מקומות ריקים בסידור העבודה
        // פרמטרים 
        // shift - המשמרת למילוי
        // ch - הכרומוזום המקורי
        // ערך מוחזר: האם הצליח למלא מקומות ריקים
        private static bool TryFillEmptyPositions(Shift shift, Chromosome ch)
        {
            //נחזיר שקר אם לא צריך עובדים כלל במשמרת
            if (shift.RequiredRoles == null) return false;

            //נאתחל מילון חדש אם עוד לא שובצו עובדים במשמרת
            if (shift.AssignedEmployees == null)
                shift.AssignedEmployees = new Dictionary<string, List<Employee>>();

            bool madeChanges = false;

            //נעבור על כל התפקידים הדרושים במשמרת
            foreach (var roleReq in shift.RequiredRoles)
            {
                string role = roleReq.Key;
                int required = roleReq.Value;
                //נאתחל רשימה חדשה אם עוד לא שובץ אף עובד לתפקיד
                if (!shift.AssignedEmployees.ContainsKey(role))
                    shift.AssignedEmployees[role] = new List<Employee>();

                //מספר העובדים שמשובצים כרגע לתפקיד
                int currentCount = shift.AssignedEmployees[role].Count;

                //בדיקה אם יש מקומות ריקים
                if (currentCount < required)
                {
                    // קבלת רשימת העובדים שכבר משובצים למשמרת
                    var assignedEmps = GetEmployeesAlreadyAssignedToShift(shift);

                    // חיפוש עובדים זמינים לתפקיד זה שביקשו את המשמרת
                    var availableEmployees = FindAvailableEmployeesForRole(role, shift.Id, assignedEmps, ch);

                    // הוספת עובדים עד למספר הנדרש
                    int toAdd = required - currentCount;
                    for (int i = 0; i < toAdd && i < availableEmployees.Count; i++)
                    {
                        shift.AssignedEmployees[role].Add(availableEmployees[i]);
                        madeChanges = true;
                    }
                }
            }

            return madeChanges;
        }

        // פונקציה המקבלת את רשימת העובדים שכבר משובצים למשמרת
        // פרמטרים
        // shift - המשמרת לבדיקה
        // ערך מוחזר: אוסף העובדים המשובצים למשמרת
        private static HashSet<Employee> GetEmployeesAlreadyAssignedToShift(Shift shift)
        {
            var assignedEmps = new HashSet<Employee>();
            foreach (var emps in shift.AssignedEmployees.Values)
                foreach (var emp in emps)
                    assignedEmps.Add(emp);

            return assignedEmps;
        }

        // פונקציה המחפשת עובדים זמינים לתפקיד מסוים שביקשו משמרת מסוימת
        // פרמטרים
        // role - התפקיד המבוקש
        // shiftId - מזהה המשמרת
        // assignedEmps - העובדים שכבר משובצים למשמרת
        // ch - הכרומוזום המקורי
        // ערך מוחזר: רשימת העובדים הזמינים
        private static List<Employee> FindAvailableEmployeesForRole(string role, int shiftId, HashSet<Employee> assignedEmps, Chromosome ch)
        {
            return Program.Employees
                .Where(e => e.roles.Contains(role) &&//בדיקה שהעובד מתאים לתפקיד
                       !assignedEmps.Contains(e) &&//בדיקה שהעובד עוד לא משובץ למשמרת
                         !IsEmployeeAssignedToOverlappingShift(ch, e, FindShiftById(shiftId)) &&//בדיקה שעובד לא משובץ במשמרת חופפת 
                       e.requestedShifts.Contains(shiftId)) //בדיקה שהעובד ביקש משמרת זו
                .OrderByDescending(e => e.isMentor)//נתינת עדיפות לעובדים מנוסים
                .ThenByDescending(e => e.Rate)//נתינת עדיפות לעובדים בעל ציון גבוה
                .ToList();
        }

        // פונקציה המנסה לשדרג את רמת העובדים במשמרת
        // פרמטרים
        // shift - המשמרת לשדרוג
        // ch - הכרומוזום המקורי
        // ערך מוחזר: האם הצליח לשדרג עובדים
        private static bool TryUpgradeEmployees(Shift shift, Chromosome ch)
        {
            if (shift.AssignedEmployees == null || shift.AssignedEmployees.Count == 0)
                return false;

            // חיפוש העובד עם הציון הנמוך ביותר
            var lowestRatedEmployee = FindLowestRatedEmployee(shift, out string lowestRole, out int lowestIndex);

            if (lowestRatedEmployee == null) return false;

            // קבלת רשימת העובדים שכבר במשמרת
            var employeesInShift = GetEmployeesAlreadyAssignedToShift(shift);

            // חיפוש עובד טוב יותר שביקש את המשמרת
            Employee better = FindBetterEmployeeForRole(
                lowestRole, lowestRatedEmployee, shift.Id, employeesInShift, ch);

            //אם מצאנו עובד נחליף את העובד הגרוע בעובד הטוב
            if (better != null)
            {
                shift.AssignedEmployees[lowestRole][lowestIndex] = better;
                return true;
            }

            return false;
        }

        // פונקציה המחפשת את העובד עם הציון הנמוך ביותר במשמרת
        // פרמטרים
        // shift - המשמרת לבדיקה
        // lowestRole - פרמטר יציאה: התפקיד של העובד עם הציון הנמוך ביותר
        // lowestIndex - פרמטר יציאה: האינדקס של העובד עם הציון הנמוך ביותר
        // ערך מוחזר: העובד עם הציון הנמוך ביותר
        private static Employee FindLowestRatedEmployee(
            Shift shift, out string lowestRole, out int lowestIndex)
        {
            Employee lowest = null;
            lowestRole = null;
            lowestIndex = -1;
            int lowestRate = int.MaxValue;
            //מעבר על כל העובדים במשמרת
            foreach (var roleEntry in shift.AssignedEmployees)
            {
                for (int i = 0; i < roleEntry.Value.Count; i++)
                {
                    //שמירת העובד הכי גרוע עד כה
                    if (roleEntry.Value[i].Rate < lowestRate)
                    {
                        lowest = roleEntry.Value[i];
                        lowestRole = roleEntry.Key;
                        lowestIndex = i;
                        lowestRate = roleEntry.Value[i].Rate;
                    }
                }
            }
            //החזרת העובד הכי גרוע
            return lowest;
        }
        // פונקציה המחפשת עובד טוב יותר לתפקיד מסוים
        // פרמטרים
        // role - התפקיד המבוקש
        // currentEmployee - העובד הנוכחי
        // shiftId - מזהה המשמרת
        // employeesInShift - העובדים הנוכחיים במשמרת
        // ch - הכרומוזום המקורי
        //null ערך מוחזר: עובד טוב יותר אם נמצא, אחרת
        private static Employee FindBetterEmployeeForRole(
            string role, Employee currentEmployee, int shiftId, HashSet<Employee> employeesInShift, Chromosome ch)
        {
            return Program.Employees
                .Where(e => e.roles.Contains(role) &&//בדיקה שהעובד מתאים לתפקיד
                       e.Rate > currentEmployee.Rate &&//בדיקה שיש לעובד ציון גבוה יותר
                       e.requestedShifts.Contains(shiftId) && //  בדיקה שהעובד ביקש משמרת זו
                       !employeesInShift.Contains(e) &&//בדיקה שהעובד עוד לא שובץ למשמרת
                       !IsEmployeeAssignedToOverlappingShift(ch, e, FindShiftById(shiftId))) // בדיקה שהעובד לא משובץ למשמרת חופפת
                .OrderByDescending(e => e.Rate)//נתינת עדיפות לעובדים בעלי ציון גבוה
                .FirstOrDefault();
        }

        // פונקציה המקבלת משמרת אקראית מכרומוזום
        // פרמטרים
        // chromosome - הכרומוזום לבחירת משמרת ממנו
        // ערך מוחזר: משמרת אקראית מהכרומוזום
        private static Shift GetRandomShift(Chromosome chromosome)
        {
            if (chromosome.Shifts.Count == 0) return null;

            var branchKeys = chromosome.Shifts.Keys.ToList();
            string branch = branchKeys[random.Next(branchKeys.Count)];

            if (chromosome.Shifts[branch].Count == 0) return null;

            return chromosome.Shifts[branch][random.Next(chromosome.Shifts[branch].Count)];
        }
        #endregion

        #region Fitness

        // פונקציה לחישוב ציון הכושר של כרומוזום
        // פרמטרים
        // chromosome - הכרומוזום לחישוב ציון הכושר
        // ערך מוחזר: ציון הכושר של הכרומוזום
        public static double CalculateChromosomeFitness(Chromosome chromosome)
        {
            // החזרת ערך מינימלי אם הכרומוזם ריק
            if (chromosome == null ||
                chromosome.Shifts == null)
                return double.MinValue;

            // אתחול מעקב אחרי מספר השעות של כל עובד
            Dictionary<Employee, double> weeklyHoursPerEmployee = new Dictionary<Employee, double>();
            Dictionary<Employee, Dictionary<string, double>> dailyHoursPerEmployee = new Dictionary<Employee, Dictionary<string, double>>();

            // חישוב ציון הכושר בעבור כל הסניפים
            double totalFitness = CalculateFitnessForAllBranches(
                chromosome, weeklyHoursPerEmployee, dailyHoursPerEmployee);

            // העלאת/הורדת ציון הכושר הכולל בהתאם למספר השעות השבועיות והיומיות של כל עובד
            totalFitness = ApplyHoursConstraintsPenalties(
                totalFitness, weeklyHoursPerEmployee, dailyHoursPerEmployee);

            return totalFitness;
        }

        // פונקציה לחישוב ציון הכושר לכל הסניפים והמשמרות
        // פרמטרים
        // chromosome - הכרומוזום לחישוב
        // weeklyHoursPerEmployee - מילון לשמירת מספר השעות השבועיות לכל עובד
        // dailyHoursPerEmployee - מילון לשמירת מספר השעות היומיות לכל עובד
        // ערך מוחזר: ציון הכושר הכולל של הכרומוזום
        private static double CalculateFitnessForAllBranches(
            Chromosome chromosome,
            Dictionary<Employee, double> weeklyHoursPerEmployee,
            Dictionary<Employee, Dictionary<string, double>> dailyHoursPerEmployee)
        {
            //הכזרת ציון הכושר הכולל בעבור כל המשמרות בכל הסניפים
            return chromosome.Shifts
                .SelectMany(branchEntry => branchEntry.Value ?? new List<Shift>())
                .Sum(shift => CalculateShiftFitness(shift, weeklyHoursPerEmployee, dailyHoursPerEmployee));
        }

        // פונקציה המחשבת קנסות על חריגה ממגבלות שעות עבודה
        // פרמטרים
        // totalFitness - ציון הכושר הכולל
        // weeklyHoursPerEmployee - מילון לשמירת מספר השעות השבועיות לכל עובד
        // dailyHoursPerEmployee - מילון לשמירת מספר השעות היומיות לכל עובד
        // ערך מוחזר: ציון הכושר לאחר הקנסות
        private static double ApplyHoursConstraintsPenalties(
            double totalFitness,
            Dictionary<Employee, double> weeklyHoursPerEmployee,
            Dictionary<Employee, Dictionary<string, double>> dailyHoursPerEmployee)
        {
            // קנסות על חריגה ממגבלת השעות השבועיות
            totalFitness = ApplyWeeklyHoursConstraints(totalFitness, weeklyHoursPerEmployee);

            // קנסות על חריגה ממגבלת השעות היומיות
            totalFitness = ApplyDailyHoursConstraints(totalFitness, dailyHoursPerEmployee);

            return totalFitness;
        }

        // פונקציה לנתינת קנסות על חריגה ממגבלת השעות השבועיות
        // פרמטרים
        // totalFitness - ציון הכושר הכולל
        // weeklyHoursPerEmployee - מילון לשמירת מספר השעות השבועיות לכל עובד
        // ערך מוחזר: ציון הכושר לאחר הקנסות
        private static double ApplyWeeklyHoursConstraints(
            double totalFitness,
            Dictionary<Employee, double> weeklyHoursPerEmployee)
        {
            //מעבר על כל העובדים
            foreach (var entry in weeklyHoursPerEmployee)
            {
                double hours = entry.Value;
                //נתינת קנס אם השעות השבועיויות של העובד מעל המותר
                if (hours > hoursPerWeek)
                {
                    totalFitness -= (hours - hoursPerWeek) * WeeklyHoursOveragePenalty;
                }
            }

            return totalFitness;
        }

        // פונקציה לנתינת קנסות על חריגה ממגבלת השעות היומיות
        // פרמטרים
        // totalFitness - ציון הכושר הכולל
        // dailyHoursPerEmployee - מילון לשמירת מספר השעות היומיות לכל עובד
        // ערך מוחזר: ציון הכושר לאחר הקנסות
        private static double ApplyDailyHoursConstraints(
            double totalFitness,
            Dictionary<Employee, Dictionary<string, double>> dailyHoursPerEmployee)
        {
            //מעבר על כל העובדים
            foreach (var empEntry in dailyHoursPerEmployee)
            {
                var dayHours = empEntry.Value;
                //מעבר על כל הימים שהעובד עובד בהם
                foreach (var dayEntry in dayHours)
                {
                    double hours = dayEntry.Value;
                    //נתינת קנס אם השעות היומיות של העובד מעל המותר
                    if (hours > hoursPerDay)
                    {
                        totalFitness -= (hours - hoursPerDay) * DailyHoursOveragePenalty;
                    }
                }
            }

            return totalFitness;
        }

        // פונקציה לחישוב ציון הכושר של משמרת בכרומוזום
        // פרמטרים
        // shift - המשמרת לחישוב
        // weeklyHoursPerEmployee - מילון לשמירת מספר השעות השבועיות לכל עובד
        // dailyHoursPerEmployee - מילון לשמירת מספר השעות היומיות לכל עובד
        // ערך מוחזר: ציון הכושר של המשמרת
        private static double CalculateShiftFitness(Shift shift,
            Dictionary<Employee, double> weeklyHoursPerEmployee,
            Dictionary<Employee, Dictionary<string, double>> dailyHoursPerEmployee)
        {
            //השמת ערך התחלתי לציון המשמרת
            double shiftFitness = 0;

            //קבלת כמות העובדים הדרושים וכמות העובדים בפועל למשמרת
            int totalEmployees = GetTotalEmployeesInShift(shift);
            int requiredEmployees = shift.GetTotalRequiredEmployees();

            // חישוב ציון על כמות עובדים נדרשת
            shiftFitness = CalculateEmployeeCountFitness(shiftFitness, totalEmployees, requiredEmployees);

            // עדכון שעות עבודה לעובדים וחישוב סטטיסטיקות
            UpdateEmployeeWorkingHours(shift, weeklyHoursPerEmployee, dailyHoursPerEmployee);

            // חישוב ציון על נוכחות מנטור במשמרת
            shiftFitness = CalculateMentorPresenceFitness(shift, shiftFitness);

            // חישוב ציון על התאמת רמת העובדים למידת העומס
            shiftFitness = CalculateEmployeeRatingVsEventTypeFitness(shift, shiftFitness);

            // חישוב ציון על תמהיל צוותי מאוזן
            shiftFitness = CalculateTeamBalanceFitness(shift, shiftFitness);

            // חישוב ציון על עלות המשמרת
            shiftFitness = CalculateShiftCostFitness(shift, shiftFitness);

            return shiftFitness;
        }

        // פונקציה לנתינת קנסות על מחסור בכמות העובדים הנדרשת
        // פרמטרים
        // shiftFitness - ציון הכושר הנוכחי
        // totalEmployees - מספר העובדים בפועל
        // requiredEmployees - מספר העובדים הנדרשים
        // ערך מוחזר: ציון הכושר לאחר הקנסות
        private static double CalculateEmployeeCountFitness(
            double shiftFitness, int totalEmployees, int requiredEmployees)
        {
            // בדיקת אילוץ - כמות מינימלית של עובדים
            // קנס חמור על חוסר בעובדים
            if (totalEmployees < requiredEmployees)
            {
                shiftFitness -= (requiredEmployees - totalEmployees) * MissingEmployeePenalty;
            }

            return shiftFitness;
        }

        // פונקציה המחשבת את עלות המשמרת
        // פרמטרים
        // shift - המשמרת לחישוב
        // hours - מספר השעות במשמרת
        // ערך מוחזר: עלות המשמרת
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

        // פונקציה המקבלת משמרת ומחזירה את כמות העובדים ששובצו אליה
        // פרמטרים
        // shift - המשמרת לבדיקה
        // ערך מוחזר: מספר העובדים במשמרת
        private static int GetTotalEmployeesInShift(Shift shift)
        {
            return shift.AssignedEmployees.Values.Sum(lst => lst.Count);
        }

        // פונקציה המעדכנת שעות עבודה לעובדים
        // פרמטרים
        // shift - המשמרת לעדכון
        // weeklyHoursPerEmployee - מילון לשמירת מספר השעות השבועיות לכל עובד
        // dailyHoursPerEmployee - מילון לשמירת מספר השעות היומיות לכל עובד
        // ערך מוחזר: אין
        private static void UpdateEmployeeWorkingHours(
            Shift shift,
            Dictionary<Employee, double> weeklyHoursPerEmployee,
            Dictionary<Employee, Dictionary<string, double>> dailyHoursPerEmployee)
        {
            // מעבר על העובדים
            foreach (var roleEntry in shift.AssignedEmployees)
            {
                foreach (var employee in roleEntry.Value)
                {
                    // עדכון מעקב שעות עבודה לעובד
                    UpdateWorkingHours(
                        employee, shift.day, hoursPerShift, weeklyHoursPerEmployee, dailyHoursPerEmployee);
                }
            }
        }

        // פונקציה המעדכנת את המילון ששומר את השעות של כל עובד
        // פרמטרים
        // employee - העובד לעדכון
        // day - היום
        // hours - מספר השעות
        // weeklyHoursPerEmployee - מילון לשמירת מספר השעות השבועיות לכל עובד
        // dailyHoursPerEmployee - מילון לשמירת מספר השעות היומיות לכל עובד
        // ערך מוחזר: אין
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

        // פונקצציה המחשבת ציון על נוכחות עובד מנוסה במשמרת
        // פרמטרים
        // shift - המשמרת לחישוב
        // shiftFitness - ציון הכושר הנוכחי
        // ערך מוחזר: ציון הכושר המעודכן
        private static double CalculateMentorPresenceFitness(Shift shift, double shiftFitness)
        {
            // בדיקה אם יש עובד מנוסה
            bool hasMentor = shift.AssignedEmployees.Values
                .Any(employeeList => employeeList.Any(emp => emp.isMentor));

            // העלאת או הורדת ציון על נוכחות עובד מנוסה
            if (hasMentor)
            {
                shiftFitness += MentorBonus; // בונוס על נוכחות עובד מנוסה במשמרת
            }
            else
            {
                shiftFitness -= MentorPenalty; // קנס על היעדר עובד מנוסה במשמרת
            }

            return shiftFitness;
        }

        // פונקציה המחשבת ציון על התאמת רמת העובדים למידת העומס
        // פרמטרים
        // shift - המשמרת לחישוב
        // shiftFitness - ציון הכושר הנוכחי
        // ערך מוחזר: ציון הכושר המעודכן
        private static double CalculateEmployeeRatingVsEventTypeFitness(Shift shift, double shiftFitness)
        {
            // חישוב הציון הממוצע של העובדים במשמרת
            double avgRate = CalculateAverageEmployeeRate(shift);

            // בדיקת אילוץ - התאמת רמת העובדים למידת העומס
            if (shift.EventType.Equals("Special"))
            {
                if (avgRate >= GoodRatingThreshold)
                {
                    shiftFitness += SpecialEventHighRatingBonus; // בונוס על צוות חזק במשמרת עמוסה
                }
                else
                {
                    shiftFitness -= SpecialEventLowRatingPenalty; // קנס על צוות חלש במשמרת עמוסה
                }
            }
            else if (shift.EventType.Equals("Regular"))
            {
                if (avgRate >= GoodRatingThreshold)
                {
                    shiftFitness += RegularEventHighRatingBonus; // בונוס על צוות חזק במשמרת רגילה
                }
                else
                {
                    shiftFitness -= RegularEventLowRatingPenalty; // קנס על צוות חלש במשמרת רגילה
                }
            }

            return shiftFitness;
        }

        // פונקציה לחישוב ציון על תמהיל צוותי מאוזן
        // פרמטרים 
        // shift - המשמרת לחישוב
        // shiftFitness - ציון הכושר הנוכחי
        // ערך מוחזר: ציון הכושר המעודכן
        private static double CalculateTeamBalanceFitness(Shift shift, double shiftFitness)
        {
            // בדיקת אילוץ - תמהיל צוותי מאוזן
            double experiencedRatio = CalculateExperiencedRatio(shift);
            if (experiencedRatio >= MinBalancedExperienceRatio &&
                experiencedRatio <= MaxBalancedExperienceRatio)
            {
                shiftFitness += BalancedTeamBonus; // בונוס על איזון טוב בין מנוסים לחדשים
            }

            return shiftFitness;
        }

        // פונקציה לחישוב ציון על עלות המשמרת
        // פרמטרים
        // shift - המשמרת לחישוב
        // shiftFitness - ציון הכושר הנוכחי
        // ערך מוחזר: ציון הכושר המעודכן
        private static double CalculateShiftCostFitness(Shift shift, double shiftFitness)
        {
            // בדיקת אילוץ - מינימום עלות משמרת
            double shiftCost = CalculateShiftCost(shift, hoursPerShift);
            shiftFitness -= shiftCost / CostDivisor; // קנס לפי עלות המשמרת

            return shiftFitness;
        }

        // פונקציה לחישוב ציון הממוצע של עובדים במשמרת
        // פרמטרים
        // shift - המשמרת לחישוב
        // ערך מוחזר: הציון הממוצע של העובדים במשמרת
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

        // פונקציה המחשבת את כמות העובדים המנוסים מסך כל העובדים
        // פרמטרים
        // shift - המשמרת לחישוב
        // ערך מוחזר: יחס העובדים המנוסים מתוך סך העובדים
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

        // פונקציה המחזירה את הכרומוזום הטוב ביותר באוכלוסייה
        // פרמטרים: אין
        // ערך מוחזר: הכרומוזום הטוב ביותר
        public static Chromosome GetBestChromosome()
        {
            // שומרים הפניה לכרומוזום הטוב ביותר במקום למיין את כל האוכלוסייה
            Chromosome bestChromosome = null;
            double bestFitness = double.MinValue;
            if (pop.Chromoshomes == null)
            {
                return null;
            }
            foreach (var chromosome in pop.Chromoshomes)
            {
                if (chromosome.Fitness > bestFitness)
                {
                    bestFitness = chromosome.Fitness;
                    bestChromosome = chromosome;
                }
            }

            return bestChromosome;
        }
        #endregion

        // הפעלת האפליקציה
        // פרמטרים: אין
        // ערך מוחזר: אין
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new HomePage());

        }
    }
}
