using shibutz_project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EmployeeSchedulingApp
{
    public partial class AddBranchPage : Form
    {
        public AddBranchPage()
        {
            InitializeComponent();
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

            Label idLabel = new Label() { Text = "מזהה (ID):", Location = new System.Drawing.Point(50, 70) };
            TextBox idTextBox = new TextBox() { Location = new System.Drawing.Point(150, 70), Width = 180 };

            Label nameLabel = new Label() { Text = "שם הסניף:", Location = new System.Drawing.Point(50, 110) };
            TextBox nameTextBox = new TextBox() { Location = new System.Drawing.Point(150, 110), Width = 180 };

            Button saveButton = new Button()
            {
                Text = "שמור",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(150, 150)
            };
            saveButton.Click += (sender, e) => { SaveBranch(idTextBox.Text, nameTextBox.Text); };

            Button cancelButton = new Button()
            {
                Text = "ביטול",
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(150, 200)
            };
            cancelButton.Click += (sender, e) => { this.Close(); };

            this.Controls.Add(titleLabel);
            this.Controls.Add(idLabel);
            this.Controls.Add(idTextBox);
            this.Controls.Add(nameLabel);
            this.Controls.Add(nameTextBox);
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
        }


        private void SaveBranch(string branchId, string branchName)
        {
            if (string.IsNullOrWhiteSpace(branchId) || string.IsNullOrWhiteSpace(branchName))
            {
                MessageBox.Show("נא למלא את כל השדות.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // בדיקה אם מזהה הסניף כבר קיים
            if (Program.Branches.Any(b => b.ID == int.Parse(branchId)))
            {
                MessageBox.Show("מזהה הסניף כבר קיים במערכת.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // יצירת סניף חדש
            Branch newBranch = new Branch(
                int.Parse(branchId),
                branchName,
                new List<Shift>() // בהתחלה אין משמרות לסניף החדש
            );

            Program.Branches.Add(newBranch);

            MessageBox.Show($"הסניף {branchName} נוסף בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AddBranchPage
            // 
            this.ClientSize = new System.Drawing.Size(436, 348);
            this.Name = "AddBranchPage";
            this.ResumeLayout(false);

        }

      
    }
}