using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Final;

namespace EmployeeSchedulingApp.Tests.UnitTests.AlgorithmTests
{
    [TestClass]
    public class SchedulingEdgeCaseTests
    {
        // הגדרת סביבת הבדיקה
        [TestInitialize]
        public void TestInitialize()
        {
            // איפוס הערכים הסטטיים בין הבדיקות
            Program.succefulMutation = 0;
            Program.UnsuccefulMutation = 0;
            Program.generationCount = 0;
        }

        // בדיקת מקרה קיצון: מחסור גדול בעובדים
        [TestMethod]
        public void EmployeeShortage_ShouldFindBestPossibleSolution()
        {
            // Arrange
            // יצירת סניף עם הרבה משמרות
            var branch = CreateBranchWithManyShifts();

            // יצירת מעט עובדים - לא מספיק לכיסוי כל המשמרות
            var employees = CreateFewEmployees();

            // הגדרת נתוני הבדיקה
            Program.Branches = new List<Branch> { branch };
            Program.Employees = employees;

            // Act
            Program.pop = new Population(new List<Chromosome>());
            Program.pop = Program.initializeFirstPopulation(Program.pop);
            Program.RunGeneticAlgorithm();

            // Assert
            Chromosome bestChromosome = Program.GetBestChromosome();
            Assert.IsNotNull(bestChromosome, "האלגוריתם צריך למצוא פתרון גם במקרה של מחסור בעובדים");

            // בדיקה שהאלגוריתם שיבץ את העובדים במשמרות האפשריות
            int totalAssignedEmployees = CountTotalAssignedEmployees(bestChromosome);
            Assert.IsTrue(totalAssignedEmployees > 0, "האלגוריתם צריך לשבץ לפחות חלק מהעובדים");

            // בדיקה שהאלגוריתם ניסה להתמודד עם המצב ולא קרס
            Assert.IsTrue(Program.generationCount > 0, "האלגוריתם צריך לרוץ גם במקרה של מחסור");
        }

        // בדיקת מקרה קיצון: עומס גדול על עובדים מסוימים
        [TestMethod]
        public void HighDemandForCertainRoles_ShouldBalanceWorkload()
        {
            // Arrange
            // יצירת סניף עם דרישה גבוהה לתפקידים מסוימים
            var branch = CreateBranchWithHighDemandRoles();

            // יצירת עובדים עם תפקידים שונים
            var employees = CreateEmployeesWithSpecificRoles();

            // הגדרת נתוני הבדיקה
            Program.Branches = new List<Branch> { branch };
            Program.Employees = employees;

            // Act
            Program.pop = new Population(new List<Chromosome>());
            Program.pop = Program.initializeFirstPopulation(Program.pop);
            Program.RunGeneticAlgorithm();

            // Assert
            Chromosome bestChromosome = Program.GetBestChromosome();
            Assert.IsNotNull(bestChromosome, "האלגוריתם צריך למצוא פתרון גם במקרה של עומס על תפקידים מסוימים");

            // בדיקה שהאלגוריתם מאזן את העומס בין העובדים
            Dictionary<Employee, int> assignmentsPerEmployee = CountAssignmentsPerEmployee(bestChromosome);

            // חישוב ממוצע וסטיית תקן של מספר המשמרות לעובד
            double averageAssignments = assignmentsPerEmployee.Values.Average();
            double stdDev = CalculateStandardDeviation(assignmentsPerEmployee.Values, averageAssignments);

            // בדיקה שסטיית התקן נמוכה יחסית (איזון טוב)
            Assert.IsTrue(stdDev < averageAssignments * 0.5,
                "סטיית התקן של מספר המשמרות לעובד צריכה להיות נמוכה יחסית");
        }

