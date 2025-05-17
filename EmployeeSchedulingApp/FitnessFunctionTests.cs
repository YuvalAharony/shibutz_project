using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Final;

namespace EmployeeSchedulingApp.Tests.UnitTests.AlgorithmTests
{
    [TestClass]
    public class FitnessFunctionTests
    {
        // נתוני בדיקה
        private List<Employee> testEmployees;
        private List<Branch> testBranches;
        private Shift testShift;

        // הגדרת סביבת הבדיקה
        [TestInitialize]
        public void TestInitialize()
        {
            // יצירת נתוני בדיקה
            SetupTestData();
        }

        // בדיקת חישוב ציון כושר של משמרת
        [TestMethod]
        public void CalculateShiftFitness_WithMentor_ShouldGiveBonus()
        {
            // Arrange - משמרת עם עובד מנטור
            var shift = CreateShiftWithMentor();
            var weeklyHours = new Dictionary<Employee, double>();
            var dailyHours = new Dictionary<Employee, Dictionary<string, double>>();

            // נשתמש בשיקוף (Reflection) כדי לגשת למתודה פרטית
            var method = typeof(Program).GetMethod("CalculateShiftFitness",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var result = method.Invoke(null, new object[] { shift, weeklyHours, dailyHours });
            double fitnessWithMentor = Convert.ToDouble(result);

            // Arrange - משמרת ללא עובד מנטור
            var shiftWithoutMentor = CreateShiftWithoutMentor();

            // Act
            var resultWithoutMentor = method.Invoke(null, new object[] { shiftWithoutMentor, weeklyHours, dailyHours });
            double fitnessWithoutMentor = Convert.ToDouble(resultWithoutMentor);

            // Assert
            Assert.IsTrue(fitnessWithMentor > fitnessWithoutMentor,
                "משמרת עם עובד מנטור אמורה לקבל ציון כושר גבוה יותר");
        }

        // בדיקת חישוב ציון עבור התאמת רמת עובדים לסוג אירוע
        [TestMethod]
        public void CalculateEmployeeRatingVsEventTypeFitness_SpecialEvent_HighRating_ShouldGiveBonus()
        {
            // Arrange - משמרת מיוחדת עם עובדים בעלי דירוג גבוה
            var specialShiftHighRating = CreateSpecialShiftWithHighRatingEmployees();
            var weeklyHours = new Dictionary<Employee, double>();
            var dailyHours = new Dictionary<Employee, Dictionary<string, double>>();

            // הפעלת החישוב
            var method = typeof(Program).GetMethod("CalculateShiftFitness",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var result = method.Invoke(null, new object[] { specialShiftHighRating, weeklyHours, dailyHours });
            double fitnessHighRating = Convert.ToDouble(result);

            // Arrange - משמרת מיוחדת עם עובדים בעלי דירוג נמוך
            var specialShiftLowRating = CreateSpecialShiftWithLowRatingEmployees();

            // Act
            var resultLowRating = method.Invoke(null, new object[] { specialShiftLowRating, weeklyHours, dailyHours });
            double fitnessLowRating = Convert.ToDouble(resultLowRating);

            // Assert
            Assert.IsTrue(fitnessHighRating > fitnessLowRating,
                "משמרת מיוחדת עם עובדים בעלי דירוג גבוה אמורה לקבל ציון גבוה יותר");
        }

        // בדיקת חישוב ציון עבור איזון צוותי
        [TestMethod]
        public void CalculateTeamBalanceFitness_BalancedTeam_ShouldGiveBonus()
        {
            // Arrange - צוות מאוזן (30-70% מנטורים)
            var balancedShift = CreateBalancedShift();
            var weeklyHours = new Dictionary<Employee, double>();
            var dailyHours = new Dictionary<Employee, Dictionary<string, double>>();

            // הפעלת החישוב
            var method = typeof(Program).GetMethod("CalculateShiftFitness",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var result = method.Invoke(null, new object[] { balancedShift, weeklyHours, dailyHours });
            double fitnessBalanced = Convert.ToDouble(result);

            // Arrange - צוות לא מאוזן (יותר מדי מנטורים)
            var unbalancedShift = CreateUnbalancedShift();

            // Act
            var resultUnbalanced = method.Invoke(null, new object[] { unbalancedShift, weeklyHours, dailyHours });
            double fitnessUnbalanced = Convert.ToDouble(resultUnbalanced);

            // Assert
            Assert.IsTrue(fitnessBalanced > fitnessUnbalanced,
                "משמרת עם צוות מאוזן אמורה לקבל ציון גבוה יותר");
        }

        // בדיקת חישוב ציון עם קנסות על חריגה ממגבלות שעות
        [TestMethod]
        public void ApplyHoursConstraintsPenalties_ShouldReduceFitness()
        {
            // Arrange
            double baseScore = 1000.0;

            // עובדים עם שעות בגבולות המותר
            var regularEmployee = testEmployees[0];
            var weeklyHoursRegular = new Dictionary<Employee, double>
            {
                { regularEmployee, Program.hoursPerWeek - 5 }
            };

            var dailyHoursRegular = new Dictionary<Employee, Dictionary<string, double>>
            {
                { regularEmployee, new Dictionary<string, double> { { "Sunday", Program.hoursPerDay - 1 } } }
            };

            // עובד עם חריגה בשעות
            var overworkedEmployee = testEmployees[1];
            var weeklyHoursOver = new Dictionary<Employee, double>
            {
                { overworkedEmployee, Program.hoursPerWeek + 10 }
            };

            var dailyHoursOver = new Dictionary<Employee, Dictionary<string, double>>
            {
                { overworkedEmployee, new Dictionary<string, double> { { "Sunday", Program.hoursPerDay + 3 } } }
            };

            // נשתמש בשיקוף לגישה למתודות הפרטיות
            var methodWeekly = typeof(Program).GetMethod("ApplyWeeklyHoursConstraints",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var methodDaily = typeof(Program).GetMethod("ApplyDailyHoursConstraints",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act - בדיקת עובד רגיל
            var resultRegular = methodWeekly.Invoke(null, new object[] { baseScore, weeklyHoursRegular });
            double fitnessRegularWeekly = Convert.ToDouble(resultRegular);

            resultRegular = methodDaily.Invoke(null, new object[] { fitnessRegularWeekly, dailyHoursRegular });
            double fitnessRegular = Convert.ToDouble(resultRegular);

            // Act - בדיקת עובד עם שעות חריגות
            var resultOver = methodWeekly.Invoke(null, new object[] { baseScore, weeklyHoursOver });
            double fitnessOverWeekly = Convert.ToDouble(resultOver);

            resultOver = methodDaily.Invoke(null, new object[] { fitnessOverWeekly, dailyHoursOver });
            double fitnessOver = Convert.ToDouble(resultOver);

            // Assert
            Assert.AreEqual(baseScore, fitnessRegular,
                "ציון עובד עם שעות בגבולות המותר לא אמור להשתנות");

            Assert.IsTrue(fitnessOver < baseScore,
                "ציון עובד עם שעות חריגות אמור להיות נמוך יותר");
        }

        // בדיקת חישוב הציון הממוצע של העובדים במשמרת
        [TestMethod]
        public void CalculateAverageEmployeeRate_ShouldReturnCorrectValue()
        {
            // Arrange
            var shift = CreateShiftWithMixedRatings();

            // נשתמש בשיקוף לגישה למתודה הפרטית
            var method = typeof(Program).GetMethod("CalculateAverageEmployeeRate",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var result = method.Invoke(null, new object[] { shift });
            double averageRate = Convert.ToDouble(result);

            // Assert
            // חישוב הממוצע הצפוי: (7 + 4 + 3 + 5) / 4 = 4.75
            Assert.AreEqual(4.75, averageRate, 0.001,
                "ממוצע הדירוגים אמור להיות 4.75");
        }

        // בדיקת חישוב יחס העובדים המנוסים
        [TestMethod]
        public void CalculateExperiencedRatio_ShouldReturnCorrectValue()
        {
            // Arrange
            var shift = CreateShiftForExperiencedRatio();

            // נשתמש בשיקוף לגישה למתודה הפרטית
            var method = typeof(Program).GetMethod("CalculateExperiencedRatio",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var result = method.Invoke(null, new object[] { shift });
            double experiencedRatio = Convert.ToDouble(result);

            // Assert
            // יחס צפוי: 2 מנטורים מתוך 5 עובדים = 0.4
            Assert.AreEqual(0.4, experiencedRatio, 0.001,
                "יחס העובדים המנוסים אמור להיות 0.4");
        }

        #region Helper Methods

        private void SetupTestData()
        {
            // יצירת עובדים לבדיקות
            testEmployees = new List<Employee>();

            // עובד 1 - מנטור עם דירוג גבוה
            testEmployees.Add(new Employee(
                1, "Mentor Employee", new HashSet<string> { "Manager" },
                new HashSet<int> { 1, 2, 3 }, 7, 100, true,
                new List<string> { "Test Branch" }
            ));

            // עובד 2 - לא מנטור עם דירוג נמוך
            testEmployees.Add(new Employee(
                2, "Regular Employee", new HashSet<string> { "Waiter" },
                new HashSet<int> { 1, 2, 3 }, 4, 80, false,
                new List<string> { "Test Branch" }
            ));

            // עובד 3 - לא מנטור עם דירוג נמוך מאוד
            testEmployees.Add(new Employee(
                3, "Low Rated Employee", new HashSet<string> { "Waiter" },
                new HashSet<int> { 1, 2, 3 }, 3, 60, false,
                new List<string> { "Test Branch" }
            ));

            // עובד 4 - מנטור עם דירוג בינוני
            testEmployees.Add(new Employee(
                4, "Medium Mentor", new HashSet<string> { "Chef" },
                new HashSet<int> { 1, 2, 3 }, 5, 90, true,
                new List<string> { "Test Branch" }
            ));

            // עובד 5 - לא מנטור עם דירוג גבוה
            testEmployees.Add(new Employee(
                5, "High Regular", new HashSet<string> { "Bartender" },
                new HashSet<int> { 1, 2, 3 }, 6, 85, false,
                new List<string> { "Test Branch" }
            ));

            // יצירת סניף לבדיקות
            testBranches = new List<Branch>
            {
                new Branch
                {
                    ID = 1,
                    Name = "Test Branch",
                    Shifts = new List<Shift>()
                }
            };

            // יצירת משמרת בסיסית לבדיקות
            testShift = new Shift
            {
                Id = 1,
                branch = "Test Branch",
                day = "Sunday",
                TimeSlot = "Morning",
                EventType = "Regular",
                RequiredRoles = new Dictionary<string, int>
                {
                    { "Manager", 1 },
                    { "Chef", 1 },
                    { "Waiter", 2 },
                    { "Bartender", 1 }
                },
                AssignedEmployees = new Dictionary<string, List<Employee>>()
            };
        }

        private Shift CreateShiftWithMentor()
        {
            // העתקת המשמרת הבסיסית
            Shift shift = new Shift
            {
                Id = testShift.Id,
                branch = testShift.branch,
                day = testShift.day,
                TimeSlot = testShift.TimeSlot,
                EventType = testShift.EventType,
                RequiredRoles = new Dictionary<string, int>(testShift.RequiredRoles),
                AssignedEmployees = new Dictionary<string, List<Employee>>()
            };

            // הוספת עובד מנטור למשמרת
            shift.AssignedEmployees["Manager"] = new List<Employee> { testEmployees[0] }; // מנטור
            shift.AssignedEmployees["Waiter"] = new List<Employee> { testEmployees[1], testEmployees[2] }; // לא מנטורים
            shift.AssignedEmployees["Chef"] = new List<Employee> { testEmployees[3] }; // מנטור
            shift.AssignedEmployees["Bartender"] = new List<Employee> { testEmployees[4] }; // לא מנטור

            return shift;
        }

        private Shift CreateShiftWithoutMentor()
        {
            // העתקת המשמרת הבסיסית
            Shift shift = new Shift
            {
                Id = testShift.Id,
                branch = testShift.branch,
                day = testShift.day,
                TimeSlot = testShift.TimeSlot,
                EventType = testShift.EventType,
                RequiredRoles = new Dictionary<string, int>(testShift.RequiredRoles),
                AssignedEmployees = new Dictionary<string, List<Employee>>()
            };

            // הוספת עובדים שאינם מנטורים בלבד
            shift.AssignedEmployees["Manager"] = new List<Employee> { testEmployees[4] }; // לא מנטור
            shift.AssignedEmployees["Waiter"] = new List<Employee> { testEmployees[1], testEmployees[2] }; // לא מנטורים
            shift.AssignedEmployees["Chef"] = new List<Employee>(); // אין עובד
            shift.AssignedEmployees["Bartender"] = new List<Employee>(); // אין עובד

            return shift;
        }

        private Shift CreateSpecialShiftWithHighRatingEmployees()
        {
            // העתקת המשמרת הבסיסית עם שינוי לאירוע מיוחד
            Shift shift = new Shift
            {
                Id = testShift.Id,
                branch = testShift.branch,
                day = testShift.day,
                TimeSlot = testShift.TimeSlot,
                EventType = "Special", // אירוע מיוחד
                RequiredRoles = new Dictionary<string, int>(testShift.RequiredRoles),
                AssignedEmployees = new Dictionary<string, List<Employee>>()
            };

            // הוספת עובדים בעלי דירוג גבוה
            shift.AssignedEmployees["Manager"] = new List<Employee> { testEmployees[0] }; // דירוג 7
            shift.AssignedEmployees["Waiter"] = new List<Employee> { testEmployees[4] }; // דירוג 6

            return shift;
        }

        private Shift CreateSpecialShiftWithLowRatingEmployees()
        {
            // העתקת המשמרת הבסיסית עם שינוי לאירוע מיוחד
            Shift shift = new Shift
            {
                Id = testShift.Id,
                branch = testShift.branch,
                day = testShift.day,
                TimeSlot = testShift.TimeSlot,
                EventType = "Special", // אירוע מיוחד
                RequiredRoles = new Dictionary<string, int>(testShift.RequiredRoles),
                AssignedEmployees = new Dictionary<string, List<Employee>>()
            };

            // הוספת עובדים בעלי דירוג נמוך
            shift.AssignedEmployees["Manager"] = new List<Employee> { testEmployees[2] }; // דירוג 3
            shift.AssignedEmployees["Waiter"] = new List<Employee> { testEmployees[1] }; // דירוג 4

            return shift;
        }

        private Shift CreateBalancedShift()
        {
            // העתקת המשמרת הבסיסית
            Shift shift = new Shift
            {
                Id = testShift.Id,
                branch = testShift.branch,
                day = testShift.day,
                TimeSlot = testShift.TimeSlot,
                EventType = testShift.EventType,
                RequiredRoles = new Dictionary<string, int>(testShift.RequiredRoles),
                AssignedEmployees = new Dictionary<string, List<Employee>>()
            };

            // הוספת צוות מאוזן - 40% מנטורים (2 מתוך 5)
            shift.AssignedEmployees["Manager"] = new List<Employee> { testEmployees[0] }; // מנטור
            shift.AssignedEmployees["Waiter"] = new List<Employee> { testEmployees[1], testEmployees[2] }; // לא מנטורים
            shift.AssignedEmployees["Chef"] = new List<Employee> { testEmployees[3] }; // מנטור
            shift.AssignedEmployees["Bartender"] = new List<Employee> { testEmployees[4] }; // לא מנטור

            return shift;
        }

        private Shift CreateUnbalancedShift()
        {
            // העתקת המשמרת הבסיסית
            Shift shift = new Shift
            {
                Id = testShift.Id,
                branch = testShift.branch,
                day = testShift.day,
                TimeSlot = testShift.TimeSlot,
                EventType = testShift.EventType,
                RequiredRoles = new Dictionary<string, int>(testShift.RequiredRoles),
                AssignedEmployees = new Dictionary<string, List<Employee>>()
            };

            // הוספת צוות לא מאוזן - 100% מנטורים
            shift.AssignedEmployees["Manager"] = new List<Employee> { testEmployees[0] }; // מנטור
            shift.AssignedEmployees["Chef"] = new List<Employee> { testEmployees[3] }; // מנטור

            return shift;
        }

        private Shift CreateShiftWithMixedRatings()
        {
            // העתקת המשמרת הבסיסית
            Shift shift = new Shift
            {
                Id = testShift.Id,
                branch = testShift.branch,
                day = testShift.day,
                TimeSlot = testShift.TimeSlot,
                EventType = testShift.EventType,
                RequiredRoles = new Dictionary<string, int>(testShift.RequiredRoles),
                AssignedEmployees = new Dictionary<string, List<Employee>>()
            };

            // הוספת עובדים עם דירוגים מעורבים
            shift.AssignedEmployees["Manager"] = new List<Employee> { testEmployees[0] }; // דירוג 7
            shift.AssignedEmployees["Waiter"] = new List<Employee> { testEmployees[1] }; // דירוג 4
            shift.AssignedEmployees["Chef"] = new List<Employee> { testEmployees[2] }; // דירוג 3
            shift.AssignedEmployees["Bartender"] = new List<Employee> { testEmployees[3] }; // דירוג 5

            return shift;
        }

        private Shift CreateShiftForExperiencedRatio()
        {
            // העתקת המשמרת הבסיסית
            Shift shift = new Shift
            {
                Id = testShift.Id,
                branch = testShift.branch,
                day = testShift.day,
                TimeSlot = testShift.TimeSlot,
                EventType = testShift.EventType,
                RequiredRoles = new Dictionary<string, int>(testShift.RequiredRoles),
                AssignedEmployees = new Dictionary<string, List<Employee>>()
            };

            // יצירת צוות עם 2 מנטורים מתוך 5 עובדים (40%)
            shift.AssignedEmployees["Manager"] = new List<Employee> { testEmployees[0] }; // מנטור
            shift.AssignedEmployees["Waiter"] = new List<Employee> { testEmployees[1], testEmployees[2] }; // לא מנטורים
            shift.AssignedEmployees["Chef"] = new List<Employee> { testEmployees[3] }; // מנטור
            shift.AssignedEmployees["Bartender"] = new List<Employee> { testEmployees[4] }; // לא מנטור

            return shift;
        }

        #endregion
    }
}