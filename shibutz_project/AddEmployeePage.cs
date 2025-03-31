using shibutz_project;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EmployeeSchedulingApp
{
    public partial class AddEmployeePage : Form
    {
<<<<<<< HEAD
        // במקום TextBox, נשתמש ב-CheckedListBox
        private CheckedListBox branchesCheckedListBox;

=======
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
        public AddEmployeePage()
        {
            InitializeComponent();
            SetupUI();
        }

<<<<<<< HEAD
        private void SetupUI()
        {
            this.Text = "הוספת עובד חדש";
            this.Size = new System.Drawing.Size(400, 600);
=======

        private void SetupUI()
        {
            this.Text = "הוספת עובד חדש";
            this.Size = new System.Drawing.Size(400, 550);
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74

            Label titleLabel = new Label()
            {
                Text = "הוספת עובד חדש",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(120, 20)
            };

<<<<<<< HEAD
            // שם העובד
            Label nameLabel = new Label() { Text = "שם העובד:", Location = new System.Drawing.Point(50, 70) };
            TextBox nameTextBox = new TextBox() { Location = new System.Drawing.Point(150, 70), Width = 180 };

            // מזהה
            Label idLabel = new Label() { Text = "מזהה (ID):", Location = new System.Drawing.Point(50, 110) };
            TextBox idTextBox = new TextBox() { Location = new System.Drawing.Point(150, 110), Width = 180 };

            // תפקיד (ComboBox)
=======
            Label nameLabel = new Label() { Text = "שם העובד:", Location = new System.Drawing.Point(50, 70) };
            TextBox nameTextBox = new TextBox() { Location = new System.Drawing.Point(150, 70), Width = 180 };

            Label idLabel = new Label() { Text = "מזהה (ID):", Location = new System.Drawing.Point(50, 110) };
            TextBox idTextBox = new TextBox() { Location = new System.Drawing.Point(150, 110), Width = 180 };

>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
            Label roleLabel = new Label() { Text = "תפקיד:", Location = new System.Drawing.Point(50, 150) };
            ComboBox roleComboBox = new ComboBox()
            {
                Location = new System.Drawing.Point(150, 150),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            roleComboBox.Items.AddRange(new string[] { "מלצר", "טבח", "ברמן", "מנהל" });

<<<<<<< HEAD
            // שכר שעתי
            Label salaryLabel = new Label() { Text = "שכר שעתי:", Location = new System.Drawing.Point(50, 190) };
            TextBox salaryTextBox = new TextBox() { Location = new System.Drawing.Point(150, 190), Width = 180 };

            // משמרות (כמו קודם, מופרדות בפסיקים)
            Label shiftsLabel = new Label() { Text = "משמרות (מזהים מופרדים בפסיקים):", Location = new System.Drawing.Point(50, 230) };
            TextBox shiftsTextBox = new TextBox() { Location = new System.Drawing.Point(50, 260), Width = 280 };

            // ציון עובד
            Label rateLabel = new Label() { Text = "ציון עובד:", Location = new System.Drawing.Point(50, 300) };
            TextBox rateTextBox = new TextBox() { Location = new System.Drawing.Point(150, 300), Width = 180 };

            // האם מנוסה (CheckBox)
=======
            Label salaryLabel = new Label() { Text = "שכר שעתי:", Location = new System.Drawing.Point(50, 190) };
            TextBox salaryTextBox = new TextBox() { Location = new System.Drawing.Point(150, 190), Width = 180 };

            Label shiftsLabel = new Label() { Text = "משמרות (מזהים מופרדים בפסיקים):", Location = new System.Drawing.Point(50, 230) };
            TextBox shiftsTextBox = new TextBox() { Location = new System.Drawing.Point(50, 260), Width = 280 };

            Label rateLabel = new Label() { Text = "ציון עובד:", Location = new System.Drawing.Point(50, 300) };
            TextBox rateTextBox = new TextBox() { Location = new System.Drawing.Point(150, 300), Width = 180 };

>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
            CheckBox isExperiencedCheckBox = new CheckBox()
            {
                Text = "האם עובד מנוסה?",
                Location = new System.Drawing.Point(150, 340)
            };

<<<<<<< HEAD
            // **בחירת סניפים (CheckedListBox)**
            Label branchesLabel = new Label() { Text = "בחר סניפים:", Location = new System.Drawing.Point(50, 380) };
            branchesCheckedListBox = new CheckedListBox()
            {
                Location = new System.Drawing.Point(50, 410),
                Width = 280,
                Height = 80, // נקבע גובה שיציג מספר אופציות
                CheckOnClick = true // מאפשר סימון/ביטול ע"י לחיצה
            };
            // הוספת הסניפים הרצויים
            branchesCheckedListBox.Items.AddRange(new string[] {
                "Branch1",
                "Branch2",
                "Branch3",
                "Branch4",
                "Branch5"
            });

            // כפתור שמירה
=======
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
            Button saveButton = new Button()
            {
                Text = "שמור",
                Size = new System.Drawing.Size(100, 40),
<<<<<<< HEAD
                Location = new System.Drawing.Point(150, 500)
            };
            saveButton.Click += (sender, e) => {
                SaveEmployee(
                    nameTextBox.Text,
                    idTextBox.Text,
                    rateTextBox.Text,
                    roleComboBox.SelectedItem?.ToString(),
                    salaryTextBox.Text,
                    shiftsTextBox.Text,
                    isExperiencedCheckBox.Checked
                );
            };

            // כפתור ביטול
=======
                Location = new System.Drawing.Point(150, 380)
            };
            saveButton.Click += (sender, e) => { SaveEmployee(nameTextBox.Text, idTextBox.Text, rateTextBox.Text, roleComboBox.SelectedItem?.ToString(), salaryTextBox.Text, shiftsTextBox.Text, isExperiencedCheckBox.Checked); };

>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
            Button cancelButton = new Button()
            {
                Text = "ביטול",
                Size = new System.Drawing.Size(100, 40),
<<<<<<< HEAD
                Location = new System.Drawing.Point(150, 550)
            };
            cancelButton.Click += (sender, e) => { this.Close(); };

            // הוספת כל הפקדים לטופס
=======
                Location = new System.Drawing.Point(150, 430)
            };
            cancelButton.Click += (sender, e) => { this.Close(); };

>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
            this.Controls.Add(titleLabel);
            this.Controls.Add(nameLabel);
            this.Controls.Add(nameTextBox);
            this.Controls.Add(idLabel);
            this.Controls.Add(idTextBox);
            this.Controls.Add(roleLabel);
            this.Controls.Add(roleComboBox);
            this.Controls.Add(salaryLabel);
            this.Controls.Add(salaryTextBox);
            this.Controls.Add(shiftsLabel);
            this.Controls.Add(shiftsTextBox);
            this.Controls.Add(rateLabel);
            this.Controls.Add(rateTextBox);
            this.Controls.Add(isExperiencedCheckBox);
<<<<<<< HEAD
            this.Controls.Add(branchesLabel);
            this.Controls.Add(branchesCheckedListBox);
=======
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
        }

<<<<<<< HEAD
        private void SaveEmployee(
            string name,
            string id,
            string rate,
            string role,
            string salary,
            string shifts,
            bool isExperienced)
        {
            // בדיקת שדות ריקים
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(id) ||
                string.IsNullOrWhiteSpace(role) ||
                string.IsNullOrWhiteSpace(salary))
=======
        private void SaveEmployee(string name, string id, string rate, string role, string salary, string shifts, bool isExperienced)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(salary))
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
            {
                MessageBox.Show("נא למלא את כל השדות.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

<<<<<<< HEAD
            // המרת המשמרות ל-HashSet<int>
=======
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
            HashSet<int> requestedShifts = new HashSet<int>();
            if (!string.IsNullOrWhiteSpace(shifts))
            {
                foreach (var shift in shifts.Split(','))
                {
                    if (int.TryParse(shift.Trim(), out int shiftId))
                    {
                        requestedShifts.Add(shiftId);
                    }
                }
            }

<<<<<<< HEAD
            // איסוף הפריטים המסומנים מה-CheckedListBox אל רשימת מחרוזות
            List<string> branchList = new List<string>();
            foreach (var item in branchesCheckedListBox.CheckedItems)
            {
                branchList.Add(item.ToString());
            }

            try
            {
                // יצירת מופע של עובד עם הבנאי המעודכן (הכולל Branches)
                Employee newEmployee = new Employee(
                    int.Parse(id),
                    name,
                    new List<string> { role },
                    requestedShifts,
                    double.Parse(rate),
                    int.Parse(salary),
                    7,                // לדוגמה, assignedHours קבוע
                    isExperienced,    // true/false לפי CheckBox
                    branchList        // כאן מגיעה רשימת הסניפים שנבחרו
                );

                // הוספה לרשימת העובדים הכללית
                Program.Employees.Add(newEmployee);

                MessageBox.Show($"העובד {name} נוסף בהצלחה!",
                                "הצלחה",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"אירעה שגיאה בהוספת העובד: {ex.Message}",
                                "שגיאה",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

     
    }
}
=======
            Employee newEmployee = new Employee(
                int.Parse(id),
                name,
                new List<string> { role },
                requestedShifts,
                double.Parse(rate),
                int.Parse(salary),
                7,
                isExperienced
            );

            Program.Employees.Add(newEmployee);

            MessageBox.Show($"העובד {name} נוסף בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
          

            this.Close();
        }
    }
}
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
