using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HumanitarianProjectManagement.Models;
using System.Globalization;
using System.Diagnostics;

namespace HumanitarianProjectManagement
{
    public partial class BudgetTabUserControl : UserControl
    {
        private Project _currentProject;
        private BudgetCategoriesEnum _selectedMainCategory;

        private Color defaultButtonBackColor = Color.FromArgb(225, 225, 225);
        private Color selectedButtonBackColor = Color.FromArgb(0, 122, 204);

        // Fields for dynamically created controls for direct budget lines
        private TextBox txtDirectItem;
        private TextBox txtDirectDescription;
        private TextBox txtDirectUnit;
        private NumericUpDown numDirectQuantity;
        private NumericUpDown numDirectUnitCost;
        private NumericUpDown numDirectDuration;
        private NumericUpDown numDirectPercentageCBPF;
        private Button btnDirectAddLine;
        private DataGridView dgvDirectLines;

        public BudgetTabUserControl()
        {
            InitializeComponent();
            this.btnAddNewSubCategory.Click += btnAddNewSubCategory_Click;
        }

        public void LoadProject(Project project)
        {
            System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.LoadProject: Method called. Project is {(project == null ? "null" : "not null, ProjectID: " + project.ProjectID)}");
            try
            {
                _currentProject = project;
                if (_currentProject == null)
                {
                    System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.LoadProject: Project is null, calling ClearAndHideAll.");
                    ClearAndHideAll();
                    InitializeMainCategoryButtons();
                    return;
                }

                if (_currentProject.BudgetSubCategories == null)
                {
                    _currentProject.BudgetSubCategories = new BindingList<BudgetSubCategory>();
                }
                if (_currentProject.DetailedBudgetLines == null)
                {
                    _currentProject.DetailedBudgetLines = new List<DetailedBudgetLine>();
                }

                InitializeMainCategoryButtons();

                if (tlpCategoryButtons.Controls.Count > 0 && tlpCategoryButtons.Controls[0] is Button firstCatButton)
                {
                    System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.LoadProject: Performing click on the first category button.");
                    firstCatButton.PerformClick();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.LoadProject: No category buttons found after initialization, calling ClearAndHideAll.");
                    ClearAndHideAll();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.LoadProject: EXCEPTION: {ex.ToString()}");
                MessageBox.Show($"Error loading project budget: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeMainCategoryButtons()
        {
            if (this.tlpCategoryButtons == null)
            {
                System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.InitializeMainCategoryButtons: tlpCategoryButtons is null, cannot initialize.");
                return;
            }
            System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.InitializeMainCategoryButtons: Started.");

            this.tlpCategoryButtons.SuspendLayout();
            this.tlpCategoryButtons.Controls.Clear();
            this.tlpCategoryButtons.RowStyles.Clear();

            int currentRow = 0;
            foreach (BudgetCategoriesEnum catEnum in Enum.GetValues(typeof(BudgetCategoriesEnum)))
            {
                this.tlpCategoryButtons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                Button btnCategory = new Button
                {
                    Text = GetCategoryDisplayName(catEnum),
                    Tag = catEnum,
                    Dock = DockStyle.Top,
                    Height = 35,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 0, 0),
                    Margin = new Padding(0, 3, 0, 3),
                    BackColor = defaultButtonBackColor,
                    ForeColor = Color.Black,
                    Cursor = Cursors.Hand
                };
                btnCategory.FlatAppearance.BorderSize = 0;
                btnCategory.Click += btnBudgetCategory_Internal_Click;
                this.tlpCategoryButtons.Controls.Add(btnCategory, 0, currentRow++);
            }
            this.tlpCategoryButtons.RowCount = currentRow;
            this.tlpCategoryButtons.ResumeLayout(true);
            System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.InitializeMainCategoryButtons: Finished. Added {currentRow} buttons.");
        }

        private string GetCategoryDisplayName(BudgetCategoriesEnum category)
        {
            string name = category.ToString();
            if (name.Length > 2 && name[1] == '_')
            {
                name = name[0] + ". " + System.Text.RegularExpressions.Regex.Replace(name.Substring(2), "([a-z])([A-Z])", "$1 $2");
            }
            else
            {
                name = System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
            }
            return name;
        }

        private void btnBudgetCategory_Internal_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton == null || !(clickedButton.Tag is BudgetCategoriesEnum))
            {
                System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.btnBudgetCategory_Internal_Click: Clicked button or its tag is invalid.");
                return;
            }

            _selectedMainCategory = (BudgetCategoriesEnum)clickedButton.Tag;
            System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.btnBudgetCategory_Internal_Click: Selected main category: {_selectedMainCategory}");

            foreach (Control c in this.tlpCategoryButtons.Controls)
            {
                if (c is Button btn)
                {
                    btn.BackColor = defaultButtonBackColor;
                    btn.ForeColor = Color.Black;
                }
            }
            clickedButton.BackColor = selectedButtonBackColor;
            clickedButton.ForeColor = Color.White;

            string categoryDisplayPrefix = GetCategoryDisplayName(_selectedMainCategory).Split('.')[0];
            this.btnAddNewSubCategory.Text = $"Add New Subcategory to {categoryDisplayPrefix} (Future)";
            this.btnAddNewSubCategory.Enabled = true;

            if (this.pnlMainBudgetContentArea == null)
            {
                System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.btnBudgetCategory_Internal_Click: pnlMainBudgetContentArea is null. Cannot update UI.");
                return;
            }

            this.pnlMainBudgetContentArea.SuspendLayout();
            this.pnlMainBudgetContentArea.Controls.Clear();

            Label lblCategoryHeader = new Label
            {
                Text = GetCategoryDisplayName(_selectedMainCategory),
                Font = new Font(this.Font.FontFamily, 12F, FontStyle.Bold),
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(5),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.pnlMainBudgetContentArea.Controls.Add(lblCategoryHeader);

            TableLayoutPanel tlpInputRow = new TableLayoutPanel
            {
                ColumnCount = 8,
                RowCount = 2,
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 5, 0, 10),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            for (int i = 0; i < 7; i++) { tlpInputRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F)); }
            tlpInputRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));

