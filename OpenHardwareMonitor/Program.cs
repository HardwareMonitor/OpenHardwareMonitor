using System;
using System.Windows.Forms;
using OpenHardwareMonitor.UI;

namespace OpenHardwareMonitor;

internal static class Program
{
    [STAThread]
    public static void Main()
    {
        Crasher.Listen();

        if (!OSHelper.IsCompatible(false, out string errorMessage, out var fixAction))
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