        // בדיקת מקרה קיצון: דרישה לעובדים מנוסים בכל המשמרות
        [TestMethod]
        public void RequireMentorsInAllShifts_ShouldOptimizeDistribution()
        {
            // Arrange
            // יצירת סניף עם דרישה לעובדים מנוסים בכל המשמרות
            var branch = CreateStandardBranch();

            // יצירת עובדים עם מעט עובדים מנוסים
            var employees = CreateEmployeesWithFewMentors();

            // הגדרת נתוני הבדיקה
            Program.Branches = new List<Branch> { branch };
            Program.Employees = employees;

            // Act
            Program.pop = new Population(new List<Chromosome>());
            Program.pop = Program.initializeFirstPopulation(Program.pop);
            Program.RunGeneticAlgorithm();

            // Assert
            Chromosome bestChromosome = Program.GetBestChromosome();
            Assert.IsNotNull(bestChromosome, "האלגוריתם צריך למצוא פתרון גם במקרה של מחסור בעובדים מנוסים");

            // בדיקה שהאלגוריתם שיבץ את העובדים המנוסים בצורה מיטבית
            int shiftsWithMentors = CountShiftsWithMentors(bestChromosome);
            int totalMentors = employees.Count(e => e.isMentor);

            // אין להגיע למספר גדול יותר של משמרות עם מנטורים ממספר המנטורים הכללי
            Assert.IsTrue(shiftsWithMentors <= totalMentors * 7,
                "מספר המשמרות עם מנטורים לא יכול להיות גדול ממספר המנטורים כפול מספר הימים");
        }

        // בדיקת מקרה קיצון: רמת איכות דומה לכל העובדים
        [TestMethod]
        public void SimilarQualityEmployees_ShouldFocusOnOtherFactors()
        {
            // Arrange
            // יצירת סניף סטנדרטי
            var branch = CreateStandardBranch();

            // יצירת עובדים עם רמת איכות דומה
            var employees = CreateEmployeesWithSimilarRatings();

            // הגדרת נתוני הבדיקה
            Program.Branches = new List<Branch> { branch };
            Program.Employees = employees;

            // Act
            Program.pop = new Population(new List<Chromosome>());
            Program.pop = Program.initializeFirstPopulation(Program.pop);
            Program.RunGeneticAlgorithm();

            // Assert
            Chromosome bestChromosome = Program.GetBestChromosome();
            Assert.IsNotNull(bestChromosome, "האלגוריתם צריך למצוא פתרון גם כאשר כל העובדים ברמה דומה");

            // בדיקת ציון הכושר - צריך להיות סביר
            double fitness = bestChromosome.Fitness;
            Assert.IsTrue(fitness > 0, "ציון הכושר צריך להיות חיובי גם כאשר כל העובדים ברמה דומה");
        }

        // בדיקת מקרה קיצון: עובדים עם העדפות משמרת נוקשות
        [TestMethod]
        public void RigidShiftPreferences_ShouldRespectPreferences()
        {
            // Arrange
            // יצירת סניף סטנדרטי
            var branch = CreateStandardBranch();

            // יצירת עובדים עם העדפות משמרת מאוד מוגדרות
            var employees = CreateEmployeesWithSpecificShiftPreferences(branch);

            // הגדרת נתוני הבדיקה
            Program.Branches = new List<Branch> { branch };
            Program.Employees = employees;

            // Act
            Program.pop = new Population(new List<Chromosome>());
            Program.pop = Program.initializeFirstPopulation(Program.pop);
            Program.RunGeneticAlgorithm();

            // Assert
            Chromosome bestChromosome = Program.GetBestChromosome();
            Assert.IsNotNull(bestChromosome, "האלגוריתם צריך למצוא פתרון גם כאשר העובדים מעדיפים משמרות ספציפיות");

            // בדיקה שהאלגוריתם מכבד את העדפות המשמרת
            int satisfiedPreferences = CountSatisfiedShiftPreferences(bestChromosome, employees);
            int totalAssignments = CountTotalAssignedEmployees(bestChromosome);

            // לפחות חלק מההעדפות צריכות להיות מסופקות
            Assert.IsTrue(satisfiedPreferences > 0, "האלגוריתם צריך לספק לפחות חלק מהעדפות המשמרת");

            // מרבית המשמרות שמאוישות צריכות להיות לפי העדפות
            double satisfactionRatio = (double)satisfiedPreferences / totalAssignments;
            Assert.IsTrue(satisfactionRatio > 0.5, "מרבית המשמרות שמאוישות צריכות להיות לפי העדפות העובדים");
        }

