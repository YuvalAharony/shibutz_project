using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Final;

namespace EmployeeSchedulingApp
{
    // דף לעריכת משמרות של סניף במערכת
    public partial class EditBranchShift : Form
    {
        // הסניף הנוכחי שעליו עובדים
        private Branch currentBranch;
        // המשמרת הנבחרת לעריכה
        private Shift selectedShift;
        // רשימה להצגת המשמרות
        private ListView shiftsListView;
        // פאנל לעריכת משמרת
        private Panel editPanel;
        // מופע של מחלקת העזר לבסיס הנתונים
        private static DataBaseHelper helper = new DataBaseHelper();
        // מחרוזת חיבור לבסיס הנתונים
        private static string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=EmployeeScheduling;Integrated Security=True";

        // בנאי המחלקה - יוצר טופס עריכת משמרות לסניף
        // פרמטרים
        // branch - הסניף שאת משמרותיו יש לערוך
        // ערך מוחזר: אין
        public EditBranchShift(Branch branch)
        {
            currentBranch = branch;
            InitializeComponent();
            SetupUI();
            LoadShifts();
        }

        // אתחול הרכיבים של הטופס
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // EditBranchShift
            // 
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Name = "EditBranchShift";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Text = "עריכת משמרות סניף";
            this.CenterToScreen();
            this.ResumeLayout(false);

        }

        // הגדרת ממשק המשתמש של הטופס
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void SetupUI()
        {
            this.BackColor = Color.Tan;
            // כותרת
            Label titleLabel = new Label
            {
                Text = $"ניהול משמרות - סניף {currentBranch.Name}",
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(400, 20)
            };
            this.Controls.Add(titleLabel);

            // רשימת המשמרות
            shiftsListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(20, 60),
                Size = new Size(450, 480),
                MultiSelect = false
            };

            shiftsListView.Columns.Add("מזהה", 50);
            shiftsListView.Columns.Add("יום", 100);
            shiftsListView.Columns.Add("זמן", 100);
            shiftsListView.Columns.Add("סוג אירוע", 100);
            shiftsListView.Columns.Add("מספר עובדים", 100);

            shiftsListView.SelectedIndexChanged += ShiftsListView_SelectedIndexChanged;
            this.Controls.Add(shiftsListView);

            // כפתור הוספת משמרת חדשה
            Button addShiftButton = new Button
            {
                Text = "הוסף משמרת חדשה",
                Location = new Point(150, 550),
                Size = new Size(150, 35),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            addShiftButton.Click += AddShiftButton_Click;
            this.Controls.Add(addShiftButton);

            // פאנל עריכה
            editPanel = new Panel
            {
                Location = new Point(500, 60),
                Size = new Size(480, 480),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };
            this.Controls.Add(editPanel);
        }

        // טעינת משמרות הסניף לרשימה
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void LoadShifts()
        {
            shiftsListView.Items.Clear();
            List<Shift> shifts = new List<Shift>();
            shifts = helper.LoadBranchShifts(currentBranch.ID);
            foreach (Shift shift in shifts)
            {
                shift.RequiredRoles = helper.LoadShiftRequiredRoles(shift.Id);

                ListViewItem item = new ListViewItem(shift.Id.ToString());
                item.SubItems.Add(shift.day);
                item.SubItems.Add(shift.TimeSlot);
                item.SubItems.Add(shift.EventType);
                item.SubItems.Add(shift.GetTotalRequiredEmployees().ToString());
                item.Tag = shift;
                shiftsListView.Items.Add(item);
            }
            // מיון לפי ימי השבוע ואז לפי זמן
            SortShiftsByDayAndTime();
        }

        // מיון המשמרות לפי יום ושעה
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void SortShiftsByDayAndTime()
        {
            // מיון על פי ימים ולאחר מכן זמנים
            Dictionary<string, int> dayOrder = new Dictionary<string, int>
            {
                { "Sunday", 0 },
                { "Monday", 1 },
                { "Tuesday", 2 },
                { "Wednesday", 3 },
                { "Thursday", 4 },
                { "Friday", 5 },
                { "Saturday", 6 }
            };

            Dictionary<string, int> timeOrder = new Dictionary<string, int>
            {
                { "Morning", 0 },
                { "Evening", 1 }
            };

            // יצירת רשימה ממוינת
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in shiftsListView.Items)
            {
                items.Add(item);
            }

