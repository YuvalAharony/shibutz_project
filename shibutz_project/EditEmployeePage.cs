using shibutz_project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EmployeeSchedulingApp
{
    public partial class EditEmployeePage : Form
    {
        private Employee selectedEmployee;
        private TextBox nameTextBox, idTextBox, rateTextBox, salaryTextBox, shiftsTextBox;
        private ComboBox roleComboBox;
        private CheckBox isExperiencedCheckBox;
        private Button saveButton, cancelButton;

        public EditEmployeePage(Employee employee)
        {
            selectedEmployee = employee;
            SetupUI();
            LoadEmployeeData();
        }

        private void SetupUI()
        {
            this.Text = "עריכת עובד";
            this.Size = new System.Drawing.Size(400, 550);

            Label titleLabel = new Label()
            {
                Text = "עריכת פרטי עובד",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(120, 20)
            };

            Label nameLabel = new Label() { Text = "שם העובד:", Location = new System.Drawing.Point(50, 70) };
            nameTextBox = new TextBox() { Location = new System.Drawing.Point(150, 70), Width = 180 };

            Label idLabel = new Label() { Text = "מזהה (ID):", Location = new System.Drawing.Point(50, 110) };
            idTextBox = new TextBox() { Location = new System.Drawing.Point(150, 110), Width = 180, ReadOnly = true };

            Label roleLabel = new Label() { Text = "תפקיד:", Location = new System.Drawing.Point(50, 150) };
            roleComboBox = new ComboBox()
            {
                Location = new System.Drawing.Point(150, 150),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            roleComboBox.Items.AddRange(new string[] { "Waiter", "Chef", "Bartender", "Host", "Manager" });

            Label salaryLabel = new Label() { Text = "שכר שעתי:", Location = new System.Drawing.Point(50, 190) };
            salaryTextBox = new TextBox() { Location = new System.Drawing.Point(150, 190), Width = 180 };

            Label shiftsLabel = new Label() { Text = "משמרות (מזהים מופרדים בפסיקים):", Location = new System.Drawing.Point(50, 230) };
            shiftsTextBox = new TextBox() { Location = new System.Drawing.Point(50, 260), Width = 280 };

            Label rateLabel = new Label() { Text = "ציון עובד:", Location = new System.Drawing.Point(50, 300) };
            rateTextBox = new TextBox() { Location = new System.Drawing.Point(150, 300), Width = 180 };

            isExperiencedCheckBox = new CheckBox()
            {
                Text = "האם עובד מנוסה?",
                Location = new System.Drawing.Point(150, 340)
            };

            saveButton = new Button()
            {
                Text = "שמור",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(150, 380)
            };
            saveButton.Click += SaveEmployeeChanges;

            cancelButton = new Button()
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

        private void LoadEmployeeData()
        {
            nameTextBox.Text = selectedEmployee.Name;
            idTextBox.Text = selectedEmployee.ID.ToString();
            roleComboBox.SelectedItem = selectedEmployee.Roles.FirstOrDefault();
            salaryTextBox.Text = selectedEmployee.HourlySalary.ToString();
            shiftsTextBox.Text = string.Join(",", selectedEmployee.requestedShifts);
            rateTextBox.Text = selectedEmployee.Rate.ToString();
            isExperiencedCheckBox.Checked = selectedEmployee.isMentor;
        }

        private void SaveEmployeeChanges(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) || string.IsNullOrWhiteSpace(roleComboBox.Text) ||
                string.IsNullOrWhiteSpace(salaryTextBox.Text))
            {
                MessageBox.Show("נא למלא את כל השדות.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            selectedEmployee.Name = nameTextBox.Text;
            selectedEmployee.Roles = new List<string> { roleComboBox.SelectedItem.ToString() };
            selectedEmployee.HourlySalary = int.Parse(salaryTextBox.Text);
            selectedEmployee.Rate = double.Parse(rateTextBox.Text);
            selectedEmployee.isMentor = isExperiencedCheckBox.Checked;

            selectedEmployee.requestedShifts = new HashSet<int>();
            foreach (var shift in shiftsTextBox.Text.Split(','))
            {
                if (int.TryParse(shift.Trim(), out int shiftId))
                {
                    selectedEmployee.requestedShifts.Add(shiftId);
                }
            }

            MessageBox.Show($"פרטי העובד {selectedEmployee.Name} עודכנו בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
