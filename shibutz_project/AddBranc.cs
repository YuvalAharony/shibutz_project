<<<<<<< HEAD
﻿using shibutz_project;
using System;
using System.Collections.Generic;
using System.Linq;
=======
﻿using System;
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
using System.Windows.Forms;

namespace EmployeeSchedulingApp
{
    public partial class AddBranchPage : Form
    {
        public AddBranchPage()
        {
<<<<<<< HEAD
            InitializeComponent();
=======
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
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

<<<<<<< HEAD
            Label idLabel = new Label() { Text = "מזהה (ID):", Location = new System.Drawing.Point(50, 70) };
            TextBox idTextBox = new TextBox() { Location = new System.Drawing.Point(150, 70), Width = 180 };

            Label nameLabel = new Label() { Text = "שם הסניף:", Location = new System.Drawing.Point(50, 110) };
            TextBox nameTextBox = new TextBox() { Location = new System.Drawing.Point(150, 110), Width = 180 };
=======
            Label nameLabel = new Label() { Text = "שם הסניף:", Location = new System.Drawing.Point(50, 70) };
            TextBox nameTextBox = new TextBox() { Location = new System.Drawing.Point(150, 70), Width = 180 };

            Label locationLabel = new Label() { Text = "מיקום:", Location = new System.Drawing.Point(50, 110) };
            TextBox locationTextBox = new TextBox() { Location = new System.Drawing.Point(150, 110), Width = 180 };
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74

            Button saveButton = new Button()
            {
                Text = "שמור",
                Size = new System.Drawing.Size(100, 40),
<<<<<<< HEAD
                Location = new System.Drawing.Point(150, 150)
            };
            saveButton.Click += (sender, e) => { SaveBranch(idTextBox.Text, nameTextBox.Text); };
=======
                Location = new System.Drawing.Point(150, 160)
            };
            saveButton.Click += (sender, e) => { SaveBranch(nameTextBox.Text, locationTextBox.Text); };
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74

            Button cancelButton = new Button()
            {
                Text = "ביטול",
                Size = new System.Drawing.Size(100, 40),
<<<<<<< HEAD
                Location = new System.Drawing.Point(150, 200)
=======
                Location = new System.Drawing.Point(150, 210)
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
            };
            cancelButton.Click += (sender, e) => { this.Close(); };

            this.Controls.Add(titleLabel);
<<<<<<< HEAD
            this.Controls.Add(idLabel);
            this.Controls.Add(idTextBox);
            this.Controls.Add(nameLabel);
            this.Controls.Add(nameTextBox);
=======
            this.Controls.Add(nameLabel);
            this.Controls.Add(nameTextBox);
            this.Controls.Add(locationLabel);
            this.Controls.Add(locationTextBox);
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
        }

<<<<<<< HEAD

        private void SaveBranch(string branchId, string branchName)
        {
            if (string.IsNullOrWhiteSpace(branchId) || string.IsNullOrWhiteSpace(branchName))
=======
        private void SaveBranch(string name, string location)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(location))
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
            {
                MessageBox.Show("נא למלא את כל השדות.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

<<<<<<< HEAD
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

      
=======
            MessageBox.Show($"הסניף {name} נוסף בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
    }
}