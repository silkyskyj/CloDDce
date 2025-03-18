using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using IL2DCE.MissionObjectModel;
using static IL2DCE.Pages.Controls.ProgressWindow;

namespace IL2DCE.Pages.Controls
{
    /// <summary>
    /// GeneralSettingsGroupBox.xaml の相互作用ロジック
    /// </summary>
    public partial class GeneralSettingsGroupBox : GroupBox
    {
        public event SelectionChangedEventHandler ComboBoxSelectionChangedEvent;

        public event TextChangedEventHandler ComboBoxTextChangedEvent;

        #region Property

        public int SelectedAdditionalAirOperationsComboBox
        {
            get
            {
                int? selected = comboBoxSelectAdditionalAirOperations.SelectedItem as int?;
                if (selected != null)
                {
                    return selected.Value;
                }

                return -1;
            }
        }

        public int SelectedAdditionalGroundOperationsComboBox
        {
            get
            {
                int? selected = comboBoxSelectAdditionalGroundOperations.SelectedItem as int?;
                if (selected != null)
                {
                    return selected.Value;
                }
                else
                {
                    string text = comboBoxSelectAdditionalGroundOperations.Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        int num;
                        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out num))
                        {
                            return num;
                        }
                    }
                }
                return -1;
            }
        }

        public bool SelectedSpawnRandomLocationFriendly
        {
            get
            {
                bool? isCheckd = checkBoxSpawnRandomLocationFriendly.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedSpawnRandomLocationEnemy
        {
            get
            {
                bool? isCheckd = checkBoxSpawnRandomLocationEnemy.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedSpawnRandomLocationPlayer
        {
            get
            {
                bool? isCheckd = checkBoxSpawnRandomLocationPlayer.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedSpawnRandomAltitudeFriendly
        {
            get
            {
                bool? isCheckd = checkBoxSpawnRandomAltitudeFriendly.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedSpawnRandomAltitudeEnemy
        {
            get
            {
                bool? isCheckd = checkBoxSpawnRandomAltitudeEnemy.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedSpawnRandomTimeFriendly
        {
            get
            {
                bool? isCheckd = checkBoxSpawnRandomTimeFriendly.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedSpawnRandomTimeEnemy
        {
            get
            {
                bool? isCheckd = checkBoxSpawnRandomTimeEnemy.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public int SelectedRandomTimeBeginComboBox
        {
            get
            {
                int? selected = comboBoxSelectRandomTimeBegin.SelectedItem as int?;
                if (selected != null)
                {
                    return selected.Value;
                }
                else
                {
                    string text = comboBoxSelectRandomTimeBegin.Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        int num;
                        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out num))
                        {
                            return num;
                        }
                    }
                }
                return -1;
            }
        }

        public int SelectedRandomTimeEndComboBox
        {
            get
            {
                int? selected = comboBoxSelectRandomTimeEnd.SelectedItem as int?;
                if (selected != null)
                {
                    return selected.Value;
                }
                else
                {
                    string text = comboBoxSelectRandomTimeEnd.Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        int num;
                        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out num))
                        {
                            return num;
                        }
                    }
                }
                return -1;
            }
        }

        #endregion

        public GeneralSettingsGroupBox()
        {
            InitializeComponent();

            comboBoxSelectAdditionalAirOperations.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            comboBoxSelectAdditionalGroundOperations.Loaded += new RoutedEventHandler(comboBox_Loaded);
            comboBoxSelectAdditionalGroundOperations.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            comboBoxSelectRandomTimeBegin.Loaded += new RoutedEventHandler(comboBox_Loaded);
            comboBoxSelectRandomTimeBegin.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            comboBoxSelectRandomTimeEnd.Loaded += new RoutedEventHandler(comboBox_Loaded);
            comboBoxSelectRandomTimeEnd.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            checkBoxSpawnRandomTimeFriendly.Checked += new RoutedEventHandler(checkBoxSpawnRandomTime_CheckedChange);
            checkBoxSpawnRandomTimeFriendly.Unchecked += new RoutedEventHandler(checkBoxSpawnRandomTime_CheckedChange);
            checkBoxSpawnRandomTimeEnemy.Checked += new RoutedEventHandler(checkBoxSpawnRandomTime_CheckedChange);
            checkBoxSpawnRandomTimeEnemy.Unchecked += new RoutedEventHandler(checkBoxSpawnRandomTime_CheckedChange);

            UpdateSelectAdditionalAirOperationsComboBox();
            UpdateSelectAdditionalGroundOperationsComboBox();
            UpdateSelectRandomTimeBeginComboBox();
            UpdateSelectRandomTimeEndComboBox();
        }

        private void GroupBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        private void comboBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ComboBoxEx comboBox = sender as ComboBoxEx;
            if (comboBox != null)
            {
                comboBox.TextBox.TextChanged += new TextChangedEventHandler(comboBox_TextChanged);
            }
        }

        private void comboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ComboBoxTextChangedEvent != null)
            {
                ComboBoxTextChangedEvent(sender, e);
            }
        }

        private void comboBoxSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxSelectionChangedEvent != null)
            {
                ComboBoxSelectionChangedEvent(sender, e);
            }
        }

        private void checkBoxSpawnRandomTime_CheckedChange(object sender, RoutedEventArgs e)
        {
            comboBoxSelectRandomTimeBegin.IsEnabled =
                comboBoxSelectRandomTimeEnd.IsEnabled = SelectedSpawnRandomTimeFriendly || SelectedSpawnRandomTimeEnemy;
        }

        public void UpdateSelectAdditionalAirOperationsComboBox()
        {
            ComboBox comboBox = comboBoxSelectAdditionalAirOperations;
            foreach (var item in Enumerable.Range(Config.MinAdditionalAirOperations, Config.MaxAdditionalAirOperations))
            {
                comboBox.Items.Add(item);
            }
            comboBox.SelectedItem = Config.DefaultAdditionalAirOperations;
        }

        public void UpdateSelectAdditionalGroundOperationsComboBox()
        {
            ComboBox comboBox = comboBoxSelectAdditionalGroundOperations;
            for (int i = Config.MinAdditionalGroundOperations; i <= Config.MaxAdditionalGroundOperations; i += 10)
            {
                comboBox.Items.Add(i);
            }
            comboBox.SelectedItem = Config.DefaultAdditionalGroundOperations;
        }

        public void UpdateSelectRandomTimeBeginComboBox()
        {
            ComboBox comboBox = comboBoxSelectRandomTimeBegin;
            for (int i = Spawn.SpawnTime.MinimumBeginSec; i <= Spawn.SpawnTime.MaximumEndSec; i += i)
            {
                comboBox.Items.Add(i);
            }
            if (comboBox.Items.IndexOf(Spawn.SpawnTime.MaximumEndSec) == -1)
            {
                comboBox.Items.Add(Spawn.SpawnTime.MaximumEndSec);
            }
            comboBox.SelectedItem = Spawn.SpawnTime.DefaultBeginSec;
            comboBox.IsEnabled = SelectedSpawnRandomTimeFriendly || SelectedSpawnRandomTimeEnemy;
        }

        public void UpdateSelectRandomTimeEndComboBox()
        {
            ComboBox comboBox = comboBoxSelectRandomTimeEnd;
            for (int i = Spawn.SpawnTime.MinimumBeginSec; i <= Spawn.SpawnTime.MaximumEndSec; i += i)
            {
                comboBox.Items.Add(i);
            }
            if (comboBox.Items.IndexOf(Spawn.SpawnTime.MaximumEndSec) == -1)
            {
                comboBox.Items.Add(Spawn.SpawnTime.MaximumEndSec);
            }
            comboBox.SelectedItem = Spawn.SpawnTime.DefaultEndSec;
            comboBox.IsEnabled = SelectedSpawnRandomTimeFriendly || SelectedSpawnRandomTimeEnemy;
        }
    }
}
