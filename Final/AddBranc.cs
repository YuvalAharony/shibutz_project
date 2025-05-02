using Final;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EmployeeSchedulingApp
{
    public partial class AddBranchPage : Form
    {
        private static string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True";
        private string currentUserName;
        private static DataBaseHelper helper = new DataBaseHelper();


        public AddBranchPage(string userName = null)
        {
            InitializeComponent();
            currentUserName = userName;
            SetupUI();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AddBranchPage
            // 
            this.ClientSize = new System.Drawing.Size(400, 450);
            this.Name = "AddBranchPage";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Text = "הוספת סניף חדש";
            this.ResumeLayout(false);

        }

        private void SetupUI()
        {
            // כותרת
            Label titleLabel = new Label
            {
                Text = "הוספת סניף חדש",
                Font = new Font("Arial", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(120, 20)
            };
            this.Controls.Add(titleLabel);

            // שם הסניף
            Label nameLabel = new Label
            {
                Text = "שם הסניף:",
                Location = new Point(50, 70),
                AutoSize = true
            };
            TextBox nameTextBox = new TextBox
            {
                Name = "nameTextBox",
                Location = new Point(150, 70),
                Width = 180
            };
            this.Controls.Add(nameLabel);
            this.Controls.Add(nameTextBox);

            // כפתורי שמירה וביטול
            Button saveButton = new Button
            {
                Text = "שמור",
                Size = new Size(100, 40),
                Location = new Point(220, 120)
            };
            saveButton.Click += SaveButton_Click;
            this.Controls.Add(saveButton);

            Button cancelButton = new Button
            {
                Text = "ביטול",
                Size = new Size(100, 40),
                Location = new Point(80, 120)
            };
            cancelButton.Click += (sender, e) => { this.Close(); };
            this.Controls.Add(cancelButton);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            // קבלת הערכים מהטופס
            TextBox nameTextBox = (TextBox)this.Controls["nameTextBox"];
            string branchName = nameTextBox.Text.Trim();

            // בדיקת תקינות
            if (string.IsNullOrEmpty(branchName))
            {
                MessageBox.Show("נא להזין שם סניף", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (helper.AddBranch(branchName, currentUserName))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();   
            }
            
            ;
        }

      
    }
}