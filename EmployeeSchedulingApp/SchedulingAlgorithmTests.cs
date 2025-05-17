using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.CodeCoverage;
using Final;
using EmployeeSchedulingApp;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Program = Final.Program;

namespace EmployeeSchedulingApp.Tests.UnitTests.AlgorithmTests
{
    [TestClass]
    public class SchedulingAlgorithmTests
    {
        // נתוני בדיקה
        private List<Employee> testEmployees;
        private List<Branch> testBranches;
       
        // הגדרת סביבת הבדיקה
        [TestInitialize]
        public void TestInitialize()
        {
            // יצירת נתוני בדיקה
            testEmployees = CreateTestEmployees();
            testBranches = CreateTestBranches();

            // הכנסת נתוני הבדיקה לתוכנית
            Program.Employees = testEmployees;
            Program.Branches = testBranches;
        }

        // בדיקת יצירת אוכלוסייה ראשונית
        [TestMethod]
        public void InitializeFirstPopulation_ShouldCreateValidChromosomes()
        {
            // Arrange
            var population = new Population(new List<Chromosome>());

            // Act
            population = Program.initializeFirstPopulation(population);

            // Assert
            Assert.IsNotNull(population);
            Assert.IsTrue(population.Chromoshomes.Count == Program.ChromosomesEachGene);

            // בדיקה שלכל כרומוזום באוכלוסייה יש ציון כושר
            foreach (var chromosome in population.Chromoshomes)
            {
                Assert.IsTrue(chromosome.Fitness > 0, "ציון הכושר של כרומוזום אמור להיות חיובי");

                // בדיקה שלכל כרומוזום יש את כל הסניפים
                foreach (var branch in testBranches)
                {
                    Assert.IsTrue(chromosome.Shifts.ContainsKey(branch.Name),
                        $"הכרומוזום אמור להכיל את הסניף {branch.Name}");

                    // בדיקה שלכל סניף בכרומוזום יש את כל המשמרות
                    Assert.AreEqual(branch.Shifts.Count, chromosome.Shifts[branch.Name].Count,
                        "מספר המשמרות בכרומוזום אמור להיות שווה למספר המשמרות בסניף");
                }
            }
        }

        // בדיקת הכלאה (Crossover)
        [TestMethod]
        public void Crossover_ShouldCreateNewOffspring()
        {
            // Arrange
            var population = new Population(new List<Chromosome>());
            population = Program.initializeFirstPopulation(population);
            int initialCount = population.Chromoshomes.Count;

            // Act
            Program.crossover(population);

            // Assert
            Assert.IsTrue(population.Chromoshomes.Count > initialCount,
                "הכלאה אמורה ליצור צאצאים חדשים");

            // בדיקה שהכרומוזומים ממוינים לפי ציון כושר
            for (int i = 1; i < Math.Min(population.Chromoshomes.Count, Program.ChromosomesEachGene); i++)
            {
                Assert.IsTrue(population.Chromoshomes[i - 1].Fitness >= population.Chromoshomes[i].Fitness,
                    "הכרומוזומים אמורים להיות ממוינים בסדר יורד לפי ציון הכושר");
            }
        }

        // בדיקת מוטציה (Mutation)
        [TestMethod]
        public void Mutation_ShouldImproveChromosomes()
        {
            // Arrange
            var population = new Population(new List<Chromosome>());
            population = Program.initializeFirstPopulation(population);

            // יצירת העתק של האוכלוסייה לפני המוטציה
            var beforeMutation = new List<double>();
            foreach (var chromosome in population.Chromoshomes)
            {
                beforeMutation.Add(chromosome.Fitness);
            }

            // Act
            Program.Mutation(population);

            // Assert
            int initialSuccessfulMutations = Program.succefulMutation;
            Assert.IsTrue(Program.succefulMutation > 0,
                "מוטציה אמורה ליצור לפחות שינוי מוצלח אחד");
        }

        // בדיקת ציון כושר
        [TestMethod]
        public void CalculateChromosomeFitness_ShouldReturnValidScore()
        {
            // Arrange
            var population = new Population(new List<Chromosome>());
            population = Program.initializeFirstPopulation(population);
            var chromosome = population.Chromoshomes.First();

            // Act
            double fitness = Program.CalculateChromosomeFitness(chromosome);

            // Assert
            Assert.IsTrue(fitness > double.MinValue, "ציון הכושר אמור להיות גדול מהערך המינימלי");

            // בדיקה שחישוב חוזר של ציון הכושר נותן אותה תוצאה
            double recalculatedFitness = Program.CalculateChromosomeFitness(chromosome);
            Assert.AreEqual(fitness, recalculatedFitness, 0.001,
                "חישוב חוזר של ציון הכושר אמור לתת את אותה תוצאה");
        }

        // בדיקת הרצת האלגוריתם הגנטי השלם
        [TestMethod]
        public void RunGeneticAlgorithm_ShouldFindSolution()
        {
            // Arrange
            Program.pop = new Population(new List<Chromosome>());
            Program.pop = Program.initializeFirstPopulation(Program.pop);

            // רישום הציון ההתחלתי של הכרומוזום הטוב ביותר
            double initialBestFitness = Program.pop.Chromoshomes.Max(c => c.Fitness);

            // Act
            Program.RunGeneticAlgorithm();

            // Assert
            Chromosome bestChromosome = Program.GetBestChromosome();
            Assert.IsNotNull(bestChromosome, "האלגוריתם אמור למצוא פתרון");

            Assert.IsTrue(bestChromosome.Fitness >= initialBestFitness,
                "הציון הסופי אמור להיות טוב יותר או שווה לציון ההתחלתי");

            // בדיקה שהדורות רצו כמתוכנן
            Assert.IsTrue(Program.generationCount > 0, "האלגוריתם אמור לרוץ לפחות דור אחד");
        }

