using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace IL2DCE.Pages.Controls
{
    /// <summary>
    /// VersionLabel.xaml の相互作用ロジック
    /// </summary>
    public partial class VersionLabel : Label
    {
        public VersionLabel()
        {
            InitializeComponent();
        }

        private void Label_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Content = Config.CreateVersionString(Assembly.GetExecutingAssembly().GetName().Version);
        }
    }
}
