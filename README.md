A dynamic campaign engine & dynamic mission for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings Release (Current Version 0.6.2 04/25/2025)
https://github.com/silkyskyj/il2dce/releases

Fixed:
 - not work AdditionalStationaries in Campaign
 - not carried over GroundGroup disable & position\[Campaign StrictMode\]
 - if a group's units are reduced to 0, they will disappear forever\[Campaign StrictMode\]
 - Temporary solution to CloD specification where AI aircraft will not take off if AirGroup is set idle. (0.6.1)

*In an existing campaign, the new settings will take effect from the next after next campaign mission. The next campaign mission will be created with the previous settings.

Artillery (The ship kind and the land kind and the anti-air kind) https://forum.il2sturmovik.com/topic/90957-artillery-the-ship-kind-and-the-land-kind-and-the-anti-air-kind/
Ship Speed and Convoys (Boat Racing) https://forum.il2sturmovik.com/topic/90951-ship-speed-and-convoys-boat-racing/
AI Skill Settings https://forum.il2sturmovik.com/topic/91024-ai-skill-settings/

*There are cases where files are not properly overwritten by the installer. I recommend uninstalling the old version -> deleting the IL2DCE folder in Program Files -> reinstalling.
*please re-import any mission files that you have previously imported.
* \[Kill count may differ from game default\] sorry, since the method for counting the number of kills in the game has not been made public, we cannot guarantee that the numbers will be the same. The calculation method is different.This is the current specification of IL2DCE. If you don't like this number, please use CloD PlayerStat API statType 1. 

04/22/2025 In the future, please refrain from posting any bugs or similar posts that have no evidence of the bugs described in this topic, as I cannot determine whether they are bugs, misunderstandings, or other reasons.  If you post something like that or if you do not understand that it is AGPL, I may hide it, so we appreciate your understanding.

* If there is a release, it will be done on the weekend. If there is a problem, please revert to the old version and use it for a while.

\[Current Bugs\]
 - There are cases where no text is displayed when viewing the battle results statistics.(the cause has been identified)

This is the silkyskyj version of IL2DCE.

IL2DCE was created and released by the original author, Stefan Rothdach, in 2011 and has been customized since.
I have forked the source code and customized it under the AGPL3.0 license terms, and am releasing it this time.

It is important to note that this add-in does not replace or attempt to compete with existing apps and add-ins such as Quick Mission, Single Mission, Campaign, and other apps and add-ins created by various developers, official development teams, and groups.
I hope you will choose it as one of the several ways to play the game.

In this release, we have focused on testing whether the program works properly, and have done little testing in abnormal cases.
(For example, most of the abnormality tests are for things like what happens if there is an unexpected format in the configuration file, what happens if the pilot name is 100 characters, etc.)
Please be aware of this.

This is a program that is available for free under the terms of AGPL3.0.
I don't guarantee operation, damages caused by use, the presence or absence of defects, bug fixes, feature additions, replies to inquiries, or explanations of features.
I'm very sorry, but please use this product only if you understand these points.

#sorry, I can't speak English, so I translate or paraphrase everything I read and write. Please understand that there may be some minor mistakes in the expressions.

Main functions
1. Dynamic Campaign
2. Dynamic Quick Misson


