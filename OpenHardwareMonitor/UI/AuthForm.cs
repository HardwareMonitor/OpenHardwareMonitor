using System;
using System.Windows.Forms;

namespace OpenHardwareMonitor.UI;

internal partial class AuthForm : Form
{
    private readonly MainForm _parent;

    public AuthForm(MainForm m)
    {
        InitializeComponent();
        Icon = System.Drawing.Icon.ExtractAssociatedIcon(Updater.CurrentFileLocation);
        _parent = m;
        Theme.Current.Apply(this);
    }

    private void AuthForm_Load(object sender, EventArgs e)
    {
        httpAuthUsernameTextBox.Enabled = httpAuthPasswordTextBox.Enabled = enableHTTPAuthCheckBox.Checked = _parent.Server.AuthEnabled;
        httpAuthUsernameTextBox.Text = _parent.Server.UserName;
    }

    private void HttpAuthCancelButton_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void HttpAuthOkButton_Click(object sender, EventArgs e)
    {
        _parent.Server.UserName = httpAuthUsernameTextBox.Text;
        if (!string.IsNullOrWhiteSpace(httpAuthPasswordTextBox.Text))
        {
            _parent.Server.SetPassword(httpAuthPasswordTextBox.Text);
        }
        _parent.Server.AuthEnabled = enableHTTPAuthCheckBox.Checked;
        _parent.AuthWebServerMenuItemChecked = _parent.Server.AuthEnabled;
        Close();
    }

    private void EnableHTTPAuthCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        httpAuthUsernameTextBox.Enabled = httpAuthPasswordTextBox.Enabled = enableHTTPAuthCheckBox.Checked;
    }
}
