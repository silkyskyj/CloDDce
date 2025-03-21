// IL2DCE: A dynamic campaign engine & dynamic mission for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings
// Copyright (C) 2016 Stefan Rothdach & 2025 silkyskyj
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

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
