using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using IL2DCE.Util;
using maddox.game;

namespace IL2DCE.Pages.Controls
{
    /// <summary>
    /// AircraftImageBorder.xaml の相互作用ロジック
    /// </summary>
    public partial class AircraftImageBorder : Border
    {
        public AircraftImageBorder()
        {
            InitializeComponent();
        }

        public void DisplayImage(GameIterface gameInterface, string aircraftClass)
        {
            string path;
            if (!string.IsNullOrEmpty(aircraftClass) &&
                !string.IsNullOrEmpty(path = new AircraftImage(gameInterface.ToFileSystemPath(Config.PartsFolder)).GetImagePath(aircraftClass)))
            {
                // using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var decoder = new TiffBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    BitmapSource source = decoder.Frames[0];
                    imageAircraft.Source = source;
                    Visibility = Visibility.Visible;
                }
            }
            else
            {
                imageAircraft.Source = null;
                Visibility = Visibility.Hidden;
            }
        }
    }
}