            string[] labels = { "Item:", "Description:", "Unit:", "Quantity:", "Unit Cost:", "Duration:", "% CBPF:" };
            Control[] inputs = new Control[7];

            txtDirectItem = new TextBox { Dock = DockStyle.Fill, Name = "txtDirectItem" };
            inputs[0] = txtDirectItem;
            txtDirectDescription = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 40, Name = "txtDirectDescription" };
            inputs[1] = txtDirectDescription;
            txtDirectUnit = new TextBox { Dock = DockStyle.Fill, Name = "txtDirectUnit" };
            inputs[2] = txtDirectUnit;
            numDirectQuantity = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 2, Minimum = 0, Maximum = 999999, Name = "numDirectQuantity" };
            inputs[3] = numDirectQuantity;
            numDirectUnitCost = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 2, Minimum = 0, Maximum = 99999999, Name = "numDirectUnitCost" };
            inputs[4] = numDirectUnitCost;
            numDirectDuration = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 2, Minimum = 1, Maximum = 9999, Value = 1, Name = "numDirectDuration" };
            inputs[5] = numDirectDuration;
            numDirectPercentageCBPF = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 0, Minimum = 0, Maximum = 100, Value = 100, Name = "numDirectPercentageCBPF" };
            inputs[6] = numDirectPercentageCBPF;

            for (int i = 0; i < labels.Length; i++)
            {
                tlpInputRow.Controls.Add(new Label { Text = labels[i], AutoSize = true, Anchor = AnchorStyles.Left, TextAlign = ContentAlignment.MiddleLeft }, i, 0);
                tlpInputRow.Controls.Add(inputs[i], i, 1);
            }

            btnDirectAddLine = new Button { Text = "Add Line", Dock = DockStyle.Fill, Name = "btnDirectAddLine" };
            btnDirectAddLine.Click += BtnDirectAddLine_Click; // Register event handler
            tlpInputRow.Controls.Add(btnDirectAddLine, 7, 1);

            this.pnlMainBudgetContentArea.Controls.Add(tlpInputRow);

            dgvDirectLines = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                Name = "dgvDirectLines",
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 9.75F, FontStyle.Bold) }
            };

            string[] headers = { "Code", "Item", "Description", "Unit", "Qty", "Unit Cost", "Duration", "% CBPF", "Total Cost", "Actions" };
            string[] dataProps = { "Code", "ItemName", "Description", "Unit", "Quantity", "UnitCost", "Duration", "PercentageChargedToCBPF", "TotalCost", "" }; // Assuming ItemName will be added to model or handled

            for (int i = 0; i < headers.Length; i++)
            {
                DataGridViewColumn col;
                if (headers[i] == "Actions")
                {
                    col = new DataGridViewButtonColumn { Name = headers[i], HeaderText = headers[i], Text = "Edit/Del", UseColumnTextForButtonValue = true, Width = 100, Frozen = false };
                }
                else
                {
                    col = new DataGridViewTextBoxColumn { Name = headers[i].Replace(" ", "").Replace("%", "Perc"), HeaderText = headers[i], DataPropertyName = dataProps[i] };
                    if (headers[i] == "Code" || headers[i] == "Total Cost") ((DataGridViewTextBoxColumn)col).ReadOnly = true;
                    if (headers[i] == "Description") ((DataGridViewTextBoxColumn)col).MinimumWidth = 150; else if (headers[i] != "Item") col.Width = 75;

                    if (new[] { "Qty", "Unit Cost", "Duration", "% CBPF", "Total Cost" }.Contains(headers[i]))
                    {
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        if (headers[i] == "Unit Cost" || headers[i] == "Total Cost") col.DefaultCellStyle.Format = "C2";
                        else if (headers[i] == "% CBPF") col.DefaultCellStyle.Format = "P0";
                        else col.DefaultCellStyle.Format = "N2";
                    }
                }
                dgvDirectLines.Columns.Add(col);
            }
            this.pnlMainBudgetContentArea.Controls.Add(dgvDirectLines);
            dgvDirectLines.BringToFront();
            this.pnlMainBudgetContentArea.ResumeLayout(true);
            System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.btnBudgetCategory_Internal_Click: Added input row and DGV for {_selectedMainCategory}.");
        }

        private void ClearAndHideAll()
        {
            System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.ClearAndHideAll: Method called.");
            if (this.pnlMainBudgetContentArea != null) this.pnlMainBudgetContentArea.Controls.Clear();

            if (this.btnAddNewSubCategory != null)
            {
                this.btnAddNewSubCategory.Enabled = false;
                this.btnAddNewSubCategory.Text = "Add New Subcategory";
            }
            _selectedMainCategory = default(BudgetCategoriesEnum);

            if (this.tlpCategoryButtons != null)
            {
                foreach (Control c in this.tlpCategoryButtons.Controls)
                {
                    if (c is Button btn)
                    {
                        btn.BackColor = defaultButtonBackColor;
                        btn.ForeColor = Color.Black;
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.ClearAndHideAll: Finished clearing UI elements.");
        }

        private void btnAddNewSubCategory_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Add New Subcategory to {_selectedMainCategory} - Functionality to be implemented in a future iteration.", "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
            System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.btnAddNewSubCategory_Click: Clicked for category {_selectedMainCategory}. Placeholder action.");
        }

        private string GetCategoryPrefix(BudgetCategoriesEnum category)
        {
            string name = category.ToString();
            if (name.Length > 2 && name[1] == '_') { return name.Substring(0, 1); } // E.g. "A_StaffAndPersonnel" -> "A"
            // For categories like "Equipment", "Travel", take the first letter.
            // This might need adjustment if multiple categories start with the same letter and are not prefixed like "A_".
            return name.Substring(0, 1);
        }

        private void RecalculateItemTotal(DetailedBudgetLine item)
        {
            if (item == null) return;
            decimal percentageFactor = item.PercentageChargedToCBPF > 0 ? item.PercentageChargedToCBPF / 100M : 1M;
            item.TotalCost = item.Quantity * item.UnitCost * item.Duration * percentageFactor;
            System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.RecalculateItemTotal: Item '{item.Description}' total cost is {item.TotalCost}.");
        }

        private string GenerateDirectLineCode(BudgetCategoriesEnum mainCategory)
        {
            if (_currentProject == null || _currentProject.DetailedBudgetLines == null)
            {
                System.Diagnostics.Debug.WriteLine("GenerateDirectLineCode: _currentProject or DetailedBudgetLines is null. Returning default code.");
                return GetCategoryPrefix(mainCategory) + ".1";
            }

            string prefix = GetCategoryPrefix(mainCategory) + ".";
            int maxSuffix = 0;

            var directLinesForCategory = _currentProject.DetailedBudgetLines
                .Where(line => line.Category == mainCategory &&
                               (line.BudgetSubCategoryID == Guid.Empty || line.BudgetSubCategoryID == null) && // Direct lines
                               line.Code != null && line.Code.StartsWith(prefix))
                .ToList();

            foreach (var line in directLinesForCategory)
            {
                string suffixPart = line.Code.Substring(prefix.Length);
                if (int.TryParse(suffixPart, out int currentSuffix))
                {
                    if (currentSuffix > maxSuffix)
                    {
                        maxSuffix = currentSuffix;
                    }
                }
            }
            return prefix + (maxSuffix + 1).ToString();
        }

        private void BtnDirectAddLine_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.BtnDirectAddLine_Click: Initiated.");
            if (_currentProject == null)
            {
                MessageBox.Show("No project is currently loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validation
            if (string.IsNullOrWhiteSpace(txtDirectItem.Text))
            {
                MessageBox.Show("Item name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDirectItem.Focus();
                return;
            }
            if (numDirectQuantity.Value <= 0)
            {
                MessageBox.Show("Quantity must be greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numDirectQuantity.Focus();
                return;
            }
            if (numDirectUnitCost.Value < 0) // Allow zero cost, but not negative
            {
                MessageBox.Show("Unit Cost cannot be negative.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numDirectUnitCost.Focus();
                return;
            }


            DetailedBudgetLine newLine = new DetailedBudgetLine
            {
                ProjectId = _currentProject.ProjectID,
                Category = _selectedMainCategory,
                BudgetSubCategoryID = Guid.Empty, // Indicates a direct line
                // Assuming txtDirectItem is the main short name, txtDirectDescription is further detail.
                // Model's Description field will store this combined or primary info.
                // For now, let's combine if txtDirectDescription is not empty.
                Description = string.IsNullOrWhiteSpace(txtDirectDescription.Text) ? txtDirectItem.Text.Trim() : $"{txtDirectItem.Text.Trim()} - {txtDirectDescription.Text.Trim()}",
                Unit = txtDirectUnit.Text.Trim(),
                Quantity = numDirectQuantity.Value,
                UnitCost = numDirectUnitCost.Value,
                Duration = numDirectDuration.Value,
                PercentageChargedToCBPF = numDirectPercentageCBPF.Value,
                Code = GenerateDirectLineCode(_selectedMainCategory)
            };

            RecalculateItemTotal(newLine);

            _currentProject.DetailedBudgetLines.Add(newLine);
            System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.BtnDirectAddLine_Click: Added new direct line '{newLine.Code} - {newLine.Description}' to category {_selectedMainCategory}.");

            // Clear input fields
            txtDirectItem.Clear();
            txtDirectDescription.Clear();
            txtDirectUnit.Clear();
            numDirectQuantity.Value = 0;
            numDirectUnitCost.Value = 0;
            numDirectDuration.Value = 1;
            numDirectPercentageCBPF.Value = 100;

            // Refresh DataGridView (Full refresh logic will be in Part 3)
            // For now, this is a placeholder for the refresh.
            // If dgvDirectLines.DataSource is set to a BindingList<DetailedBudgetLine>,
            // adding to that list would auto-refresh.
            // If it's List<T>, manual refresh like below is needed.
            if (dgvDirectLines != null)
            {
                var directLinesForCategory = _currentProject.DetailedBudgetLines
                    .Where(line => line.Category == _selectedMainCategory && (line.BudgetSubCategoryID == Guid.Empty || line.BudgetSubCategoryID == null))
                    .ToList();
                dgvDirectLines.DataSource = null;
                dgvDirectLines.DataSource = directLinesForCategory;
                System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.BtnDirectAddLine_Click: dgvDirectLines refreshed (basic).");
            }
        }
    }
}
