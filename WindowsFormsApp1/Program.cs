using System;
using System.Globalization;
using System.Windows.Forms;

namespace CrystalTable
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureSettings.NumericCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureSettings.NumericCulture;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
