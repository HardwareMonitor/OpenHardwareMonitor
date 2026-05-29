using System.Drawing;
using System.IO;
using System.Reflection;

namespace OpenHardwareMonitor.Utilities;

internal class EmbeddedResources
{
    public static Image GetImage(string name)
    {
        name = "OpenHardwareMonitor.Resources." + name;
        string[] names = Assembly.GetExecutingAssembly().GetManifestResourceNames();

        for (int i = 0; i < names.Length; i++)
        {
            if (names[i].Replace('\\', '.') == name)
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(names[i]))
                {
                    // "You must keep the stream open for the lifetime of the Image."
                    Image image = Image.FromStream(stream);

                    // so we just create a copy of the image
                    Bitmap bitmap = new Bitmap(image);

                    // and dispose it right here
                    image.Dispose();

                    return bitmap;
                }
            }
        }
        return new Bitmap(1, 1);
    }

    public static Icon GetIcon(string name)
    {
        name = "OpenHardwareMonitor.Resources." + name;
        string[] names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i].Replace('\\', '.') == name)
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(names[i]))
                {
                    return new Icon(stream);
                }
            }
        }
        return null;
    }

}
