using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Final
{
    public partial class ViewShiftsPage : Form
    {
        private Branch selectedBranch;
        private DataGridView shiftsDataGridView;
        // מערך של ימות השבוע – יש להתאים לשפה/פורמט הרצוי
        private readonly string[] daysOfWeek = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        public ViewShiftsPage(Branch branch)
        {
            selectedBranch = branch;
            InitializeComponent();
            SetupUI();
            shiftsDataGridView.CellDoubleClick += ShiftsGridView_CellDoubleClick;

            LoadShifts();
        }

        private void SetupUI()
        {
            this.Text = $"סידור משמרות - {selectedBranch.Name}";
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
        }

        private void LoadShifts()
        {
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
                            // Previous code:
                            // string employees = string.Join(Environment.NewLine, shift.AssignedEmployees
                            //     .Select(empId => Program.Employees.FirstOrDefault(e => e.ID == empId)?.Name ?? "לא ידוע"));

                            // Updated code for Dictionary<String, List<Employee>>:
                            string employees = string.Join(Environment.NewLine,
                                shift.AssignedEmployees.SelectMany(role => role.Value)
                                    .Select(emp => emp?.Name ?? "לא ידוע")
                                    .Distinct()); // Added Distinct() to avoid duplicates if an employee appears in multiple roles

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

        // Add this event handler to your ViewShiftsPage class
        // Add this event handler to your ViewShiftsPage class
        private void ShiftsGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            // Skip if header row/column is clicked
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            // Get the day of week from column header
            string dayOfWeek = shiftsDataGridView.Columns[e.ColumnIndex].HeaderText;

            // Get the shift time (Morning/Evening) from row header
            string shiftTime = shiftsDataGridView.Rows[e.RowIndex].Cells[0].Value.ToString();

            // Find the corresponding shift in the branch
            Shift selectedShift = selectedBranch.Shifts.FirstOrDefault(s =>
                s.day == dayOfWeek && s.TimeSlot == shiftTime);

            if (selectedShift != null)
            {
                // Create and show the detailed shift view
                ViewShiftDetailPage detailPage = new ViewShiftDetailPage(selectedShift, selectedBranch);
                detailPage.Show();
            }

        }

        // Helper method to get the Shift object from a row
        private Shift GetShiftFromRow(int rowIndex)
        {
            // This implementation depends on how you store shifts in your grid
            // If you store the shift object in the Tag property, you can do:
            if (shiftsDataGridView.Rows[rowIndex].Tag is Shift shift)
            {
                return shift;
            }

            // Alternative implementation if you store shift ID in the grid
            // and need to look it up from the branch
            string shiftId = shiftsDataGridView.Rows[rowIndex].Cells["Id"].Value.ToString();

            return selectedBranch.Shifts.FirstOrDefault(s => s.Id.ToString() == shiftId);
        }

        
    }
}