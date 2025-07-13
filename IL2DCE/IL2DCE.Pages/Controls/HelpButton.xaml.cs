// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover Blitz + DLC
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

using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace IL2DCE.Pages.Controls
{
    /// <summary>
    /// HelpButton.xaml の相互作用ロジック
    /// </summary>
    public partial class HelpButton : Button
    {
        public string Message
        {
            get;
            set;
        }

        public HelpButton()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBox.Show(Regex.Unescape(Message), Config.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
