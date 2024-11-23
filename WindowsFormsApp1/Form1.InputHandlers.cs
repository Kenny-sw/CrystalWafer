using System.Windows.Forms;
using System;

namespace WindowsFormsApp1
{
    public partial class Form1
    {
        private void SizeX_MouseClick(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(SizeX.Text))
            {
                SizeX.Focus();
                SizeX.SelectionStart = 0;
            }
        }

        private void SizeY_MouseClick(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(SizeY.Text))
            {
                SizeY.Focus();
                SizeY.SelectionStart = 0;
            }
        }

        private void WaferDiameter_MouseClick(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(WaferDiameter.Text))
            {
                WaferDiameter.Focus();
                WaferDiameter.SelectionStart = 0;
            }
        }

        private void SizeX_TextChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate(); // Перерисовка при изменении текста
        }

        private void SizeY_TextChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate(); // Перерисовка при изменении текста
        }

        private void WaferDiameter_TextChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate(); // Перерисовка при изменении текста
        }

        private void Create_Click(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }
    }
}