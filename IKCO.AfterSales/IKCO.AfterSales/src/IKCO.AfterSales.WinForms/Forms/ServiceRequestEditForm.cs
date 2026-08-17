using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using IKCO.AfterSales.WinForms.Common;
using IKCO.AfterSales.WinForms.Data;
using IKCO.AfterSales.WinForms.Models;

namespace IKCO.AfterSales.WinForms.Forms
{
    /// <summary>
    /// Master-detail form: the request header plus its consumed parts.
    /// </summary>
    public class ServiceRequestEditForm : Form
    {
        private readonly ServiceRequestRepository _repository = new ServiceRequestRepository();
        private readonly VehicleRepository _vehicleRepository = new VehicleRepository();
        private readonly TechnicianRepository _technicianRepository = new TechnicianRepository();
        private readonly PartRepository _partRepository = new PartRepository();
        private readonly CustomerRepository _customerRepository = new CustomerRepository();

        private int? _requestId;
        private ServiceRequest _current;

        private Label _lblRequestNo, _lblTotals;
        private ComboBox _cmbVehicle, _cmbTechnician, _cmbStatus;
        private DateTimePicker _dtpRequestDate;
        private TextBox _txtDescription, _txtLaborHours, _txtDiscount;

        private ComboBox _cmbPart;
        private NumericUpDown _numQuantity;
        private DataGridView _gridParts;
        private Button _btnAddPart, _btnRemovePart, _btnSave;

        public ServiceRequestEditForm(int? requestId)
        {
            _requestId = requestId;
            InitializeComponent();

            LoadLookups();

            if (_requestId.HasValue)
            {
                LoadRequest();
                LoadParts();
            }
            else
            {
                _lblRequestNo.Text = "شماره درخواست: (پس از ذخیره تولید می‌شود)";
                _cmbStatus.Enabled = false;
            }
        }

        private void InitializeComponent()
        {
            UiHelper.ApplyRtl(this);
            Text = _requestId.HasValue ? "ویرایش درخواست تعمیر" : "درخواست تعمیر جدید";
            ClientSize = new Size(760, 640);
            MinimizeBox = false;

            /* ------------------------------ header ------------------------------ */
            _lblRequestNo = new Label
            {
                Dock = DockStyle.Top,
                Height = 34,
                Font = new Font("Tahoma", 10f, FontStyle.Bold),
                ForeColor = UiHelper.HeaderBack,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(10, 0, 0, 0)
            };

            var headerBox = new GroupBox
            {
                Dock = DockStyle.Top,
                Height = 250,
                Text = "اطلاعات درخواست",
                RightToLeft = RightToLeft.Yes
            };

            var layout = new TableLayoutPanel
            { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12), RightToLeft = RightToLeft.Yes };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _cmbVehicle     = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbTechnician  = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbStatus      = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _dtpRequestDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
            _txtLaborHours  = new TextBox { Dock = DockStyle.Fill, Text = "0" };
            _txtDiscount    = new TextBox { Dock = DockStyle.Fill, Text = "0" };
            _txtDescription = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 50, MaxLength = 500 };

            AddRow(layout, "خودرو *", _cmbVehicle);
            AddRow(layout, "تعمیرکار", _cmbTechnician);
            AddRow(layout, "تاریخ درخواست *", _dtpRequestDate);
            AddRow(layout, "وضعیت", _cmbStatus);
            AddRow(layout, "ساعت کارکرد", _txtLaborHours);
            AddRow(layout, "تخفیف (ریال)", _txtDiscount);
            AddRow(layout, "شرح", _txtDescription);

            headerBox.Controls.Add(layout);

            /* ------------------------------ details ----------------------------- */
            var partsBox = new GroupBox
            { Dock = DockStyle.Fill, Text = "قطعات مصرفی", RightToLeft = RightToLeft.Yes };

