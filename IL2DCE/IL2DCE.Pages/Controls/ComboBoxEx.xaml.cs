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

using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace IL2DCE.Pages.Controls
{
    /// <summary>
    /// ComboBoxEx.xaml の相互作用ロジック
    /// </summary>
    public partial class ComboBoxEx : ComboBox
    {
        public TextBox TextBox;
        public Popup Popup;

        public ComboBoxEx()
        {
            InitializeComponent();
        }

        private void ComboBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            TextBox = Template.FindName("PART_EditableTextBox", this) as TextBox;
            Popup = Template.FindName("PART_Popup", this) as Popup;
        }
    }
}
