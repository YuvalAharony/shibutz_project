using Final;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace EmployeeSchedulingApp
{
    public partial class MainPage : Form
    {
        private List<Employee> EmployeesList;
        private ListView employeesListView;
        private List<Branch> BranchesList;
        private ListView branchesListView;
        private string currentUserName;
        private static DataBaseHelper helper=new DataBaseHelper();
        private static string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True;MultipleActiveResultSets=True";


   
        public MainPage(string UserName)
        {
            BranchesList = new List<Branch>();
            EmployeesList = new List<Employee>();
            currentUserName = UserName;
            helper.LoadDataForUser(UserName, BranchesList, EmployeesList);

            SetupUI();

            // Add these lines to populate the UI
            LoadBranches();
            LoadEmployees();
        }
        private void SetupUI()
        {
            this.Text = "מסך ראשי - ניהול הרשת";
            this.Size = new System.Drawing.Size(800, 600);
            Button generateShiftsButton = new Button()
            {
                Text = "צור משמרות",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(300, 500)
            };
            generateShiftsButton.Click += (sender, e) => { Program.createSceduele(currentUserName); };

            this.Controls.Add(generateShiftsButton);


            
            // Add this button to SetupUI in MainPage class
            Button generateRandomDataButton = new Button()
            {
                Text = "צור נתונים באופן רנדומלי",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(500, 500)
            };
            generateRandomDataButton.Click += GenerateRandomDataButton_Click;
            this.Controls.Add(generateRandomDataButton);

            Label titleLabel = new Label()
            {
                Text = "ניהול רשת המסעדות",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(300, 20)
            };

            Button addBranchButton = new Button()
            {
                Text = "הוסף סניף",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(50, 80)
            };
            addBranchButton.Click += (sender, e) => { OpenAddBranchPage(); };

            Button addEmployeeButton = new Button()
            {
                Text = "הוסף עובד",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(220, 80)
            };
            addEmployeeButton.Click += (sender, e) => { OpenAddEmployeePage(); };

            // כפתור חדש לעריכת משמרות סניף
            Button editBranchShiftsButton = new Button()
            {
                Text = "ערוך משמרות סניף",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(390, 80)
            };
            editBranchShiftsButton.Click += (sender, e) =>
            {
                if (branchesListView.SelectedItems.Count > 0)
                {
                    Branch selectedBranch = (Branch)branchesListView.SelectedItems[0].Tag;
                    EditBranchShift editPage = new EditBranchShift(selectedBranch);
                    editPage.Show();
                }
                else
                {
                    MessageBox.Show("אנא בחר סניף תחילה", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            this.Controls.Add(editBranchShiftsButton);

      

            branchesListView = new ListView()
            {
                Location = new System.Drawing.Point(50, 150),
                Size = new System.Drawing.Size(300, 300),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            branchesListView.Columns.Add("שם הסניף", 150);


            employeesListView = new ListView()
            {
                Location = new System.Drawing.Point(400, 150),
                Size = new System.Drawing.Size(300, 300),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            employeesListView.Columns.Add("שם העובד", 150);
            employeesListView.Columns.Add("תפקיד", 150);

            this.Controls.Add(titleLabel);
            this.Controls.Add(addBranchButton);
            this.Controls.Add(addEmployeeButton);
            this.Controls.Add(branchesListView);
            this.Controls.Add(employeesListView);
            employeesListView.MouseDoubleClick += EmployeesListView_MouseDoubleClick;
        }
        private void SetupEmployeesListViewDoubleClick()
        {
            // וודא שאירוע הלחיצה הכפולה מחובר
            employeesListView.MouseDoubleClick -= EmployeesListView_MouseDoubleClick; // למנוע חיבור כפול
            employeesListView.MouseDoubleClick += EmployeesListView_MouseDoubleClick;
        }

        private void EmployeesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (employeesListView.SelectedItems.Count > 0)
            {
                string selectedName = employeesListView.SelectedItems[0].Text;

                // מציאת העובד שנבחר לפי שמו
                Employee selectedEmployee = EmployeesList.FirstOrDefault(emp => emp.Name == selectedName);

                if (selectedEmployee != null)
                {
                    // פתיחת טופס עריכת העובד
                    EditEmployeePage editPage = new EditEmployeePage(selectedEmployee, currentUserName);

                    // הוספת אירוע שיתרחש כאשר הטופס נסגר - רענון רשימת העובדים
                    editPage.FormClosed += (s, args) =>
                    {
                        // רענון רשימת העובדים מבסיס הנתונים
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();  // Open the connection before using it
                            EmployeesList = helper.LoadUserEmployees(currentUserName, connection);
                        }  // Connection is automatically closed and disposed here

                        // עדכון תצוגת העובדים
                        LoadEmployees();
                    };

                    editPage.Show();
                }
            }
        }

        //private List<Branch> LoadUserBranches(string username)
        //{
        //    List<Branch> userBranches = new List<Branch>();

        //    try
        //    {
        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            connection.Open();

        //            // שאילתה שמביאה את הסניפים המשויכים למשתמש
        //            string query = @"
        //        SELECT b.BranchID, b.Name 
        //        FROM Branches b
        //        INNER JOIN UserBranches ub ON b.BranchID = ub.BranchID
        //        INNER JOIN Users u ON ub.UserID = u.UserID
        //        WHERE u.Username = @Username";

        //            using (SqlCommand command = new SqlCommand(query, connection))
        //            {
        //                command.Parameters.AddWithValue("@Username", username);

        //                using (SqlDataReader reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        Branch branch = new Branch
        //                        {
        //                            ID = reader.GetInt32(0),
        //                            Name = reader.GetString(1),
        //                        };

        //                        userBranches.Add(branch);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("אירעה שגיאה בטעינת הסניפים: " + ex.Message, "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }

        //    return userBranches;
        //}

        //private List<Employee> LoadUserEmployees(string username)
        //{
        //    List<Employee> userEmployees = new List<Employee>();

        //    try
        //    {
        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            connection.Open();

        //            // שאילתה שמביאה את העובדים המשויכים לסניפים של המשתמש
        //            string query = @"
        //             SELECT DISTINCT e.EmployeeID, e.Name, e.Phone, e.Email, e.HourlySalary, e.Rate, 
        //            e.IsMentor, e.AssignedHours
        //            FROM Employees e
        //            INNER JOIN EmployeeBranches eb ON e.EmployeeID = eb.EmployeeID
        //            INNER JOIN UserBranches ub ON eb.BranchID = ub.BranchID
        //            INNER JOIN Users u ON ub.UserID = u.UserID
        //            WHERE u.Username = @Username";

        //            using (SqlCommand command = new SqlCommand(query, connection))
        //            {
        //                command.Parameters.AddWithValue("@Username", username);

        //                using (SqlDataReader reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        Employee employee = new Employee
        //                        (Convert.ToInt32(reader["EmployeeID"]),
        //                        reader["Name"].ToString(),
        //                        new List<string>(),
        //                        new HashSet<int>(),
        //                        reader["Rate"] != DBNull.Value ? Convert.ToInt32(reader["Rate"]) : 0,
        //                        Convert.ToInt32(reader["HourlySalary"]),
        //                        Convert.ToInt32(reader["AssignedHours"]),
        //                        Convert.ToBoolean(reader["IsMentor"]),
        //                        null
        //                        );

        //                        // טעינת תפקידים של העובד
        //                        employee.roles = LoadEmployeeRoles(employee.ID, connection);

        //                        userEmployees.Add(employee);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("אירעה שגיאה בטעינת העובדים: " + ex.Message, "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }

        //    return userEmployees;
        //}

        //// פונקציה עזר לטעינת תפקידים של עובד
        //private List<string> LoadEmployeeRoles(int employeeId, SqlConnection existingConnection)
        //{
        //    List<string> roles = new List<string>();

        //    try
        //    {
        //        string query = @"SELECT  r.RoleName FROM Employees e join EmployeeRoles er on er.EmployeeID=e.EmployeeID join roles r on r.RoleID=er.RoleID WHERE e.EmployeeID = @EmployeeID";
        //        using (SqlCommand command = new SqlCommand(query, existingConnection))
        //        {
        //            command.Parameters.AddWithValue("@EmployeeID", employeeId);

        //            using (SqlDataReader reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    roles.Add(reader.GetString(0));
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // שגיאה בטעינת תפקידים - נחזיר רשימה ריקה
        //        Console.WriteLine("Error loading employee roles: " + ex.Message);
        //    }

        //    return roles;
        //}
        public void LoadEmployees()
        {
            employeesListView.Items.Clear(); // מנקה את הרשימה

            foreach (Employee emp in EmployeesList)
            {
                ListViewItem item = new ListViewItem(emp.Name);
                item.SubItems.Add(emp.roles.FirstOrDefault() ?? "לא מוגדר"); // מציג את התפקיד הראשון
                employeesListView.Items.Add(item);
            }
        }


        private void LoadBranches()
        {
            branchesListView.Items.Clear();

            foreach (Branch br in BranchesList)
            {
                ListViewItem item = new ListViewItem(br.Name);
                item.Tag = br;
                branchesListView.Items.Add(item);
            }

            branchesListView.MouseDoubleClick -= OpenViewShiftsPage;
            branchesListView.MouseDoubleClick += OpenViewShiftsPage;
        }
        private void OpenViewShiftsPage(object sender, EventArgs e)
        {

            Branch selectedBranch = (Branch)branchesListView.SelectedItems[0].Tag;
            ViewShiftsPage viewShiftsPage = new ViewShiftsPage(selectedBranch);
            viewShiftsPage.Show();

        }


        // Add this button to SetupUI in MainPage class
        private void GenerateRandomDataButton_Click(object sender, EventArgs e)
        {
            using (Form inputForm = new Form())
            {
                inputForm.Text = "Generate Random Data";
                inputForm.Size = new System.Drawing.Size(300, 200);
                inputForm.StartPosition = FormStartPosition.CenterParent;

                Label branchLabel = new Label() { Text = "Number of branches:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
                NumericUpDown branchCount = new NumericUpDown() { Location = new System.Drawing.Point(150, 20), Minimum = 1, Maximum = 10, Value = 2 };

                Label employeeLabel = new Label() { Text = "Total employees:", Location = new System.Drawing.Point(20, 50), AutoSize = true };
                NumericUpDown employeeCount = new NumericUpDown() { Location = new System.Drawing.Point(150, 50), Minimum = 5, Maximum = 100, Value = 20 };

                Button okButton = new Button() { Text = "OK", Location = new System.Drawing.Point(100, 100), DialogResult = DialogResult.OK };

                inputForm.Controls.AddRange(new Control[] { branchLabel, branchCount, employeeLabel, employeeCount, okButton });
                inputForm.AcceptButton = okButton;

                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        // Run random data generation
                        RandomDataGenerator.GenerateRandomData((int)branchCount.Value, (int)employeeCount.Value, currentUserName);

                        // Refresh the display
                        helper.LoadDataForUser(currentUserName, BranchesList, EmployeesList);
                        LoadBranches();
                        LoadEmployees();
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
                }
            }
        }
        private void OpenAddBranchPage()
        {
            // וודא שהמשתמש הנוכחי מועבר לטופס
            AddBranchPage addBranchPage = new AddBranchPage(currentUserName);

            addBranchPage.FormClosed += (sender, e) =>
            {
                if (addBranchPage.DialogResult == DialogResult.OK)
                {
                    // רענון רשימת הסניפים
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();  // Open the connection
                        BranchesList = helper.LoadUserBranches(currentUserName, connection);
                    }  // Connection is automatically closed here
                    LoadBranches();
                }
            };

            addBranchPage.ShowDialog();
        }
        private void OpenAddEmployeePage()
        {
            // העברת שם המשתמש הנוכחי לדף הוספת העובד
            AddEmployeePage addEmployee = new AddEmployeePage(currentUserName);

            // הוספת אירוע לרענון הרשימה לאחר סגירת הטופס
            addEmployee.FormClosed += (sender, e) =>
            {
                if (addEmployee.DialogResult == DialogResult.OK)
                {
                    // רענון רשימת העובדים על ידי טעינת הנתונים מחדש מהדאטאבייס
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();  // Open the connection before using it
                        EmployeesList = helper.LoadUserEmployees(currentUserName, connection);
                    }  // The connection will be automatically closed and disposed here

                    // עדכון הממשק המשתמש
                    LoadEmployees();
                    Console.WriteLine($"רשימת העובדים רועננה. נטענו {EmployeesList.Count} עובדים.");
                }
            };

            // שימוש ב-ShowDialog במקום Show כדי לחסום את המסך הראשי עד לסגירת הטופס
            addEmployee.ShowDialog();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainPage
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "MainPage";
            this.ResumeLayout(false);

        }

     
    }
}