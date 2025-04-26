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
            editBranchShiftsButton.Click += EditBranchShiftsButton_Click;
            this.Controls.Add(editBranchShiftsButton);


            // הוספת כפתורי מחיקה ליד רשימות הסניפים והעובדים
            Button deleteBranchButton = new Button()
            {
                Text = "מחק סניף",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(50, 460)
            };
            deleteBranchButton.Click += DeleteBranchButton_Click;
            this.Controls.Add(deleteBranchButton);

            Button deleteEmployeeButton = new Button()
            {
                Text = "מחק עובד",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(400, 460)
            };
            deleteEmployeeButton.Click += DeleteEmployeeButton_Click;
            this.Controls.Add(deleteEmployeeButton);

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
                            EmployeesList = helper.LoadUserEmployees(currentUserName);
                        }  // Connection is automatically closed and disposed here

                        // עדכון תצוגת העובדים
                        LoadEmployees();
                    };

                    editPage.Show();
                }
            }
        }

        
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
                inputForm.Text = "יצירת נתונים אקראיים";
                inputForm.Size = new System.Drawing.Size(300, 200);
                inputForm.StartPosition = FormStartPosition.CenterParent;

                // --- 1) הפיכת כל הטופס ל־RTL ---
                inputForm.RightToLeft = RightToLeft.Yes;
                inputForm.RightToLeftLayout = true;

                // --- 2) בניית הרכיבים ---
                Label branchLabel = new Label()
                {
                    Text = "מספר סניפים:",
                    AutoSize = true,
                    Location = new System.Drawing.Point(20, 20)
                };
                branchLabel.RightToLeft = RightToLeft.Yes;

                NumericUpDown branchCount = new NumericUpDown()
                {
                    Location = new System.Drawing.Point(150, 20),
                    Minimum = 1,
                    Maximum = 10,
                    Value = 2,
                    Width = 60,
                    TextAlign = HorizontalAlignment.Right
                };
                branchCount.RightToLeft = RightToLeft.Yes;

                Label employeeLabel = new Label()
                {
                    Text = "מספר עובדים:",
                    AutoSize = true,
                    Location = new System.Drawing.Point(20, 60)
                };
                employeeLabel.RightToLeft = RightToLeft.Yes;

                NumericUpDown employeeCount = new NumericUpDown()
                {
                    Location = new System.Drawing.Point(150, 60),
                    Minimum = 5,
                    Maximum = 100,
                    Value = 20,
                    Width = 60,
                    TextAlign = HorizontalAlignment.Right
                };
                employeeCount.RightToLeft = RightToLeft.Yes;

                Button okButton = new Button()
                {
                    Text = "אישור",
                    Location = new System.Drawing.Point(100, 120),
                    DialogResult = DialogResult.OK
                };
                okButton.RightToLeft = RightToLeft.Yes;

                // --- 3) הוספת הרכיבים לטופס ---
                inputForm.Controls.AddRange(new Control[]
                {
            branchLabel,
            branchCount,
            employeeLabel,
            employeeCount,
            okButton
                });
                inputForm.AcceptButton = okButton;

                // --- 4) הצגת הדיאלוג ועיבוד התוצאה ---
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        // הרצת יצירת הנתונים האקראיים
                        RandomDataGenerator.GenerateRandomData(
                            (int)branchCount.Value,
                            (int)employeeCount.Value,
                            currentUserName
                        );

                        // רענון התצוגה
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
                    BranchesList = helper.LoadUserBranches(currentUserName);
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
                  
                    EmployeesList = helper.LoadUserEmployees(currentUserName);
                    LoadEmployees();
                    
                }
            };

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
        private void DeleteBranchButton_Click(object sender, EventArgs e)
        {
            if (branchesListView.SelectedItems.Count > 0)
            {
                Branch selectedBranch = (Branch)branchesListView.SelectedItems[0].Tag;

                DialogResult result = MessageBox.Show(
                    $"האם אתה בטוח שברצונך למחוק את הסניף '{selectedBranch.Name}'?\n" +
                    "פעולה זו תסיר גם את כל המשמרות של הסניף.",
                    "אישור מחיקה",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    // מחיקת הסניף
                    if (helper.DeleteBranch(selectedBranch.ID))
                    {
                        // מחיקה מהרשימה המקומית
                        BranchesList.RemoveAll(b => b.ID == selectedBranch.ID);
                        LoadBranches(); // רענון התצוגה
                        MessageBox.Show($"הסניף '{selectedBranch.Name}' נמחק בהצלחה.", "מחיקה הושלמה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("נא לבחור סניף למחיקה.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteEmployeeButton_Click(object sender, EventArgs e)
        {
            if (employeesListView.SelectedItems.Count > 0)
            {
                string selectedEmployeeName = employeesListView.SelectedItems[0].Text;
                Employee selectedEmployee = EmployeesList.FirstOrDefault(emp => emp.Name == selectedEmployeeName);

                if (selectedEmployee == null)
                {
                    MessageBox.Show("לא ניתן למצוא את פרטי העובד.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult result = MessageBox.Show(
                    $"האם אתה בטוח שברצונך למחוק את העובד '{selectedEmployee.Name}'?",
                    "אישור מחיקה",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    // מחיקת העובד
                    if (helper.DeleteEmployee(selectedEmployee.ID));
                    {
                        // מחיקה מהרשימה המקומית
                        EmployeesList.RemoveAll(emp => emp.ID == selectedEmployee.ID);
                        LoadEmployees(); // רענון התצוגה
                        MessageBox.Show($"העובד '{selectedEmployee.Name}' נמחק בהצלחה.", "מחיקה הושלמה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("נא לבחור עובד למחיקה.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void EditBranchShiftsButton_Click(object sender, EventArgs e)
        {
            if (branchesListView.SelectedItems.Count > 0)
            {
                var selectedBranch = (Branch)branchesListView.SelectedItems[0].Tag;
                var editPage = new EditBranchShift(selectedBranch);
                editPage.Show();
            }
            else
            {
                MessageBox.Show(
                    "אנא בחר סניף תחילה",
                    "שגיאה",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

    }

}