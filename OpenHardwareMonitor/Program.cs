using System;
using System.Windows.Forms;
using OpenHardwareMonitor.UI;

namespace OpenHardwareMonitor;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        Crasher.Listen();

        if (!OperatingSystemHelper.IsCompatible(false, out string errorMessage, out var fixAction))
        {
            if (fixAction != null)
            {
                if (MessageBox.Show(errorMessage, Updater.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    fixAction();
                }
            }
            else
            {
                MessageBox.Show(errorMessage, Updater.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            Environment.Exit(0);
        }

        if (!System.Diagnostics.Debugger.IsAttached && WinApiHelper.CheckRunningInstances(true, true))
        {
            // fallback
            MessageBox.Show($"{Updater.ApplicationName} is already running.", Updater.ApplicationName,
              MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return;
        }

        if (Environment.Is64BitOperatingSystem != Environment.Is64BitProcess) {
            if (MessageBox.Show("You are running an application build made for a different OS architecture.\nIt is not compatible!\nWould you like to download correct version?", Updater.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                Updater.VisitAppSite("releases");
            }
            Environment.Exit(0);
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        using (MainForm form = new MainForm())
        {
            form.FormClosed += delegate
            {
                Application.Exit();
            };
            Application.Run();
        }
    }
}
