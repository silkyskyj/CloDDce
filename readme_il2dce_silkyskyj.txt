===========================================================================================================================
A dynamic campaign engine & dynamic mission for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings Release (Version 0.5.0)
===========================================================================================================================
                                                                                                                 03/22/2025
This is the silkyskyj version of IL2DCE.
I forked and customized the source code, which the original author Stefan Rothdach created and released in 2011 and has been customizing until recently, under the terms of the AGPL3.0 license.
My customizations do not significantly change the basic functions that originally existed in IL2DCE, but mainly modify and add functions to the User Interface and random selection parts.

It is important to note that this add-in does not replace or attempt to compete with existing apps and add-ins such as Quick Mission, Single Mission, Campaign, and other apps and add-ins created by various developers, official development teams, and groups.
I hope you will choose it as one of the several ways to play the game.

In this release, we have focused on testing whether the program works properly, and have done little testing in abnormal cases.
(For example, most of the abnormality tests are for things like what happens if there is an unexpected format in the configuration file, what happens if the pilot name is 100 characters, etc.)
Please be aware of this.

This is a program that is available for free under the terms of AGPL3.0.
We do not guarantee operation, damages caused by use, the presence or absence of defects, bug fixes, feature additions, replies to inquiries, or explanations of features.
We are very sorry, but please use this product only if you understand these points.

#I can't speak English, so I translate or paraphrase everything I read and write. Please understand that there may be some minor mistakes in the expressions.

Main functions
1. Dynamic Campaign
2. Dynamic Quick Misson

1. In Dynamic Campaign, you can select and set the following items.
Career
 - Army (Red/Blue)
 - Air Force
Red: RAF/French Air Force/USAAF/Sovies Air Force/Polish Air Force/Checoslovak Air Force
Blue: Lufutwaffe/Regia Aeronautica/Hangarian Air Force/Romanian Air Force/Finissh Air Force/Slovak Air Force
*Mission files and configuration files must be compatible with your own extensions. Please refer to the included sample missions.
 - Rank
 - PilotName
 - Campaign
 - Air Group
 - Campaign Period
 - Filter display function for existing carrier list (Army/Air Force/Campaign)

2. The following items can be selected and set in Dynamic Quick Mission.
 - Campaign (mission file for IL2DCE)
 - Army (Red/Blue)
 - Air Force
Red: RAF/French Air Force/USAAF/Sovies Air Force/Polish Air Force/Checoslovak Air Force
Blue: Lufutwaffe/Regia Aeronautica/Hangarian Air Force/Romanian Air Force/Finissh Air Force/Slovak Air Force
*Mission files and setting files must be compatible with your own extension. Please refer to the attached sample mission.
 - Rank
 - Air Group
 - Skill (system default or custom skills) 20 in total + user-defined (can be set in il2dce's conf.ini file)
 - Mission Type
 - Spwan type [idle/Parked/AirStart(Altitude)]
 - Spwan Speed
 - Fuel % value (5% increments)
 - Time
 - Weather
 - Cloud Altitude

Items that can be selected and set commonly in 1. 2.
 - Max Additional Air Operations [1-7]
 - Max Additional Ground Operations [10-300]
 - Random Air Group Spwan Location [Player/Friendly/Enemy]
 - Automatically add Air Groups (this adds to Random if there are few AirGroups)
 - Time(Spawn delay) 15-1800[sec] [Friendly/Enemy]

Sample mission (7 mission files)

 - Adlerangriff RAF/Lufutwaffe
 - Cross v Round.Tobruk [*] RAF/Sovies Air Force/Polish Air Force/Checoslovak Air Force
 - Cross v Round.Tobruk [*] Hungarian Air Force/Romanian Air Force/Finissh Air Force/Slovak Air Force
 - Isles of Doom.Tobruk [*] RAF/French Air Force/USAAF/Lufutwaffe/Regia Aeronautica
 - Kanalkampf RAF/Lufutwaffe
 - Steppe.Tobruk [*] RAF/French Air Force/USAAF/Sovies Air Force/Polish Air Force/Checoslovak Air Force/Lufutwaffe/Regia Aeronautica/Hangarian Air Force/Romanian Air Force/Finissh Air Force/Slovak Air Force
 - Supply Wars RAF/Lufutwaffe/Regia Aeronautica
 - Supply Wars.Tobruk [*] RAF/French Air Force/USAAF/Regia Aeronautica
[*]Requires Tobruk DLC

I have not been able to confirm whether this will work in any environment, but I will describe the environment in which it actually worked.
OS: Windows 10 X64 Home (Development) latest official MS patch
    Windows 11 Home (Test & Play) latest official MS patch
Software: IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings(DLC) - English Langauage

A dynamic campaign engine & dynamic mission for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings

Official website: https://github.com/silkyskyj/il2dce
*The latest source code and change history are all publicly available.

Original: https://code.google.com/archive/p/il2dce/
Install: https://code.google.com/archive/p/il2dce/wikis/Install_Instructions.wiki (+ Blitz edition: https://forum.il2sturmovik.com/topic/90336-as-a-qmb-is-currently-wip-lets-discuss-what-it-needs-to-be-successful/ )

Campaign Creation Instructions: https://code.google.com/archive/p/il2dce/wikis/Campaign_Creation_Instructions.wiki

===========================================================================================================================
History 
===========================================================================================================================

  Ver      Date       Comment

  0.5.0    03/22/2025 New release 

---
IL2DCE [Forked by silkyskyj]
Copyright © Stefan Rothdach 2011- & silkyskyj 2025-
silkyskyj https://github.com/silkyskyj/il2dce
Stefan Rothdach https://github.com/il2dce/il2dc (https://code.google.com/archive/p/il2dce/)