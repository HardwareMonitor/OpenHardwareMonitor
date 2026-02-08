using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenHardwareMonitor.Utilities;

namespace OpenHardwareMonitor.UI;

public sealed partial class AboutBox : Form
{
    public AboutBox()
    {
        InitializeComponent();
        lblAppTitle.Text = Updater.ApplicationTitle;
        lblCopyright.Text = $"Copyright © {Updater.ApplicationName}";
        Font = SystemFonts.MessageBoxFont;
        lblVersion.Text = $"Version {Application.ProductVersion} {(Environment.Is64BitProcess ? "x64" : "x86")}";
#if DEBUG
        lblVersion.Text += " (DEBUG)";
#endif
        //using (var icon = Icon.ExtractAssociatedIcon(Updater.CurrentFileLocation))
        using (var icon = EmbeddedResources.GetIcon("icon.ico"))
            picLogo.Image = icon.GetLargestBitmap();
        picLogo.SizeMode = PictureBoxSizeMode.StretchImage;
        projectLinkLabel.Links.Remove(projectLinkLabel.Links[0]);
        projectLinkLabel.Links.Add(0, projectLinkLabel.Text.Length, $"https://github.com/{Updater.ApplicationCompany}/{Updater.ApplicationName}");
        licenseLinkLabel.Links.Remove(licenseLinkLabel.Links[0]);
        licenseLinkLabel.Links.Add(0, licenseLinkLabel.Text.Length, "https://www.gnu.org/licenses/gpl-3.0.html");
        Theme.Current.Apply(this);
    }

    private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(e.Link.LinkData.ToString()));
        }
        catch { }
    }
}