        // בדיקת ביצועים: זמן הרצה סביר
        [TestMethod]
        public void PerformanceTest_ShouldCompleteInReasonableTime()
        {
            // Arrange
            // יצירת סניף גדול
            var branch = CreateLargeBranch();

            // יצירת הרבה עובדים
            var employees = CreateManyEmployees(50);

            // הגדרת נתוני הבדיקה
            Program.Branches = new List<Branch> { branch };
            Program.Employees = employees;

            // הגדרת מספר דורות קטן יותר לבדיקת ביצועים
            int originalGenerations = Program.Generations;

            // Act
            var watch = System.Diagnostics.Stopwatch.StartNew();

            Program.pop = new Population(new List<Chromosome>());
            Program.pop = Program.initializeFirstPopulation(Program.pop);
            Program.RunGeneticAlgorithm();

            watch.Stop();



            // Assert
            Assert.IsTrue(watch.ElapsedMilliseconds < 30000,
                "האלגוריתם צריך לרוץ בפחות מ-30 שניות גם עם כמות גדולה של נתונים");

            Chromosome bestChromosome = Program.GetBestChromosome();
            Assert.IsNotNull(bestChromosome, "האלגוריתם צריך למצוא פתרון גם עם כמות גדולה של נתונים");
        }

        // בדיקה שהאלגוריתם מייצר פתרונות שונים בריצות שונות
        [TestMethod]
        public void DifferentRuns_ShouldProduceDifferentSolutions()
        {
            // Arrange
            // יצירת סניף וקבוצת עובדים
            var branch = CreateStandardBranch();
            var employees = CreateVariedEmployees(20);

            // הגדרת נתוני הבדיקה
            Program.Branches = new List<Branch> { branch };
            Program.Employees = employees;

            // הגדרת מספר דורות קטן לריצה מהירה
            int originalGenerations = Program.Generations;
      

            // ריצה ראשונה
            Program.pop = new Population(new List<Chromosome>());
            Program.pop = Program.initializeFirstPopulation(Program.pop);
            Program.RunGeneticAlgorithm();
            Chromosome solution1 = Program.GetBestChromosome();

            // ריצה שנייה
            Program.pop = new Population(new List<Chromosome>());
            Program.pop = Program.initializeFirstPopulation(Program.pop);
            Program.RunGeneticAlgorithm();
            Chromosome solution2 = Program.GetBestChromosome();

            
            // Assert
            Assert.IsNotNull(solution1);
            Assert.IsNotNull(solution2);

            // בדיקה שהפתרונות שונים (לפחות חלק מהשיבוצים)
            bool solutionsAreDifferent = false;

            foreach (var branchEntry in solution1.Shifts)
            {
                string branchName = branchEntry.Key;
                if (!solution2.Shifts.ContainsKey(branchName))
                    continue;

                var shifts1 = branchEntry.Value;
                var shifts2 = solution2.Shifts[branchName];

                for (int i = 0; i < Math.Min(shifts1.Count, shifts2.Count); i++)
                {
                    var shift1 = shifts1[i];
                    var shift2 = shifts2.Find(s => s.Id == shift1.Id);

                    if (shift2 != null)
                    {
                        // השוואת העובדים המשובצים
                        solutionsAreDifferent |= EmployeeAssignmentsAreDifferent(shift1, shift2);

                        if (solutionsAreDifferent)
                            break;
                    }
                }

                if (solutionsAreDifferent)
                    break;
            }

            Assert.IsTrue(solutionsAreDifferent,
                "האלגוריתם צריך לייצר פתרונות שונים בריצות שונות (אקראיות)");
        }

