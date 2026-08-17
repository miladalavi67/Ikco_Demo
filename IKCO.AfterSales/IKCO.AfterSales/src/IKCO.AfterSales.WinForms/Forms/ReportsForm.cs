using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using IKCO.AfterSales.WinForms.Common;
using IKCO.AfterSales.WinForms.Data;

namespace IKCO.AfterSales.WinForms.Forms
{
    public class ReportsForm : Form
    {
        private readonly ReportRepository _repository = new ReportRepository();

        private TabControl _tabs;
        private DataGridView _gridRevenue, _gridTechnicians, _gridLowStock;
        private NumericUpDown _numYear;
        private DateTimePicker _dtpFrom, _dtpTo;

        public ReportsForm()
        {
            InitializeComponent();
            LoadRevenue();
        }

        private void InitializeComponent()
        {
            UiHelper.ApplyRtl(this);
            Text = "گزارش‌های مدیریتی";
            ClientSize = new Size(900, 560);

            _tabs = new TabControl { Dock = DockStyle.Fill, RightToLeft = RightToLeft.Yes };
            _tabs.SelectedIndexChanged += Tabs_SelectedIndexChanged;

            /* --------------------------- revenue tab --------------------------- */
            var tabRevenue = new TabPage("درآمد ماهانه") { RightToLeft = RightToLeft.Yes };

            _numYear = new NumericUpDown
            {
                Width = 90,
                Minimum = 2000,
                Maximum = 2100,
                Value = DateTime.Today.Year
            };
            var btnRevenue = UiHelper.MakeButton("نمایش", 80);
            btnRevenue.Click += (s, e) => LoadRevenue();

            var revenueBar = new FlowLayoutPanel
            { Dock = DockStyle.Top, Height = 44, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
            revenueBar.Controls.Add(new Label { Text = "سال میلادی:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
            revenueBar.Controls.Add(_numYear);
            revenueBar.Controls.Add(btnRevenue);

            _gridRevenue = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            UiHelper.StyleGrid(_gridRevenue);
            var monthColumn = new DataGridViewTextBoxColumn
            { HeaderText = "ماه", Name = "colMonthName", FillWeight = 90 };
            monthColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _gridRevenue.Columns.Add(monthColumn);
            _gridRevenue.Columns.Add(UiHelper.TextColumn("تعداد درخواست", "RequestCnt", 80,
                null, DataGridViewContentAlignment.MiddleCenter));
            _gridRevenue.Columns.Add(UiHelper.TextColumn("دستمزد (ریال)", "LaborSum", 110, "#,##0"));
            _gridRevenue.Columns.Add(UiHelper.TextColumn("قطعات (ریال)", "PartsSum", 110, "#,##0"));
            _gridRevenue.Columns.Add(UiHelper.TextColumn("جمع کل (ریال)", "TotalSum", 120, "#,##0"));
            _gridRevenue.CellFormatting += GridRevenue_CellFormatting;

            tabRevenue.Controls.Add(_gridRevenue);
            tabRevenue.Controls.Add(revenueBar);

            /* ------------------------- technician tab -------------------------- */
            var tabTechnicians = new TabPage("عملکرد تعمیرکاران") { RightToLeft = RightToLeft.Yes };

            _dtpFrom = new DateTimePicker { Width = 120, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddMonths(-6) };
            _dtpTo   = new DateTimePicker { Width = 120, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            var btnTechnicians = UiHelper.MakeButton("نمایش", 80);
            btnTechnicians.Click += (s, e) => LoadTechnicians();

            var techBar = new FlowLayoutPanel
            { Dock = DockStyle.Top, Height = 44, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
            techBar.Controls.Add(new Label { Text = "از:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
            techBar.Controls.Add(_dtpFrom);
            techBar.Controls.Add(new Label { Text = "تا:", AutoSize = true, Padding = new Padding(8, 6, 0, 0) });
            techBar.Controls.Add(_dtpTo);
            techBar.Controls.Add(btnTechnicians);

            _gridTechnicians = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            UiHelper.StyleGrid(_gridTechnicians);
            _gridTechnicians.Columns.Add(UiHelper.TextColumn("تعمیرکار", "FullName", 150));
            _gridTechnicians.Columns.Add(UiHelper.TextColumn("تخصص", "Specialty", 130));
            _gridTechnicians.Columns.Add(UiHelper.TextColumn("تعداد درخواست", "RequestCnt", 80,
                null, DataGridViewContentAlignment.MiddleCenter));
            _gridTechnicians.Columns.Add(UiHelper.TextColumn("جمع ساعت", "TotalHours", 80,
                "0.##", DataGridViewContentAlignment.MiddleCenter));
            _gridTechnicians.Columns.Add(UiHelper.TextColumn("درآمد (ریال)", "TotalRevenue", 120, "#,##0"));

            tabTechnicians.Controls.Add(_gridTechnicians);
            tabTechnicians.Controls.Add(techBar);

            /* -------------------------- low stock tab -------------------------- */
            var tabLowStock = new TabPage("کسری موجودی قطعات") { RightToLeft = RightToLeft.Yes };

            _gridLowStock = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            UiHelper.StyleGrid(_gridLowStock);
            _gridLowStock.Columns.Add(UiHelper.TextColumn("کد قطعه", "PartCode", 80,
                null, DataGridViewContentAlignment.MiddleCenter));
            _gridLowStock.Columns.Add(UiHelper.TextColumn("نام قطعه", "PartName", 200));
            _gridLowStock.Columns.Add(UiHelper.TextColumn("موجودی", "StockQty", 70,
                null, DataGridViewContentAlignment.MiddleCenter));
            _gridLowStock.Columns.Add(UiHelper.TextColumn("حداقل موجودی", "MinStockQty", 90,
                null, DataGridViewContentAlignment.MiddleCenter));
            _gridLowStock.Columns.Add(UiHelper.TextColumn("قیمت واحد (ریال)", "UnitPrice", 110, "#,##0"));

            tabLowStock.Controls.Add(_gridLowStock);

            _tabs.TabPages.Add(tabRevenue);
            _tabs.TabPages.Add(tabTechnicians);
            _tabs.TabPages.Add(tabLowStock);

            Controls.Add(_tabs);
        }

        private void GridRevenue_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_gridRevenue.Columns[e.ColumnIndex].Name != "colMonthName") return;

            var row = _gridRevenue.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (row != null && row["MonthNo"] != DBNull.Value)
            {
                e.Value = Convert.ToString(row["MonthNo"]);
                e.FormattingApplied = true;
            }
        }

        private void Tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (_tabs.SelectedIndex)
            {
                case 0: LoadRevenue(); break;
                case 1: LoadTechnicians(); break;
                case 2: LoadLowStock(); break;
            }
        }

        private void LoadRevenue()
        {
            try { _gridRevenue.DataSource = _repository.GetRevenueByMonth((int)_numYear.Value); }
            catch (Exception ex) { UiHelper.ShowError(ex); }
        }

        private void LoadTechnicians()
        {
            try { _gridTechnicians.DataSource = _repository.GetTechnicianPerformance(_dtpFrom.Value.Date, _dtpTo.Value.Date); }
            catch (Exception ex) { UiHelper.ShowError(ex); }
        }

        private void LoadLowStock()
        {
            try { _gridLowStock.DataSource = _repository.GetLowStockParts(); }
            catch (Exception ex) { UiHelper.ShowError(ex); }
        }
    }
}
