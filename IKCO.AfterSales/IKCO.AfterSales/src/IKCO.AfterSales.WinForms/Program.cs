using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using IKCO.AfterSales.WinForms.Common;
using IKCO.AfterSales.WinForms.Data;
using IKCO.AfterSales.WinForms.Forms;

namespace IKCO.AfterSales.WinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var culture = new CultureInfo("fa-IR");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            string error;
            if (!SqlHelper.TestConnection(out error))
            {
                MessageBox.Show(
                    Messages.DatabaseError + Environment.NewLine + Environment.NewLine + error,
                    Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new MainForm());
        }
    }
}