            // מיון לפי יום ואז לפי זמן
            items.Sort((a, b) =>
            {
                string dayA = a.SubItems[1].Text;
                string dayB = b.SubItems[1].Text;

                if (dayOrder.ContainsKey(dayA) && dayOrder.ContainsKey(dayB))
                {
                    int dayCompare = dayOrder[dayA].CompareTo(dayOrder[dayB]);
                    if (dayCompare != 0)
                        return dayCompare;
                }

                string timeA = a.SubItems[2].Text;
                string timeB = b.SubItems[2].Text;

                if (timeOrder.ContainsKey(timeA) && timeOrder.ContainsKey(timeB))
                {
                    return timeOrder[timeA].CompareTo(timeOrder[timeB]);
                }

                return 0;
            });

            // ניקוי ובנייה מחדש של התצוגה
            shiftsListView.Items.Clear();
            foreach (ListViewItem item in items)
            {
                shiftsListView.Items.Add(item);
            }
        }

        // אירוע שנקרא כאשר המשתמש בוחר משמרת מהרשימה
        // פרמטרים
        // sender - האובייקט שהפעיל את האירוע
        // e - נתוני האירוע
        // ערך מוחזר: אין
        private void ShiftsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (shiftsListView.SelectedItems.Count > 0)
            {
                selectedShift = (Shift)shiftsListView.SelectedItems[0].Tag;
                ShowShiftEditPanel();
            }
        }

        // פתיחת פאנל עריכת משמרת
        // פרמטרים: אין
        // ערך מוחזר: אין
        private void ShowShiftEditPanel()
        {
            editPanel.Controls.Clear();
            editPanel.Visible = true;

            // כותרת פאנל העריכה
            Label editTitleLabel = new Label
            {
                Text = $"עריכת משמרת - {selectedShift.day}, {selectedShift.TimeSlot}",
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(150, 10)
            };
            editPanel.Controls.Add(editTitleLabel);

            // שדה בחירת יום
            Label dayLabel = new Label
            {
                Text = "יום:",
                AutoSize = true,
                Location = new Point(400, 50)
            };
            editPanel.Controls.Add(dayLabel);

            ComboBox dayComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(200, 50),
                Size = new Size(180, 25),
                RightToLeft = RightToLeft.No
            };

            string[] days = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            dayComboBox.Items.AddRange(days);
            dayComboBox.SelectedItem = selectedShift.day;
            editPanel.Controls.Add(dayComboBox);

            // שדה בחירת זמן
            Label timeLabel = new Label
            {
                Text = "זמן:",
                AutoSize = true,
                Location = new Point(400, 90)
            };
            editPanel.Controls.Add(timeLabel);

            ComboBox timeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(200, 90),
                Size = new Size(180, 25),
                RightToLeft = RightToLeft.No
            };

            List<string> times = helper.getTimeSlots();
            timeComboBox.Items.AddRange(times.ToArray());
            timeComboBox.SelectedItem = selectedShift.TimeSlot;
            editPanel.Controls.Add(timeComboBox);

            // שדה בחירת סוג אירוע
            Label eventTypeLabel = new Label
            {
                Text = "סוג אירוע:",
                AutoSize = true,
                Location = new Point(400, 130)
            };
            editPanel.Controls.Add(eventTypeLabel);

            ComboBox eventTypeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(200, 130),
                Size = new Size(180, 25),
                RightToLeft = RightToLeft.No
            };

            List<string> shiftTypes = helper.getShiftTypes();

            eventTypeComboBox.Items.AddRange(shiftTypes.ToArray());
            eventTypeComboBox.SelectedItem = selectedShift.EventType;
            editPanel.Controls.Add(eventTypeComboBox);

            // כותרת לדרישות כוח אדם
            Label rolesTitle = new Label
            {
                Text = "דרישות כוח אדם:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(350, 170)
            };
            editPanel.Controls.Add(rolesTitle);

            //תפקידים דרושים
            List<string> roles = helper.getRoles();
            int yPos = 200;
            Dictionary<string, NumericUpDown> roleCountInputs = new Dictionary<string, NumericUpDown>();

            foreach (string role in roles)
            {
                Label roleLabel = new Label
                {
                    Text = $"{role}:",
                    AutoSize = true,
                    Location = new Point(400, yPos)
                };
                editPanel.Controls.Add(roleLabel);

                NumericUpDown countInput = new NumericUpDown
                {
                    Location = new Point(300, yPos),
                    Size = new Size(80, 25),
                    Minimum = 0,
                    Maximum = 10,
                    Value = selectedShift.RequiredRoles.ContainsKey(role) ? selectedShift.RequiredRoles[role] : 0
                };
                editPanel.Controls.Add(countInput);
                roleCountInputs[role] = countInput;

                yPos += 40;
            }

            // כפתור שמירה
            Button saveButton = new Button
            {
                Text = "שמור שינויים",
                Location = new Point(200, 400),
                Size = new Size(150, 35),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };

            saveButton.Click += (s, args) =>
            {
                try
                {
                    // עדכון פרטי המשמרת
                    selectedShift.day = dayComboBox.SelectedItem.ToString();
                    selectedShift.TimeSlot = timeComboBox.SelectedItem.ToString();
                    selectedShift.EventType = eventTypeComboBox.SelectedItem.ToString();

                    // עדכון דרישות כוח האדם
                    foreach (string role in roles)
                    {
                        int count = (int)roleCountInputs[role].Value;

                        if (selectedShift.RequiredRoles.ContainsKey(role))
                        {
                            selectedShift.RequiredRoles[role] = count;
                        }
                        else
                        {
                            selectedShift.RequiredRoles.Add(role, count);
                        }
                    }

                    // שמירה בבסיס הנתונים
                    helper.SaveShiftToDatabase(selectedShift);


                    // רענון הרשימה
                    LoadShifts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"אירעה שגיאה בעדכון המשמרת: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            editPanel.Controls.Add(saveButton);

            // כפתור מחיקה
            Button deleteButton = new Button
            {
                Text = "מחק משמרת",
                Location = new Point(50, 400),
                Size = new Size(120, 35),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };

            deleteButton.Click += (s, args) =>
            {
                if (MessageBox.Show("האם אתה בטוח שברצונך למחוק את המשמרת?", "אישור מחיקה",
                                   MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        // מחיקה מבסיס הנתונים
                        helper.DeleteShiftFromDatabase(selectedShift.Id);

                        MessageBox.Show("המשמרת נמחקה בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // רענון הרשימה והסתרת פאנל העריכה
                        LoadShifts();
                        editPanel.Visible = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"אירעה שגיאה במחיקת המשמרת: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            editPanel.Controls.Add(deleteButton);
        }

        // אירוע לחיצה על כפתור הוספת משמרת חדשה
        // פרמטרים
        // sender - האובייקט שהפעיל את האירוע
        // e - נתוני האירוע
        // ערך מוחזר: אין
        private void AddShiftButton_Click(object sender, EventArgs e)
        {
            try
            {
                // יצירת אובייקט משמרת חדש עם ערכי ברירת מחדל
                Shift newShift = new Shift
                {
                    branch = currentBranch.Name,
                    TimeSlot = "Morning",
                    day = "Sunday",
                    RequiredRoles = new Dictionary<string, int> { { "Waiter", 1 }, { "Chef", 1 }, { "Manager", 1 }, { "Bartender", 1 } },
                    AssignedEmployees = new Dictionary<string, List<Employee>>(),
                    EventType = "Regular"
                };

                // הוספה לבסיס הנתונים
                int newShiftId = helper.AddShiftToDatabase(currentBranch.ID);

                // עדכון המזהה שהתקבל מהדאטאבייס
                newShift.Id = newShiftId;

                MessageBox.Show("המשמרת נוספה בהצלחה!", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // רענון הרשימה
                LoadShifts();

                // מציאת המשמרת החדשה ברשימה ובחירתה
                foreach (ListViewItem item in shiftsListView.Items)
                {
                    if (((Shift)item.Tag).Id == newShiftId)
                    {
                        item.Selected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"אירעה שגיאה בהוספת המשמרת: {ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

     
    }
}