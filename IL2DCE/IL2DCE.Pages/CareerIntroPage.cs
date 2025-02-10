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
using maddox.game.play;

namespace IL2DCE
{
    namespace Pages
    {
        public class CareerIntroPage : PageDefImpl
        {
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
                itemArmyRed.Content = "Red";
                itemArmyRed.Tag = 1;
                FrameworkElement.comboBoxSelectArmy.Items.Add(itemArmyRed);
                ComboBoxItem itemArmyBlue = new ComboBoxItem();
                itemArmyBlue.Content = "Blue";
                itemArmyBlue.Tag = 2;
                FrameworkElement.comboBoxSelectArmy.Items.Add(itemArmyBlue);
                FrameworkElement.comboBoxSelectArmy.SelectedIndex = 0;
            }

            public override void _leave(maddox.game.IGame play, object arg)
            {
                base._leave(play, arg);

                _game = null;
            }

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

                    FrameworkElement.comboBoxSelectAirForce.Items.Clear();

                    if (armyIndex == 1)
                    {
                        ComboBoxItem itemRaf = new ComboBoxItem();
                        itemRaf.Tag = 1;
                        itemRaf.Content = "Royal Air Force";
                        FrameworkElement.comboBoxSelectAirForce.Items.Add(itemRaf);

                        ComboBoxItem itemFr = new ComboBoxItem();
                        itemFr.Tag = 2;
                        itemFr.Content = "Armee de l'air";
                        FrameworkElement.comboBoxSelectAirForce.Items.Add(itemFr);

                        ComboBoxItem itemUsa = new ComboBoxItem();
                        itemUsa.Tag = 3;
                        itemUsa.Content = "United States Army Air Forces";
                        FrameworkElement.comboBoxSelectAirForce.Items.Add(itemUsa);

                        FrameworkElement.comboBoxSelectAirForce.SelectedIndex = 0;
                    }
                    else if (armyIndex == 2)
                    {
                        ComboBoxItem itemLw = new ComboBoxItem();
                        itemLw.Tag = 1;
                        itemLw.Content = "Luftwaffe";
                        FrameworkElement.comboBoxSelectAirForce.Items.Add(itemLw);

                        ComboBoxItem itemRa = new ComboBoxItem();
                        itemRa.Tag = 2;
                        itemRa.Content = "Regia Aeronautica";
                        FrameworkElement.comboBoxSelectAirForce.Items.Add(itemRa);

                        FrameworkElement.comboBoxSelectAirForce.SelectedIndex = 0;
                    }
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

                    if (armyIndex == 1 && airForceIndex == 1)
                    {
                        FrameworkElement.textBoxPilotName.Text = "Joe Bloggs";
                    }
                    else if (armyIndex == 1 && airForceIndex == 2)
                    {
                        FrameworkElement.textBoxPilotName.Text = "Jean Dupont";
                    }
                    else if (armyIndex == 1 && airForceIndex == 3)
                    {
                        FrameworkElement.textBoxPilotName.Text = "John Smith";
                    }
                    else if (armyIndex == 2 && airForceIndex == 1)
                    {
                        FrameworkElement.textBoxPilotName.Text = "Max Mustermann";
                    }
                    else if (armyIndex == 2 && airForceIndex == 2)
                    {
                        FrameworkElement.textBoxPilotName.Text = "Mario Rossi";
                    }

                    FrameworkElement.comboBoxSelectRank.Items.Clear();
                    for (int i = 0; i < 6; i++)
                    {
                        ComboBoxItem itemRank = new ComboBoxItem();
                        if (armyIndex == 1 && airForceIndex == 1)
                        {
                            itemRank.Content = Career.RafRanks[i];
                        }
                        else if (armyIndex == 1 && airForceIndex == 2)
                        {
                            itemRank.Content = Career.AaRanks[i];
                        }
                        else if (armyIndex == 1 && airForceIndex == 3)
                        {
                            itemRank.Content = Career.UsaafRanks[i];
                        }
                        else if (armyIndex == 2 && airForceIndex == 1)
                        {
                            itemRank.Content = Career.LwRanks[i];
                        }
                        else if (armyIndex == 2 && airForceIndex == 2)
                        {
                            itemRank.Content = Career.RaRanks[i];
                        }
                        itemRank.Tag = i;
                        FrameworkElement.comboBoxSelectRank.Items.Add(itemRank);
                    }
                    FrameworkElement.comboBoxSelectRank.SelectedIndex = 0;
                }
            }

            private void Back_Click(object sender, RoutedEventArgs e)
            {
                if (Game.gameInterface.BattleIsRun())
                {
                    Game.gameInterface.BattleStop();
                }

                Game.gameInterface.PagePop(null);
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

                Career career = new Career(pilotName, armyIndex, airForceIndex, rankIndex);
                Game.Core.CurrentCareer = career;
                Game.Core.AvailableCareers.Add(career);

                Game.gameInterface.PageChange(new SelectCampaignPage(), null);
            }
        }
    }
}