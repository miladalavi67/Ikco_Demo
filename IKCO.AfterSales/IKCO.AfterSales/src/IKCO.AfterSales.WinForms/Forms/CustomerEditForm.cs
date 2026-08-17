using System;
using System.Drawing;
using System.Windows.Forms;
using IKCO.AfterSales.WinForms.Common;
using IKCO.AfterSales.WinForms.Data;
using IKCO.AfterSales.WinForms.Models;

namespace IKCO.AfterSales.WinForms.Forms
{
    public class CustomerEditForm : Form
    {
        private readonly CustomerRepository _repository = new CustomerRepository();
        private readonly int? _customerId;

        private TextBox _txtFullName, _txtNationalCode, _txtMobile, _txtEmail, _txtCity, _txtAddress;
        private CheckBox _chkIsActive;

        public CustomerEditForm(int? customerId)
        {
            _customerId = customerId;
            InitializeComponent();

            if (_customerId.HasValue) LoadCustomer();
        }

        private void InitializeComponent()
        {
            UiHelper.ApplyRtl(this);
            Text = _customerId.HasValue ? "ویرایش مشتری" : "مشتری جدید";
            ClientSize = new Size(430, 330);
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

            _txtFullName     = new TextBox { Dock = DockStyle.Fill, MaxLength = 100 };
            _txtNationalCode = new TextBox { Dock = DockStyle.Fill, MaxLength = 10 };
            _txtMobile       = new TextBox { Dock = DockStyle.Fill, MaxLength = 11 };
            _txtEmail        = new TextBox { Dock = DockStyle.Fill, MaxLength = 100 };
            _txtCity         = new TextBox { Dock = DockStyle.Fill, MaxLength = 50 };
            _txtAddress      = new TextBox { Dock = DockStyle.Fill, MaxLength = 250, Multiline = true, Height = 60 };
            _chkIsActive     = new CheckBox { Text = "فعال", Checked = true, AutoSize = true };

            AddRow(layout, "نام و نام خانوادگی *", _txtFullName);
            AddRow(layout, "کد ملی *", _txtNationalCode);
            AddRow(layout, "موبایل *", _txtMobile);
            AddRow(layout, "پست الکترونیک", _txtEmail);
            AddRow(layout, "شهر", _txtCity);
            AddRow(layout, "آدرس", _txtAddress);
            AddRow(layout, string.Empty, _chkIsActive);

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

        private void LoadCustomer()
        {
            try
            {
                var customer = _repository.GetById(_customerId.Value);
                if (customer == null) return;

                _txtFullName.Text     = customer.FullName;
                _txtNationalCode.Text = customer.NationalCode;
                _txtMobile.Text       = customer.Mobile;
                _txtEmail.Text        = customer.Email;
                _txtCity.Text         = customer.City;
                _txtAddress.Text      = customer.Address;
                _chkIsActive.Checked  = customer.IsActive;
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(ex);
            }
        }

        private bool Validate(out string message)
        {
            message = null;

            if (!ValidationHelper.IsRequired(_txtFullName.Text))
            { message = string.Format(Messages.RequiredField, "نام و نام خانوادگی"); return false; }

            if (!ValidationHelper.IsValidNationalCode(_txtNationalCode.Text))
            { message = Messages.InvalidNationalCode; return false; }

            if (!ValidationHelper.IsValidMobile(_txtMobile.Text))
            { message = Messages.InvalidMobile; return false; }

            if (!ValidationHelper.IsValidEmail(_txtEmail.Text))
            { message = Messages.InvalidEmail; return false; }

            return true;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string message;
            if (!Validate(out message)) { UiHelper.Warn(message); return; }

            var customer = new Customer
            {
                CustomerId   = _customerId ?? 0,
                FullName     = _txtFullName.Text.Trim(),
                NationalCode = _txtNationalCode.Text.Trim(),
                Mobile       = _txtMobile.Text.Trim(),
                Email        = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim(),
                City         = string.IsNullOrWhiteSpace(_txtCity.Text) ? null : _txtCity.Text.Trim(),
                Address      = string.IsNullOrWhiteSpace(_txtAddress.Text) ? null : _txtAddress.Text.Trim(),
                IsActive     = _chkIsActive.Checked
            };

            try
            {
                if (_customerId.HasValue) _repository.Update(customer);
                else _repository.Insert(customer);

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
