using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PGLData
{
    public partial class AboutPGLData : Form
    {
        public AboutPGLData()
        {
            InitializeComponent();
            label4.Text = GlobalConstants.APPVERSION;
        }
    }
}
