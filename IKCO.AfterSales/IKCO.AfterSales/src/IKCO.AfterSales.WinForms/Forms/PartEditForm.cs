using System;
using System.Drawing;
using System.Windows.Forms;
using IKCO.AfterSales.WinForms.Common;
using IKCO.AfterSales.WinForms.Data;
using IKCO.AfterSales.WinForms.Models;

namespace IKCO.AfterSales.WinForms.Forms
{
    public class PartEditForm : Form
    {
        private readonly PartRepository _repository = new PartRepository();
        private readonly int? _partId;

        private TextBox _txtPartCode, _txtPartName, _txtUnitPrice, _txtStockQty, _txtMinStockQty;
        private CheckBox _chkIsActive;

        public PartEditForm(int? partId)
        {
            _partId = partId;
            InitializeComponent();
            if (_partId.HasValue) LoadPart();
        }

        private void InitializeComponent()
        {
            UiHelper.ApplyRtl(this);
            Text = _partId.HasValue ? "ویرایش قطعه" : "قطعه جدید";
            ClientSize = new Size(410, 260);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var layout = new TableLayoutPanel
            { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(14), RightToLeft = RightToLeft.Yes };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _txtPartCode    = new TextBox { Dock = DockStyle.Fill, MaxLength = 20 };
            _txtPartName    = new TextBox { Dock = DockStyle.Fill, MaxLength = 120 };
            _txtUnitPrice   = new TextBox { Dock = DockStyle.Fill, MaxLength = 12, Text = "0" };
            _txtStockQty    = new TextBox { Dock = DockStyle.Fill, MaxLength = 6, Text = "0" };
            _txtMinStockQty = new TextBox { Dock = DockStyle.Fill, MaxLength = 6, Text = "0" };
            _chkIsActive    = new CheckBox { Text = "فعال", Checked = true, AutoSize = true };

            AddRow(layout, "کد قطعه *", _txtPartCode);
            AddRow(layout, "نام قطعه *", _txtPartName);
            AddRow(layout, "قیمت واحد (ریال) *", _txtUnitPrice);
            AddRow(layout, "موجودی انبار", _txtStockQty);
            AddRow(layout, "حداقل موجودی", _txtMinStockQty);
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

        private void LoadPart()
        {
            try
            {
                var part = _repository.GetById(_partId.Value);
                if (part == null) return;

                _txtPartCode.Text    = part.PartCode;
                _txtPartName.Text    = part.PartName;
                _txtUnitPrice.Text   = part.UnitPrice.ToString("0");
                _txtStockQty.Text    = part.StockQty.ToString();
                _txtMinStockQty.Text = part.MinStockQty.ToString();
                _chkIsActive.Checked = part.IsActive;
            }
            catch (Exception ex) { UiHelper.ShowError(ex); }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidationHelper.IsRequired(_txtPartCode.Text))
            { UiHelper.Warn(string.Format(Messages.RequiredField, "کد قطعه")); return; }

            if (!ValidationHelper.IsRequired(_txtPartName.Text))
            { UiHelper.Warn(string.Format(Messages.RequiredField, "نام قطعه")); return; }

            decimal price;
            if (!decimal.TryParse(_txtUnitPrice.Text.Trim(), out price) || price < 0)
            { UiHelper.Warn(string.Format(Messages.InvalidNumber, "قیمت واحد")); return; }

            int stock, minStock;
            if (!int.TryParse(_txtStockQty.Text.Trim(), out stock) || stock < 0)
            { UiHelper.Warn(string.Format(Messages.InvalidNumber, "موجودی انبار")); return; }

            if (!int.TryParse(_txtMinStockQty.Text.Trim(), out minStock) || minStock < 0)
            { UiHelper.Warn(string.Format(Messages.InvalidNumber, "حداقل موجودی")); return; }

            var part = new Part
            {
                PartId      = _partId ?? 0,
                PartCode    = _txtPartCode.Text.Trim(),
                PartName    = _txtPartName.Text.Trim(),
                UnitPrice   = price,
                StockQty    = stock,
                MinStockQty = minStock,
                IsActive    = _chkIsActive.Checked
            };

            try
            {
                if (_partId.HasValue) _repository.Update(part);
                else _repository.Insert(part);

                UiHelper.Info(Messages.SaveSucceeded);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { UiHelper.ShowError(ex); }
        }
    }
}
