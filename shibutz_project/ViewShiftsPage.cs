<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace shibutz_project
=======
﻿using shibutz_project;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EmployeeSchedulingApp
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
{
    public partial class ViewShiftsPage : Form
    {
        private Branch selectedBranch;
<<<<<<< HEAD
        private DataGridView shiftsDataGridView;
        // מערך של ימות השבוע – יש להתאים לשפה/פורמט הרצוי
        private readonly string[] daysOfWeek = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
=======
        private ListView shiftsListView;
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74

        public ViewShiftsPage(Branch branch)
        {
            selectedBranch = branch;
<<<<<<< HEAD
            InitializeComponent();
=======
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
            SetupUI();
            LoadShifts();
        }

        private void SetupUI()
        {
            this.Text = $"סידור משמרות - {selectedBranch.Name}";
<<<<<<< HEAD
            this.Size = new System.Drawing.Size(800, 600);

            Label titleLabel = new Label()
            {
                Text = $"📌 סידור משמרות עבור {selectedBranch.Name}",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(150, 20)
            };

            shiftsDataGridView = new DataGridView()
            {
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(750, 500),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
            };
            shiftsDataGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            // הגדרת מעבר שורה אוטומטי בתוך התאים

            shiftsDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            // התאמת גובה השורות לגובה הטקסט שבהן

            // הגדרת העמודות: העמודה הראשונה – סוג המשמרת, ולאחריה עמודה לכל יום בשבוע
            shiftsDataGridView.ColumnCount = 8;
            shiftsDataGridView.Columns[0].Name = "משמרת";
            for (int i = 0; i < daysOfWeek.Length; i++)
            {
                shiftsDataGridView.Columns[i + 1].Name = daysOfWeek[i];
            }

            this.Controls.Add(titleLabel);
            this.Controls.Add(shiftsDataGridView);
=======
            this.Size = new System.Drawing.Size(500, 400);

            Label titleLabel = new Label()
            {
                Text = $"סידור משמרות עבור {selectedBranch.Name}",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(120, 20)
            };

            shiftsListView = new ListView()
            {
                Location = new System.Drawing.Point(50, 80),
                Size = new System.Drawing.Size(400, 250),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            shiftsListView.Columns.Add("מזהה משמרת", 100);
            shiftsListView.Columns.Add("תאריך", 150);
            shiftsListView.Columns.Add("עובדים שובצו", 150);

            this.Controls.Add(titleLabel);
            this.Controls.Add(shiftsListView);
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
        }

        private void LoadShifts()
        {
<<<<<<< HEAD
            shiftsDataGridView.Rows.Clear();

            // קריאה ישירה לפונקציה הסטטית
            Chromosome bestChromosome = Program.GetBestChromosome();

            if (bestChromosome == null)
            {
                MessageBox.Show("לא נמצא סידור משמרות.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (bestChromosome.Shifts.ContainsKey(selectedBranch.Name))
            {
                var shifts = bestChromosome.Shifts[selectedBranch.Name];

                // קיבוץ לפי סוג המשמרת (TimeSlot: Morning/Evening)
                var groupedShifts = shifts.GroupBy(s => s.TimeSlot)
                                          .OrderBy(g => g.Key);

                foreach (var group in groupedShifts)
                {
                    // מערך: תא ראשון = סוג המשמרת, תאים 1..n = ימים
                    string[] row = new string[daysOfWeek.Length + 1];
                    row[0] = group.Key; // לדוגמה "Morning" או "Evening"

                    foreach (var shift in group)
                    {
                        int dayIndex = Array.IndexOf(daysOfWeek, shift.day);
                        if (dayIndex >= 0)
                        {
                            string employees = string.Join(Environment.NewLine, shift.AssignedEmployees
                                .Select(empId => Program.Employees.FirstOrDefault(e => e.ID == empId)?.Name ?? "לא ידוע"));

                            if (string.IsNullOrEmpty(row[dayIndex + 1]))
                                row[dayIndex + 1] = employees;
                            else if (!string.IsNullOrEmpty(employees))
                                row[dayIndex + 1] += "; " + employees;
                        }
                    }

                    shiftsDataGridView.Rows.Add(row);
                }
            }
            else
            {
                MessageBox.Show("אין משמרות עבור סניף זה.", "הודעה", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
=======
            shiftsListView.Items.Clear();

            foreach (var shift in selectedBranch.Shifts)
            {
                string employees = string.Join(", ", shift.AssignedEmployees);
                ListViewItem item = new ListViewItem(new string[]
                {
                    shift.Id.ToString(),
                    shift.TimeSlot,
                    employees
                });
                shiftsListView.Items.Add(item);
            }
        }
    }
}
>>>>>>> 19e2b8d4529dc0491c2c2b3681ed44f2ecf7ab74
