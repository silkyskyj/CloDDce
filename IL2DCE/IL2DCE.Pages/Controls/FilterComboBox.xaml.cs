// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings
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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace IL2DCE.Pages.Controls
{
    /// <summary>
    /// FilterComboBox.xaml の相互作用ロジック
    /// </summary>
    public partial class FilterComboBox : ComboBox
    {
        private TextBox textBox;
        private Popup popup;

        public FilterComboBox()
        {
            InitializeComponent();
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            textBox = Template.FindName("PART_EditableTextBox", this) as TextBox;
            popup = Template.FindName("PART_Popup", this) as Popup;

            if (textBox != null)
            {
                textBox.TextChanged += new TextChangedEventHandler(textBox_TextChanged);
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (popup != null && !popup.IsOpen && string.IsNullOrEmpty(textBox.Text))
            {
                Items.Filter += obj =>
                {
                    return true;
                };

                return;
            }

            Items.Filter += obj =>
            {
                if (obj is ComboBoxItem)
                {
                    return (((string)(obj as ComboBoxItem).Content).ToLower().Contains(textBox.Text.ToLower()));
                }

                return obj.ToString().ToLower().Contains(textBox.Text.ToLower());
            };

            if (popup != null)
            {
                popup.IsOpen = true;
            }
        }

        private void ComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (popup != null && !popup.IsOpen)
            {
                popup.IsOpen = true;
            }
        }
    }
}
