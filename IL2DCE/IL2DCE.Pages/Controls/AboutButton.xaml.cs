// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover Blitz + DLC
// Copyright (C) 2016 Stefan Rothdach & 2025 silkysky
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

using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace IL2DCE.Pages.Controls
{
    /// <summary>
    /// AboutButton.xaml の相互作用ロジック
    /// </summary>
    public partial class AboutButton : Button
    {
        public AboutButton()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string message = string.Format("IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover Blitz + DLC\n{0}\nCopyright (C) 2011- Stefan Rothdach & 2025- silkysky\n\nDo you want to open the distribution and official website?\nUrl: https://github.com/silkysky/il2dce\n",
                Config.CreateVersionString(Assembly.GetExecutingAssembly().GetName().Version));
            if (MessageBox.Show(message, Config.AppName, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                Process.Start("https://github.com/silkysky/il2dce");
            }
        }
    }
}
