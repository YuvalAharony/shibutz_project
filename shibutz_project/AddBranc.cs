using System;
using System.Windows.Forms;

namespace EmployeeSchedulingApp
{
    public partial class AddBranchPage : Form
    {
        public AddBranchPage()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "הוספת סניף חדש";
            this.Size = new System.Drawing.Size(400, 300);

            Label titleLabel = new Label()
            {
                Text = "הוספת סניף חדש",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(120, 20)
            };

            Label nameLabel = new Label() { Text = "שם הסניף:", Location = new System.Drawing.Point(50, 70) };
            TextBox nameTextBox = new TextBox() { Location = new System.Drawing.Point(150, 70), Width = 180 };

            Label locationLabel = new Label() { Text = "מיקום:", Location = new System.Drawing.Point(50, 110) };
            TextBox locationTextBox = new TextBox() { Location = new System.Drawing.Point(150, 110), Width = 180 };

            Button saveButton = new Button()
            {
                Text = "שמור",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(150, 160)
            };
            saveButton.Click += (sender, e) => { SaveBranch(nameTextBox.Text, locationTextBox.Text); };

            Button cancelButton = new Button()
            {
                Text = "ביטול",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(150, 210)
            };
            cancelButton.Click += (sender, e) => { this.Close(); };

            this.Controls.Add(titleLabel);
            this.Controls.Add(nameLabel);
            this.Controls.Add(nameTextBox);
            this.Controls.Add(locationLabel);
            this.Controls.Add(locationTextBox);
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
        }

        private void SaveBranch(string name, string location)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(location))
            {
                MessageBox.Show("נא למלא את כל השדות.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show($"הסניף {name} נוסף בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}