using shibutz_project;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace shibutz_project
{
<<<<<<< HEAD
    public class DB
    {
        public static List<Branch> addBranches()
        {
            #region shifts
            List<Shift> Branch1Shifts = new List<Shift>
            {
                new Shift(101, "Branch 1", "Morning", "Sunday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(102, "Branch 1", "Evening", "Sunday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 2}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(103, "Branch 1", "Morning", "Monday", new Dictionary<string, int>{{"Waiter", 2}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(104, "Branch 1", "Evening", "Monday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(105, "Branch 1", "Morning", "Tuesday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(106, "Branch 1", "Evening", "Tuesday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 2}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(107, "Branch 1", "Morning", "Wednesday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 3}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(108, "Branch 1", "Evening", "Wednesday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(109, "Branch 1", "Morning", "Thursday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Special Event"),
                new Shift(110, "Branch 1", "Evening", "Thursday", new Dictionary<string, int>{{"Waiter", 5}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Special Event"),
                new Shift(111, "Branch 1", "Morning", "Friday", new Dictionary<string, int>{{"Waiter", 5}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Special Event"),
                new Shift(112, "Branch 1", "Evening", "Friday", new Dictionary<string, int>{{"Waiter", 6}, {"Chef", 4}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Peak Hour"),
                new Shift(113, "Branch 1", "Morning", "Saturday", new Dictionary<string, int>{{"Waiter", 6}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Peak Hour"),
                new Shift(114, "Branch 1", "Evening", "Saturday", new Dictionary<string, int>{{"Waiter", 7}, {"Chef", 4}, {"Bartender", 3}, {"Manager", 1}}, false, new HashSet<int>(), "Peak Hour")
            };

            // ----------------------------------------------------
            // BRANCH 2
            // ----------------------------------------------------
            List<Shift> Branch2Shifts = new List<Shift>
            {
                new Shift(201, "Branch 2", "Morning", "Sunday", new Dictionary<string, int>{{"Waiter", 2}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(202, "Branch 2", "Evening", "Sunday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 2}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(203, "Branch 2", "Morning", "Monday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(204, "Branch 2", "Evening", "Monday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(205, "Branch 2", "Morning", "Tuesday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(206, "Branch 2", "Evening", "Tuesday", new Dictionary<string, int>{{"Waiter", 5}, {"Chef", 2}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(207, "Branch 2", "Morning", "Wednesday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 3}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(208, "Branch 2", "Evening", "Wednesday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(209, "Branch 2", "Morning", "Thursday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 2}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Special Event"),
                new Shift(210, "Branch 2", "Evening", "Thursday", new Dictionary<string, int>{{"Waiter", 5}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Special Event"),
                new Shift(211, "Branch 2", "Morning", "Friday", new Dictionary<string, int>{{"Waiter", 5}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Special Event"),
                new Shift(212, "Branch 2", "Evening", "Friday", new Dictionary<string, int>{{"Waiter", 6}, {"Chef", 4}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Peak Hour"),
                new Shift(213, "Branch 2", "Morning", "Saturday", new Dictionary<string, int>{{"Waiter", 6}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Peak Hour"),
                new Shift(214, "Branch 2", "Evening", "Saturday", new Dictionary<string, int>{{"Waiter", 7}, {"Chef", 4}, {"Bartender", 3}, {"Manager", 1}}, false, new HashSet<int>(), "Peak Hour")
            };

            // ----------------------------------------------------
            // BRANCH 3
            // ----------------------------------------------------
            List<Shift> Branch3Shifts = new List<Shift>
            {
                new Shift(301, "Branch 3", "Morning", "Sunday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(302, "Branch 3", "Evening", "Sunday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 2}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(303, "Branch 3", "Morning", "Monday", new Dictionary<string, int>{{"Waiter", 2}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(304, "Branch 3", "Evening", "Monday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(305, "Branch 3", "Morning", "Tuesday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(306, "Branch 3", "Evening", "Tuesday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 2}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(307, "Branch 3", "Morning", "Wednesday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 3}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(308, "Branch 3", "Evening", "Wednesday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(309, "Branch 3", "Morning", "Thursday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Special Event"),
                new Shift(310, "Branch 3", "Evening", "Thursday", new Dictionary<string, int>{{"Waiter", 5}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Special Event"),
                new Shift(311, "Branch 3", "Morning", "Friday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Special Event"),
                new Shift(312, "Branch 3", "Evening", "Friday", new Dictionary<string, int>{{"Waiter", 6}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Peak Hour"),
                new Shift(313, "Branch 3", "Morning", "Saturday", new Dictionary<string, int>{{"Waiter", 5}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Peak Hour"),
                new Shift(314, "Branch 3", "Evening", "Saturday", new Dictionary<string, int>{{"Waiter", 7}, {"Chef", 4}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Peak Hour")
            };

            // ----------------------------------------------------
            // BRANCH 4
            // ----------------------------------------------------
            List<Shift> Branch4Shifts = new List<Shift>
            {
                new Shift(401, "Branch 4", "Morning", "Sunday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(402, "Branch 4", "Evening", "Sunday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 2}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(403, "Branch 4", "Morning", "Monday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(404, "Branch 4", "Evening", "Monday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(405, "Branch 4", "Morning", "Tuesday", new Dictionary<string, int>{{"Waiter", 2}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(406, "Branch 4", "Evening", "Tuesday", new Dictionary<string, int>{{"Waiter", 5}, {"Chef", 2}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(407, "Branch 4", "Morning", "Wednesday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 3}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(408, "Branch 4", "Evening", "Wednesday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(409, "Branch 4", "Morning", "Thursday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 2}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Special Event"),
                new Shift(410, "Branch 4", "Evening", "Thursday", new Dictionary<string, int>{{"Waiter", 5}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Special Event"),
                new Shift(411, "Branch 4", "Morning", "Friday", new Dictionary<string, int>{{"Waiter", 5}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Special Event"),
                new Shift(412, "Branch 4", "Evening", "Friday", new Dictionary<string, int>{{"Waiter", 6}, {"Chef", 4}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Peak Hour"),
                new Shift(413, "Branch 4", "Morning", "Saturday", new Dictionary<string, int>{{"Waiter", 6}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Peak Hour"),
                new Shift(414, "Branch 4", "Evening", "Saturday", new Dictionary<string, int>{{"Waiter", 7}, {"Chef", 4}, {"Bartender", 3}, {"Manager", 1}}, false, new HashSet<int>(), "Peak Hour")
            };

            // ----------------------------------------------------
            // BRANCH 5
            // ----------------------------------------------------
            List<Shift> Branch5Shifts = new List<Shift>
            {
                new Shift(501, "Branch 5", "Morning", "Sunday", new Dictionary<string, int>{{"Waiter", 2}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(502, "Branch 5", "Evening", "Sunday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 2}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(503, "Branch 5", "Morning", "Monday", new Dictionary<string, int>{{"Waiter", 2}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(504, "Branch 5", "Evening", "Monday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(505, "Branch 5", "Morning", "Tuesday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 2}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(506, "Branch 5", "Evening", "Tuesday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 2}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(507, "Branch 5", "Morning", "Wednesday", new Dictionary<string, int>{{"Waiter", 3}, {"Chef", 3}, {"Bartender", 1}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(508, "Branch 5", "Evening", "Wednesday", new Dictionary<string, int>{{"Waiter", 5}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Regular"),
                new Shift(509, "Branch 5", "Morning", "Thursday", new Dictionary<string, int>{{"Waiter", 4}, {"Chef", 2}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Special Event"),
                new Shift(510, "Branch 5", "Evening", "Thursday", new Dictionary<string, int>{{"Waiter", 5}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Special Event"),
                new Shift(511, "Branch 5", "Morning", "Friday", new Dictionary<string, int>{{"Waiter", 5}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Special Event"),
                new Shift(512, "Branch 5", "Evening", "Friday", new Dictionary<string, int>{{"Waiter", 6}, {"Chef", 4}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Peak Hour"),
                new Shift(513, "Branch 5", "Morning", "Saturday", new Dictionary<string, int>{{"Waiter", 6}, {"Chef", 3}, {"Bartender", 2}, {"Manager", 1}}, false, new HashSet<int>(), "Peak Hour"),
                new Shift(514, "Branch 5", "Evening", "Saturday", new Dictionary<string, int>{{"Waiter", 7}, {"Chef", 4}, {"Bartender", 3}, {"Manager", 1}}, false, new HashSet<int>(), "Peak Hour")
            };
            #endregion

            #region branches
            List<Branch> branches = new List<Branch>();
            branches.Add(new Branch(1, "Branch 1", Branch1Shifts));
            branches.Add(new Branch(2, "Branch 2", Branch2Shifts));
            branches.Add(new Branch(3, "Branch 3", Branch3Shifts));
            branches.Add(new Branch(4, "Branch 4", Branch4Shifts));
            branches.Add(new Branch(5, "Branch 5", Branch5Shifts));
            #endregion

=======
    class DB
    {
        
      

      

        public static List<Branch> addBranches()
        {
            List<Branch> branches = new List<Branch>();
            branches.Add(new Branch(1, "Branch 1", new List<Shift>()));
            branches.Add(new Branch(2, "Branch 2", new List<Shift>()));
            branches.Add(new Branch(3, "Branch 3", new List<Shift>()));
            branches.Add(new Branch(4, "Branch 4", new List<Shift>()));
            branches.Add(new Branch(5, "Branch 5", new List<Shift>()));
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
            return branches;
        }

        public static List<Employee> addEmployees()
        {
<<<<<<< HEAD
            List<Employee> employees = new List<Employee>();

            // עובדים 1-10 (Branches: "Branch1", "Branch2")
            employees.Add(new Employee(
                1,
                "Olivia Carter",
                new List<string> { "Waiter" },
                new HashSet<int> { 205, 309 }, // 3 משמרות
                0, 30, 0, true,
                new List<string> { "Branch1", "Branch2" }
            ));
            employees.Add(new Employee(
                2,
                "Liam Anderson",
                new List<string> { "Chef" },
                new HashSet<int> { 102, 207, 310, 412 }, // 4 משמרות
                1, 31, 1, false,
                new List<string> { "Branch1", "Branch2" }
            ));
            employees.Add(new Employee(
                3,
                "Emma Thompson",
                new List<string> { "Bartender" },
                new HashSet<int> { 103, 208, 311, 413, 512 }, // 5 משמרות
                2, 32, 2, true,
                new List<string> { "Branch1", "Branch2" }
            ));
            employees.Add(new Employee(
                4,
                "Noah Wilson",
                new List<string> { "Host" },
                new HashSet<int> { 104, 209, 312, 414, 110, 203 }, // 6 משמרות
                3, 33, 3, false,
                new List<string> { "Branch1", "Branch2" }
            ));
            employees.Add(new Employee(
                5,
                "Ava Robinson",
                new List<string> { "Manager" },
                new HashSet<int> { 105, 210, 313, 411, 501, 112, 214 }, // 7 משמרות
                4, 34, 4, true,
                new List<string> { "Branch1", "Branch2" }
            ));
            employees.Add(new Employee(
                6,
                "Mason Clark",
                new List<string> { "Waiter", "Chef" },
                new HashSet<int> { 106, 211, 314, 407, 502, 107, 212, 303 }, // 8 משמרות
                5, 35, 5, false,
                new List<string> { "Branch1", "Branch2" }
            ));
            employees.Add(new Employee(
                7,
                "Sophia Lewis",
                new List<string> { "Bartender", "Host", "Manager" },
                new HashSet<int> { 108, 213, 305, 409, 505, 109, 210, 306, 410 }, // 9 משמרות
                6, 36, 6, true,
                new List<string> { "Branch1", "Branch2" }
            ));
            employees.Add(new Employee(
                8,
                "Logan Walker",
                new List<string> { "Waiter", "Chef", "Bartender" },
                new HashSet<int> { 110, 211, 306, 411, 506, 111, 212, 307, 412, 507 }, // 10 משמרות
                7, 37, 7, false,
                new List<string> { "Branch1", "Branch2" }
            ));
            employees.Add(new Employee(
                9,
                "Isabella Young",
                new List<string> { "Chef", "Host", "Manager" },
                new HashSet<int> { 111, 212, 307, 412, 507, 112, 213, 308, 413, 508, 113 }, // 11 משמרות
                8, 38, 8, true,
                new List<string> { "Branch1", "Branch2" }
            ));
            employees.Add(new Employee(
                10,
                "Lucas Hall",
                new List<string> { "Waiter", "Bartender", "Host", "Manager" },
                new HashSet<int> { 112, 213, 308, 413, 508, 113, 214, 309, 414, 509, 110, 205 }, // 12 משמרות
                9, 39, 9, false,
                new List<string> { "Branch1", "Branch2" }
            ));

            // עובדים 11-20 (Branches: "Branch1", "Branch3")
            employees.Add(new Employee(
                11,
                "Mia Parker",
                new List<string> { "Waiter" },
                new HashSet<int> { 103, 210, 310 }, // 3 משמרות
                10, 40, 10, true,
                new List<string> { "Branch2", "Branch3" }
            ));
            employees.Add(new Employee(
                12,
                "Benjamin Mitchell",
                new List<string> { "Chef" },
                new HashSet<int> { 101, 211, 311, 411,201 }, // 4 משמרות
                11, 41, 11, false,
                new List<string> { "Branch1", "Branch3" }
            ));
            employees.Add(new Employee(
                13,
                "Charlotte Adams",
                new List<string> { "Bartender" },
                new HashSet<int> { 102, 212, 312, 412, 512 }, // 5 משמרות
                12, 42, 12, true,
                new List<string> { "Branch1", "Branch3" }
            ));
            employees.Add(new Employee(
                14,
                "Amelia Turner",
                new List<string> { "Host" },
                new HashSet<int> { 103, 213, 313, 413, 111, 204 }, // 6 משמרות
                13, 43, 13, false,
                new List<string> { "Branch1", "Branch3" }
            ));
            employees.Add(new Employee(
                15,
                "Oliver Scott",
                new List<string> { "Manager" },
                new HashSet<int> { 104, 214, 314, 414, 112, 205, 305 }, // 7 משמרות
                14, 44, 14, true,
                new List<string> { "Branch1", "Branch3" }
            ));
            employees.Add(new Employee(
                16,
                "Evelyn Morris",
                new List<string> { "Waiter", "Chef" },
                new HashSet<int> { 105, 210, 310, 410, 510, 106, 211, 312 }, // 8 משמרות
                15, 45, 15, false,
                new List<string> { "Branch1", "Branch3" }
            ));
            employees.Add(new Employee(
                17,
                "Jacob Rivera",
                new List<string> { "Bartender", "Host", "Manager" },
                new HashSet<int> { 107, 207, 307, 407, 507, 108, 208, 308, 408 }, // 9 משמרות
                16, 46, 16, true,
                new List<string> { "Branch1", "Branch3" }
            ));
            employees.Add(new Employee(
                18,
                "Harper Cooper",
                new List<string> { "Waiter", "Chef", "Bartender" },
                new HashSet<int> { 109, 209, 309, 409, 509, 110, 210, 310, 410, 510 }, // 10 משמרות
                17, 47, 17, false,
                new List<string> { "Branch1", "Branch3" }
            ));
            employees.Add(new Employee(
                19,
                "William Bennett",
                new List<string> { "Chef", "Host", "Manager" },
                new HashSet<int> { 110, 210, 310, 410, 510, 111, 211, 311, 411, 511, 112 }, // 11 משמרות
                18, 48, 18, true,
                new List<string> { "Branch1", "Branch3" }
            ));
            employees.Add(new Employee(
                20,
                "Avery Powell",
                new List<string> { "Waiter", "Bartender", "Host", "Manager" },
                new HashSet<int> { 111, 211, 311, 411, 511, 112, 212, 312, 412, 512, 113, 213 }, // 12 משמרות
                19, 49, 19, false,
                new List<string> { "Branch1", "Branch3" }
            ));

            // עובדים 21-30 (Branches: "Branch2", "Branch4")
            employees.Add(new Employee(
                21,
                "Grace Brooks",
                new List<string> { "Waiter" },
                new HashSet<int> { 102, 202, 302 }, // 3 משמרות
                20, 50, 20, true,
                new List<string> { "Branch2", "Branch4" }
            ));
            employees.Add(new Employee(
                22,
                "Elijah Price",
                new List<string> { "Chef" },
                new HashSet<int> { 103, 203, 303, 403 }, // 4 משמרות
                21, 51, 21, false,
                new List<string> { "Branch2", "Branch4" }
            ));
            employees.Add(new Employee(
                23,
                "Chloe Bryant",
                new List<string> { "Bartender" },
                new HashSet<int> { 104, 204, 304, 404, 504 }, // 5 משמרות
                22, 52, 22, true,
                new List<string> { "Branch2", "Branch4" }
            ));
            employees.Add(new Employee(
                24,
                "Michael Hayes",
                new List<string> { "Host" },
                new HashSet<int> { 105, 205, 305, 405, 505, 106 }, // 6 משמרות
                23, 53, 23, false,
                new List<string> { "Branch2", "Branch4" }
            ));
            employees.Add(new Employee(
                25,
                "Scarlett Jenkins",
                new List<string> { "Manager" },
                new HashSet<int> { 106, 206, 306, 406, 506, 107, 207 }, // 7 משמרות
                24, 54, 24, true,
                new List<string> { "Branch2", "Branch4" }
            ));
            employees.Add(new Employee(
                26,
                "Daniel Perry",
                new List<string> { "Waiter", "Chef" },
                new HashSet<int> { 107, 207, 307, 407, 507, 108, 208, 308 }, // 8 משמרות
                25, 55, 25, false,
                new List<string> { "Branch2", "Branch4" }
            ));
            employees.Add(new Employee(
                27,
                "Victoria Russell",
                new List<string> { "Bartender", "Host", "Manager" },
                new HashSet<int> { 108, 208, 308, 408, 508, 109, 209, 309, 409 }, // 9 משמרות
                26, 56, 26, true,
                new List<string> { "Branch2", "Branch4" }
            ));
            employees.Add(new Employee(
                28,
                "Matthew Reed",
                new List<string> { "Waiter", "Chef", "Bartender" },
                new HashSet<int> { 109, 209, 309, 409, 509, 110, 210, 310, 410, 510 }, // 10 משמרות
                27, 57, 27, false,
                new List<string> { "Branch2", "Branch4" }
            ));
            employees.Add(new Employee(
                29,
                "Madison Griffin",
                new List<string> { "Chef", "Host", "Manager" },
                new HashSet<int> { 110, 210, 310, 410, 510, 111, 211, 311, 411, 511, 112 }, // 11 משמרות
                28, 58, 28, true,
                new List<string> { "Branch2", "Branch4" }
            ));
            employees.Add(new Employee(
                30,
                "James Foster",
                new List<string> { "Waiter", "Bartender", "Host", "Manager" },
                new HashSet<int> { 111, 211, 311, 411, 511, 112, 212, 312, 412, 512, 113, 213 }, // 12 משמרות
                29, 59, 29, false,
                new List<string> { "Branch2", "Branch4" }
            ));

            // עובדים 31-40 (Branches: "Branch3", "Branch5")
            employees.Add(new Employee(
                31,
                "Abigail Butler",
                new List<string> { "Waiter" },
                new HashSet<int> { 112, 212, 312 }, // 3 משמרות
                30, 60, 30, true,
                new List<string> { "Branch3", "Branch5" }
            ));
            employees.Add(new Employee(
                32,
                "Alexander Freeman",
                new List<string> { "Chef" },
                new HashSet<int> { 113, 213, 313, 413 }, // 4 משמרות
                31, 61, 31, false,
                new List<string> { "Branch3", "Branch5" }
            ));
            employees.Add(new Employee(
                33,
                "Elizabeth Simmons",
                new List<string> { "Bartender" },
                new HashSet<int> { 114, 214, 314, 414, 514 }, // 5 משמרות
                32, 62, 32, true,
                new List<string> { "Branch3", "Branch5" }
            ));
            employees.Add(new Employee(
                34,
                "Henry Coleman",
                new List<string> { "Host" },
                new HashSet<int> { 100, 200, 300, 400, 500, 101 }, // 6 משמרות
                33, 63, 33, false,
                new List<string> { "Branch3", "Branch5" }
            ));
            employees.Add(new Employee(
                35,
                "Sofia Gonzales",
                new List<string> { "Manager" },
                new HashSet<int> { 102, 202, 302, 402, 502, 103, 203 }, // 7 משמרות
                34, 64, 34, true,
                new List<string> { "Branch3", "Branch5" }
            ));
            employees.Add(new Employee(
                36,
                "Jackson Cruz",
                new List<string> { "Waiter", "Chef" },
                new HashSet<int> { 103, 203, 303, 403, 503, 104, 204, 304 }, // 8 משמרות
                35, 65, 35, false,
                new List<string> { "Branch3", "Branch5" }
            ));
            employees.Add(new Employee(
                37,
                "Aubrey Ortiz",
                new List<string> { "Bartender", "Host", "Manager" },
                new HashSet<int> { 104, 204, 304, 404, 504, 105, 205, 305, 405 }, // 9 משמרות
                36, 66, 36, true,
                new List<string> { "Branch3", "Branch5" }
            ));
            employees.Add(new Employee(
                38,
                "Sebastian Murphy",
                new List<string> { "Waiter", "Chef", "Bartender" },
                new HashSet<int> { 105, 205, 305, 405, 505, 106, 206, 306, 406, 506 }, // 10 משמרות
                37, 67, 37, false,
                new List<string> { "Branch3", "Branch5" }
            ));
            employees.Add(new Employee(
                39,
                "Aria Steele",
                new List<string> { "Chef", "Host", "Manager" },
                new HashSet<int> { 106, 206, 306, 406, 506, 107, 207, 307, 407, 507, 108 }, // 11 משמרות
                38, 68, 38, true,
                new List<string> { "Branch3", "Branch5" }
            ));
            employees.Add(new Employee(
                40,
                "Owen Reynolds",
                new List<string> { "Waiter", "Bartender", "Host", "Manager" },
                new HashSet<int> { 107, 207, 307, 407, 507, 108, 208, 308, 408, 508, 109, 209 }, // 12 משמרות
                39, 69, 39, false,
                new List<string> { "Branch3", "Branch5" }
            ));

            // עובדים 41-50 (Branches: "Branch1", "Branch4", "Branch5")
            employees.Add(new Employee(
                41,
                "Chloe Patterson",
                new List<string> { "Waiter" },
                new HashSet<int> { 108, 208, 308 }, // 3 משמרות
                40, 70, 40, true,
                new List<string> { "Branch1", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                42,
                "Gabriel Hughes",
                new List<string> { "Chef" },
                new HashSet<int> { 109, 209, 309, 409 }, // 4 משמרות
                41, 71, 41, false,
                new List<string> { "Branch1", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                43,
                "Victoria Chapman",
                new List<string> { "Bartender" },
                new HashSet<int> { 110, 210, 310, 410, 510 }, // 5 משמרות
                42, 72, 42, true,
                new List<string> { "Branch1", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                44,
                "Caleb Henderson",
                new List<string> { "Host" },
                new HashSet<int> { 111, 211, 311, 411, 511, 112 }, // 6 משמרות
                43, 73, 43, false,
                new List<string> { "Branch1", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                45,
                "Lily Harper",
                new List<string> { "Manager" },
                new HashSet<int> { 112, 212, 312, 412, 512, 113, 213 }, // 7 משמרות
                44, 74, 44, true,
                new List<string> { "Branch1", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                46,
                "Jayden Bishop",
                new List<string> { "Waiter", "Chef" },
                new HashSet<int> { 113, 213, 313, 413, 513, 114, 214, 314 }, // 8 משמרות
                45, 75, 45, false,
                new List<string> { "Branch1", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                47,
                "Zoey Manning",
                new List<string> { "Bartender", "Host", "Manager" },
                new HashSet<int> { 114, 214, 314, 414, 514, 100, 200, 300, 400 }, // 9 משמרות
                46, 76, 46, true,
                new List<string> { "Branch1", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                48,
                "Nathan Grant",
                new List<string> { "Waiter", "Chef", "Bartender" },
                new HashSet<int> { 100, 200, 300, 400, 500, 101, 201, 301, 401, 501 }, // 10 משמרות
                47, 77, 47, false,
                new List<string> { "Branch1", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                49,
                "Eleanor Lawson",
                new List<string> { "Chef", "Host", "Manager" },
                new HashSet<int> { 101, 201, 301, 401, 501, 102, 202, 302, 402, 502, 103 }, // 11 משמרות
                48, 78, 48, true,
                new List<string> { "Branch1", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                50,
                "Isaac Fletcher",
                new List<string> { "Waiter", "Bartender", "Host", "Manager" },
                new HashSet<int> { 102, 202, 302, 402, 502, 103, 203, 303, 403, 503, 104, 204 }, // 12 משמרות
                49, 79, 49, false,
                new List<string> { "Branch1", "Branch4", "Branch5" }
            ));

            // עובדים 51-60 (Branches: "Branch2", "Branch3")
            employees.Add(new Employee(
                51,
                "Penelope Burke",
                new List<string> { "Waiter" },
                new HashSet<int> { 103, 203, 303 }, // 3 משמרות
                50, 80, 50, true,
                new List<string> { "Branch2", "Branch3" }
            ));
            employees.Add(new Employee(
                52,
                "Samuel Sullivan",
                new List<string> { "Chef" },
                new HashSet<int> { 104, 204, 304, 404 }, // 4 משמרות
                51, 81, 51, false,
                new List<string> { "Branch2", "Branch3" }
            ));
            employees.Add(new Employee(
                53,
                "Hannah Palmer",
                new List<string> { "Bartender" },
                new HashSet<int> { 105, 205, 305, 405, 505 }, // 5 משמרות
                52, 82, 52, true,
                new List<string> { "Branch2", "Branch3" }
            ));
            employees.Add(new Employee(
                54,
                "Anthony Ramsey",
                new List<string> { "Host" },
                new HashSet<int> { 106, 206, 306, 406, 506, 107 }, // 6 משמרות
                53, 83, 53, false,
                new List<string> { "Branch2", "Branch3" }
            ));
            employees.Add(new Employee(
                55,
                "Addison Hart",
                new List<string> { "Manager" },
                new HashSet<int> { 107, 207, 307, 407, 507, 108, 208 }, // 7 משמרות
                54, 84, 54, true,
                new List<string> { "Branch2", "Branch3" }
            ));
            employees.Add(new Employee(
                56,
                "Christopher Gilbert",
                new List<string> { "Waiter", "Chef" },
                new HashSet<int> { 108, 208, 308, 408, 508, 109, 209, 309 }, // 8 משמרות
                55, 85, 55, false,
                new List<string> { "Branch2", "Branch3" }
            ));
            employees.Add(new Employee(
                57,
                "Lucy Delgado",
                new List<string> { "Bartender", "Host", "Manager" },
                new HashSet<int> { 109, 209, 309, 409, 509, 110, 210, 310, 410 }, // 9 משמרות
                56, 86, 56, true,
                new List<string> { "Branch2", "Branch3" }
            ));
            employees.Add(new Employee(
                58,
                "David Hicks",
                new List<string> { "Waiter", "Chef", "Bartender" },
                new HashSet<int> { 110, 210, 310, 410, 510, 111, 211, 311, 411, 511 }, // 10 משמרות
                57, 87, 57, false,
                new List<string> { "Branch2", "Branch3" }
            ));
            employees.Add(new Employee(
                59,
                "Stella Vaughn",
                new List<string> { "Chef", "Host", "Manager" },
                new HashSet<int> { 111, 211, 311, 411, 511, 112, 212, 312, 412, 512, 113 }, // 11 משמרות
                58, 88, 58, true,
                new List<string> { "Branch2", "Branch3" }
            ));
            employees.Add(new Employee(
                60,
                "Josephine Abbott",
                new List<string> { "Waiter", "Bartender", "Host", "Manager" },
                new HashSet<int> { 112, 212, 312, 412, 512, 113, 213, 313, 413, 513, 114, 214 }, // 12 משמרות
                59, 89, 59, false,
                new List<string> { "Branch2", "Branch3" }
            ));

            // עובדים 61-70 (Branches: "Branch1", "Branch2", "Branch3", "Branch4", "Branch5")
            employees.Add(new Employee(
                61,
                "Ariana Wells",
                new List<string> { "Waiter" },
                new HashSet<int> { 113, 213, 313 }, // 3 משמרות
                60, 90, 60, true,
                new List<string> { "Branch1", "Branch2", "Branch3", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                62,
                "Maverick Stone",
                new List<string> { "Chef" },
                new HashSet<int> { 114, 214, 314, 414 }, // 4 משמרות
                61, 91, 61, false,
                new List<string> { "Branch1", "Branch2", "Branch3", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                63,
                "Zoe Mercer",
                new List<string> { "Bartender" },
                new HashSet<int> { 100, 200, 300, 400, 500 }, // 5 משמרות
                62, 92, 62, true,
                new List<string> { "Branch1", "Branch2", "Branch3", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                64,
                "Leo Sutton",
                new List<string> { "Host" },
                new HashSet<int> { 101, 201, 301, 401, 501, 102 }, // 6 משמרות
                63, 93, 63, false,
                new List<string> { "Branch1", "Branch2", "Branch3", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                65,
                "Lillian Drake",
                new List<string> { "Manager" },
                new HashSet<int> { 202, 302, 402, 502, 103, 203 }, // 7 משמרות
                64, 94, 64, true,
                new List<string> { "Branch1", "Branch2", "Branch3", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                66,
                "Hudson Day",
                new List<string> { "Waiter", "Chef" },
                new HashSet<int> { 103, 203, 303, 403, 503, 104, 204, 304 }, // 8 משמרות
                65, 95, 65, false,
                new List<string> { "Branch1", "Branch2", "Branch3", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                67,
                "Samantha Keller",
                new List<string> { "Bartender", "Host", "Manager" },
                new HashSet<int> { 104, 204, 304, 404, 504, 105, 205, 305, 405 }, // 9 משמרות
                66, 96, 66, true,
                new List<string> { "Branch1", "Branch2", "Branch3", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                68,
                "Miles Donovan",
                new List<string> { "Waiter", "Chef", "Bartender" },
                new HashSet<int> { 105, 205, 305, 405, 505, 106, 206, 306, 406, 506 }, // 10 משמרות
                67, 97, 67, false,
                new List<string> { "Branch1", "Branch2", "Branch3", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                69,
                "Ruby McBride",
                new List<string> { "Chef", "Host", "Manager" },
                new HashSet<int> { 106, 206, 306, 406, 506, 107, 207, 307, 407, 507, 108 }, // 11 משמרות
                68, 98, 68, true,
                new List<string> { "Branch1", "Branch2", "Branch3", "Branch4", "Branch5" }
            ));
            employees.Add(new Employee(
                70,
                "Dylan Parrish",
                new List<string> { "Waiter", "Bartender", "Host", "Manager" },
                new HashSet<int> { 107, 207, 307, 407, 507, 108, 208, 308, 408, 508, 109, 209 }, // 12 משמרות
                69, 99, 69, false,
                new List<string> { "Branch1", "Branch2", "Branch3", "Branch4", "Branch5" }
            ));

            return employees;
        }
    }
}
=======

            List<Employee> employees = new List<Employee>();
            // ============ BLOCK 1: IDs 1..10 ============
            employees.Add(new Employee(
                    1,
                    "Alice Johnson",
                    new List<string> { "Waiter" },
                    new HashSet<int> { 1, 2 },
                    0,      // rating
                    30,     // hours
                    0,      // experience
                    true    // isActive (i=1 => odd => true)
                ));
            employees.Add(new Employee(
                2,
                "Bob Smith",
                new List<string> { "Chef" },
                new HashSet<int> { 1, 2, 3 },
                1,
                31,
                1,
                false
            ));
            employees.Add(new Employee(
                3,
                "Charlie Brown",
                new List<string> { "Bartender" },
                new HashSet<int> { 1, 2, 3, 4 },
                2,
                32,
                2,
                true
            ));
            employees.Add(new Employee(
                4,
                "Diana Prince",
                new List<string> { "Host" },
                new HashSet<int> { 1, 2, 3, 4, 5 },
                3,
                33,
                3,
                false
            ));
            employees.Add(new Employee(
                5,
                "Ethan Cohen",
                new List<string> { "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6 },
                4,
                34,
                4,
                true
            ));
            employees.Add(new Employee(
                6,
                "Fiona Martinez",
                new List<string> { "Waiter", "Chef" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7 },
                5,
                35,
                5,
                false
            ));
            employees.Add(new Employee(
                7,
                "George Davis",
                new List<string> { "Bartender", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8 },
                6,
                36,
                6,
                true
            ));
            employees.Add(new Employee(
                8,
                "Hannah Lee",
                new List<string> { "Waiter", "Chef", "Bartender" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                7,
                37,
                7,
                false
            ));
            employees.Add(new Employee(
                9,
                "Ian Miller",
                new List<string> { "Chef", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                8,
                38,
                8,
                true
            ));
            employees.Add(new Employee(
                10,
                "Jade Garcia",
                new List<string> { "Waiter", "Bartender", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
                9,
                39,
                9,
                false
            ));

            // ============ BLOCK 2: IDs 11..20 ============
            employees.Add(new Employee(
                11,
                "Alice Johnson",
                new List<string> { "Waiter" },
                new HashSet<int> { 1, 2 },
                10,
                40,
                10,
                true
            ));
            employees.Add(new Employee(
                12,
                "Bob Smith",
                new List<string> { "Chef" },
                new HashSet<int> { 1, 2, 3 },
                11,
                41,
                11,
                false
            ));
            employees.Add(new Employee(
                13,
                "Charlie Brown",
                new List<string> { "Bartender" },
                new HashSet<int> { 1, 2, 3, 4 },
                12,
                42,
                12,
                true
            ));
            employees.Add(new Employee(
                14,
                "Diana Prince",
                new List<string> { "Host" },
                new HashSet<int> { 1, 2, 3, 4, 5 },
                13,
                43,
                13,
                false
            ));
            employees.Add(new Employee(
                15,
                "Ethan Cohen",
                new List<string> { "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6 },
                14,
                44,
                14,
                true
            ));
            employees.Add(new Employee(
                16,
                "Fiona Martinez",
                new List<string> { "Waiter", "Chef" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7 },
                15,
                45,
                15,
                false
            ));
            employees.Add(new Employee(
                17,
                "George Davis",
                new List<string> { "Bartender", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8 },
                16,
                46,
                16,
                true
            ));
            employees.Add(new Employee(
                18,
                "Hannah Lee",
                new List<string> { "Waiter", "Chef", "Bartender" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                17,
                47,
                17,
                false
            ));
            employees.Add(new Employee(
                19,
                "Ian Miller",
                new List<string> { "Chef", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                18,
                48,
                18,
                true
            ));
            employees.Add(new Employee(
                20,
                "Jade Garcia",
                new List<string> { "Waiter", "Bartender", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
                19,
                49,
                19,
                false
            ));

            // ============ BLOCK 3: IDs 21..30 ============
            employees.Add(new Employee(
                21,
                "Alice Johnson",
                new List<string> { "Waiter" },
                new HashSet<int> { 1, 2 },
                20,
                50,
                20,
                true
            ));
            employees.Add(new Employee(
                22,
                "Bob Smith",
                new List<string> { "Chef" },
                new HashSet<int> { 1, 2, 3 },
                21,
                51,
                21,
                false
            ));
            employees.Add(new Employee(
                23,
                "Charlie Brown",
                new List<string> { "Bartender" },
                new HashSet<int> { 1, 2, 3, 4 },
                22,
                52,
                22,
                true
            ));
            employees.Add(new Employee(
                24,
                "Diana Prince",
                new List<string> { "Host" },
                new HashSet<int> { 1, 2, 3, 4, 5 },
                23,
                53,
                23,
                false
            ));
            employees.Add(new Employee(
                25,
                "Ethan Cohen",
                new List<string> { "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6 },
                24,
                54,
                24,
                true
            ));
            employees.Add(new Employee(
                26,
                "Fiona Martinez",
                new List<string> { "Waiter", "Chef" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7 },
                25,
                55,
                25,
                false
            ));
            employees.Add(new Employee(
                27,
                "George Davis",
                new List<string> { "Bartender", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8 },
                26,
                56,
                26,
                true
            ));
            employees.Add(new Employee(
                28,
                "Hannah Lee",
                new List<string> { "Waiter", "Chef", "Bartender" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                27,
                57,
                27,
                false
            ));
            employees.Add(new Employee(
                29,
                "Ian Miller",
                new List<string> { "Chef", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                28,
                58,
                28,
                true
            ));
            employees.Add(new Employee(
                30,
                "Jade Garcia",
                new List<string> { "Waiter", "Bartender", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
                29,
                59,
                29,
                false
            ));

            // ============ BLOCK 4: IDs 31..40 ============
            employees.Add(new Employee(
                31,
                "Alice Johnson",
                new List<string> { "Waiter" },
                new HashSet<int> { 1, 2 },
                30,
                60,
                30,
                true
            ));
            employees.Add(new Employee(
                32,
                "Bob Smith",
                new List<string> { "Chef" },
                new HashSet<int> { 1, 2, 3 },
                31,
                61,
                31,
                false
            ));
            employees.Add(new Employee(
                33,
                "Charlie Brown",
                new List<string> { "Bartender" },
                new HashSet<int> { 1, 2, 3, 4 },
                32,
                62,
                32,
                true
            ));
            employees.Add(new Employee(
                34,
                "Diana Prince",
                new List<string> { "Host" },
                new HashSet<int> { 1, 2, 3, 4, 5 },
                33,
                63,
                33,
                false
            ));
            employees.Add(new Employee(
                35,
                "Ethan Cohen",
                new List<string> { "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6 },
                34,
                64,
                34,
                true
            ));
            employees.Add(new Employee(
                36,
                "Fiona Martinez",
                new List<string> { "Waiter", "Chef" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7 },
                35,
                65,
                35,
                false
            ));
            employees.Add(new Employee(
                37,
                "George Davis",
                new List<string> { "Bartender", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8 },
                36,
                66,
                36,
                true
            ));
            employees.Add(new Employee(
                38,
                "Hannah Lee",
                new List<string> { "Waiter", "Chef", "Bartender" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                37,
                67,
                37,
                false
            ));
            employees.Add(new Employee(
                39,
                "Ian Miller",
                new List<string> { "Chef", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                38,
                68,
                38,
                true
            ));
            employees.Add(new Employee(
                40,
                "Jade Garcia",
                new List<string> { "Waiter", "Bartender", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
                39,
                69,
                39,
                false
            ));

            // ============ BLOCK 5: IDs 41..50 ============
            employees.Add(new Employee(
                41,
                "Alice Johnson",
                new List<string> { "Waiter" },
                new HashSet<int> { 1, 2 },
                40,
                70,
                40,
                true
            ));
            employees.Add(new Employee(
                42,
                "Bob Smith",
                new List<string> { "Chef" },
                new HashSet<int> { 1, 2, 3 },
                41,
                71,
                41,
                false
            ));
            employees.Add(new Employee(
                43,
                "Charlie Brown",
                new List<string> { "Bartender" },
                new HashSet<int> { 1, 2, 3, 4 },
                42,
                72,
                42,
                true
            ));
            employees.Add(new Employee(
                44,
                "Diana Prince",
                new List<string> { "Host" },
                new HashSet<int> { 1, 2, 3, 4, 5 },
                43,
                73,
                43,
                false
            ));
            employees.Add(new Employee(
                45,
                "Ethan Cohen",
                new List<string> { "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6 },
                44,
                74,
                44,
                true
            ));
            employees.Add(new Employee(
                46,
                "Fiona Martinez",
                new List<string> { "Waiter", "Chef" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7 },
                45,
                75,
                45,
                false
            ));
            employees.Add(new Employee(
                47,
                "George Davis",
                new List<string> { "Bartender", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8 },
                46,
                76,
                46,
                true
            ));
            employees.Add(new Employee(
                48,
                "Hannah Lee",
                new List<string> { "Waiter", "Chef", "Bartender" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                47,
                77,
                47,
                false
            ));
            employees.Add(new Employee(
                49,
                "Ian Miller",
                new List<string> { "Chef", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                48,
                78,
                48,
                true
            ));
            employees.Add(new Employee(
                50,
                "Jade Garcia",
                new List<string> { "Waiter", "Bartender", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
                49,
                79,
                49,
                false
            ));

            // ============ BLOCK 6: IDs 51..60 ============
            employees.Add(new Employee(
                51,
                "Alice Johnson",
                new List<string> { "Waiter" },
                new HashSet<int> { 1, 2 },
                50,
                80,
                50,
                true
            ));
            employees.Add(new Employee(
                52,
                "Bob Smith",
                new List<string> { "Chef" },
                new HashSet<int> { 1, 2, 3 },
                51,
                81,
                51,
                false
            ));
            employees.Add(new Employee(
                53,
                "Charlie Brown",
                new List<string> { "Bartender" },
                new HashSet<int> { 1, 2, 3, 4 },
                52,
                82,
                52,
                true
            ));
            employees.Add(new Employee(
                54,
                "Diana Prince",
                new List<string> { "Host" },
                new HashSet<int> { 1, 2, 3, 4, 5 },
                53,
                83,
                53,
                false
            ));
            employees.Add(new Employee(
                55,
                "Ethan Cohen",
                new List<string> { "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6 },
                54,
                84,
                54,
                true
            ));
            employees.Add(new Employee(
                56,
                "Fiona Martinez",
                new List<string> { "Waiter", "Chef" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7 },
                55,
                85,
                55,
                false
            ));
            employees.Add(new Employee(
                57,
                "George Davis",
                new List<string> { "Bartender", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8 },
                56,
                86,
                56,
                true
            ));
            employees.Add(new Employee(
                58,
                "Hannah Lee",
                new List<string> { "Waiter", "Chef", "Bartender" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                57,
                87,
                57,
                false
            ));
            employees.Add(new Employee(
                59,
                "Ian Miller",
                new List<string> { "Chef", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                58,
                88,
                58,
                true
            ));
            employees.Add(new Employee(
                60,
                "Jade Garcia",
                new List<string> { "Waiter", "Bartender", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
                59,
                89,
                59,
                false
            ));

            // ============ BLOCK 7: IDs 61..70 ============
            employees.Add(new Employee(
                61,
                "Alice Johnson",
                new List<string> { "Waiter" },
                new HashSet<int> { 1, 2 },
                60,
                90,
                60,
                true
            ));
            employees.Add(new Employee(
                62,
                "Bob Smith",
                new List<string> { "Chef" },
                new HashSet<int> { 1, 2, 3 },
                61,
                91,
                61,
                false
            ));
            employees.Add(new Employee(
                63,
                "Charlie Brown",
                new List<string> { "Bartender" },
                new HashSet<int> { 1, 2, 3, 4 },
                62,
                92,
                62,
                true
            ));
            employees.Add(new Employee(
                64,
                "Diana Prince",
                new List<string> { "Host" },
                new HashSet<int> { 1, 2, 3, 4, 5 },
                63,
                93,
                63,
                false
            ));
            employees.Add(new Employee(
                65,
                "Ethan Cohen",
                new List<string> { "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6 },
                64,
                94,
                64,
                true
            ));
            employees.Add(new Employee(
                66,
                "Fiona Martinez",
                new List<string> { "Waiter", "Chef" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7 },
                65,
                95,
                65,
                false
            ));
            employees.Add(new Employee(
                67,
                "George Davis",
                new List<string> { "Bartender", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8 },
                66,
                96,
                66,
                true
            ));
            employees.Add(new Employee(
                68,
                "Hannah Lee",
                new List<string> { "Waiter", "Chef", "Bartender" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                67,
                97,
                67,
                false
            ));
            employees.Add(new Employee(
                69,
                "Ian Miller",
                new List<string> { "Chef", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                68,
                98,
                68,
                true
            ));
            employees.Add(new Employee(
                70,
                "Jade Garcia",
                new List<string> { "Waiter", "Bartender", "Host", "Manager" },
                new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
                69,
                99,
                69,
                false
            ));
            return employees;

        }

    }

}

>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