        #region Helper Methods

        private int CountTotalAssignedEmployees(Chromosome chromosome)
        {
            int total = 0;

            foreach (var branchShifts in chromosome.Shifts.Values)
            {
                foreach (var shift in branchShifts)
                {
                    if (shift.AssignedEmployees != null)
                    {
                        foreach (var roleEmployees in shift.AssignedEmployees.Values)
                        {
                            total += roleEmployees.Count;
                        }
                    }
                }
            }

            return total;
        }

        private Dictionary<Employee, int> CountAssignmentsPerEmployee(Chromosome chromosome)
        {
            var counts = new Dictionary<Employee, int>();

            foreach (var branchShifts in chromosome.Shifts.Values)
            {
                foreach (var shift in branchShifts)
                {
                    if (shift.AssignedEmployees != null)
                    {
                        foreach (var roleEmployees in shift.AssignedEmployees.Values)
                        {
                            foreach (var employee in roleEmployees)
                            {
                                if (!counts.ContainsKey(employee))
                                    counts[employee] = 0;

                                counts[employee]++;
                            }
                        }
                    }
                }
            }

            return counts;
        }

        private double CalculateStandardDeviation(IEnumerable<int> values, double mean)
        {
            if (!values.Any())
                return 0;

            double sumOfSquaresOfDifferences = values.Sum(val => Math.Pow(val - mean, 2));
            return Math.Sqrt(sumOfSquaresOfDifferences / values.Count());
        }

        private int CountShiftsWithMentors(Chromosome chromosome)
        {
            int count = 0;

            foreach (var branchShifts in chromosome.Shifts.Values)
            {
                foreach (var shift in branchShifts)
                {
                    if (shift.AssignedEmployees != null)
                    {
                        bool hasMentor = shift.AssignedEmployees.Values
                            .Any(emps => emps.Any(e => e.isMentor));

                        if (hasMentor)
                            count++;
                    }
                }
            }

            return count;
        }

        private int CountSatisfiedShiftPreferences(Chromosome chromosome, List<Employee> employees)
        {
            int count = 0;

            foreach (var branchShifts in chromosome.Shifts.Values)
            {
                foreach (var shift in branchShifts)
                {
                    if (shift.AssignedEmployees != null)
                    {
                        foreach (var roleEmployees in shift.AssignedEmployees.Values)
                        {
                            foreach (var employee in roleEmployees)
                            {
                                if (employee.requestedShifts.Contains(shift.Id))
                                    count++;
                            }
                        }
                    }
                }
            }

            return count;
        }

        private bool EmployeeAssignmentsAreDifferent(Shift shift1, Shift shift2)
        {
            if (shift1.AssignedEmployees == null || shift2.AssignedEmployees == null)
                return shift1.AssignedEmployees != shift2.AssignedEmployees;

            // השוואת התפקידים
            var roles1 = shift1.AssignedEmployees.Keys.ToList();
            var roles2 = shift2.AssignedEmployees.Keys.ToList();

            if (roles1.Count != roles2.Count)
                return true;

            foreach (var role in roles1)
            {
                if (!shift2.AssignedEmployees.ContainsKey(role))
                    return true;

                var employees1 = shift1.AssignedEmployees[role];
                var employees2 = shift2.AssignedEmployees[role];

                if (employees1.Count != employees2.Count)
                    return true;

                for (int i = 0; i < employees1.Count; i++)
                {
                    if (employees1[i].ID != employees2[i].ID)
                        return true;
                }
            }

            return false;
        }

