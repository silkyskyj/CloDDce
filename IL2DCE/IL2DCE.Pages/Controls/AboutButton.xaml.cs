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
            string message = string.Format("IL2DCE: A dynamic campaign engine & dynamic mission for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings\n{0}\nCopyright (C) 2011-2025 Stefan Rothdach & 2025 silkyskyj\n\nDo you want to open the distribution and support site ?\nUrl: https://github.com/silkyskyj/il2dce\n",
                Config.CreateVersionString(Assembly.GetExecutingAssembly().GetName().Version));
            if (MessageBox.Show(message, "IL2DCE", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                Process.Start("https://github.com/silkyskyj/il2dce");
            }
        }
    }
}
