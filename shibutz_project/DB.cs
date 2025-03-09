using shibutz_project;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace shibutz_project
{
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
            return branches;
        }

        public static List<Employee> addEmployees()
        {

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

