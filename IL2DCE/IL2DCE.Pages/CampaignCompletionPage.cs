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

using System.Linq;
using System.Windows;
using IL2DCE.MissionObjectModel;
using maddox.game.play;

namespace IL2DCE.Pages
{
    public class CampaignCompletionPage : PageDefImpl
    {
        private CampaignCompletion FrameworkElement
        {
            get
            {
                return FE as CampaignCompletion;
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

        public CampaignCompletionPage()
            : base("Campaign Completion", new CampaignCompletion())
        {
            FrameworkElement.Complete.Click += new System.Windows.RoutedEventHandler(Complete_Click);
        }

        public override void _enter(maddox.game.IGame play, object arg)
        {
            base._enter(play, arg);

            _game = play as IGame;

            Career career = Game.Core.CurrentCareer;
            CampaignInfo campaignInfo = career.CampaignInfo;

            FrameworkElement.textBoxInfo.Text = campaignInfo.ToSummaryString();
            FrameworkElement.textBoxStatus.Text = string.Format("Current Status\n{0}\nTotal Result\n{1}\n",
                                                                career.ToStringCurrestStatus(),
                                                                career.ToStringTotalResult());

            UpdateAircraftImage(career);
        }

        public override void _leave(maddox.game.IGame play, object arg)
        {
            base._leave(play, arg);

            _game = null;
        }

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            Game.gameInterface.PageChange(new SelectCareerPage(), null);
        }

        private void UpdateAircraftImage(Career career)
        {
            CampaignInfo campaignInfo = career.CampaignInfo;
            MissionFile missionFile = new MissionFile(Game, campaignInfo.InitialMissionTemplateFiles, campaignInfo.AirGroupInfos);
            AirGroup airGroup = missionFile.AirGroups.Where(x => x.ArmyIndex == career.ArmyIndex && string.Compare(x.ToString(), career.AirGroup) == 0).FirstOrDefault();
            FrameworkElement.borderImage.DisplayImage(Game.gameInterface, airGroup != null ? airGroup.Class : string.Empty);
        }
    }
}
