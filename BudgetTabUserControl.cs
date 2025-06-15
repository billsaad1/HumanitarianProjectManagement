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
using System.Diagnostics; // Added for Debug.WriteLine

namespace HumanitarianProjectManagement
{
    public partial class BudgetTabUserControl : UserControl
    {
        private Project _currentProject;
        private BudgetCategoriesEnum _selectedMainCategory;
        private BudgetSubCategory _currentlySelectedSubCategory;
        private DetailedBudgetLine _currentlySelectedItemForDetailView;

        private Color defaultButtonBackColor = Color.FromArgb(225, 225, 225);
        private Color selectedButtonBackColor = Color.FromArgb(0, 122, 204);

        public BudgetTabUserControl()
        {
            InitializeComponent();
            InitializeItemizedDetailsDataGridView();

            this.btnAddNewSubCategory.Click += btnAddNewSubCategory_Click;
            this.btnAddNewItemizedDetail.Click += btnAddNewItemizedDetail_Click;
            this.dgvItemizedDetails.CellContentClick += dgvItemizedDetails_CellContentClick;
            this.dgvItemizedDetails.CellValueChanged += dgvItemizedDetails_CellValueChanged;
            this.dgvItemizedDetails.DataError += dgvItemizedDetails_DataError;
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
                    ClearAndHideAll(); // Clear UI if project is null
                    InitializeMainCategoryButtons(); // Still initialize buttons so UI doesn't look broken, just no data.
                    return; // Exit early if project is null
                }

                if (_currentProject.BudgetSubCategories == null)
                {
                    System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.LoadProject: Project.BudgetSubCategories is null, initializing.");
                    _currentProject.BudgetSubCategories = new BindingList<BudgetSubCategory>();
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
            this.btnAddNewSubCategory.Text = $"Add New Subcategory to {categoryDisplayPrefix}";
            this.btnAddNewSubCategory.Enabled = true;

            this.pnlItemizedDetailsHolder.Visible = false;
            _currentlySelectedItemForDetailView = null;
            _currentlySelectedSubCategory = null;

            RenderSubCategoriesView();
        }

