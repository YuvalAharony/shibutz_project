using Final;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Drawing;

namespace EmployeeSchedulingApp
{
    // דף ראשי למערכת ניהול המשמרות והעובדים
    public partial class MainPage : Form
    {
        // רשימת העובדים במערכת
        private List<Employee> EmployeesList;
        // רשימת הצגת העובדים בממשק
        private ListView employeesListView;
        // רשימת הסניפים במערכת
        private List<Branch> BranchesList;
        // רשימת הצגת הסניפים בממשק
        private ListView branchesListView;
        // שם המשתמש המחובר כעת
        private string currentUserName;
        // מופע של מחלקת העזר לבסיס הנתונים
        private static DataBaseHelper helper = new DataBaseHelper();
       
        // בנאי המחלקה - יוצר את העמוד הראשי
        // פרמטרים
        // UserName - שם המשתמש המחובר למערכת
        // ערך מוחזר: אין
        public MainPage(string UserName)
        {
            BranchesList = new List<Branch>();
            EmployeesList = new List<Employee>();
            currentUserName = UserName;
            helper.LoadDataForUser(UserName, BranchesList, EmployeesList);

            SetupUI();

            LoadBranches();
            this.CenterToScreen();

            LoadEmployees();
        }

        // הגדרת ממשק המשתמש של העמוד הראשי
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void SetupUI()
        {
            this.BackColor = Color.Tan;
            this.Text = "מסך ראשי - ניהול הרשת";
            this.Size = new System.Drawing.Size(800, 600);
            //כפתור יצרית משמרות
            Button generateShiftsButton = new Button()
            {
                Text = "צור משמרות",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(50, 20),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            generateShiftsButton.Click += (sender, e) => { Program.createSceduele(currentUserName); };

            this.Controls.Add(generateShiftsButton);

            //כפתור יצירת נתונים באופן רנדומלי
            Button generateRandomDataButton = new Button()
            {
                Text = "צור נתונים באופן רנדומלי",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(550, 80),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            generateRandomDataButton.Click += GenerateRandomDataButton_Click;
            this.Controls.Add(generateRandomDataButton);

            //כותרת
            Label titleLabel = new Label()
            {
                Text = "ניהול רשת המסעדות",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(300, 20),
                ForeColor = Color.Black,
            };

            //כפתור הוספת סניף
            Button addBranchButton = new Button()
            {
                Text = "הוסף סניף",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(50, 80),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            addBranchButton.Click += (sender, e) => { OpenAddBranchPage(); };

            //כפתור הוספת עובד
            Button addEmployeeButton = new Button()
            {
                Text = "הוסף עובד",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(220, 80),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            addEmployeeButton.Click += (sender, e) => { OpenAddEmployeePage(); };

            // כפתור עריכת משמרות סניף
            Button editBranchShiftsButton = new Button()
            {
                Text = "ערוך משמרות סניף",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(390, 80),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            editBranchShiftsButton.Click += EditBranchShiftsButton_Click;
            this.Controls.Add(editBranchShiftsButton);

            //  כפתורי מחיקה ליד רשימות הסניפים והעובדים
            Button deleteBranchButton = new Button()
            {
                Text = "מחק סניף",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(50, 460),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat

            };
            deleteBranchButton.Click += DeleteBranchButton_Click;
            this.Controls.Add(deleteBranchButton);

            Button deleteEmployeeButton = new Button()
            {
                Text = "מחק עובד",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(400, 460),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat

            };
            deleteEmployeeButton.Click += DeleteEmployeeButton_Click;
            this.Controls.Add(deleteEmployeeButton);

            //רשימת הסניפים של המשתמש
            branchesListView = new ListView()
            {
                Location = new System.Drawing.Point(50, 150),
                Size = new System.Drawing.Size(300, 300),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            branchesListView.Columns.Add("שם הסניף", 150);

            //רשימת העובדים של המשתמש
            employeesListView = new ListView()
            {
                Location = new System.Drawing.Point(400, 150),
                Size = new System.Drawing.Size(300, 300),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            employeesListView.Columns.Add("שם העובד", 150);

            //הוספת רכיבים לטופס
            this.Controls.Add(titleLabel);
            this.Controls.Add(addBranchButton);
            this.Controls.Add(addEmployeeButton);
            this.Controls.Add(branchesListView);
            this.Controls.Add(employeesListView);
            employeesListView.MouseDoubleClick += EmployeesListView_MouseDoubleClick;
        }

        // אירוע לחיצה כפולה על רשימת העובדים - פותח את דף עריכת העובד
        // פרמטרים
        // sender - האובייקט שהפעיל את האירוע
        // e - נתוני האירוע
        // ערך מוחזר: אין
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

                    // רענון רשימת העובדים
                    editPage.FormClosed += (s, args) =>
                    {
                        EmployeesList = helper.LoadUserEmployees(currentUserName);

                        // עדכון תצוגת העובדים
                        LoadEmployees();
                    };

                    editPage.Show();
                }
            }
        }

        // טעינת רשימת העובדים לתצוגה
        // פרמטרים: אין
        // ערך מוחזר: אין
        public void LoadEmployees()
        {
            employeesListView.Items.Clear(); // מנקה את הרשימה

            foreach (Employee emp in EmployeesList)
            {
                ListViewItem item = new ListViewItem(emp.Name);
                employeesListView.Items.Add(item);
            }
        }

        // טעינת רשימת הסניפים לתצוגה
        // פרמטרים: אין
        // ערך מוחזר: אין
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

        // אירוע לחיצה כפולה על רשימת הסניפים - פותח את דף הצגת המשמרות
        // פרמטרים
        // sender - האובייקט שהפעיל את האירוע
        // e - נתוני האירוע
        // ערך מוחזר: אין
        private void OpenViewShiftsPage(object sender, EventArgs e)
        {
            Branch selectedBranch = (Branch)branchesListView.SelectedItems[0].Tag;
            ViewShiftsPage viewShiftsPage = new ViewShiftsPage(selectedBranch);
            viewShiftsPage.Show();
        }

        // אירוע לחיצה על כפתור יצירת נתונים אקראיים
        // פרמטרים
        // sender - האובייקט שהפעיל את האירוע
        // e - נתוני האירוע
        // ערך מוחזר: אין
        private void GenerateRandomDataButton_Click(object sender, EventArgs e)
        {
            using (Form inputForm = new Form())
            {
                inputForm.Text = "יצירת נתונים אקראיים";
                inputForm.Size = new System.Drawing.Size(300, 200);
                inputForm.StartPosition = FormStartPosition.CenterParent;

                inputForm.RightToLeft = RightToLeft.Yes;
                inputForm.RightToLeftLayout = true;

                //  בניית הרכיבים
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

                //הוספת הרכיבים לטופס
                inputForm.Controls.AddRange(new Control[]
                {
                    branchLabel,
                    branchCount,
                    employeeLabel,
                    employeeCount,
                    okButton
                });
                inputForm.AcceptButton = okButton;

                //  הצגת הדיאלוג ועיבוד התוצאה 
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
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
                    catch (Exception ex) { 
                         
                    }
                  
                }
            }
        }

        // פתיחת דף הוספת סניף חדש
        // פרמטרים: אין
        // ערך מוחזר: אין
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

        // פתיחת דף הוספת עובד חדש
        // פרמטרים: אין
        // ערך מוחזר: אין
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

        // אתחול הרכיבים של הטופס
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void InitializeComponent()
        {
            this.SuspendLayout(); 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "MainPage";
            this.ResumeLayout(false);

        }

        // אירוע לחיצה על כפתור מחיקת סניף
        // פרמטרים
        // sender - האובייקט שהפעיל את האירוע
        // e - נתוני האירוע
        // ערך מוחזר: אין
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

        // אירוע לחיצה על כפתור מחיקת עובד
        // פרמטרים
        // sender - האובייקט שהפעיל את האירוע
        // e - נתוני האירוע
        // ערך מוחזר: אין
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
                    if (helper.DeleteEmployee(selectedEmployee.ID))
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

        // אירוע לחיצה על כפתור עריכת משמרות סניף
        // פרמטרים
        // sender - האובייקט שהפעיל את האירוע
        // e - נתוני האירוע
        // ערך מוחזר: אין
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