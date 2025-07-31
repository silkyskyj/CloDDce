// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover
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
        public bool EnableFilter
        {
            get;
            set;
        }

        public TextBox TextBox;
        public Popup Popup;

        public FilterComboBox()
        {
            InitializeComponent();
        }

        protected virtual void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox = Template.FindName("PART_EditableTextBox", this) as TextBox;
            Popup = Template.FindName("PART_Popup", this) as Popup;

            if (TextBox != null)
            {
                TextBox.TextChanged += new TextChangedEventHandler(textBox_TextChanged);
            }
        }

        protected virtual void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EnableFilter)
            {
                if (Popup != null && !Popup.IsOpen && string.IsNullOrEmpty(TextBox.Text))
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
                        return (((string)(obj as ComboBoxItem).Content).ToLower().Contains(TextBox.Text.ToLower()));
                    }

                    return obj.ToString().ToLower().Contains(TextBox.Text.ToLower());
                };

                if (Popup != null)
                {
                    Popup.IsOpen = true;
                }
            }
        }

        protected virtual void ComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EnableFilter)
            {
                if (Popup != null && !Popup.IsOpen)
                {
                    Popup.IsOpen = true;
                }
            }
        }
    }
}
