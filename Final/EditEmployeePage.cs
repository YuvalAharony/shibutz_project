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
    public partial class EditEmployeePage : Form
    {
        private Employee selectedEmployee;
        private TextBox nameTextBox, idTextBox, rateTextBox, salaryTextBox;
        private TextBox phoneTextBox, emailTextBox; // הוספת שדות טלפון ואימייל
        private ComboBox roleComboBox;
        private CheckBox isExperiencedCheckBox;
        private CheckedListBox branchesCheckedListBox; // רשימת סניפים
        private CheckedListBox shiftsCheckedListBox;   // רשימת משמרות
        private Button saveButton, cancelButton;
        private Dictionary<string, List<ShiftDisplayInfo>> branchShifts; // מילון לשמירת המשמרות לפי סניף
        private static string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True";

        // מחלקת עזר לתצוגת מידע על משמרות
        private class ShiftDisplayInfo
        {
            public int ShiftID { get; set; }
            public string BranchName { get; set; }
            public string DayOfWeek { get; set; }
            public string TimeSlot { get; set; }
            public string ShiftType { get; set; }

            public override string ToString()
            {
                return $"{BranchName} - {DayOfWeek} {TimeSlot} ({ShiftType})";
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // EditEmployeePage
            // 
            this.ClientSize = new System.Drawing.Size(400, 750);
            this.Name = "EditEmployeePage";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.ResumeLayout(false);

        }

        

        private void SetupUI()
        {
            this.Text = "עריכת עובד";
            this.Size = new System.Drawing.Size(400, 750);

            Label titleLabel = new Label()
            {
                Text = "עריכת פרטי עובד",
                AutoSize = true,
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(120, 20)
            };

            // שם העובד
            Label nameLabel = new Label() { Text = "שם העובד:", Location = new Point(50, 70) };
            nameTextBox = new TextBox() { Location = new Point(150, 70), Width = 180 };

            // מזהה (לא ניתן לעריכה)
            Label idLabel = new Label() { Text = "מזהה (ID):", Location = new Point(50, 110) };
            idTextBox = new TextBox() { Location = new Point(150, 110), Width = 180, ReadOnly = true };

            // טלפון
            Label phoneLabel = new Label() { Text = "טלפון:", Location = new Point(50, 150) };
            phoneTextBox = new TextBox() { Location = new Point(150, 150), Width = 180 };

            // אימייל
            Label emailLabel = new Label() { Text = "אימייל:", Location = new Point(50, 190) };
            emailTextBox = new TextBox() { Location = new Point(150, 190), Width = 180 };

            // תפקיד
            Label roleLabel = new Label() { Text = "תפקיד:", Location = new Point(50, 230) };
            roleComboBox = new ComboBox()
            {
                Location = new Point(150, 230),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            roleComboBox.Items.AddRange(new string[] { "Waiter", "Chef", "Bartender", "Manager" });

            // שכר שעתי
            Label salaryLabel = new Label() { Text = "שכר שעתי:", Location = new Point(50, 270) };
            salaryTextBox = new TextBox() { Location = new Point(150, 270), Width = 180 };

            // ציון עובד
            Label rateLabel = new Label() { Text = "ציון עובד:", Location = new Point(50, 310) };
            rateTextBox = new TextBox() { Location = new Point(150, 310), Width = 180 };

            // האם מנוסה
            isExperiencedCheckBox = new CheckBox()
            {
                Text = "האם עובד מנוסה?",
                Location = new Point(150, 350)
            };

            // בחירת סניפים
            Label branchesLabel = new Label() { Text = "סניפים:", Location = new Point(50, 390) };
            branchesCheckedListBox = new CheckedListBox()
            {
                Location = new Point(50, 410),
                Width = 280,
                Height = 80,
                CheckOnClick = true
            };

            // אירוע בחירת סניפים
            branchesCheckedListBox.ItemCheck += BranchesCheckedListBox_ItemCheck;

            // בחירת משמרות מועדפות
            Label shiftsLabel = new Label() { Text = "משמרות מועדפות:", Location = new Point(50, 500) };
            shiftsCheckedListBox = new CheckedListBox()
            {
                Location = new Point(50, 520),
                Width = 280,
                Height = 120,
                CheckOnClick = true
            };

            // כפתורי שמירה וביטול
            saveButton = new Button()
            {
                Text = "שמור",
                Size = new Size(100, 40),
                Location = new Point(200, 660)
            };
            saveButton.Click += SaveEmployeeChanges;

            cancelButton = new Button()
            {
                Text = "ביטול",
                Size = new Size(100, 40),
                Location = new Point(70, 660)
            };
            cancelButton.Click += (sender, e) => { this.Close(); };

            // הוספת כל הרכיבים לטופס
            this.Controls.Add(titleLabel);
            this.Controls.Add(nameLabel);
            this.Controls.Add(nameTextBox);
            this.Controls.Add(idLabel);
            this.Controls.Add(idTextBox);
            this.Controls.Add(phoneLabel);
            this.Controls.Add(phoneTextBox);
            this.Controls.Add(emailLabel);
            this.Controls.Add(emailTextBox);
            this.Controls.Add(roleLabel);
            this.Controls.Add(roleComboBox);
            this.Controls.Add(salaryLabel);
            this.Controls.Add(salaryTextBox);
            this.Controls.Add(rateLabel);
            this.Controls.Add(rateTextBox);
            this.Controls.Add(isExperiencedCheckBox);
            this.Controls.Add(branchesLabel);
            this.Controls.Add(branchesCheckedListBox);
            this.Controls.Add(shiftsLabel);
            this.Controls.Add(shiftsCheckedListBox);
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
        }

        private void LoadBranchesAndShifts()
        {
            try
            {
                // טעינת כל הסניפים
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // טעינת כל הסניפים
                    string branchesQuery = "SELECT BranchID, Name FROM Branches";
                    using (SqlCommand command = new SqlCommand(branchesQuery, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int branchId = reader.GetInt32(0);
                                string branchName = reader.GetString(1);
                                branchesCheckedListBox.Items.Add(branchName);

                                // טעינת המשמרות של הסניף
                                LoadBranchShifts(branchId, branchName);
                            }
                        }
                    }

                    // טעינת הסניפים שהעובד משויך אליהם
                    string employeeBranchesQuery = @"
                        SELECT b.Name
                        FROM Branches b
                        INNER JOIN EmployeeBranches eb ON b.BranchID = eb.BranchID
                        WHERE eb.EmployeeID = @EmployeeID";

                    using (SqlCommand command = new SqlCommand(employeeBranchesQuery, connection))
                    {
                        command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string branchName = reader.GetString(0);
                                int index = branchesCheckedListBox.Items.IndexOf(branchName);
                                if (index >= 0)
                                {
                                    branchesCheckedListBox.SetItemChecked(index, true);
                                }
                            }
                        }
                    }

                    // טעינת המשמרות המועדפות של העובד
                    string preferredShiftsQuery = @"
                        SELECT ShiftID
                        FROM EmployeePreferredShifts
                        WHERE EmployeeID = @EmployeeID";

                    HashSet<int> preferredShiftIds = new HashSet<int>();
                    using (SqlCommand command = new SqlCommand(preferredShiftsQuery, connection))
                    {
                        command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                preferredShiftIds.Add(reader.GetInt32(0));
                            }
                        }
                    }

                    // עדכון המשמרות המועדפות של העובד באובייקט
                    selectedEmployee.requestedShifts = preferredShiftIds;
                }

                // עדכון רשימת המשמרות
                UpdateShiftsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("אירעה שגיאה בטעינת הסניפים והמשמרות: " + ex.Message, "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

       

        private void LoadBranchShifts(int branchId, string branchName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT s.ShiftID, s.TimeSlot, s.DayOfWeek, st.TypeName
                        FROM Shifts s
                        INNER JOIN ShiftTypes st ON s.ShiftTypeID = st.ShiftTypeID
                        WHERE s.BranchID = @BranchID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BranchID", branchId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            List<ShiftDisplayInfo> shifts = new List<ShiftDisplayInfo>();

                            while (reader.Read())
                            {
                                shifts.Add(new ShiftDisplayInfo
                                {
                                    ShiftID = reader.GetInt32(0),
                                    BranchName = branchName,
                                    TimeSlot = reader.GetString(1),
                                    DayOfWeek = reader.GetString(2),
                                    ShiftType = reader.GetString(3)
                                });
                            }

                            branchShifts[branchName] = shifts;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("אירעה שגיאה בטעינת המשמרות: " + ex.Message, "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

       

        // אירוע שמופעל כאשר מסמנים או מבטלים סימון של סניף
        private void BranchesCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // הרצה מושהית כדי לאפשר את עדכון הסימון לפני הפעולה
            this.BeginInvoke(new Action(() =>
            {
                UpdateShiftsList();
            }));
        }

        // עדכון רשימת המשמרות על פי הסניפים שנבחרו
        private void UpdateShiftsList()
        {
            shiftsCheckedListBox.Items.Clear();

            // איסוף כל המשמרות מהסניפים שנבחרו
            List<ShiftDisplayInfo> availableShifts = new List<ShiftDisplayInfo>();

            foreach (var checkedItem in branchesCheckedListBox.CheckedItems)
            {
                string branchName = checkedItem.ToString();
                if (branchShifts.ContainsKey(branchName))
                {
                    availableShifts.AddRange(branchShifts[branchName]);
                }
            }

            // מיון המשמרות לפי יום ושעה
            availableShifts.Sort((a, b) =>
            {
                // מיון לפי יום
                string[] days = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                int dayCompare = Array.IndexOf(days, a.DayOfWeek).CompareTo(Array.IndexOf(days, b.DayOfWeek));

                if (dayCompare != 0)
                    return dayCompare;

                // מיון לפי זמן
                string[] times = { "Morning", "Evening" };
                return Array.IndexOf(times, a.TimeSlot).CompareTo(Array.IndexOf(times, b.TimeSlot));
            });

            // הוספת המשמרות לרשימה ובדיקת אילו מהן מועדפות על העובד
            foreach (var shift in availableShifts)
            {
                int index = shiftsCheckedListBox.Items.Add(shift);
                if (selectedEmployee.requestedShifts.Contains(shift.ShiftID))
                {
                    shiftsCheckedListBox.SetItemChecked(index, true);
                }
            }
        }

        private void LoadEmployeeData()
        {
            nameTextBox.Text = selectedEmployee.Name;
            idTextBox.Text = selectedEmployee.ID.ToString();

            // טעינת נתוני טלפון ואימייל מהדאטאבייס
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Phone, Email FROM Employees WHERE EmployeeID = @EmployeeID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                phoneTextBox.Text = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : "";
                                emailTextBox.Text = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : "";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("אירעה שגיאה בטעינת נתוני העובד: " + ex.Message, "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // בחירת התפקיד הנוכחי
            if (selectedEmployee.roles != null && selectedEmployee.roles.Count > 0)
            {
                string currentRole = selectedEmployee.roles.FirstOrDefault();
                for (int i = 0; i < roleComboBox.Items.Count; i++)
                {
                    if (roleComboBox.Items[i].ToString() == currentRole)
                    {
                        roleComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }

            salaryTextBox.Text = selectedEmployee.HourlySalary.ToString();
            rateTextBox.Text = selectedEmployee.Rate.ToString();
            isExperiencedCheckBox.Checked = selectedEmployee.isMentor;
        }

        private void SaveEmployeeChanges(object sender, EventArgs e)
        {
            // בדיקת שדות חובה
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) ||
                roleComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(salaryTextBox.Text) ||
                string.IsNullOrWhiteSpace(rateTextBox.Text))
            {
                MessageBox.Show("נא למלא את כל השדות הדרושים.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // בדיקה שלפחות סניף אחד נבחר
            if (branchesCheckedListBox.CheckedItems.Count == 0)
            {
                MessageBox.Show("נא לבחור לפחות סניף אחד לשיבוץ העובד.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // עדכון פרטי העובד
                            string updateEmployeeQuery = @"
                                UPDATE Employees 
                                SET Name = @Name, 
                                    Phone = @Phone, 
                                    Email = @Email, 
                                    HourlySalary = @HourlySalary, 
                                    Rate = @Rate, 
                                    IsMentor = @IsMentor 
                                WHERE EmployeeID = @EmployeeID";

                            using (SqlCommand command = new SqlCommand(updateEmployeeQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);
                                command.Parameters.AddWithValue("@Name", nameTextBox.Text);
                                command.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phoneTextBox.Text) ? (object)DBNull.Value : phoneTextBox.Text);
                                command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(emailTextBox.Text) ? (object)DBNull.Value : emailTextBox.Text);
                                command.Parameters.AddWithValue("@HourlySalary", Convert.ToDecimal(salaryTextBox.Text));
                                command.Parameters.AddWithValue("@Rate", Convert.ToInt32(rateTextBox.Text));
                                command.Parameters.AddWithValue("@IsMentor", isExperiencedCheckBox.Checked);

                                command.ExecuteNonQuery();
                            }

                            // עדכון התפקיד - מחיקת התפקידים הקיימים והוספת התפקיד החדש
                            string deleteRolesQuery = "DELETE FROM EmployeeRoles WHERE EmployeeID = @EmployeeID";
                            using (SqlCommand command = new SqlCommand(deleteRolesQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);
                                command.ExecuteNonQuery();
                            }

                            // בדיקה אם התפקיד קיים, ואם לא - הוספתו
                            string newRole = roleComboBox.SelectedItem.ToString();
                            int roleId;

                            string getRoleIdQuery = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
                            using (SqlCommand command = new SqlCommand(getRoleIdQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@RoleName", newRole);
                                object result = command.ExecuteScalar();

                                if (result == null) // התפקיד לא קיים
                                {
                                    string insertRoleQuery = "INSERT INTO Roles (RoleName) VALUES (@RoleName); SELECT CAST(SCOPE_IDENTITY() AS INT)";
                                    using (SqlCommand insertCommand = new SqlCommand(insertRoleQuery, connection, transaction))
                                    {
                                        insertCommand.Parameters.AddWithValue("@RoleName", newRole);
                                        roleId = (int)insertCommand.ExecuteScalar();
                                    }
                                }
                                else
                                {
                                    roleId = (int)result;
                                }
                            }

                            // הוספת התפקיד החדש
                            string insertEmployeeRoleQuery = "INSERT INTO EmployeeRoles (EmployeeID, RoleID) VALUES (@EmployeeID, @RoleID)";
                            using (SqlCommand command = new SqlCommand(insertEmployeeRoleQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);
                                command.Parameters.AddWithValue("@RoleID", roleId);
                                command.ExecuteNonQuery();
                            }

                            // עדכון סניפים - מחיקת כל השיוכים הקיימים והוספת החדשים
                            string deleteEmployeeBranchesQuery = "DELETE FROM EmployeeBranches WHERE EmployeeID = @EmployeeID";
                            using (SqlCommand command = new SqlCommand(deleteEmployeeBranchesQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);
                                command.ExecuteNonQuery();
                            }

                            // הוספת הסניפים החדשים
                            List<string> newBranches = new List<string>();
                            foreach (var branch in branchesCheckedListBox.CheckedItems)
                            {
                                string branchName = branch.ToString();
                                newBranches.Add(branchName);

                                string getBranchIdQuery = "SELECT BranchID FROM Branches WHERE Name = @BranchName";
                                int branchId;

                                using (SqlCommand command = new SqlCommand(getBranchIdQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@BranchName", branchName);
                                    object result = command.ExecuteScalar();

                                    if (result != null)
                                    {
                                        branchId = (int)result;

                                        string insertEmployeeBranchQuery = "INSERT INTO EmployeeBranches (EmployeeID, BranchID) VALUES (@EmployeeID, @BranchID)";
                                        using (SqlCommand insertCommand = new SqlCommand(insertEmployeeBranchQuery, connection, transaction))
                                        {
                                            insertCommand.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);
                                            insertCommand.Parameters.AddWithValue("@BranchID", branchId);
                                            insertCommand.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }

                            // עדכון משמרות מועדפות - מחיקת כל ההעדפות הקיימות והוספת החדשות
                            string deletePreferredShiftsQuery = "DELETE FROM EmployeePreferredShifts WHERE EmployeeID = @EmployeeID";
                            using (SqlCommand command = new SqlCommand(deletePreferredShiftsQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);
                                command.ExecuteNonQuery();
                            }

                            // הוספת המשמרות המועדפות החדשות
                            HashSet<int> newPreferredShifts = new HashSet<int>();
                            foreach (ShiftDisplayInfo shift in shiftsCheckedListBox.CheckedItems)
                            {
                                int shiftId = shift.ShiftID;
                                newPreferredShifts.Add(shiftId);

                                string insertPreferredShiftQuery = "INSERT INTO EmployeePreferredShifts (EmployeeID, ShiftID) VALUES (@EmployeeID, @ShiftID)";
                                using (SqlCommand command = new SqlCommand(insertPreferredShiftQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);
                                    command.Parameters.AddWithValue("@ShiftID", shiftId);
                                    command.ExecuteNonQuery();
                                }
                            }

                            // עדכון האובייקט בזיכרון
                            selectedEmployee.Name = nameTextBox.Text;
                            selectedEmployee.roles = new List<string> { roleComboBox.SelectedItem.ToString() };
                            selectedEmployee.HourlySalary = int.Parse(salaryTextBox.Text);
                            selectedEmployee.Rate = int.Parse(rateTextBox.Text);
                            selectedEmployee.isMentor = isExperiencedCheckBox.Checked;
                            selectedEmployee.Branches = newBranches;
                            selectedEmployee.requestedShifts = newPreferredShifts;

                            // אישור העסקה
                            transaction.Commit();

                            MessageBox.Show($"פרטי העובד {selectedEmployee.Name} עודכנו בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            // ביטול העסקה במקרה של שגיאה
                            transaction.Rollback();
                            throw new Exception("אירעה שגיאה בשמירת השינויים: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public EditEmployeePage(Employee employee)
        {
            selectedEmployee = employee;
            InitializeComponent();
            branchShifts = new Dictionary<string, List<ShiftDisplayInfo>>();
            SetupUI();

            // מנטרל את האירוע זמנית
            branchesCheckedListBox.ItemCheck -= BranchesCheckedListBox_ItemCheck;

            // טעינת הסניפים והמשמרות
            LoadBranchesAndShifts();

            // טעינת נתוני העובד
            LoadEmployeeData();

            // חיבור האירוע מחדש אחרי שכל הנתונים טעונים
            branchesCheckedListBox.ItemCheck += BranchesCheckedListBox_ItemCheck;
        }
    }
}