        private Branch CreateBranchWithManyShifts()
        {
            // יצירת סניף עם 14 משמרות (7 ימים × 2 משמרות ביום)
            var branch = new Branch
            {
                ID = 1,
                Name = "Branch with Many Shifts",
                Shifts = new List<Shift>()
            };

            string[] daysOfWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            string[] timeSlots = { "Morning", "Evening" };

            int shiftId = 1;
            foreach (var day in daysOfWeek)
            {
                foreach (var timeSlot in timeSlots)
                {
                    // יצירת תפקידים נדרשים למשמרת
                    var requiredRoles = new Dictionary<string, int>
                    {
                        { "Manager", 1 },
                        { "Chef", 2 },
                        { "Waiter", 3 },
                        { "Bartender", 1 }
                    };

                    // יצירת המשמרת
                    branch.Shifts.Add(new Shift(
                        shiftId++,               // ID
                        branch.Name,             // Branch
                        timeSlot,                // Time slot
                        day,                     // Day
                        requiredRoles,           // Required roles (7 עובדים למשמרת)
                        false,                   // Is busy
                        new Dictionary<string, List<Employee>>(), // Assigned employees
                        "Regular"                // Event type
                    ));
                }
            }

            return branch;
        }

        private Branch CreateBranchWithHighDemandRoles()
        {
            // יצירת סניף עם דרישה גבוהה לתפקידים מסוימים
            var branch = new Branch
            {
                ID = 1,
                Name = "Branch with High Demand Roles",
                Shifts = new List<Shift>()
            };

            string[] daysOfWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            string[] timeSlots = { "Morning", "Evening" };

            int shiftId = 1;
            foreach (var day in daysOfWeek)
            {
                foreach (var timeSlot in timeSlots)
                {
                    // יצירת תפקידים נדרשים למשמרת עם דרישה גבוהה לשפים
                    var requiredRoles = new Dictionary<string, int>
                    {
                        { "Manager", 1 },
                        { "Chef", 3 },      // דרישה גבוהה לשפים
                        { "Waiter", 1 },
                        { "Bartender", 1 }
                    };

                    // יצירת המשמרת
                    branch.Shifts.Add(new Shift(
                        shiftId++,               // ID
                        branch.Name,             // Branch
                        timeSlot,                // Time slot
                        day,                     // Day
                        requiredRoles,           // Required roles
                        false,                   // Is busy
                        new Dictionary<string, List<Employee>>(), // Assigned employees
                        "Regular"                // Event type
                    ));
                }
            }

            return branch;
        }

        private Branch CreateStandardBranch()
        {
            // יצירת סניף סטנדרטי
            var branch = new Branch
            {
                ID = 1,
                Name = "Standard Branch",
                Shifts = new List<Shift>()
            };

            string[] daysOfWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            string[] timeSlots = { "Morning", "Evening" };

            int shiftId = 1;
            foreach (var day in daysOfWeek)
            {
                foreach (var timeSlot in timeSlots)
                {
                    // יצירת תפקידים נדרשים למשמרת
                    var requiredRoles = new Dictionary<string, int>
                    {
                        { "Manager", 1 },
                        { "Chef", 1 },
                        { "Waiter", 2 },
                        { "Bartender", 1 }
                    };

                    // יצירת המשמרת
                    branch.Shifts.Add(new Shift(
                        shiftId++,               // ID
                        branch.Name,             // Branch
                        timeSlot,                // Time slot
                        day,                     // Day
                        requiredRoles,           // Required roles
                        false,                   // Is busy
                        new Dictionary<string, List<Employee>>(), // Assigned employees
                        "Regular"                // Event type
                    ));
                }
            }

            return branch;
        }

        private Branch CreateLargeBranch()
        {
            // יצירת סניף גדול עם הרבה משמרות
            var branch = new Branch
            {
                ID = 1,
                Name = "Large Branch",
                Shifts = new List<Shift>()
            };

            string[] daysOfWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            string[] timeSlots = { "Morning", "Noon", "Afternoon", "Evening", "Night" };

            int shiftId = 1;
            foreach (var day in daysOfWeek)
            {
                foreach (var timeSlot in timeSlots)
                {
                    // יצירת תפקידים נדרשים למשמרת
                    var requiredRoles = new Dictionary<string, int>
                    {
                        { "Manager", 1 },
                        { "Chef", 2 },
                        { "Waiter", 3 },
                        { "Bartender", 1 },
                        { "Host", 1 },
                        { "Cleaner", 1 }
                    };

                    // יצירת המשמרת
                    branch.Shifts.Add(new Shift(
                        shiftId++,               // ID
                        branch.Name,             // Branch
                        timeSlot,                // Time slot
                        day,                     // Day
                        requiredRoles,           // Required roles
                        false,                   // Is busy
                        new Dictionary<string, List<Employee>>(), // Assigned employees
                        "Regular"                // Event type
                    ));
                }
            }

            return branch;
        }

