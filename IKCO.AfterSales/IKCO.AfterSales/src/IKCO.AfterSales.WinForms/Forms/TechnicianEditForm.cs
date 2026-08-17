using System;
using System.Drawing;
using System.Windows.Forms;
using IKCO.AfterSales.WinForms.Common;
using IKCO.AfterSales.WinForms.Data;
using IKCO.AfterSales.WinForms.Models;

namespace IKCO.AfterSales.WinForms.Forms
{
    public class TechnicianEditForm : Form
    {
        private readonly TechnicianRepository _repository = new TechnicianRepository();
        private readonly int? _technicianId;

        private TextBox _txtFullName, _txtPersonnelCode, _txtSpecialty, _txtHourlyRate;
        private CheckBox _chkIsActive;

        public TechnicianEditForm(int? technicianId)
        {
            _technicianId = technicianId;
            InitializeComponent();
            if (_technicianId.HasValue) LoadTechnician();
        }

        private void InitializeComponent()
        {
            UiHelper.ApplyRtl(this);
            Text = _technicianId.HasValue ? "ویرایش تعمیرکار" : "تعمیرکار جدید";
            ClientSize = new Size(410, 230);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var layout = new TableLayoutPanel
            { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(14), RightToLeft = RightToLeft.Yes };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _txtFullName      = new TextBox { Dock = DockStyle.Fill, MaxLength = 100 };
            _txtPersonnelCode = new TextBox { Dock = DockStyle.Fill, MaxLength = 10 };
            _txtSpecialty     = new TextBox { Dock = DockStyle.Fill, MaxLength = 60 };
            _txtHourlyRate    = new TextBox { Dock = DockStyle.Fill, MaxLength = 12, Text = "0" };
            _chkIsActive      = new CheckBox { Text = "فعال", Checked = true, AutoSize = true };

            AddRow(layout, "نام و نام خانوادگی *", _txtFullName);
            AddRow(layout, "کد پرسنلی *", _txtPersonnelCode);
            AddRow(layout, "تخصص", _txtSpecialty);
            AddRow(layout, "نرخ ساعتی (ریال)", _txtHourlyRate);
            AddRow(layout, string.Empty, _chkIsActive);

            var buttons = new FlowLayoutPanel
            { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.LeftToRight, Height = 46, Padding = new Padding(10) };
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
            { Text = caption, AutoSize = true, Anchor = AnchorStyles.Right, Padding = new Padding(0, 6, 0, 0) });
            layout.Controls.Add(control);
        }

        private void LoadTechnician()
        {
            try
            {
                var technician = _repository.GetById(_technicianId.Value);
                if (technician == null) return;

                _txtFullName.Text      = technician.FullName;
                _txtPersonnelCode.Text = technician.PersonnelCode;
                _txtSpecialty.Text     = technician.Specialty;
                _txtHourlyRate.Text    = technician.HourlyRate.ToString("0");
                _chkIsActive.Checked   = technician.IsActive;
            }
            catch (Exception ex) { UiHelper.ShowError(ex); }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidationHelper.IsRequired(_txtFullName.Text))
            { UiHelper.Warn(string.Format(Messages.RequiredField, "نام و نام خانوادگی")); return; }

            if (!ValidationHelper.IsRequired(_txtPersonnelCode.Text))
            { UiHelper.Warn(string.Format(Messages.RequiredField, "کد پرسنلی")); return; }

            decimal rate;
            if (!decimal.TryParse(string.IsNullOrWhiteSpace(_txtHourlyRate.Text) ? "0" : _txtHourlyRate.Text.Trim(), out rate))
            { UiHelper.Warn(string.Format(Messages.InvalidNumber, "نرخ ساعتی")); return; }

            var technician = new Technician
            {
                TechnicianId  = _technicianId ?? 0,
                FullName      = _txtFullName.Text.Trim(),
                PersonnelCode = _txtPersonnelCode.Text.Trim(),
                Specialty     = string.IsNullOrWhiteSpace(_txtSpecialty.Text) ? null : _txtSpecialty.Text.Trim(),
                HourlyRate    = rate,
                IsActive      = _chkIsActive.Checked
            };

            try
            {
                if (_technicianId.HasValue) _repository.Update(technician);
                else _repository.Insert(technician);

                UiHelper.Info(Messages.SaveSucceeded);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { UiHelper.ShowError(ex); }
        }
    }
}