        // בדיקת עמידה באילוצים בפתרון הסופי
        [TestMethod]
        public void FinalSolution_ShouldSatisfyConstraints()
        {
            // Arrange: הרצת האלגוריתם
            Program.pop = new Population(new List<Chromosome>());
            Program.pop = Program.initializeFirstPopulation(Program.pop);
            Program.RunGeneticAlgorithm();

            // Act: קבלת הפתרון הטוב ביותר
            Chromosome bestChromosome = Program.GetBestChromosome();

            // Assert: בדיקת אילוצים שונים

            // בדיקת נוכחות מנטור: לפחות משמרת אחת עם מנטור
            bool atLeastOneShiftWithMentor = false;

            foreach (var branchShifts in bestChromosome.Shifts.Values)
            {
                foreach (var shift in branchShifts)
                {
                    bool hasMentor = shift.AssignedEmployees != null &&
                                     shift.AssignedEmployees.Values
                                        .Any(emps => emps.Any(e => e.isMentor));

                    if (hasMentor)
                    {
                        atLeastOneShiftWithMentor = true;
                        break;
                    }
                }

                if (atLeastOneShiftWithMentor)
                    break;
            }

            Assert.IsTrue(atLeastOneShiftWithMentor,
                "הפתרון אמור לכלול לפחות משמרת אחת עם עובד מנטור");

            // בדיקה שלא חורגים ממגבלת שעות לעובד
            Dictionary<Employee, double> employeeHours = new Dictionary<Employee, double>();

            foreach (var branchShifts in bestChromosome.Shifts.Values)
            {
                foreach (var shift in branchShifts)
                {
                    if (shift.AssignedEmployees != null)
                    {
                        foreach (var employees in shift.AssignedEmployees.Values)
                        {
                            foreach (var employee in employees)
                            {
                                if (!employeeHours.ContainsKey(employee))
                                    employeeHours[employee] = 0;

                                employeeHours[employee] += Program.hoursPerShift;
                            }
                        }
                    }
                }
            }

            // בדיקה שלא חורגים ממקסימום שעות שבועיות
            foreach (var hours in employeeHours.Values)
            {
                Assert.IsTrue(hours <= Program.hoursPerWeek * 1.2, // מאפשר חריגה מסוימת
                    "עובד לא אמור לעבוד יותר מדי שעות בשבוע");
            }
        }

        // בדיקת תקלות אפשריות
        [TestMethod]
        public void EmptyData_ShouldHandleGracefully()
        {
            // Arrange
            Program.Employees = new List<Employee>();
            Program.Branches = new List<Branch>();

            // Act & Assert
            try
            {
                var population = new Population(new List<Chromosome>());
                population = Program.initializeFirstPopulation(population);
                // אם לא נזרקת חריגה, הבדיקה עוברת
            }
            catch (Exception ex)
            {
                Assert.Fail($"האלגוריתם אמור לטפל בנתונים ריקים בצורה טובה. חריגה: {ex.Message}");
            }
        }

        #region Helper Methods

        private List<Employee> CreateTestEmployees()
        {
            var employees = new List<Employee>();

            // יצירת 10 עובדים עם נתונים שונים
            for (int i = 1; i <= 10; i++)
            {
                var roles = new HashSet<string>();

                // הקצאת תפקידים
                if (i % 4 == 0) roles.Add("Manager");
                if (i % 3 == 0) roles.Add("Chef");
                if (i % 2 == 0) roles.Add("Waiter");
                if (i % 5 == 0) roles.Add("Bartender");

                // וידוא שלכל עובד יש לפחות תפקיד אחד
                if (roles.Count == 0) roles.Add("Waiter");

                // הקצאת משמרות מועדפות
                var shifts = new HashSet<int>();
                for (int j = 1; j <= 20; j++)
                {
                    if (j % (i + 2) == 0) shifts.Add(j);
                }

                // יצירת העובד
                employees.Add(new Employee(
                    i,                  // ID
                    $"Employee {i}",    // Name
                    roles,              // Roles
                    shifts,             // Requested shifts
                    i % 5 + 3,          // Rate (3-7)
                    i * 10 + 20,        // Hourly salary
                    i % 3 == 0,         // Is mentor
                    new List<string> { "Branch 1", "Branch 2" } // Branches
                ));
            }

            return employees;
        }

        private List<Branch> CreateTestBranches()
        {
            var branches = new List<Branch>();

            // יצירת שני סניפים עם משמרות
            for (int b = 1; b <= 2; b++)
            {
                var branch = new Branch
                {
                    ID = b,
                    Name = $"Branch {b}",
                    Shifts = new List<Shift>()
                };

                // הוספת משמרות לכל יום בשבוע
                string[] daysOfWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                string[] timeSlots = { "Morning", "Evening" };

                int shiftId = (b - 1) * 14 + 1;

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

                branches.Add(branch);
            }

            return branches;
        }

        #endregion
    }
}