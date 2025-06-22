using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HumanitarianProjectManagement.Models;

namespace HumanitarianProjectManagement
{
    public partial class BudgetTabUserControl : UserControl
    {
        private Project _currentProject;
        private BudgetCategoriesEnum? _selectedMainCategory;

        // Main input fields
        private TextBox txtDirectItem;
        private TextBox txtDirectDescription;
        private TextBox txtDirectUnit;
        private NumericUpDown numDirectQuantity;
        private NumericUpDown numDirectUnitCost;
        private NumericUpDown numDirectDuration;
        private NumericUpDown numDirectPercentageCBPF;
        private Button btnDirectAddLine;

        public BudgetTabUserControl()
        {
            InitializeComponent();

            // Ensure pnlMainBudgetContentArea is a FlowLayoutPanel for vertical stacking
            if (!(this.pnlMainBudgetContentArea is FlowLayoutPanel))
            {
                var flp = new FlowLayoutPanel
                {
                    Name = "pnlMainBudgetContentArea",
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    Padding = new Padding(0),
                    Margin = new Padding(0)
                };
                this.pnlBudgetMainArea.Controls.Remove(this.pnlMainBudgetContentArea);
                this.pnlBudgetMainArea.Controls.Add(flp);
                this.pnlMainBudgetContentArea = flp;
            }
        }

        public void LoadProject(Project project)
        {
            _currentProject = project;
            if (_currentProject == null)
            {
                ClearAndHideAll();
                InitializeMainCategoryButtons();
                return;
            }
            _currentProject.DetailedBudgetLines = _currentProject.DetailedBudgetLines ?? new BindingList<DetailedBudgetLine>();
            InitializeMainCategoryButtons();
            if (tlpCategoryButtons.Controls.Count > 0 && tlpCategoryButtons.Controls[0] is Button firstCatButton)
            {
                firstCatButton.PerformClick();
            }
            else { ClearAndHideAllContentArea(); }
        }

