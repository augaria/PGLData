using System;
using System.Windows.Forms;

namespace PGLData
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainPanel());
            }
            catch
            {
                return;
            }
        }
    }
}
