using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace IKCO.AfterSales.WinForms.Common
{
    /// <summary>
    /// Shared look-and-feel and message helpers for the forms.
    /// </summary>
    public static class UiHelper
    {
        public static readonly Font BaseFont    = new Font("Tahoma", 8.5f);
        public static readonly Color HeaderBack = Color.FromArgb(0, 82, 155);
        public static readonly Color HeaderFore = Color.White;
        public static readonly Color AltRowBack = Color.FromArgb(244, 247, 250);
        public static readonly Color WarnBack   = Color.FromArgb(255, 235, 235);

        public static void ApplyRtl(Form form)
        {
            form.RightToLeft = RightToLeft.Yes;
            form.RightToLeftLayout = true;
            form.Font = BaseFont;
            form.StartPosition = FormStartPosition.CenterParent;
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.AutoGenerateColumns = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.RowHeadersVisible = false;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowTemplate.Height = 28;
            grid.ColumnHeadersHeight = 32;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = HeaderBack;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = HeaderFore;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 8.5f, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.AlternatingRowsDefaultCellStyle.BackColor = AltRowBack;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(197, 219, 240);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;
        }

        public static DataGridViewTextBoxColumn TextColumn(string header, string property,
            int fillWeight = 100, string format = null,
            DataGridViewContentAlignment align = DataGridViewContentAlignment.MiddleRight)
        {
            var column = new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                DataPropertyName = property,
                Name = "col" + property,
                FillWeight = fillWeight
            };
            column.DefaultCellStyle.Alignment = align;
            if (!string.IsNullOrEmpty(format))
                column.DefaultCellStyle.Format = format;
            return column;
        }

        public static Button MakeButton(string text, int width = 90)
        {
            return new Button
            {
                Text = text,
                Width = width,
                Height = 30,
                FlatStyle = FlatStyle.System,
                UseVisualStyleBackColor = true
            };
        }

        public static void Info(string message)
        {
            MessageBox.Show(message, Messages.InfoTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);
        }

        public static void Warn(string message)
        {
            MessageBox.Show(message, Messages.ValidationTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);
        }

        public static bool Confirm(string message)
        {
            return MessageBox.Show(message, Messages.ConfirmDeleteTitle,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2, MessageBoxOptions.RtlReading) == DialogResult.Yes;
        }

        /// <summary>
        /// Turns a SQL exception into the message the stored procedure raised.
        /// </summary>
        public static void ShowError(Exception ex)
        {
            var sqlEx = ex as SqlException;
            string message = sqlEx != null && sqlEx.Number >= 50000
                ? sqlEx.Message
                : Messages.DatabaseError + Environment.NewLine + ex.Message;

            MessageBox.Show(message, Messages.ErrorTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);
        }
    }
}