        private List<Employee> CreateFewEmployees()
        {
            // יצירת 5 עובדים בלבד
            var employees = new List<Employee>();

            string[] roleOptions = { "Manager", "Chef", "Waiter", "Bartender" };

            for (int i = 1; i <= 5; i++)
            {
                var roles = new HashSet<string>();

                // מתן תפקידים מגוונים
                roles.Add(roleOptions[i % roleOptions.Length]);

                if (i == 1 || i == 3)
                    roles.Add(roleOptions[(i + 1) % roleOptions.Length]);

                // הקצאת משמרות מועדפות
                var shifts = new HashSet<int>();
                for (int j = 1; j <= 14; j++)
                {
                    if (j % (i + 1) == 0)
                        shifts.Add(j);
                }

                // יצירת העובד
                employees.Add(new Employee(
                    i,                  // ID
                    $"Employee {i}",    // Name
                    roles,              // Roles
                    shifts,             // Requested shifts
                    i % 5 + 3,          // Rate (3-7)
                    i * 10 + 20,        // Hourly salary
                    i == 1,             // Is mentor (רק העובד הראשון)
                    new List<string> { "Branch with Many Shifts" } // Branches
                ));
            }

            return employees;
        }

        private List<Employee> CreateEmployeesWithSpecificRoles()
        {
            // יצירת 15 עובדים עם תפקידים ספציפיים
            var employees = new List<Employee>();

            // יצירת 3 מנהלים
            for (int i = 1; i <= 3; i++)
            {
                var shifts = new HashSet<int>();
                for (int j = 1; j <= 14; j++)
                {
                    if (j % 3 == i % 3)
                        shifts.Add(j);
                }

                employees.Add(new Employee(
                    i,                  // ID
                    $"Manager {i}",     // Name
                    new HashSet<string> { "Manager" },  // Roles
                    shifts,             // Requested shifts
                    i + 5,              // Rate
                    80 + i * 5,         // Hourly salary
                    i == 1,             // Is mentor
                    new List<string> { "Branch with High Demand Roles" } // Branches
                ));
            }

            // יצירת 4 שפים בלבד (פחות מהדרישה הכוללת)
            for (int i = 4; i <= 7; i++)
            {
                var shifts = new HashSet<int>();
                for (int j = 1; j <= 14; j++)
                {
                    if (j % 4 == i % 4)
                        shifts.Add(j);
                }

                employees.Add(new Employee(
                    i,                  // ID
                    $"Chef {i - 3}",      // Name
                    new HashSet<string> { "Chef" },  // Roles
                    shifts,             // Requested shifts
                    i % 3 + 4,          // Rate
                    70 + i * 3,         // Hourly salary
                    i == 4,             // Is mentor
                    new List<string> { "Branch with High Demand Roles" } // Branches
                ));
            }

            // יצירת 5 מלצרים
            for (int i = 8; i <= 12; i++)
            {
                var shifts = new HashSet<int>();
                for (int j = 1; j <= 14; j++)
                {
                    if (j % 5 == i % 5)
                        shifts.Add(j);
                }

                employees.Add(new Employee(
                    i,                  // ID
                    $"Waiter {i - 7}",    // Name
                    new HashSet<string> { "Waiter" },  // Roles
                    shifts,             // Requested shifts
                    i % 3 + 3,          // Rate
                    50 + i * 2,         // Hourly salary
                    i == 8,             // Is mentor
                    new List<string> { "Branch with High Demand Roles" } // Branches
                ));
            }

            // יצירת 3 ברמנים
            for (int i = 13; i <= 15; i++)
            {
                var shifts = new HashSet<int>();
                for (int j = 1; j <= 14; j++)
                {
                    if (j % 3 == i % 3)
                        shifts.Add(j);
                }

                employees.Add(new Employee(
                    i,                  // ID
                    $"Bartender {i - 12}", // Name
                    new HashSet<string> { "Bartender" },  // Roles
                    shifts,             // Requested shifts
                    i % 3 + 4,          // Rate
                    60 + i * 2,         // Hourly salary
                    i == 13,            // Is mentor
                    new List<string> { "Branch with High Demand Roles" } // Branches
                ));
            }

            return employees;
        }

