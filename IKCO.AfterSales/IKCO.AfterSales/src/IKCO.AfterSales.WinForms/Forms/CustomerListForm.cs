using System;
using System.Drawing;
using System.Windows.Forms;
using IKCO.AfterSales.WinForms.Common;
using IKCO.AfterSales.WinForms.Data;
using IKCO.AfterSales.WinForms.Models;

namespace IKCO.AfterSales.WinForms.Forms
{
    public class CustomerListForm : Form
    {
        private readonly CustomerRepository _repository = new CustomerRepository();

        private TextBox _txtSearch;
        private CheckBox _chkOnlyActive;
        private DataGridView _grid;
        private Label _lblPaging;
        private Button _btnPrev, _btnNext;

        private int _pageIndex;
        private int _pageCount;
        private readonly int _pageSize = AppSettings.DefaultPageSize;

        public CustomerListForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            UiHelper.ApplyRtl(this);
            Text = "مدیریت مشتریان";
            ClientSize = new Size(950, 560);

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(8) };

            _txtSearch = new TextBox { Width = 220, Right = 0, Top = 10 };
            _txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { _pageIndex = 0; LoadData(); } };

            var btnSearch = UiHelper.MakeButton("جستجو", 80);
            btnSearch.Click += (s, e) => { _pageIndex = 0; LoadData(); };

            _chkOnlyActive = new CheckBox { Text = "فقط فعال", AutoSize = true, Top = 14 };
            _chkOnlyActive.CheckedChanged += (s, e) => { _pageIndex = 0; LoadData(); };

            var btnNew = UiHelper.MakeButton("جدید");
            btnNew.Click += BtnNew_Click;

            var btnEdit = UiHelper.MakeButton("ویرایش");
            btnEdit.Click += BtnEdit_Click;

            var btnDelete = UiHelper.MakeButton("حذف");
            btnDelete.Click += BtnDelete_Click;

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };
            flow.Controls.Add(_txtSearch);
            flow.Controls.Add(btnSearch);
            flow.Controls.Add(_chkOnlyActive);
            flow.Controls.Add(new Label { Width = 30 });
            flow.Controls.Add(btnNew);
            flow.Controls.Add(btnEdit);
            flow.Controls.Add(btnDelete);
            toolbar.Controls.Add(flow);

            _grid = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StyleGrid(_grid);
            _grid.Columns.Add(UiHelper.TextColumn("نام و نام خانوادگی", "FullName", 160));
            _grid.Columns.Add(UiHelper.TextColumn("کد ملی", "NationalCode", 90));
            _grid.Columns.Add(UiHelper.TextColumn("موبایل", "Mobile", 90));
            _grid.Columns.Add(UiHelper.TextColumn("شهر", "City", 70));
            _grid.Columns.Add(UiHelper.TextColumn("پست الکترونیک", "Email", 140));
            _grid.Columns.Add(UiHelper.TextColumn("تعداد خودرو", "VehicleCount", 70,
                null, DataGridViewContentAlignment.MiddleCenter));
            _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };
            _grid.RowPrePaint += Grid_RowPrePaint;

            var pager = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(8) };
            _btnPrev = UiHelper.MakeButton("قبلی", 70);
            _btnPrev.Click += (s, e) => { if (_pageIndex > 0) { _pageIndex--; LoadData(); } };
            _btnNext = UiHelper.MakeButton("بعدی", 70);
            _btnNext.Click += (s, e) => { if (_pageIndex < _pageCount - 1) { _pageIndex++; LoadData(); } };
            _lblPaging = new Label { AutoSize = true, Top = 8, Text = "" };

            var pagerFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };
            pagerFlow.Controls.Add(_btnPrev);
            pagerFlow.Controls.Add(_btnNext);
            pagerFlow.Controls.Add(_lblPaging);
            pager.Controls.Add(pagerFlow);

            Controls.Add(_grid);
            Controls.Add(pager);
            Controls.Add(toolbar);
        }

        private void Grid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var customer = _grid.Rows[e.RowIndex].DataBoundItem as Customer;
            if (customer != null && !customer.IsActive)
                _grid.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Gray;
        }

        private void LoadData()
        {
            try
            {
                var result = _repository.GetList(_txtSearch.Text, _chkOnlyActive.Checked, _pageIndex, _pageSize);
                _grid.DataSource = result.Items;
                _pageCount = result.PageCount;

                _lblPaging.Text = string.Format("صفحه {0} از {1}  |  مجموع: {2} رکورد",
                    _pageCount == 0 ? 0 : _pageIndex + 1, _pageCount, result.TotalCount);

                _btnPrev.Enabled = _pageIndex > 0;
                _btnNext.Enabled = _pageIndex < _pageCount - 1;
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(ex);
            }
        }

        private Customer Selected
        {
            get
            {
                return _grid.CurrentRow == null ? null : _grid.CurrentRow.DataBoundItem as Customer;
            }
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            using (var form = new CustomerEditForm(null))
            {
                if (form.ShowDialog(this) == DialogResult.OK) LoadData();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var customer = Selected;
            if (customer == null) { UiHelper.Warn(Messages.NoRowSelected); return; }

            using (var form = new CustomerEditForm(customer.CustomerId))
            {
                if (form.ShowDialog(this) == DialogResult.OK) LoadData();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var customer = Selected;
            if (customer == null) { UiHelper.Warn(Messages.NoRowSelected); return; }
            if (!UiHelper.Confirm(Messages.ConfirmDelete)) return;

            try
            {
                _repository.Delete(customer.CustomerId);
                UiHelper.Info(Messages.DeleteSucceeded);
                LoadData();
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(ex);
            }
        }
    }
}
