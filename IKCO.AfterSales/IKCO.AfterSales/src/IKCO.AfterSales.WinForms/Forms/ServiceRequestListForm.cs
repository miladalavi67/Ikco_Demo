using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using IKCO.AfterSales.WinForms.Common;
using IKCO.AfterSales.WinForms.Data;
using IKCO.AfterSales.WinForms.Models;

namespace IKCO.AfterSales.WinForms.Forms
{
    public class ServiceRequestListForm : Form
    {
        private readonly ServiceRequestRepository _repository = new ServiceRequestRepository();

        private TextBox _txtSearch;
        private ComboBox _cmbStatus;
        private DateTimePicker _dtpFrom, _dtpTo;
        private CheckBox _chkDateFilter;
        private DataGridView _grid;
        private Label _lblPaging, _lblSummary;
        private Button _btnPrev, _btnNext;

        private int _pageIndex;
        private int _pageCount;
        private readonly int _pageSize = AppSettings.DefaultPageSize;

        public ServiceRequestListForm()
        {
            InitializeComponent();
            LoadStatuses();
            LoadData();
        }

        private void InitializeComponent()
        {
            UiHelper.ApplyRtl(this);
            Text = "درخواست‌های تعمیر";
            ClientSize = new Size(1050, 600);

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 84, Padding = new Padding(8) };

            _txtSearch = new TextBox { Width = 200 };
            _txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { _pageIndex = 0; LoadData(); } };

            var btnSearch = UiHelper.MakeButton("جستجو", 80);
            btnSearch.Click += (s, e) => { _pageIndex = 0; LoadData(); };

            _cmbStatus = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbStatus.SelectedIndexChanged += (s, e) => { _pageIndex = 0; LoadData(); };

            var btnNew = UiHelper.MakeButton("جدید");
            btnNew.Click += BtnNew_Click;
            var btnEdit = UiHelper.MakeButton("باز کردن");
            btnEdit.Click += BtnEdit_Click;
            var btnDelete = UiHelper.MakeButton("حذف");
            btnDelete.Click += BtnDelete_Click;

            var row1 = new FlowLayoutPanel
            { Dock = DockStyle.Top, Height = 38, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
            row1.Controls.Add(_txtSearch);
            row1.Controls.Add(btnSearch);
            row1.Controls.Add(new Label { Text = "وضعیت:", AutoSize = true, Padding = new Padding(8, 8, 0, 0) });
            row1.Controls.Add(_cmbStatus);
            row1.Controls.Add(new Label { Width = 20 });
            row1.Controls.Add(btnNew);
            row1.Controls.Add(btnEdit);
            row1.Controls.Add(btnDelete);

            _chkDateFilter = new CheckBox { Text = "فیلتر تاریخ", AutoSize = true, Padding = new Padding(0, 6, 0, 0) };
            _chkDateFilter.CheckedChanged += (s, e) => { _pageIndex = 0; LoadData(); };

            _dtpFrom = new DateTimePicker { Width = 120, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddMonths(-6) };
            _dtpTo   = new DateTimePicker { Width = 120, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            _dtpFrom.ValueChanged += (s, e) => { if (_chkDateFilter.Checked) { _pageIndex = 0; LoadData(); } };
            _dtpTo.ValueChanged   += (s, e) => { if (_chkDateFilter.Checked) { _pageIndex = 0; LoadData(); } };

            var row2 = new FlowLayoutPanel
            { Dock = DockStyle.Top, Height = 36, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
            row2.Controls.Add(_chkDateFilter);
            row2.Controls.Add(new Label { Text = "از:", AutoSize = true, Padding = new Padding(8, 6, 0, 0) });
            row2.Controls.Add(_dtpFrom);
            row2.Controls.Add(new Label { Text = "تا:", AutoSize = true, Padding = new Padding(8, 6, 0, 0) });
            row2.Controls.Add(_dtpTo);

            toolbar.Controls.Add(row2);
            toolbar.Controls.Add(row1);

            _grid = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StyleGrid(_grid);
            _grid.Columns.Add(UiHelper.TextColumn("شماره درخواست", "RequestNo", 100,
                null, DataGridViewContentAlignment.MiddleCenter));
            _grid.Columns.Add(UiHelper.TextColumn("پلاک", "PlateNumber", 110,
                null, DataGridViewContentAlignment.MiddleCenter));
            _grid.Columns.Add(UiHelper.TextColumn("مشتری", "CustomerName", 120));
            _grid.Columns.Add(UiHelper.TextColumn("تعمیرکار", "TechnicianName", 110));
            _grid.Columns.Add(UiHelper.TextColumn("وضعیت", "StatusName", 90,
                null, DataGridViewContentAlignment.MiddleCenter));
            _grid.Columns.Add(UiHelper.TextColumn("مبلغ کل (ریال)", "TotalCost", 110, "#,##0"));

            var dateColumn = new DataGridViewTextBoxColumn
            { HeaderText = "تاریخ", Name = "colPersianDate", FillWeight = 90 };
            dateColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _grid.Columns.Add(dateColumn);

            _grid.CellFormatting += Grid_CellFormatting;
            _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };

            var pager = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(8) };
            _btnPrev = UiHelper.MakeButton("قبلی", 70);
            _btnPrev.Click += (s, e) => { if (_pageIndex > 0) { _pageIndex--; LoadData(); } };
            _btnNext = UiHelper.MakeButton("بعدی", 70);
            _btnNext.Click += (s, e) => { if (_pageIndex < _pageCount - 1) { _pageIndex++; LoadData(); } };
            _lblPaging  = new Label { AutoSize = true, Top = 8 };
            _lblSummary = new Label { AutoSize = true, Top = 8, ForeColor = UiHelper.HeaderBack };

            var pagerFlow = new FlowLayoutPanel
            { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
            pagerFlow.Controls.Add(_btnPrev);
            pagerFlow.Controls.Add(_btnNext);
            pagerFlow.Controls.Add(_lblPaging);
            pagerFlow.Controls.Add(new Label { Width = 20 });
            pagerFlow.Controls.Add(_lblSummary);
            pager.Controls.Add(pagerFlow);

            Controls.Add(_grid);
            Controls.Add(pager);
            Controls.Add(toolbar);
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_grid.Columns[e.ColumnIndex].Name != "colPersianDate") return;

            var request = _grid.Rows[e.RowIndex].DataBoundItem as ServiceRequest;
            if (request != null)
            {
                e.Value = PersianDateHelper.ToPersianDate(request.RequestDate);
                e.FormattingApplied = true;
            }
        }