            var partsToolbar = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8) };
            _cmbPart = new ComboBox { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
            _numQuantity = new NumericUpDown { Width = 70, Minimum = 1, Maximum = 999, Value = 1 };
            _btnAddPart = UiHelper.MakeButton("افزودن قطعه", 110);
            _btnAddPart.Click += BtnAddPart_Click;
            _btnRemovePart = UiHelper.MakeButton("حذف ردیف", 100);
            _btnRemovePart.Click += BtnRemovePart_Click;

            var partsFlow = new FlowLayoutPanel
            { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
            partsFlow.Controls.Add(_cmbPart);
            partsFlow.Controls.Add(new Label { Text = "تعداد:", AutoSize = true, Padding = new Padding(8, 8, 0, 0) });
            partsFlow.Controls.Add(_numQuantity);
            partsFlow.Controls.Add(_btnAddPart);
            partsFlow.Controls.Add(_btnRemovePart);
            partsToolbar.Controls.Add(partsFlow);

            _gridParts = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StyleGrid(_gridParts);
            _gridParts.Columns.Add(UiHelper.TextColumn("کد", "PartCode", 70,
                null, DataGridViewContentAlignment.MiddleCenter));
            _gridParts.Columns.Add(UiHelper.TextColumn("نام قطعه", "PartName", 200));
            _gridParts.Columns.Add(UiHelper.TextColumn("تعداد", "Quantity", 60,
                null, DataGridViewContentAlignment.MiddleCenter));
            _gridParts.Columns.Add(UiHelper.TextColumn("قیمت واحد", "UnitPrice", 100, "#,##0"));
            _gridParts.Columns.Add(UiHelper.TextColumn("جمع ردیف", "LineTotal", 110, "#,##0"));

            partsBox.Controls.Add(_gridParts);
            partsBox.Controls.Add(partsToolbar);

            /* ------------------------------ footer ------------------------------ */
            _lblTotals = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 34,
                Font = new Font("Tahoma", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = UiHelper.AltRowBack
            };

            var buttons = new FlowLayoutPanel
            { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.LeftToRight, Height = 48, Padding = new Padding(10) };
            var btnClose = UiHelper.MakeButton("بستن");
            btnClose.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
            _btnSave = UiHelper.MakeButton("ذخیره");
            _btnSave.Click += BtnSave_Click;
            buttons.Controls.Add(btnClose);
            buttons.Controls.Add(_btnSave);

            Controls.Add(partsBox);
            Controls.Add(_lblTotals);
            Controls.Add(buttons);
            Controls.Add(headerBox);
            Controls.Add(_lblRequestNo);
        }

        private static void AddRow(TableLayoutPanel layout, string caption, Control control)
        {
            layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(new Label
            { Text = caption, AutoSize = true, Anchor = AnchorStyles.Right, Padding = new Padding(0, 6, 0, 0) });
            layout.Controls.Add(control);
        }

        private void LoadLookups()
        {
            try
            {
                var vehicles = _vehicleRepository.GetList(null, null, 0, 1000);
                _cmbVehicle.DisplayMember = "DisplayTitle";
                _cmbVehicle.ValueMember = "VehicleId";
                _cmbVehicle.DataSource = vehicles.Items;
                _cmbVehicle.SelectedIndex = -1;

                var technicians = new List<Technician>
                { new Technician { TechnicianId = 0, FullName = "(تخصیص نیافته)" } };
                technicians.AddRange(_technicianRepository.GetActiveLookup());
                _cmbTechnician.DisplayMember = "FullName";
                _cmbTechnician.ValueMember = "TechnicianId";
                _cmbTechnician.DataSource = technicians;
                _cmbTechnician.SelectedIndex = 0;

                _cmbStatus.DisplayMember = "StatusName";
                _cmbStatus.ValueMember = "StatusId";
                _cmbStatus.DataSource = _repository.GetStatuses();

                var parts = _partRepository.GetActiveLookup();
                _cmbPart.DisplayMember = "PartName";
                _cmbPart.ValueMember = "PartId";
                _cmbPart.DataSource = parts;
                _cmbPart.SelectedIndex = -1;
            }
            catch (Exception ex) { UiHelper.ShowError(ex); }
        }

        private void LoadRequest()
        {
            try
            {
                _current = _repository.GetById(_requestId.Value);
                if (_current == null) return;

                _lblRequestNo.Text = "شماره درخواست: " + _current.RequestNo;

                _cmbVehicle.SelectedValue    = _current.VehicleId;
                _cmbTechnician.SelectedValue = _current.TechnicianId ?? 0;
                _cmbStatus.SelectedValue     = _current.StatusId;
                _dtpRequestDate.Value        = _current.RequestDate;
                _txtDescription.Text         = _current.Description;
                _txtLaborHours.Text          = _current.LaborHours.ToString("0.##");
                _txtDiscount.Text            = _current.DiscountAmount.ToString("0");

                UpdateTotals();
                ApplyFinalStateLock();
            }
            catch (Exception ex) { UiHelper.ShowError(ex); }
        }

        /// <summary>
        /// A completed or cancelled request is read-only.
        /// </summary>
        private void ApplyFinalStateLock()
        {
            if (_current == null || !_current.IsFinal) return;

            _cmbVehicle.Enabled = false;
            _cmbTechnician.Enabled = false;
            _dtpRequestDate.Enabled = false;
            _txtDescription.ReadOnly = true;
            _txtLaborHours.ReadOnly = true;
            _txtDiscount.ReadOnly = true;
            _cmbPart.Enabled = false;
            _numQuantity.Enabled = false;
            _btnAddPart.Enabled = false;
            _btnRemovePart.Enabled = false;
            _btnSave.Enabled = false;
        }

        private void LoadParts()
        {
            try
            {
                _gridParts.DataSource = _repository.GetParts(_requestId.Value);
            }
            catch (Exception ex) { UiHelper.ShowError(ex); }
        }

        private void UpdateTotals()
        {
            if (_current == null) { _lblTotals.Text = string.Empty; return; }

            _lblTotals.Text = string.Format(
                "دستمزد: {0}     قطعات: {1}     تخفیف: {2}     مبلغ کل: {3}",
                PersianDateHelper.ToCurrency(_current.LaborCost),
                PersianDateHelper.ToCurrency(_current.PartsCost),
                PersianDateHelper.ToCurrency(_current.DiscountAmount),
                PersianDateHelper.ToCurrency(_current.TotalCost));
        }

        /// <summary>
        /// Loyalty rule: a customer with enough completed requests gets a percentage
        /// off the labour cost. Suggested here, then written into the discount box.
        /// </summary>
        private decimal CalculateSuggestedDiscount(int vehicleId, decimal laborHours)
        {
            try
            {
                var vehicle = _vehicleRepository.GetById(vehicleId);
                if (vehicle == null) return 0m;

                int completed = _customerRepository.GetCompletedRequestCount(vehicle.CustomerId);
                if (completed < AppSettings.LoyaltyThreshold) return 0m;

                if (_cmbTechnician.SelectedValue == null) return 0m;
                int technicianId = Convert.ToInt32(_cmbTechnician.SelectedValue);
                if (technicianId == 0) return 0m;

                var technician = _technicianRepository.GetById(technicianId);
                if (technician == null) return 0m;

                decimal laborCost = laborHours * technician.HourlyRate;
                return Math.Round(laborCost * AppSettings.LoyaltyDiscountPercent / 100m, 0);
            }
            catch
            {
                return 0m;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_cmbVehicle.SelectedValue == null)
            { UiHelper.Warn(string.Format(Messages.RequiredField, "خودرو")); return; }

            decimal laborHours;
            if (!decimal.TryParse(string.IsNullOrWhiteSpace(_txtLaborHours.Text) ? "0" : _txtLaborHours.Text.Trim(), out laborHours)
                || laborHours < 0)
            { UiHelper.Warn(string.Format(Messages.InvalidNumber, "ساعت کارکرد")); return; }

            decimal discount;
            if (!decimal.TryParse(string.IsNullOrWhiteSpace(_txtDiscount.Text) ? "0" : _txtDiscount.Text.Trim(), out discount)
                || discount < 0)
            { UiHelper.Warn(string.Format(Messages.InvalidNumber, "تخفیف")); return; }

            int vehicleId = Convert.ToInt32(_cmbVehicle.SelectedValue);
            int technicianId = _cmbTechnician.SelectedValue == null ? 0 : Convert.ToInt32(_cmbTechnician.SelectedValue);

            // apply the loyalty suggestion only when the operator has not typed a discount
            if (discount == 0m)
            {
                decimal suggested = CalculateSuggestedDiscount(vehicleId, laborHours);
                if (suggested > 0m)
                {
                    var answer = MessageBox.Show(
                        "این مشتری مشمول تخفیف وفاداری است." + Environment.NewLine +
                        "مبلغ پیشنهادی: " + PersianDateHelper.ToCurrency(suggested) + Environment.NewLine +
                        "اعمال شود؟",
                        Messages.InfoTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);

                    if (answer == DialogResult.Yes)
                    {
                        discount = suggested;
                        _txtDiscount.Text = discount.ToString("0");
                    }
                }
            }

            var request = new ServiceRequest
            {
                RequestId      = _requestId ?? 0,
                VehicleId      = vehicleId,
                TechnicianId   = technicianId == 0 ? (int?)null : technicianId,
                RequestDate    = _dtpRequestDate.Value.Date,
                Description    = string.IsNullOrWhiteSpace(_txtDescription.Text) ? null : _txtDescription.Text.Trim(),
                LaborHours     = laborHours,
                DiscountAmount = discount
            };

            try
            {
                if (_requestId.HasValue)
                {
                    _repository.Update(request);

                    if (_cmbStatus.SelectedValue != null)
                    {
                        int statusId = Convert.ToInt32(_cmbStatus.SelectedValue);
                        if (_current == null || statusId != _current.StatusId)
                            _repository.ChangeStatus(_requestId.Value, statusId);
                    }
                }
                else
                {
                    _repository.Insert(request);
                    _requestId = request.RequestId;
                    _lblRequestNo.Text = "شماره درخواست: " + request.RequestNo;
                    _cmbStatus.Enabled = true;
                }

                LoadRequest();
                LoadParts();
                UiHelper.Info(Messages.SaveSucceeded);
            }
            catch (Exception ex) { UiHelper.ShowError(ex); }
        }

        private void BtnAddPart_Click(object sender, EventArgs e)
        {
            if (!_requestId.HasValue) { UiHelper.Warn(Messages.SaveRequestFirst); return; }
            if (_current != null && _current.IsFinal) { UiHelper.Warn(Messages.RequestIsFinal); return; }
            if (_cmbPart.SelectedValue == null) { UiHelper.Warn(Messages.SelectPart); return; }

            int quantity = (int)_numQuantity.Value;
            if (quantity <= 0) { UiHelper.Warn(Messages.QuantityMustBePositive); return; }

            try
            {
                _repository.AddPart(_requestId.Value, Convert.ToInt32(_cmbPart.SelectedValue), quantity);
                LoadParts();
                LoadRequest();
                _numQuantity.Value = 1;
            }
            catch (Exception ex) { UiHelper.ShowError(ex); }
        }

        private void BtnRemovePart_Click(object sender, EventArgs e)
        {
            if (_current != null && _current.IsFinal) { UiHelper.Warn(Messages.RequestIsFinal); return; }

            var line = _gridParts.CurrentRow == null
                ? null : _gridParts.CurrentRow.DataBoundItem as ServiceRequestPart;

            if (line == null) { UiHelper.Warn(Messages.NoRowSelected); return; }
            if (!UiHelper.Confirm(Messages.ConfirmDelete)) return;

            try
            {
                _repository.RemovePart(line.Id);
                LoadParts();
                LoadRequest();
            }
            catch (Exception ex) { UiHelper.ShowError(ex); }
        }
    }
}
