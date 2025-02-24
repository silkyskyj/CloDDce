// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover
// Copyright (C) 2016 Stefan Rothdach
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

using System;
using System.Windows;
using System.Windows.Controls;
using IL2DCE.MissionObjectModel;
using maddox.game;
using maddox.game.play;

namespace IL2DCE
{
    namespace Pages
    {
        public class CareerIntroPage : PageDefImpl
        {
            private CareerIntro FrameworkElement
            {
                get
                {
                    return FE as CareerIntro;
                }
            }

            private IGame Game
            {
                get
                {
                    return _game;
                }
            }
            private IGame _game;

            public CareerIntroPage()
                : base("Career Intro", new CareerIntro())
            {
                FrameworkElement.Start.Click += new RoutedEventHandler(Start_Click);
                FrameworkElement.Back.Click += new RoutedEventHandler(Back_Click);
                FrameworkElement.comboBoxSelectArmy.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectArmy_SelectionChanged);
                FrameworkElement.comboBoxSelectAirForce.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAirForce_SelectionChanged);
                FrameworkElement.textBoxPilotName.TextChanged += new TextChangedEventHandler(textBoxPilotName_TextChanged);
            }

            public override void _enter(maddox.game.IGame play, object arg)
            {
                base._enter(play, arg);

                FrameworkElement.comboBoxSelectArmy.Items.Clear();

                _game = play as IGame;

                ComboBoxItem itemArmyRed = new ComboBoxItem();
                itemArmyRed.Content = Career.Army[0];
                itemArmyRed.Tag = 1;
                FrameworkElement.comboBoxSelectArmy.Items.Add(itemArmyRed);
                ComboBoxItem itemArmyBlue = new ComboBoxItem();
                itemArmyBlue.Content = Career.Army[1];
                itemArmyBlue.Tag = 2;
                FrameworkElement.comboBoxSelectArmy.Items.Add(itemArmyBlue);
                FrameworkElement.comboBoxSelectArmy.SelectedIndex = 0;
            }

            public override void _leave(maddox.game.IGame play, object arg)
            {
                base._leave(play, arg);

                _game = null;
            }

            void textBoxPilotName_TextChanged(object sender, TextChangedEventArgs e)
            {
                if (Game != null)
                {
                    string pilotName = FrameworkElement.textBoxPilotName.Text;
                    if (pilotName != null && pilotName != String.Empty)
                    {
                        foreach (Career career in Game.Core.AvailableCareers)
                        {
                            if (career.PilotName == pilotName)
                            {
                                FrameworkElement.Start.IsEnabled = false;
                                return;
                            }
                        }
                    }

                    FrameworkElement.Start.IsEnabled = true;
                }
            }

            void comboBoxSelectArmy_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count == 1)
                {
                    ComboBoxItem armySelected = e.AddedItems[0] as ComboBoxItem;
                    int armyIndex = (int)armySelected.Tag;

                    ComboBox comboBox = FrameworkElement.comboBoxSelectAirForce;
                    comboBox.Items.Clear();

                    if (armyIndex == (int)EArmy.Red)
                    {
                        for (int i = 0; i < (int)AirForceRed.Count; i++)
                        {
                            comboBox.Items.Add(new ComboBoxItem() { Tag = i + 1, Content = Career.AirForce[i] });
                        }
                    }
                    else if (armyIndex == (int)EArmy.Blue)
                    {
                        int diff = (int)AirForceRed.Count;
                        for (int i = 0; i < (int)AirForceBlue.Count; i++)
                        {
                            comboBox.Items.Add(new ComboBoxItem() { Tag = i + 1, Content = Career.AirForce[i + diff] });
                        }
                    }

                    comboBox.SelectedIndex = 0;
                }
            }

            void comboBoxSelectAirForce_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count == 1)
                {
                    ComboBoxItem armySelected = FrameworkElement.comboBoxSelectArmy.SelectedItem as ComboBoxItem;
                    int armyIndex = (int)armySelected.Tag;

                    ComboBoxItem airForceSelected = e.AddedItems[0] as ComboBoxItem;
                    int airForceIndex = (int)airForceSelected.Tag;

                    int airforce = (armyIndex - 1) * 3 + airForceIndex - 1;
                    FrameworkElement.textBoxPilotName.Text = Career.PilotNameDefault[airforce];

                    ComboBox comboBoxRank = FrameworkElement.comboBoxSelectRank;
                    comboBoxRank.Items.Clear();
                    for (int i = 0; i <= Career.RankMax; i++)
                    {
                        comboBoxRank.Items.Add(
                            new ComboBoxItem()
                            {
                                Content = Career.Rank[airforce][i],
                                Tag = i,
                            });
                    }
                    comboBoxRank.SelectedIndex = 0;
                }
            }

            private void Back_Click(object sender, RoutedEventArgs e)
            {
                if (Game.gameInterface.BattleIsRun())
                {
                    Game.gameInterface.BattleStop();
                }

                Game.gameInterface.PageChange(new SelectCareerPage(), null);
            }

            private void Start_Click(object sender, RoutedEventArgs e)
            {
                string pilotName = FrameworkElement.textBoxPilotName.Text;

                ComboBoxItem armySelected = FrameworkElement.comboBoxSelectArmy.SelectedItem as ComboBoxItem;
                int armyIndex = (int)armySelected.Tag;

                ComboBoxItem airForceSelected = FrameworkElement.comboBoxSelectAirForce.SelectedItem as ComboBoxItem;
                int airForceIndex = (int)airForceSelected.Tag;

                ComboBoxItem rankSelected = FrameworkElement.comboBoxSelectRank.SelectedItem as ComboBoxItem;
                int rankIndex = (int)rankSelected.Tag;

                try
                {
                    Career career = new Career(pilotName, armyIndex, airForceIndex, rankIndex);
                    career.BattleType = EBattleType.Campaign;
                    Game.Core.CurrentCareer = career;
                    Game.Core.AvailableCareers.Add(career);

                    Game.gameInterface.PageChange(new SelectCampaignPage(), null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("{0}", ex.Message));
                    Game.gameInterface.LogErrorToConsole(string.Format("{0} - {1}", "CareerIntroPage.Start_Click", ex.Message));
                }
            }
        }
    }
}