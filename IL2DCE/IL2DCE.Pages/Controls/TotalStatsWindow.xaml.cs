using System.Text;
using System.Windows;

namespace IL2DCE.Pages.Controls
{
    /// <summary>
    /// TotalStatsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TotalStatsWindow : Window
    {
        private Career career;
        public TotalStatsWindow(Career career)
        {
            InitializeComponent();

            this.career = career;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Total Status[{0}]\n{1}", career.PilotName, career.ToTotalResultString());

            textBoxStats.Text = sb.ToString();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
