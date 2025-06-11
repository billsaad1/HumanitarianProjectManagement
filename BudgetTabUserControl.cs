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
            _currentProject = project;
            if (_currentProject != null && _currentProject.BudgetSubCategories == null)
            {
                _currentProject.BudgetSubCategories = new BindingList<BudgetSubCategory>();
            }
            
            InitializeMainCategoryButtons();

            if (tlpCategoryButtons.Controls.Count > 0 && tlpCategoryButtons.Controls[0] is Button firstCatButton)
            {
                firstCatButton.PerformClick();
            }
            else
            {
                ClearAndHideAll(); 
            }
        }

        private void InitializeMainCategoryButtons()
        {
            if (this.tlpCategoryButtons == null) return;

            this.tlpCategoryButtons.SuspendLayout();
            this.tlpCategoryButtons.Controls.Clear();
            this.tlpCategoryButtons.RowStyles.Clear();
            // RowCount will be set dynamically by adding rows.
            // this.tlpCategoryButtons.RowCount = Enum.GetValues(typeof(BudgetCategoriesEnum)).Length; 

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
            this.tlpCategoryButtons.RowCount = currentRow; // Adjust row count to actual number of buttons
            this.tlpCategoryButtons.ResumeLayout(true);
        }
        
        private string GetCategoryDisplayName(BudgetCategoriesEnum category)
        {
            string name = category.ToString();
            if (name.Length > 2 && name[1] == '_')
            {
                // Format like "A. Staff And Personnel"
                name = name[0] + ". " + System.Text.RegularExpressions.Regex.Replace(name.Substring(2), "([a-z])([A-Z])", "$1 $2");
            }
            else
            {
                 // Format like "Other Direct Costs"
                name = System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
            }
            return name;
        }

        private void btnBudgetCategory_Internal_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton == null || !(clickedButton.Tag is BudgetCategoriesEnum))
            {
                return;
            }

            _selectedMainCategory = (BudgetCategoriesEnum)clickedButton.Tag;

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
            if (this.pnlSubCategoryListArea != null) this.pnlSubCategoryListArea.Controls.Clear();
            if (this.pnlItemizedDetailsHolder != null) this.pnlItemizedDetailsHolder.Visible = false;
            if (this.btnAddNewSubCategory != null) 
            {
                this.btnAddNewSubCategory.Enabled = false;
                this.btnAddNewSubCategory.Text = "Add New Subcategory";
            }
            _currentlySelectedItemForDetailView = null;
            _currentlySelectedSubCategory = null;
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
        }
        
        private void InitializeItemizedDetailsDataGridView()
        {
           dgvItemizedDetails.AutoGenerateColumns = false;
           dgvItemizedDetails.AllowUserToAddRows = false;
           dgvItemizedDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
           dgvItemizedDetails.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);


           var colItemDescDGV = new DataGridViewTextBoxColumn { Name = "ItemDescription", HeaderText = "Item Description", DataPropertyName = "Description", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 40, MinimumWidth = 150 };
           var colItemQtyDGV = new DataGridViewTextBoxColumn { Name = "ItemQuantity", HeaderText = "Quantity", DataPropertyName = "Quantity", Width = 70, MinimumWidth = 50, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } };
           var colItemUnitPriceDGV = new DataGridViewTextBoxColumn { Name = "ItemUnitPrice", HeaderText = "Unit Price", DataPropertyName = "UnitPrice", Width = 90, MinimumWidth = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", CultureInfo = CultureInfo.InvariantCulture, Alignment = DataGridViewContentAlignment.MiddleRight } };
           var colItemTotalDGV = new DataGridViewTextBoxColumn { Name = "ItemTotalCost", HeaderText = "Total Cost", DataPropertyName = "TotalCost", ReadOnly = true, Width = 100, MinimumWidth = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", CultureInfo = CultureInfo.InvariantCulture, Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightGray } };
           var colDeleteItemDGV = new DataGridViewButtonColumn { Name = "DeleteItemDetail", HeaderText = "Action", Width = 70, Text = "Delete", UseColumnTextForButtonValue = true, FlatStyle = FlatStyle.Popup };
           
           dgvItemizedDetails.Columns.Clear(); // Ensure no pre-existing columns from designer if any
           dgvItemizedDetails.Columns.AddRange(colItemDescDGV, colItemQtyDGV, colItemUnitPriceDGV, colItemTotalDGV, colDeleteItemDGV);
        }

        private void RenderSubCategoriesView()
        {
            if (this.pnlSubCategoryListArea == null) return;

            this.pnlSubCategoryListArea.SuspendLayout();
            this.pnlSubCategoryListArea.Controls.Clear();

            if (_currentProject == null || _currentProject.BudgetSubCategories == null)
            {
                this.pnlSubCategoryListArea.ResumeLayout(true);
                return;
            }

            var subCategoriesForMain = _currentProject.BudgetSubCategories
                                            .Where(sc => sc.MainCategory == _selectedMainCategory)
                                            .OrderBy(sc => sc.SubCategoryCodeSuffix) 
                                            .ToList();

            foreach (var subCat in subCategoriesForMain)
            {
                Panel subCatPanel = CreateSubCategoryPanel(subCat);
                this.pnlSubCategoryListArea.Controls.Add(subCatPanel);
            }
            this.pnlSubCategoryListArea.ResumeLayout(true);
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
                ColumnCount = 6, // Label, Total, Add Item, Edit Sub, Delete Sub, Toggle 
                RowCount = 1,
                Padding = new Padding(0,0,0,5)
            };
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); // Label
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // Total Label
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
                Text = "Total: " + CalculateSubCategoryTotal(subCategory).ToString("C", CultureInfo.InvariantCulture),
                Font = new Font(this.Font.FontFamily, 9F, FontStyle.Italic),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0,0,10,0) // Add some padding to the right
            };
            tlpHeader.Controls.Add(lblSubCategoryTotal, 1, 0);


            Button btnAddItemToSub = new Button { Text = "Add Item", Tag = subCategory, AutoSize = true, Margin = new Padding(3,0,3,0) }; 
            btnAddItemToSub.Click += btnAddItemToSub_Click;
            tlpHeader.Controls.Add(btnAddItemToSub, 2, 0);

            Button btnEditSub = new Button { Text = "Edit Sub", Tag = subCategory, AutoSize = true, Margin = new Padding(3,0,3,0) }; 
            btnEditSub.Click += btnEditSubCategory_Click;
            tlpHeader.Controls.Add(btnEditSub, 3, 0);

            Button btnDeleteSub = new Button { Text = "Delete Sub", Tag = subCategory, AutoSize = true, Margin = new Padding(3,0,3,0) }; 
            btnDeleteSub.Click += btnDeleteSubCategory_Click;
            tlpHeader.Controls.Add(btnDeleteSub, 4, 0);
            
            Panel pnlItemsArea = new Panel { Name = $"pnlItemsArea_{subCategory.BudgetSubCategoryID}", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Visible = false, Padding = new Padding(20, 5, 0, 5), MinimumSize = new Size(0,50) };
            Button btnToggleItems = new Button { Text = "Show Items (+)", Tag = pnlItemsArea, AutoSize = true, Margin = new Padding(3,0,3,0) }; 
            btnToggleItems.Click += btnToggleSubCategoryItems_Click;
            tlpHeader.Controls.Add(btnToggleItems, 5, 0);
            
            // Dynamically create DataGridView for items
            DataGridView dgvItems = new DataGridView
            {
                Name = $"dgvItems_{subCategory.BudgetSubCategoryID}",
                Dock = DockStyle.Top,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 9F, FontStyle.Regular), BackColor = Color.LightSteelBlue },
                Height = 150, // Default height, can be adjusted
                Tag = subCategory // Store subCategory for context in event handlers
            };

            var colCode = new DataGridViewTextBoxColumn { Name = "ItemCode", HeaderText = "Code", DataPropertyName = "Code", ReadOnly = true, Width = 70, Frozen = true };
            var colDesc = new DataGridViewTextBoxColumn { Name = "ItemDescription", HeaderText = "Description", DataPropertyName = "Description", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 35, MinimumWidth = 150 };
            var colUnit = new DataGridViewTextBoxColumn { Name = "ItemUnit", HeaderText = "Unit", DataPropertyName = "Unit", Width = 60 };
            var colQty = new DataGridViewTextBoxColumn { Name = "ItemQuantity", HeaderText = "Qty", DataPropertyName = "Quantity", Width = 60, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } };
            var colUnitCost = new DataGridViewTextBoxColumn { Name = "ItemUnitCost", HeaderText = "Unit Cost", DataPropertyName = "UnitCost", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", CultureInfo = CultureInfo.InvariantCulture, Alignment = DataGridViewContentAlignment.MiddleRight } };
            var colDuration = new DataGridViewTextBoxColumn { Name = "ItemDuration", HeaderText = "Duration", DataPropertyName = "Duration", Width = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } };
            var colCBPF = new DataGridViewTextBoxColumn { Name = "ItemCBPF", HeaderText = "% CBPF", DataPropertyName = "PercentageChargedToCBPF", Width = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "P0", Alignment = DataGridViewContentAlignment.MiddleRight } }; // P0 for percentage
            var colTotal = new DataGridViewTextBoxColumn { Name = "ItemTotalCost", HeaderText = "Total", DataPropertyName = "TotalCost", ReadOnly = true, Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", CultureInfo = CultureInfo.InvariantCulture, Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightGray } };
            
            var colEdit = new DataGridViewButtonColumn { Name = "EditItemButton", HeaderText = "Edit", Text = "Edit", UseColumnTextForButtonValue = true, Width = 60, FlatStyle = FlatStyle.Popup };
            var colDelete = new DataGridViewButtonColumn { Name = "DeleteItemButton", HeaderText = "Delete", Text = "Delete", UseColumnTextForButtonValue = true, Width = 60, FlatStyle = FlatStyle.Popup };
            var colExpand = new DataGridViewButtonColumn { Name = "ExpandItemizedButton", HeaderText = "Details", Text = "+", UseColumnTextForButtonValue = true, Width = 60, FlatStyle = FlatStyle.System };

            dgvItems.Columns.AddRange(colCode, colDesc, colUnit, colQty, colUnitCost, colDuration, colCBPF, colTotal, colEdit, colDelete, colExpand);
            
            dgvItems.CellContentClick += dgvSubCategoryItems_CellContentClick;
            dgvItems.CellValueChanged += dgvSubCategoryItems_CellValueChanged;
            dgvItems.DataError += dgvSubCategoryItems_DataError;

            dgvItems.DataSource = subCategory.DetailedBudgetLines; // Bind to the subcategory's items
            pnlItemsArea.Controls.Add(dgvItems); // Add DGV to its panel

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
            // Replace with actual SubCategoryEntryForm invocation
            // using (SubCategoryEntryForm entryForm = new SubCategoryEntryForm())
            // {
            //     if (entryForm.ShowDialog(this) == DialogResult.OK)
            //     {
            //         BudgetSubCategory newSubCategory = new BudgetSubCategory
            //         {
            //             BudgetSubCategoryID = Guid.NewGuid(),
            //             ProjectId = _currentProject.ProjectID,
            //             MainCategory = _selectedMainCategory,
            //             SubCategoryCodeSuffix = GenerateNewSubCategorySuffix(_selectedMainCategory), // Or from entryForm.SubCategoryCodeSuffix
            //             SubCategoryName = entryForm.SubCategoryName, // From entryForm.SubCategoryName
            //             DetailedBudgetLines = new BindingList<DetailedBudgetLine>()
            //         };
            //         _currentProject.BudgetSubCategories.Add(newSubCategory);
            //         RenderSubCategoriesView(); 
            //     }
            // }
            MessageBox.Show($"Add New Subcategory to {_selectedMainCategory} - Placeholder for SubCategoryEntryForm.");
             // --- TEMPORARY TEST CODE ---
            BudgetSubCategory testSub = new BudgetSubCategory {
                ProjectId = _currentProject.ProjectID,
                MainCategory = _selectedMainCategory,
                SubCategoryCodeSuffix = GenerateNewSubCategorySuffix(_selectedMainCategory),
                SubCategoryName = "Test Subcategory " + DateTime.Now.Second
            };
            _currentProject.BudgetSubCategories.Add(testSub);
            RenderSubCategoriesView();
            // --- END TEMPORARY TEST CODE ---
        }

        private void btnAddItemToSub_Click(object sender, EventArgs e) 
        { 
            BudgetSubCategory parentSub = (sender as Button)?.Tag as BudgetSubCategory;
            if (parentSub == null) return;
            _currentlySelectedSubCategory = parentSub; 

            // Replace with actual DetailedBudgetLineForm invocation
            // using (DetailedBudgetLineForm itemForm = new DetailedBudgetLineForm())
            // {
            //     if (itemForm.ShowDialog(this) == DialogResult.OK)
            //     {
            //         DetailedBudgetLine newItem = itemForm.LineData; // Assuming form populates a new line
            //         newItem.DetailedBudgetLineID = Guid.NewGuid();
            //         newItem.BudgetSubCategoryID = parentSub.BudgetSubCategoryID;
            //         newItem.ProjectId = _currentProject.ProjectID;
            //         newItem.Category = parentSub.MainCategory; 
            //         newItem.Code = GenerateNewItemCode(parentSub);
            //         RecalculateItemTotal(newItem); // Calculate initial total cost
            //         parentSub.DetailedBudgetLines.Add(newItem);
            //         RecalculateSubCategoryTotal(parentSub);
            //         // DGV should update via BindingList. Refresh its panel if necessary.
            //     }
            // }
            MessageBox.Show($"Add Item to {parentSub.SubCategoryName} - Placeholder for DetailedBudgetLineForm.");
            // --- TEMPORARY TEST CODE ---
            DetailedBudgetLine testItem = new DetailedBudgetLine {
                BudgetSubCategoryID = parentSub.BudgetSubCategoryID,
                ProjectId = _currentProject.ProjectID,
                Category = parentSub.MainCategory,
                Code = GenerateNewItemCode(parentSub),
                Description = "Test Item " + DateTime.Now.Millisecond,
                Quantity = 1, UnitCost = 10, Duration = 1, PercentageChargedToCBPF = 100
            };
            RecalculateItemTotal(testItem);
            parentSub.DetailedBudgetLines.Add(testItem);
            RecalculateSubCategoryTotal(parentSub);
            // --- END TEMPORARY TEST CODE ---
        }

        private void btnEditSubCategory_Click(object sender, EventArgs e) 
        { 
            BudgetSubCategory subToEdit = (sender as Button)?.Tag as BudgetSubCategory;
            if (subToEdit == null) return;
            // using (SubCategoryEntryForm entryForm = new SubCategoryEntryForm(subToEdit)) { ... }
            MessageBox.Show($"Edit Subcategory: {subToEdit.SubCategoryName} - To be implemented."); 
        }

        private void btnDeleteSubCategory_Click(object sender, EventArgs e) 
        { 
            BudgetSubCategory subToDelete = (sender as Button)?.Tag as BudgetSubCategory;
            if (subToDelete == null) return;
            if (MessageBox.Show($"Delete subcategory '{subToDelete.SubCategoryName}' and all its items?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _currentProject.BudgetSubCategories.Remove(subToDelete);
                RenderSubCategoriesView();
            }
        }
        
        private void btnToggleSubCategoryItems_Click(object sender, EventArgs e) 
        { 
            Button btn = sender as Button;
            Panel itemsPanel = btn?.Tag as Panel; // pnlItemsArea is stored in Tag
            if (itemsPanel != null)
            {
                itemsPanel.Visible = !itemsPanel.Visible;
                btn.Text = itemsPanel.Visible ? "Hide Items (-)" : "Show Items (+)";
                // DGV is now created and added in CreateSubCategoryPanel, so no need to add placeholder here.
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
                // using (DetailedBudgetLineForm itemForm = new DetailedBudgetLineForm(lineClicked)) { ... }
                MessageBox.Show($"Edit item: {lineClicked.Description} - To be implemented.");
            }
            else if (dgv.Columns[e.ColumnIndex].Name == "DeleteItemButton")
            {
                if (MessageBox.Show($"Delete item '{lineClicked.Description}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    parentSubCategory.DetailedBudgetLines.Remove(lineClicked);
                    RecalculateSubCategoryTotal(parentSubCategory); // DGV updates via BindingList
                }
            }
            else if (dgv.Columns[e.ColumnIndex].Name == "ExpandItemizedButton")
            {
                DataGridViewButtonCell cell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewButtonCell;
                if (_currentlySelectedItemForDetailView == lineClicked && pnlItemizedDetailsHolder.Visible)
                {
                    ShowItemizedDetailsView(null); // Hide
                    if(cell != null) cell.Value = "+";
                }
                else
                {
                    ShowItemizedDetailsView(lineClicked);
                    if(cell != null) cell.Value = "-";
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
            dgv.InvalidateRow(e.RowIndex); // Refresh the row to show new total
            RecalculateSubCategoryTotal(parentSubCategory);
        }
        
        private void dgvSubCategoryItems_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
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
            // Find the label in the subcategory panel header and update it
            Control[] foundControls = this.pnlSubCategoryListArea.Controls.Find($"pnlSubView_{subCategory.BudgetSubCategoryID}", false);
            if (foundControls.Length > 0 && foundControls[0] is Panel pnlSubView)
            {
                Control[] lblControls = pnlSubView.Controls.Find($"lblSubCategoryTotal_{subCategory.BudgetSubCategoryID}", true);
                if (lblControls.Length > 0 && lblControls[0] is Label lblTotal)
                {
                    lblTotal.Text = "Total: " + total.ToString("C", CultureInfo.InvariantCulture);
                }
            }
            // Potentially bubble up to recalculate main category and project totals here
        }


        private void ShowItemizedDetailsView(DetailedBudgetLine line)
        {
           _currentlySelectedItemForDetailView = line;
           if (line == null) 
           {
               if(pnlItemizedDetailsHolder != null) pnlItemizedDetailsHolder.Visible = false;
               return;
           }
           if(lblItemizedDetailsHeader != null) lblItemizedDetailsHeader.Text = $"Itemized Details for: {(line.Code ?? "N/A")} - {(line.Description ?? "N/A")}";
           
           line.ItemizedDetails = line.ItemizedDetails ?? new BindingList<ItemizedBudgetDetail>();
           if(dgvItemizedDetails != null)
           {
                dgvItemizedDetails.DataSource = null; 
                dgvItemizedDetails.DataSource = line.ItemizedDetails;
           }
           if(pnlItemizedDetailsHolder != null) pnlItemizedDetailsHolder.Visible = true;
        }

        private void btnAddNewItemizedDetail_Click(object sender, EventArgs e) 
        {
            if (_currentlySelectedItemForDetailView == null) 
            {
                MessageBox.Show("Please select a main budget item first to add itemized details.", "No Item Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _currentlySelectedItemForDetailView.ItemizedDetails = _currentlySelectedItemForDetailView.ItemizedDetails ?? new BindingList<ItemizedBudgetDetail>();
            var newItem = new ItemizedBudgetDetail { 
                ItemizedBudgetDetailID = Guid.NewGuid(),
                ParentBudgetLineID = _currentlySelectedItemForDetailView.DetailedBudgetLineID 
            };
            _currentlySelectedItemForDetailView.ItemizedDetails.Add(newItem);
            UpdateParentItemTotalCost(_currentlySelectedItemForDetailView);
        }

        private void dgvItemizedDetails_CellContentClick(object sender, DataGridViewCellEventArgs e) 
        {
            if (e.RowIndex < 0 || _currentlySelectedItemForDetailView == null ) return;
            
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
                   UpdateParentItemTotalCost(_currentlySelectedItemForDetailView);
               }
            }
        }

        private void dgvItemizedDetails_DataError(object sender, DataGridViewDataErrorEventArgs e) 
        {
           MessageBox.Show($"Data error in itemized details: {e.Exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
           e.Cancel = true; // To prevent the DGV from trying to commit the erroneous value and keep the focus on the cell
           // e.ThrowException = false; // Default is false, so this isn't strictly necessary unless set to true elsewhere
        }
        
        private void UpdateParentItemTotalCost(DetailedBudgetLine parentItem)
        {
           if (parentItem == null) return;

           if (parentItem.ItemizedDetails != null && parentItem.ItemizedDetails.Any())
           {
               parentItem.TotalCost = parentItem.ItemizedDetails.Sum(d => d.TotalCost);
           }
           else
           {
               RecalculateItemTotal(parentItem); // Use the item's own fields if no itemized details
           }
           
           // Find the DGV for the parent subcategory and refresh the row for parentItem
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
                           // Find the row and invalidate it
                           for(int i=0; i < dgvItems.Rows.Count; i++)
                           {
                               if(dgvItems.Rows[i].DataBoundItem == parentItem)
                               {
                                   dgvItems.InvalidateRow(i);
                                   break;
                               }
                           }
                       }
                   }
                   RecalculateSubCategoryTotal(ownerSubCategory); // Also update the subcategory total label
               }
           }
        }
    }
}
