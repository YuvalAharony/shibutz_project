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
            Console.WriteLine($"נוצר עמוד הוספת סניף עם משתמש: {currentUserName}");
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
            //try
            //{
            //    using (SqlConnection connection = new SqlConnection(connectionString))
            //    {
            //        connection.Open();

            //        // בדיקה אם הסניף כבר קיים
            //        string checkBranchQuery = "SELECT COUNT(*) FROM Branches WHERE Name = @Name";
            //        using (SqlCommand command = new SqlCommand(checkBranchQuery, connection))
            //        {
            //            command.Parameters.AddWithValue("@Name", branchName);
            //            int count = (int)command.ExecuteScalar();

            //            if (count > 0)
            //            {
            //                MessageBox.Show("סניף בשם זה כבר קיים במערכת", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //                return;
            //            }
            //        }

            //        // הוספת הסניף החדש
            //        int branchId;
            //        string insertBranchQuery = @"
            //            INSERT INTO Branches (Name)
            //            VALUES (@Name);
            //            SELECT CAST(SCOPE_IDENTITY() AS INT)";

            //        using (SqlCommand command = new SqlCommand(insertBranchQuery, connection))
            //        {
            //            command.Parameters.AddWithValue("@Name", branchName);
            //            branchId = (int)command.ExecuteScalar();
                        
            //            Console.WriteLine($"נוסף סניף חדש עם מזהה {branchId}");
            //        }

            //        // קישור הסניף למשתמש הנוכחי
            //        if (!string.IsNullOrEmpty(currentUserName))
            //        {
            //            // קבלת מזהה המשתמש
            //            int userId = helper.GetUserIdByUsername(currentUserName, connection);

            //            if (userId > 0)
            //            {
            //                // הוספת הקישור בין המשתמש לסניף
            //                string insertUserBranchQuery = @"
            //                    INSERT INTO UserBranches (UserID, BranchID)
            //                    VALUES (@UserID, @BranchID)";

            //                using (SqlCommand command = new SqlCommand(insertUserBranchQuery, connection))
            //                {
            //                    command.Parameters.AddWithValue("@UserID", userId);
            //                    command.Parameters.AddWithValue("@BranchID", branchId);
            //                    int rowsAffected = command.ExecuteNonQuery();
            //                    Console.WriteLine($"קישור המשתמש לסניף - שורות שהושפעו: {rowsAffected}");
            //                }
            //            }
            //            else
            //            {
            //                MessageBox.Show($"לא נמצא משתמש בשם {currentUserName}", "אזהרה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //            }
            //        }

            //        // הוספת משמרות לסניף
            //        GroupBox shiftsGroupBox = this.Controls.OfType<GroupBox>().FirstOrDefault(gb => gb.Text == "משמרות קבועות");
            //        if (shiftsGroupBox != null)
            //        {
            //            // קבלת סוגי המשמרות
            //            int regularShiftTypeId = helper.GetOrCreateShiftType("Regular", connection);

            //            // מעבר על כל תיבות הסימון של המשמרות
            //            foreach (CheckBox cb in shiftsGroupBox.Controls.OfType<CheckBox>())
            //            {
            //                if (cb.Checked && cb.Tag != null && cb.Tag.ToString().Contains("_"))
            //                {
            //                    string[] parts = cb.Tag.ToString().Split('_');
            //                    string dayOfWeek = parts[0];
            //                    string timeSlot = parts[1];

            //                    // הוספת משמרת חדשה
            //                    helper.AddShift(branchId, dayOfWeek, timeSlot, regularShiftTypeId, connection);
            //                }
            //            }
            //        }

            //        // הוספת הסניף לרשימה בזיכרון (אם יש צורך)
            //        Branch newBranch = new Branch
            //        {
            //            ID = branchId,
            //            Name = branchName,
            //            Shifts = new List<Shift>() // משמרות יטענו בנפרד
            //        };

            //        Program.Branches.Add(newBranch);

            //        MessageBox.Show($"הסניף {branchName} נוסף בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        this.DialogResult = DialogResult.OK;
            //        this.Close();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"אירעה שגיאה בהוספת הסניף: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

      
        
    }
}