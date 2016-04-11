using System;
using System.Drawing;
using System.Windows.Forms;

namespace PGLData
{
    //message box for this app
    public partial class SelfDesignedMsg : Form
    {

        public SelfDesignedMsg(string msg, bool extraButton)
        {
            InitializeComponent();

            if (GlobalConstants.dpiX == 120)
            {
                foreach (Control ct in this.Controls)
                    ct.Font = new System.Drawing.Font(ct.Font.FontFamily, (float)(ct.Font.Size / 1.25), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }

            label1.Text = msg;
            if (this.Width < label1.Width + 168)
            {
                this.Width = label1.Width + 168;
                button1.Location = new Point(this.Width - 217, button1.Location.Y);
                button2.Location = new Point(this.Width - 113, button2.Location.Y);
                button3.Location = new Point(this.Width - 113, button3.Location.Y);
                pictureBox2.Size = new Size(this.Width, pictureBox2.Height);
            }
            if (this.Height < label1.Height + 170)
            {
                this.Height = label1.Height + 170;
                button1.Location = new Point(button1.Location.X, this.Height - 76);
                button2.Location = new Point(button2.Location.X, this.Height - 76);
                button3.Location = new Point(button3.Location.X, this.Height - 76);
                pictureBox2.Location = new Point(0, this.Height - 84);
            }

            if (extraButton)
            {
                button1.Visible = true;
                button1.Enabled = true;
                button2.Visible = true;
                button2.Enabled = true;
                button3.Visible = false;
                button3.Enabled = false;
            }
            else
            {

                button1.Visible = false;
                button1.Enabled = false;
                button2.Visible = false;
                button2.Enabled = false;
                button3.Visible = true;
                button3.Enabled = true;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