        private List<Employee> CreateEmployeesWithFewMentors()
        {
            // יצירת 20 עובדים עם מעט עובדים מנוסים
            var employees = new List<Employee>();

            string[] roleOptions = { "Manager", "Chef", "Waiter", "Bartender" };

            for (int i = 1; i <= 20; i++)
            {
                var roles = new HashSet<string>();

                // מתן 1-2 תפקידים לכל עובד
                roles.Add(roleOptions[i % roleOptions.Length]);
                if (i % 3 == 0)
                    roles.Add(roleOptions[(i + 1) % roleOptions.Length]);

                // הקצאת משמרות מועדפות
                var shifts = new HashSet<int>();
                for (int j = 1; j <= 14; j++)
                {
                    if (j % (i % 5 + 1) == 0)
                        shifts.Add(j);
                }

                // רק 3 עובדים מנוסים (15%)
                bool isMentor = i == 1 || i == 8 || i == 15;

                // יצירת העובד
                employees.Add(new Employee(
                    i,                  // ID
                    $"Employee {i}",    // Name
                    roles,              // Roles
                    shifts,             // Requested shifts
                    isMentor ? 7 : (i % 4 + 3),  // Rate - גבוה יותר למנוסים
                    isMentor ? 90 : 50 + i,      // Hourly salary - גבוה יותר למנוסים
                    isMentor,           // Is mentor
                    new List<string> { "Standard Branch" } // Branches
                ));
            }

            return employees;
        }

        private List<Employee> CreateEmployeesWithSimilarRatings()
        {
            // יצירת 15 עובדים עם רמת איכות דומה
            var employees = new List<Employee>();

            string[] roleOptions = { "Manager", "Chef", "Waiter", "Bartender" };

            for (int i = 1; i <= 15; i++)
            {
                var roles = new HashSet<string>();

                // מתן 1-2 תפקידים לכל עובד
                roles.Add(roleOptions[i % roleOptions.Length]);
                if (i % 4 == 0)
                    roles.Add(roleOptions[(i + 1) % roleOptions.Length]);

                // הקצאת משמרות מועדפות
                var shifts = new HashSet<int>();
                for (int j = 1; j <= 14; j++)
                {
                    if (j % (i % 7 + 1) == 0)
                        shifts.Add(j);
                }

                // כולם עם רמת איכות דומה
                int rate = 4 + (i % 3);  // כולם בין 4-6

                // 20% מנוסים
                bool isMentor = i % 5 == 0;

                // יצירת העובד
                employees.Add(new Employee(
                    i,                  // ID
                    $"Employee {i}",    // Name
                    roles,              // Roles
                    shifts,             // Requested shifts
                    rate,               // Rate
                    50 + i * 2,         // Hourly salary
                    isMentor,           // Is mentor
                    new List<string> { "Standard Branch" } // Branches
                ));
            }

            return employees;
        }

