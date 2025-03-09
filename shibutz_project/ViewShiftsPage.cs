using shibutz_project;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EmployeeSchedulingApp
{
    public partial class ViewShiftsPage : Form
    {
        private Branch selectedBranch;
        private ListView shiftsListView;

        public ViewShiftsPage(Branch branch)
        {
            selectedBranch = branch;
            SetupUI();
            LoadShifts();
        }

        private void SetupUI()
        {
            this.Text = $"סידור משמרות - {selectedBranch.Name}";
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
        }

        private void LoadShifts()
        {
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