1. In Dynamic Campaign, you can select and set the following items. 
Career
 - Army (Red/Blue)
 - Air Force
     Red: RAF/French Air Force/USAAF/Sovies Air Force/Polish Air Force/Checoslovak Air Force/Royal Netherlands Air Force/Belgian Air Component
     Blue: Lufutwaffe/Regia Aeronautica/Hangarian Air Force/Romanian Air Force/Finissh Air Force/Slovak Air Force
     *Mission files and configuration files must be compatible with your own extensions. Please refer to the included sample missions.
 - Rank
 - PilotName
 - Campaign (mission file for IL2DCE)
 - Air Group
 - Campaign Period - Filter display function for existing career list (Army/Air Force/Campaign/StrictMode/Playabe)
 - Strict Mode <- v0.6.0
 - Spawn Parked <- v0.6.0
 - Initial AirGroup Skill (system default or custom skills) 19 in total + user-defined (can be set in il2dce's conf.ini file) <- v0.6.0
 - Progress (AnyTime/Daily/AnyDay/AnyDayAnyTime) <- v0.6.0

2. The following items can be selected and set in Dynamic Quick Mission.

 - Campaign (mission file for IL2DCE)
 - Army (Red/Blue)
 - Air Force
     Red: RAF/French Air Force/USAAF/Sovies Air Force/Polish Air Force/Checoslovak Air Force/Royal Netherlands Air Force/Belgian Air Component
     Blue: Lufutwaffe/Regia Aeronautica/Hangarian Air Force/Romanian Air Force/Finissh Air Force/Slovak Air Force
     *Mission files and setting files must be compatible with your own extension. Please refer to the attached sample mission.
 - Rank
 - Air Group
 - Skill (system default or custom skills) 19 in total + user-defined (can be set in il2dce's conf.ini file)
 - Mission Type
 - Flight (count & size) <- v0.5.4
 - Formation <- v0.5.5
 - Spawn type \[idle/Parked/AirStart(Altitude)\]
 - Spawn Speed
 - Fuel % value (5% increments)
 - Time
 - Weather
 - Cloud Altitude

  and Ability to convert and import existing mission files

Items that can be selected and set commonly in 1. 2.
 - Max Additional Air Operations \[1-12\]
 - Max Additional Ground Operations \[10-300\]
 - Random Air Group Spawn Location \[Player/Friendly/Enemy\]
 - Automatically add Air Groups (this adds to Random if there are few AirGroups)
 - Time(Spawn delay) 15-1800\[sec\] \[Friendly/Enemy\]
 - Auto re-Arm (bullets only) <- v0.5.6
 - Auto re-Fuel <- v0.5.6
 - Track Recording <- v0.5.6
 - AI Air Group Skill <- v0.5.10 
 - AI Ship Skill <- v0.6.0
-  Add Random Ground units <- v0.6.0
 - Ground Unit Setting <- v0.6.0
 - Unit Nums <- v0.6.0
 - Convert Generic type <- v0.6.0
 - Save/Load general settings <- v0.6.0

Sample mission (7 mission files)
 - Adlerangriff RAF/Lufutwaffe
 - Cross v Round.Tobruk \[*\] RAF/Sovies Air Force/Polish Air Force/Checoslovak Air Force/Hungarian Air Force/Romanian Air Force/Finissh Air Force/Slovak Air Force
 - Isles of Doom.Tobruk \[*\] RAF/French Air Force/USAAF/Lufutwaffe/Regia Aeronautica
 - Kanalkampf RAF/Lufutwaffe
 - Steppe.Tobruk \[*\] RAF/French Air Force/USAAF/Sovies Air Force/Polish Air Force/Checoslovak Air Force/Lufutwaffe/Regia Aeronautica/Hangarian Air Force/Romanian Air Force/Finissh Air Force/Slovak Air Force/Royal Netherlands Air Force/Belgian Air Component
 - Supply Wars RAF/Lufutwaffe/Regia Aeronautica
 - Supply Wars.Tobruk \[*\] RAF/French Air Force/USAAF/Regia Aeronautica
\[*\]Requires Tobruk DLC

I have not been able to confirm whether this will work in any environment, but I will describe the environment in which it actually worked.
OS: Windows 10 x64 Home (Development) latest official MS patch
    Windows 11 Home (Test & Play) latest official MS patch
Software: IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings(DLC) - English Langauage
Display Resolution: 1920x1080

A dynamic campaign engine & dynamic mission for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings
Official website: https://github.com/silkyskyj/il2dce

*The latest source code and change history are all publicly available.

Original: https://code.google.com/archive/p/il2dce/
Install: https://code.google.com/archive/p/il2dce/wikis/Install_Instructions.wiki (+ Blitz edition: https://forum.il2sturmovik.com/topic/90336-as-a-qmb-is-currently-wip-lets-discuss-what-it-needs-to-be-successful/ )

Campaign Creation Instructions: https://code.google.com/archive/p/il2dce/wikis/Campaign_Creation_Instructions.wiki

---
I am a programmer who runs a small software company in Japan. Aside from my main job, I customize this OSS as a personal hobby. In this way, OSS can be developed by individuals as they like while respecting copyrights and rules. I would like to express my sincere gratitude to the original developer, Stefan Rothdach, for creating and releasing this wonderful software.

---
IL2DCE \[Forked by silkyskyj\]
Copyright © Stefan Rothdach 2011- & silkyskyj 2025-
silkyskyj https://github.com/silkyskyj/il2dce
Stefan Rothdach https://github.com/il2dce/il2dc (https://code.google.com/archive/p/il2dce/)