        private void ClearAndHideAll()
        {
            System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.ClearAndHideAll: Method called.");
            if (this.pnlSubCategoryListArea != null) this.pnlSubCategoryListArea.Controls.Clear();
            if (this.pnlItemizedDetailsHolder != null) this.pnlItemizedDetailsHolder.Visible = false;
            if (this.btnAddNewSubCategory != null)
            {
                this.btnAddNewSubCategory.Enabled = false;
                this.btnAddNewSubCategory.Text = "Add New Subcategory";
            }
            _currentlySelectedItemForDetailView = null;
            _currentlySelectedSubCategory = null;
            // _selectedMainCategory = default(BudgetCategoriesEnum); // Resetting this might not be desired if we want to keep the last selection context

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

        private void InitializeItemizedDetailsDataGridView()
        {
            dgvItemizedDetails.AutoGenerateColumns = false;
            dgvItemizedDetails.AllowUserToAddRows = false;
            dgvItemizedDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvItemizedDetails.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);

            var colItemDescDGV = new DataGridViewTextBoxColumn { Name = "ItemDescription", HeaderText = "Item Description", DataPropertyName = "Description", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 40, MinimumWidth = 150 };
            var colItemQtyDGV = new DataGridViewTextBoxColumn { Name = "ItemQuantity", HeaderText = "Quantity", DataPropertyName = "Quantity", Width = 70, MinimumWidth = 50, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } };
            var colItemUnitPriceDGV = new DataGridViewTextBoxColumn { Name = "ItemUnitPrice", HeaderText = "Unit Price", DataPropertyName = "UnitPrice", Width = 90, MinimumWidth = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight } };
            var colItemTotalDGV = new DataGridViewTextBoxColumn { Name = "ItemTotalCost", HeaderText = "Total Cost", DataPropertyName = "TotalCost", ReadOnly = true, Width = 100, MinimumWidth = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightGray } };
            var colDeleteItemDGV = new DataGridViewButtonColumn { Name = "DeleteItemDetail", HeaderText = "Action", Width = 70, Text = "Delete", UseColumnTextForButtonValue = true, FlatStyle = FlatStyle.Popup };

            dgvItemizedDetails.Columns.Clear();
            dgvItemizedDetails.Columns.AddRange(colItemDescDGV, colItemQtyDGV, colItemUnitPriceDGV, colItemTotalDGV, colDeleteItemDGV);
        }

        private void RenderSubCategoriesView()
        {
            System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.RenderSubCategoriesView: Called for main category {_selectedMainCategory}.");
            if (this.pnlSubCategoryListArea == null)
            {
                System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.RenderSubCategoriesView: pnlSubCategoryListArea is null. Cannot render.");
                return;
            }

            this.pnlSubCategoryListArea.SuspendLayout();
            this.pnlSubCategoryListArea.Controls.Clear();

            if (_currentProject == null || _currentProject.BudgetSubCategories == null)
            {
                System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.RenderSubCategoriesView: _currentProject or BudgetSubCategories is null. No subcategories to render.");
                this.pnlSubCategoryListArea.ResumeLayout(true);
                return;
            }

            var subCategoriesForMain = _currentProject.BudgetSubCategories
                                            .Where(sc => sc.MainCategory == _selectedMainCategory)
                                            .OrderBy(sc => sc.SubCategoryCodeSuffix)
                                            .ToList();

            System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.RenderSubCategoriesView: Found {subCategoriesForMain.Count} subcategories for {_selectedMainCategory}.");

            foreach (var subCat in subCategoriesForMain)
            {
                Panel subCatPanel = CreateSubCategoryPanel(subCat);
                this.pnlSubCategoryListArea.Controls.Add(subCatPanel);
            }
            this.pnlSubCategoryListArea.ResumeLayout(true);
            System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.RenderSubCategoriesView: Finished rendering subcategories.");
        }

        private Panel CreateSubCategoryPanel(BudgetSubCategory subCategory)
        {
            Panel pnlSubView = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 5, 0, 5),
                Padding = new Padding(10),
                Name = $"pnlSubView_{subCategory.BudgetSubCategoryID}"
            };

            TableLayoutPanel tlpHeader = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 6,
                RowCount = 1,
                Padding = new Padding(0, 0, 0, 5)
            };
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            Label lblSubHeader = new Label
            {
                Text = $"{GetCategoryPrefix(_selectedMainCategory)}.{subCategory.SubCategoryCodeSuffix} - {subCategory.SubCategoryName}",
                Font = new Font(this.Font.FontFamily, 10F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Name = $"lblSubHeader_{subCategory.BudgetSubCategoryID}"
            };
            tlpHeader.Controls.Add(lblSubHeader, 0, 0);

            Label lblSubCategoryTotal = new Label
            {
                Name = $"lblSubCategoryTotal_{subCategory.BudgetSubCategoryID}",
                Text = "Total: " + CalculateSubCategoryTotal(subCategory).ToString("C", CultureInfo.InvariantCulture), // Retain CultureInfo for ToString
                Font = new Font(this.Font.FontFamily, 9F, FontStyle.Italic),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 10, 0)
            };
            tlpHeader.Controls.Add(lblSubCategoryTotal, 1, 0);

            Button btnAddItemToSub = new Button { Text = "Add Item", Tag = subCategory, AutoSize = true, Margin = new Padding(3, 0, 3, 0) };
            btnAddItemToSub.Click += btnAddItemToSub_Click;
            tlpHeader.Controls.Add(btnAddItemToSub, 2, 0);

            Button btnEditSub = new Button { Text = "Edit Sub", Tag = subCategory, AutoSize = true, Margin = new Padding(3, 0, 3, 0) };
            btnEditSub.Click += btnEditSubCategory_Click;
            tlpHeader.Controls.Add(btnEditSub, 3, 0);

            Button btnDeleteSub = new Button { Text = "Delete Sub", Tag = subCategory, AutoSize = true, Margin = new Padding(3, 0, 3, 0) };
            btnDeleteSub.Click += btnDeleteSubCategory_Click;
            tlpHeader.Controls.Add(btnDeleteSub, 4, 0);

            Panel pnlItemsArea = new Panel { Name = $"pnlItemsArea_{subCategory.BudgetSubCategoryID}", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Visible = false, Padding = new Padding(20, 5, 0, 5), MinimumSize = new Size(0, 50) };
            Button btnToggleItems = new Button { Text = "Show Items (+)", Tag = pnlItemsArea, AutoSize = true, Margin = new Padding(3, 0, 3, 0) };
            btnToggleItems.Click += btnToggleSubCategoryItems_Click;
            tlpHeader.Controls.Add(btnToggleItems, 5, 0);

            DataGridView dgvItems = new DataGridView
            {
                Name = $"dgvItems_{subCategory.BudgetSubCategoryID}",
                Dock = DockStyle.Top,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 9F, FontStyle.Regular), BackColor = Color.LightSteelBlue },
                Height = 150,
                Tag = subCategory
            };

            var colCode = new DataGridViewTextBoxColumn { Name = "ItemCode", HeaderText = "Code", DataPropertyName = "Code", ReadOnly = true, Width = 70, Frozen = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None };
            var colDesc = new DataGridViewTextBoxColumn { Name = "ItemDescription", HeaderText = "Description", DataPropertyName = "Description", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 35, MinimumWidth = 150 };
            var colUnit = new DataGridViewTextBoxColumn { Name = "ItemUnit", HeaderText = "Unit", DataPropertyName = "Unit", Width = 60 };
            var colQty = new DataGridViewTextBoxColumn { Name = "ItemQuantity", HeaderText = "Qty", DataPropertyName = "Quantity", Width = 60, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } };
            var colUnitCost = new DataGridViewTextBoxColumn { Name = "ItemUnitCost", HeaderText = "Unit Cost", DataPropertyName = "UnitCost", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight } };
            var colDuration = new DataGridViewTextBoxColumn { Name = "ItemDuration", HeaderText = "Duration", DataPropertyName = "Duration", Width = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } };
            var colCBPF = new DataGridViewTextBoxColumn { Name = "ItemCBPF", HeaderText = "% CBPF", DataPropertyName = "PercentageChargedToCBPF", Width = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "P0", Alignment = DataGridViewContentAlignment.MiddleRight } };
            var colTotal = new DataGridViewTextBoxColumn { Name = "ItemTotalCost", HeaderText = "Total", DataPropertyName = "TotalCost", ReadOnly = true, Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightGray } };

            var colEdit = new DataGridViewButtonColumn { Name = "EditItemButton", HeaderText = "Edit", Text = "Edit", UseColumnTextForButtonValue = true, Width = 60, FlatStyle = FlatStyle.Popup };
            var colDelete = new DataGridViewButtonColumn { Name = "DeleteItemButton", HeaderText = "Delete", Text = "Delete", UseColumnTextForButtonValue = true, Width = 60, FlatStyle = FlatStyle.Popup };
            var colExpand = new DataGridViewButtonColumn { Name = "ExpandItemizedButton", HeaderText = "Details", Text = "+", UseColumnTextForButtonValue = true, Width = 60, FlatStyle = FlatStyle.System };

            dgvItems.Columns.AddRange(colCode, colDesc, colUnit, colQty, colUnitCost, colDuration, colCBPF, colTotal, colEdit, colDelete, colExpand);

            dgvItems.CellContentClick += dgvSubCategoryItems_CellContentClick;
            dgvItems.CellValueChanged += dgvSubCategoryItems_CellValueChanged;
            dgvItems.DataError += dgvSubCategoryItems_DataError;

            dgvItems.DataSource = subCategory.DetailedBudgetLines;
            pnlItemsArea.Controls.Add(dgvItems);

            pnlSubView.Controls.Add(pnlItemsArea);
            pnlSubView.Controls.Add(tlpHeader);

            return pnlSubView;
        }

        private string GetCategoryPrefix(BudgetCategoriesEnum category)
        {
            string name = category.ToString();
            if (name.Length > 2 && name[1] == '_') { return name.Substring(0, 1); }
            return name.Substring(0, Math.Min(1, name.Length));
        }

        private string GenerateNewSubCategorySuffix(BudgetCategoriesEnum mainCat)
        {
            if (_currentProject == null || _currentProject.BudgetSubCategories == null) return "1";
            int maxSuffix = 0;
            var existingSuffixes = _currentProject.BudgetSubCategories
                                     .Where(sc => sc.MainCategory == mainCat)
                                     .Select(sc => {
                                         int.TryParse(sc.SubCategoryCodeSuffix, out int val);
                                         return val;
                                     });
            if (existingSuffixes.Any()) maxSuffix = existingSuffixes.Max();
            return (maxSuffix + 1).ToString();
        }

        private string GenerateNewItemCode(BudgetSubCategory parentSub)
        {
            if (parentSub == null || parentSub.DetailedBudgetLines == null) return $"{GetCategoryPrefix(parentSub.MainCategory)}.{parentSub.SubCategoryCodeSuffix}.1";
            int itemCount = parentSub.DetailedBudgetLines.Count;
            return $"{GetCategoryPrefix(parentSub.MainCategory)}.{parentSub.SubCategoryCodeSuffix}.{itemCount + 1}";
        }


        private void btnAddNewSubCategory_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Add New Subcategory to {_selectedMainCategory} - Placeholder for SubCategoryEntryForm.");
            if (_currentProject == null)
            {
                System.Diagnostics.Debug.WriteLine("btnAddNewSubCategory_Click: _currentProject is null. Cannot add subcategory.");
                MessageBox.Show("Current project is not loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            BudgetSubCategory testSub = new BudgetSubCategory
            {
                ProjectId = _currentProject.ProjectID,
                MainCategory = _selectedMainCategory,
                SubCategoryCodeSuffix = GenerateNewSubCategorySuffix(_selectedMainCategory),
                SubCategoryName = "Test Subcategory " + DateTime.Now.Second
            };
            _currentProject.BudgetSubCategories.Add(testSub);
            RenderSubCategoriesView();
        }

        private void btnAddItemToSub_Click(object sender, EventArgs e)
        {
            BudgetSubCategory parentSub = (sender as Button)?.Tag as BudgetSubCategory;
            if (parentSub == null)
            {
                System.Diagnostics.Debug.WriteLine("btnAddItemToSub_Click: parentSub is null. Cannot add item.");
                return;
            }
            _currentlySelectedSubCategory = parentSub;

            MessageBox.Show($"Add Item to {parentSub.SubCategoryName} - Placeholder for DetailedBudgetLineForm.");
            if (_currentProject == null)
            {
                System.Diagnostics.Debug.WriteLine("btnAddItemToSub_Click: _currentProject is null. Cannot add item.");
                MessageBox.Show("Current project is not loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DetailedBudgetLine testItem = new DetailedBudgetLine
            {
                BudgetSubCategoryID = parentSub.BudgetSubCategoryID,
                ProjectId = _currentProject.ProjectID,
                Category = parentSub.MainCategory,
                Code = GenerateNewItemCode(parentSub),
                Description = "Test Item " + DateTime.Now.Millisecond,
                Quantity = 1,
                UnitCost = 10,
                Duration = 1,
                PercentageChargedToCBPF = 100
            };
            RecalculateItemTotal(testItem);
            parentSub.DetailedBudgetLines.Add(testItem);
            RecalculateSubCategoryTotal(parentSub);
        }

        private void btnEditSubCategory_Click(object sender, EventArgs e)
        {
            BudgetSubCategory subToEdit = (sender as Button)?.Tag as BudgetSubCategory;
            if (subToEdit == null) return;
            MessageBox.Show($"Edit Subcategory: {subToEdit.SubCategoryName} - To be implemented.");
        }

        private void btnDeleteSubCategory_Click(object sender, EventArgs e)
        {
            BudgetSubCategory subToDelete = (sender as Button)?.Tag as BudgetSubCategory;
            if (subToDelete == null) return;
            if (_currentProject == null || _currentProject.BudgetSubCategories == null)
            {
                System.Diagnostics.Debug.WriteLine("btnDeleteSubCategory_Click: _currentProject or its BudgetSubCategories is null.");
                return;
            }
            if (MessageBox.Show($"Delete subcategory '{subToDelete.SubCategoryName}' and all its items?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _currentProject.BudgetSubCategories.Remove(subToDelete);
                RenderSubCategoriesView();
            }
        }

        private void btnToggleSubCategoryItems_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Panel itemsPanel = btn?.Tag as Panel;
            if (itemsPanel != null)
            {
                itemsPanel.Visible = !itemsPanel.Visible;
                btn.Text = itemsPanel.Visible ? "Hide Items (-)" : "Show Items (+)";
            }
        }

        private void dgvSubCategoryItems_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridView dgv = sender as DataGridView;
            if (dgv == null) return;

            BudgetSubCategory parentSubCategory = dgv.Tag as BudgetSubCategory;
            if (parentSubCategory == null || e.RowIndex >= parentSubCategory.DetailedBudgetLines.Count) return;

            DetailedBudgetLine lineClicked = parentSubCategory.DetailedBudgetLines[e.RowIndex];

            if (dgv.Columns[e.ColumnIndex].Name == "EditItemButton")
            {
                MessageBox.Show($"Edit item: {lineClicked.Description} - To be implemented.");
            }
            else if (dgv.Columns[e.ColumnIndex].Name == "DeleteItemButton")
            {
                if (MessageBox.Show($"Delete item '{lineClicked.Description}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    parentSubCategory.DetailedBudgetLines.Remove(lineClicked);
                    RecalculateSubCategoryTotal(parentSubCategory);
                }
            }
            else if (dgv.Columns[e.ColumnIndex].Name == "ExpandItemizedButton")
            {
                DataGridViewButtonCell cell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewButtonCell;
                if (_currentlySelectedItemForDetailView == lineClicked && pnlItemizedDetailsHolder.Visible)
                {
                    ShowItemizedDetailsView(null);
                    if (cell != null) cell.Value = "+";
                }
                else
                {
                    ShowItemizedDetailsView(lineClicked);
                    if (cell != null) cell.Value = "-";
                }
            }
        }

        private void dgvSubCategoryItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            DataGridView dgv = sender as DataGridView;
            if (dgv == null) return;
            BudgetSubCategory parentSubCategory = dgv.Tag as BudgetSubCategory;
            if (parentSubCategory == null || e.RowIndex >= parentSubCategory.DetailedBudgetLines.Count) return;

            DetailedBudgetLine changedLine = parentSubCategory.DetailedBudgetLines[e.RowIndex];
            RecalculateItemTotal(changedLine);
            dgv.InvalidateRow(e.RowIndex);
            RecalculateSubCategoryTotal(parentSubCategory);
        }

        private void dgvSubCategoryItems_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.dgvSubCategoryItems_DataError: Row {e.RowIndex}, Col {e.ColumnIndex}. Exception: {e.Exception.ToString()}");
            MessageBox.Show($"Data error in subcategory items grid: {e.Exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            e.ThrowException = false;
        }


        private void RecalculateItemTotal(DetailedBudgetLine item)
        {
            if (item == null) return;
            decimal percentage = item.PercentageChargedToCBPF > 0 ? item.PercentageChargedToCBPF / 100M : 1M;
            item.TotalCost = item.Quantity * item.UnitCost * item.Duration * percentage;
        }

        private decimal CalculateSubCategoryTotal(BudgetSubCategory subCategory)
        {
            if (subCategory == null || subCategory.DetailedBudgetLines == null) return 0;
            return subCategory.DetailedBudgetLines.Sum(line => line.TotalCost);
        }

        private void RecalculateSubCategoryTotal(BudgetSubCategory subCategory)
        {
            if (subCategory == null) return;
            decimal total = CalculateSubCategoryTotal(subCategory);

            Control[] foundControls = this.pnlSubCategoryListArea.Controls.Find($"pnlSubView_{subCategory.BudgetSubCategoryID}", false);
            if (foundControls.Length > 0 && foundControls[0] is Panel pnlSubView)
            {
                Control[] lblControls = pnlSubView.Controls.Find($"lblSubCategoryTotal_{subCategory.BudgetSubCategoryID}", true);
                if (lblControls.Length > 0 && lblControls[0] is Label lblTotal)
                {
                    lblTotal.Text = "Total: " + total.ToString("C", CultureInfo.InvariantCulture); // Retain CultureInfo for ToString
                }
            }
        }

        private void ShowItemizedDetailsView(DetailedBudgetLine line)
        {
            System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.ShowItemizedDetailsView: Called. Line is {(line == null ? "null" : "not null, ID: " + line.DetailedBudgetLineID)}");
            _currentlySelectedItemForDetailView = line;
            if (line == null)
            {
                if (pnlItemizedDetailsHolder != null) pnlItemizedDetailsHolder.Visible = false;
                System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.ShowItemizedDetailsView: Hiding itemized details panel.");
                return;
            }
            if (lblItemizedDetailsHeader != null) lblItemizedDetailsHeader.Text = $"Itemized Details for: {(line.Code ?? "N/A")} - {(line.Description ?? "N/A")}";

            line.ItemizedDetails = line.ItemizedDetails ?? new BindingList<ItemizedBudgetDetail>();
            if (dgvItemizedDetails != null)
            {
                dgvItemizedDetails.DataSource = null;
                dgvItemizedDetails.DataSource = line.ItemizedDetails;
                System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.ShowItemizedDetailsView: dgvItemizedDetails DataSource set. Item count: {line.ItemizedDetails.Count}");
            }
            if (pnlItemizedDetailsHolder != null)
            {
                pnlItemizedDetailsHolder.Visible = true;
                System.Diagnostics.Debug.WriteLine("BudgetTabUserControl.ShowItemizedDetailsView: pnlItemizedDetailsHolder set to visible.");
            }
        }

        private void btnAddNewItemizedDetail_Click(object sender, EventArgs e)
        {
            if (_currentlySelectedItemForDetailView == null)
            {
                MessageBox.Show("Please select a main budget item first to add itemized details.", "No Item Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _currentlySelectedItemForDetailView.ItemizedDetails = _currentlySelectedItemForDetailView.ItemizedDetails ?? new BindingList<ItemizedBudgetDetail>();
            var newItem = new ItemizedBudgetDetail
            {
                ItemizedBudgetDetailID = Guid.NewGuid(),
                ParentBudgetLineID = _currentlySelectedItemForDetailView.DetailedBudgetLineID
            };
            _currentlySelectedItemForDetailView.ItemizedDetails.Add(newItem);
            System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.btnAddNewItemizedDetail_Click: Added new itemized detail to line ID {_currentlySelectedItemForDetailView.DetailedBudgetLineID}.");
            UpdateParentItemTotalCost(_currentlySelectedItemForDetailView);
        }

        private void dgvItemizedDetails_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _currentlySelectedItemForDetailView == null) return;

            DataGridView dgv = sender as DataGridView;
            if (dgv == null || e.RowIndex >= _currentlySelectedItemForDetailView.ItemizedDetails.Count) return;

            if (dgv.Columns[e.ColumnIndex].Name == "DeleteItemDetail")
            {
                ItemizedBudgetDetail detailToDelete = dgv.Rows[e.RowIndex].DataBoundItem as ItemizedBudgetDetail;
                if (detailToDelete != null)
                {
                    if (MessageBox.Show($"Delete itemized detail: '{detailToDelete.Description}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        _currentlySelectedItemForDetailView.ItemizedDetails.Remove(detailToDelete);
                        System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.dgvItemizedDetails_CellContentClick: Deleted itemized detail ID {detailToDelete.ItemizedBudgetDetailID}.");
                        UpdateParentItemTotalCost(_currentlySelectedItemForDetailView);
                    }
                }
            }
        }

        private void dgvItemizedDetails_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _currentlySelectedItemForDetailView == null) return;

            DataGridView dgv = sender as DataGridView;
            if (dgv == null || e.RowIndex >= _currentlySelectedItemForDetailView.ItemizedDetails.Count) return;

            ItemizedBudgetDetail changedDetail = dgv.Rows[e.RowIndex].DataBoundItem as ItemizedBudgetDetail;
            if (changedDetail != null)
            {
                string columnName = dgv.Columns[e.ColumnIndex].Name;
                if (columnName == "ItemQuantity" || columnName == "ItemUnitPrice" || columnName == "Description")
                {
                    changedDetail.UpdateTotalCost();
                    dgv.InvalidateCell(dgv.Columns["ItemTotalCost"].Index, e.RowIndex);
                    System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.dgvItemizedDetails_CellValueChanged: Cell value changed for itemized detail ID {changedDetail.ItemizedBudgetDetailID}. Column: {columnName}.");
                    UpdateParentItemTotalCost(_currentlySelectedItemForDetailView);
                }
            }
        }

        private void dgvItemizedDetails_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.dgvItemizedDetails_DataError: Row {e.RowIndex}, Col {e.ColumnIndex}. Exception: {e.Exception.ToString()}");
            MessageBox.Show($"Data error in itemized details: {e.Exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            e.Cancel = true;
        }

        private void UpdateParentItemTotalCost(DetailedBudgetLine parentItem)
        {
            if (parentItem == null) return;
            System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.UpdateParentItemTotalCost: Called for parent item ID {parentItem.DetailedBudgetLineID}.");

            if (parentItem.ItemizedDetails != null && parentItem.ItemizedDetails.Any())
            {
                parentItem.TotalCost = parentItem.ItemizedDetails.Sum(d => d.TotalCost);
                System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.UpdateParentItemTotalCost: Calculated total from itemized details: {parentItem.TotalCost}.");
            }
            else
            {
                RecalculateItemTotal(parentItem); // Use the item's own fields if no itemized details
                System.Diagnostics.Debug.WriteLine($"BudgetTabUserControl.UpdateParentItemTotalCost: Calculated total from item's own fields: {parentItem.TotalCost}.");
            }

            if (parentItem.BudgetSubCategoryID != Guid.Empty && _currentProject != null)
            {
                BudgetSubCategory ownerSubCategory = _currentProject.BudgetSubCategories.FirstOrDefault(sc => sc.BudgetSubCategoryID == parentItem.BudgetSubCategoryID);
                if (ownerSubCategory != null)
                {
                    Control[] pnlSubViewControls = this.pnlSubCategoryListArea.Controls.Find($"pnlSubView_{ownerSubCategory.BudgetSubCategoryID}", false);
                    if (pnlSubViewControls.Length > 0 && pnlSubViewControls[0] is Panel pnlSubView)
                    {
                        Control[] dgvItemsControls = pnlSubView.Controls.Find($"dgvItems_{ownerSubCategory.BudgetSubCategoryID}", true);
                        if (dgvItemsControls.Length > 0 && dgvItemsControls[0] is DataGridView dgvItems)
                        {
                            for (int i = 0; i < dgvItems.Rows.Count; i++)
                            {
                                if (dgvItems.Rows[i].DataBoundItem == parentItem)
                                {
                                    dgvItems.InvalidateRow(i);
                                    break;
                                }
                            }
                        }
                    }
                    RecalculateSubCategoryTotal(ownerSubCategory);
                }
            }
        }
    }
}


