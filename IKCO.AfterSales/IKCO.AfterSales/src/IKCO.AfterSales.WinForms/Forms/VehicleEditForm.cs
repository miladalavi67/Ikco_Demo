using System;
using System.Drawing;
using System.Windows.Forms;
using IKCO.AfterSales.WinForms.Common;
using IKCO.AfterSales.WinForms.Data;
using IKCO.AfterSales.WinForms.Models;

namespace IKCO.AfterSales.WinForms.Forms
{
    public class VehicleEditForm : Form
    {
        private readonly VehicleRepository _repository = new VehicleRepository();
        private readonly CustomerRepository _customerRepository = new CustomerRepository();
        private readonly int? _vehicleId;

        private ComboBox _cmbCustomer;
        private TextBox _txtPlate, _txtModel, _txtYear, _txtVin, _txtMileage;

        public VehicleEditForm(int? vehicleId)
        {
            _vehicleId = vehicleId;
            InitializeComponent();
            LoadCustomers();

            if (_vehicleId.HasValue) LoadVehicle();
        }

        private void InitializeComponent()
        {
            UiHelper.ApplyRtl(this);
            Text = _vehicleId.HasValue ? "ویرایش خودرو" : "خودرو جدید";
            ClientSize = new Size(430, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(14),
                RightToLeft = RightToLeft.Yes
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _cmbCustomer = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _txtPlate   = new TextBox { Dock = DockStyle.Fill, MaxLength = 20 };
            _txtModel   = new TextBox { Dock = DockStyle.Fill, MaxLength = 60 };
            _txtYear    = new TextBox { Dock = DockStyle.Fill, MaxLength = 4 };
            _txtVin     = new TextBox { Dock = DockStyle.Fill, MaxLength = 17 };
            _txtMileage = new TextBox { Dock = DockStyle.Fill, MaxLength = 9, Text = "0" };

            AddRow(layout, "مالک *", _cmbCustomer);
            AddRow(layout, "شماره پلاک *", _txtPlate);
            AddRow(layout, "مدل *", _txtModel);
            AddRow(layout, "سال تولید *", _txtYear);
            AddRow(layout, "شماره شاسی", _txtVin);
            AddRow(layout, "کارکرد (کیلومتر)", _txtMileage);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.LeftToRight,
                Height = 46,
                Padding = new Padding(10)
            };
            var btnCancel = UiHelper.MakeButton("انصراف");
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            var btnSave = UiHelper.MakeButton("ذخیره");
            btnSave.Click += BtnSave_Click;
            buttons.Controls.Add(btnCancel);
            buttons.Controls.Add(btnSave);

            Controls.Add(layout);
            Controls.Add(buttons);
            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private static void AddRow(TableLayoutPanel layout, string caption, Control control)
        {
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(new Label
            {
                Text = caption,
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                Padding = new Padding(0, 6, 0, 0)
            });
            layout.Controls.Add(control);
        }

        private void LoadCustomers()
        {
            try
            {
                // the combo shows every active customer; the list is small enough to load at once
                var customers = _customerRepository.GetList(null, true, 0, 1000);
                _cmbCustomer.DisplayMember = "FullName";
                _cmbCustomer.ValueMember = "CustomerId";
                _cmbCustomer.DataSource = customers.Items;
                _cmbCustomer.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(ex);
            }
        }

        private void LoadVehicle()
        {
            try
            {
                var vehicle = _repository.GetById(_vehicleId.Value);
                if (vehicle == null) return;

                _cmbCustomer.SelectedValue = vehicle.CustomerId;
                _txtPlate.Text   = vehicle.PlateNumber;
                _txtModel.Text   = vehicle.Model;
                _txtYear.Text    = vehicle.ProductionYear.ToString();
                _txtVin.Text     = vehicle.VIN;
                _txtMileage.Text = vehicle.Mileage.ToString();
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(ex);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_cmbCustomer.SelectedValue == null)
            { UiHelper.Warn(string.Format(Messages.RequiredField, "مالک")); return; }

            if (!ValidationHelper.IsRequired(_txtPlate.Text))
            { UiHelper.Warn(string.Format(Messages.RequiredField, "شماره پلاک")); return; }

            if (!ValidationHelper.IsRequired(_txtModel.Text))
            { UiHelper.Warn(string.Format(Messages.RequiredField, "مدل")); return; }

            int year;
            if (!int.TryParse(_txtYear.Text.Trim(), out year) || !ValidationHelper.IsValidProductionYear(year))
            { UiHelper.Warn(Messages.InvalidYear); return; }

            int mileage;
            if (!int.TryParse(string.IsNullOrWhiteSpace(_txtMileage.Text) ? "0" : _txtMileage.Text.Trim(), out mileage))
            { UiHelper.Warn(string.Format(Messages.InvalidNumber, "کارکرد")); return; }

            var vehicle = new Vehicle
            {
                VehicleId      = _vehicleId ?? 0,
                CustomerId     = Convert.ToInt32(_cmbCustomer.SelectedValue),
                PlateNumber    = _txtPlate.Text.Trim(),
                Model          = _txtModel.Text.Trim(),
                ProductionYear = year,
                VIN            = string.IsNullOrWhiteSpace(_txtVin.Text) ? null : _txtVin.Text.Trim(),
                Mileage        = mileage
            };

            try
            {
                if (_vehicleId.HasValue) _repository.Update(vehicle);
                else _repository.Insert(vehicle);

                UiHelper.Info(Messages.SaveSucceeded);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(ex);
            }
        }
    }
}
