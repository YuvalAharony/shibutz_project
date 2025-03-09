using shibutz_project;
using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace EmployeeSchedulingApp
{
    public partial class MainPage : Form
    {
        private List<Employee> EmployeesList;
        private ListView employeesListView;
        private List<Branch> BranchesList;
        private ListView branchesListView;


        public MainPage()
        {
            EmployeesList=Program.Employees;
            BranchesList=Program.Branches;
            SetupUI();
            LoadEmployees();
            LoadBranches(); 
        }

        private void SetupUI()
        {
            this.Text = "מסך ראשי - ניהול הרשת";
            this.Size = new System.Drawing.Size(800, 600);
            Button generateShiftsButton = new Button()
            {
                Text = "צור משמרות",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(300, 500)
            };
           // generateShiftsButton.Click += (sender, e) => { GenerateShifts(); };

            this.Controls.Add(generateShiftsButton);

            Label titleLabel = new Label()
            {
                Text = "ניהול רשת המסעדות",
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(300, 20)
            };

            Button addBranchButton = new Button()
            {
                Text = "הוסף סניף",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(50, 80)
            };
           addBranchButton.Click += (sender, e) => { OpenAddBranchPage(); };

            Button addEmployeeButton = new Button()
            {
                Text = "הוסף עובד",
                Size = new System.Drawing.Size(150, 40),
                Location = new System.Drawing.Point(220, 80)
            };
            addEmployeeButton.Click += (sender, e) => { OpenAddEmployeePage(); };

             branchesListView = new ListView()
            {
                Location = new System.Drawing.Point(50, 150),
                Size = new System.Drawing.Size(300, 300),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            branchesListView.Columns.Add("שם הסניף", 150);


            employeesListView = new ListView() 
            {
                Location = new System.Drawing.Point(400, 150),
                Size = new System.Drawing.Size(300, 300),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            employeesListView.Columns.Add("שם העובד", 150);
            employeesListView.Columns.Add("תפקיד", 150);

 
  
            this.Controls.Add(titleLabel);
            this.Controls.Add(addBranchButton);
            this.Controls.Add(addEmployeeButton);
            this.Controls.Add(branchesListView);
            this.Controls.Add(employeesListView);
        }

        public void LoadEmployees()
        {

            foreach (Employee emp in EmployeesList)
            {
                ListViewItem item = new ListViewItem(emp.Name);
                item.SubItems.Add(emp.Roles[0]); // מציג את התפקיד הראשון
                employeesListView.Items.Add(item);
            }
        }

        public void LoadBranches()
        {

            foreach (Branch br in BranchesList)
            {
                ListViewItem item = new ListViewItem(br.Name);
               
                branchesListView.Items.Add(item);
                item.Tag = br; 

            }
        }


        private void OpenAddBranchPage()
         {
             AddBranchPage addBranch = new AddBranchPage();
            addBranch.FormClosed += (sender, e) => { LoadBranches(); };

            addBranch.Show();
         }

        private void OpenAddEmployeePage()
        {
            AddEmployeePage addEmployee = new AddEmployeePage();
            addEmployee.FormClosed += (sender, e) => { LoadEmployees(); };
            addEmployee.Show();
        }



    }
}
