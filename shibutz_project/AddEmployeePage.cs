using shibutz_project;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EmployeeSchedulingApp
{
    public partial class AddEmployeePage : Form
    {
        public AddEmployeePage()
        {
            InitializeComponent();
            SetupUI();
        }


        private void SetupUI()
        {
            this.Text = "הוספת עובד חדש";
            this.Size = new System.Drawing.Size(400, 550);

            Label titleLabel = new Label()
            {
                Text = "הוספת עובד חדש",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(120, 20)
            };

            Label nameLabel = new Label() { Text = "שם העובד:", Location = new System.Drawing.Point(50, 70) };
            TextBox nameTextBox = new TextBox() { Location = new System.Drawing.Point(150, 70), Width = 180 };

            Label idLabel = new Label() { Text = "מזהה (ID):", Location = new System.Drawing.Point(50, 110) };
            TextBox idTextBox = new TextBox() { Location = new System.Drawing.Point(150, 110), Width = 180 };

            Label roleLabel = new Label() { Text = "תפקיד:", Location = new System.Drawing.Point(50, 150) };
            ComboBox roleComboBox = new ComboBox()
            {
                Location = new System.Drawing.Point(150, 150),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            roleComboBox.Items.AddRange(new string[] { "מלצר", "טבח", "ברמן", "מנהל" });

            Label salaryLabel = new Label() { Text = "שכר שעתי:", Location = new System.Drawing.Point(50, 190) };
            TextBox salaryTextBox = new TextBox() { Location = new System.Drawing.Point(150, 190), Width = 180 };

            Label shiftsLabel = new Label() { Text = "משמרות (מזהים מופרדים בפסיקים):", Location = new System.Drawing.Point(50, 230) };
            TextBox shiftsTextBox = new TextBox() { Location = new System.Drawing.Point(50, 260), Width = 280 };

            Label rateLabel = new Label() { Text = "ציון עובד:", Location = new System.Drawing.Point(50, 300) };
            TextBox rateTextBox = new TextBox() { Location = new System.Drawing.Point(150, 300), Width = 180 };

            CheckBox isExperiencedCheckBox = new CheckBox()
            {
                Text = "האם עובד מנוסה?",
                Location = new System.Drawing.Point(150, 340)
            };

            Button saveButton = new Button()
            {
                Text = "שמור",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(150, 380)
            };
            saveButton.Click += (sender, e) => { SaveEmployee(nameTextBox.Text, idTextBox.Text, rateTextBox.Text, roleComboBox.SelectedItem?.ToString(), salaryTextBox.Text, shiftsTextBox.Text, isExperiencedCheckBox.Checked); };

            Button cancelButton = new Button()
            {
                Text = "ביטול",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(150, 430)
            };
            cancelButton.Click += (sender, e) => { this.Close(); };

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
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
        }

        private void SaveEmployee(string name, string id, string rate, string role, string salary, string shifts, bool isExperienced)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(salary))
            {
                MessageBox.Show("נא למלא את כל השדות.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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