        private List<Employee> CreateEmployeesWithSpecificShiftPreferences(Branch branch)
        {
            // יצירת 15 עובדים עם העדפות משמרת מאוד ספציפיות
            var employees = new List<Employee>();

            string[] roleOptions = { "Manager", "Chef", "Waiter", "Bartender" };

            for (int i = 1; i <= 15; i++)
            {
                var roles = new HashSet<string>();

                // מתן 1-2 תפקידים לכל עובד
                roles.Add(roleOptions[i % roleOptions.Length]);
                if (i % 4 == 0)
                    roles.Add(roleOptions[(i + 1) % roleOptions.Length]);

                // כל עובד מבקש רק 1-3 משמרות ספציפיות
                var shifts = new HashSet<int>();
                int preferredShiftCount = i % 3 + 1;

                for (int j = 0; j < preferredShiftCount; j++)
                {
                    shifts.Add(((i + j) % branch.Shifts.Count) + 1);
                }

                // יצירת העובד
                employees.Add(new Employee(
                    i,                  // ID
                    $"Employee {i}",    // Name
                    roles,              // Roles
                    shifts,             // Requested shifts - מעט מאוד
                    i % 5 + 3,          // Rate
                    50 + i * 2,         // Hourly salary
                    i % 4 == 0,         // Is mentor
                    new List<string> { branch.Name } // Branches
                ));
            }

            return employees;
        }

        private List<Employee> CreateManyEmployees(int count)
        {
            // יצירת מספר משתנה של עובדים
            var employees = new List<Employee>();

            string[] roleOptions = { "Manager", "Chef", "Waiter", "Bartender", "Host", "Cleaner" };

            for (int i = 1; i <= count; i++)
            {
                var roles = new HashSet<string>();

                // מתן 1-3 תפקידים לכל עובד
                int roleCount = i % 3 + 1;
                for (int r = 0; r < roleCount; r++)
                {
                    roles.Add(roleOptions[(i + r) % roleOptions.Length]);
                }

                // הקצאת משמרות מועדפות
                var shifts = new HashSet<int>();
                int prefCount = 10 + i % 20;  // בין 10-30 משמרות מועדפות

                for (int j = 1; j <= 35; j++)  // 35 משמרות אפשריות בסניף
                {
                    if (j % (35 / prefCount + 1) == 0)
                        shifts.Add(j);
                }

                // יצירת העובד
                employees.Add(new Employee(
                    i,                  // ID
                    $"Employee {i}",    // Name
                    roles,              // Roles
                    shifts,             // Requested shifts
                    i % 5 + 3,          // Rate
                    50 + (i % 50),      // Hourly salary
                    i % 5 == 0,         // Is mentor (20%)
                    new List<string> { "Large Branch" } // Branches
                ));
            }

            return employees;
        }

        private List<Employee> CreateVariedEmployees(int count)
        {
            // יצירת עובדים שונים ומגוונים
            var employees = new List<Employee>();

            string[] roleOptions = { "Manager", "Chef", "Waiter", "Bartender" };

            for (int i = 1; i <= count; i++)
            {
                var roles = new HashSet<string>();

                // מתן 1-3 תפקידים לכל עובד
                int roleCount = i % 3 + 1;
                for (int r = 0; r < roleCount; r++)
                {
                    roles.Add(roleOptions[(i + r) % roleOptions.Length]);
                }

                // הקצאת משמרות מועדפות
                var shifts = new HashSet<int>();
                int prefCount = i % 10 + 2;  // מספר משתנה של משמרות מועדפות

                for (int j = 1; j <= 14; j++)
                {
                    if (j % prefCount == 0 || j % (prefCount + 1) == 0)
                        shifts.Add(j);
                }

                // יצירת העובד
                employees.Add(new Employee(
                    i,                  // ID
                    $"Employee {i}",    // Name
                    roles,              // Roles
                    shifts,             // Requested shifts
                    i % 7 + 2,          // Rate - שונות גדולה
                    40 + i * 3,         // Hourly salary - שונות גדולה
                    i % 4 == 0,         // Is mentor (25%)
                    new List<string> { "Standard Branch" } // Branches
                ));
            }

            return employees;
        }

        #endregion
    }
}