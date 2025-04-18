﻿using Final;
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
            Label nameLabel = new Label { Text = "שם הסניף:", Location = new Point(50, 70) };
            TextBox nameTextBox = new TextBox { Name = "nameTextBox", Location = new Point(150, 70), Width = 180 };
            this.Controls.Add(nameLabel);
            this.Controls.Add(nameTextBox);

            // הגדרת משמרות קבועות
            GroupBox shiftsGroupBox = new GroupBox
            {
                Text = "משמרות קבועות",
                Location = new Point(0, 120),
                Size = new Size(280, 220)
            };
            this.Controls.Add(shiftsGroupBox);

            // ימי פעילות
            string[] days = { "ראשון", "שני", "שלישי", "רביעי", "חמישי", "שישי", "שבת" };
            string[] dayValues = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

            for (int i = 0; i < days.Length; i++)
            {
                CheckBox dayCheckBox = new CheckBox
                {
                    Text = days[i],
                    Tag = dayValues[i],
                    Location = new Point(-20, 30 + i * 25),
                    Checked = true // ברירת מחדל לבחור את כל הימים
                };

                // משמרות בוקר וערב
                CheckBox morningShift = new CheckBox
                {
                    Text = "בוקר",
                    Tag = $"{dayValues[i]}_Morning",
                    Location = new Point(50, 30 + i * 25),
                    Checked = true
                };

                CheckBox eveningShift = new CheckBox
                {
                    Text = "ערב",
                    Tag = $"{dayValues[i]}_Evening",
                    Location = new Point(130, 30 + i * 25),
                    Checked = true
                };

                shiftsGroupBox.Controls.Add(dayCheckBox);
                shiftsGroupBox.Controls.Add(morningShift);
                shiftsGroupBox.Controls.Add(eveningShift);

                // כאשר מבטלים יום, מבטלים גם את המשמרות שלו
                dayCheckBox.CheckedChanged += (sender, e) =>
                {
                    var cb = (CheckBox)sender;
                    morningShift.Enabled = cb.Checked;
                    eveningShift.Enabled = cb.Checked;

                    if (!cb.Checked)
                    {
                        morningShift.Checked = false;
                        eveningShift.Checked = false;
                    }
                };
            }

            // כפתורי שמירה וביטול
            Button saveButton = new Button
            {
                Text = "שמור",
                Size = new Size(100, 40),
                Location = new Point(220, 360)
            };
            saveButton.Click += SaveButton_Click;
            this.Controls.Add(saveButton);

            Button cancelButton = new Button
            {
                Text = "ביטול",
                Size = new Size(100, 40),
                Location = new Point(80, 360)
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

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // בדיקה אם הסניף כבר קיים
                    string checkBranchQuery = "SELECT COUNT(*) FROM Branches WHERE Name = @Name";
                    using (SqlCommand command = new SqlCommand(checkBranchQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Name", branchName);
                        int count = (int)command.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("סניף בשם זה כבר קיים במערכת", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // הוספת הסניף החדש
                    int branchId;
                    string insertBranchQuery = @"
                        INSERT INTO Branches (Name)
                        VALUES (@Name);
                        SELECT CAST(SCOPE_IDENTITY() AS INT)";

                    using (SqlCommand command = new SqlCommand(insertBranchQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Name", branchName);
                        branchId = (int)command.ExecuteScalar();
                        
                        Console.WriteLine($"נוסף סניף חדש עם מזהה {branchId}");
                    }

                    // קישור הסניף למשתמש הנוכחי
                    if (!string.IsNullOrEmpty(currentUserName))
                    {
                        // קבלת מזהה המשתמש
                        int userId = GetUserIdByUsername(currentUserName, connection);

                        if (userId > 0)
                        {
                            // הוספת הקישור בין המשתמש לסניף
                            string insertUserBranchQuery = @"
                                INSERT INTO UserBranches (UserID, BranchID)
                                VALUES (@UserID, @BranchID)";

                            using (SqlCommand command = new SqlCommand(insertUserBranchQuery, connection))
                            {
                                command.Parameters.AddWithValue("@UserID", userId);
                                command.Parameters.AddWithValue("@BranchID", branchId);
                                int rowsAffected = command.ExecuteNonQuery();
                                Console.WriteLine($"קישור המשתמש לסניף - שורות שהושפעו: {rowsAffected}");
                            }
                        }
                        else
                        {
                            MessageBox.Show($"לא נמצא משתמש בשם {currentUserName}", "אזהרה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }

                    // הוספת משמרות לסניף
                    GroupBox shiftsGroupBox = this.Controls.OfType<GroupBox>().FirstOrDefault(gb => gb.Text == "משמרות קבועות");
                    if (shiftsGroupBox != null)
                    {
                        // קבלת סוגי המשמרות
                        int regularShiftTypeId = GetOrCreateShiftType("Regular", connection);

                        // מעבר על כל תיבות הסימון של המשמרות
                        foreach (CheckBox cb in shiftsGroupBox.Controls.OfType<CheckBox>())
                        {
                            if (cb.Checked && cb.Tag != null && cb.Tag.ToString().Contains("_"))
                            {
                                string[] parts = cb.Tag.ToString().Split('_');
                                string dayOfWeek = parts[0];
                                string timeSlot = parts[1];

                                // הוספת משמרת חדשה
                                AddShift(branchId, dayOfWeek, timeSlot, regularShiftTypeId, connection);
                            }
                        }
                    }

                    // הוספת הסניף לרשימה בזיכרון (אם יש צורך)
                    Branch newBranch = new Branch
                    {
                        ID = branchId,
                        Name = branchName,
                        Shifts = new List<Shift>() // משמרות יטענו בנפרד
                    };

                    Program.Branches.Add(newBranch);

                    MessageBox.Show($"הסניף {branchName} נוסף בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"אירעה שגיאה בהוספת הסניף: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // פונקציית עזר לקבלת מזהה משתמש לפי שם משתמש
        private int GetUserIdByUsername(string username, SqlConnection connection)
        {
            try
            {
                string query = "SELECT UserID FROM Users WHERE Username = @Username";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"שגיאה בקבלת מזהה משתמש: {ex.Message}");
            }

            return -1;
        }

        // פונקציית עזר לקבלת מזהה סוג משמרת (או יצירת סוג חדש אם אינו קיים)
        private int GetOrCreateShiftType(string typeName, SqlConnection connection)
        {
            try
            {
                // בדיקה אם סוג המשמרת כבר קיים
                string query = "SELECT ShiftTypeID FROM ShiftTypes WHERE TypeName = @TypeName";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TypeName", typeName);
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }

                // אם לא קיים, יצירת סוג משמרת חדש
                string insertQuery = @"
                    INSERT INTO ShiftTypes (TypeName)
                    VALUES (@TypeName);
                    SELECT CAST(SCOPE_IDENTITY() AS INT)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@TypeName", typeName);
                    return (int)command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"שגיאה בקבלת/יצירת סוג משמרת: {ex.Message}");
                return -1;
            }
        }

        // פונקציית עזר להוספת משמרת חדשה
        private void AddShift(int branchId, string dayOfWeek, string timeSlot, int shiftTypeId, SqlConnection connection)
        {
            try
            {
                // הוספת המשמרת
                string insertShiftQuery = @"
                    INSERT INTO Shifts (BranchID, TimeSlot, DayOfWeek, ShiftTypeID, IsBusy)
                    VALUES (@BranchID, @TimeSlot, @DayOfWeek, @ShiftTypeID, 0);
                    SELECT CAST(SCOPE_IDENTITY() AS INT)";

                int shiftId;
                using (SqlCommand command = new SqlCommand(insertShiftQuery, connection))
                {
                    command.Parameters.AddWithValue("@BranchID", branchId);
                    command.Parameters.AddWithValue("@TimeSlot", timeSlot);
                    command.Parameters.AddWithValue("@DayOfWeek", dayOfWeek);
                    command.Parameters.AddWithValue("@ShiftTypeID", shiftTypeId);

                    shiftId = (int)command.ExecuteScalar();
                    Console.WriteLine($"נוספה משמרת חדשה: {dayOfWeek}, {timeSlot}, ID: {shiftId}");
                }

                // הוספת דרישות תפקידים בסיסיות למשמרת
                Dictionary<string, int> defaultRoles = new Dictionary<string, int>
                {
                    { "Waiter", 2 },
                    { "Chef", 1 },
                    { "Bartender", 1 },
                    { "Manager", 1 }
                };

                foreach (var role in defaultRoles)
                {
                    int roleId = GetOrCreateRole(role.Key, connection);
                    if (roleId > 0)
                    {
                        AddShiftRequiredRole(shiftId, roleId, role.Value, connection);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"שגיאה בהוספת משמרת: {ex.Message}");
            }
        }

        // פונקציית עזר לקבלת מזהה תפקיד (או יצירת תפקיד חדש אם אינו קיים)
        private int GetOrCreateRole(string roleName, SqlConnection connection)
        {
            try
            {
                // בדיקה אם התפקיד כבר קיים
                string query = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RoleName", roleName);
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }

                // אם לא קיים, יצירת תפקיד חדש
                string insertQuery = @"
                    INSERT INTO Roles (RoleName)
                    VALUES (@RoleName);
                    SELECT CAST(SCOPE_IDENTITY() AS INT)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@RoleName", roleName);
                    return (int)command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"שגיאה בקבלת/יצירת תפקיד: {ex.Message}");
                return -1;
            }
        }

        // פונקציית עזר להוספת דרישת תפקיד למשמרת
        private void AddShiftRequiredRole(int shiftId, int roleId, int requiredCount, SqlConnection connection)
        {
            try
            {
                string insertQuery = @"
                    INSERT INTO ShiftRequiredRoles (ShiftID, RoleID, RequiredCount)
                    VALUES (@ShiftID, @RoleID, @RequiredCount)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@ShiftID", shiftId);
                    command.Parameters.AddWithValue("@RoleID", roleId);
                    command.Parameters.AddWithValue("@RequiredCount", requiredCount);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"שגיאה בהוספת דרישת תפקיד: {ex.Message}");
            }
        }

    }
}