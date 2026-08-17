using System;
using System.Drawing;
using System.Windows.Forms;
using IKCO.AfterSales.WinForms.Common;

namespace IKCO.AfterSales.WinForms.Forms
{
    public class MainForm : Form
    {
        private MenuStrip _menu;
        private StatusStrip _status;
        private ToolStripStatusLabel _statusLabel;
        private Panel _banner;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            UiHelper.ApplyRtl(this);
            Text = Messages.AppTitle;
            WindowState = FormWindowState.Maximized;
            IsMdiContainer = true;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(900, 600);

            _menu = new MenuStrip { RightToLeft = RightToLeft.Yes, Font = UiHelper.BaseFont };

            var baseInfo = new ToolStripMenuItem("اطلاعات پایه");
            baseInfo.DropDownItems.Add(NewItem("مشتریان", (s, e) => Open(new CustomerListForm())));
            baseInfo.DropDownItems.Add(NewItem("خودروها", (s, e) => Open(new VehicleListForm())));
            baseInfo.DropDownItems.Add(NewItem("تعمیرکاران", (s, e) => Open(new TechnicianListForm())));
            baseInfo.DropDownItems.Add(NewItem("قطعات", (s, e) => Open(new PartListForm())));

            var operations = new ToolStripMenuItem("عملیات");
            operations.DropDownItems.Add(NewItem("درخواست‌های تعمیر", (s, e) => Open(new ServiceRequestListForm())));

            var reports = new ToolStripMenuItem("گزارش‌ها");
            reports.DropDownItems.Add(NewItem("گزارش‌های مدیریتی", (s, e) => Open(new ReportsForm())));

            var help = new ToolStripMenuItem("راهنما");
            help.DropDownItems.Add(NewItem("درباره", (s, e) => UiHelper.Info(
                Messages.AppTitle + Environment.NewLine + "نسخه ۱.۰.۰" + Environment.NewLine +
                "پروژه نمونه برای مهاجرت به وب")));
            help.DropDownItems.Add(NewItem("خروج", (s, e) => Close()));

            _menu.Items.AddRange(new ToolStripItem[] { baseInfo, operations, reports, help });

            _banner = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = UiHelper.HeaderBack };
            var title = new Label
            {
                Text = Messages.AppTitle,
                ForeColor = Color.White,
                Font = new Font("Tahoma", 14f, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                RightToLeft = RightToLeft.Yes
            };
            _banner.Controls.Add(title);

            _statusLabel = new ToolStripStatusLabel
            {
                Text = "تاریخ امروز: " + PersianDateHelper.ToPersianDate(DateTime.Now),
                Spring = true,
                TextAlign = ContentAlignment.MiddleRight
            };
            _status = new StatusStrip { RightToLeft = RightToLeft.Yes };
            _status.Items.Add(_statusLabel);

            Controls.Add(_banner);
            Controls.Add(_status);
            Controls.Add(_menu);
            MainMenuStrip = _menu;
        }

        private static ToolStripMenuItem NewItem(string text, EventHandler handler)
        {
            var item = new ToolStripMenuItem(text);
            item.Click += handler;
            return item;
        }

        private void Open(Form form)
        {
            foreach (var child in MdiChildren)
            {
                if (child.GetType() == form.GetType())
                {
                    form.Dispose();
                    child.Activate();
                    return;
                }
            }

            form.MdiParent = this;
            form.WindowState = FormWindowState.Maximized;
            form.Show();
        }
    }
}
