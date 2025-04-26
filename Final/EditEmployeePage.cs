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
        private TextBox phoneTextBox, emailTextBox;
        private CheckedListBox rolesCheckedListBox; // שינוי מ-ComboBox ל-CheckedListBox
        private CheckBox isExperiencedCheckBox;
        private CheckedListBox branchesCheckedListBox;
        private CheckedListBox shiftsCheckedListBox;
        private Button saveButton, cancelButton;
        private Dictionary<string, List<ShiftDisplayInfo>> branchShifts;
        string currentUserName;
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
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            this.Text = "עריכת עובד";
            this.Size = new System.Drawing.Size(450, 700);
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // מרכוז הכותרת
            Label titleLabel = new Label()
            {
                Text = "עריכת פרטי עובד",
                AutoSize = true,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point((this.ClientSize.Width - 200) / 2, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // הגדרת מרחקים קבועים
            int labelX = 50;
            int controlX = 200;
            int verticalSpacing = 40;
            int currentY = 70;

            // שם העובד
            Label nameLabel = new Label()
            {
                Text = "שם העובד:",
                Location = new Point(labelX, currentY),
                AutoSize = true
            };
            nameTextBox = new TextBox()
            {
                Location = new Point(controlX, currentY),
                Width = 180
            };
            currentY += verticalSpacing;

            // טלפון
            Label phoneLabel = new Label()
            {
                Text = "טלפון:",
                Location = new Point(labelX, currentY),
                AutoSize = true
            };
            phoneTextBox = new TextBox()
            {
                Location = new Point(controlX, currentY),
                Width = 180
            };
            currentY += verticalSpacing;

            // אימייל
            Label emailLabel = new Label()
            {
                Text = "אימייל:",
                Location = new Point(labelX, currentY),
                AutoSize = true
            };
            emailTextBox = new TextBox()
            {
                Location = new Point(controlX, currentY),
                Width = 180
            };
            currentY += verticalSpacing;

            // תפקידים (CheckedListBox במקום ComboBox)
            Label roleLabel = new Label()
            {
                Text = "תפקידים:",
                Location = new Point(labelX, currentY),
                AutoSize = true
            };

            // יצירת CheckedListBox לתפקידים
            rolesCheckedListBox = new CheckedListBox()
            {
                Location = new Point(controlX, currentY),
                Width = 180,
                Height = 80,
                CheckOnClick = true
            };

            // הוספת התפקידים האפשריים
            string[] roles = { "Waiter", "Chef", "Bartender", "Manager" };
            rolesCheckedListBox.Items.AddRange(roles);

            currentY += rolesCheckedListBox.Height + 5;

            // שכר שעתי
            Label salaryLabel = new Label()
            {
                Text = "שכר שעתי:",
                Location = new Point(labelX, currentY),
                AutoSize = true
            };
            salaryTextBox = new TextBox()
            {
                Location = new Point(controlX, currentY),
                Width = 180
            };
            currentY += verticalSpacing;

            // ציון עובד
            Label rateLabel = new Label()
            {
                Text = "ציון עובד:",
                Location = new Point(labelX, currentY),
                AutoSize = true
            };
            rateTextBox = new TextBox()
            {
                Location = new Point(controlX, currentY),
                Width = 180
            };
            currentY += verticalSpacing;

            // האם מנוסה
            isExperiencedCheckBox = new CheckBox()
            {
                Text = "האם עובד מנוסה?",
                Location = new Point(controlX, currentY),
                AutoSize = true
            };
            currentY += verticalSpacing;

            //  סניפים
            Label branchesLabel = new Label()
            {
                Text = "סניפים:",
                Location = new Point(labelX, currentY),
                AutoSize = true
            };
            currentY += 20;
            branchesCheckedListBox = new CheckedListBox()
            {
                Location = new Point(labelX, currentY),
                Width = this.ClientSize.Width - 100,
                Height = 80,
                CheckOnClick = true
            };
            branchesCheckedListBox.ItemCheck += BranchesCheckedListBox_ItemCheck;
            currentY += 100;

            // = משמרות מועדפות
            Label shiftsLabel = new Label()
            {
                Text = "משמרות מועדפות:",
                Location = new Point(labelX, currentY),
                AutoSize = true
            };
            currentY += 20;
            shiftsCheckedListBox = new CheckedListBox()
            {
                Location = new Point(labelX, currentY),
                Width = this.ClientSize.Width - 100,
                Height = 120,
            };
            currentY += 140;

            // כפתורי שמירה וביטול
            int buttonWidth = 100;
            int buttonHeight = 40;
            int buttonSpacing = (this.ClientSize.Width - (2 * buttonWidth)) / 2;

            cancelButton = new Button()
            {
                Text = "ביטול",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(buttonSpacing, currentY)
            };
            cancelButton.Click += (sender, e) => { this.Close(); };

            saveButton = new Button()
            {
                Text = "שמור",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(buttonSpacing + buttonWidth + 20, currentY)
            };
            saveButton.Click += SaveEmployeeChanges;

            // הוספת כל הרכיבים לטופס
            this.Controls.Add(titleLabel);
            this.Controls.Add(nameLabel);
            this.Controls.Add(nameTextBox);
            this.Controls.Add(phoneLabel);
            this.Controls.Add(phoneTextBox);
            this.Controls.Add(emailLabel);
            this.Controls.Add(emailTextBox);
            this.Controls.Add(roleLabel);
            this.Controls.Add(rolesCheckedListBox);
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
                    string branchesQuery = "SELECT b.BranchID, Name FROM Branches b join UserBranches ub on ub.BranchID=b.BranchID " +
                        "join Users u on u.UserID=ub.UserID " +
                        "where u.UserName=@UserName";
                    using (SqlCommand command = new SqlCommand(branchesQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserName", currentUserName);

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
                        SELECT s.ShiftID, ts.TimeSlotName, s.DayOfWeek, st.TypeName
                        FROM Shifts s
                        INNER JOIN ShiftTypes st ON s.ShiftTypeID = st.ShiftTypeID
                        INNER JOIN TimeSlots ts ON s.TimeSlotID=ts.TimeSlotID 
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

            // טעינת נתוני טלפון ואימייל מהדאטאבייס
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // טעינת נתוני טלפון ואימייל
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

                    // טעינת התפקידים הנוכחיים של העובד
                    string rolesQuery = @"
                        SELECT r.RoleName
                        FROM EmployeeRoles er
                        JOIN Roles r ON er.RoleID = r.RoleID
                        WHERE er.EmployeeID = @EmployeeID";

                    using (SqlCommand command = new SqlCommand(rolesQuery, connection))
                    {
                        command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // סימון התפקידים הקיימים ברשימה
                            while (reader.Read())
                            {
                                string roleName = reader.GetString(0);
                                int index = rolesCheckedListBox.Items.IndexOf(roleName);
                                if (index >= 0)
                                {
                                    rolesCheckedListBox.SetItemChecked(index, true);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("אירעה שגיאה בטעינת נתוני העובד: " + ex.Message, "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            salaryTextBox.Text = selectedEmployee.HourlySalary.ToString();
            rateTextBox.Text = selectedEmployee.Rate.ToString();
            isExperiencedCheckBox.Checked = selectedEmployee.isMentor;
        }

        private void SaveEmployeeChanges(object sender, EventArgs e)
        {
            // בדיקת שדות חובה
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) ||
                rolesCheckedListBox.CheckedItems.Count == 0 || // בדיקה שנבחר לפחות תפקיד אחד
                string.IsNullOrWhiteSpace(salaryTextBox.Text) ||
                string.IsNullOrWhiteSpace(rateTextBox.Text))
            {
                MessageBox.Show("נא למלא את כל השדות הדרושים ולבחור לפחות תפקיד אחד.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                            // עדכון התפקידים - מחיקת התפקידים הקיימים והוספת התפקידים החדשים
                            string deleteRolesQuery = "DELETE FROM EmployeeRoles WHERE EmployeeID = @EmployeeID";
                            using (SqlCommand command = new SqlCommand(deleteRolesQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@EmployeeID", selectedEmployee.ID);
                                command.ExecuteNonQuery();
                            }

                            // הוספת התפקידים הנבחרים
                            foreach (var item in rolesCheckedListBox.CheckedItems)
                            {
                                string roleName = item.ToString();

                                // בדיקה אם התפקיד קיים, אם לא - הוספתו
                                int roleId;
                                string getRoleIdQuery = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
                                using (SqlCommand command = new SqlCommand(getRoleIdQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@RoleName", roleName);
                                    object result = command.ExecuteScalar();

                                    if (result == null) // התפקיד לא קיים
                                    {
                                        string insertRoleQuery = "INSERT INTO Roles (RoleName) VALUES (@RoleName); SELECT CAST(SCOPE_IDENTITY() AS INT)";
                                        using (SqlCommand insertCommand = new SqlCommand(insertRoleQuery, connection, transaction))
                                        {
                                            insertCommand.Parameters.AddWithValue("@RoleName", roleName);
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

                            // איסוף התפקידים הנבחרים
                            HashSet<string> selectedRoles = new HashSet<string>();
                            foreach (var item in rolesCheckedListBox.CheckedItems)
                            {
                                selectedRoles.Add(item.ToString());
                            }

                            // עדכון האובייקט בזיכרון
                            selectedEmployee.Name = nameTextBox.Text;
                            selectedEmployee.roles = selectedRoles;
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

        public EditEmployeePage(Employee employee, string userName)
        {
            selectedEmployee = employee;
            InitializeComponent();
            branchShifts = new Dictionary<string, List<ShiftDisplayInfo>>();
            SetupUI();
            currentUserName = userName;

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