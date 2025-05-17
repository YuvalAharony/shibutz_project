using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeSchedulingApp;
using Final;

namespace EmployeeSchedulingApp.Tests.TestHelpers
{
    /// <summary>
    /// מתאם (Adapter) לביצוע בדיקות על אלגוריתם השיבוץ
    /// </summary>
    public class SchedulingAlgorithmAdapter
    {
        // הרצת האלגוריתם הגנטי
        public static Chromosome RunAlgorithm(List<Branch> branches, List<Employee> employees, int generations = 200)
        {
            // שמירת הערכים המקוריים
            var originalEmployees = Program.Employees;
            var originalBranches = Program.Branches;
            var originalGenerations = Program.Generations;

            try
            {
                // הגדרת הנתונים לבדיקה
                Program.Employees = employees;
                Program.Branches = branches;

                // הרצת האלגוריתם
                Program.pop = new Population(new List<Chromosome>());
                Program.pop = Program.initializeFirstPopulation(Program.pop);
                Program.RunGeneticAlgorithm();

                // החזרת הפתרון הטוב ביותר
                return Program.GetBestChromosome();
            }
            finally
            {
                // שחזור הערכים המקוריים
                Program.Employees = originalEmployees;
                Program.Branches = originalBranches;
            }
        }

        // הרצת האלגוריתם באופן מקבילי עם נתונים שונים
        public static async Task<List<Chromosome>> RunParallelAlgorithms(
            int numRuns,
            List<Branch> branches,
            List<Employee> employees,
            int generations = 10)
        {
            var tasks = new List<Task<Chromosome>>();

            for (int i = 0; i < numRuns; i++)
            {
                // יצירת העתק של הנתונים למניעת התנגשויות בשימוש מקבילי
                var branchesCopy = CopyBranches(branches);
                var employeesCopy = CopyEmployees(employees);

                // הוספת משימה חדשה
                tasks.Add(Task.Run(() => RunAlgorithm(branchesCopy, employeesCopy, generations)));
            }

            // המתנה לסיום כל המשימות
            var results = await Task.WhenAll(tasks);

            return new List<Chromosome>(results);
        }

