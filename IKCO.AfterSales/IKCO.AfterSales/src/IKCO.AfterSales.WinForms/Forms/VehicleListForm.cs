using System;
using System.Drawing;
using System.Windows.Forms;
using IKCO.AfterSales.WinForms.Common;
using IKCO.AfterSales.WinForms.Data;
using IKCO.AfterSales.WinForms.Models;

namespace IKCO.AfterSales.WinForms.Forms
{
    public class VehicleListForm : Form
    {
        private readonly VehicleRepository _repository = new VehicleRepository();

        private TextBox _txtSearch;
        private DataGridView _grid;
        private Label _lblPaging;
        private Button _btnPrev, _btnNext;

        private int _pageIndex;
        private int _pageCount;
        private readonly int _pageSize = AppSettings.DefaultPageSize;

        public VehicleListForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            UiHelper.ApplyRtl(this);
            Text = "مدیریت خودروها";
            ClientSize = new Size(950, 560);

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(8) };

            _txtSearch = new TextBox { Width = 240 };
            _txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { _pageIndex = 0; LoadData(); } };

            var btnSearch = UiHelper.MakeButton("جستجو", 80);
            btnSearch.Click += (s, e) => { _pageIndex = 0; LoadData(); };

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
            flow.Controls.Add(new Label { Width = 30 });
            flow.Controls.Add(btnNew);
            flow.Controls.Add(btnEdit);
            flow.Controls.Add(btnDelete);
            toolbar.Controls.Add(flow);

            _grid = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StyleGrid(_grid);
            _grid.Columns.Add(UiHelper.TextColumn("شماره پلاک", "PlateNumber", 130,
                null, DataGridViewContentAlignment.MiddleCenter));
            _grid.Columns.Add(UiHelper.TextColumn("مدل", "Model", 120));
            _grid.Columns.Add(UiHelper.TextColumn("سال تولید", "ProductionYear", 70,
                null, DataGridViewContentAlignment.MiddleCenter));
            _grid.Columns.Add(UiHelper.TextColumn("مالک", "CustomerName", 140));
            _grid.Columns.Add(UiHelper.TextColumn("کارکرد (کیلومتر)", "Mileage", 90, "#,##0"));
            _grid.Columns.Add(UiHelper.TextColumn("شماره شاسی", "VIN", 130,
                null, DataGridViewContentAlignment.MiddleCenter));
            _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };

            var pager = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(8) };
            _btnPrev = UiHelper.MakeButton("قبلی", 70);
            _btnPrev.Click += (s, e) => { if (_pageIndex > 0) { _pageIndex--; LoadData(); } };
            _btnNext = UiHelper.MakeButton("بعدی", 70);
            _btnNext.Click += (s, e) => { if (_pageIndex < _pageCount - 1) { _pageIndex++; LoadData(); } };
            _lblPaging = new Label { AutoSize = true, Top = 8 };

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

        private void LoadData()
        {
            try
            {
                var result = _repository.GetList(_txtSearch.Text, null, _pageIndex, _pageSize);
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

        private Vehicle Selected
        {
            get { return _grid.CurrentRow == null ? null : _grid.CurrentRow.DataBoundItem as Vehicle; }
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            using (var form = new VehicleEditForm(null))
            {
                if (form.ShowDialog(this) == DialogResult.OK) LoadData();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var vehicle = Selected;
            if (vehicle == null) { UiHelper.Warn(Messages.NoRowSelected); return; }

            using (var form = new VehicleEditForm(vehicle.VehicleId))
            {
                if (form.ShowDialog(this) == DialogResult.OK) LoadData();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var vehicle = Selected;
            if (vehicle == null) { UiHelper.Warn(Messages.NoRowSelected); return; }
            if (!UiHelper.Confirm(Messages.ConfirmDelete)) return;

            try
            {
                _repository.Delete(vehicle.VehicleId);
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
