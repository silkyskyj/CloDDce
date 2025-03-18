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