        private void InitializeMainCategoryButtons()
        {
            if (this.tlpCategoryButtons == null) return;
            this.tlpCategoryButtons.SuspendLayout();
            this.tlpCategoryButtons.Controls.Clear(); this.tlpCategoryButtons.RowStyles.Clear();
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
                    BackColor = Color.FromArgb(225, 225, 225),
                    ForeColor = Color.Black,
                    Cursor = Cursors.Hand
                };
                btnCategory.FlatAppearance.BorderSize = 0;
                btnCategory.Click += btnBudgetCategory_Internal_Click;
                this.tlpCategoryButtons.Controls.Add(btnCategory, 0, currentRow++);
            }
            this.tlpCategoryButtons.RowCount = currentRow; this.tlpCategoryButtons.ResumeLayout(true);
        }

        private string GetCategoryDisplayName(BudgetCategoriesEnum? category)
        {
            if (!category.HasValue) return "No Category Selected";
            string name = category.Value.ToString();
            if (name.Contains("_"))
            {
                var parts = name.Split('_');
                name = parts[0] + ". " + string.Join(" ", parts.Skip(1).Select(p => System.Text.RegularExpressions.Regex.Replace(p, "([a-z])([A-Z])", "$1 $2")));
            }
            else { name = System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2"); }
            return name;
        }

        private string GetCategoryPrefix(BudgetCategoriesEnum? category)
        {
            if (!category.HasValue) return "N/A";
            string name = category.Value.ToString();
            if (name.Contains("_")) return name.Split('_')[0];
            if (string.IsNullOrEmpty(name)) return "ERR";
            return name.Substring(0, 1);
        }

        private void btnBudgetCategory_Internal_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton == null || !(clickedButton.Tag is BudgetCategoriesEnum)) return;
            _selectedMainCategory = (BudgetCategoriesEnum?)clickedButton.Tag;
            foreach (Control c in this.tlpCategoryButtons.Controls) { if (c is Button btn) { btn.BackColor = Color.FromArgb(225, 225, 225); btn.ForeColor = Color.Black; } }
            clickedButton.BackColor = Color.FromArgb(0, 122, 204); clickedButton.ForeColor = Color.White;

            var flp = this.pnlMainBudgetContentArea as FlowLayoutPanel;
            if (flp == null) return;
            flp.SuspendLayout();
            flp.Controls.Clear();

            // 1. Category title at the very top
            Label lblCategoryHeader = new Label
            {
                Text = "Budget for: " + GetCategoryDisplayName(_selectedMainCategory),
                Font = new Font(this.Font.FontFamily, 13F, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(3, 8, 3, 8),
                Margin = new Padding(0, 6, 0, 6),
                ForeColor = Color.Black,
                BackColor = Color.Transparent // No gray background
            };
            flp.Controls.Add(lblCategoryHeader);

            // 2. Input row for adding top-level direct lines
            flp.Controls.Add(CreateInputRowForLine(null));

            // 3. Render all top-level lines for this category
            if (_currentProject != null && _selectedMainCategory.HasValue)
            {
                var topLines = _currentProject.DetailedBudgetLines
                    .Where(l => l.Category == _selectedMainCategory.Value && l.ParentDetailedBudgetLineID == null)
                    .OrderBy(l => l.Code)
                    .ToList();

                foreach (var line in topLines)
                {
                    RenderBudgetLineWithChildren(line, 0, flp);
                }
            }

            flp.ResumeLayout(true);
            flp.PerformLayout();
        }

        private TableLayoutPanel CreateInputRowForLine(DetailedBudgetLine parentLine)
        {
            TableLayoutPanel inputRow = new TableLayoutPanel
            {
                ColumnCount = 8,
                RowCount = 2,
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 5, 0, 10),
                Margin = new Padding(parentLine == null ? 0 : 20, 0, 0, 5),
                BackColor = Color.White
            };
            for (int i = 0; i < 7; i++) inputRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.0F));
            inputRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.0F));
            string[] directLabels = { "Item/Activity:", "Description:", "Unit:", "Quantity:", "Unit Cost:", "Duration:", "% CBPF:" };

            TextBox txtItem = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F), Margin = new Padding(2) };
            TextBox txtDescription = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F), Margin = new Padding(2) };
            TextBox txtUnit = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F), Margin = new Padding(2) };
            NumericUpDown numQuantity = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 2, Minimum = 0, Font = new Font("Segoe UI", 10F), Margin = new Padding(2) };
            NumericUpDown numUnitCost = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 2, Minimum = 0, Font = new Font("Segoe UI", 10F), Margin = new Padding(2) };
            NumericUpDown numDuration = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 1, Minimum = 0, Value = 1, Font = new Font("Segoe UI", 10F), Margin = new Padding(2) };
            NumericUpDown numPercentageCBPF = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 0, Minimum = 0, Maximum = 100, Value = 100, Font = new Font("Segoe UI", 10F), Margin = new Padding(2) };
            Control[] directInputs = { txtItem, txtDescription, txtUnit, numQuantity, numUnitCost, numDuration, numPercentageCBPF };
            for (int i = 0; i < directLabels.Length; i++)
            {
                inputRow.Controls.Add(new Label { Text = directLabels[i], AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 10F), Margin = new Padding(2) }, i, 0);
                inputRow.Controls.Add(directInputs[i], i, 1);
            }
            Button btnAddLine = new Button { Text = parentLine == null ? "Add Top-Level Direct Line" : "Add Sub-line", Dock = DockStyle.Fill, FlatStyle = FlatStyle.System, Height = 30, Font = new Font("Segoe UI", 10F), Margin = new Padding(2) };
            btnAddLine.Click += (s, e) =>
            {
                if (_currentProject == null || !_selectedMainCategory.HasValue) { MessageBox.Show("Project or Main Category not selected."); return; }
                if (string.IsNullOrWhiteSpace(txtItem.Text)) { MessageBox.Show("Item name required."); txtItem.Focus(); return; }
                if (numQuantity.Value <= 0) { MessageBox.Show("Quantity must be > 0."); numQuantity.Focus(); return; }

                DetailedBudgetLine newLine = new DetailedBudgetLine
                {
                    DetailedBudgetLineID = Guid.NewGuid(),
                    ProjectId = _currentProject.ProjectID,
                    Category = _selectedMainCategory.Value,
                    ItemName = txtItem.Text.Trim(),
                    Description = txtDescription.Text.Trim(),
                    Unit = txtUnit.Text.Trim(),
                    Quantity = numQuantity.Value,
                    UnitCost = numUnitCost.Value,
                    Duration = numDuration.Value,
                    PercentageChargedToCBPF = numPercentageCBPF.Value,
                    Code = parentLine == null ? GenerateDirectLineCode(_selectedMainCategory.Value) : GenerateChildLineCode(parentLine),
                    ParentDetailedBudgetLineID = parentLine?.DetailedBudgetLineID
                };
                RecalculateItemTotal(newLine);
                _currentProject.DetailedBudgetLines.Add(newLine);

                // Refresh view
                Button btnToRefresh = tlpCategoryButtons.Controls.OfType<Button>().FirstOrDefault(b => b.Tag is BudgetCategoriesEnum tag && tag == _selectedMainCategory.Value);
                if (btnToRefresh != null) btnBudgetCategory_Internal_Click(btnToRefresh, EventArgs.Empty);
            };
            inputRow.Controls.Add(btnAddLine, 7, 1);
            return inputRow;
        }

        private void RenderBudgetLineWithChildren(DetailedBudgetLine line, int indentLevel, FlowLayoutPanel flp)
        {
            TableLayoutPanel row = new TableLayoutPanel
            {
                ColumnCount = 10,
                RowCount = 1,
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(indentLevel * 20, 2, 0, 2),
                BackColor = indentLevel % 2 == 0 ? Color.White : Color.FromArgb(245, 245, 245)
            };

            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60)); // Code
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15)); // Item
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25)); // Description
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60)); // Unit
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60)); // Qty
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80)); // Unit Cost
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60)); // Duration
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60)); // % CBPF
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80)); // Total
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); // Actions

            row.Controls.Add(new Label { Text = line.Code, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F) }, 0, 0);
            row.Controls.Add(new Label { Text = line.ItemName, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F) }, 1, 0);
            row.Controls.Add(new Label { Text = line.Description, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F) }, 2, 0);
            row.Controls.Add(new Label { Text = line.Unit, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F) }, 3, 0);
            row.Controls.Add(new Label { Text = line.Quantity.ToString("N2"), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10F) }, 4, 0);
            row.Controls.Add(new Label { Text = line.UnitCost.ToString("C2"), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10F) }, 5, 0);
            row.Controls.Add(new Label { Text = line.Duration.ToString("N1"), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10F) }, 6, 0);
            row.Controls.Add(new Label { Text = line.PercentageChargedToCBPF.ToString("N0") + "%", AutoSize = true, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10F) }, 7, 0);
            row.Controls.Add(new Label { Text = line.TotalCost.ToString("C2"), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10F) }, 8, 0);

            // Actions
            FlowLayoutPanel actionsPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Fill };
            Button btnAddSubLine = new Button { Text = "+ Sub", Tag = line, AutoSize = true, Font = new Font("Segoe UI", 10F) };
            btnAddSubLine.Click += (s, e) =>
            {
                var inputRow = CreateInputRowForLine(line);
                flp.Controls.Add(inputRow);
                flp.Controls.SetChildIndex(inputRow, flp.Controls.GetChildIndex(row) + 1);
            };
            actionsPanel.Controls.Add(btnAddSubLine);
            // Add Edit/Delete buttons as needed
            row.Controls.Add(actionsPanel, 9, 0);

            flp.Controls.Add(row);

            // Recursively add children, if any
            var childLines = _currentProject.DetailedBudgetLines
                .Where(l => l.ParentDetailedBudgetLineID == line.DetailedBudgetLineID)
                .OrderBy(l => l.Code)
                .ToList();

            foreach (var child in childLines)
                RenderBudgetLineWithChildren(child, indentLevel + 1, flp);
        }

        private string GenerateDirectLineCode(BudgetCategoriesEnum mainCategory)
        {
            if (_currentProject == null) return GetCategoryPrefix(mainCategory) + ".ERR";
            string prefix = GetCategoryPrefix(mainCategory) + ".";
            int maxSuffix = _currentProject.DetailedBudgetLines
                .Where(line => line.Category == mainCategory && line.ParentDetailedBudgetLineID == null)
                .Select(line => { if (line.Code != null && line.Code.StartsWith(prefix) && !line.Code.Substring(prefix.Length).Contains(".")) { string s = line.Code.Substring(prefix.Length); return int.TryParse(s, out int n) ? n : 0; } return 0; })
                .DefaultIfEmpty(0).Max();
            return prefix + (maxSuffix + 1).ToString();
        }

        private string GenerateChildLineCode(DetailedBudgetLine parentLine)
        {
            if (_currentProject == null || parentLine == null) return "ERR.CODE";
            string basePrefix = parentLine.Code + ".";
            int maxSuffix = _currentProject.DetailedBudgetLines
                .Where(line => line.ParentDetailedBudgetLineID == parentLine.DetailedBudgetLineID && line.Code != null && line.Code.StartsWith(basePrefix))
                .Select(line => { string suffixPart = line.Code.Substring(basePrefix.Length); return int.TryParse(suffixPart, out int num) ? num : 0; })
                .DefaultIfEmpty(0).Max();
            return basePrefix + (maxSuffix + 1).ToString();
        }

        private void RecalculateItemTotal(DetailedBudgetLine item)
        {
            if (item == null) return;
            decimal pf = item.PercentageChargedToCBPF > 0 ? item.PercentageChargedToCBPF / 100M : 1M;
            item.TotalCost = item.Quantity * item.UnitCost * item.Duration * pf;
        }

        private void ClearAndHideAll()
        {
            ClearAndHideAllContentArea();
            if (this.tlpCategoryButtons != null)
                foreach (Control c in this.tlpCategoryButtons.Controls)
                    if (c is Button btn) { btn.BackColor = Color.FromArgb(225, 225, 225); btn.ForeColor = Color.Black; }
        }

        private void ClearAndHideAllContentArea()
        {
            if (this.pnlMainBudgetContentArea != null) this.pnlMainBudgetContentArea.Controls.Clear();
            _selectedMainCategory = null;
        }
    }
}
