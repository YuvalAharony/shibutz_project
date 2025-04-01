using Final;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Final
{
    public partial class AddEmployeePage : Form
    {
        // במקום TextBox, נשתמש ב-CheckedListBox
        private CheckedListBox branchesCheckedListBox;

        public AddEmployeePage()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "הוספת עובד חדש";
            this.Size = new System.Drawing.Size(400, 600);

            Label titleLabel = new Label()
            {
                Text = "הוספת עובד חדש",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(120, 20)
            };

            // שם העובד
            Label nameLabel = new Label() { Text = "שם העובד:", Location = new System.Drawing.Point(50, 70) };
            TextBox nameTextBox = new TextBox() { Location = new System.Drawing.Point(150, 70), Width = 180 };

            // מזהה
            Label idLabel = new Label() { Text = "מזהה (ID):", Location = new System.Drawing.Point(50, 110) };
            TextBox idTextBox = new TextBox() { Location = new System.Drawing.Point(150, 110), Width = 180 };

            // תפקיד (ComboBox)
            Label roleLabel = new Label() { Text = "תפקיד:", Location = new System.Drawing.Point(50, 150) };
            ComboBox roleComboBox = new ComboBox()
            {
                Location = new System.Drawing.Point(150, 150),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            roleComboBox.Items.AddRange(new string[] { "מלצר", "טבח", "ברמן", "מנהל" });

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
            CheckBox isExperiencedCheckBox = new CheckBox()
            {
                Text = "האם עובד מנוסה?",
                Location = new System.Drawing.Point(150, 340)
            };

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
            Button saveButton = new Button()
            {
                Text = "שמור",
                Size = new System.Drawing.Size(100, 40),
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
            Button cancelButton = new Button()
            {
                Text = "ביטול",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(150, 550)
            };
            cancelButton.Click += (sender, e) => { this.Close(); };

            // הוספת כל הפקדים לטופס
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
            this.Controls.Add(branchesLabel);
            this.Controls.Add(branchesCheckedListBox);
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
        }

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
            {
                MessageBox.Show("נא למלא את כל השדות.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // המרת המשמרות ל-HashSet<int>
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