        private void LoadStatuses()
        {
            try
            {
                var statuses = new List<ServiceStatus>
                {
                    new ServiceStatus { StatusId = 0, StatusName = "همه" }
                };
                statuses.AddRange(_repository.GetStatuses());

                _cmbStatus.DisplayMember = "StatusName";
                _cmbStatus.ValueMember = "StatusId";
                _cmbStatus.DataSource = statuses;
                _cmbStatus.SelectedIndex = 0;
            }
            catch (Exception ex) { UiHelper.ShowError(ex); }
        }

        private void LoadData()
        {
            try
            {
                int? statusId = null;
                if (_cmbStatus.SelectedValue != null)
                {
                    int selected = Convert.ToInt32(_cmbStatus.SelectedValue);
                    if (selected > 0) statusId = selected;
                }

                DateTime? from = _chkDateFilter.Checked ? (DateTime?)_dtpFrom.Value.Date : null;
                DateTime? to   = _chkDateFilter.Checked ? (DateTime?)_dtpTo.Value.Date : null;

                var result = _repository.GetList(_txtSearch.Text, statusId, from, to, _pageIndex, _pageSize);
                _grid.DataSource = result.Items;
                _pageCount = result.PageCount;

                _lblPaging.Text = string.Format("صفحه {0} از {1}  |  مجموع: {2} رکورد",
                    _pageCount == 0 ? 0 : _pageIndex + 1, _pageCount, result.TotalCount);

                decimal pageSum = 0;
                foreach (var item in result.Items) pageSum += item.TotalCost;
                _lblSummary.Text = "جمع این صفحه: " + PersianDateHelper.ToCurrency(pageSum);

                _btnPrev.Enabled = _pageIndex > 0;
                _btnNext.Enabled = _pageIndex < _pageCount - 1;
            }
            catch (Exception ex) { UiHelper.ShowError(ex); }
        }

        private ServiceRequest Selected
        {
            get { return _grid.CurrentRow == null ? null : _grid.CurrentRow.DataBoundItem as ServiceRequest; }
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            using (var form = new ServiceRequestEditForm(null))
            { if (form.ShowDialog(this) == DialogResult.OK) LoadData(); }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var request = Selected;
            if (request == null) { UiHelper.Warn(Messages.NoRowSelected); return; }

            using (var form = new ServiceRequestEditForm(request.RequestId))
            { if (form.ShowDialog(this) == DialogResult.OK) LoadData(); }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var request = Selected;
            if (request == null) { UiHelper.Warn(Messages.NoRowSelected); return; }

            if (request.IsFinal) { UiHelper.Warn(Messages.RequestIsFinal); return; }
            if (!UiHelper.Confirm(Messages.ConfirmDelete)) return;

            try
            {
                _repository.Delete(request.RequestId);
                UiHelper.Info(Messages.DeleteSucceeded);
                LoadData();
            }
            catch (Exception ex) { UiHelper.ShowError(ex); }
        }
    }
}