        // השוואת פתרונות
        public static double CompareChromosomes(Chromosome c1, Chromosome c2)
        {
            if (c1 == null || c2 == null)
                return -1;

            int totalShifts = 0;
            int differentAssignments = 0;

            foreach (var branchEntry in c1.Shifts)
            {
                string branchName = branchEntry.Key;
                if (!c2.Shifts.ContainsKey(branchName))
                    continue;

                var shifts1 = branchEntry.Value;
                var shifts2 = c2.Shifts[branchName];

                foreach (var shift1 in shifts1)
                {
                    var shift2 = shifts2.Find(s => s.Id == shift1.Id);

                    if (shift2 != null)
                    {
                        totalShifts++;

                        if (shift1.AssignedEmployees == null && shift2.AssignedEmployees == null)
                            continue;

                        if ((shift1.AssignedEmployees == null) != (shift2.AssignedEmployees == null))
                        {
                            differentAssignments++;
                            continue;
                        }

                        foreach (var roleEntry in shift1.AssignedEmployees)
                        {
                            string role = roleEntry.Key;

                            if (!shift2.AssignedEmployees.ContainsKey(role))
                            {
                                differentAssignments++;
                                continue;
                            }

                            var employees1 = roleEntry.Value;
                            var employees2 = shift2.AssignedEmployees[role];

                            if (employees1.Count != employees2.Count)
                            {
                                differentAssignments++;
                                continue;
                            }

                            for (int i = 0; i < employees1.Count; i++)
                            {
                                if (employees1[i].ID != employees2[i].ID)
                                {
                                    differentAssignments++;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return totalShifts > 0 ? (double)differentAssignments / totalShifts : 0;
        }

        // חישוב מדדי איכות שונים לפתרון
        public static Dictionary<string, double> CalculateSolutionMetrics(Chromosome solution, List<Employee> employees)
        {
            var metrics = new Dictionary<string, double>();

            // סך העובדים ששובצו
            int totalAssigned = 0;

            // סך המשמרות עם עובד מנוסה
            int shiftsWithMentor = 0;

            // סך המשמרות שעומדות במכסת העובדים
            int shiftsWithRequiredCount = 0;

            // סך המשמרות
            int totalShifts = 0;

            // סך תפקידים שדורשים שיבוץ
            int totalRequiredRoles = 0;

            // סך שיבוצים לעובדים מועדפים
            int preferredAssignments = 0;

            // העמסת עובדים
            Dictionary<Employee, int> employeeLoad = new Dictionary<Employee, int>();

            foreach (var branchShifts in solution.Shifts.Values)
            {
                foreach (var shift in branchShifts)
                {
                    totalShifts++;
                    int requiredCount = 0;
                    int assignedCount = 0;

                    // חישוב העובדים הדרושים
                    foreach (var requiredRole in shift.RequiredRoles)
                    {
                        requiredCount += requiredRole.Value;
                        totalRequiredRoles += requiredRole.Value;
                    }

                    if (shift.AssignedEmployees != null)
                    {
                        bool hasMentor = false;

                        foreach (var roleEmployees in shift.AssignedEmployees)
                        {
                            foreach (var employee in roleEmployees.Value)
                            {
                                assignedCount++;
                                totalAssigned++;

                                // בדיקה אם העובד הוא מנטור
                                if (employee.isMentor)
                                    hasMentor = true;

                                // בדיקה אם המשמרת היא מועדפת
                                if (employee.requestedShifts.Contains(shift.Id))
                                    preferredAssignments++;

                                // עדכון העומס על העובד
                                if (!employeeLoad.ContainsKey(employee))
                                    employeeLoad[employee] = 0;

                                employeeLoad[employee]++;
                            }
                        }

                        if (hasMentor)
                            shiftsWithMentor++;
                    }

                    if (assignedCount >= requiredCount)
                        shiftsWithRequiredCount++;
                }
            }

            // חישוב ממוצע וסטיית תקן של עומס
            double avgLoad = 0;
            double stdDevLoad = 0;

            if (employeeLoad.Count > 0)
            {
                avgLoad = employeeLoad.Values.Average();
                double sumSquares = 0;

                foreach (var load in employeeLoad.Values)
                {
                    sumSquares += Math.Pow(load - avgLoad, 2);
                }

                stdDevLoad = Math.Sqrt(sumSquares / employeeLoad.Count);
            }

            // הוספת המדדים למילון
            metrics["TotalEmployees"] = employees.Count;
            metrics["TotalAssigned"] = totalAssigned;
            metrics["TotalShifts"] = totalShifts;
            metrics["TotalRequiredRoles"] = totalRequiredRoles;
            metrics["ShiftsWithMentor"] = shiftsWithMentor;
            metrics["ShiftsWithRequiredCount"] = shiftsWithRequiredCount;
            metrics["PreferredAssignments"] = preferredAssignments;
            metrics["EmployeesAssigned"] = employeeLoad.Count;
            metrics["AverageLoad"] = avgLoad;
            metrics["StdDevLoad"] = stdDevLoad;
            metrics["AssignmentRatio"] = totalRequiredRoles > 0 ? (double)totalAssigned / totalRequiredRoles : 0;
            metrics["MentorRatio"] = totalShifts > 0 ? (double)shiftsWithMentor / totalShifts : 0;
            metrics["PreferredRatio"] = totalAssigned > 0 ? (double)preferredAssignments / totalAssigned : 0;
            metrics["CompletionRatio"] = totalShifts > 0 ? (double)shiftsWithRequiredCount / totalShifts : 0;

            return metrics;
        }

        #region Helper Methods

        private static List<Branch> CopyBranches(List<Branch> branches)
        {
            var branchesCopy = new List<Branch>();

            foreach (var branch in branches)
            {
                var newBranch = new Branch
                {
                    ID = branch.ID,
                    Name = branch.Name,
                    Shifts = new List<Shift>()
                };

                foreach (var shift in branch.Shifts)
                {
                    var newShift = new Shift
                    {
                        Id = shift.Id,
                        branch = shift.branch,
                        day = shift.day,
                        TimeSlot = shift.TimeSlot,
                        EventType = shift.EventType,
                        RequiredRoles = new Dictionary<string, int>(),
                        AssignedEmployees = new Dictionary<string, List<Employee>>()
                    };

                    foreach (var roleEntry in shift.RequiredRoles)
                    {
                        newShift.RequiredRoles[roleEntry.Key] = roleEntry.Value;
                    }

                    newBranch.Shifts.Add(newShift);
                }

                branchesCopy.Add(newBranch);
            }

            return branchesCopy;
        }

        private static List<Employee> CopyEmployees(List<Employee> employees)
        {
            var employeesCopy = new List<Employee>();

            foreach (var emp in employees)
            {
                var newRoles = new HashSet<string>(emp.roles);
                var newShifts = new HashSet<int>(emp.requestedShifts);
                var newBranches = new List<string>(emp.Branches);

                var newEmployee = new Employee(
                    emp.ID,
                    emp.Name,
                    newRoles,
                    newShifts,
                    emp.Rate,
                    emp.HourlySalary,
                    emp.isMentor,
                    newBranches
                );

                employeesCopy.Add(newEmployee);
            }

            return employeesCopy;
        }

        #endregion
    }
}